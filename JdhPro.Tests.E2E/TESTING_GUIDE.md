# Comprehensive E2E Testing Guide

## Overview

This document provides comprehensive guidance for running and maintaining the E2E test suite for JDH Productions. The test suite covers blog syndication, site pages, performance, and prepares for future blog functionality.

## Test Categories

### 1. Blog Syndication Tests (`BlogSyndication.feature`)

Tests the blog syndication tool that fetches posts from GitHub and generates `posts.json`.

**Key Scenarios:**
- ✅ Successfully syndicate posts from GitHub
- ✅ Validate generated posts structure
- ✅ Filter out personal posts and drafts
- ✅ Validate syndicated posts have canonical URLs
- ✅ Validate posts are sorted by date
- ✅ Validate HTML content conversion
- ✅ Validate file size and metadata

**Running Blog Syndication Tests:**
```powershell
# Run all blog syndication tests
dotnet test --filter "FullyQualifiedName~BlogSyndication"

# Run specific scenario
dotnet test --filter "FullyQualifiedName~BlogSyndication.SuccessfullySyndicatePostsFromGitHub"
```

**Prerequisites:**
- GitHub repository with blog posts
- Valid `appsettings.json` configuration in `JdhPro.BuildTools`
- Network connectivity to GitHub

### 2. Performance & Loading Tests (`Performance.feature`)

Tests page load performance, WASM initialization, and resource loading.

**Key Scenarios:**
- ✅ App loader disappears after startup
- ✅ Pages load within acceptable time (< 5 seconds)
- ✅ Blazor WASM initializes properly
- ✅ Static assets load correctly
- ✅ No console errors during page load
- ✅ Mobile responsiveness
- ✅ Performance consistency across multiple loads

**Running Performance Tests:**
```powershell
# Run all performance tests
dotnet test --filter "FullyQualifiedName~Performance"

# Run tagged performance tests
dotnet test --filter "Category=performance"

# Run in headed mode to observe
$env:Headless="false"
dotnet test --filter "FullyQualifiedName~Performance"
```

**Performance Thresholds:**
- Page Load: < 5 seconds
- DOM Ready: < 3 seconds
- First Paint: < 2 seconds
- Time to Interactive: < 6 seconds

### 3. Site Page Tests

Tests for existing site pages (Homepage, Services, Contact, Projects, Navigation).

**Running Site Tests:**
```powershell
# Homepage tests
dotnet test --filter "FullyQualifiedName~Homepage"

# Services page tests
dotnet test --filter "FullyQualifiedName~Services"

# Contact page tests
dotnet test --filter "FullyQualifiedName~Contact"

# Projects page tests
dotnet test --filter "FullyQualifiedName~Projects"

# Navigation tests
dotnet test --filter "FullyQualifiedName~Navigation"
```

### 4. Future Blog UI Tests (🚧 Prepared but not active)

Tests prepared for when blog listing and post detail pages are implemented.

**Features:**
- `BlogListing.feature` - Blog listing page tests
- `BlogPost.feature` - Blog post detail page tests

**Tagged with:** `@future @blog`

These tests will be skipped until blog pages are implemented. To activate:
1. Implement blog pages in the web project
2. Remove `@future` tag from scenarios
3. Add `@implemented` tag
4. Run tests: `dotnet test --filter "Category=blog"`

## Test Helpers

### BlogTestHelpers

Located in `Support/Helpers/BlogTestHelpers.cs`, provides utilities for blog-related testing:

**Key Methods:**
```csharp
// Load and parse posts.json
var posts = await BlogTestHelpers.LoadPostsJsonAsync();

// Validate post structure
BlogTestHelpers.ValidatePostStructure(post);
BlogTestHelpers.ValidateAllPostsStructure(posts);

// Validate filtering
BlogTestHelpers.ValidateNoExcludedCategories(posts, "Personal");
BlogTestHelpers.ValidateNoExcludedTags(posts, "Draft");

// Validate syndication
BlogTestHelpers.ValidateSyndicatedPostsHaveCanonicalUrls(posts);
BlogTestHelpers.ValidatePostsSortedByDateDescending(posts);

// Query posts
var technicalPosts = BlogTestHelpers.GetPostsByCategory(posts, "Technical");
var syndicatedPosts = BlogTestHelpers.GetPostsBySource(posts, "syndicated");
```

### PageTestHelpers

Located in `Support/Helpers/PageTestHelpers.cs`, provides utilities for page testing:

