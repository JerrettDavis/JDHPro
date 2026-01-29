using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace JdhPro.Web.Services;

/// <summary>
/// Fetches blog posts from GitHub repository
/// </summary>
public class GitHubPostFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubPostFetcher>? _logger;

    public GitHubPostFetcher(HttpClient httpClient, ILogger<GitHubPostFetcher>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Set GitHub API headers
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "JdhPro-BlogSyndication");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    }

    /// <summary>
    /// Fetches all posts from a GitHub repository
    /// </summary>
    public async Task<List<(string Filename, string Content)>> FetchPostsAsync(
        string owner,
        string repo,
        string branch,
        string postsDirectory)
    {
        var posts = new List<(string, string)>();

        try
        {
            // Get list of files in the posts directory
            var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/contents/{postsDirectory}?ref={branch}";
            _logger?.LogInformation("Fetching post list from: {Url}", apiUrl);

            var response = await _httpClient.GetAsync(apiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("Failed to fetch post list. Status: {Status}", response.StatusCode);
                
                // Check for rate limiting
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var rateLimitRemaining = response.Headers.TryGetValues("X-RateLimit-Remaining", out var values) 
                        ? values.FirstOrDefault() 
                        : "unknown";
                    _logger?.LogWarning("GitHub API rate limit may be exceeded. Remaining: {Remaining}", rateLimitRemaining);
                }
                
                return posts;
            }

            var files = await response.Content.ReadFromJsonAsync<List<GitHubFile>>();
            
            if (files == null || files.Count == 0)
            {
                _logger?.LogWarning("No files found in {Directory}", postsDirectory);
                return posts;
            }

            // Filter for MDX files
            var mdxFiles = files.Where(f => f.Name.EndsWith(".mdx", StringComparison.OrdinalIgnoreCase) 
                                          || f.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                                .ToList();

            _logger?.LogInformation("Found {Count} markdown files", mdxFiles.Count);

            // Fetch content for each file
            foreach (var file in mdxFiles)
            {
                try
                {
                    var content = await FetchFileContentAsync(owner, repo, branch, file.Path);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        posts.Add((file.Name, content));
                        _logger?.LogInformation("Fetched: {Filename} ({Size} bytes)", file.Name, content.Length);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error fetching content for {Filename}", file.Name);
                }
            }

            _logger?.LogInformation("Successfully fetched {Count} posts", posts.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error fetching posts from GitHub");
        }

        return posts;
    }

    /// <summary>
    /// Fetches raw content of a single file
    /// </summary>
    private async Task<string> FetchFileContentAsync(string owner, string repo, string branch, string path)
    {
        var rawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{path}";
        _logger?.LogDebug("Fetching raw content from: {Url}", rawUrl);

        var response = await _httpClient.GetAsync(rawUrl);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// GitHub API file object
    /// </summary>
    private class GitHubFile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("download_url")]
        public string? DownloadUrl { get; set; }
    }
}
