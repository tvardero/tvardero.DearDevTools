using BepInEx;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RWCustom;
using tvardero.DearDevTools.Internal;
using tvardero.DearDevTools.Logging;
using tvardero.DearDevTools.Services;
using tvardero.DearDevTools.Util;
using tvardero.DearDevTools.Views;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace tvardero.DearDevTools;

/// <summary>
/// Dear Dev Tools mod.
/// </summary>
[BepInPlugin("tvardero.DearDevTools", "Dear Dev Tools", "0.0.7")]
[BepInDependency("rwimgui")]
[PublicAPI]
public sealed class DearDevToolsPlugin : BaseUnityPlugin, IDisposable
{
    private static DearDevToolsPlugin? _instance;
    private static bool _skipOnModsInit;
    private static readonly List<Action<IServiceCollection>> _configureServiceCollection = [];
    private static readonly List<Action<IServiceProvider>> _configureServiceProvider = [];
    private readonly Eventer _updateEvent = new();
    private EndEscaperService _endEscaperService = null!;
    private MenuManager _menuManager = null!;
    private ModImGuiContext _modImGuiContext = null!;

    private ServiceProvider _serviceProvider = null!;

    public DearDevToolsPlugin()
    {
        Logger = new BepInExLogger(() => LogLevel.Trace, base.Logger);
    }

    /// <summary>
    /// Singleton instance of fully initialized Dear Dev Tools mod.
    /// </summary>
    /// <exception cref="InvalidOperationException"> Dear Dev Tools mod is not initialized. </exception>
    public static DearDevToolsPlugin Instance => _instance ?? throw new InvalidOperationException("Dear Dev Tools mod is not initialized");

    /// <summary>
    /// Is Dear Dev Tools mod initialized.
    /// </summary>
    public static bool IsInitialized => _instance != null;

    public new ILogger Logger { get; }

    /// <summary>
    /// Main UI visible. Includes main menu bar, many menus and tools like room info panel, room settings panel and others.
    /// Pinned menus and tools will remain to be visible while <see cref="AreDearDevToolsActive" /> is true.<br />
    /// Mouse cursor will be visible as well when <see cref="IsMainUiVisible" /> is true and hidden when false.
    /// </summary>
    /// <remarks>
    /// Setting this to true will automatically set <see cref="AreDearDevToolsActive" /> to true as well.
    /// </remarks>
    public bool IsMainUiVisible
    {
        get => field && AreDearDevToolsActive;

        set
        {
            if (value == field) return;

            field = value;

            if (value)
            {
                AreDearDevToolsActive = true;
                _modImGuiContext.Activate();
            }

            ShowMouseCursor(value);
        }
    }

