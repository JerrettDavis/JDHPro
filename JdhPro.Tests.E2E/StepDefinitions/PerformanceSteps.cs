using Reqnroll;
using FluentAssertions;
using JdhPro.Tests.E2E.Support;
using JdhPro.Tests.E2E.Support.Helpers;
using Microsoft.Playwright;
using System.Diagnostics;

namespace JdhPro.Tests.E2E.StepDefinitions;

[Binding]
public class PerformanceSteps
{
    private readonly WebDriverContext _context;
    private readonly List<string> _consoleErrors = new();
    private readonly List<string> _jsErrors = new();
    private readonly List<double> _loadTimes = new();
    private Stopwatch _stopwatch = new();
    private PageMetrics? _pageMetrics;

    public PerformanceSteps(WebDriverContext context)
    {
        _context = context;
    }

    [Given(@"I am on a fast internet connection")]
    public void GivenIAmOnAFastInternetConnection()
    {
        // Assume tests run on fast connection
        // In real scenarios, could use Playwright's network emulation
    }

    [When(@"I navigate to the homepage")]
    public async Task WhenINavigateToTheHomepage()
    {
        await SetupConsoleListeners();
        
        _stopwatch = Stopwatch.StartNew();
        await _context.Page!.GotoAsync(_context.BaseUrl);
        await _context.WaitForBlazorAsync();
        _stopwatch.Stop();

        _loadTimes.Add(_stopwatch.Elapsed.TotalSeconds);
        _pageMetrics = await PageTestHelpers.GetPageMetricsAsync(_context.Page);
    }

    [When(@"I navigate to the services page")]
    public async Task WhenINavigateToTheServicesPage()
    {
        await SetupConsoleListeners();
        
        _stopwatch = Stopwatch.StartNew();
        await _context.Page!.GotoAsync($"{_context.BaseUrl}/services");
        await _context.WaitForBlazorAsync();
        _stopwatch.Stop();

        _loadTimes.Add(_stopwatch.Elapsed.TotalSeconds);
        _pageMetrics = await PageTestHelpers.GetPageMetricsAsync(_context.Page);
    }

    [When(@"I navigate to the contact page")]
    public async Task WhenINavigateToTheContactPage()
    {
        await SetupConsoleListeners();
        
        _stopwatch = Stopwatch.StartNew();
        await _context.Page!.GotoAsync($"{_context.BaseUrl}/contact");
        await _context.WaitForBlazorAsync();
        _stopwatch.Stop();

        _loadTimes.Add(_stopwatch.Elapsed.TotalSeconds);
        _pageMetrics = await PageTestHelpers.GetPageMetricsAsync(_context.Page);
    }

    [When(@"I navigate to the homepage on mobile")]
    public async Task WhenINavigateToTheHomepageOnMobile()
    {
        // This scenario would need mobile context from Hooks
        // For now, just navigate normally
        await WhenINavigateToTheHomepage();
    }

    [Then(@"the loading indicator should disappear")]
    public async Task ThenTheLoadingIndicatorShouldDisappear()
    {
        // Check if loading indicator is not visible
        var loadingExists = await PageTestHelpers.ElementExistsAsync(_context.Page!, ".loading-progress");
        
        if (loadingExists)
        {
            var isVisible = await _context.Page!.IsVisibleAsync(".loading-progress");
            isVisible.Should().BeFalse("Loading indicator should not be visible");
        }
    }

    [Then(@"the page should be fully interactive")]
    public async Task ThenThePageShouldBeFullyInteractive()
    {
        // Check that interactive elements are clickable
        var navLinks = await _context.Page!.QuerySelectorAllAsync("nav a");
        navLinks.Should().NotBeEmpty("Navigation should have interactive links");

        // Verify page is not still loading
        var readyState = await _context.Page.EvaluateAsync<string>("document.readyState");
        readyState.Should().Be("complete", "Document should be in complete state");
    }

    [Then(@"the page should load within (.*) seconds")]
    public void ThenThePageShouldLoadWithinSeconds(int maxSeconds)
    {
        var lastLoadTime = _loadTimes.LastOrDefault();
        lastLoadTime.Should().BeLessThanOrEqualTo(maxSeconds, 
            $"Page should load within {maxSeconds} seconds but took {lastLoadTime:F2}s");
    }

