namespace JdhPro.BuildTools.Models;

/// <summary>
/// Configuration for blog syndication from external sources
/// </summary>
public class BlogSyndicationConfig
{
    public GitHubSourceConfig? GitHub { get; set; }
    public SyndicationFilters? Filters { get; set; }
}

public class GitHubSourceConfig
{
    public required string Owner { get; set; }
    public required string Repo { get; set; }
    public string Branch { get; set; } = "main";
    public string PostsDirectory { get; set; } = "posts";
    public string? BaseUrl { get; set; } // For canonical URLs
}

public class SyndicationFilters
{
    /// <summary>
    /// Only include posts with these categories (OR logic)
    /// </summary>
    public List<string> IncludedCategories { get; set; } = new();
    
    /// <summary>
    /// Exclude posts with these categories
    /// </summary>
    public List<string> ExcludedCategories { get; set; } = new();
    
    /// <summary>
    /// Exclude posts with these tags
    /// </summary>
    public List<string> ExcludedTags { get; set; } = new();
}
