namespace JdhPro.Tests.E2E.Models;

/// <summary>
/// Data transfer object representing a blog post from posts.json
/// </summary>
public class BlogPostDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? Featured { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string? Series { get; set; }
    public int? SeriesOrder { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public string Stub { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public bool UseToc { get; set; }
    public string Source { get; set; } = "local";
    public string? CanonicalUrl { get; set; }
}
