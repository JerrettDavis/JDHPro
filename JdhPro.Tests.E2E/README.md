# JDH Productions E2E Tests

Comprehensive End-to-End testing suite for the JDH Productions website using **Playwright** and **Reqnroll** (BDD).

## 🚀 Quick Start

### Prerequisites

- .NET 9.0 SDK or later
- Node.js (for Playwright browser installation)

### Installation

1. **Install Playwright Browsers** (first time only):
   ```powershell
   cd JdhPro.Tests.E2E
   pwsh bin/Debug/net10.0/playwright.ps1 install
   ```

   Or using dotnet:
   ```powershell
   dotnet build
   pwsh bin/Debug/net10.0/playwright.ps1 install chromium
   ```

2. **Restore packages**:
   ```powershell
   dotnet restore
   ```

### Running Tests

#### Run all tests (headless):
```powershell
dotnet test
```

#### Run tests in headed mode (see browser):
```powershell
$env:Headless="false"
dotnet test
```

#### Run specific feature:
```powershell
dotnet test --filter "FullyQualifiedName~Homepage"
```

#### Run with specific base URL:
```powershell
$env:BaseUrl="http://localhost:5233"
dotnet test
```

### Configuration

#### appsettings.json
```json
{
  "BaseUrl": "http://localhost:5233",
  "Headless": "true",
  "Timeout": "30000",
  "ScreenshotOnFailure": "true",
  "VideoRecording": "true"
}
```

#### Environment Variables
You can override settings via environment variables:
- `BaseUrl` - The base URL of the application under test
- `Headless` - Run tests in headless mode (true/false)

Example:
```powershell
$env:BaseUrl="https://jdhproductions.com"
$env:Headless="false"
dotnet test
```

## 📁 Project Structure

```
JdhPro.Tests.E2E/
├── Features/                    # Gherkin feature files
│   ├── Homepage.feature
│   ├── ServicesPage.feature
│   ├── ContactPage.feature
│   └── Navigation.feature
├── StepDefinitions/             # Step implementations
│   ├── HomepageSteps.cs
│   ├── ServicesSteps.cs
│   ├── ContactSteps.cs
│   └── NavigationSteps.cs
├── Support/                     # Test infrastructure
│   ├── Hooks.cs                # Before/After hooks
│   ├── WebDriverContext.cs     # Browser context management
│   └── PageObjects/            # (Optional) Page Object Models
├── test-results/               # Generated at runtime
│   ├── screenshots/            # Failure screenshots
│   └── videos/                 # Test execution videos
├── reqnroll.json               # Reqnroll configuration
└── appsettings.json            # Test settings
```

## 🎯 Test Scenarios

### Homepage
- ✅ Homepage loads successfully with hero heading
- ✅ Services grid displays all 4 services
- ✅ "Start Your Project" CTA scrolls to contact section

### Services Page
- ✅ Services page loads with proper title
- ✅ All service sections display with titles, descriptions, and benefits
- ✅ Each service has a "Discuss This Service" button

### Contact Page
- ✅ Contact page loads with heading and form
- ✅ Form validation works (prevents empty submissions)
- ✅ Service preselection via query parameter works

### Navigation
- ✅ Header navigation links work correctly
- ✅ Mobile menu displays and functions properly

## 🐛 Debugging Tests

### Run in headed mode:
```powershell
$env:Headless="false"
dotnet test
```

### View failure screenshots:
Screenshots are automatically captured on test failure in:
```
test-results/screenshots/
```

### View test videos:
All tests are recorded (in context initialization). Videos are saved to:
```
test-results/videos/
```

### Debug in Visual Studio:
1. Set breakpoints in step definition files
2. Right-click on a feature file or test
3. Select "Debug Tests"

### Debug in VS Code:
1. Install the .NET Test Explorer extension
2. Set breakpoints in step definitions
3. Click "Debug Test" in the test explorer

## 📊 Test Reports

### Run tests with logger:
```powershell
dotnet test --logger "console;verbosity=detailed"
```

### Generate HTML report (with ReqnRoll LivingDoc):
```powershell
# Install the tool
dotnet tool install --global Reqnroll.Tools.LivingDoc.CLI

# Generate report
livingdoc feature-data -p JdhPro.Tests.E2E.csproj -o TestResults/feature-data.json
livingdoc test-assembly JdhPro.Tests.E2E.dll -t TestResults/feature-data.json
```

## 🔧 Common Issues

### "Playwright driver not found"
```powershell
pwsh bin/Debug/net10.0/playwright.ps1 install
```

### "Target application not running"
Make sure the JdhPro.Web application is running:
```powershell
cd ..\JdhPro.Web
dotnet run
```

### Tests are slow
- Run in headless mode: `$env:Headless="true"`
- Reduce default timeout in `appsettings.json`
- Run tests in parallel (xUnit runs tests in parallel by default)

### Screenshot/video files not generated
Make sure the `test-results` folder has write permissions.

## 🚦 CI/CD Integration

### GitHub Actions example:
```yaml
- name: Install Playwright
  run: pwsh JdhPro.Tests.E2E/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium

- name: Run E2E Tests
  run: dotnet test JdhPro.Tests.E2E --logger "trx;LogFileName=test-results.trx"
  env:
    BaseUrl: http://localhost:5233
    Headless: true

- name: Upload Test Results
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: test-results
    path: |
      **/test-results/
      **/*.trx
```

## 📝 Writing New Tests

### 1. Create a feature file:
```gherkin
Feature: New Feature
  As a user
  I want to do something
  So that I can achieve a goal

Scenario: My scenario
  Given I am on the page
  When I do something
  Then I should see the result
```

### 2. Generate step definitions:
Run the tests and Reqnroll will suggest step definitions for undefined steps.

### 3. Implement the steps:
```csharp
[Binding]
public class MySteps
{
    private readonly WebDriverContext _context;

    public MySteps(WebDriverContext context)
    {
        _context = context;
    }

    [Given(@"I am on the page")]
    public async Task GivenIAmOnThePage()
    {
        await _context.Page!.GotoAsync(_context.BaseUrl);
    }
}
```

## 🎓 Best Practices

1. **Keep scenarios focused** - One scenario should test one thing
2. **Use descriptive scenario names** - Be clear about what is being tested
3. **Avoid UI-specific details in scenarios** - Keep Gherkin business-focused
4. **Reuse step definitions** - Keep steps generic and reusable
5. **Use Page Objects for complex pages** - Encapsulate page logic
6. **Take screenshots on failure** - Already configured in Hooks.cs
7. **Run tests in CI/CD** - Catch issues early

## 📚 Resources

- [Playwright Documentation](https://playwright.dev/dotnet/)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [Gherkin Syntax](https://cucumber.io/docs/gherkin/)
- [FluentAssertions](https://fluentassertions.com/)

## 🤝 Contributing

When adding new tests:
1. Create feature file with clear scenarios
2. Implement step definitions
3. Ensure tests pass in both headed and headless mode
4. Add any new configuration to appsettings.json
5. Update this README if needed
