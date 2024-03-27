var target = Argument("target", "Publish");
var configuration = Argument("configuration", "Release");

var version = "4.8.0";


Task("CleanInstaller")
    .Does(() => 
{
    CleanDirectory($"./artifacts/FortRise.Installer.v{version}-NoANSI");
    CleanDirectory($"./artifacts/FortRise.Installer.v{version}-OSXLinux");
});


Task("BuildInstaller")
    .IsDependentOn("CleanInstaller")
    .Does(() => 
{
    DotNetBuild("./Installer/Installer.NoAnsi.csproj", new DotNetBuildSettings 
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
    DotNetPublish("./Installer/Installer.NoAnsi.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/FortRise.Installer.v{version}-NoANSI",
        NoBuild = true
    });
    DotNetPublish("./Installer/Installer.Kick.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/FortRise.Installer.v{version}-OSXLinux",
        NoBuild = true
    });
});

RunTarget(target);