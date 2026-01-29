using JdhPro.Web.Models;
using Microsoft.Extensions.Logging;

namespace JdhPro.Web.Services;

/// <summary>
/// Filters blog posts based on categories and tags
/// </summary>
public class PostFilterService
{
    private readonly ILogger<PostFilterService>? _logger;

    public PostFilterService(ILogger<PostFilterService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Filters posts based on syndication configuration
    /// </summary>
    public List<BlogPost> FilterPosts(List<BlogPost> posts, SyndicationFilters filters)
    {
        if (filters == null)
        {
            _logger?.LogInformation("No filters configured, returning all posts");
            return posts;
        }

        var filtered = posts.Where(post => ShouldIncludePost(post, filters)).ToList();
        
        _logger?.LogInformation("Filtered {Original} posts down to {Filtered} posts", 
            posts.Count, filtered.Count);

        return filtered;
    }

    /// <summary>
    /// Determines if a post should be included based on filters
    /// </summary>
    private bool ShouldIncludePost(BlogPost post, SyndicationFilters filters)
    {
        // Check excluded tags first (highest priority)
        if (filters.ExcludedTags.Any() && post.Tags.Any(tag => 
            filters.ExcludedTags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
        {
            _logger?.LogDebug("Excluding post '{Title}' - has excluded tag", post.Title);
            return false;
        }

        // Check excluded categories
        if (filters.ExcludedCategories.Any() && post.Categories.Any(category =>
            IsMatchingCategory(category, filters.ExcludedCategories)))
        {
            _logger?.LogDebug("Excluding post '{Title}' - has excluded category", post.Title);
            return false;
        }

        // Check included categories (if specified)
        if (filters.IncludedCategories.Any())
        {
            var hasIncludedCategory = post.Categories.Any(category =>
                IsMatchingCategory(category, filters.IncludedCategories));

            if (!hasIncludedCategory)
            {
                _logger?.LogDebug("Excluding post '{Title}' - does not match included categories", post.Title);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if a category matches any filter category (supports hierarchical matching)
    /// Example: "Programming/Architecture" matches "Programming"
    /// </summary>
    private bool IsMatchingCategory(string postCategory, List<string> filterCategories)
    {
        foreach (var filterCategory in filterCategories)
        {
            // Exact match
            if (string.Equals(postCategory, filterCategory, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Hierarchical match: post category starts with filter category
            // e.g., "Programming/Architecture" matches "Programming"
            if (postCategory.StartsWith(filterCategory + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Reverse hierarchical match: filter category starts with post category
            // e.g., "Programming" matches "Programming/Architecture"
            if (filterCategory.StartsWith(postCategory + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
