namespace JdhPro.Web.Models;

/// <summary>
/// Represents a complete blog post with content and metadata
/// </summary>
public class BlogPost
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required DateTime Date { get; init; }
    public string? Description { get; init; }
    public string? Featured { get; init; }
    public List<string> Tags { get; init; } = new();
    public List<string> Categories { get; init; } = new();
    public string? Series { get; init; }
    public int? SeriesOrder { get; init; }
    public required string Content { get; init; }
    public required string ContentHtml { get; init; }
    public required string Stub { get; init; }
    public int WordCount { get; init; }
    public bool UseToc { get; init; }
    public string Source { get; init; } = "local"; // "local" or "syndicated"
    public string? CanonicalUrl { get; init; }
}

/// <summary>
/// Represents a blog post summary for list views
/// </summary>
public class BlogPostSummary
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required DateTime Date { get; init; }
    public string? Description { get; init; }
    public string? Featured { get; init; }
    public List<string> Tags { get; init; } = new();
    public List<string> Categories { get; init; } = new();
    public string? Series { get; init; }
    public int? SeriesOrder { get; init; }
    public required string Stub { get; init; }
    public string Source { get; init; } = "local";
    public string? CanonicalUrl { get; init; }
}

/// <summary>
/// Represents the frontmatter metadata from MDX files
/// </summary>
public class PostFrontmatter
{
    public string? Title { get; set; }
    public string? Date { get; set; }
    public string? Description { get; set; }
    public string? Featured { get; set; }
    public object? Tags { get; set; } // Can be string[] or string
    public object? Categories { get; set; } // Can be string[] or string
    public string? Series { get; set; }
    public object? SeriesOrder { get; set; } // Can be int or string
    public bool UseToc { get; set; }
    public bool? Syndicate { get; set; }
}
