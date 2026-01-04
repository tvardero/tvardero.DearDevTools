using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using tvardero.DearDevTools.Models;

namespace tvardero.DearDevTools.Services;

/// <summary>
/// Service to work with palettes - create, edit, delete, apply to room, etc.
/// </summary>
public class PaletteService
{
    private static readonly Regex _paletteFilterRegex = new(@"^palette(-?\d{1,10})\.png$");
    private readonly GameStateService _gameStateService;
    private readonly ILogger _logger;
    private PaletteModel[]? _palettesCache;

    public PaletteService(GameStateService gameStateService, ILogger<PaletteService> logger)
    {
        _gameStateService = gameStateService;
        _logger = logger;
    }

    /// <summary>
    /// Change room palette.
    /// </summary>
    /// <param name="room"> Room. </param>
    /// <param name="palette"> Selected palette. Should be null or an integer no less than 0. </param>
    /// <param name="effectA"> Selected effect A. Should be null or an integer between 0 (incl.) and 21 (incl.). </param>
    /// <param name="effectB"> Selected effect B. Should be null or an integer between 0 (incl.) and 21 (incl.). </param>
    /// <param name="fadePalette"> Selected fade palette. Should be null or an integer no less than 0. </param>
    /// <param name="fadeRates"> Array of fade palette rates per room camera. Ignored if <paramref name="fadePalette" /> is null. </param>
    /// <param name="force"> Apply settings to camera even if no changes were made. </param>
    /// <exception cref="ArgumentException"> Some argument is invalid. </exception>
    public void ApplyPaletteToRoom(Room room, int? palette, int? effectA, int? effectB, int? fadePalette, float[]? fadeRates, bool force = false)
    {
        // validate
        if (palette is < 0) throw new ArgumentOutOfRangeException(nameof(palette), "Palette should be a positive number or null.");
        if (effectA is < 0 or > 21) throw new ArgumentOutOfRangeException(nameof(effectA), "Effect A should be a number in range [0, 21] or null.");
        if (effectB is < 0 or > 21) throw new ArgumentOutOfRangeException(nameof(effectA), "Effect B should be a number in range [0, 21] or null.");
        if (fadePalette is < 0) throw new ArgumentOutOfRangeException(nameof(palette), "Fade palette should be a positive number or null.");

        if (fadePalette != null)
        {
            if (fadeRates == null)
            {
                throw new ArgumentNullException(nameof(fadeRates),
                    "If fade palette is selected, then array of fade palette rates should be specified as well.");
            }

            int roomCameras = room.cameraPositions.Length;
            if (fadeRates.Length != roomCameras)
            {
                throw new ArgumentException(
                    $"Fade palette rates array length ({fadeRates.Length}) should be equal to amount of cameras in the room ({roomCameras}).",
                    nameof(fadeRates));
            }
        }

        _logger.LogInformation("Saving new palettes to room settings of '{RoomName}'", room.roomSettings.name);

        RoomSettings roomSettings = room.roomSettings;

        bool palIsNew = roomSettings.pal != palette;
        bool eColIsNew = roomSettings.eColA != effectA || roomSettings.eColB != effectB;
        roomSettings.pal = palette;
        roomSettings.eColA = effectA;
        roomSettings.eColB = effectB;

        bool fadeIsNew;
        if (!fadePalette.HasValue)
        {
            fadeIsNew = roomSettings.fadePalette != null;
            roomSettings.fadePalette = null;
        }
        else if (roomSettings.fadePalette == null)
        {
            fadeIsNew = true;
            roomSettings.fadePalette = new RoomSettings.FadePalette(fadePalette.Value, room.cameraPositions.Length)
            {
                fades = fadeRates,
            };
        }
        else
        {
            fadeIsNew = roomSettings.fadePalette.palette != fadePalette || !Enumerable.SequenceEqual(roomSettings.fadePalette.fades, fadeRates!);
            roomSettings.fadePalette.palette = fadePalette.Value;
            roomSettings.fadePalette.fades = fadeRates;
        }
        
        if (!force && !palIsNew && !eColIsNew && !fadeIsNew) return;
        if (_gameStateService.RainWorldGame == null) return;

        _logger.LogInformation("Applying new palettes to camera");
        
        foreach (RoomCamera roomCamera in _gameStateService.RainWorldGame.cameras)
        {
            if (roomCamera.room != room) continue;

            if (palIsNew) roomCamera.ChangeMainPalette(roomSettings.Palette);
            if (eColIsNew) roomCamera.ApplyEffectColorsToAllPaletteTextures(roomSettings.EffectColorA, roomSettings.EffectColorB);

            if (!fadeIsNew) continue;

            int cameraIdx = roomCamera.currentCameraPosition;
            int fadePaletteToApply = roomSettings.fadePalette?.palette ?? -1;
            float fadeRateToApply = roomSettings.fadePalette == null ? 0f : roomSettings.fadePalette!.fades![cameraIdx];
            roomCamera.ChangeFadePalette(fadePaletteToApply, fadeRateToApply);
        }
    }

    /// <summary>
    /// Get list of all known palettes.
    /// </summary>
    /// <param name="forceRescan"> Disable cache and force rescan of "palettes/" folder. </param>
    /// <returns> </returns>
    public PaletteModel[] GetPalettes(bool forceRescan = false)
    {
        if (!forceRescan && _palettesCache != null) return _palettesCache;

        _logger.LogInformation("Reloading available palettes");

        PaletteModel[] palettes = AssetManager.ListDirectory("palettes")
            .Select(fileName => _paletteFilterRegex.Match(fileName))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .Select(int.Parse)
            .Select(id => new PaletteModel(id))
            .ToArray();

        _palettesCache = palettes;
        return palettes;
    }
}