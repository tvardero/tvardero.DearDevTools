using Microsoft.Extensions.Logging;
using RWCustom;
using tvardero.DearDevTools.Components;
using tvardero.DearDevTools.Services;

namespace tvardero.DearDevTools.Views;

public class PaletteEditorMenu : ImGuiWindowBase
{
    private readonly PaletteService _paletteService;
    private readonly GameStateService _gameStateService;
    private readonly ILogger _logger;

    public PaletteEditorMenu(
        PaletteService paletteService,
        GameStateService gameStateService,
        ILogger<PaletteEditorMenu> logger) : base("Palette editor")
    {
        _paletteService = paletteService;
        _gameStateService = gameStateService;
        _logger = logger;
    }

    /// <inheritdoc />
    public override bool IsBlockingWMEvent => false;

    /// <inheritdoc />
    protected override void OnDrawWindowContent()
    {
        try
        {
            RainWorld? rw = Custom.rainWorld;
            var game = rw.processManager.currentMainLoop as RainWorldGame;
            if (game == null) return;

            Room? room = game.cameras[0].room;
            RoomSettings? roomSettings = room?.roomSettings;

            if (roomSettings == null) return;

            ImGui.Text($"Palette: {roomSettings.pal}");
            ImGui.Text($"Effect A: {roomSettings.eColA}");
            ImGui.Text($"Effect B: {roomSettings.eColB}");

            ImGui.Text($"Fade: {roomSettings.fadePalette?.palette}");
            if (roomSettings.fadePalette != null)
            {
                for (int i = 0; i < roomSettings.fadePalette.fades.Length; i++)
                {
                    float fadeRate = roomSettings.fadePalette.fades[i];
                    ImGui.Text($"Screen {i}: {fadeRate}");
                }
            }
        }
        catch (Exception e) { _logger.LogError(e, "Error drawing PaletteEditor"); }
    }
}