**Key Methods:**
```csharp
// Wait for Blazor to load
await PageTestHelpers.WaitForBlazorAsync(page);

// Navigate and wait
await PageTestHelpers.NavigateAndWaitAsync(page, url);

// Scroll and visibility
await PageTestHelpers.ScrollIntoViewAsync(page, selector);
var isVisible = await PageTestHelpers.IsInViewportAsync(page, selector);

// Get page metrics
var metrics = await PageTestHelpers.GetPageMetricsAsync(page);
Console.WriteLine($"Load Time: {metrics.LoadTime}ms");

// Take screenshots
await PageTestHelpers.TakeScreenshotAsync(page, "debug");

// Get text content
var texts = await PageTestHelpers.GetAllTextAsync(page, ".post-title");

// Check element existence
var exists = await PageTestHelpers.ElementExistsAsync(page, ".blog-post");
```

## Configuration

### appsettings.json

Located in `JdhPro.Tests.E2E/appsettings.json`:

```json
{
  "BaseUrl": "http://localhost:5233",
  "Headless": "true",
  "Timeout": "30000",
  "ScreenshotOnFailure": "true",
  "VideoRecording": "true"
}
```

**Environment Variable Overrides:**
```powershell
$env:BaseUrl="https://jdhproductions.com"
$env:Headless="false"
dotnet test
```

### reqnroll.json

Configuration for Reqnroll BDD framework:

```json
{
  "filter": {
    "tags": "not @future or @implemented"
  }
}
```

This configuration skips tests tagged with `@future` unless they also have `@implemented`.

## Running Tests Locally

### Prerequisites

1. **Install .NET 10.0 SDK**
2. **Install Playwright browsers:**
   ```powershell
   cd JdhPro.Tests.E2E
   dotnet build
   pwsh bin/Debug/net10.0/playwright.ps1 install chromium
   ```

### Quick Start

```powershell
# Start the web application (in one terminal)
cd JdhPro.Web
dotnet run

# Run tests (in another terminal)
cd JdhPro.Tests.E2E
dotnet test
```

### Advanced Test Execution

```powershell
# Run in headed mode (see browser)
$env:Headless="false"
dotnet test

# Run specific test category
dotnet test --filter "FullyQualifiedName~BlogSyndication"

# Run with detailed logging
dotnet test --logger "console;verbosity=detailed"

# Run with test result output
dotnet test --logger "trx;LogFileName=test-results.trx"

# Run against production site
$env:BaseUrl="https://jdhproductions.com"
dotnet test
```

### Debugging Tests

**Visual Studio:**
1. Set breakpoints in step definition files
2. Right-click on test → "Debug Tests"

**VS Code:**
1. Install .NET Test Explorer extension
2. Set breakpoints
3. Click "Debug Test" in explorer

**Headed Mode (see browser):**
```powershell
$env:Headless="false"
dotnet test --filter "FullyQualifiedName~YourTest"
```

## CI/CD Integration

### GitHub Actions Workflow

Located at `.github/workflows/build-test-deploy.yml`.

**Workflow Jobs:**
1. **blog-syndication** - Runs blog syndication and generates posts.json
2. **build** - Builds solution and runs unit tests
3. **e2e-tests** - Runs E2E tests in headless mode
4. **publish** - Publishes application (main branch only)
5. **test-summary** - Generates test report

**Artifacts:**
- `blog-posts` - Generated posts.json
- `e2e-test-results` - Test results (.trx files)
- `e2e-screenshots` - Screenshots on failure
- `e2e-videos` - Test execution videos
- `published-app` - Published application

**Running Workflow:**
- Automatically runs on push to `main` or `develop`
- Manually trigger via GitHub Actions UI

### Local CI Simulation

Simulate the CI pipeline locally:

```powershell
# Step 1: Run blog syndication
cd JdhPro.BuildTools
dotnet run

# Step 2: Build solution
cd ..
dotnet build --configuration Release

# Step 3: Start web app
cd JdhPro.Web
Start-Process dotnet -ArgumentList "run --no-build --configuration Release" -PassThru

# Wait for app to start
Start-Sleep -Seconds 10

# Step 4: Run E2E tests
cd ..\JdhPro.Tests.E2E
dotnet test --configuration Release --no-build
```

## Test Maintenance

### Adding New Tests

1. **Create Feature File:**
   ```gherkin
   Feature: New Feature
     Scenario: Test something
       Given precondition
       When action
       Then expected result
   ```

2. **Generate Step Definitions:**
   Run tests to see missing step suggestions, then implement them.

