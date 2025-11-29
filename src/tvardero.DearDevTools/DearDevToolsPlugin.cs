using BepInEx;
using JetBrains.Annotations;
using tvardero.DearDevTools.Components;
using tvardero.DearDevTools.Internal;
using UnityEngine;

namespace tvardero.DearDevTools;

/// <summary>
/// Dear Dev Tools mod.
/// </summary>
[BepInPlugin("tvardero.DearDevTools", "Dear Dev Tools", "0.0.5")]
[BepInDependency("rwimgui")]
public sealed class DearDevToolsPlugin : BaseUnityPlugin, IDisposable
{
    private static DearDevToolsPlugin? _instance;

    private readonly List<Func<DearDevToolsPlugin, ImGuiDrawableBase>> _registeredDrawables = [];
    private ModImGuiContext _imGuiContext = null!;

    /// <summary>
    /// Singleton instance of fully initialized Dear Dev Tools mod.
    /// </summary>
    /// <exception cref="InvalidOperationException"> Dear Dev Tools mod is not initialized. </exception>
    public static DearDevToolsPlugin Instance => _instance ?? throw new InvalidOperationException("Dear Dev Tools mod is not initialized");

    /// <summary>
    /// Is Dear Dev Tools mod initialized.
    /// </summary>
    public static bool IsInitialized => _instance != null;

    /// <summary>
    /// Main UI visible. Includes main menu bar, room info panel, room settings panel, and others by default.
    /// </summary>
    public bool IsMainUiVisible { get; private set; }

    /// <summary>
    /// Quick tools enabled. Includes many utils like 'reset rain timer', 'teleport player', 'kill all creatures' and others by default.
    /// Can not be disabled while <see cref="IsMainUiVisible" /> is true;
    /// </summary>
    public bool AreQuickToolsEnabled
    {
        get => IsMainUiVisible || field;
        private set;
    }

    /// <summary>
    /// List of windows to render.
    /// </summary>
    public IReadOnlyList<ImGuiDrawableBase> RenderList { get; private set; } = null!;

    [UsedImplicitly]
    private void Update()
    {
        if (_instance != this) return;

        // todo: make configurable

        bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool hPressed = Input.GetKeyDown(KeyCode.H);
        bool oPressed = Input.GetKeyDown(KeyCode.O);

        if (ctrlPressed && hPressed) ShowMainUi(!IsMainUiVisible);

        if (ctrlPressed && oPressed) EnableQuickTools(!AreQuickToolsEnabled);
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += OnModsInit;
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        On.RainWorld.OnModsInit -= OnModsInit;

        if (_instance != this) return;

        _instance = null;
        _imGuiContext.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _imGuiContext.Dispose();
    }

    public event Action<bool>? OnMainUiVisibleChange;

    public event Action<bool>? OnQuickToolsEnabledChange;

    /// <summary>
    /// Disable quick tools. Can't be disabled while <see cref="IsMainUiVisible" /> is true.
    /// </summary>
    public void DisableQuickTools()
    {
        AreQuickToolsEnabled = false;
        OnQuickToolsEnabledChange?.Invoke(false);
    }

    /// <summary>
    /// Enable quick tools. Can't be disabled while <see cref="IsMainUiVisible" /> is true.
    /// </summary>
    /// <param name="enable"> Value to set. Default is true. </param>
    public void EnableQuickTools(bool enable = true)
    {
        AreQuickToolsEnabled = enable;
        OnQuickToolsEnabledChange?.Invoke(enable);
    }

    /// <summary>
    /// Hide Dear Dev Tools main UI.
    /// </summary>
    public void HideMainUi()
    {
        IsMainUiVisible = false;
        OnMainUiVisibleChange?.Invoke(false);
    }

    /// <summary>
    /// Show Dear Dev Tools main UI.
    /// </summary>
    /// <param name="show"> Value to set. Default is true. </param>
    public void ShowMainUi(bool show = true)
    {
        IsMainUiVisible = show;
        OnMainUiVisibleChange?.Invoke(show);
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_instance == this) return;

        _imGuiContext = new ModImGuiContext(this);
        RenderList = _imGuiContext.RenderList.AsReadOnly();

        RecreateImGuiDrawablesInContext();

        _instance = this;
    }

    /// <summary>
    /// Register ImGui window or menu to draw with Dear Dev Tools. Call <see cref="RecreateImGuiDrawablesInContext" /> after registering all
    /// drawables.
    /// </summary>
    /// <param name="drawable"> Drawable instance. </param>
    public void RegisterImGuiDrawable(ImGuiDrawableBase drawable)
    {
        RegisterImGuiDrawable(_ => drawable);
    }

    /// <summary>
    /// Register ImGui window or menu to draw with Dear Dev Tools. Call <see cref="RecreateImGuiDrawablesInContext" /> after registering all
    /// drawables.
    /// </summary>
    /// <param name="drawableFactory"> Factory that accepts active Dear Dev Tools plugin instance and returns drawable instance. </param>
    public void RegisterImGuiDrawable(Func<DearDevToolsPlugin, ImGuiDrawableBase> drawableFactory)
    {
        _registeredDrawables.Add(drawableFactory);
    }

    /// <summary>
    /// Recreate Dear Dev Tools drawable list.
    /// </summary>
    public void RecreateImGuiDrawablesInContext()
    {
        IEnumerable<ImGuiDrawableBase?> drawablesToRegister = _registeredDrawables
            .Select(factory =>
            {
                try { return factory(this); }
                catch (Exception e)
                {
                    Logger.LogError("Failed to create ImGui drawable\n" + e.Message);
                    return null;
                }
            })
            .Where(drawable => drawable != null)
            .GroupBy(drawable => drawable!.GetType())
            .Select(group => group.Last());

        _imGuiContext.RenderList.Clear();
        _imGuiContext.RenderList.AddRange(drawablesToRegister!);
    }
}