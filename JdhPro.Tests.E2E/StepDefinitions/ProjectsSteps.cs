using Reqnroll;
using FluentAssertions;
using JdhPro.Tests.E2E.Support;
using JdhPro.Tests.E2E.Support.Helpers;
using System.Diagnostics;

namespace JdhPro.Tests.E2E.StepDefinitions;

[Binding]
public class ProjectsSteps
{
    private readonly WebDriverContext _context;
    private readonly List<string> _consoleErrors = new();
    private Stopwatch _stopwatch = new();

    public ProjectsSteps(WebDriverContext context)
    {
        _context = context;
    }

    [When(@"I navigate to the projects page")]
    public async Task WhenINavigateToTheProjectsPage()
    {
        SetupConsoleListeners();
        
        _stopwatch = Stopwatch.StartNew();
        await _context.Page!.GotoAsync($"{_context.BaseUrl}/projects");
        await _context.WaitForBlazorAsync();
        _stopwatch.Stop();
    }

    [When(@"I click the Projects navigation link")]
    public async Task WhenIClickTheProjectsNavigationLink()
    {
        // Find and click the Projects link in navigation
        var projectsLink = await _context.Page!.QuerySelectorAsync("nav a[href*='projects'], a[href='/projects']");
        
        if (projectsLink == null)
        {
            // Try alternative selectors
            projectsLink = await _context.Page.QuerySelectorAsync("a:text('Projects')");
        }

        projectsLink.Should().NotBeNull("Projects navigation link should exist");
        await projectsLink!.ClickAsync();
        await _context.WaitForBlazorAsync();
    }

    [Then(@"I should be on the projects page")]
    public void ThenIShouldBeOnTheProjectsPage()
    {
        _context.Page!.Url.Should().Contain("/projects", "Should navigate to projects page");
    }

    [Then(@"the page should display correctly")]
    public async Task ThenThePageShouldDisplayCorrectly()
    {
        // Verify page has rendered content
        var content = await _context.Page!.ContentAsync();
        content.Should().NotBeNullOrEmpty("Page should have content");

        // Check that it's not just the loading screen
        var loadingVisible = await _context.Page.IsVisibleAsync(".loading-progress").ConfigureAwait(false);
        loadingVisible.Should().BeFalse("Loading indicator should not be visible");
    }

    [Then(@"the URL should contain ""([^""]*)""")]
    public void ThenTheUrlShouldContain(string expectedUrlPart)
    {
        _context.Page!.Url.Should().Contain(expectedUrlPart, $"URL should contain '{expectedUrlPart}'");
    }

    [Then(@"the page should load within (.*) seconds")]
    public void ThenThePageShouldLoadWithinSeconds(int maxSeconds)
    {
        _stopwatch.Elapsed.TotalSeconds.Should().BeLessThanOrEqualTo(maxSeconds,
            $"Page should load within {maxSeconds} seconds but took {_stopwatch.Elapsed.TotalSeconds:F2}s");
    }

    private void SetupConsoleListeners()
    {
        if (_context.Page == null) return;

        _consoleErrors.Clear();

        _context.Page.Console -= OnConsoleMessage;
        _context.Page.Console += OnConsoleMessage;
    }

    private void OnConsoleMessage(object? sender, Microsoft.Playwright.IConsoleMessage msg)
    {
        if (msg.Type == "error")
        {
            _consoleErrors.Add(msg.Text);
        }
    }
}
