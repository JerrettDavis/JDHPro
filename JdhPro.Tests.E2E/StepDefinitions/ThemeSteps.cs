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
        var trigger = Page.GetByTestId("theme-select");
        var panel = Page.GetByTestId("theme-panel");
        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Page.GetByTestId($"theme-option-{theme}").ClickAsync();
        await Assertions.Expect(panel).Not.ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("data-theme-choice", theme);
        await Assertions.Expect(trigger).ToBeFocusedAsync();
    }

    [When(@"I open the appearance picker with the keyboard")]
    public async Task OpenAppearancePicker()
    {
        var trigger = Page.GetByTestId("theme-select");
        await trigger.FocusAsync();
        await trigger.PressAsync("Enter");
        await Assertions.Expect(Page.GetByTestId("theme-panel")).ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Then(@"the active theme option should have keyboard focus")]
    public async Task AssertOptionFocus() =>
        await Assertions.Expect(Page.GetByTestId("theme-panel").Locator("[aria-pressed='true']")).ToBeFocusedAsync();

    [When(@"I dismiss the appearance picker with Escape")]
    public async Task DismissAppearancePicker()
    {
        await Page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(Page.GetByTestId("theme-panel")).Not.ToBeVisibleAsync();
        await Assertions.Expect(Page.GetByTestId("theme-select")).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(Page.GetByTestId("theme-select")).ToBeFocusedAsync();
    }

    [Given(@"the device uses dark appearance")]
    public async Task SetDarkAppearance() =>
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });

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
