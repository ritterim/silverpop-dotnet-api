var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var artifactsDir = Directory("./artifacts");

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore("./Silverpop.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreBuild("./Silverpop.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        MSBuildSettings = new DotNetCoreMSBuildSettings
        {
            TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
            Verbosity = DotNetCoreVerbosity.Minimal
        }

        // msbuild.log specified explicitly, see https://github.com/cake-build/cake/issues/1764
        .AddFileLogger(new MSBuildFileLoggerSettings { LogFile = "msbuild.log" })
    });
});

Task("Run-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projectFiles = GetFiles("./test/**/*.csproj");
    foreach(var file in projectFiles)
    {
        if (file.ToString().Contains("Silverpop.Core.Performance"))
        {
            continue;
        }

        DotNetCoreTest(file.FullPath);
    }
});

Task("Package")
    .IsDependentOn("Run-Tests")
    .Does(() =>
{
    DotNetCorePack("./src/Silverpop.Client/Silverpop.Client.csproj", new DotNetCorePackSettings
    {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = artifactsDir
    });
});

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);
