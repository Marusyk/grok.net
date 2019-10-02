var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var artifactsDir = "./artifacts/";
var projectFile = "./src/Grok.Net/Grok.Net.csproj";

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
    // No unit tests yet!
});

Task("NuGet-Pack")
    .IsDependentOn("Run-Unit-Tests")
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
