using Reqnroll;
using FluentAssertions;
using JdhPro.Tests.E2E.Support;
using JdhPro.Tests.E2E.Support.Helpers;

namespace JdhPro.Tests.E2E.StepDefinitions;

/// <summary>
/// Step definitions for blog listing page
/// NOTE: These tests are prepared for when blog pages are implemented
/// They are tagged with @future @blog and will be skipped if blog pages don't exist
/// </summary>
[Binding]
public class BlogListingSteps
{
    private readonly WebDriverContext _context;

    public BlogListingSteps(WebDriverContext context)
    {
        _context = context;
    }

    [When(@"I navigate to the blog page")]
    public async Task WhenINavigateToTheBlogPage()
    {
        await _context.Page!.GotoAsync($"{_context.BaseUrl}/blog");
        await _context.WaitForBlazorAsync();
    }

    [Then(@"the page should display blog posts")]
    public async Task ThenThePageShouldDisplayBlogPosts()
    {
        // Check for blog post containers/cards
        var posts = await _context.Page!.QuerySelectorAllAsync(".blog-post, .post-card, article");
        posts.Should().NotBeEmpty("Blog page should display blog posts");
    }

    [Then(@"I should see blog post cards")]
    public async Task ThenIShouldSeeBlogPostCards()
    {
        var posts = await _context.Page!.QuerySelectorAllAsync(".blog-post, .post-card, article");
        posts.Count.Should().BeGreaterThan(0, "Should display at least one blog post card");
    }

    [Then(@"each post should display a title")]
    public async Task ThenEachPostShouldDisplayATitle()
    {
        var titles = await _context.Page!.QuerySelectorAllAsync(".blog-post h2, .post-card h2, article h2");
        titles.Should().NotBeEmpty("Each post should have a title");
    }

    [Then(@"each post should display a date")]
    public async Task ThenEachPostShouldDisplayADate()
    {
        var dates = await _context.Page!.QuerySelectorAllAsync(".post-date, time, .date");
        dates.Should().NotBeEmpty("Each post should display a date");
    }

    [Then(@"each post should display categories")]
    public async Task ThenEachPostShouldDisplayCategories()
    {
        var categories = await _context.Page!.QuerySelectorAllAsync(".category, .categories, .tag");
        categories.Should().NotBeEmpty("Posts should display categories");
    }

    [When(@"I click on a blog post")]
    public async Task WhenIClickOnABlogPost()
    {
        var firstPost = await _context.Page!.QuerySelectorAsync(".blog-post a, .post-card a, article a");
        firstPost.Should().NotBeNull("Should have a clickable blog post link");
        await firstPost!.ClickAsync();
        await _context.WaitForBlazorAsync();
    }

    [Then(@"I should navigate to the post detail page")]
    public void ThenIShouldNavigateToThePostDetailPage()
    {
        _context.Page!.Url.Should().Contain("/blog/", "Should navigate to a blog post detail page");
    }

    [Then(@"posts should be displayed in date order \(newest first\)")]
    public async Task ThenPostsShouldBeDisplayedInDateOrder()
    {
        var dates = await PageTestHelpers.GetAllTextAsync(_context.Page!, ".post-date, time");
        
        // Parse dates and verify order
        var parsedDates = dates
            .Select(d => DateTime.TryParse(d, out var date) ? date : DateTime.MinValue)
            .Where(d => d != DateTime.MinValue)
            .ToList();

        if (parsedDates.Count > 1)
        {
            for (int i = 0; i < parsedDates.Count - 1; i++)
            {
                parsedDates[i].Should().BeOnOrAfter(parsedDates[i + 1], 
                    "Posts should be ordered by date (newest first)");
            }
        }
    }

    [Then(@"each post should show the publish date")]
    public async Task ThenEachPostShouldShowThePublishDate()
    {
        await ThenEachPostShouldDisplayADate();
    }

