var target = Argument("target", "Publish");
var configuration = Argument("configuration", "Release");

var version = "5.0.0-beta.1";


Task("CleanInstaller")
    .Does(() => 
{
    CleanDirectory($"./artifacts/FortRise.v{version}-win-x64");
    CleanDirectory($"./artifacts/FortRise.v{version}-win-x86");
    CleanDirectory($"./artifacts/FortRise.v{version}-linux-x64");
    CleanDirectory($"./artifacts/FortRise.v{version}-osx-x64");
});


Task("BuildInstaller")
    .IsDependentOn("CleanInstaller")
    .Does(() => 
{
    DotNetBuild("./FortLauncher/FortLauncher.csproj", new DotNetBuildSettings 
    {
        Configuration = configuration
    });
});

Task("PublishInstaller")
    .Does(() => 
{
    DotNetPublish("./FortLauncher/FortLauncher.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/FortRise.v{version}-win-x64",
        Runtime = "win-x64",
        SelfContained = true
    });
    DotNetPublish("./FortLauncher/FortLauncher.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/FortRise.v{version}-linux-x64",
        Runtime = "linux-x64",
        SelfContained = true
    });
    DotNetPublish("./FortLauncher/FortLauncher.csproj", new DotNetPublishSettings 
    {
        Configuration = configuration,
        OutputDirectory = $"./artifacts/FortRise.v{version}-osx-x64",
        Runtime = "osx-x64",
        SelfContained = true
    });
});

RunTarget(target);