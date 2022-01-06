#addin nuget:?package=Cake.Powershell&version=1.0.1&loaddependencies=true

var psNugetApiKey = EnvironmentVariable("PS_NUGET_API_KEY");

Task("PsGalleryPush")
    .IsDependentOn("Test")
    .WithCriteria(!string.IsNullOrWhiteSpace(psNugetApiKey))
    .Does(() =>
{
    var srcDir = projectFilePowerShell.Path.GetDirectory().FullPath;
    var buildDir = string.Concat(srcDir, "/bin/", configuration, "/netstandard2.0");
    var destDir = artifactsDir.FullPath + "/Grok";
    EnsureDirectoryExists(destDir);
    CopyDirectory(buildDir, destDir);
    
    StartPowershellScript("Publish-Module", new PowershellSettings()
        {
            Modules = new List<string>() { "PowerShellGet" },
            BypassExecutionPolicy = true
        }
        .WithArguments(args =>
        {
            args.Append("Path", destDir)
            .AppendSecret("NuGetApiKey", psNugetApiKey)
            .Append("Force", "");
        }));
});

#load "build.cake"