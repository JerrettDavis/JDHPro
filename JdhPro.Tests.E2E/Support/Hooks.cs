using Microsoft.Extensions.Configuration;
using Reqnroll;

namespace JdhPro.Tests.E2E.Support;

[Binding]
public class Hooks
{
    private readonly WebDriverContext _webDriverContext;

    public Hooks(WebDriverContext webDriverContext)
    {
        _webDriverContext = webDriverContext;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        // Ensure Playwright browsers are installed
        Console.WriteLine("Starting E2E Test Run...");
        Console.WriteLine("Ensure Playwright browsers are installed: pwsh bin/Debug/net9.0/playwright.ps1 install");
    }

    [BeforeScenario]
    public async Task BeforeScenario(ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var baseUrl = configuration["BaseUrl"] ?? "http://localhost:5233";
        var headless = bool.Parse(configuration["Headless"] ?? "true");

        // Configure the context
        _webDriverContext.Configure(baseUrl, headless);

        // Check if this is a mobile scenario
        if (scenarioContext.ScenarioInfo.Tags.Contains("mobile") || 
            scenarioContext.ScenarioInfo.Title.Contains("mobile", StringComparison.OrdinalIgnoreCase))
        {
            await _webDriverContext.InitializeMobileAsync();
        }
        else
        {
            await _webDriverContext.InitializeAsync();
        }

        // Capture browser console logs
        if (_webDriverContext.Page != null)
        {
            _webDriverContext.Page.Console += (_, msg) =>
            {
                Console.WriteLine($"[Browser Console {msg.Type}]: {msg.Text}");
            };

            _webDriverContext.Page.PageError += (_, error) =>
            {
                Console.WriteLine($"[Page Error]: {error}");
            };
        }
    }

    [AfterScenario]
    public async Task AfterScenario(ScenarioContext scenarioContext)
    {
        if (_webDriverContext.Page != null)
        {
            // Always take screenshot at end of scenario for debugging
            var screenshotPath = $"test-results/screenshots/{scenarioContext.ScenarioInfo.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            Directory.CreateDirectory("test-results/screenshots");
            
            try
            {
                await _webDriverContext.Page.ScreenshotAsync(new()
                {
                    Path = screenshotPath,
                    FullPage = true
                });
                Console.WriteLine($"Screenshot saved: {screenshotPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            }

            // Take screenshot and HTML on failure
            if (scenarioContext.TestError != null)
            {
                try
                {
                    var htmlPath = $"test-results/screenshots/{scenarioContext.ScenarioInfo.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                    var html = await _webDriverContext.Page.ContentAsync();
                    await File.WriteAllTextAsync(htmlPath, html);
                    Console.WriteLine($"HTML saved: {htmlPath}");
                    Console.WriteLine($"Current URL: {_webDriverContext.Page.Url}");
                    Console.WriteLine($"Test Error: {scenarioContext.TestError.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save HTML: {ex.Message}");
                }
            }
        }

        _webDriverContext.Dispose();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        Console.WriteLine("E2E Test Run Completed.");
    }
}
