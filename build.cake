var target = Argument("target", "Publish");
var configuration = Argument("configuration", "Release");

var version = "5.1.0";


Task("CleanInstaller")
    .Does(() => 
{
    CleanDirectory($"./artifacts/Installer.v{version}-Windows");
    CleanDirectory($"./artifacts/Installer.v{version}-OSXLinux");
});


Task("BuildInstaller")
    .IsDependentOn("CleanInstaller")
    .Does(() => 
{
    DotNetBuild("./Installer/Installer.csproj", new DotNetBuildSettings 
    {
        Configuration = configuration
    });
    DotNetBuild("./Installer/Installer.Kick.csproj", new DotNetBuildSettings 
    {
        Configuration = configuration
    });
});

Task("PublishInstaller")
    .IsDependentOn("BuildInstaller")
    .Does(() => 
{
    DotNetPublish("./Installer/Installer.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/Installer.v{version}-Windows",
        NoBuild = true
    });
    DotNetPublish("./Installer/Installer.Kick.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/Installer.v{version}-OSXLinux",
        NoBuild = true
    });
});

RunTarget(target);