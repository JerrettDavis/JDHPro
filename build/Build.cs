using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.CI);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Parameter("Enable GitHub Pages mode (adjusts base path)")]
    readonly bool GitHubPages = false;

    [Parameter("Repository name for GitHub Pages base path")]
    readonly string RepositoryName = "jdhpro";

    [Solution(GenerateProjects = false)]
    readonly Solution? Solution;

    // Project references
    Project? WebProject => Solution?.GetProject("JdhPro.Web");
    Project? BuildToolsProject => Solution?.GetProject("JdhPro.BuildTools");
    Project? E2ETestsProject => Solution?.GetProject("JdhPro.Tests.E2E");

    // Paths
    AbsolutePath SourceDirectory => RootDirectory;
    AbsolutePath WebProjectDirectory => RootDirectory / "JdhPro.Web";
    AbsolutePath BuildToolsDirectory => RootDirectory / "JdhPro.BuildTools";
    AbsolutePath E2ETestsDirectory => RootDirectory / "JdhPro.Tests.E2E";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath PublishDirectory => RootDirectory / "publish";
    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test-results";

    Target Clean => _ => _
        .Description("Clean build artifacts and temporary files")
        .Executes(() =>
        {
            Log.Information("Cleaning build artifacts...");
            
            // Clean bin/obj directories
            SourceDirectory.GlobDirectories("**/bin", "**/obj")
                .Where(d => !d.ToString().Contains("node_modules"))
                .ForEach(d => d.DeleteDirectory());
            
            // Clean artifacts and publish directories
            ArtifactsDirectory.CreateOrCleanDirectory();
            PublishDirectory.DeleteDirectory();
            
            // Clean Tailwind CSS output
            (WebProjectDirectory / "wwwroot" / "css" / "app.min.css").DeleteFile();
            
            Log.Information("✅ Clean completed");
        });

    Target Restore => _ => _
        .Description("Restore .NET and npm dependencies")
        .Executes(() =>
        {
            Log.Information("Installing .NET workloads...");
            DotNet("workload restore");

            Log.Information("Restoring .NET dependencies...");
            if (Solution != null)
            {
                DotNetRestore(s => s
                    .SetProjectFile(Solution));
            }
            else
            {
                // Restore all projects in the directory
                DotNetRestore(s => s
                    .SetProcessWorkingDirectory(RootDirectory));
            }

            Log.Information("Restoring npm dependencies...");
            NpmInstall(s => s
                .SetProcessWorkingDirectory(WebProjectDirectory));

            Log.Information("✅ Restore completed");
        });

    Target BuildCss => _ => _
        .Description("Build Tailwind CSS")
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information("Building Tailwind CSS...");
            
            var isProduction = Configuration == "Release";
            
            NpmRun(s => s
                .SetProcessWorkingDirectory(WebProjectDirectory)
                .SetCommand(isProduction ? "build" : "build:css")
                .SetProcessEnvironmentVariable("NODE_ENV", isProduction ? "production" : "development"));

            var cssFile = WebProjectDirectory / "wwwroot" / "css" / "app.min.css";
            if (!cssFile.FileExists())
            {
                throw new Exception("Tailwind CSS build failed - app.min.css not found");
            }

            var fileSize = new FileInfo(cssFile).Length;
            Log.Information($"✅ Tailwind CSS built: {fileSize / 1024}KB");
        });

    Target SyndicateBlog => _ => _
        .Description("Run blog syndication tool")
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information("Running blog syndication...");
            
            if (BuildToolsProject != null)
            {
                DotNetRun(s => s
                    .SetProjectFile(BuildToolsProject)
                    .SetProcessWorkingDirectory(BuildToolsDirectory)
                    .SetNoRestore(true)
                    .SetConfiguration(Configuration));
            }
            else
            {
                // Run with explicit project file path and working directory
                DotNetRun(s => s
                    .SetProjectFile(BuildToolsDirectory / "JdhPro.BuildTools.csproj")
                    .SetProcessWorkingDirectory(BuildToolsDirectory)
                    .SetNoRestore(true)
                    .SetConfiguration(Configuration));
            }

            var postsJson = WebProjectDirectory / "wwwroot" / "data" / "posts.json";
            if (!postsJson.FileExists())
            {
                Log.Warning("⚠️ Blog syndication did not create posts.json");
            }
            else
            {
                var fileSize = new FileInfo(postsJson).Length;
                Log.Information($"✅ Blog posts syndicated: {fileSize / 1024}KB");
            }
        });

    Target Compile => _ => _
        .Description("Compile all projects")
        .DependsOn(Restore, BuildCss, SyndicateBlog)
        .Executes(() =>
        {
            Log.Information("Compiling solution...");
            
            if (Solution != null)
            {
                DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetVerbosity(DotNetVerbosity.minimal));
            }
            else
            {
                // Build all projects in the directory
                DotNetBuild(s => s
                    .SetProcessWorkingDirectory(RootDirectory)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetVerbosity(DotNetVerbosity.minimal));
            }

            Log.Information("✅ Compilation completed");
        });

    Target Test => _ => _
        .Description("Run unit tests (excluding E2E tests)")
        .DependsOn(Compile)
        .Produces(TestResultsDirectory / "*.trx")
        .Executes(() =>
        {
            Log.Information("Running unit tests...");
            
            if (Solution != null)
            {
                DotNetTest(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetFilter("FullyQualifiedName!~E2E")
                    .SetResultsDirectory(TestResultsDirectory)
                    .SetLoggers("trx;LogFileName=unit-tests.trx")
                    .SetVerbosity(DotNetVerbosity.minimal));
            }
            else
            {
                // Test all projects in the directory
                DotNetTest(s => s
                    .SetProcessWorkingDirectory(RootDirectory)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetFilter("FullyQualifiedName!~E2E")
                    .SetResultsDirectory(TestResultsDirectory)
                    .SetLoggers("trx;LogFileName=unit-tests.trx")
                    .SetVerbosity(DotNetVerbosity.minimal));
            }

            Log.Information("✅ Unit tests completed");
        });

    Target TestE2E => _ => _
        .Description("Run E2E tests with Playwright")
        .DependsOn(Compile)
        .Produces(TestResultsDirectory / "e2e-*.trx")
        .Executes(() =>
        {
            Log.Information("Running E2E tests...");
            Log.Warning("⚠️ E2E tests require the web app to be running separately");
            Log.Information("Run: cd JdhPro.Web && dotnet run");
            
            if (E2ETestsProject != null)
            {
                DotNetTest(s => s
                    .SetProjectFile(E2ETestsProject)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetResultsDirectory(TestResultsDirectory)
                    .SetLoggers("trx;LogFileName=e2e-tests.trx")
                    .SetProcessEnvironmentVariable("BaseUrl", "http://localhost:5233")
                    .SetProcessEnvironmentVariable("Headless", "true")
                    .SetVerbosity(DotNetVerbosity.normal));
            }
            else
            {
                // Test with explicit project file path
                DotNetTest(s => s
                    .SetProjectFile(E2ETestsDirectory / "JdhPro.Tests.E2E.csproj")
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetResultsDirectory(TestResultsDirectory)
                    .SetLoggers("trx;LogFileName=e2e-tests.trx")
                    .SetProcessEnvironmentVariable("BaseUrl", "http://localhost:5233")
                    .SetProcessEnvironmentVariable("Headless", "true")
                    .SetVerbosity(DotNetVerbosity.normal));
            }

            Log.Information("✅ E2E tests completed");
        });

    Target Publish => _ => _
        .Description("Publish web project to publish directory")
        .DependsOn(Compile)
        .Produces(PublishDirectory / "**/*")
        .Executes(() =>
        {
            Log.Information("Publishing web application...");
            
            if (WebProject != null)
            {
                DotNetPublish(s => s
                    .SetProject(WebProject)
                    .SetConfiguration(Configuration)
                    .SetOutput(PublishDirectory)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetProperty("DebugType", "None")
                    .SetProperty("DebugSymbols", "false")
                    .SetVerbosity(DotNetVerbosity.minimal));
            }
            else
            {
                // Publish with explicit project file path
                DotNetPublish(s => s
                    .SetProject(WebProjectDirectory / "JdhPro.Web.csproj")
                    .SetConfiguration(Configuration)
                    .SetOutput(PublishDirectory)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetProperty("DebugType", "None")
                    .SetProperty("DebugSymbols", "false")
                    .SetVerbosity(DotNetVerbosity.minimal));
            }

            Log.Information("✅ Publish completed");
            LogPublishInfo();
        });

    Target PublishGitHubPages => _ => _
        .Description("Publish with GitHub Pages optimizations")
        .DependsOn(Publish)
        .Executes(() =>
        {
            Log.Information("Applying GitHub Pages optimizations...");
            
            var wwwroot = PublishDirectory / "wwwroot";
            
            // Create .nojekyll to disable Jekyll processing
            var nojekyll = wwwroot / ".nojekyll";
            nojekyll.TouchFile();
            Log.Information("✅ Created .nojekyll file");
            
            // Copy index.html to 404.html for SPA routing
            var indexHtml = wwwroot / "index.html";
            var notFoundHtml = wwwroot / "404.html";
            File.Copy(indexHtml, notFoundHtml, overwrite: true);
            Log.Information("✅ Created 404.html for SPA routing");
            
            // Update base href if GitHub Pages mode
            if (GitHubPages)
            {
                Log.Information($"Updating base href for GitHub Pages (/{RepositoryName}/)...");
                var content = File.ReadAllText(indexHtml);
                content = content.Replace(
                    "<base href=\"/\" />", 
                    $"<base href=\"/{RepositoryName}/\" />");
                
                // Fix blazor.webassembly.js reference to use hashed version
                // The import map handles module imports, but the script tag needs the actual file
                var importMapMatch = System.Text.RegularExpressions.Regex.Match(content, 
                    @"""\./_framework/blazor\.webassembly\.js"":\s*""\./_framework/blazor\.webassembly\.([^""]+)\.js""");
                if (importMapMatch.Success)
                {
                    var hashedFile = $"_framework/blazor.webassembly.{importMapMatch.Groups[1].Value}.js";
                    content = System.Text.RegularExpressions.Regex.Replace(content,
                        @"<script src=""_framework/blazor\.webassembly\.js""",
                        $@"<script src=""{hashedFile}""");
                    Log.Information($"✅ Updated blazor.webassembly.js script reference to {hashedFile}");
                }
                
                File.WriteAllText(indexHtml, content);
                
                // Also update 404.html
                content = File.ReadAllText(notFoundHtml);
                content = content.Replace(
                    "<base href=\"/\" />", 
                    $"<base href=\"/{RepositoryName}/\" />");
                    
                // Fix blazor.webassembly.js in 404.html too
                importMapMatch = System.Text.RegularExpressions.Regex.Match(content, 
                    @"""\./_framework/blazor\.webassembly\.js"":\s*""\./_framework/blazor\.webassembly\.([^""]+)\.js""");
                if (importMapMatch.Success)
                {
                    var hashedFile = $"_framework/blazor.webassembly.{importMapMatch.Groups[1].Value}.js";
                    content = System.Text.RegularExpressions.Regex.Replace(content,
                        @"<script src=""_framework/blazor\.webassembly\.js""",
                        $@"<script src=""{hashedFile}""");
                }
                
                File.WriteAllText(notFoundHtml, content);
                
                Log.Information("✅ Updated base href and script references in index.html and 404.html");
            }
            
            Log.Information("✅ GitHub Pages optimizations applied");
            LogPublishInfo();
        });

    Target VerifyPublish => _ => _
        .Description("Verify published output")
        .DependsOn(Publish)
        .Executes(() =>
        {
            Log.Information("Verifying published output...");
            
            var wwwroot = PublishDirectory / "wwwroot";
            
            // Check critical files
            var criticalFiles = new[]
            {
                wwwroot / "index.html",
                wwwroot / "css" / "app.min.css",
                wwwroot / "data" / "posts.json"
            };

            var allExist = true;
            foreach (var file in criticalFiles)
            {
                if (file.FileExists())
                {
                    var size = new FileInfo(file).Length;
                    Log.Information($"✅ {file.Name}: {size / 1024}KB");
                }
                else
                {
                    Log.Error($"❌ Missing: {file}");
                    allExist = false;
                }
            }

            if (!allExist)
            {
                throw new Exception("Published output verification failed - critical files missing");
            }

            // Check for WASM files
            var wasmFiles = (wwwroot / "_framework").GlobFiles("*.wasm");
            Log.Information($"✅ Found {wasmFiles.Count} WASM files");

            Log.Information("✅ Publish verification completed");
        });

    Target CI => _ => _
        .Description("Full CI pipeline: Clean → Restore → BuildCss → SyndicateBlog → Compile → Test")
        .DependsOn(Clean, Restore, BuildCss, SyndicateBlog, Compile, Test)
        .Executes(() =>
        {
            Log.Information("✅ CI pipeline completed successfully");
        });

    Target Full => _ => _
        .Description("Full build: CI → TestE2E → Publish → Verify")
        .DependsOn(CI, TestE2E, Publish, VerifyPublish)
        .Executes(() =>
        {
            Log.Information("✅ Full build completed successfully");
        });

    Target Deploy => _ => _
        .Description("Deployment pipeline: Full → PublishGitHubPages")
        .DependsOn(Full, PublishGitHubPages)
        .Executes(() =>
        {
            Log.Information("✅ Deployment artifacts ready");
            Log.Information($"📁 Output: {PublishDirectory / "wwwroot"}");
        });

    // Helper method to log publish information
    void LogPublishInfo()
    {
        var wwwroot = PublishDirectory / "wwwroot";
        if (wwwroot.DirectoryExists())
        {
            var totalSize = wwwroot.GlobFiles("**/*")
                .Sum(f => new FileInfo(f).Length);
            
            Log.Information($"📦 Total size: {totalSize / 1024 / 1024}MB");
            Log.Information($"📁 Output: {wwwroot}");
        }
    }
}
