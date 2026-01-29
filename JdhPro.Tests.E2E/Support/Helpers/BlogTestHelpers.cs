using System.Text.Json;
using FluentAssertions;
using JdhPro.Tests.E2E.Models;

namespace JdhPro.Tests.E2E.Support.Helpers;

/// <summary>
/// Helper methods for testing blog-related functionality
/// </summary>
public static class BlogTestHelpers
{
    /// <summary>
    /// Loads posts.json from the Web project's wwwroot/data folder
    /// </summary>
    public static async Task<List<BlogPostDto>> LoadPostsJsonAsync()
    {
        var postsJsonPath = GetPostsJsonPath();
        
        if (!File.Exists(postsJsonPath))
        {
            throw new FileNotFoundException($"posts.json not found at {postsJsonPath}. Run blog syndication first.");
        }

        var json = await File.ReadAllTextAsync(postsJsonPath);
        var posts = JsonSerializer.Deserialize<List<BlogPostDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return posts ?? new List<BlogPostDto>();
    }

    /// <summary>
    /// Gets the path to posts.json in the Web project
    /// </summary>
    public static string GetPostsJsonPath()
    {
        // Navigate from test project to web project
        var webProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "JdhPro.Web",
            "wwwroot", "data", "posts.json"
        );

        return Path.GetFullPath(webProjectPath);
    }

    /// <summary>
    /// Validates that a blog post has all required fields
    /// </summary>
    public static void ValidatePostStructure(BlogPostDto post, string postId = "")
    {
        var context = string.IsNullOrEmpty(postId) ? "Post" : $"Post '{postId}'";
        
        post.Should().NotBeNull($"{context} should not be null");
        post.Id.Should().NotBeNullOrEmpty($"{context} should have an ID");
        post.Title.Should().NotBeNullOrEmpty($"{context} should have a title");
        post.Date.Should().NotBe(default(DateTime), $"{context} should have a valid date");
        post.Content.Should().NotBeNullOrEmpty($"{context} should have content");
        post.ContentHtml.Should().NotBeNullOrEmpty($"{context} should have HTML content");
        post.Stub.Should().NotBeNullOrEmpty($"{context} should have a stub/slug");
        post.Source.Should().NotBeNullOrEmpty($"{context} should have a source");
        post.Categories.Should().NotBeNull($"{context} should have categories list");
        post.Tags.Should().NotBeNull($"{context} should have tags list");
    }

    /// <summary>
    /// Validates that all posts in the list have required fields
    /// </summary>
    public static void ValidateAllPostsStructure(List<BlogPostDto> posts)
    {
        posts.Should().NotBeNull("Posts list should not be null");
        posts.Should().NotBeEmpty("Posts list should contain at least one post");

        foreach (var post in posts)
        {
            ValidatePostStructure(post, post.Id);
        }
    }

    /// <summary>
    /// Validates that no posts contain excluded categories
    /// </summary>
    public static void ValidateNoExcludedCategories(List<BlogPostDto> posts, params string[] excludedCategories)
    {
        foreach (var post in posts)
        {
            foreach (var category in excludedCategories)
            {
                post.Categories.Should().NotContain(
                    category,
                    $"Post '{post.Id}' should not contain excluded category '{category}'"
                );
            }
        }
    }

    /// <summary>
    /// Validates that no posts contain excluded tags
    /// </summary>
    public static void ValidateNoExcludedTags(List<BlogPostDto> posts, params string[] excludedTags)
    {
        foreach (var post in posts)
        {
            foreach (var tag in excludedTags)
            {
                post.Tags.Should().NotContain(
                    tag,
                    $"Post '{post.Id}' should not contain excluded tag '{tag}'"
                );
            }
        }
    }

    /// <summary>
    /// Validates that posts have valid canonical URLs if syndicated
    /// </summary>
    public static void ValidateSyndicatedPostsHaveCanonicalUrls(List<BlogPostDto> posts)
    {
        var syndicatedPosts = posts.Where(p => p.Source == "syndicated").ToList();

        foreach (var post in syndicatedPosts)
        {
            post.CanonicalUrl.Should().NotBeNullOrEmpty(
                $"Syndicated post '{post.Id}' should have a canonical URL"
            );

            // Validate URL format
            var isValidUrl = Uri.TryCreate(post.CanonicalUrl, UriKind.Absolute, out var uri);
            isValidUrl.Should().BeTrue($"Post '{post.Id}' should have a valid canonical URL");
            uri!.Scheme.Should().BeOneOf("http", "https");
        }
    }

    /// <summary>
    /// Validates that posts are sorted by date (newest first)
    /// </summary>
    public static void ValidatePostsSortedByDateDescending(List<BlogPostDto> posts)
    {
        posts.Should().NotBeEmpty("Cannot validate sort order on empty list");

        for (int i = 0; i < posts.Count - 1; i++)
        {
            posts[i].Date.Should().BeOnOrAfter(
                posts[i + 1].Date,
                $"Posts should be sorted by date descending (newest first). Post '{posts[i].Id}' ({posts[i].Date:yyyy-MM-dd}) should be >= '{posts[i + 1].Id}' ({posts[i + 1].Date:yyyy-MM-dd})"
            );
        }
    }

    /// <summary>
    /// Validates that HTML content is properly escaped and doesn't contain raw markdown
    /// </summary>
    public static void ValidateHtmlContent(BlogPostDto post)
    {
        post.ContentHtml.Should().NotBeNullOrEmpty($"Post '{post.Id}' should have HTML content");
        
        // Should contain HTML tags
        post.ContentHtml.Should().Match(html => 
            html.Contains("<p>") || html.Contains("<h1>") || html.Contains("<h2>"),
            $"Post '{post.Id}' HTML content should contain HTML tags"
        );

        // Should not contain raw markdown syntax for common elements
        post.ContentHtml.Should().NotContain("](", $"Post '{post.Id}' HTML should not contain raw markdown links");
    }

    /// <summary>
    /// Calculates expected word count for validation
    /// </summary>
    public static int CalculateWordCount(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return 0;

        var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Length;
    }

    /// <summary>
    /// Validates post metadata completeness
    /// </summary>
    public static void ValidatePostMetadata(BlogPostDto post, bool requireDescription = false)
    {
        ValidatePostStructure(post, post.Id);

        if (requireDescription)
        {
            post.Description.Should().NotBeNullOrEmpty($"Post '{post.Id}' should have a description");
        }

        // Word count should be reasonable
        post.WordCount.Should().BeGreaterThan(0, $"Post '{post.Id}' should have a word count");
        
        // Stub should be URL-friendly
        post.Stub.Should().Match(stub => !stub.Contains(" "), 
            $"Post '{post.Id}' stub should not contain spaces"
        );
        post.Stub.Should().Match(stub => stub.ToLower() == stub, 
            $"Post '{post.Id}' stub should be lowercase"
        );
    }

    /// <summary>
    /// Gets posts by category
    /// </summary>
    public static List<BlogPostDto> GetPostsByCategory(List<BlogPostDto> posts, string category)
    {
        return posts.Where(p => p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Gets posts by tag
    /// </summary>
    public static List<BlogPostDto> GetPostsByTag(List<BlogPostDto> posts, string tag)
    {
        return posts.Where(p => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Gets posts by source (local or syndicated)
    /// </summary>
    public static List<BlogPostDto> GetPostsBySource(List<BlogPostDto> posts, string source)
    {
        return posts.Where(p => p.Source.Equals(source, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Validates JSON file size is reasonable
    /// </summary>
    public static void ValidateJsonFileSize(long fileSizeBytes, long minBytes = 100, long maxMegabytes = 10)
    {
        fileSizeBytes.Should().BeGreaterThan(minBytes, "posts.json should contain data");
        fileSizeBytes.Should().BeLessThan(maxMegabytes * 1024 * 1024, 
            $"posts.json should be smaller than {maxMegabytes}MB"
        );
    }
}
