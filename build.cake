var target = Argument("target", "Publish");
var configuration = Argument("configuration", "Release");
var version = "4.1.0";

Task("CleanInstaller")
    .Does(() => 
{
    CleanDirectory($"./artifacts/FortRise.Installer.v{version}-NoANSI");
});


Task("BuildInstaller")
    .IsDependentOn("CleanInstaller")
    .Does(() => 
{
    DotNetBuild("./Installer/Installer.NoAnsi.csproj", new DotNetBuildSettings 
    {
        Configuration = configuration,
        MSBuildSettings = new DotNetMSBuildSettings 
        {
            Version = version
        }
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
        MSBuildSettings = new DotNetMSBuildSettings 
        {
            Version = version
        },
        NoBuild = true
    });
});

RunTarget(target);