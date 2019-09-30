#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");


var buildDir = Directory($"./build/{configuration}/");
var solutionFile = "./src/Grok.Net.sln";
var outputNupkg = $"./src/Grok.Net/bin/{configuration}/*.nupkg";

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionFile);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      MSBuild(solutionFile, settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      XBuild(solutionFile, settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    // No unit tests yet!
    //NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
    //    NoResults = true
    //    });
});

Task("Copy-Output")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    CopyFiles(outputNupkg, buildDir);
});

Task("Default")
    .IsDependentOn("Copy-Output");

RunTarget(target);
