# tvardero.DearDevTools

## FAQ:

### For Linux / macOS players:

> I'm playing on macOS / Linux and not a single mod loaded and no mods work!

Proton configures Rain World to run perfect, however misses one configuration to support modding.
Rain World uses BepInEx library to load mods, and BepInEx requires `winhttp` library to be loaded.
But fresh Rain World installation does not have this library selected in wineconfig.
See these instructions on how to add it: https://docs.bepinex.dev/articles/advanced/proton_wine.html.

For ease of modifying wineconfig, I recommend installing `protontricks` (with dependencies) first.
After adding `winhttp` library, be sure to **UNLOAD ALL** mods in Remix menu, restart, and then load them back again.

---

> I'm playing on macOS / Linux and have a black immovable screen in front / behind the game.

Ensure you are running the game with Proton version 9.0, not Experimental, not Hotfix. Changing Proton version might
reset your wineconfig, re-add `winhttp` library if needed (see previous question).

## Build

Prerequsites:

- [.NET SDK version 10.0](https://dotnet.microsoft.com/en-us/download) (not .NET Framework!) or newer installed
- [Knowledge of modern C# (version 13+)](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new)
- [Knowledge of nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Knowledge of RainWorld modding (video not by me)](https://www.youtube.com/watch?v=JG9cyL5FW90)
- [Rain World](https://store.steampowered.com/app/312520/Rain_World) installed
- [ImGUI API mod by Rawra](https://steamcommunity.com/sharedfiles/filedetails/?id=3417372413) installed to Rain World,
  with dependencies

Steps:

1. Go to the RainWorld installation folder (`Steam/steamapps/common/Rain World`).
2. Copy following `.dll` files from RainWorld folder to `thirdparty/` folder in repository:
    - From `Rain World/BepInEx/core/`: `BepInEx.dll`
    - From `Rain World/BepInEx/plugins/`: `HOOKS-Assembly-CSharp.dll`
    - From `Rain World/BepInEx/utils/`: `PUBLIC-Assembly-CSharp.dll`
    - From `Rain World/RainWorld_Data/Managed/`: `UnityEngine.dll`, `UnityEngine.CoreModule.dll`,
      `UnityEngine.InputLegacyModule.dll`
3. Run `dotnet tool restore` once after you have cloned the repository.
4. Create `.env.local` file in the root of the repository, see `.env.local.example` as an example.
5. Run one of the available commands:
    - `dotnet build` - builds the solution.
    - `dotnet build src/MyProject` - builds the project "MyProject";
    - `dotnet cake --target Clean --project MyProject` - clears the `dist/MyProject` folder in repository and runs
      `dotnet clean src/MyProject`.
    - `dotnet cake --target PackMod --project MyProject --configuration Debug` - runs target "Clean", builds the project
      at `src/MyProject` in `Debug` configuration and packs the mod into `dist/MyProject` folder in repository.
    - `dotnet cake --target CopyModToRW  --project MyProject --configuration Debug` - runs target "PackMod" and copies
      the mod into Rain World mods folder. Requires `RAINWORLD_PATH` environment variable to be set, see
      `.env.local.example` for details.
    - Default value for `--configuration` is `Release`;
    - Default value for `--project` is `tvardero.DearDevTools`;
    - Default value for `--target` is `CopyModToRW`.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

For imported (inside `thirdparty/` folder) third-party components - see
the [THIRD-PARTY-LICENSES](THIRD-PARTY-LICENSES.md) file for details.

## Support development

https://ko-fi.com/tvardero