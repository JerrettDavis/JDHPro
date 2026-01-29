using System.Text.RegularExpressions;
using System.Text.Json;
using Markdig;
using JdhPro.Web.Models;

namespace JdhPro.Web.Services;

public class ContentService
{
    private readonly HttpClient _httpClient;
    private readonly MarkdownPipeline _pipeline;
    private List<BlogPost>? _cachedPosts;

    public ContentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public async Task<List<Service>> GetServicesAsync()
    {
        var serviceFiles = new[]
        {
            "technical-consulting",
            "custom-development",
            "ai-workflow-consulting",
            "process-engineering"
        };

        var services = new List<Service>();

        foreach (var fileName in serviceFiles)
        {
            try
            {
                var content = await _httpClient.GetStringAsync($"content/services/{fileName}.md");
                var service = ParseServiceMarkdown(fileName, content);
                if (service != null)
                {
                    services.Add(service);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading service {fileName}: {ex.Message}");
            }
        }

        return services.OrderBy(s => s.Order).ToList();
    }

    private Service? ParseServiceMarkdown(string id, string markdown)
    {
        var service = new Service { Id = id };

        // Extract front matter
        var frontMatterMatch = Regex.Match(markdown, @"^---\s*\n(.*?)\n---\s*\n", RegexOptions.Singleline);
        if (frontMatterMatch.Success)
        {
            var frontMatter = frontMatterMatch.Groups[1].Value;
            service.Title = ExtractFrontMatterValue(frontMatter, "title") ?? "";
            service.Icon = ExtractFrontMatterValue(frontMatter, "icon") ?? "";
            service.Summary = ExtractFrontMatterValue(frontMatter, "summary") ?? "";
            
            if (int.TryParse(ExtractFrontMatterValue(frontMatter, "order"), out var order))
            {
                service.Order = order;
            }

            if (bool.TryParse(ExtractFrontMatterValue(frontMatter, "featured"), out var featured))
            {
                service.Featured = featured;
            }

            // Remove front matter from content
            markdown = markdown.Substring(frontMatterMatch.Length);
        }

        // Convert markdown to HTML
        service.ContentHtml = Markdown.ToHtml(markdown, _pipeline);

        // Extract benefits section
        var benefitsMatch = Regex.Match(markdown, @"## Key Benefits\s*\n\n((?:- .*\n?)+)", RegexOptions.Multiline);
        if (benefitsMatch.Success)
        {
            service.Benefits = ExtractListItems(benefitsMatch.Groups[1].Value);
        }

        // Extract use cases - look for sections that start with ### and are under a "Use Cases" or similar section
        var useCasesSection = Regex.Match(markdown, @"## (?:Typical Use Cases|Use Cases|Common Engagements|Typical Projects).*?\n\n(.*?)(?=\n## |\n---|\z)", RegexOptions.Singleline);
        if (useCasesSection.Success)
        {
            var useCasesText = useCasesSection.Groups[1].Value;
            var useCaseMatches = Regex.Matches(useCasesText, @"### (.+?)\n");
            service.UseCases = useCaseMatches.Select(m => m.Groups[1].Value.Trim()).ToList();
        }

        return service;
    }

    private string? ExtractFrontMatterValue(string frontMatter, string key)
    {
        var match = Regex.Match(frontMatter, $@"{key}:\s*(.+)", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private List<string> ExtractListItems(string listText)
    {
        var items = new List<string>();
        var matches = Regex.Matches(listText, @"^- \*\*(.+?)\*\*:?\s*(.+?)$", RegexOptions.Multiline);
        
        foreach (Match match in matches)
        {
            var title = match.Groups[1].Value.Trim();
            var description = match.Groups[2].Value.Trim();
            items.Add($"{title}: {description}");
        }

        return items;
    }

    private string GetIconForService(string icon)
    {
        return icon switch
        {
            "code-brackets" => "💻",
            "window-code" => "🚀",
            "cpu-chip" => "🤖",
            "cog" => "⚙️",
            _ => "🔧"
        };
    }

    // Blog Post Methods
    public async Task<List<BlogPost>> GetAllPostsAsync()
    {
        if (_cachedPosts != null)
        {
            return _cachedPosts;
        }

        try
        {
            var json = await _httpClient.GetStringAsync("data/posts.json");
            var posts = JsonSerializer.Deserialize<List<BlogPost>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<BlogPost>();

            // Sort by date descending (newest first)
            _cachedPosts = posts.OrderByDescending(p => p.Date).ToList();
            return _cachedPosts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading blog posts: {ex.Message}");
            return new List<BlogPost>();
        }
    }

    public async Task<BlogPost?> GetPostBySlugAsync(string slug)
    {
        var posts = await GetAllPostsAsync();
        return posts.FirstOrDefault(p => p.Id.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<BlogPost>> GetPostsByTagAsync(string tag)
    {
        var posts = await GetAllPostsAsync();
        return posts.Where(p => p.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                   .ToList();
    }

    public async Task<List<BlogPost>> GetPostsByCategoryAsync(string category)
    {
        var posts = await GetAllPostsAsync();
        return posts.Where(p => p.Categories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase)))
                   .ToList();
    }

    public async Task<List<string>> GetAllTagsAsync()
    {
        var posts = await GetAllPostsAsync();
        return posts.SelectMany(p => p.Tags)
                   .Distinct()
                   .OrderBy(t => t)
                   .ToList();
    }

    public async Task<List<string>> GetAllCategoriesAsync()
    {
        var posts = await GetAllPostsAsync();
        return posts.SelectMany(p => p.Categories)
                   .Distinct()
                   .OrderBy(c => c)
                   .ToList();
    }
}
