# tvardero.RainWorldModsPublic

## tvardero.DearDevTools

### Build

Prerequsites:

- [.NET SDK version 9.0](https://dotnet.microsoft.com/en-us/download) (not .NET Framework!) or newer installed
- [Rain World](https://store.steampowered.com/app/312520/Rain_World) installed
- [Watcher DLC for RainWorld](https://store.steampowered.com/app/2857120/Rain_World_The_Watcher/) installed
- [Knowledge of modern C# (version 13+)](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new)
- [Knowledge of nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Knowledge of RainWorld modding (video not by me)](https://www.youtube.com/watch?v=JG9cyL5FW90)
- [ImGUI API mod by Rawra](https://steamcommunity.com/sharedfiles/filedetails/?id=3417372413) installed to Rain World,
  with dependencies

Steps:

1. Go to the RainWorld installation folder (`Steam/steamapps/common/Rain World`).
2. Copy following `.dll` files from RainWorld folder to `thirdparty/` folder in repository, if they don't exist:
    - From `Rain World/BepInEx/core/`: `BepInEx.dll`
    - From `Rain World/BepInEx/plugins/`: `HOOKS-Assembly-CSharp.dll`
    - From `Rain World/BepInEx/utils/`: `PUBLIC-Assembly-CSharp.dll`
    - From `Rain World/RainWorld_Data/Managed/`: `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.InputLegacyModule.dll`
3. Run `dotnet tool restore` once after you have cloned the repository.
4. Create `.env.local` file in the root of the repository if not exist, see `.env.local.example` for details.
5. Run one of the available commands:
    - `dotnet build` - builds the project.
    - `dotnet cake --target=Clean` - clears the `dist/` folder in repository and runs `dotnet clean`.
    - `dotnet cake --target=PackMod` - build the project in `Debug` configuration and packs the mod into `dist/` folder
      in repository.
    - `dotnet cake --target=PackMod --configuration=Release` - same, but in `Release` configuration.
    - `dotnet cake --target=CopyModToRW` - builds the project in `Debug` configuration and copies the mod into Rain
      World mods folder. Requires `RAINWORLD_PATH` environment variable to be set, see `.env.local.example` for details.
    - `dotnet cake --target=CopyModToRW-Release` - same, but in `Release` configuration.
    - `dotnet cake` - same as `dotnet cake --target=CopyModToRW`.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

For imported third-party components - see the [THIRD-PARTY-LICENSES](THIRD-PARTY-LICENSES.md) file for details.

## Support development

https://ko-fi.com/tvardero