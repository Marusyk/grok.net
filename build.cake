// Install addins.
#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.Coveralls&version=1.0.0"
#addin nuget:?package=Cake.Coverlet&version=2.5.4

// Install tools
#tool "nuget:https://api.nuget.org/v3/index.json?package=coveralls.io&version=1.4.2"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var artifactsDir = "./artifacts/";
var projectFile = "./src/Grok.Net/Grok.Net.csproj";
var testResultFile = "./test-results/results.xml";
var coverallsRepoToken = EnvironmentVariable("COVERALLS_REPO_TOKEN");

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);

    if(BuildSystem.IsLocalBuild)
    {
        CleanDirectories(GetDirectories("./**/obj") + GetDirectories("./**/bin"));
    }
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(projectFile);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreBuild(projectFile, new DotNetCoreBuildSettings()
    { 
        Configuration = configuration,
        NoRestore = true
    });
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projectFile = Directory("./src/Grok.Net.Tests") + File("Grok.Net.Tests.csproj");

    if(BuildSystem.IsLocalBuild || BuildSystem.IsPullRequest)
    {
        DotNetCoreTest(System.IO.Path.GetFullPath(projectFile), new DotNetCoreTestSettings()
        {
            Configuration = configuration
        });
    }
    else
    {
        var coverletSettings = new CoverletSettings {
            CollectCoverage = true,
            CoverletOutputFormat = CoverletOutputFormat.opencover,
            CoverletOutputDirectory = Directory(@".\test-results\"),
            CoverletOutputName = "results.xml"
        };

        DotNetCoreTest(System.IO.Path.GetFullPath(projectFile), new DotNetCoreTestSettings()
        {
            Configuration = configuration
        },
        coverletSettings);
    }
});

Task("Upload-Coverage-Report")
    .IsDependentOn("Run-Unit-Tests")
    .WithCriteria((context) => !BuildSystem.IsLocalBuild))
    .Does(() =>
{
    CoverallsIo(testResultFile, new CoverallsIoSettings()
    {
        RepoToken = coverallsRepoToken,
        Debug = true
    });
});

Task("NuGet-Pack")
    .IsDependentOn("Upload-Coverage-Report")
    .Does(() =>
{
    DotNetCorePack(projectFile, new DotNetCorePackSettings()
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = artifactsDir
    });
});

Task("Default")
    .IsDependentOn("NuGet-Pack");

RunTarget(target);
