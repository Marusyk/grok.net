#addin nuget:?package=Cake.Coveralls&version=1.1.0
#addin nuget:?package=Cake.Coverlet&version=3.0.4
#addin nuget:?package=Cake.Powershell&version=2.0.0&loaddependencies=true

#tool nuget:?package=coveralls.io&version=1.4.2

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var coverallsRepoToken = EnvironmentVariable("COVERALLS_REPO_TOKEN");
var nugetApiKey = EnvironmentVariable("NUGET_API_KEY");
var nugetApiUrl = EnvironmentVariable("NUGET_API_URL");
var psNugetApiKey = EnvironmentVariable("PS_NUGET_API_KEY");

var artifactsDir = Directory("./artifacts/");
var testResultsDir = artifactsDir.Combine("test-results");
var psModuleDir = artifactsDir.Combine("Grok");
var testCoverageOutputFilePath = testResultsDir.CombineWithFilePath("OpenCover.xml");

var projectFileMain = File("./src/Grok.Net/Grok.Net.csproj");
var projectFilePowerShell = File("./src/Grok.Net.PowerShell/Grok.Net.PowerShell.csproj");

var commonSettings = new DotNetBuildSettings
{
    Configuration = configuration,
    NoRestore = true,
    NoLogo = true
};

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    CleanDirectories(GetDirectories("./**/obj") + GetDirectories("./**/bin"));
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetRestore(projectFileMain);
    DotNetRestore(projectFilePowerShell);
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetBuild(projectFileMain, commonSettings);
    DotNetBuild(projectFilePowerShell, commonSettings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testSettings = new DotNetTestSettings
    {
        Configuration = configuration,
        NoLogo = true
    };

    DotNetTest("./src/Grok.Net.Tests/Grok.Net.Tests.csproj", testSettings, new CoverletSettings
    {
        CollectCoverage = true,
        CoverletOutputFormat = CoverletOutputFormat.opencover,
        CoverletOutputDirectory = testResultsDir,
        CoverletOutputName = testCoverageOutputFilePath.GetFilename().ToString()
    });
});

Task("UploadTestReport")
    .IsDependentOn("Test")
    .WithCriteria(context => FileExists(testCoverageOutputFilePath) &&
                             !string.IsNullOrWhiteSpace(coverallsRepoToken) &&
                             !BuildSystem.IsPullRequest &&
                             !BuildSystem.IsLocalBuild)
    .Does(() =>
{
    CoverallsIo(testCoverageOutputFilePath, new CoverallsIoSettings
    {
        RepoToken = coverallsRepoToken,
        Debug = true
    });
});

Task("NuGetPack")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetPack(projectFileMain, new DotNetPackSettings
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        NoLogo = true,
        OutputDirectory = artifactsDir
    });
});

Task("NuGetPush")
    .IsDependentOn("NuGetPack")
    .WithCriteria(context => !string.IsNullOrWhiteSpace(nugetApiUrl) && !string.IsNullOrWhiteSpace(nugetApiKey))
    .Does(() =>
{
    var packages = GetFiles($"{artifactsDir.FullPath}/*.nupkg");
    DotNetNuGetPush(packages.First(), new DotNetNuGetPushSettings
    {
        Source = nugetApiUrl,
        ApiKey = nugetApiKey
    });
});

Task("PsModulePack")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetPublish(projectFilePowerShell, new DotNetPublishSettings
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        NoLogo = true,
        OutputDirectory = psModuleDir
    });
});

Task("PsModulePush")
    .IsDependentOn("PsModulePack")
    .WithCriteria(context => !string.IsNullOrWhiteSpace(psNugetApiKey))
    .Does(() =>
{
    StartPowershellScript("Publish-Module", new PowershellSettings()
        .WithModule("PowerShellGet")
        .BypassExecutionPolicy()
        .WithArguments(args =>
        {
            args.Append("Path", psModuleDir.FullPath)
                .AppendSecret("NuGetApiKey", psNugetApiKey)
                .Append("Force", "");
        }));
});

Task("Default")
    .IsDependentOn("Test");

RunTarget(target);
