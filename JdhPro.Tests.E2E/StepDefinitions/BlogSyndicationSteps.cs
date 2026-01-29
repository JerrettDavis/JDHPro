using Reqnroll;
using FluentAssertions;
using JdhPro.Tests.E2E.Support.Helpers;
using JdhPro.Tests.E2E.Models;
using System.Diagnostics;

namespace JdhPro.Tests.E2E.StepDefinitions;

[Binding]
public class BlogSyndicationSteps
{
    private int _exitCode;
    private List<BlogPostDto> _posts = new();
    private FileInfo? _postsJsonFile;
    private string _syndicationOutput = string.Empty;

    [Given(@"the blog syndication tool is available")]
    public void GivenTheBlogSyndicationToolIsAvailable()
    {
        var buildToolsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "JdhPro.BuildTools",
            "JdhPro.BuildTools.csproj"
        );

        var fullPath = Path.GetFullPath(buildToolsPath);
        File.Exists(fullPath).Should().BeTrue($"Build tools project should exist at {fullPath}");
    }

    [When(@"I run the blog syndication tool")]
    public async Task WhenIRunTheBlogSyndicationTool()
    {
        var buildToolsDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "JdhPro.BuildTools"
        );

        var fullPath = Path.GetFullPath(buildToolsDir);

        // Run the syndication tool
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run --project JdhPro.BuildTools.csproj",
            WorkingDirectory = fullPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        
        var output = new System.Text.StringBuilder();
        var error = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
                Console.WriteLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
                Console.WriteLine($"ERROR: {e.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Wait up to 2 minutes for syndication to complete
        await Task.Run(() => process.WaitForExit(120000));

        if (!process.HasExited)
        {
            process.Kill();
            throw new TimeoutException("Blog syndication tool timed out after 2 minutes");
        }

        _exitCode = process.ExitCode;
        _syndicationOutput = output.ToString();

        if (error.Length > 0)
        {
            Console.WriteLine($"Syndication errors: {error}");
        }
    }

    [Then(@"the syndication should complete successfully")]
    public void ThenTheSyndicationShouldCompleteSuccessfully()
    {
        _exitCode.Should().Be(0, $"Syndication should exit with code 0. Output:\n{_syndicationOutput}");
        _syndicationOutput.Should().Contain("successfully", "Syndication output should indicate success");
    }

    [Then(@"posts\.json should be generated")]
    public void ThenPostsJsonShouldBeGenerated()
    {
        var postsJsonPath = BlogTestHelpers.GetPostsJsonPath();
        File.Exists(postsJsonPath).Should().BeTrue($"posts.json should exist at {postsJsonPath}");
        _postsJsonFile = new FileInfo(postsJsonPath);
    }

    [Then(@"posts\.json should contain valid data")]
    public async Task ThenPostsJsonShouldContainValidData()
    {
        var posts = await BlogTestHelpers.LoadPostsJsonAsync();
        posts.Should().NotBeNull("posts.json should deserialize successfully");
        posts.Should().NotBeEmpty("posts.json should contain at least one post");
    }

    [Given(@"blog syndication has completed")]
    public async Task GivenBlogSyndicationHasCompleted()
    {
        // Check if posts.json exists, if not, run syndication
        var postsJsonPath = BlogTestHelpers.GetPostsJsonPath();
        
        if (!File.Exists(postsJsonPath))
        {
            await WhenIRunTheBlogSyndicationTool();
            ThenTheSyndicationShouldCompleteSuccessfully();
        }

        _postsJsonFile = new FileInfo(postsJsonPath);
    }

    [When(@"I load the posts\.json file")]
    public async Task WhenILoadThePostsJsonFile()
    {
        _posts = await BlogTestHelpers.LoadPostsJsonAsync();
    }

    [Then(@"all posts should have required fields")]
    public void ThenAllPostsShouldHaveRequiredFields()
    {
        BlogTestHelpers.ValidateAllPostsStructure(_posts);
    }

    [Then(@"all posts should have an ID")]
    public void ThenAllPostsShouldHaveAnId()
    {
        _posts.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Id), "All posts must have an ID");
    }

    [Then(@"all posts should have a title")]
    public void ThenAllPostsShouldHaveATitle()
    {
        _posts.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Title), "All posts must have a title");
    }

    [Then(@"all posts should have a date")]
    public void ThenAllPostsShouldHaveADate()
    {
        _posts.Should().OnlyContain(p => p.Date != default(DateTime), "All posts must have a valid date");
    }

    [Then(@"all posts should have content")]
    public void ThenAllPostsShouldHaveContent()
    {
        _posts.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Content), "All posts must have content");
    }

    [Then(@"all posts should have HTML content")]
    public void ThenAllPostsShouldHaveHtmlContent()
    {
        _posts.Should().OnlyContain(p => !string.IsNullOrEmpty(p.ContentHtml), "All posts must have HTML content");
    }

    [Then(@"all posts should have a stub")]
    public void ThenAllPostsShouldHaveAStub()
    {
        _posts.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Stub), "All posts must have a stub");
    }

    [Then(@"no posts should have the ""([^""]*)"" category")]
    public void ThenNoPostsShouldHaveTheCategory(string excludedCategory)
    {
        BlogTestHelpers.ValidateNoExcludedCategories(_posts, excludedCategory);
    }

    [Then(@"no posts should have the ""([^""]*)"" tag")]
    public void ThenNoPostsShouldHaveTheTag(string excludedTag)
    {
        BlogTestHelpers.ValidateNoExcludedTags(_posts, excludedTag);
    }

    [Then(@"all syndicated posts should have a canonical URL")]
    public void ThenAllSyndicatedPostsShouldHaveACanonicalUrl()
    {
        var syndicatedPosts = BlogTestHelpers.GetPostsBySource(_posts, "syndicated");
        
        if (syndicatedPosts.Any())
        {
            syndicatedPosts.Should().OnlyContain(
                p => !string.IsNullOrEmpty(p.CanonicalUrl),
                "All syndicated posts must have a canonical URL"
            );
        }
    }

    [Then(@"all canonical URLs should be valid HTTP URLs")]
    public void ThenAllCanonicalUrlsShouldBeValidHttpUrls()
    {
        BlogTestHelpers.ValidateSyndicatedPostsHaveCanonicalUrls(_posts);
    }

    [Then(@"posts should be sorted by date descending")]
    public void ThenPostsShouldBeSortedByDateDescending()
    {
        BlogTestHelpers.ValidatePostsSortedByDateDescending(_posts);
    }

    [Then(@"HTML content should not contain raw markdown")]
    public void ThenHtmlContentShouldNotContainRawMarkdown()
    {
        foreach (var post in _posts)
        {
            BlogTestHelpers.ValidateHtmlContent(post);
        }
    }

    [When(@"I check the posts\.json file")]
    public void WhenICheckThePostsJsonFile()
    {
        var postsJsonPath = BlogTestHelpers.GetPostsJsonPath();
        _postsJsonFile = new FileInfo(postsJsonPath);
    }

    [Then(@"the file size should be reasonable")]
    public void ThenTheFileSizeShouldBeReasonable()
    {
        _postsJsonFile.Should().NotBeNull("posts.json file should exist");
        BlogTestHelpers.ValidateJsonFileSize(_postsJsonFile!.Length);
    }

    [Then(@"the file should contain at least (.*) post")]
    public async Task ThenTheFileShouldContainAtLeastPost(int minPosts)
    {
        _posts = await BlogTestHelpers.LoadPostsJsonAsync();
        _posts.Count.Should().BeGreaterThanOrEqualTo(minPosts, $"posts.json should contain at least {minPosts} post(s)");
    }

    [Then(@"the file should be smaller than (.*)MB")]
    public void ThenTheFileShouldBeSmallerThanMb(int maxMegabytes)
    {
        _postsJsonFile.Should().NotBeNull("posts.json file should exist");
        var maxBytes = maxMegabytes * 1024 * 1024;
        _postsJsonFile!.Length.Should().BeLessThan(maxBytes, $"posts.json should be smaller than {maxMegabytes}MB");
    }

    [Then(@"all posts should have valid metadata")]
    public void ThenAllPostsShouldHaveValidMetadata()
    {
        foreach (var post in _posts)
        {
            BlogTestHelpers.ValidatePostMetadata(post, requireDescription: false);
        }
    }

    [Then(@"all stubs should be URL-friendly")]
    public void ThenAllStubsShouldBeUrlFriendly()
    {
        _posts.Should().OnlyContain(
            p => !p.Stub.Contains(" ") && p.Stub == p.Stub.ToLower(),
            "All stubs should be lowercase and contain no spaces"
        );
    }

    [Then(@"all word counts should be greater than zero")]
    public void ThenAllWordCountsShouldBeGreaterThanZero()
    {
        _posts.Should().OnlyContain(p => p.WordCount > 0, "All posts should have a word count greater than zero");
    }

    [Then(@"only Technical posts should be included")]
    public void ThenOnlyTechnicalPostsShouldBeIncluded()
    {
        // All posts should have at least one technical category
        _posts.Should().OnlyContain(
            p => p.Categories.Contains("Technical", StringComparer.OrdinalIgnoreCase),
            "All posts should be in the Technical category"
        );
    }

    [Then(@"posts should have at least one included category")]
    public void ThenPostsShouldHaveAtLeastOneIncludedCategory()
    {
        _posts.Should().OnlyContain(p => p.Categories.Any(), "All posts should have at least one category");
    }

    [Then(@"all syndicated posts should have source set to ""([^""]*)""")]
    public void ThenAllSyndicatedPostsShouldHaveSourceSetTo(string expectedSource)
    {
        var syndicatedPosts = _posts.Where(p => !string.IsNullOrEmpty(p.CanonicalUrl)).ToList();
        
        if (syndicatedPosts.Any())
        {
            syndicatedPosts.Should().OnlyContain(
                p => p.Source.Equals(expectedSource, StringComparison.OrdinalIgnoreCase),
                $"All syndicated posts should have source = '{expectedSource}'"
            );
        }
    }

    [Then(@"syndicated posts should have a canonical URL")]
    public void ThenSyndicatedPostsShouldHaveACanonicalUrl()
    {
        var syndicatedPosts = BlogTestHelpers.GetPostsBySource(_posts, "syndicated");
        
        if (syndicatedPosts.Any())
        {
            BlogTestHelpers.ValidateSyndicatedPostsHaveCanonicalUrls(syndicatedPosts);
        }
    }

    [Then(@"the tool should handle errors gracefully")]
    public void ThenTheToolShouldHandleErrorsGracefully()
    {
        // The tool should exit with code 0 even if some posts fail
        // Or exit with code 1 but provide meaningful error messages
        if (_exitCode != 0)
        {
            _syndicationOutput.Should().Contain("Error", "Error output should contain error information");
        }
    }

    [Then(@"should continue processing valid posts")]
    public void ThenShouldContinueProcessingValidPosts()
    {
        // Even if there are errors, valid posts should still be processed
        var postsJsonPath = BlogTestHelpers.GetPostsJsonPath();
        
        if (File.Exists(postsJsonPath))
        {
            var posts = BlogTestHelpers.LoadPostsJsonAsync().GetAwaiter().GetResult();
            // If any posts were processed, consider it a partial success
            posts.Should().NotBeNull("Some posts should be processed even with errors");
        }
    }
}
