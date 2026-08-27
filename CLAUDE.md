# CLAUDE.md

Project: `tvardero.RainWorldModsPublic` — BepInEx mods for Rain World.

- **`src/tvardero.DearDevTools`** — the actual mod. Adds an alternative DevUI (Rain World's in-game debug UI,
  see `DevInterface.*` in the base game) implemented with Dear ImGui, via the `ImGuiNET` C# bindings. Note:
  `ImGuiNET`/`ImGui.NET` here comes from the `imgui` project's own C# binding release, not the NuGet package —
  don't "fix" the reference to pull from NuGet.
- **`src/tvardero.MenuTests`** — not a mod, not shipped. A scratch harness for iterating on menus being built for
  DearDevTools without going through a full Rain World launch each time.

## Build & deploy (build.cake)

Run via the local tool `cake.tool` (see `dotnet-tools.json`, same manifest as `ilspycmd`):

```bash
dotnet tool restore                 # first time / after clone
dotnet cake --version               # `dotnet` resolves local-tool manifest commands directly, so `dotnet cake` works
```

Default target `CopyModToRW` chains `Clean` → `PackMod` → `CopyModToRW`. Run with e.g.
`dotnet cake --target=CopyModToRW --rainWorldPath=/path/to/Rain\ World` (or rely on env/`.env.local`, see below).
`--project` selects which project under `src/` to build (defaults to `tvardero.DearDevTools`); `--configuration`
defaults to `Debug`, not `Release` — that's the normal dev-loop build, not a release build.

- **`Clean`** — `dotnet clean`s the project, wipes `./dist/{projectName}`.
- **`PackMod`** — stages a ready-to-drop mod folder at `./dist/{projectName}`:
  - `dotnet publish` output goes to `dist/{projectName}/plugins/` (the mod DLL + its NuGet deps, `.pdb`s included —
    nothing strips symbols before deploy).
  - Everything under `src/{projectName}/Assets/**/*` (currently `modinfo.json`, `thumbnail.png`) is copied to
    `dist/{projectName}/` root, alongside `plugins/`, preserving relative structure.
  - End shape: `dist/{projectName}/{modinfo.json, thumbnail.png, plugins/*.dll}` — this is Rain World's expected
    mod-folder layout, same shape as a Steam Workshop item.
- **`CopyModToRW`** — mirrors that staged folder into a real local Rain World install:
  - Resolves `rainWorldPath` in order: `--rainWorldPath` arg → `RAINWORLD_PATH` env var → `RAINWORLD_PATH` key in
    `.env.local` then `.env` (repo root; `.env.local` is the real one in use, mode 600, don't read/cat it — treat
    as a secret-ish local config even though it's just a filesystem path).
  - Deploys to `{rainWorldPath}/RainWorld_Data/StreamingAssets/mods/{projectName}` — Rain World's standard
    mod-loading dir (local equivalent of the Steam Workshop content path). If that folder exists it's *cleaned*
    (contents wiped) before the fresh copy, not deleted/recreated.

## Decompiling game/mod assemblies

This comes up often — inspecting base-game or mod internals to understand how something works before
hooking/patching it.

**Tool**: `ilspycmd` (ICSharpCode.Decompiler), installed as a local dotnet tool in this repo's manifest
(`dotnet-tools.json`, root — note: not the usual `.config/dotnet-tools.json` path).

```bash
dotnet tool restore                 # first time / after clone, installs ilspycmd per the manifest
dotnet ilspycmd --version           # invoke via `dotnet ilspycmd` or `dotnet tool run ilspycmd`
```

If the manifest/restore isn't available for some reason, fall back to a global install:
`dotnet tool install -g ilspycmd`, then it's on PATH at `~/.dotnet/tools`.

**Common commands:**
```bash
dotnet ilspycmd --list c <dll>              # list all types in an assembly
dotnet ilspycmd -t <TypeName> <dll>         # decompile a single type to C#
dotnet ilspycmd -o <outdir> <dll>           # dump the whole assembly to a browsable project tree
```

### Where to find assemblies

- **Base game** (`Assembly-CSharp.dll` etc.): `~/.local/share/Steam/steamapps/common/Rain World/RainWorld_Data/Managed/`
  - Also `BepInEx/utils/PUBLIC-Assembly-CSharp.dll` / `HOOKS-Assembly-CSharp.dll` under the Rain World install dir —
    these are BepInEx's split public-API vs. hook-patched variants.
  - This repo's own `thirdparty/` folder has copies of these for build references — don't treat those as
    "discovered" fresh; if the user asks for something not sourced from the repo, read from the Steam paths above.
- **Steam Workshop mods** (already subscribed): `~/.local/share/Steam/steamapps/workshop/content/312520/<published_file_id>/`
  — `312520` is Rain World's Steam AppID. Each subfolder is one workshop item; look for a `plugins/*.dll` inside.
  - Not sure which folder is which mod? Check `<id>/modinfo.json` first — every mod has one at its content root with
    `id`/`name`/`authors`/`description`/`tags`, e.g.:
    ```bash
    for f in ~/.local/share/Steam/steamapps/workshop/content/312520/*/modinfo.json; do
      jq -r '.name' "$f"; done
    ```
    Much faster than decompiling+grepping every DLL to find the right one. Only fall back to grepping/decompiling
    DLL contents (distinctive namespace, plugin GUID) if `modinfo.json` is missing or ambiguous, or to check against
    the Workshop page (fetch `https://steamcommunity.com/sharedfiles/filedetails/?id=<id>`) for anything modinfo
    doesn't cover.
  - To get a mod NOT already subscribed: downloading needs `steamcmd` or the Steam client itself (Workshop content
    isn't fetchable over plain HTTP) — this requires an install I don't have standing permission for. Ask the user
    to subscribe via their Steam client (lands in the path above automatically), or ask for explicit go-ahead to
    install `steamcmd` for anonymous `workshop_download_item` pulls.

### Notes from past sessions

- `RoomSettings.pal` (nullable int, inherited from parent/template when unset) is the room-palette index; DevUI's
  `DevInterface.PaletteController` mutates it directly and calls `RoomCamera.ChangeMainPalette`. Persisted as a
  plain `Palette: N` line in the room's `.txt` settings file.
- `RoomCamera.LoadPalette` resolves palette assets as `palettes/palette{N}.png` (32×16 lookup-table PNGs) via
  `AssetManager.ResolveFilePath`, falling back to `palette-1.png` if the index is missing.
- The "Rain World ImGUI API" workshop mod (BepInEx GUID `rwimgui`) lives in namespace `RWIMGUI.*`; entry point is
  `RWIMGUI.Plugin`, hooks `RainWorld.OnModsInit`/`RainWorld.Start` via MonoMod `On.` hooks, renders via a
  DirectX11/Win32 interop layer (`RWIMGUI.Windows.DirectX.*`).
