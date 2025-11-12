var target = Argument("target", "Default");
var project = Argument("project", "ta.RWImGui");
var distDirectory = Directory("./dist");
var srcDirectory = Directory("./src");
var projectPath = srcDirectory + Directory(project);
var distSrcDirectory = distDirectory + Directory($"src-{project}");
var distProjectDirectory = distDirectory + Directory(project);

Task("Clean")
    .Does(() =>
{
    if (DirectoryExists(distDirectory))
    {
        CleanDirectory(distDirectory);
        DeleteDirectory(distDirectory, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }
});

Task("Publish")
    .Does(() =>
{
    // Clean the dist/src-{project} directory
    if (DirectoryExists(distSrcDirectory))
    {
        CleanDirectory(distSrcDirectory);
        DeleteDirectory(distSrcDirectory, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }

    // Publish the project
    DotNetPublish(projectPath.ToString(), new DotNetPublishSettings
    {
        Configuration = "Release",
        OutputDirectory = distSrcDirectory
    });

    // Create dist/{project}/plugins directory
    var pluginsDirectory = distProjectDirectory + Directory("plugins");
    EnsureDirectoryExists(pluginsDirectory);

    // Copy published files to dist/{project}/plugins/
    CopyDirectory(distSrcDirectory, pluginsDirectory);

    // Copy modinfo.json
    var modinfoPath = projectPath + File("modinfo.json");
    if (FileExists(modinfoPath))
    {
        CopyFile(modinfoPath, distProjectDirectory + File("modinfo.json"));
    }

    // Copy workshopdata.json (optional)
    var workshopdataPath = projectPath + File("workshopdata.json");
    if (FileExists(workshopdataPath))
    {
        CopyFile(workshopdataPath, distProjectDirectory + File("workshopdata.json"));
    }

    // Copy thumbnail.png (optional)
    var thumbnailPath = projectPath + File("thumbnail.png");
    if (FileExists(thumbnailPath))
    {
        CopyFile(thumbnailPath, distProjectDirectory + File("thumbnail.png"));
    }

    // Clean up the temporary src directory
    if (DirectoryExists(distSrcDirectory))
    {
        CleanDirectory(distSrcDirectory);
        DeleteDirectory(distSrcDirectory, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }
});

Task("Publish-RWImGui")
    .Does(() =>
{
    project = "ta.RWImGui";
    RunTarget("Publish");
});

RunTarget(target);
