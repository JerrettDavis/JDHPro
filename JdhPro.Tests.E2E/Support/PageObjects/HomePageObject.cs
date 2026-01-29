using Microsoft.Playwright;

namespace JdhPro.Tests.E2E.Support.PageObjects;

/// <summary>
/// Example Page Object for the Homepage
/// Use this pattern for complex pages with many interactions
/// </summary>
public class HomePageObject
{
    private readonly IPage _page;

    // Locators
    private ILocator HeroHeading => _page.Locator("h1").First;
    private ILocator Tagline => _page.Locator(".tagline, [class*='tagline']");
    private ILocator ServiceCards => _page.Locator("[class*='service'], [class*='card']");
    private ILocator StartProjectButton => _page.GetByRole(AriaRole.Link, new() { Name = "Start Your Project" });
    private ILocator ContactSection => _page.Locator("#contact, [id*='contact']").First;

    public HomePageObject(IPage page)
    {
        _page = page;
    }

    // Actions
    public async Task NavigateAsync(string baseUrl)
    {
        await _page.GotoAsync(baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClickStartProjectAsync()
    {
        await StartProjectButton.ClickAsync();
        await Task.Delay(500); // Wait for scroll animation
    }

    // Assertions/Queries
    public async Task<string> GetHeroHeadingTextAsync()
    {
        return await HeroHeading.TextContentAsync() ?? string.Empty;
    }

    public async Task<bool> IsTaglineVisibleAsync()
    {
        return await Tagline.IsVisibleAsync();
    }

    public async Task<int> GetServiceCardCountAsync()
    {
        var cards = await ServiceCards.AllAsync();
        return cards.Count;
    }

    public async Task<bool> IsTextVisibleAsync(string text)
    {
        return await _page.GetByText(text, new() { Exact = false }).IsVisibleAsync();
    }

    public async Task<bool> IsContactSectionVisibleAsync()
    {
        return await ContactSection.IsVisibleAsync();
    }
}

// Example usage in step definitions:
/*
[Binding]
public class HomepageStepsWithPageObject
{
    private readonly WebDriverContext _context;
    private HomePageObject _homePage;

    public HomepageStepsWithPageObject(WebDriverContext context)
    {
        _context = context;
        _homePage = new HomePageObject(_context.Page!);
    }

    [Given(@"I navigate to the homepage")]
    public async Task GivenINavigateToTheHomepage()
    {
        await _homePage.NavigateAsync(_context.BaseUrl);
    }

    [Then(@"I should see the hero heading ""(.*)""")]
    public async Task ThenIShouldSeeTheHeroHeading(string expectedHeading)
    {
        var heading = await _homePage.GetHeroHeadingTextAsync();
        heading.Should().Contain(expectedHeading);
    }

    [When(@"I click ""Start Your Project""")]
    public async Task WhenIClickStartYourProject()
    {
        await _homePage.ClickStartProjectAsync();
    }

    [Then(@"I should be scrolled to the contact section")]
    public async Task ThenIShouldBeScrolledToTheContactSection()
    {
        var isVisible = await _homePage.IsContactSectionVisibleAsync();
        isVisible.Should().BeTrue();
    }
}
*/
