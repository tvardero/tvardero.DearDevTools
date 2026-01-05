using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using tvardero.DearDevTools.Components;
using tvardero.DearDevTools.Models;
using tvardero.DearDevTools.Services;

namespace tvardero.DearDevTools.Menus;

public class PaletteEditorMenu : ImGuiWindowBase
{
    private static PaletteModel[] _availablePalettes = [];
    private readonly PaletteService _paletteService;
    private readonly GameStateService _gameStateService;
    private readonly IDisposable _registerOnStateChanged;
    private readonly IDisposable _registerOnRoomChanged;
    private Room? _loadedRoom;
    private int _palette;
    private int _effectA;
    private int _effectB;
    private int _fadePalette;
    private bool _usePaletteFromTemplate;
    private bool _useEffectAFromTemplate;
    private bool _useEffectBFromTemplate;
    private bool _hasFadePalette;
    private bool _hasChanges;
    private float[] _fadeRates = [];

    public PaletteEditorMenu(PaletteService paletteService, GameStateService gameStateService, ILogger<PaletteEditorMenu> logger)
        : base("Palette editor",
            ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize,
            disposeOnClose: true,
            allowMultipleInstances: true,
            logger: logger)
    {
        _paletteService = paletteService;
        if (_availablePalettes.Length == 0) _availablePalettes = paletteService.GetPalettes();

        _gameStateService = gameStateService;
        _registerOnStateChanged = _gameStateService.RegisterOnStateChanged(state =>
        {
            if (!state.IsInGame) Close();
        });

        _registerOnRoomChanged = _gameStateService.RegisterOnRoomChanged(roomChange =>
        {
            if (_loadedRoom == null) return;

            if (roomChange.newRoom != _loadedRoom && !Title.Contains(" (NOT CURRENT)")) Title += " (NOT CURRENT)";
            else if (roomChange.newRoom == _loadedRoom && Title.Contains(" (NOT CURRENT)")) Title = Title.Replace(" (NOT CURRENT)", string.Empty);
        });
    }

    [MemberNotNullWhen(true, nameof(_loadedRoom))]
    private bool HasRoomLoaded => _loadedRoom != null;

    [MemberNotNullWhen(true, nameof(_loadedRoom))]
    private bool RoomHasTemplate => _loadedRoom is { roomSettings.parent.isAncestor: false };

    /// <inheritdoc />
    protected override void OnDispose()
    {
        _registerOnStateChanged.Dispose();
        _registerOnRoomChanged.Dispose();

        base.OnDispose();
    }

    /// <inheritdoc />
    protected override void OnDrawWindowContent()
    {
        if (!_gameStateService.IsInGame) Close();

        ImGui.BeginMenuBar();

        if (ImGui.BeginMenu("Room"))
        {
            if (ImGui.MenuItem("Switch to current room")) LoadCurrentRoom();

            if (!HasRoomLoaded) ImGui.BeginDisabled();
            if (ImGui.MenuItem("Force re-apply to current room")) ApplyToCurrentRoom();
            if (!HasRoomLoaded) ImGui.EndDisabled();

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Palettes"))
        {
            if (ImGui.MenuItem("Reload available palettes"))
            {
                _availablePalettes = _paletteService.GetPalettes(true);
                ApplyToCurrentRoom();
            }

            ImGui.EndMenu();
        }

        ImGui.EndMenuBar();

        if (!HasRoomLoaded) ImGui.BeginDisabled();

        bool updates = false;

        updates |= ImGui.InputInt("Palette", ref _palette);
        updates |= ImGui.InputInt("Effect A", ref _effectA);
        updates |= ImGui.InputInt("Effect B", ref _effectB);

        if (!HasRoomLoaded) ImGui.EndDisabled();

        if (updates)
        {
            ApplyToCurrentRoom();
            _hasChanges = true;
            WindowFlags |= ImGuiWindowFlags.UnsavedDocument;
        }

        if (!HasRoomLoaded || !_hasChanges) ImGui.BeginDisabled();

        if (ImGui.Button("Save settings")) { SaveRoomSettings(); }

        if (!HasRoomLoaded || !_hasChanges) ImGui.EndDisabled();
    }

    private void SaveRoomSettings()
    {
        _loadedRoom?.roomSettings.Save();
        _hasChanges = false;
        WindowFlags ^= ImGuiWindowFlags.UnsavedDocument;
    }

    private static int GetNextFrom(int palette)
    {
        if (_availablePalettes.Length == 0) return palette;
        if (_availablePalettes.Length == 1) return _availablePalettes[0].Id;

        PaletteModel next = _availablePalettes.FirstOrDefault(palInfo => palInfo.Id > palette) ?? _availablePalettes[0];
        return next.Id;
    }

    private static int GetPrevFrom(int palette)
    {
        if (_availablePalettes.Length == 0) return palette;
        if (_availablePalettes.Length == 1) return _availablePalettes[0].Id;

        PaletteModel prev = _availablePalettes.LastOrDefault(palInfo => palInfo.Id < palette) ?? _availablePalettes[^1];
        return prev.Id;
    }

    private void ApplyToCurrentRoom()
    {
        if (!HasRoomLoaded) return;

        Logger.LogInformation("Applying palette to current room '{RoomName}'", _loadedRoom.roomSettings.name);

        int? palette = RoomHasTemplate && _usePaletteFromTemplate ? null : _palette;
        int? effectA = RoomHasTemplate && _useEffectAFromTemplate ? null : _effectA;
        int? effectB = RoomHasTemplate && _useEffectBFromTemplate ? null : _effectB;
        int? fadePalette = _hasFadePalette ? _fadePalette : null;
        float[]? fadeRates = _hasFadePalette ? _fadeRates : null;

        try { _paletteService.ApplyPaletteToRoom(_loadedRoom, palette, effectA, effectB, fadePalette, fadeRates); }
        catch (Exception e) { Logger.LogError(e, "Failed to apply settings to room"); }
    }

    private void LoadCurrentRoom()
    {
        _loadedRoom = _gameStateService.CurrentRoom;
        Logger.LogInformation("Loading current room: {RoomName}", _loadedRoom?.roomSettings.name ?? "<NULL>");

        if (_loadedRoom == null)
        {
            Title = "Palette editor";
            _usePaletteFromTemplate = false;
            _useEffectAFromTemplate = false;
            _useEffectBFromTemplate = false;
            _hasFadePalette = false;
            _palette = 0;
            _effectA = 0;
            _effectB = 0;
            _fadePalette = 0;
            _fadeRates = [];
            return;
        }

        RoomSettings settings = _loadedRoom.roomSettings;
        Title = "Palette editor - " + settings.name;

        _usePaletteFromTemplate = RoomHasTemplate && settings.pal == null;
        _useEffectAFromTemplate = RoomHasTemplate && settings.eColA == null;
        _useEffectBFromTemplate = RoomHasTemplate && settings.eColB == null;
        _palette = (_usePaletteFromTemplate ? settings.parent.pal : settings.pal) ?? 0;
        _effectA = (_usePaletteFromTemplate ? settings.parent.eColA : settings.eColA) ?? 0;
        _effectB = (_usePaletteFromTemplate ? settings.parent.eColB : settings.eColB) ?? 0;

        _hasFadePalette = settings.fadePalette != null;
        if (_hasFadePalette)
        {
            _fadePalette = settings.fadePalette!.palette;
            _fadeRates = settings.fadePalette.fades;
        }
        else
        {
            _fadePalette = _palette;
            _fadeRates = new float[_loadedRoom.cameraPositions.Length];
        }
    }
}