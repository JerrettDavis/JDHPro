using System.Globalization;
using JdhPro.Web.Models;
using Microsoft.Extensions.Logging;

namespace JdhPro.Web.Services;

/// <summary>
/// Orchestrates blog post syndication from external sources
/// </summary>
public class BlogSyndicationService
{
    private readonly GitHubPostFetcher _githubFetcher;
    private readonly FrontmatterParser _frontmatterParser;
    private readonly PostFilterService _filterService;
    private readonly MarkdownService _markdownService;
    private readonly ILogger<BlogSyndicationService>? _logger;

    public BlogSyndicationService(
        GitHubPostFetcher githubFetcher,
        FrontmatterParser frontmatterParser,
        PostFilterService filterService,
        MarkdownService markdownService,
        ILogger<BlogSyndicationService>? logger = null)
    {
        _githubFetcher = githubFetcher;
        _frontmatterParser = frontmatterParser;
        _filterService = filterService;
        _markdownService = markdownService;
        _logger = logger;
    }

    /// <summary>
    /// Syndicates posts from configured sources and combines with local posts
    /// </summary>
    public async Task<List<BlogPost>> SyndicatePostsAsync(
        BlogSyndicationConfig config,
        List<BlogPost>? localPosts = null)
    {
        var allPosts = new List<BlogPost>();

        // Add local posts if provided
        if (localPosts != null && localPosts.Any())
        {
            _logger?.LogInformation("Adding {Count} local posts", localPosts.Count);
            allPosts.AddRange(localPosts);
        }

        // Fetch and process GitHub posts
        if (config.GitHub != null)
        {
            _logger?.LogInformation("Starting GitHub syndication from {Owner}/{Repo}", 
                config.GitHub.Owner, config.GitHub.Repo);

            var githubPosts = await FetchGitHubPostsAsync(config.GitHub);
            
            if (config.Filters != null)
            {
                _logger?.LogInformation("Applying filters to {Count} GitHub posts", githubPosts.Count);
                githubPosts = _filterService.FilterPosts(githubPosts, config.Filters);
            }

            _logger?.LogInformation("Adding {Count} syndicated posts", githubPosts.Count);
            allPosts.AddRange(githubPosts);
        }

        // Deduplicate by ID (local posts take precedence)
        var deduplicated = DeduplicatePosts(allPosts);
        
        // Sort by date descending
        var sorted = deduplicated.OrderByDescending(p => p.Date).ToList();

        _logger?.LogInformation("Final post count: {Count} posts", sorted.Count);
        
        return sorted;
    }

    /// <summary>
    /// Fetches and processes posts from GitHub
    /// </summary>
    private async Task<List<BlogPost>> FetchGitHubPostsAsync(GitHubSourceConfig config)
    {
        var posts = new List<BlogPost>();

        try
        {
            // Fetch raw posts
            var rawPosts = await _githubFetcher.FetchPostsAsync(
                config.Owner,
                config.Repo,
                config.Branch,
                config.PostsDirectory);

            _logger?.LogInformation("Processing {Count} raw posts from GitHub", rawPosts.Count);

            // Parse and convert each post
            foreach (var (filename, content) in rawPosts)
            {
                try
                {
                    var post = ParsePost(filename, content, config);
                    
                    if (post != null)
                    {
                        posts.Add(post);
                        _logger?.LogDebug("Parsed post: {Title} ({Id})", post.Title, post.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error parsing post {Filename}", filename);
                }
            }

            _logger?.LogInformation("Successfully parsed {Count} GitHub posts", posts.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error fetching GitHub posts");
        }

        return posts;
    }

    /// <summary>
    /// Parses a single post from raw content
    /// </summary>
    private BlogPost? ParsePost(string filename, string content, GitHubSourceConfig config)
    {
        _logger?.LogDebug("Parsing post: {Filename}", filename);
        
        // Parse frontmatter
        var (frontmatter, markdownContent) = _frontmatterParser.Parse(content);

        // Generate slug from filename if not in frontmatter
        var slug = GenerateSlug(filename);

        // Parse date
        if (!TryParseDate(frontmatter.Date, out var date))
        {
            _logger?.LogWarning("Invalid or missing date for {Filename}, skipping", filename);
            return null;
        }

        // Normalize categories and tags
        var categories = FrontmatterParser.NormalizeCategories(frontmatter.Categories);
        var tags = FrontmatterParser.NormalizeTags(frontmatter.Tags);

        // Render markdown to HTML
        var html = _markdownService.RenderToHtml(markdownContent);

        // Generate summary if not provided
        var description = frontmatter.Description 
            ?? _markdownService.GenerateSummary(markdownContent);

        // Calculate word count
        var wordCount = _markdownService.CalculateWordCount(markdownContent);

        // Build canonical URL
        var canonicalUrl = config.BaseUrl != null 
            ? $"{config.BaseUrl.TrimEnd('/')}/blog/{slug}"
            : null;

        return new BlogPost
        {
            Id = slug,
            Title = frontmatter.Title ?? Path.GetFileNameWithoutExtension(filename),
            Date = date,
            Description = description,
            Featured = frontmatter.Featured,
            Tags = tags,
            Categories = categories,
            Series = frontmatter.Series,
            SeriesOrder = FrontmatterParser.ParseSeriesOrder(frontmatter.SeriesOrder),
            Content = markdownContent,
            ContentHtml = html,
            Stub = description,
            WordCount = wordCount,
            UseToc = frontmatter.UseToc,
            Source = "syndicated",
            CanonicalUrl = canonicalUrl
        };
    }

    /// <summary>
    /// Generates a slug from a filename
    /// "my-awesome-post.mdx" -> "my-awesome-post"
    /// </summary>
    private string GenerateSlug(string filename)
    {
        return Path.GetFileNameWithoutExtension(filename)
            .ToLowerInvariant()
            .Replace(" ", "-");
    }

    /// <summary>
    /// Tries to parse a date string
    /// </summary>
    private bool TryParseDate(string? dateString, out DateTime date)
    {
        date = DateTime.MinValue;

        if (string.IsNullOrWhiteSpace(dateString))
        {
            return false;
        }

        // Try various date formats
        var formats = new[]
        {
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-dd HH:mm:ss",
            "MM/dd/yyyy",
            "dd/MM/yyyy"
        };

        return DateTime.TryParseExact(
            dateString,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }

    /// <summary>
    /// Deduplicates posts by ID, preferring local posts over syndicated
    /// </summary>
    private List<BlogPost> DeduplicatePosts(List<BlogPost> posts)
    {
        var deduplicated = new Dictionary<string, BlogPost>();

        foreach (var post in posts)
        {
            if (!deduplicated.ContainsKey(post.Id))
            {
                deduplicated[post.Id] = post;
            }
            else
            {
                // Local posts take precedence
                var existing = deduplicated[post.Id];
                if (post.Source == "local" && existing.Source == "syndicated")
                {
                    deduplicated[post.Id] = post;
                    _logger?.LogInformation("Local post '{Title}' overrides syndicated version", post.Title);
                }
                else
                {
                    _logger?.LogDebug("Skipping duplicate post '{Title}' ({Id})", post.Title, post.Id);
                }
            }
        }

        return deduplicated.Values.ToList();
    }
}