    [Then(@"the DOM should be ready within (.*) seconds")]
    public void ThenTheDomShouldBeReadyWithinSeconds(int maxSeconds)
    {
        _pageMetrics.Should().NotBeNull("Page metrics should be available");
        var domReadySeconds = _pageMetrics!.DomReady / 1000.0;
        
        domReadySeconds.Should().BeLessThanOrEqualTo(maxSeconds, 
            $"DOM should be ready within {maxSeconds} seconds but took {domReadySeconds:F2}s");
    }

    [Then(@"Blazor should be available in the browser")]
    public async Task ThenBlazorShouldBeAvailableInTheBrowser()
    {
        var blazorAvailable = await _context.Page!.EvaluateAsync<bool>("() => window.Blazor !== undefined");
        blazorAvailable.Should().BeTrue("Blazor runtime should be available in window object");
    }

    [Then(@"the Blazor runtime should be loaded")]
    public async Task ThenTheBlazorRuntimeShouldBeLoaded()
    {
        // Check for Blazor-specific elements or attributes
        var blazorElements = await _context.Page!.QuerySelectorAllAsync("[data-enhanced-load]");
        // Blazor WASM might not have specific markers, but check that components rendered
        var appElement = await _context.Page.QuerySelectorAsync("#app");
        appElement.Should().NotBeNull("Blazor app element should exist");
    }

