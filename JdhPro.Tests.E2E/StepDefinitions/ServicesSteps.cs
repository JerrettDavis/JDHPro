using FluentAssertions;
using Microsoft.Playwright;
using Reqnroll;
using JdhPro.Tests.E2E.Support;

namespace JdhPro.Tests.E2E.StepDefinitions;

[Binding]
public class ServicesSteps
{
    private readonly WebDriverContext _context;
    private IPage Page => _context.Page!;

    public ServicesSteps(WebDriverContext context)
    {
        _context = context;
    }

    [Then(@"I should see the page title ""(.*)""")]
    public async Task ThenIShouldSeeThePageTitle(string expectedTitle)
    {
        // Wait for page to fully load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10000 });
        
        var h1Locator = Page.Locator("h1").First;
        await h1Locator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        
        var title = await h1Locator.TextContentAsync();
        // Normalize whitespace for comparison
        var normalizedTitle = string.Join(" ", title!.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        normalizedTitle.Should().Contain(expectedTitle, $"Expected h1 to contain '{expectedTitle}' but got '{normalizedTitle}'");
    }

    [Then(@"I should see (.*) detailed service sections")]
    public async Task ThenIShouldSeeDetailedServiceSections(int expectedCount)
    {
        // Wait for loading spinner to disappear
        var spinner = Page.Locator(".animate-spin");
        if (await spinner.CountAsync() > 0)
        {
            await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        }
        
        // Wait for service sections to be visible
        var serviceSections = Page.Locator("[data-testid='service-section']");
        await serviceSections.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        
        var count = await serviceSections.CountAsync();
        count.Should().Be(expectedCount, $"Expected exactly {expectedCount} service sections");
    }

    [Then(@"each service should have a title")]
    public async Task ThenEachServiceShouldHaveATitle()
    {
        var titles = await Page.Locator("h2, h3").AllAsync();
        titles.Count.Should().BeGreaterThan(0, "Each service section should have a title");
    }

    [Then(@"each service should have a description")]
    public async Task ThenEachServiceShouldHaveADescription()
    {
        var descriptions = await Page.Locator("p").AllAsync();
        descriptions.Count.Should().BeGreaterThan(0, "Each service should have a description");
    }

    [Then(@"each service should have benefits")]
    public async Task ThenEachServiceShouldHaveBenefits()
    {
        var lists = await Page.Locator("ul, ol").AllAsync();
        lists.Count.Should().BeGreaterThan(0, "Services should have benefits listed");
    }

    [Then(@"each service should have a ""(.*)"" button")]
    public async Task ThenEachServiceShouldHaveAButton(string buttonText)
    {
        // Wait for service sections to load
        await Page.WaitForSelectorAsync("[data-testid='service-section']", new() { Timeout = 10000 });
        
        // Try to find buttons with the text (case-insensitive partial match)
        var buttons = await Page.Locator("button").AllAsync();
        var matchingButtons = new List<ILocator>();
        
        foreach (var button in buttons)
        {
            var text = await button.TextContentAsync();
            if (text?.Contains(buttonText.Replace(" ", "."), StringComparison.OrdinalIgnoreCase) == true ||
                text?.Contains(buttonText, StringComparison.OrdinalIgnoreCase) == true)
            {
                matchingButtons.Add(button);
            }
        }
        
        matchingButtons.Count.Should().BeGreaterThan(0, $"Each service should have a button containing '{buttonText}'");
    }
}