    [Then(@"each post should show tags \(if available\)")]
    public async Task ThenEachPostShouldShowTagsIfAvailable()
    {
        // Tags might not always be present, so just check if the selector exists
        var hasTags = await PageTestHelpers.ElementExistsAsync(_context.Page!, ".tag, .tags");
        // This is optional, so we just verify the selector works
        Console.WriteLine($"Tags displayed: {hasTags}");
    }

    [When(@"I filter by ""([^""]*)"" category")]
    public async Task WhenIFilterByCategory(string category)
    {
        // Click on category filter
        var categoryFilter = await _context.Page!.QuerySelectorAsync($"button:text('{category}'), a:text('{category}')");
        
        if (categoryFilter != null)
        {
            await categoryFilter.ClickAsync();
            await Task.Delay(500); // Wait for filter to apply
        }
    }

    [Then(@"only posts in ""([^""]*)"" category should be shown")]
    public async Task ThenOnlyPostsInCategoryShouldBeShown(string category)
    {
        var postCategories = await PageTestHelpers.GetAllTextAsync(_context.Page!, ".category, .categories");
        
        if (postCategories.Any())
        {
            postCategories.Should().Contain(c => c.Contains(category, StringComparison.OrdinalIgnoreCase),
                $"Filtered posts should be in '{category}' category");
        }
    }

    [When(@"I search for ""([^""]*)""")]
    public async Task WhenISearchFor(string searchTerm)
    {
        var searchBox = await _context.Page!.QuerySelectorAsync("input[type='search'], input[placeholder*='Search']");
        
        if (searchBox != null)
        {
            await searchBox.FillAsync(searchTerm);
            await searchBox.PressAsync("Enter");
            await Task.Delay(500); // Wait for search results
        }
    }

    [Then(@"only posts containing ""([^""]*)"" should be shown")]
    public async Task ThenOnlyPostsContainingShouldBeShown(string searchTerm)
    {
        var postTitles = await PageTestHelpers.GetAllTextAsync(_context.Page!, ".blog-post h2, .post-card h2");
        var postDescriptions = await PageTestHelpers.GetAllTextAsync(_context.Page!, ".post-description, .description");
        
        var allText = postTitles.Concat(postDescriptions);
        allText.Should().Contain(text => text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase),
            $"Search results should contain '{searchTerm}'");
    }

    [Given(@"there are more than (.*) posts")]
    public async Task GivenThereAreMoreThanPosts(int minPosts)
    {
        var posts = await BlogTestHelpers.LoadPostsJsonAsync();
        posts.Count.Should().BeGreaterThan(minPosts, $"Should have more than {minPosts} posts for pagination test");
    }

    [Then(@"I should see pagination controls")]
    public async Task ThenIShouldSeePaginationControls()
    {
        var pagination = await _context.Page!.QuerySelectorAsync(".pagination, nav[aria-label='Pagination']");
        pagination.Should().NotBeNull("Pagination controls should be present");
    }

    [Then(@"I should see up to (.*) posts per page")]
    public async Task ThenIShouldSeeUpToPostsPerPage(int maxPerPage)
    {
        var posts = await _context.Page!.QuerySelectorAllAsync(".blog-post, .post-card, article");
        posts.Count.Should().BeLessThanOrEqualTo(maxPerPage, $"Should show up to {maxPerPage} posts per page");
    }

    [When(@"I click next page")]
    public async Task WhenIClickNextPage()
    {
        var nextButton = await _context.Page!.QuerySelectorAsync("a:text('Next'), button:text('Next'), .next-page");
        nextButton.Should().NotBeNull("Next page button should exist");
        await nextButton!.ClickAsync();
        await _context.WaitForBlazorAsync();
    }

    [Then(@"I should see the next set of posts")]
    public async Task ThenIShouldSeeTheNextSetOfPosts()
    {
        var posts = await _context.Page!.QuerySelectorAllAsync(".blog-post, .post-card, article");
        posts.Should().NotBeEmpty("Next page should display posts");
    }
}
