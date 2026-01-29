using System.Text.RegularExpressions;
using Markdig;
using Microsoft.Extensions.Logging;

namespace JdhPro.Web.Services;

/// <summary>
/// Renders Markdown to HTML and generates post metadata
/// </summary>
public class MarkdownService
{
    private readonly MarkdownPipeline _pipeline;
    private readonly ILogger<MarkdownService>? _logger;
    
    // Regex to strip MDX components like <Component {...props} />
    private static readonly Regex MdxComponentRegex = new(
        @"<[A-Z][a-zA-Z0-9]*(?:\s+[^>]*)?\s*/?>(?:.*?</[A-Z][a-zA-Z0-9]*>)?",
        RegexOptions.Compiled | RegexOptions.Singleline
    );

    public MarkdownService(ILogger<MarkdownService>? logger = null)
    {
        _logger = logger;
        
        // Configure Markdig pipeline with common extensions
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UsePipeTables()
            .UseAutoLinks()
            .Build();
    }

    /// <summary>
    /// Renders Markdown content to HTML
    /// </summary>
    public string RenderToHtml(string markdown)
    {
        try
        {
            // Strip MDX components before rendering
            var cleanedMarkdown = StripMdxComponents(markdown);
            
            // Render to HTML
            var html = Markdown.ToHtml(cleanedMarkdown, _pipeline);
            
            return html;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error rendering markdown to HTML");
            return $"<p>Error rendering content</p>";
        }
    }

    /// <summary>
    /// Generates a summary from markdown content
    /// First 200 characters or first paragraph, whichever is shorter
    /// </summary>
    public string GenerateSummary(string markdown, int maxLength = 200)
    {
        try
        {
            // Strip MDX components
            var cleanedMarkdown = StripMdxComponents(markdown);
            
            // Convert to plain text (remove markdown syntax)
            var plainText = ConvertToPlainText(cleanedMarkdown);
            
            // Get first paragraph or maxLength characters
            var lines = plainText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var firstParagraph = lines.FirstOrDefault()?.Trim() ?? string.Empty;
            
            if (firstParagraph.Length <= maxLength)
            {
                return firstParagraph;
            }
            
            // Truncate to maxLength at word boundary
            var truncated = firstParagraph.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');
            
            if (lastSpace > 0)
            {
                truncated = truncated.Substring(0, lastSpace);
            }
            
            return truncated + "...";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error generating summary");
            return string.Empty;
        }
    }

    /// <summary>
    /// Calculates word count from markdown content
    /// </summary>
    public int CalculateWordCount(string markdown)
    {
        try
        {
            // Strip MDX components and convert to plain text
            var cleanedMarkdown = StripMdxComponents(markdown);
            var plainText = ConvertToPlainText(cleanedMarkdown);
            
            // Count words
            var words = plainText.Split(new[] { ' ', '\n', '\r', '\t' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            return words.Length;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error calculating word count");
            return 0;
        }
    }

    /// <summary>
    /// Strips MDX components from content
    /// </summary>
    private string StripMdxComponents(string content)
    {
        return MdxComponentRegex.Replace(content, string.Empty);
    }

    /// <summary>
    /// Converts markdown to plain text by removing common syntax
    /// </summary>
    private string ConvertToPlainText(string markdown)
    {
        var text = markdown;
        
        // Remove code blocks
        text = Regex.Replace(text, @"```[\s\S]*?```", string.Empty);
        text = Regex.Replace(text, @"`[^`]+`", string.Empty);
        
        // Remove links but keep text: [text](url) -> text
        text = Regex.Replace(text, @"\[([^\]]+)\]\([^\)]+\)", "$1");
        
        // Remove images: ![alt](url)
        text = Regex.Replace(text, @"!\[([^\]]*)\]\([^\)]+\)", string.Empty);
        
        // Remove bold/italic: **text** or *text* -> text
        text = Regex.Replace(text, @"\*\*([^\*]+)\*\*", "$1");
        text = Regex.Replace(text, @"\*([^\*]+)\*", "$1");
        text = Regex.Replace(text, @"__([^_]+)__", "$1");
        text = Regex.Replace(text, @"_([^_]+)_", "$1");
        
        // Remove headings: ## Heading -> Heading
        text = Regex.Replace(text, @"^#+\s+", string.Empty, RegexOptions.Multiline);
        
        // Remove blockquotes: > text -> text
        text = Regex.Replace(text, @"^>\s+", string.Empty, RegexOptions.Multiline);
        
        // Remove horizontal rules
        text = Regex.Replace(text, @"^[\-\*_]{3,}$", string.Empty, RegexOptions.Multiline);
        
        // Clean up extra whitespace
        text = Regex.Replace(text, @"\s+", " ");
        
        return text.Trim();
    }
}
