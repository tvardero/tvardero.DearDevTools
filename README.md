# ta.RainWorldModsPublic

## License

Do whatever you want with this code. Just don't republish it without my permission.

## ta.ModMaker

## ta.UiKit

### Build

Prerequsites:

- [.NET SDK version 9.0](https://dotnet.microsoft.com/en-us/download) (not .NET Framework!) or newer installed
- [RainWorld](https://store.steampowered.com/app/312520/Rain_World) installed
- [Watcher DLC for RainWorld](https://store.steampowered.com/app/2857120/Rain_World_The_Watcher/) installed
- [Knowledge of modern C# (version 13+)](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new)
- [Knowledge of nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Knowledge of RainWorld modding (video not by me)](https://www.youtube.com/watch?v=JG9cyL5FW90)

Steps:

1. Go to the RainWorld installation folder (f.e. `C:/Program Files (x86)/Steam/steamapps/common/Rain World`)
2. Copy following `.dll` files from RainWorld folder to `thirdparty/` folder in repository:
    - From `Rain World/BepInEx/core/`: `0Harmony.dll`, `BepInEx.dll`, `Mono.Cecil.dll`, `MonoMod.Utils.dll`
    - From `Rain World/BepInEx/plugins/`: `HOOKS-Assembly-CSharp.dll`
    - From `Rain World/BepInEx/utils/`: `PUBLIC-Assembly-CSharp.dll`
    - From `Rain World/RainWorld_Data/Managed/`: `Assembly-CSharp-firstpass.dll`, `GoKit.dll`, `Rewired_Core.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.InputLegacyModule.dll`
3. Run `dotnet build`

### Packaging mod

Requires that you have copied all `.dll` files from RainWorld installation folder to `thirdparty/` folder in repository. See the [build](#build) section for details.

Prerequsites:

- "Git Bash" or any other terminal with Linux commands installed (f.e. WSL)
- "Make" tool installed and available from Git Bash

Steps:

1. Run Git Bash or any other terminal with Linux commands
2. Run `make publish-uikit`
3. Mod is at `/dist/ta.UiKit`, ready to be zipped
