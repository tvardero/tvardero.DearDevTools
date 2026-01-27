var target = Argument("target", "CopyModToRW");
var configuration = Argument("configuration", "Debug");
var rainWorldPath = Argument("rainWorldPath", string.Empty);
var projectName = Argument("project", "tvardero.DearDevTools");

Task("Clean")
    .Does(() =>
{
    var projectPath = $"./src/{projectName}";
    var outputPath = $"./dist/{projectName}";

    DotNetClean(projectPath);

    if (DirectoryExists(outputPath))
    {
        CleanDirectory(outputPath);
    }
    
    Information($"Cleaned project output");
});

Task("PackMod")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var projectPath = $"./src/{projectName}";
    var outputPath = $"./dist/{projectName}";
    var pluginsPath = $"{outputPath}/plugins";

    DotNetPublish(projectPath, new DotNetPublishSettings
    {
        Configuration = configuration,
        OutputDirectory = pluginsPath
    });
    Information($"Built and copied .dll files");
    
    var assetsSource = MakeAbsolute(new DirectoryPath($"{projectPath}/Assets"));
    var assetsTarget = MakeAbsolute(new DirectoryPath($"{outputPath}/"));    
    var assetSourceFiles = GetFiles($"{assetsSource}/**/*");

    Information(assetsSource);
    Information(assetsTarget);
        
    foreach (FilePath file in assetSourceFiles) 
    {
        var targetFilePath = new FilePath(file.ToString().Replace(assetsSource.ToString(), assetsTarget.ToString()));
        CreateDirectory(targetFilePath.GetDirectory()); // Ensure directory exists
        CopyFile(file, targetFilePath);
    }
    
    Information($"Done packing the mod");
});

Task("CopyModToRW")
    .IsDependentOn("PackMod")
    .Does(() =>
{
    rainWorldPath = rainWorldPath?.Trim();
        
    if (string.IsNullOrEmpty(rainWorldPath)) 
    {
        rainWorldPath = EnvironmentVariable("RAINWORLD_PATH");
        rainWorldPath = rainWorldPath?.Trim();
    }
    
    if (string.IsNullOrEmpty(rainWorldPath)) 
    {
        rainWorldPath = ReadEnvFile("RAINWORLD_PATH");
        rainWorldPath = rainWorldPath?.Trim();        
    }
    
    if (string.IsNullOrEmpty(rainWorldPath)) 
        throw new Exception("Rain World installation path is required. Specify it with --rainWorldPath argument, "
                          + "RAINWORLD_PATH environment variable or with .env or .env.local file.");

    var rainWorldModsPath = $"{rainWorldPath}/RainWorld_Data/StreamingAssets/mods";
    var modPath = $"{rainWorldModsPath}/{projectName}";

    if (!DirectoryExists(modPath))
    {
        CreateDirectory(modPath);
    }
    else
    {
        CleanDirectory(modPath);
    }

    var outputPath = $"./dist/{projectName}";
    
    CopyDirectory(outputPath, modPath);
    Information($"Copied packed mod to {modPath}");
});

RunTarget(target);

// Helper method to read environment variable from .env files
string ReadEnvFile(string key)
{
    var envFiles = new[] { ".env.local", ".env" };

    foreach (var envFile in envFiles)
    {
        if (FileExists(envFile))
        {
            var lines = System.IO.File.ReadAllLines(envFile);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2 && parts[0].Trim() == key)
                {
                    var value = parts[1].Trim();
                    
                    // Remove surrounding quotes if present
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    return value;
                }
            }
        }
    }

    return null;
}