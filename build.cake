#tool nuget:?package=ReportGenerator&version=4.8.13
#tool nuget:?package=OpenCover&version=4.7.1221

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var artifactsDir = "./artifacts/";
var projectFile = "./src/Grok.Net/Grok.Net.csproj";
var testResultFile = "./test-results/results.xml";

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

Task("Run-Unit-Tests-With-OpenCover")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projectFile = Directory("./src/Grok.Net.Tests") + File("Grok.Net.Tests.csproj");
    OpenCover(tool => {
            tool.DotNetCoreTest(
              System.IO.Path.GetFullPath(projectFile),
              new DotNetCoreTestSettings()
              {
                  Configuration = configuration
              }
            );
        },
        testResultFile, new OpenCoverSettings()
    );
});

Task("Generate-Coverage-Badges")
    .IsDependentOn("Run-Unit-Tests-With-OpenCover")
    .Does(() =>
{    
    ReportGenerator((FilePath)testResultFile,
        "./coverage",
        new ReportGeneratorSettings()
        {
            ReportTypes = new List<ReportGeneratorReportType>() { ReportGeneratorReportType.Badges },
            AssemblyFilters = new List<string>() { "-xunit.*" }
        }
    );
});

Task("NuGet-Pack")
    .IsDependentOn("Generate-Coverage-Badges")
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
