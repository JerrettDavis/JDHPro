using Microsoft.Playwright;
using Reqnroll;
using JdhPro.Tests.E2E.Support;

namespace JdhPro.Tests.E2E.StepDefinitions;

[Binding]
public class ThemeSteps(WebDriverContext context)
{
    private IPage Page => context.Page!;
    private ILocator Trigger(string axis) => Page.GetByTestId($"{axis}-trigger");
    private ILocator Panel(string axis) => Page.GetByTestId($"{axis}-panel");

    [When(@"I select the website identity ""(.*)""")]
    public Task SelectIdentity(string value) => SelectAsync("identity", value);

    [When(@"I select the website color mode ""(.*)""")]
    public Task SelectMode(string value) => SelectAsync("mode", value);

    private async Task SelectAsync(string axis, string value)
    {
        await Trigger(axis).ClickAsync();
        await Assertions.Expect(Panel(axis)).ToBeVisibleAsync();
        await Page.GetByTestId($"{axis}-option-{value}").ClickAsync();
        await Assertions.Expect(Panel(axis)).Not.ToBeVisibleAsync();
        await Assertions.Expect(Trigger(axis)).ToHaveAttributeAsync($"data-{axis}", value);
        await Assertions.Expect(Trigger(axis)).ToBeFocusedAsync();
    }

    [When(@"I open the ""(.*)"" appearance menu with the keyboard")]
    public async Task OpenAppearancePicker(string axis)
    {
        await Assertions.Expect(Trigger(axis)).ToBeEnabledAsync();
        await Trigger(axis).FocusAsync();
        await Trigger(axis).PressAsync("Enter");
        await Assertions.Expect(Panel(axis)).ToBeVisibleAsync();
        await Assertions.Expect(Trigger(axis)).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Then(@"the active ""(.*)"" option should have keyboard focus")]
    public async Task AssertOptionFocus(string axis) =>
        await Assertions.Expect(Panel(axis).Locator("[aria-pressed='true']")).ToBeFocusedAsync();

    [When(@"I dismiss the ""(.*)"" appearance menu with Escape")]
    public async Task DismissAppearancePicker(string axis)
    {
        await Page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(Panel(axis)).Not.ToBeVisibleAsync();
        await Assertions.Expect(Trigger(axis)).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(Trigger(axis)).ToBeFocusedAsync();
    }

    [Given(@"the device uses ""(.*)"" appearance")]
    [When(@"the device uses ""(.*)"" appearance")]
    public async Task SetDeviceAppearance(string value) =>
        await Page.EmulateMediaAsync(new() { ColorScheme = value == "dark" ? ColorScheme.Dark : ColorScheme.Light });

    [Then(@"the resolved website theme should be ""(.*)""")]
    public async Task AssertTheme(string theme) =>
        await Assertions.Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", theme);

    [Then(@"the appearance controls should show ""(.*)"" and ""(.*)""")]
    public async Task AssertAppearance(string identity, string mode)
    {
        await Assertions.Expect(Trigger("identity")).ToHaveAttributeAsync("data-identity", identity);
        await Assertions.Expect(Trigger("mode")).ToHaveAttributeAsync("data-mode", mode);
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

    [When(@"I open an appearance URL for ""(.*)"" and ""(.*)""")]
    public async Task OpenAppearanceUrl(string identity, string mode)
    {
        await Page.GotoAsync(context.BaseUrl.TrimEnd('/') + "/?identity=" + Uri.EscapeDataString(identity) + "&mode=" + Uri.EscapeDataString(mode));
        await context.WaitForBlazorAsync();
    }

    [Then(@"both appearance controls should be visible")]
    public async Task ControlsVisible()
    {
        await Assertions.Expect(Trigger("mode")).ToBeVisibleAsync();
        await Assertions.Expect(Trigger("identity")).ToBeVisibleAsync();
    }
}
