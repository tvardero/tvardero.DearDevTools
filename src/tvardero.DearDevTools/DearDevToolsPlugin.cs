using BepInEx;
using JetBrains.Annotations;
using tvardero.DearDevTools.Menus;
using UnityEngine;

namespace tvardero.DearDevTools;

/// <summary>
/// Dear Dev Tools mod.
/// </summary>
[BepInPlugin("tvardero.DearDevTools", "Dear Dev Tools", "0.0.6")]
[BepInDependency("rwimgui")]
public sealed class DearDevToolsPlugin : BaseUnityPlugin, IDisposable
{
    private static DearDevToolsPlugin? _instance;
    private static bool _skipOnModsInit = true;

    private ModImGuiContext _modImGuiContext = null!;

    /// <summary>
    /// Singleton instance of fully initialized Dear Dev Tools mod.
    /// </summary>
    /// <exception cref="InvalidOperationException"> Dear Dev Tools mod is not initialized. </exception>
    public static DearDevToolsPlugin Instance => _instance ?? throw new InvalidOperationException("Dear Dev Tools mod is not initialized");

    /// <summary>
    /// Is Dear Dev Tools mod initialized.
    /// </summary>
    public static bool IsInitialized => _instance != null;

    /// <summary> Main UI visible. Includes main menu bar, room info panel, room settings panel, and others by default. </summary>
    /// <remarks> Can't be enabled while <see cref="AreDearDevToolsActive" /> is false. </remarks>
    public bool IsMainUiVisible
    {
        get => field && AreDearDevToolsActive;

        private set
        {
            if (value == field) return;

            if (value) AreDearDevToolsActive = true;
            field = value;

            if (value) _modImGuiContext.Activate();
        }
    }

    /// <summary>
    /// Quick tools enabled. Includes many utils like 'reset rain timer', 'teleport player', 'kill all creatures' and others by default.
    /// </summary>
    /// <remarks> Can't be disabled while <see cref="IsMainUiVisible" /> is true. </remarks>
    public bool AreDearDevToolsActive
    {
        get => field || IsMainUiVisible;

        private set
        {
            if (value == field) return;

            if (!value)
            {
                _modImGuiContext.Deactivate();
                IsMainUiVisible = false;
            }

            field = value;

            if (value) _modImGuiContext.Activate();
        }
    }

    [UsedImplicitly]
    private void Update()
    {
        if (_instance != this) return;

        // todo: make configurable

        bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool hPressed = Input.GetKeyDown(KeyCode.H);
        bool oPressed = Input.GetKeyDown(KeyCode.O);

        if (ctrlPressed && oPressed)
        {
            bool willBeActivated = !AreDearDevToolsActive;
            Logger.LogInfo(willBeActivated ? "Activating Dear Dev Tools" : "Deactivating Dear Dev Tools");
            AreDearDevToolsActive = willBeActivated;
        }

        if (ctrlPressed && hPressed)
        {
            bool willBeVisible = !IsMainUiVisible;
            Logger.LogInfo(willBeVisible ? "Showing main UI" : "Hiding main UI");
            IsMainUiVisible = willBeVisible;
        }
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        Logger.LogInfo("OnEnable called, registering initialization callback");

        if (_skipOnModsInit) Initialize();
        else On.RainWorld.OnModsInit += OnModsInit;
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        Logger.LogInfo("OnDisable called, deinitializing mod instance");

        if (_instance == this) _instance = null;

        On.RainWorld.OnModsInit -= OnModsInit;

        _modImGuiContext.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Logger.LogInfo("Dispose called, deinitializing mod instance");

        if (_instance == this) _instance = null;

        On.RainWorld.OnModsInit -= OnModsInit;

        _modImGuiContext.Dispose();
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        Initialize();
        _skipOnModsInit = true;
    }

    private void Initialize()
    {
        Logger.LogInfo("Initializing mod instance");

        if (_instance == this) return;

        _modImGuiContext = new ModImGuiContext(this);
        _modImGuiContext.RenderList.Add(new DearDevToolsEnabledTooltip());
        _modImGuiContext.RenderList.Add(new MainMenuBar());

        _instance = this;
    }
}