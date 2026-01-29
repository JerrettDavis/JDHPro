using Microsoft.Playwright;

namespace JdhPro.Tests.E2E.Support;

public class WebDriverContext : IDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private bool _disposed;

    public IPage? Page { get; private set; }
    public IBrowserContext? Context { get; private set; }
    public string BaseUrl { get; private set; }
    public bool IsHeadless { get; private set; }

    public WebDriverContext(string baseUrl = "http://localhost:5233", bool isHeadless = true)
    {
        BaseUrl = baseUrl;
        IsHeadless = isHeadless;
    }

    public void Configure(string baseUrl, bool isHeadless)
    {
        BaseUrl = baseUrl;
        IsHeadless = isHeadless;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = IsHeadless,
            SlowMo = IsHeadless ? 0 : 100
        });

        Context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            RecordVideoDir = "test-results/videos"
        });

        Page = await Context.NewPageAsync();
        Page.SetDefaultTimeout(60000); // Increase timeout for Blazor WASM
    }

    public async Task InitializeMobileAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = IsHeadless,
            SlowMo = IsHeadless ? 0 : 100
        });

        var device = _playwright.Devices["iPhone 13"];
        Context = await _browser.NewContextAsync(device);
        Page = await Context.NewPageAsync();
        Page.SetDefaultTimeout(60000); // Increase timeout for Blazor WASM
    }

    public async Task WaitForBlazorAsync()
    {
        if (Page == null)
            throw new InvalidOperationException("Page is not initialized");

        try
        {
            // First, wait for network to be idle (Blazor WASM loads)
            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            
            // Wait for the loading SVG to disappear (indicates Blazor has started replacing content)
            try
            {
                await Page.WaitForSelectorAsync(".loading-progress", new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });
            }
            catch
            {
                // Loading progress might have already disappeared, continue
            }
            
            // Wait for Blazor WebAssembly to be fully loaded
            await Page.WaitForFunctionAsync(@"
                () => {
                    // Check if Blazor has loaded
                    return window.Blazor !== undefined;
                }
            ", new PageWaitForFunctionOptions { Timeout = 20000 });

            // Wait for network to be idle (all resources loaded)
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10000 });

            // Extra delay to ensure JS event handlers and components are fully hydrated
            await Task.Delay(1500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Blazor wait encountered error - {ex.Message}");
            // Fallback: just wait for network idle and DOM content loaded
            try
            {
                await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 5000 });
                await Task.Delay(1000);
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"Warning: Fallback wait also failed - {fallbackEx.Message}");
                // Continue anyway, tests might still work
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Context?.CloseAsync().GetAwaiter().GetResult();
        _browser?.CloseAsync().GetAwaiter().GetResult();
        _playwright?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