3. **Implement Steps:**
   ```csharp
   [Binding]
   public class NewFeatureSteps
   {
       private readonly WebDriverContext _context;
       
       public NewFeatureSteps(WebDriverContext context)
       {
           _context = context;
       }
       
       [Given(@"precondition")]
       public async Task GivenPrecondition()
       {
           // Implementation
       }
   }
   ```

### Updating Existing Tests

1. Modify feature file scenarios
2. Update step definitions if needed
3. Run tests to verify: `dotnet test`
4. Update documentation if behavior changes

### Handling Flaky Tests

**Common Causes:**
- Network latency
- Timing issues with Blazor WASM loading
- Element not ready for interaction

**Solutions:**
```csharp
// Use proper waits
await _context.WaitForBlazorAsync();
await PageTestHelpers.WaitForTextAsync(page, selector, expectedText);

// Increase timeouts for slow operations
Page.SetDefaultTimeout(60000); // 60 seconds

// Use retry logic for flaky elements
for (int i = 0; i < 3; i++)
{
    try
    {
        await element.ClickAsync();
        break;
    }
    catch when (i < 2)
    {
        await Task.Delay(1000);
    }
}
```

## Troubleshooting

### Common Issues

**Issue: "Playwright driver not found"**
```powershell
# Solution: Install Playwright browsers
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

**Issue: "Target application not running"**
```powershell
# Solution: Start the web application
cd JdhPro.Web
dotnet run
```

**Issue: "posts.json not found"**
```powershell
# Solution: Run blog syndication first
cd JdhPro.BuildTools
dotnet run
```

**Issue: Tests timeout waiting for Blazor**
```powershell
# Solution: Increase timeout in WebDriverContext
# Or run in headed mode to see what's happening
$env:Headless="false"
dotnet test
```

**Issue: Screenshots not saved**
```powershell
# Solution: Ensure test-results directory exists and has write permissions
New-Item -ItemType Directory -Force -Path "test-results/screenshots"
```

### Viewing Test Results

**Screenshots:**
```
JdhPro.Tests.E2E/test-results/screenshots/
```

**Videos:**
```
JdhPro.Tests.E2E/test-results/videos/
```

**Test Results (.trx):**
```
JdhPro.Tests.E2E/TestResults/
```

**View in browser:**
```powershell
# Generate HTML report
dotnet tool install --global Reqnroll.Tools.LivingDoc.CLI
livingdoc feature-data -p JdhPro.Tests.E2E.csproj -o TestResults/feature-data.json
livingdoc test-assembly bin/Debug/net10.0/JdhPro.Tests.E2E.dll -t TestResults/feature-data.json -o TestResults/LivingDoc.html
```

## Performance Optimization

### Parallel Test Execution

xUnit runs tests in parallel by default. Configure in `xunit.runner.json`:

```json
{
  "parallelizeAssembly": false,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4
}
```

### Headless Mode

Always run in headless mode for CI/CD:
```powershell
$env:Headless="true"
dotnet test
```

### Selective Test Execution

Run only necessary tests during development:
```powershell
# Run only fast tests
dotnet test --filter "Category!=performance"

# Skip future tests
dotnet test --filter "Category!=future"
```

## Best Practices

1. **Keep Scenarios Focused** - One scenario tests one thing
2. **Use Descriptive Names** - Clear scenario and step names
3. **Avoid UI-Specific Details in Gherkin** - Keep it business-focused
4. **Reuse Step Definitions** - Keep steps generic and reusable
5. **Use Page Objects for Complex Pages** - Encapsulate page logic
6. **Always Take Screenshots on Failure** - Configured in Hooks.cs
7. **Run Tests in CI/CD** - Catch issues early
8. **Keep Tests Independent** - Each test should work in isolation
9. **Use Proper Waits** - Don't use Thread.Sleep, use Playwright waits
10. **Clean Up Test Data** - Ensure tests don't leave artifacts

## Resources

- [Playwright Documentation](https://playwright.dev/dotnet/)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [Gherkin Syntax](https://cucumber.io/docs/gherkin/)
- [FluentAssertions](https://fluentassertions.com/)
- [xUnit Documentation](https://xunit.net/)

## Support

For issues or questions:
1. Check this documentation first
2. Review test output and screenshots
3. Run in headed mode to observe behavior
4. Check GitHub Actions logs for CI failures
5. Review existing tests for patterns

## Future Enhancements

- [ ] Visual regression testing
- [ ] API integration tests
- [ ] Mobile app testing (if applicable)
- [ ] Load testing
- [ ] Accessibility testing (a11y)
- [ ] Cross-browser testing (Firefox, Safari)
