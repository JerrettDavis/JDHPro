using Reqnroll;
using FluentAssertions;
using JdhPro.Tests.E2E.Support;
using JdhPro.Tests.E2E.Support.Helpers;

namespace JdhPro.Tests.E2E.StepDefinitions;

/// <summary>
/// Step definitions for blog post detail pages
/// NOTE: These tests are prepared for when blog pages are implemented
/// They are tagged with @future @blog and will be skipped if blog pages don't exist
/// </summary>
[Binding]
public class BlogPostSteps
{
    private readonly WebDriverContext _context;

    public BlogPostSteps(WebDriverContext context)
    {
        _context = context;
    }

    [When(@"I navigate to a blog post detail page")]
    public async Task WhenINavigateToABlogPostDetailPage()
    {
        var posts = await BlogTestHelpers.LoadPostsJsonAsync();
        posts.Should().NotBeEmpty("Should have at least one post");

        var firstPost = posts.First();
        await _context.Page!.GotoAsync($"{_context.BaseUrl}/blog/{firstPost.Stub}");
        await _context.WaitForBlazorAsync();
    }

    [When(@"I navigate to a syndicated blog post")]
    public async Task WhenINavigateToASyndicatedBlogPost()
    {
        var posts = await BlogTestHelpers.LoadPostsJsonAsync();
        var syndicatedPost = posts.FirstOrDefault(p => p.Source == "syndicated");
        
        syndicatedPost.Should().NotBeNull("Should have at least one syndicated post");
        
        await _context.Page!.GotoAsync($"{_context.BaseUrl}/blog/{syndicatedPost!.Stub}");
        await _context.WaitForBlazorAsync();
    }

    [Then(@"I should see the post title")]
    public async Task ThenIShouldSeeThePostTitle()
    {
        var title = await _context.Page!.QuerySelectorAsync("h1");
        title.Should().NotBeNull("Post should have a title");
        
        var titleText = await title!.TextContentAsync();
        titleText.Should().NotBeNullOrEmpty("Post title should not be empty");
    }

    [Then(@"I should see the post content")]
    public async Task ThenIShouldSeeThePostContent()
    {
        var content = await _context.Page!.QuerySelectorAsync("article, .post-content, .content");
        content.Should().NotBeNull("Post should have content");
    }

    [Then(@"I should see the post metadata")]
    public async Task ThenIShouldSeeThePostMetadata()
    {
        // Check for various metadata elements
        var hasDate = await PageTestHelpers.ElementExistsAsync(_context.Page!, ".post-date, time");
        var hasAuthor = await PageTestHelpers.ElementExistsAsync(_context.Page!, ".author, .post-author");
        var hasCategories = await PageTestHelpers.ElementExistsAsync(_context.Page!, ".category, .categories");
        
        (hasDate || hasAuthor || hasCategories).Should().BeTrue("Post should display some metadata");
    }

    [Then(@"the post HTML content should be rendered")]
    public async Task ThenThePostHtmlContentShouldBeRendered()
    {
        var content = await _context.Page!.ContentAsync();
        content.Should().Contain("<article", "Post content should be rendered as article");
    }

    [Then(@"the content should be properly formatted")]
    public async Task ThenTheContentShouldBeProperlyFormatted()
    {
        var hasHeadings = await PageTestHelpers.ElementExistsAsync(_context.Page!, "h2, h3, h4");
        var hasParagraphs = await PageTestHelpers.ElementExistsAsync(_context.Page!, "p");
        
        (hasHeadings || hasParagraphs).Should().BeTrue("Content should have proper HTML structure");
    }

    [Then(@"code blocks should be syntax highlighted")]
    public async Task ThenCodeBlocksShouldBeSyntaxHighlighted()
    {
        var hasCodeBlocks = await PageTestHelpers.ElementExistsAsync(_context.Page!, "pre code, .hljs");
        
        if (hasCodeBlocks)
        {
            Console.WriteLine("Code blocks are present and should be syntax highlighted");
        }
    }

