using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JdhPro.BuildTools.Models;
using JdhPro.BuildTools.Services;

Console.WriteLine("=== JDH Productions Blog Syndication Tool ===\n");

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

// Setup logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
});

// Get syndication config
var syndicationConfig = configuration.GetSection("BlogSyndication").Get<BlogSyndicationConfig>();

if (syndicationConfig == null)
{
    Console.WriteLine("❌ Error: BlogSyndication configuration not found in appsettings.json");
    return 1;
}

// Initialize services
var httpClient = new HttpClient();
var githubFetcher = new GitHubPostFetcher(httpClient, loggerFactory.CreateLogger<GitHubPostFetcher>());
var frontmatterParser = new FrontmatterParser();
var filterService = new PostFilterService(loggerFactory.CreateLogger<PostFilterService>());
var markdownService = new MarkdownService(loggerFactory.CreateLogger<MarkdownService>());
var syndicationService = new BlogSyndicationService(
    githubFetcher,
    frontmatterParser,
    filterService,
    markdownService,
    loggerFactory.CreateLogger<BlogSyndicationService>()
);

try
{
    Console.WriteLine("📡 Starting blog post syndication...\n");
    
    // Run syndication
    var posts = await syndicationService.SyndicatePostsAsync(syndicationConfig);
    
    Console.WriteLine($"\n✅ Successfully syndicated {posts.Count} posts\n");
    
    // Display post summary
    Console.WriteLine("📝 Post Summary:");
    Console.WriteLine(new string('-', 80));
    foreach (var post in posts.Take(10))
    {
        Console.WriteLine($"  • {post.Date:yyyy-MM-dd} - {post.Title}");
        Console.WriteLine($"    Categories: {string.Join(", ", post.Categories)}");
        Console.WriteLine($"    Source: {post.Source}");
        Console.WriteLine();
    }
    
    if (posts.Count > 10)
    {
        Console.WriteLine($"  ... and {posts.Count - 10} more posts\n");
    }
    
    // Determine output path (relative to JdhPro.Web project)
    var webProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "JdhPro.Web");
    var outputDir = Path.Combine(webProjectPath, "wwwroot", "data");
    var outputPath = Path.Combine(outputDir, "posts.json");
    
    // Ensure directory exists
    Directory.CreateDirectory(outputDir);
    
    // Serialize to JSON
    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    var json = JsonSerializer.Serialize(posts, jsonOptions);
    await File.WriteAllTextAsync(outputPath, json);
    
    Console.WriteLine($"💾 Posts saved to: {outputPath}");
    Console.WriteLine($"📊 File size: {new FileInfo(outputPath).Length / 1024} KB\n");
    
    Console.WriteLine("✨ Blog syndication completed successfully!");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error during syndication: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    return 1;
}
