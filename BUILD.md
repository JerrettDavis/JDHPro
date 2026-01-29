# Building with Nuke

This project uses [Nuke](https://nuke.build/) for build automation, providing a consistent, testable, and cross-platform build experience.

## 🎯 Why Nuke?

- **Consistency:** Same build logic runs on your machine and CI
- **Testable:** Build logic is C# code - step through and debug
- **Maintainable:** Strongly-typed, IntelliSense-enabled build scripts
- **Cross-platform:** Works on Windows, Linux, and macOS
- **Powerful:** Rich ecosystem with GitVersion, ReportGenerator, and more

## 🚀 Quick Start

### Prerequisites

- .NET 10 SDK or later
- Node.js 20 or later (for Tailwind CSS)

### Running Build Targets

#### Windows
```powershell
# Show available targets
.\build.cmd --help

# Run CI build (default)
.\build.cmd

# Run specific target
.\build.cmd Compile
.\build.cmd Test
.\build.cmd PublishGitHubPages
```

#### Linux/macOS
```bash
# Make executable (first time only)
chmod +x build.sh

# Show available targets
./build.sh --help

# Run CI build (default)
./build.sh

# Run specific target
./build.sh Compile
./build.sh Test
./build.sh PublishGitHubPages
```

## 📋 Available Targets

### Core Targets

#### `Clean`
Cleans build artifacts and temporary files.
```bash
./build.sh Clean
```
- Removes `bin/` and `obj/` directories
- Cleans `artifacts/` and `publish/` directories
- Removes compiled CSS

#### `Restore`
Restores .NET and npm dependencies.
```bash
./build.sh Restore
```
- Runs `dotnet restore` for .NET packages
- Runs `npm install` for Tailwind CSS dependencies

#### `BuildCss`
Compiles Tailwind CSS.
```bash
./build.sh BuildCss
```
- Production build: Minifies and purges unused CSS
- Development build: Faster compilation with all utilities

#### `SyndicateBlog`
Runs the blog syndication tool to fetch posts.
```bash
./build.sh SyndicateBlog
```
- Fetches latest posts from jerrettdavis.com
- Generates `wwwroot/data/posts.json`
- Filters business-relevant content

#### `Compile`
Compiles all C# projects.
```bash
./build.sh Compile
```
Dependencies: `Restore`, `BuildCss`, `SyndicateBlog`

#### `Test`
Runs unit tests (excludes E2E tests).
```bash
./build.sh Test
```
- Runs all unit tests
- Generates test results in `artifacts/test-results/`
- Outputs: `unit-tests.trx`

#### `TestE2E`
Runs E2E tests with Playwright.
```bash
./build.sh TestE2E
```
⚠️ **Note:** Requires web app to be running separately.

**Setup:**
```bash
# Terminal 1: Start web app
cd JdhPro.Web
dotnet run

# Terminal 2: Run E2E tests
./build.sh TestE2E
```

#### `Publish`
Publishes the web project to `publish/` directory.
```bash
./build.sh Publish
```
- Creates optimized production build
- AOT compilation enabled
- Outputs to `publish/wwwroot/`

#### `PublishGitHubPages`
Publishes with GitHub Pages optimizations.
```bash
./build.sh PublishGitHubPages
```
- All features of `Publish`
- Creates `.nojekyll` file
- Creates `404.html` for SPA routing
- Updates base href if `GitHubPages=true`

**With GitHub Pages base path:**
```bash
./build.sh PublishGitHubPages --GitHubPages true --RepositoryName jdhpro
```

#### `VerifyPublish`
Verifies published output.
```bash
./build.sh VerifyPublish
```
- Checks critical files exist
- Reports file sizes
- Validates WASM files

### Composite Targets

#### `CI` (Default)
Full CI pipeline: Clean → Restore → BuildCss → SyndicateBlog → Compile → Test
```bash
./build.sh
# or
./build.sh CI
```
**Use this for:** Quick validation before committing

#### `Full`
Complete build: CI → TestE2E → Publish → VerifyPublish
```bash
./build.sh Full
```
**Use this for:** Complete validation before deploying

⚠️ **Note:** Requires web app running for E2E tests

#### `Deploy`
Deployment pipeline: Full → PublishGitHubPages
```bash
./build.sh Deploy --GitHubPages true
```
**Use this for:** Creating deployment-ready artifacts

## 🔧 Configuration

### Parameters

Pass parameters using `--ParameterName value` syntax:

#### `Configuration`
Build configuration (Debug or Release).
```bash
./build.sh Compile --Configuration Release
```
- Default: `Debug` (local), `Release` (CI)

#### `GitHubPages`
Enable GitHub Pages mode.
```bash
./build.sh PublishGitHubPages --GitHubPages true
```
- Updates base href to `/<RepositoryName>/`

#### `RepositoryName`
Repository name for GitHub Pages base path.
```bash
./build.sh PublishGitHubPages --GitHubPages true --RepositoryName jdhpro
```
- Default: `jdhpro`

### Example Combinations

```bash
# Development build
./build.sh Compile --Configuration Debug

# Production build for GitHub Pages
./build.sh PublishGitHubPages --Configuration Release --GitHubPages true

# CI build with verbose output
./build.sh CI --Configuration Release
```

## 📁 Output Directories

```
jdhpro/
├── artifacts/              # Build artifacts
│   ├── test-results/      # Test result files (*.trx)
│   └── coverage/          # Code coverage reports
├── publish/               # Published web application
│   └── wwwroot/          # Deployable static site
└── .nuke/                 # Nuke temporary files (gitignored)
```

## 🐛 Troubleshooting

### Build Fails with "npm not found"
**Solution:** Install Node.js 20+ and ensure `npm` is in PATH.

```bash
node --version  # Should be 20.x+
npm --version   # Should be 10.x+
```

### "dotnet not found" Error
**Solution:** Nuke automatically downloads .NET SDK if not found. If this fails:

1. Install .NET 10 SDK manually
2. Verify installation: `dotnet --version`
3. Try again

### E2E Tests Timeout
**Issue:** Web app not running or taking too long to start.

**Solution:**
```bash
# Start web app in separate terminal
cd JdhPro.Web
dotnet run

# Wait for "Now listening on: http://localhost:5233"
# Then run tests
./build.sh TestE2E
```

### Permission Denied (Linux/macOS)
**Issue:** `./build.sh: Permission denied`

**Solution:**
```bash
chmod +x build.sh
./build.sh
```

### Stale Build Cache
**Issue:** Build using old files after file changes.

**Solution:**
```bash
./build.sh Clean
./build.sh Compile
```

### NuGet Restore Fails
**Issue:** NuGet package restore errors.

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Retry
./build.sh Restore
```

## 🔄 CI/CD Integration

### GitHub Actions

The CI/CD pipeline (`.github/workflows/ci.yml`) uses Nuke:

```yaml
- name: Run Nuke CI Build
  run: ./build.sh CI
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

**Benefits:**
- Local builds match CI exactly
- Easy to reproduce CI failures locally
- Debug build issues by stepping through C# code

### Manual Testing Before CI

```bash
# Simulate CI pipeline locally
./build.sh CI

# Simulate deployment build
./build.sh Deploy --Configuration Release --GitHubPages true
```

## 📚 Advanced Usage

### Custom Build Project

The build project is located at `build/_build.csproj`. You can:

- Add NuGet packages for build tools
- Extend `Build.cs` with custom targets
- Add parameters for custom configuration

### Adding New Targets

Edit `build/Build.cs`:

```csharp
Target MyCustomTarget => _ => _
    .Description("My custom build target")
    .DependsOn(Compile)
    .Executes(() =>
    {
        Log.Information("Running custom target...");
        // Your custom logic here
    });
```

Run it:
```bash
./build.sh MyCustomTarget
```

### Debugging Build Script

Since build scripts are C# code, you can debug them:

1. Open `jdhpro.sln` in Visual Studio/Rider
2. Set breakpoint in `build/Build.cs`
3. Run/Debug the `_build` project
4. Step through build logic

### Build Graph Visualization

Nuke can generate build dependency graphs:

```bash
./build.sh --plan
```

Shows execution order and dependencies.

## 📖 Resources

- **Nuke Documentation:** https://nuke.build/docs/
- **Build.cs Reference:** `build/Build.cs` (this project)
- **GitHub Actions Integration:** `.github/workflows/ci.yml`

## 🆘 Getting Help

1. **Build fails:** Run with `--verbosity Verbose` for detailed logs
   ```bash
   ./build.sh CI --verbosity Verbose
   ```

2. **List all targets:**
   ```bash
   ./build.sh --help
   ```

3. **Show build plan:**
   ```bash
   ./build.sh CI --plan
   ```

4. **Check build project:**
   ```bash
   cd build
   dotnet build _build.csproj
   ```

---

**Quick Reference Card**

| Task | Command |
|------|---------|
| Daily development | `./build.sh CI` |
| Before committing | `./build.sh CI` |
| Create deployment | `./build.sh Deploy --GitHubPages true` |
| Run all tests | `./build.sh Full` |
| Clean everything | `./build.sh Clean` |
| Just compile | `./build.sh Compile` |
| Just CSS | `./build.sh BuildCss` |

---

*Build automation powered by [Nuke Build](https://nuke.build/)*