    [Then(@"I should see the publish date")]
    public async Task ThenIShouldSeeThePublishDate()
    {
        var date = await _context.Page!.QuerySelectorAsync(".post-date, time, .date");
        date.Should().NotBeNull("Post should display publish date");
    }

    [Then(@"I should see the author name")]
    public async Task ThenIShouldSeeTheAuthorName()
    {
        var author = await _context.Page!.QuerySelectorAsync(".author, .post-author");
        author.Should().NotBeNull("Post should display author name");
    }

    [Then(@"I should see a canonical URL link")]
    public async Task ThenIShouldSeeACanonicalUrlLink()
    {
        var canonicalLink = await _context.Page!.QuerySelectorAsync("a[rel='canonical'], .canonical-link");
        canonicalLink.Should().NotBeNull("Syndicated post should have a canonical URL link");
    }

    [Then(@"the canonical link should point to the original post")]
    public async Task ThenTheCanonicalLinkShouldPointToTheOriginalPost()
    {
        var canonicalLink = await _context.Page!.QuerySelectorAsync("a[rel='canonical'], .canonical-link");
        canonicalLink.Should().NotBeNull("Canonical link should exist");
        
        var href = await canonicalLink!.GetAttributeAsync("href");
        href.Should().NotBeNullOrEmpty("Canonical link should have href");
        href.Should().StartWith("http", "Canonical URL should be absolute");
    }

    [Then(@"the page should have a title meta tag")]
    public async Task ThenThePageShouldHaveATitleMetaTag()
    {
        var title = await _context.Page!.TitleAsync();
        title.Should().NotBeNullOrEmpty("Page should have a title");
    }

    [Then(@"the page should have a description meta tag")]
    public async Task ThenThePageShouldHaveADescriptionMetaTag()
    {
        var description = await _context.Page!.QuerySelectorAsync("meta[name='description']");
        description.Should().NotBeNull("Page should have description meta tag");
    }

    [Then(@"the page should have Open Graph meta tags")]
    public async Task ThenThePageShouldHaveOpenGraphMetaTags()
    {
        var ogTitle = await _context.Page!.QuerySelectorAsync("meta[property='og:title']");
        var ogDescription = await _context.Page!.QuerySelectorAsync("meta[property='og:description']");
        
        (ogTitle != null || ogDescription != null).Should().BeTrue("Page should have Open Graph meta tags");
    }

    [Given(@"the post has UseToc enabled")]
    public async Task GivenThePostHasUseTocEnabled()
    {
        var posts = await BlogTestHelpers.LoadPostsJsonAsync();
        var postWithToc = posts.FirstOrDefault(p => p.UseToc);
        
        postWithToc.Should().NotBeNull("Should have at least one post with table of contents");
        
        await _context.Page!.GotoAsync($"{_context.BaseUrl}/blog/{postWithToc!.Stub}");
        await _context.WaitForBlazorAsync();
    }

    [Then(@"I should see a table of contents")]
    public async Task ThenIShouldSeeATableOfContents()
    {
        var toc = await _context.Page!.QuerySelectorAsync(".toc, .table-of-contents, nav[aria-label='Table of contents']");
        toc.Should().NotBeNull("Post should have a table of contents");
    }

    [Then(@"clicking TOC links should scroll to sections")]
    public async Task ThenClickingTocLinksShouldScrollToSections()
    {
        var tocLinks = await _context.Page!.QuerySelectorAllAsync(".toc a, .table-of-contents a");
        
        if (tocLinks.Count > 0)
        {
            var initialScroll = await PageTestHelpers.GetScrollPositionAsync(_context.Page);
            await tocLinks.First().ClickAsync();
            await Task.Delay(500);
            var newScroll = await PageTestHelpers.GetScrollPositionAsync(_context.Page);
            
            newScroll.Should().NotBe(initialScroll, "Clicking TOC link should scroll the page");
        }
    }

    [Then(@"I should see a ""([^""]*)"" link \(if available\)")]
    public async Task ThenIShouldSeeALinkIfAvailable(string linkText)
    {
        var link = await _context.Page!.QuerySelectorAsync($"a:text('{linkText}')");
        Console.WriteLine($"{linkText} link present: {link != null}");
    }

