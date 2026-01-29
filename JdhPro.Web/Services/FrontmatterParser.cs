using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using JdhPro.Web.Models;

namespace JdhPro.Web.Services;

/// <summary>
/// Parses YAML frontmatter from MDX/Markdown files
/// </summary>
public class FrontmatterParser
{
    private static readonly Regex FrontmatterRegex = new(
        @"^---\s*\n(.*?)\n---\s*\n(.*)$",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    private readonly IDeserializer _yamlDeserializer;

    public FrontmatterParser()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public (PostFrontmatter Frontmatter, string Content) Parse(string fileContent)
    {
        var match = FrontmatterRegex.Match(fileContent);
        
        if (!match.Success)
        {
            // No frontmatter found, return defaults
            return (new PostFrontmatter(), fileContent);
        }

        var yamlContent = match.Groups[1].Value;
        var markdownContent = match.Groups[2].Value.Trim();

        try
        {
            var frontmatter = _yamlDeserializer.Deserialize<PostFrontmatter>(yamlContent) 
                             ?? new PostFrontmatter();
            return (frontmatter, markdownContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing frontmatter: {ex.Message}");
            return (new PostFrontmatter(), markdownContent);
        }
    }

    /// <summary>
    /// Normalizes tags from string or string[] to List&lt;string&gt;
    /// </summary>
    public static List<string> NormalizeTags(object? tags)
    {
        if (tags == null)
            return new List<string>();
        
        return tags switch
        {
            string[] arr => arr.ToList(),
            string str => str.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList(),
            List<string> list => list,
            List<object> objList => objList.Select(o => o?.ToString() ?? "").Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Normalizes categories from string or string[] to List&lt;string&gt;
    /// </summary>
    public static List<string> NormalizeCategories(object? categories)
    {
        if (categories == null)
            return new List<string>();
        
        return categories switch
        {
            string[] arr => arr.ToList(),
            string str => new List<string> { str },
            List<string> list => list,
            List<object> objList => objList.Select(o => o?.ToString() ?? "").Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Parses series order from int or string
    /// </summary>
    public static int? ParseSeriesOrder(object? seriesOrder)
    {
        return seriesOrder switch
        {
            int i => i,
            string s when int.TryParse(s, out var result) => result,
            _ => null
        };
    }
}
