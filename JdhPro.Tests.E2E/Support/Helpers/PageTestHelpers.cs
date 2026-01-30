using Microsoft.Playwright;

namespace JdhPro.Tests.E2E.Support.Helpers;

/// <summary>
/// Helper methods for common page testing operations
/// </summary>
public static class PageTestHelpers
{
    /// <summary>
    /// Waits for the Blazor WASM app to fully load
    /// </summary>
    public static async Task WaitForBlazorAsync(IPage page)
    {
        try
        {
            // Wait for loading indicator to disappear
            await page.WaitForSelectorAsync(".loading-progress", new() 
            { 
                State = WaitForSelectorState.Hidden, 
                Timeout = 30000 
            });
        }
        catch
        {
            // Already hidden
        }

        try
        {
            // Wait for Blazor to be available
            await page.WaitForFunctionAsync("() => window.Blazor !== undefined", 
                new PageWaitForFunctionOptions { Timeout = 20000 });
        }
        catch
        {
            // Continue even if Blazor check fails
        }

        // Wait for network idle
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10000 });
        
        // Small buffer for component hydration
        await Task.Delay(500);
    }

    /// <summary>
    /// Scrolls an element into view
    /// </summary>
    public static async Task ScrollIntoViewAsync(IPage page, string selector)
    {
        await page.EvaluateAsync($@"
            const element = document.querySelector('{selector}');
            if (element) {{
                element.scrollIntoView({{ behavior: 'smooth', block: 'center' }});
            }}
        ");
        await Task.Delay(500); // Wait for scroll animation
    }

    /// <summary>
    /// Checks if an element is visible in viewport
    /// </summary>
    public static async Task<bool> IsInViewportAsync(IPage page, string selector)
    {
        return await page.EvaluateAsync<bool>($@"
            const element = document.querySelector('{selector}');
            if (!element) return false;
            
            const rect = element.getBoundingClientRect();
            return (
                rect.top >= 0 &&
                rect.left >= 0 &&
                rect.bottom <= window.innerHeight &&
                rect.right <= window.innerWidth
            );
        ");
    }

    /// <summary>
    /// Gets the current scroll position
    /// </summary>
    public static async Task<int> GetScrollPositionAsync(IPage page)
    {
        return await page.EvaluateAsync<int>("() => window.pageYOffset || document.documentElement.scrollTop");
    }

    /// <summary>
    /// Waits for navigation to complete and Blazor to load
    /// </summary>
    public static async Task NavigateAndWaitAsync(IPage page, string url)
    {
        await page.GotoAsync(url);
        await WaitForBlazorAsync(page);
    }

    /// <summary>
    /// Clicks an element and waits for navigation
    /// </summary>
    public static async Task ClickAndWaitForNavigationAsync(IPage page, string selector)
    {
        await page.ClickAsync(selector);
        await WaitForBlazorAsync(page);
    }

    /// <summary>
    /// Fills a form field and waits for any validation
    /// </summary>
    public static async Task FillFieldAsync(IPage page, string selector, string value)
    {
        await page.FillAsync(selector, value);
        await Task.Delay(300); // Wait for validation
    }

    /// <summary>
    /// Checks if page has no console errors
    /// </summary>
    public static void AssertNoConsoleErrors(List<string> consoleErrors)
    {
        if (consoleErrors.Any())
        {
            throw new Exception($"Console errors detected:\n{string.Join("\n", consoleErrors)}");
        }
    }

    /// <summary>
    /// Takes a screenshot with timestamp
    /// </summary>
    public static async Task<string> TakeScreenshotAsync(IPage page, string name)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filename = $"{name}_{timestamp}.png";
        var path = Path.Combine("test-results", "screenshots", filename);
        
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await page.ScreenshotAsync(new() { Path = path, FullPage = true });
        
        return path;
    }

    /// <summary>
    /// Waits for an element to contain specific text
    /// </summary>
    public static async Task WaitForTextAsync(IPage page, string selector, string expectedText, int timeoutMs = 5000)
    {
        await page.WaitForFunctionAsync($@"
            () => {{
                const element = document.querySelector('{selector}');
                return element && element.textContent.includes('{expectedText}');
            }}
        ", new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    /// <summary>
    /// Gets all text content from elements matching selector
    /// </summary>
    public static async Task<List<string>> GetAllTextAsync(IPage page, string selector)
    {
        var elements = await page.QuerySelectorAllAsync(selector);
        var texts = new List<string>();

        foreach (var element in elements)
        {
            var text = await element.TextContentAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                texts.Add(text.Trim());
            }
        }

        return texts;
    }

    /// <summary>
    /// Checks if element exists (without waiting)
    /// </summary>
    public static async Task<bool> ElementExistsAsync(IPage page, string selector)
    {
        try
        {
            var element = await page.QuerySelectorAsync(selector);
            return element != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets computed style property of an element
    /// </summary>
    public static async Task<string> GetComputedStyleAsync(IPage page, string selector, string property)
    {
        return await page.EvaluateAsync<string>(@"
            (args) => {
                const element = document.querySelector(args.selector);
                if (!element) return '';
                return window.getComputedStyle(element)[args.property];
            }
        ", new { selector, property });
    }

    /// <summary>
    /// Waits for all images to load
    /// </summary>
    public static async Task WaitForImagesAsync(IPage page, int timeoutMs = 10000)
    {
        await page.WaitForFunctionAsync(@"
            () => {
                const images = Array.from(document.images);
                return images.every(img => img.complete && img.naturalHeight !== 0);
            }
        ", new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    /// <summary>
    /// Gets page load metrics
    /// </summary>
    public static async Task<PageMetrics> GetPageMetricsAsync(IPage page)
    {
        var performanceJson = await page.EvaluateAsync<string>(@"
            JSON.stringify({
                loadTime: performance.timing.loadEventEnd - performance.timing.navigationStart,
                domReady: performance.timing.domContentLoadedEventEnd - performance.timing.navigationStart,
                firstPaint: performance.getEntriesByType('paint')[0]?.startTime || 0
            })
        ");

        return System.Text.Json.JsonSerializer.Deserialize<PageMetrics>(performanceJson) 
            ?? new PageMetrics();
    }

    /// <summary>
    /// Checks if page is responsive (mobile-friendly)
    /// </summary>
    public static async Task<bool> IsResponsiveAsync(IPage page)
    {
        return await page.EvaluateAsync<bool>(@"
            () => {
                const viewportWidth = window.innerWidth;
                const hasViewportMeta = document.querySelector('meta[name=""viewport""]') !== null;
                return hasViewportMeta && viewportWidth <= 1024;
            }
        ");
    }
}

public class PageMetrics
{
    public double LoadTime { get; set; }
    public double DomReady { get; set; }
    public double FirstPaint { get; set; }
}