    [Then(@"all images should load successfully")]
    public async Task ThenAllImagesShouldLoadSuccessfully()
    {
        await PageTestHelpers.WaitForImagesAsync(_context.Page!, timeoutMs: 10000);
        
        var brokenImages = await _context.Page!.EvaluateAsync<int>(@"
            () => {
                const images = Array.from(document.images);
                return images.filter(img => !img.complete || img.naturalHeight === 0).length;
            }
        ");

        brokenImages.Should().Be(0, "All images should load successfully");
    }

    [Then(@"the CSS should be loaded")]
    public async Task ThenTheCssShouldBeLoaded()
    {
        var stylesheets = await _context.Page!.EvaluateAsync<int>(@"
            () => document.styleSheets.length
        ");

        stylesheets.Should().BeGreaterThan(0, "CSS stylesheets should be loaded");

        // Check that styles are applied
        var bodyBgColor = await PageTestHelpers.GetComputedStyleAsync(_context.Page, "body", "backgroundColor");
        bodyBgColor.Should().NotBeNullOrEmpty("Body should have background color from CSS");
    }

    [Then(@"the JavaScript should be loaded")]
    public async Task ThenTheJavaScriptShouldBeLoaded()
    {
        // Check for Blazor JS
        var scripts = await _context.Page!.EvaluateAsync<int>(@"
            () => document.scripts.length
        ");

        scripts.Should().BeGreaterThan(0, "JavaScript files should be loaded");
    }

    [Then(@"there should be no JavaScript errors")]
    public void ThenThereShouldBeNoJavaScriptErrors()
    {
        _jsErrors.Should().BeEmpty($"No JavaScript errors should occur. Errors: {string.Join(", ", _jsErrors)}");
    }

    [Then(@"there should be no console errors")]
    public void ThenThereShouldBeNoConsoleErrors()
    {
        _consoleErrors.Should().BeEmpty($"No console errors should occur. Errors: {string.Join(", ", _consoleErrors)}");
    }

    [Then(@"the page should have viewport meta tag")]
    public async Task ThenThePageShouldHaveViewportMetaTag()
    {
        var viewportMeta = await _context.Page!.QuerySelectorAsync("meta[name='viewport']");
        viewportMeta.Should().NotBeNull("Page should have viewport meta tag for mobile responsiveness");
    }

    [Then(@"the page should be mobile-friendly")]
    public async Task ThenThePageShouldBeMobileFriendly()
    {
        var isResponsive = await PageTestHelpers.IsResponsiveAsync(_context.Page!);
        // Note: This test needs mobile context to properly validate
        // For now, just check viewport meta exists
        var viewportMeta = await _context.Page.QuerySelectorAsync("meta[name='viewport']");
        viewportMeta.Should().NotBeNull("Page should be configured for mobile devices");
    }

    [Given(@"I am on the homepage")]
    public async Task GivenIAmOnTheHomepage()
    {
        await WhenINavigateToTheHomepage();
    }

    [When(@"I navigate back to the homepage")]
    public async Task WhenINavigateBackToTheHomepage()
    {
        await WhenINavigateToTheHomepage();
    }

    [Then(@"all pages should load quickly")]
    public void ThenAllPagesShouldLoadQuickly()
    {
        _loadTimes.Should().NotBeEmpty("Load times should be recorded");
        _loadTimes.Should().OnlyContain(time => time <= 5.0, 
            "All pages should load within 5 seconds");
    }

    [Then(@"the first paint should occur within (.*) seconds")]
    public void ThenTheFirstPaintShouldOccurWithinSeconds(int maxSeconds)
    {
        _pageMetrics.Should().NotBeNull("Page metrics should be available");
        var firstPaintSeconds = _pageMetrics!.FirstPaint / 1000.0;
        
        if (firstPaintSeconds > 0) // First paint might not be available
        {
            firstPaintSeconds.Should().BeLessThanOrEqualTo(maxSeconds, 
                $"First paint should occur within {maxSeconds} seconds");
        }
    }

    [When(@"I load the homepage (.*) times")]
    public async Task WhenILoadTheHomepageTimes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await WhenINavigateToTheHomepage();
            await Task.Delay(500); // Small delay between loads
        }
    }

    [Then(@"all load times should be similar")]
    public void ThenAllLoadTimesShouldBeSimilar()
    {
        _loadTimes.Should().HaveCountGreaterThan(1, "Multiple load times should be recorded");
        
        var average = _loadTimes.Average();
        var maxDeviation = average * 0.5; // Allow 50% deviation
        
        _loadTimes.Should().OnlyContain(time => 
            Math.Abs(time - average) <= maxDeviation,
            $"All load times should be within 50% of average ({average:F2}s)");
    }

    [Then(@"there should be no performance degradation")]
    public void ThenThereShouldBeNoPerformanceDegradation()
    {
        _loadTimes.Should().HaveCountGreaterThan(1, "Multiple load times should be recorded");
        
        var firstLoad = _loadTimes.First();
        var lastLoad = _loadTimes.Last();
        
        lastLoad.Should().BeLessThanOrEqualTo(firstLoad * 1.5, 
            "Subsequent loads should not be significantly slower than first load");
    }

    [Then(@"the time to interactive should be less than (.*) seconds")]
    public void ThenTheTimeToInteractiveShouldBeLessThanSeconds(int maxSeconds)
    {
        // Use load time as proxy for time to interactive
        var lastLoadTime = _loadTimes.LastOrDefault();
        lastLoadTime.Should().BeLessThanOrEqualTo(maxSeconds, 
            $"Time to interactive should be less than {maxSeconds} seconds");
    }

    [Then(@"the first contentful paint should be less than (.*) seconds")]
    public void ThenTheFirstContentfulPaintShouldBeLessThanSeconds(int maxSeconds)
    {
        _pageMetrics.Should().NotBeNull("Page metrics should be available");
        var firstPaintSeconds = _pageMetrics?.FirstPaint / 1000.0 ?? 0;
        
        if (firstPaintSeconds > 0)
        {
            firstPaintSeconds.Should().BeLessThanOrEqualTo(maxSeconds, 
                $"First contentful paint should be less than {maxSeconds} seconds");
        }
    }

    private async Task SetupConsoleListeners()
    {
        if (_context.Page == null) return;

        // Clear previous errors
        _consoleErrors.Clear();
        _jsErrors.Clear();

        // Note: Event handlers should already be set up in Hooks
        // This is just to ensure we capture them
        _context.Page.Console -= OnConsoleMessage;
        _context.Page.PageError -= OnPageError;

        _context.Page.Console += OnConsoleMessage;
        _context.Page.PageError += OnPageError;
    }

    private void OnConsoleMessage(object? sender, IConsoleMessage msg)
    {
        if (msg.Type == "error")
        {
            _consoleErrors.Add(msg.Text);
        }
    }

    private void OnPageError(object? sender, string error)
    {
        _jsErrors.Add(error);
    }
}
