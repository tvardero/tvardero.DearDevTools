using System.Text.RegularExpressions;
using tvardero.DearDevTools.Models;

namespace tvardero.DearDevTools.Services;

public class PaletteService
{
    private readonly GameStateService _gameStateService;

    public PaletteService(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
    }

    private static readonly Regex _paletteFilterRegex = new(@"^palette(-?\d{1,10})\.png$");

    public PaletteModel[] AllPalettes { get; private set; } = [];

    public PaletteModel[] Scan()
    {
        PaletteModel[] palettes = AssetManager.ListDirectory("palettes")
            .Select(fileName => _paletteFilterRegex.Match(fileName))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .Select(int.Parse)
            .Select(id => new PaletteModel(id))
            .ToArray();

        AllPalettes = palettes;
        return palettes;
    }

    public void ApplyPaletteToRoom(Room room, int? palette, int? effectA, int? effectB, int? fadePalette, float[] fadeRates)
    {
        if (palette is < 0) throw new ArgumentOutOfRangeException(nameof(palette), "Palette should be a positive number or null");
        if (effectA is < 0 or > 21) throw new ArgumentOutOfRangeException(nameof(effectA), "Effect A should be a number between 0 and 21 or null");
        if (effectB is < 0 or > 21) throw new ArgumentOutOfRangeException(nameof(effectA), "Effect B should be a number between 0 and 21 or null");

        RoomSettings settings = room.roomSettings;

        settings.pal = palette;
        settings.eColA = effectA;
        settings.eColB = effectB;

        if (fadePalette == null) { settings.fadePalette = null; }
        else if (settings.fadePalette == null)
        {
            settings.fadePalette = new RoomSettings.FadePalette(fadePalette.Value, room.cameraPositions.Length)
            {
                fades = fadeRates,
            };
        }
        else
        {
            settings.fadePalette.palette = fadePalette.Value;
            settings.fadePalette.fades = fadeRates;
        }

        if (_gameStateService.RainWorldGame == null) return;

        foreach (RoomCamera roomCamera in _gameStateService.RainWorldGame.cameras)
        {
            if (roomCamera.room != room) continue;

            roomCamera.ApplyPalette();
            roomCamera.ApplyFade();
        }
    }
}