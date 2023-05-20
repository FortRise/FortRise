var target = Argument("target", "Publish");
var configuration = Argument("configuration", "Release");

Task("CleanInstallerANSI")
    .Does(() => 
{
    CleanDirectory("./artifacts/ANSI");
});

Task("CleanInstallerNoANSI")
    .Does(() => 
{
    CleanDirectory("./artifacts/NoANSI");
});

Task("BuildInstallerANSI")
    .IsDependentOn("CleanInstallerANSI")
    .Does(() => 
{
    DotNetBuild("./Installer/Installer.csproj", new DotNetBuildSettings 
    {
        Configuration = configuration
    });
});

Task("PublishInstallerANSI")
    .IsDependentOn("BuildInstallerANSI")
    .Does(() => 
{
    DotNetPublish("./Installer/Installer.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = "./artifacts/ANSI",
        NoBuild = true
    });
});

Task("BuildInstallerNoANSI")
    .IsDependentOn("CleanInstallerNoANSI")
    .Does(() => 
{
    DotNetBuild("./Installer/Installer.NoAnsi.csproj", new DotNetBuildSettings 
    {
        Configuration = configuration
    });
});

Task("PublishInstallerNoANSI")
    .IsDependentOn("BuildInstallerNoANSI")
    .Does(() => 
{
    DotNetPublish("./Installer/Installer.NoAnsi.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = "./artifacts/NoANSI",
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
        OutputDirectory = "./artifacts/NoANSI",
        NoBuild = true
    });
});



RunTarget(target);