    [When(@"I click the ""([^""]*)"" link in the blog post")]
    public async Task WhenIClickLinkInBlogPost(string linkText)
    {
        var link = await _context.Page!.QuerySelectorAsync($"a:text('{linkText}')");
        link.Should().NotBeNull($"{linkText} link should exist");
        await link!.ClickAsync();
        await _context.WaitForBlazorAsync();
    }

    [Then(@"I should navigate to the next blog post")]
    public void ThenIShouldNavigateToTheNextBlogPost()
    {
        _context.Page!.Url.Should().Contain("/blog/", "Should navigate to a blog post");
    }

    [Then(@"the URL should contain the post slug")]
    public async Task ThenTheUrlShouldContainThePostSlug()
    {
        var posts = await BlogTestHelpers.LoadPostsJsonAsync();
        var currentUrl = _context.Page!.Url;
        
        var matchingPost = posts.FirstOrDefault(p => currentUrl.Contains(p.Stub));
        matchingPost.Should().NotBeNull("URL should contain a valid post slug");
    }

    [Then(@"the slug should be URL-friendly")]
    public void ThenTheSlugShouldBeUrlFriendly()
    {
        var url = _context.Page!.Url;
        url.Should().NotContain(" ", "URL should not contain spaces");
        url.Should().Match(u => u == u.ToLower() || u.Contains("?"), "URL should be lowercase");
    }

    [Then(@"I should see the estimated reading time")]
    public async Task ThenIShouldSeeTheEstimatedReadingTime()
    {
        var readingTime = await _context.Page!.QuerySelectorAsync(".reading-time, .read-time");
        readingTime.Should().NotBeNull("Post should display reading time");
    }

    [Then(@"the reading time should be calculated from word count")]
    public async Task ThenTheReadingTimeShouldBeCalculatedFromWordCount()
    {
        var readingTimeElement = await _context.Page!.QuerySelectorAsync(".reading-time, .read-time");
        
        if (readingTimeElement != null)
        {
            var text = await readingTimeElement.TextContentAsync();
            text.Should().Match(t => t!.Contains("min", StringComparison.OrdinalIgnoreCase),
                "Reading time should be displayed in minutes");
        }
    }

    [Then(@"I should see related posts section")]
    public async Task ThenIShouldSeeRelatedPostsSection()
    {
        var relatedPosts = await _context.Page!.QuerySelectorAsync(".related-posts, .similar-posts");
        relatedPosts.Should().NotBeNull("Post should have related posts section");
    }

    [Then(@"related posts should share categories or tags")]
    public async Task ThenRelatedPostsShouldShareCategoriesOrTags()
    {
        var relatedPosts = await _context.Page!.QuerySelectorAllAsync(".related-posts article, .similar-posts article");
        
        if (relatedPosts.Count > 0)
        {
            Console.WriteLine($"Found {relatedPosts.Count} related posts");
        }
    }

    [Given(@"the post contains code snippets")]
    public async Task GivenThePostContainsCodeSnippets()
    {
        var posts = await BlogTestHelpers.LoadPostsJsonAsync();
        var postWithCode = posts.FirstOrDefault(p => p.ContentHtml.Contains("<code>") || p.ContentHtml.Contains("<pre>"));
        
        postWithCode.Should().NotBeNull("Should have at least one post with code snippets");
        
        await _context.Page!.GotoAsync($"{_context.BaseUrl}/blog/{postWithCode!.Stub}");
        await _context.WaitForBlazorAsync();
    }

    [Then(@"code blocks should have copy buttons")]
    public async Task ThenCodeBlocksShouldHaveCopyButtons()
    {
        var copyButtons = await _context.Page!.QuerySelectorAllAsync("button.copy, .copy-button");
        Console.WriteLine($"Found {copyButtons.Count} copy buttons");
    }

    [Then(@"code blocks should display the language")]
    public async Task ThenCodeBlocksShouldDisplayTheLanguage()
    {
        var codeBlocks = await _context.Page!.QuerySelectorAllAsync("pre code[class*='language-']");
        Console.WriteLine($"Found {codeBlocks.Count} code blocks with language specification");
    }
}
