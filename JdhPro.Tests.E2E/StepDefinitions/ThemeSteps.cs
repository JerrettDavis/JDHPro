using FluentAssertions;
using Microsoft.Playwright;
using Reqnroll;
using JdhPro.Tests.E2E.Support;

namespace JdhPro.Tests.E2E.StepDefinitions;

[Binding]
public class ThemeSteps(WebDriverContext context)
{
    private IPage Page => context.Page!;

    [When(@"I select the website theme ""(.*)""")]
    public async Task SelectTheme(string theme)
    {
        await Page.GetByTestId("theme-select").SelectOptionAsync(theme);
    }

    [Then(@"the resolved website theme should be ""(.*)""")]
    public async Task AssertTheme(string theme)
    {
        await Assertions.Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", theme);
    }

    [When(@"I reload the website")]
    public async Task ReloadWebsite()
    {
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await context.WaitForBlazorAsync();
    }

    [When(@"I open a theme URL for ""(.*)""")]
    public async Task OpenThemeUrl(string theme)
    {
        await Page.GotoAsync(context.BaseUrl.TrimEnd('/') + "/?theme=" + Uri.EscapeDataString(theme));
        await context.WaitForBlazorAsync();
    }

    [Then(@"the website theme selector should remain visible")]
    public async Task SelectorVisible() =>
        await Assertions.Expect(Page.GetByTestId("theme-select")).ToBeVisibleAsync();
}
