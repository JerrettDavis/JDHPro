# JDH Productions Website

> Modern, high-performance static website built with Blazor WebAssembly and Tailwind CSS

[![Build](https://img.shields.io/github/actions/workflow/status/JerrettDavis/JDHPro/ci.yml?branch=main&label=build&logo=github)](https://github.com/JerrettDavis/JDHPro/actions/workflows/ci.yml)
[![CodeQL](https://img.shields.io/github/actions/workflow/status/JerrettDavis/JDHPro/codeql.yml?branch=main&label=codeql&logo=github)](https://github.com/JerrettDavis/JDHPro/actions/workflows/codeql.yml)
[![Deploy](https://img.shields.io/github/deployments/JerrettDavis/JDHPro/github-pages?label=deploy&logo=github)](https://github.com/JerrettDavis/JDHPro/deployments)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4?logo=blazor)](https://blazor.net/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-3.4-38B2AC?logo=tailwind-css)](https://tailwindcss.com/)
[![Nuke Build](https://img.shields.io/badge/Build-Nuke-00A98F?logo=nucleo)](https://nuke.build/)

JDH Productions is a professional services company specializing in software development, digital consulting, and technical solutions. This repository contains the source code for our company website - a blazing-fast, fully static site built with modern web technologies.

## ✨ Features

- 🚀 **100% Static** - No server required, deploy anywhere
- ⚡ **AOT Compiled** - Native WebAssembly for maximum performance
- 📱 **Responsive Design** - Mobile-first, works beautifully on all devices
- 🎨 **Tailwind CSS** - Modern utility-first styling
- 📝 **Markdown Content** - Easy content management with Markdown files
- 🔌 **Pluggable Contact Providers** - Flexible contact form implementations
- 🌐 **Multi-CDN Ready** - Deploy to Vercel, GitHub Pages, Cloudflare, or any static host
- 🤖 **Automated CI/CD** - Push to deploy with GitHub Actions

## 🛠️ Tech Stack

### Frontend
- **Blazor WebAssembly** - Standalone, fully static
- **.NET 10** - Latest .NET runtime with AOT compilation
- **Tailwind CSS 3.4** - Utility-first CSS framework
- **Markdig** - Markdown processing for content

### Build & Deployment
- **Nuke Build System** - Unified, testable build automation
- **GitHub Actions** - Automated CI/CD pipelines
- **Vercel** (Primary) - Edge network deployment
- **GitHub Pages** - Free static hosting
- **Cloudflare Pages** - Global CDN deployment
- **Node.js** - Build tooling for Tailwind CSS

### Development
- **Visual Studio 2022** / **VS Code** / **Rider**
- **npm** - Package management for CSS tooling
- **Git** - Version control

## 📁 Project Structure

```
JDHPro/
├── .github/
│   └── workflows/           # CI/CD pipelines
│       ├── deploy-vercel.yml
│       ├── deploy-github-pages.yml
│       └── deploy-cloudflare.yml
├── JdhPro.Web/              # Main Blazor project
│   ├── Components/          # Reusable Blazor components
│   ├── Layout/              # Layout components (MainLayout, NavMenu)
│   ├── Models/              # Data models
│   ├── Pages/               # Page components (Home, Services, Contact, etc.)
│   ├── Services/            # Business logic services
│   ├── Styles/              # Tailwind CSS source files
│   ├── wwwroot/             # Static assets
│   │   ├── content/         # Content files
│   │   │   ├── services/    # Service markdown files
│   │   │   └── config/      # Configuration JSON files
│   │   ├── css/             # Compiled CSS
│   │   └── icon-192.png     # Site icon
│   ├── Program.cs           # Application entry point
│   ├── App.razor            # Root component
│   ├── tailwind.config.js   # Tailwind configuration
│   ├── postcss.config.js    # PostCSS configuration
│   └── package.json         # npm dependencies
├── content/                 # Content source (copied to wwwroot)
│   ├── services/            # Service pages (.md files)
│   └── config/              # Site configuration (.json files)
├── CI-CD-SETUP.md           # CI/CD setup documentation
├── DEPLOYMENT.md            # Detailed deployment guide
├── SETUP_COMPLETE.md        # Initial setup documentation
└── README.md                # This file
```

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 10 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 20+** - [Download here](https://nodejs.org/)
- **Git** - [Download here](https://git-scm.com/)

Verify installations:
```bash
dotnet --version  # Should be 10.0.x
node --version    # Should be 20.x or higher
npm --version     # Should be 10.x or higher
```

### Clone and Setup

```bash
# Clone the repository
git clone https://github.com/JerrettDavis/JDHPro.git
cd JDHPro

# Quick start with Nuke (recommended)
./build.sh CI

# Or manual setup
cd JdhPro.Web
dotnet restore
npm install
```

### Building with Nuke (Recommended)

This project uses [Nuke](https://nuke.build/) for build automation.

#### Windows
```powershell
# Show available build targets
.\build.cmd --help

# Run full CI build (restore, build CSS, compile, test)
.\build.cmd CI

# Create deployment artifacts
.\build.cmd Deploy --GitHubPages true
```

#### Linux/macOS
```bash
# Make executable (first time only)
chmod +x build.sh

# Show available build targets
./build.sh --help

# Run full CI build
./build.sh CI

# Create deployment artifacts
./build.sh Deploy --GitHubPages true
```

**Common Nuke targets:**
- `CI` - Full CI pipeline (default)
- `Compile` - Just compile the solution
- `Test` - Run unit tests
- `Publish` - Create deployable artifacts
- `PublishGitHubPages` - Publish with GitHub Pages optimizations
- `Clean` - Clean build artifacts

📖 **See [BUILD.md](BUILD.md) for complete Nuke build documentation**

### Manual Build (Alternative)

If you prefer not to use Nuke:

# Navigate to the web project
cd JdhPro.Web

# Restore .NET dependencies
dotnet restore

# Install npm packages for Tailwind CSS
npm install

# Build Tailwind CSS
npm run build:css
```

### Run Locally

Start the development server:

```bash
dotnet run
```

Or with hot reload:

```bash
dotnet watch run
```

The site will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

The browser will automatically open and navigate to the site.

### Build for Production

Create an optimized production build:

```bash
# From the JdhPro.Web directory
dotnet publish -c Release -o ../publish
```

The static files will be in `publish/wwwroot/` and can be deployed to any static hosting service.

**Production Build Features:**
- AOT-compiled WebAssembly for maximum performance
- IL stripping to reduce bundle size
- Brotli and gzip pre-compression
- Minified Tailwind CSS
- Optimized assets

## 💻 Development

### Adding New Services

Services are defined as Markdown files in `content/services/`:

1. **Create a new Markdown file:**
   ```bash
   # In the project root
   echo "# My New Service" > content/services/my-new-service.md
   ```

2. **Add content using Markdown syntax:**
   ```markdown
   # My New Service
   
   A brief description of your service.
   
   ## Features
   - Feature 1
   - Feature 2
   
   ## Benefits
   This service provides...
   ```

3. **Copy to wwwroot (automated in build):**
   The build process automatically copies content files to `wwwroot/content/`.

4. **Update navigation (if needed):**
   Edit `JdhPro.Web/Layout/NavMenu.razor` to add a link to the new service page.

The `MarkdownService` will automatically parse and display your content on the Services page.

### Modifying Content

#### Services Content
- Location: `content/services/*.md`
- Format: Markdown
- Edit files directly and rebuild

#### Site Configuration
- Location: `content/config/*.json`
- Contains site metadata, contact settings, etc.
- Edit JSON files and rebuild

#### Blog Posts (Future)
- Will be located in: `content/blog/*.md`
- Follow same pattern as services

### Updating Styling (Tailwind CSS)

Tailwind CSS is configured with JIT (Just-In-Time) mode for optimal performance.

#### Modify Tailwind Configuration
Edit `JdhPro.Web/tailwind.config.js`:

```javascript
module.exports = {
  content: [
    './Pages/**/*.{razor,html}',
    './Components/**/*.{razor,html}',
    './Layout/**/*.{razor,html}',
  ],
  theme: {
    extend: {
      colors: {
        // Add custom colors
        'brand-blue': '#1e40af',
      }
    }
  }
}
```

#### Add Custom CSS
Edit `JdhPro.Web/Styles/app.css`:

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

/* Custom component classes */
@layer components {
  .btn-primary {
    @apply px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition;
  }
}
```

#### Rebuild Tailwind CSS
```bash
npm run build:css
```

For development with auto-rebuild:
```bash
npm run watch:css
```

### Testing Contact Providers

The site supports pluggable contact form providers. To test different providers:

1. **FormSpree Provider** (Default)
   - Sign up at [formspree.io](https://formspree.io)
   - Get your form endpoint
   - Update configuration

2. **Web3Forms Provider**
   - Sign up at [web3forms.com](https://web3forms.com)
   - Get your access key
   - Switch provider in configuration

3. **Custom Provider**
   - Implement `IContactProvider` interface
   - Register in `Program.cs`
   - Configure as needed

**Testing Tips:**
- Use browser DevTools Network tab to inspect form submissions
- Check console for errors
- Verify provider configuration is correct
- Test with valid and invalid inputs

## 🧪 Testing

### E2E Testing with Playwright

Comprehensive end-to-end testing suite built with Playwright and Reqnroll (BDD framework).

**Test Coverage:**
- ✅ **28 Active Scenarios** - Blog syndication, performance, site pages
- 🚧 **21 Future Scenarios** - Blog UI tests (prepared for when blog pages are implemented)

**Key Test Suites:**

1. **Blog Syndication Tests** (`BlogSyndication.feature`)
   - Validates blog post syndication from GitHub
   - Tests posts.json generation and structure
   - Verifies filtering (no Personal/Draft posts)
   - Validates canonical URLs and metadata

2. **Performance Tests** (`Performance.feature`)
   - Page load times (< 5 seconds)
   - Blazor WASM initialization
   - Static asset loading
   - Console error detection
   - Mobile responsiveness

3. **Site Page Tests**
   - Homepage, Services, Contact, Projects
   - Navigation and routing
   - Form validation

4. **Future Blog UI Tests** (Prepared, tagged `@future`)
   - Blog listing page
   - Post detail pages
   - Search and filtering
   - Pagination

### Running Tests

```bash
# Install prerequisites (first time only)
cd JdhPro.Tests.E2E
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium

# Start web application (in one terminal)
cd JdhPro.Web
dotnet run

# Run all tests (in another terminal)
cd JdhPro.Tests.E2E
dotnet test

# Run specific test suite
dotnet test --filter "FullyQualifiedName~BlogSyndication"
dotnet test --filter "FullyQualifiedName~Performance"

# Run in headed mode (see browser)
$env:Headless="false"
dotnet test
```

### Test Against Production

```bash
$env:BaseUrl="https://jdhproductions.com"
dotnet test
```

### Test Documentation

For comprehensive testing documentation, see:
- **[JdhPro.Tests.E2E/TESTING_GUIDE.md](JdhPro.Tests.E2E/TESTING_GUIDE.md)** - Complete testing guide
- **[JdhPro.Tests.E2E/README.md](JdhPro.Tests.E2E/README.md)** - Testing overview

### CI/CD Testing

Tests automatically run in GitHub Actions pipeline:
- Blog syndication runs first, generates posts.json
- E2E tests run in headless Chrome
- Screenshots and videos captured on failure
- Test results uploaded as artifacts

**Artifacts:**
- Test results (.trx files)
- Screenshots (on failure)
- Videos (on failure)
- Blog posts (posts.json)



## 🌐 Deployment

For detailed build and deployment instructions, see **[BUILD.md](BUILD.md)**.

### Supported Platforms

| Platform | Status | Primary | Notes |
|----------|--------|---------|-------|
| **Vercel** | ✅ Ready | ✅ Yes | Recommended, automatic previews |
| **GitHub Pages** | ✅ Ready | | Free, good performance |
| **Cloudflare Pages** | ✅ Ready | | Global CDN, excellent performance |
| **Netlify** | ⚠️ Manual | | Requires manual setup |
| **Azure Static Web Apps** | ⚠️ Manual | | Requires manual setup |
| **AWS S3 + CloudFront** | ⚠️ Manual | | Requires manual setup |

### Quick Deploy

**Prerequisites:**
- GitHub repository
- Platform account (Vercel, etc.)
- Required secrets configured

**Steps:**
1. Configure platform secrets in GitHub repository settings
2. Push to `main` branch
3. GitHub Actions automatically builds and deploys
4. Visit deployment URL

### CI/CD Automation

All deployments are fully automated with GitHub Actions:

- **Trigger on push** to `main` branch
- **Trigger on pull request** (preview deployments on Vercel)
- **Scheduled builds** daily to refresh content
- **Manual trigger** via workflow dispatch

**Workflow Features:**
- Dependency caching for faster builds
- AOT compilation with .NET 10
- Tailwind CSS minification
- Automatic deployment to configured platforms
- Build status notifications

## 📝 Content Management

### Services

Service pages are managed as Markdown files:

**Location:** `content/services/*.md`

**Example:**
```markdown
# Web Development

We build modern, responsive websites and web applications.

## Our Approach
- User-centered design
- Performance optimization
- SEO best practices

## Technologies
- React, Vue, Blazor
- Node.js, .NET
- Tailwind CSS
```

**To add a new service:**
1. Create `content/services/service-name.md`
2. Write content in Markdown
3. Commit and push
4. Service automatically appears on Services page

### Configuration

Site configuration is managed via JSON files:

**Location:** `content/config/*.json`

**Example - Site Metadata:**
```json
{
  "siteName": "JDH Productions",
  "tagline": "Building Digital Excellence",
  "description": "Professional software development and consulting",
  "contactEmail": "info@jdhproductions.com"
}
```

### Blog Posts (Future Enhancement)

While not yet implemented, the architecture supports blog posts:

**Planned location:** `content/blog/*.md`

**Example structure:**
```markdown
---
title: "My Blog Post"
date: 2026-01-15
author: "John Doe"
tags: ["web", "development"]
---

# Post content here...
```

## ⚙️ Configuration

### Contact Provider Setup

The site uses pluggable contact providers. Configure in `Program.cs`:

```csharp
// FormSpree (default)
builder.Services.AddScoped<IContactProvider, FormSpreeProvider>();

// Or Web3Forms
builder.Services.AddScoped<IContactProvider, Web3FormsProvider>();
```

**Environment Variables:**
- `CONTACT_PROVIDER_ENDPOINT` - Form endpoint URL
- `CONTACT_PROVIDER_KEY` - API key (if required)

### Site Metadata

Edit `content/config/site.json`:

```json
{
  "siteName": "JDH Productions",
  "url": "https://jdhproductions.com",
  "description": "Professional services company",
  "author": "JDH Productions LLC",
  "socialMedia": {
    "twitter": "@jdhpro",
    "linkedin": "company/jdh-productions"
  }
}
```

### Build Configuration

Modify `JdhPro.Web/JdhPro.Web.csproj` for build settings:

```xml
<PropertyGroup>
  <!-- Enable/disable AOT compilation -->
  <RunAOTCompilation>true</RunAOTCompilation>
  
  <!-- IL stripping for smaller bundles -->
  <WasmStripILAfterAOT>true</WasmStripILAfterAOT>
  
  <!-- Compression -->
  <BlazorEnableCompression>true</BlazorEnableCompression>
</PropertyGroup>
```

## 🎯 Performance

### Expected Lighthouse Scores

With AOT compilation and optimizations, the site achieves excellent Lighthouse scores:

- **Performance:** 95-100
- **Accessibility:** 95-100
- **Best Practices:** 95-100
- **SEO:** 95-100

### AOT Compilation Benefits

Ahead-of-Time (AOT) compilation provides significant performance improvements:

| Metric | Interpreted | AOT | Improvement |
|--------|-------------|-----|-------------|
| **First Load** | ~2.5s | ~2.8s | -10% (larger download) |
| **Runtime Speed** | Baseline | 40-50% faster | ✅ Significant |
| **Memory Usage** | Baseline | ~20% less | ✅ Improved |
| **Bundle Size** | Smaller | Larger | ⚠️ Trade-off |

**When to use AOT:**
- Production deployments (always)
- Performance-critical applications
- CPU-intensive workloads

**When to disable AOT:**
- Development (faster build times)
- Quick prototyping
- Size-constrained scenarios

### Multi-CDN Strategy

The site can be deployed to multiple CDN providers simultaneously:

- **Vercel** - Edge network, 30+ regions
- **Cloudflare** - 300+ edge locations worldwide
- **GitHub Pages** - Fastly CDN

**Benefits:**
- Geographic redundancy
- Failover capability
- Performance testing and comparison
- Load distribution

**Implementation:**
- Single source repository
- Multiple GitHub Action workflows
- Independent deployments
- DNS-level routing (optional)

## 🤝 Contributing

We welcome contributions! Here's how to get started:

### Development Workflow

1. **Fork the repository**
2. **Clone your fork:**
   ```bash
   git clone https://github.com/yourusername/JDHPro.git
   ```
3. **Create a feature branch:**
   ```bash
   git checkout -b feature/my-new-feature
   ```
4. **Make your changes**
5. **Test thoroughly:**
   ```bash
   dotnet build
   dotnet test  # If tests exist
   dotnet run   # Verify locally
   ```
6. **Commit with meaningful messages:**
   ```bash
   git commit -m "feat: add new service page"
   ```
7. **Push to your fork:**
   ```bash
   git push origin feature/my-new-feature
   ```
8. **Open a Pull Request**

### Pull Request Process

1. **Update documentation** if you've changed functionality
2. **Follow existing code style** (uses .NET conventions)
3. **Write clear PR description** explaining what and why
4. **Ensure CI passes** (GitHub Actions will run automatically)
5. **Request review** from maintainers
6. **Address feedback** promptly

### Code Style

- Follow [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Comment complex logic (but prefer self-documenting code)
- Keep components small and focused
- Write Tailwind classes in logical order (layout → spacing → colors → effects)

### Commit Message Format

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Types:**
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, etc.)
- `refactor:` - Code refactoring
- `perf:` - Performance improvements
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks

**Examples:**
```
feat(services): add cloud consulting service page
fix(contact): resolve form validation issue
docs(readme): update deployment instructions
style(tailwind): reorder utility classes
```

## 📄 License

Copyright © 2026 JDH Productions LLC. All rights reserved.

This project is proprietary software. Unauthorized copying, distribution, or use of this software, via any medium, is strictly prohibited without explicit written permission from JDH Productions LLC.

For licensing inquiries, please contact: licensing@jdhproductions.com

---

## 📚 Additional Documentation

- **[BUILD.md](BUILD.md)** - Comprehensive Nuke build system documentation
- **[JdhPro.BuildTools/README.md](JdhPro.BuildTools/README.md)** - Blog syndication tool
- **[JdhPro.Tests.E2E/README.md](JdhPro.Tests.E2E/README.md)** - E2E testing overview
- **[JdhPro.Tests.E2E/TESTING_GUIDE.md](JdhPro.Tests.E2E/TESTING_GUIDE.md)** - Comprehensive testing guide

## 🔗 Useful Links

### Documentation
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [Markdig Documentation](https://github.com/xoofx/markdig)

### Deployment Platforms
- [Vercel Documentation](https://vercel.com/docs)
- [GitHub Pages Documentation](https://docs.github.com/en/pages)
- [Cloudflare Pages Documentation](https://developers.cloudflare.com/pages)

### Tools & Resources
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Lighthouse Performance Tool](https://developers.google.com/web/tools/lighthouse)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
- [VS Code](https://code.visualstudio.com/)

## 🙋 Support & Contact

### Questions or Issues?

- **Technical Issues:** Open an issue in this repository
- **Business Inquiries:** info@jdhproductions.com
- **General Questions:** Contact through website contact form

### Community

- **Discussions:** Use GitHub Discussions for questions and ideas
- **Bug Reports:** Use GitHub Issues with detailed reproduction steps
- **Feature Requests:** Use GitHub Issues with `enhancement` label

---

<div align="center">

**Built with ❤️ by JDH Productions**

[Website](https://jdhproductions.com) • [GitHub](https://github.com/jdhproductions) • [Contact](mailto:info@jdhproductions.com)

</div>
