using System.Diagnostics.CodeAnalysis;
using Menu;
using Microsoft.Extensions.Logging;
using MoreSlugcats;
using tvardero.DearDevTools.Util;
using tvardero.JetBrains.Annotations;

namespace tvardero.DearDevTools.Services;

[MustDisposeResource]
public class GameStateService : IDisposable
{
    private readonly ILogger<GameStateService> _logger;
    private readonly Eventer<GameStateService> _stateChanged = new();
    private readonly Eventer<RoomChanged> _roomChanged = new();
    private readonly IDisposable _updateRegistration;

    public GameStateService(ILogger<GameStateService> logger, RainWorld rainWorld, DearDevToolsPlugin plugin)
    {
        _logger = logger;
        RainWorld = rainWorld;
        _updateRegistration = plugin.RegisterOnUpdate(OnUpdate);
    }

    public bool IsInGame => CurrentProcess is RainWorldGame;

    public bool IsInSleepOrDeathMenu => CurrentProcess is SleepAndDeathScreen or GhostEncounterScreen;

    public bool IsInMainMenu =>
        CurrentProcess is MainMenu or SlugcatSelectMenu or Menu.RegionSelectMenu or MultiplayerMenu or FastTravelScreen or InputOptionsMenu
                       or ModdingMenu or OptionsMenu or BackgroundOptionsMenu or ExpeditionMenu or CollectionsMenu;

    public RainWorld RainWorld { get; private set; }

    public MainLoopProcess? CurrentProcess { get; private set; }

    public RainWorldGame? RainWorldGame => CurrentProcess as RainWorldGame;

    public RoomCamera? CameraZero => RainWorldGame?.cameras[0];

    public Room? CurrentRoom { get; private set; }

    [NotNullIfNotNull(nameof(CurrentRoom))]
    public RoomSettings? CurrentRoomSettings => CurrentRoom?.roomSettings;

    public RoomSettings? CurrentRoomSettingsTemplate => CurrentRoomSettings?.parent is { isAncestor: false } ? CurrentRoomSettings.parent : null;

    /// <inheritdoc />
    public void Dispose()
    {
        _updateRegistration.Dispose();
        GC.SuppressFinalize(this);
    }

    public void OnUpdate()
    {
        MainLoopProcess? newCurrentProcess = RainWorld.processManager.currentMainLoop;

        Room? newRoom = (newCurrentProcess as RainWorldGame)?.cameras[0].room;
        if (newRoom != CurrentRoom)
        {
            Room? oldRoom = CurrentRoom;
            CurrentRoom = newRoom;

            try { _roomChanged.Fire(new RoomChanged(oldRoom, newRoom)); }
            catch (Exception e) { _logger.LogWarning(e, "Some room change handler failed"); }
        }

        if (CurrentProcess != newCurrentProcess)
        {
            CurrentProcess = newCurrentProcess;

            try { _stateChanged.Fire(this); }
            catch (Exception e) { _logger.LogWarning(e, "Some game state change handler failed"); }
        }
    }

    [MustDisposeResource]
    public IDisposable RegisterOnRoomChanged(Action<RoomChanged> handler)
    {
        return _roomChanged.Register(handler);
    }

    [MustDisposeResource]
    public IDisposable RegisterOnStateChanged(Action<GameStateService> handler)
    {
        return _stateChanged.Register(handler);
    }

    public record struct RoomChanged(Room? oldRoom, Room? newRoom);
}