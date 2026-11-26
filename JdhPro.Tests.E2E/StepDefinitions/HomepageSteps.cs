using FluentAssertions;
using Microsoft.Playwright;
using Reqnroll;
using JdhPro.Tests.E2E.Support;

namespace JdhPro.Tests.E2E.StepDefinitions;

[Binding]
public class HomepageSteps
{
    private readonly WebDriverContext _context;
    private IPage Page => _context.Page!;

    public HomepageSteps(WebDriverContext context)
    {
        _context = context;
    }

    [Given(@"I navigate to the homepage")]
    public async Task GivenINavigateToTheHomepage()
    {
        await Page.GotoAsync(_context.BaseUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await _context.WaitForBlazorAsync();
    }

    [Then(@"I should see the hero heading ""(.*)""")]
    public async Task ThenIShouldSeeTheHeroHeading(string expectedHeading)
    {
        var headingLocator = Page.Locator("h1").First;
        await headingLocator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        var heading = await headingLocator.TextContentAsync();
        heading.Should().Contain(expectedHeading);
    }

    [Then(@"I should see the tagline containing ""(.*)""")]
    public async Task ThenIShouldSeeTheTaglineContaining(string expectedText)
    {
        var content = await Page.ContentAsync();
        content.Should().Contain(expectedText, because: "the tagline should be visible on the homepage");
    }

    [Then(@"I should see (.*) service cards")]
    public async Task ThenIShouldSeeServiceCards(int expectedCount)
    {
        // Wait for service cards to load
        var serviceCards = Page.Locator("[data-testid='service-card']");
        await serviceCards.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        
        var count = await serviceCards.CountAsync();
        count.Should().Be(expectedCount, $"Expected exactly {expectedCount} service cards");
    }

    [Then(@"I should see ""(.*)""")]
    public async Task ThenIShouldSee(string expectedText)
    {
        var locator = Page.GetByText(expectedText, new() { Exact = false }).First;
        await Assertions.Expect(locator).ToBeVisibleAsync(new() { Timeout = 15000 });
    }

    [When(@"I click ""(.*)""")]
    public async Task WhenIClick(string buttonText)
    {
        // Try button role first, then link role as fallback
        var buttonLocator = Page.GetByRole(AriaRole.Button, new() { Name = buttonText, Exact = false });
        var linkLocator = Page.GetByRole(AriaRole.Link, new() { Name = buttonText, Exact = false });
        
        var buttonCount = await buttonLocator.CountAsync();
        if (buttonCount > 0)
        {
            await buttonLocator.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await buttonLocator.First.ClickAsync();
        }
        else
        {
            await linkLocator.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await linkLocator.First.ClickAsync();
        }
    }

    [Then(@"I should be scrolled to the contact section")]
    public async Task ThenIShouldBeScrolledToTheContactSection()
    {
        await Assertions.Expect(Page.Locator("#contact")).ToBeInViewportAsync(new() { Timeout = 5000 });
    }
}
