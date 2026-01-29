using FluentAssertions;
using Microsoft.Playwright;
using Reqnroll;
using JdhPro.Tests.E2E.Support;

namespace JdhPro.Tests.E2E.StepDefinitions;

[Binding]
public class ContactSteps
{
    private readonly WebDriverContext _context;
    private IPage Page => _context.Page!;

    public ContactSteps(WebDriverContext context)
    {
        _context = context;
    }

    [Given(@"I navigate to ""(.*)""")]
    public async Task GivenINavigateTo(string path)
    {
        await Page.GotoAsync($"{_context.BaseUrl}{path}", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await _context.WaitForBlazorAsync();
    }

    [Then(@"I should see a contact form")]
    public async Task ThenIShouldSeeAContactForm()
    {
        var form = Page.Locator("form").First;
        var isVisible = await form.IsVisibleAsync();
        isVisible.Should().BeTrue("Contact form should be visible");
    }

    [When(@"I click ""(.*)"" without filling the form")]
    public async Task WhenIClickWithoutFillingTheForm(string buttonText)
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = buttonText, Exact = false }).ClickAsync();
        // Wait for validation to trigger
        await Task.Delay(500);
    }

    [Then(@"I should see validation errors")]
    public async Task ThenIShouldSeeValidationErrors()
    {
        // Check for HTML5 validation or custom error messages
        var invalidInputs = await Page.Locator("input:invalid, textarea:invalid").CountAsync();
        var errorMessages = await Page.Locator("[class*='error'], [class*='invalid']").CountAsync();
        
        (invalidInputs + errorMessages).Should().BeGreaterThan(0, "Validation errors should be displayed");
    }

    [Then(@"the service dropdown should have ""(.*)"" selected")]
    public async Task ThenTheServiceDropdownShouldHaveSelected(string serviceName)
    {
        var select = Page.Locator("select#service, select[id*='service' i]").First;
        await select.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        var selectedValue = await select.InputValueAsync();
        selectedValue.Should().ContainEquivalentOf(serviceName);
    }
}
