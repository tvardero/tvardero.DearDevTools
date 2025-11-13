# ta.RainWorldModsPublic

## ta.ImGuiDevTools

### Build

Prerequsites:

- [.NET SDK version 9.0](https://dotnet.microsoft.com/en-us/download) (not .NET Framework!) or newer installed
- [Rain World](https://store.steampowered.com/app/312520/Rain_World) installed
- [Watcher DLC for RainWorld](https://store.steampowered.com/app/2857120/Rain_World_The_Watcher/) installed
- [Knowledge of modern C# (version 13+)](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new)
- [Knowledge of nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Knowledge of RainWorld modding (video not by me)](https://www.youtube.com/watch?v=JG9cyL5FW90)
- [ImGUI API mod by Rawra](https://steamcommunity.com/sharedfiles/filedetails/?id=3417372413) installed to Rain World, with dependencies

Steps:

1. Go to the RainWorld installation folder (**Steam/steamapps/common/Rain World**).
2. Copy following `.dll` files from RainWorld folder to **thirdparty/** folder in repository:
    - From **Rain World/BepInEx/core/**: `0Harmony.dll`, `BepInEx.dll`, `Mono.Cecil.dll`, `MonoMod.Utils.dll`
    - From **Rain World/BepInEx/plugins/**: `HOOKS-Assembly-CSharp.dll`
    - From **Rain World/BepInEx/utils/**: `PUBLIC-Assembly-CSharp.dll`
    - From **Rain World/RainWorld_Data/Managed/**: `Assembly-CSharp-firstpass.dll`, `GoKit.dll`, `Rewired_Core.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.InputLegacyModule.dll`
3. Go to the Steam workshop folder for Rain World (**Steam/steamapps/workshop/content/312520**). Go to ImGUI API mod folder (which is **3417372413**).
4. Copy following `.dll` files from RainWorld folder to **thirdparty/** folder in repository:
    - From **plugins/**: `ImGui.NET.dll`, `rain-world-imgui-api.dll`
5. Run `dotnet build`

### Packaging mod

Requires that you have copied all `.dll` files from RainWorld installation folder to **thirdparty/** folder in repository. See the [build](#build) section for details.

Steps:

1. Run `dotnet tool restore` (only once you clone the repository)
2. Run `dotnet cake --target Publish-ImGuiDevTools`
3. Mod is at `/dist/ta.ImGuiDevTools`, ready to be zipped

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
