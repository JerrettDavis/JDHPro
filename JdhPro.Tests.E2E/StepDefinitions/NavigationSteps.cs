using FluentAssertions;
using Microsoft.Playwright;
using Reqnroll;
using JdhPro.Tests.E2E.Support;

namespace JdhPro.Tests.E2E.StepDefinitions;

[Binding]
public class NavigationSteps
{
    private readonly WebDriverContext _context;
    private IPage Page => _context.Page!;

    public NavigationSteps(WebDriverContext context)
    {
        _context = context;
    }

    [When(@"I click ""(.*)"" in the navigation")]
    public async Task WhenIClickInTheNavigation(string linkText)
    {
        var nav = Page.Locator("nav, header");
        await nav.GetByRole(AriaRole.Link, new() { Name = linkText, Exact = false }).ClickAsync();
        await _context.WaitForBlazorAsync();
    }

    [Then(@"I should be on the ""(.*)"" page")]
    public async Task ThenIShouldBeOnThePage(string expectedPath)
    {
        await Page.WaitForURLAsync($"**{expectedPath}");
        var currentUrl = Page.Url;
        currentUrl.Should().Contain(expectedPath);
    }

    [Given(@"I am on a mobile device")]
    public async Task GivenIAmOnAMobileDevice()
    {
        // This is handled by the Hooks class when it detects "mobile" in scenario
        // Just verify viewport is mobile size
        var viewport = Page.ViewportSize;
        viewport.Should().NotBeNull();
        viewport!.Width.Should().BeLessThan(768, "Mobile devices have viewport width < 768px");
    }

    [When(@"I click the mobile menu button")]
    public async Task WhenIClickTheMobileMenuButton()
    {
        var menuButton = Page.Locator("[data-testid='mobile-menu-button']");
        await menuButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await menuButton.ClickAsync();
        await Task.Delay(500); // Wait for menu animation
    }

    [Then(@"I should see the mobile navigation menu")]
    public async Task ThenIShouldSeeTheMobileNavigationMenu()
    {
        var mobileNav = Page.Locator("[data-testid='mobile-menu']");
        await mobileNav.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        var isVisible = await mobileNav.IsVisibleAsync();
        isVisible.Should().BeTrue("Mobile navigation menu should be visible");
    }
}