    /// <summary>
    /// Quick tools enabled. Includes many utils like 'reset rain timer', 'teleport player', 'kill all creatures' and others.
    /// </summary>
    /// <remarks>
    /// Setting this to false will automatically set <see cref="IsMainUiVisible" /> to false.
    /// </remarks>
    public bool AreDearDevToolsActive
    {
        get => field || IsMainUiVisible;

        set
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

    /// <summary>
    /// Service provider used to resolve services and instances of menus.
    /// </summary>
    /// <remarks>
    /// Do not hold on value of this property during mod initialization.
    /// Service provider might be rebuilt multiple times by other dependent mods by call to <see cref="RebuildServiceProvider" />.<br />
    /// Use <see cref="ConfigureServiceProvider" /> as a callback that is executed each time <see cref="RebuildServiceProvider" /> is called.
    /// </remarks>
    public IServiceProvider ServiceProvider => _serviceProvider;

    [UsedImplicitly]
    private void Update()
    {
        if (_instance != this) return;

        try { _updateEvent.Fire(); }
        catch (Exception e) { Logger.LogWarning(e, "Some update handler failed"); }

        // todo: make shortcuts configurable
        bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool altJustPressed = Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
        bool escPressed = Input.GetKey(KeyCode.Escape);
        bool endPressed = Input.GetKey(KeyCode.End);
        bool hJustPressed = Input.GetKeyDown(KeyCode.H);
        bool oJustPressed = Input.GetKeyDown(KeyCode.O);

        bool switchedCursorVisibility = false;

        if (escPressed && endPressed) _endEscaperService.EscapeTheEnd();

        if (ctrlPressed && oJustPressed)
        {
            AreDearDevToolsActive = !AreDearDevToolsActive;
            if (!AreDearDevToolsActive) switchedCursorVisibility = true;

            Logger.LogDebug("Dear Dev Tools active: {AreDearDevToolsActive}", AreDearDevToolsActive);
        }

        if (AreDearDevToolsActive && ctrlPressed && hJustPressed)
        {
            IsMainUiVisible = !IsMainUiVisible;
            switchedCursorVisibility = true;

            Logger.LogDebug("Dear Dev Tools main UI visible: {IsMainUiVisible}", IsMainUiVisible);
        }

        if (!switchedCursorVisibility && AreDearDevToolsActive && altJustPressed) ShowMouseCursor(!Cursor.visible);
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        Logger.LogInformation("OnEnable called, registering initialization callback");

        if (_skipOnModsInit) Initialize();
        else On.RainWorld.OnModsInit += OnModsInit;
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        Logger.LogInformation("OnDisable called, deinitializing mod instance");

        if (_instance == this) _instance = null;

        On.RainWorld.OnModsInit -= OnModsInit;

        _modImGuiContext.Dispose();
    }

    /// <summary>
    /// Disposes (destroys) current mod instance.
    /// </summary>
    public void Dispose()
    {
        Logger.LogInformation("Dispose called, deinitializing mod instance");

        if (_instance == this) _instance = null;

        On.RainWorld.OnModsInit -= OnModsInit;

        _serviceProvider.Dispose();
    }

    /// <summary>
    /// Subscribes a handler to run every frame.
    /// </summary>
    /// <param name="handler"> Handler. </param>
    /// <returns> Subscription token, which on dispose unsubscribes the handler. </returns>
    public IDisposable RegisterOnUpdate(Action handler)
    {
        return _updateEvent.Register(handler);
    }

    /// <summary>
    /// Shows mouse cursor.
    /// </summary>
    /// <remarks>
    /// Cursor is shown automatically when <see cref="IsMainUiVisible" /> is true, and hidden automatically when false.
    /// </remarks>
    /// <param name="show"> Show or hide? </param>
    public static void ShowMouseCursor(bool show = true)
    {
        Cursor.visible = show;
    }

    /// <summary>
    /// Hides mouse cursor.
    /// </summary>
    /// <remarks>
    /// Cursor is shown automatically when <see cref="IsMainUiVisible" /> is true, and hidden automatically when false.
    /// </remarks>
    public static void HideMouseCursor()
    {
        Cursor.visible = false;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        Initialize();
        _skipOnModsInit = true;
    }

    /// <summary>
    /// Register or override services for service provider.<br />
    /// After registering all services, call <see cref="RebuildServiceProvider" /> to rebuild service provider.
    /// </summary>
    /// <remarks>
    /// Dear Dev Tools does not use Scoped lifetime, it uses only Singleton and Transient.
    /// If you want to use Scoped lifetime - go ahead, but note that you need to handle scopes yourself.
    /// </remarks>
    /// <param name="configure"> Configure action. </param>
    /// <exception cref="ArgumentNullException"> Configure action is null. </exception>
    public static void ConfigureServiceCollection(Action<IServiceCollection> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        if (!_configureServiceCollection.Contains(configure)) _configureServiceCollection.Add(configure);
    }

    /// <summary>
    /// Register callback to resolve services from rebuilt service provider and additionally configure them.<br />
    /// Called everytime service provider is rebuilt by <see cref="RebuildServiceProvider" /> method.
    /// </summary>
    /// <param name="configure"> Configure action. </param>
    /// <exception cref="ArgumentNullException"> Configure action is null. </exception>
    public static void ConfigureServiceProvider(Action<IServiceProvider> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        if (!_configureServiceProvider.Contains(configure)) _configureServiceProvider.Add(configure);
    }

    /// <summary>
    /// Rebuilds service provider. Resets the Dear Dev Tools mod.
    /// </summary>
    public void RebuildServiceProvider()
    {
        Logger.LogInformation("Rebuilding service provider");

        // NOTE: multiple downstream dependents might call this method multiple times 

        var serviceCollection = new ServiceCollection();

        foreach (Action<IServiceCollection> configure in _configureServiceCollection)
        {
            try { configure(serviceCollection); }
            catch (Exception e) { Logger.LogError(e, "Error while executing service collection configure action (pre-build)"); }
        }

        ConfigureDefaults(serviceCollection);

        ServiceProvider serviceProvider;
        try
        {
            serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while building service provider, did you register all necessary services?");
            throw;
        }

        foreach (Action<IServiceProvider> configure in _configureServiceProvider)
        {
            try { configure(serviceProvider); }
            catch (Exception e) { Logger.LogError(e, "Error while executing service provider configure action (post-build)"); }
        }

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _serviceProvider?.Dispose();
        _serviceProvider = serviceProvider;

        _modImGuiContext = _serviceProvider.GetRequiredService<ModImGuiContext>();
        _menuManager = _serviceProvider.GetRequiredService<MenuManager>();
        _endEscaperService = _serviceProvider.GetRequiredService<EndEscaperService>();

        _menuManager.CreateNew<DearDevToolsEnabledOverlay>();
        _menuManager.CreateNew<MainMenuBar>();

#if DEBUG
        AreDearDevToolsActive = true;
        IsMainUiVisible = true;
#else
        AreDearDevToolsActive = false;
        IsMainUiVisible = false;
#endif
    }

    private void ConfigureDefaults(ServiceCollection serviceCollection)
    {
#if DEBUG
        var minimumLogLevel = LogLevel.Trace;
#else
        var minimumLogLevel = LogLevel.Information;
#endif

        // no override allowed:
        serviceCollection.AddLogging(c => c.AddProvider(new BepInExLoggingProvider(minimumLogLevel)));
        serviceCollection.AddSingleton(this);
        serviceCollection.AddSingleton<ModImGuiContext>();
        serviceCollection.AddSingleton<MenuManager>();
        serviceCollection.AddSingleton(Custom.rainWorld);
        serviceCollection.AddSingleton<EndEscaperService>();

        // override allowed:
        serviceCollection.TryAddSingleton<DearDevToolsEnabledOverlay>();
        serviceCollection.TryAddSingleton<MainMenuBar>();
        serviceCollection.TryAddSingleton<HelpMenu>();
        serviceCollection.TryAddSingleton<WhatsNewMenu>();
        serviceCollection.TryAddSingleton<PaletteService>();
        serviceCollection.TryAddSingleton<PaletteEditorMenu>();
        serviceCollection.TryAddSingleton<GameStateService>();
    }

    private void Initialize()
    {
        Logger.LogInformation("Initializing mod instance");

        if (_instance == this) return;

        try { RebuildServiceProvider(); }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Fatal error during Dear Dev Tool initialization");
            throw;
        }

        _instance = this;
        Logger.LogInformation("Initialization complete");
    }
}