using BepInEx;
using tvardero.DearDevTools.Internal;

namespace tvardero.DearDevTools;

/// <summary>
/// Dear Dev Tools mod.
/// </summary>
[BepInPlugin("tvardero.DearDevTools", "Dear Dev Tools", "0.0.4")]
[BepInDependency("rwimgui")]
public sealed class DearDevToolsPlugin : BaseUnityPlugin
{
    private static DearDevToolsPlugin? _instance;

    /// <summary>
    /// Singleton instance of fully initialized Dear Dev Tools mod.
    /// </summary>
    /// <exception cref="InvalidOperationException"> Dear Dev Tools mod is not initialized. </exception>
    public static DearDevToolsPlugin Instance => _instance ?? throw new InvalidOperationException("Dear Dev Tools mod is not initialized");

    /// <summary>
    /// Is Dear Dev Tools mod initialized.
    /// </summary>
    public static bool IsInitialized => _instance != null;

    internal ModContext ModContext { get; private set; } = null!;

    /// <summary>
    /// Enable quick tools.
    /// </summary>
    public void EnableQuickTools()
    {
        ModContext.QuickToolsEnabled = true;
    }

    /// <summary>
    /// Disable quick tools.
    /// </summary>
    public void DisableQuickTools()
    {
        ModContext.QuickToolsEnabled = false;
    }

    /// <summary>
    /// Toggle quick tools.
    /// </summary>
    /// <param name="state"> Optional parameter to directly specify the state. </param>
    public void ToggleQuickTools(bool? state = null)
    {
        ModContext.QuickToolsEnabled = state ?? !ModContext.QuickToolsEnabled;
    }

    /// <summary>
    /// Show Dear Dev Tools main UI.
    /// </summary>
    public void ShowMainUi()
    {
        ModContext.MainUiVisible = true;
    }

    /// <summary>
    /// Hide Dear Dev Tools main UI.
    /// </summary>
    public void HideMainUi()
    {
        ModContext.MainUiVisible = false;
    }

    /// <summary>
    /// Toggle Dear Dev Tools main UI.
    /// </summary>
    /// <param name="state"> Optional parameter to directly specify the state. </param>
    public void ToggleMainUi(bool? state = null)
    {
        ModContext.MainUiVisible = state ?? !ModContext.MainUiVisible;
    }

    private void OnEnable()
    {
        On.RainWorld.OnModsInit += OnModsInit;
    }

    private void OnDisable()
    {
        On.RainWorld.OnModsInit -= OnModsInit;

        if (_instance != this) return;

        _instance = null;
        ModContext.Dispose();
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_instance == this) return;

        ModContext = ModContext.Create();
        _instance = this;
    }
}
