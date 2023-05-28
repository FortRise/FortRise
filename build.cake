var target = Argument("target", "Publish");
var configuration = Argument("configuration", "Release");
var version = "3.2.0";

Task("CleanInstallerANSI")
    .Does(() => 
{
    CleanDirectory($"./artifacts/FortRise.Installer.v{version}");
});

Task("CleanInstallerNoANSI")
    .Does(() => 
{
    CleanDirectory($"./artifacts/FortRise.Installer.v{version}-NoANSI");
});

Task("BuildInstallerANSI")
    .IsDependentOn("CleanInstallerANSI")
    .Does(() => 
{
    DotNetBuild("./Installer/Installer.csproj", new DotNetBuildSettings 
    {
        Configuration = configuration,
        MSBuildSettings = new DotNetMSBuildSettings 
        {
            Version = version
        }
    });
});

Task("PublishInstallerANSI")
    .IsDependentOn("BuildInstallerANSI")
    .Does(() => 
{
    DotNetPublish("./Installer/Installer.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/FortRise.Installer.v{version}",
        Runtime = "win-x64",
        NoBuild = true,
        MSBuildSettings = new DotNetMSBuildSettings 
        {
            Version = version
        },
    });
});

Task("BuildInstallerNoANSI")
    .IsDependentOn("CleanInstallerNoANSI")
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

Task("PublishInstallerNoANSI")
    .IsDependentOn("BuildInstallerNoANSI")
    .Does(() => 
{
    DotNetPublish("./Installer/Installer.NoAnsi.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/FortRise.Installer.v{version}-NoANSI",
        Runtime = "win-x64",
        MSBuildSettings = new DotNetMSBuildSettings 
        {
            Version = version
        },
        NoBuild = true
    });
});

Task("Publish")
    .IsDependentOn("CleanInstallerNoANSI")
    .IsDependentOn("CleanInstallerANSI")
    .IsDependentOn("BuildInstallerANSI")
    .IsDependentOn("PublishInstallerANSI")
    .IsDependentOn("BuildInstallerNoANSI")
    .Does(() => 
{
    DotNetPublish("./Installer/Installer.NoAnsi.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/FortRise.Installer.v{version}-NoANSI",
        NoBuild = true,
        MSBuildSettings = new DotNetMSBuildSettings 
        {
            Version = version
        },
    });
});



RunTarget(target);