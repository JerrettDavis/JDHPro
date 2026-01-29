# JDH Productions Blog Syndication Tool

This tool syndicates blog posts from external sources (currently GitHub) to the JDH Productions website.

## Features

- ✅ Fetches blog posts from GitHub repositories
- ✅ Parses MDX/Markdown with YAML frontmatter
- ✅ Filters posts by categories and tags
- ✅ Renders Markdown to HTML using Markdig
- ✅ Generates post summaries and word counts
- ✅ Combines with local posts and deduplicates
- ✅ Exports to JSON for static site generation

## Usage

```bash
# Run from the BuildTools directory
cd JdhPro.BuildTools
dotnet run

# Or run from solution root
dotnet run --project JdhPro.BuildTools
```

## Configuration

Edit `appsettings.json` to configure syndication sources and filters:

```json
{
  "BlogSyndication": {
    "GitHub": {
      "Owner": "jerrettdavis",
      "Repo": "personal-site",
      "Branch": "main",
      "PostsDirectory": "posts",
      "BaseUrl": "https://jerrettdavis.com"
    },
    "Filters": {
      "IncludedCategories": ["Programming", "Software Engineering"],
      "ExcludedCategories": ["Personal", "Private"],
      "ExcludedTags": ["draft", "private"]
    }
  }
}
```

## Output

Posts are saved to:
```
../JdhPro.Web/wwwroot/data/posts.json
```

This file contains all syndicated and local posts, sorted by date descending.

## Post Filtering

### Category Filtering
- **Hierarchical matching**: "Programming" matches "Programming/Architecture"
- **IncludedCategories**: Only posts with these categories are included (OR logic)
- **ExcludedCategories**: Posts with these categories are excluded (highest priority)

### Tag Filtering
- **ExcludedTags**: Posts with these tags are excluded (highest priority)

## Post Structure

Each post includes:
- **Id**: Unique identifier (slug from filename)
- **Title**: Post title from frontmatter
- **Date**: Publication date
- **Description**: Summary from frontmatter or first 200 chars
- **Categories**: Hierarchical categories (e.g., "Programming/Architecture")
- **Tags**: Flat tags
- **Content**: Raw Markdown content
- **ContentHtml**: Rendered HTML
- **Stub**: Short summary
- **WordCount**: Calculated word count
- **Source**: "syndicated" or "local"
- **CanonicalUrl**: Original URL for SEO

## Development

### Adding a New Syndication Source

1. Create a new fetcher service (similar to `GitHubPostFetcher`)
2. Add configuration to `BlogSyndicationConfig`
3. Update `BlogSyndicationService` to call the new fetcher
4. Update `appsettings.json` with new source configuration

### Testing

```bash
dotnet build
dotnet run
```

Check the output in the console and verify `posts.json` was generated correctly.
