using BepInEx;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RWCustom;
using tvardero.DearDevTools.Logging;
using tvardero.DearDevTools.Menus;
using tvardero.DearDevTools.Services;
using tvardero.DearDevTools.Util;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace tvardero.DearDevTools;

/// <summary>
/// Dear Dev Tools mod.
/// </summary>
[BepInPlugin("tvardero.DearDevTools", "Dear Dev Tools", "0.0.8")]
[BepInDependency("rwimgui")]
[PublicAPI]
public sealed class DearDevToolsPlugin : BaseUnityPlugin, IDisposable
{
    private static DearDevToolsPlugin? _instance;
    private static bool _skipOnModsInit;
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
    /// Pinned menus and tools will remain to be visible while <see cref="IsActivated" /> is true.<br />
    /// Mouse cursor will be visible as well when <see cref="IsMainUiVisible" /> is true and hidden when false.
    /// </summary>
    /// <remarks>
    /// Setting this to true will automatically set <see cref="IsActivated" /> to true as well.
    /// </remarks>
    public bool IsMainUiVisible
    {
        get => field && IsActivated;

        set
        {
            if (value == field) return;

            if (value) IsActivated = true;

            Cursor.visible = value;

            field = value;
            Logger.LogDebug("Dear Dev Tools main UI visible: {IsMainUiVisible}", value);
        }
    }

    /// <summary>
    /// Quick tools enabled. Includes many utils like 'reset rain timer', 'teleport player', 'kill all creatures' and others.
    /// </summary>
    /// <remarks>
    /// Setting this to false will automatically set <see cref="IsMainUiVisible" /> to false.
    /// </remarks>
    public bool IsActivated
    {
        get => field || IsMainUiVisible;

        set
        {
            if (value == field) return;

            if (value) _modImGuiContext.Activate();
            else
            {
                IsMainUiVisible = false;
                _modImGuiContext.Deactivate();
            }

            field = value;
            Logger.LogDebug("Dear Dev Tools active: {AreDearDevToolsActive}", value);
        }
    }

    /// <summary>
    /// Service provider used to resolve services and instances of menus.
    /// </summary>
    public IServiceProvider ServiceProvider => _serviceProvider;

    [UsedImplicitly]
    private void Update()
    {
        if (_instance != this) return;

        // TODO: make global shortcuts configurable
        bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool altJustPressed = Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
        bool escPressed = Input.GetKey(KeyCode.Escape);
        bool endPressed = Input.GetKey(KeyCode.End);
        bool hJustPressed = Input.GetKeyDown(KeyCode.H);
        bool oJustPressed = Input.GetKeyDown(KeyCode.O);

        if (escPressed && endPressed) _endEscaperService.EscapeTheEnd();

        if (ctrlPressed && oJustPressed) IsActivated = !IsActivated;

        if (IsActivated && ctrlPressed && hJustPressed) IsMainUiVisible = !IsMainUiVisible;

        if (IsActivated && !IsMainUiVisible && altJustPressed)
        {
            Logger.LogDebug("Switching cursor visibility");
            Cursor.visible = !Cursor.visible;
        }

        try { _updateEvent.Fire(); }
        catch
        {
            // ignore
        }
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        Logger.LogDebug("OnEnable called, registering initialization callback");

        if (_skipOnModsInit) Initialize();
        else On.RainWorld.OnModsInit += OnModsInit;
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        Logger.LogDebug("OnDisable called, deinitializing mod instance");
        Deinitialize();
    }

    /// <summary>
    /// Disposes (destroys) current mod instance.
    /// </summary>
    public void Dispose()
    {
        Logger.LogDebug("Dispose called, deinitializing mod instance");
        Deinitialize();
    }

    private void Deinitialize()
    {
        Logger.LogInformation("Deinitializing mod instance");

        if (_instance == this) _instance = null;
        On.RainWorld.OnModsInit -= OnModsInit;
        _serviceProvider.Dispose();

        Logger.LogInformation("Deinitialization complete. Goodbye!");
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

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        Initialize();
        _skipOnModsInit = true;
    }

    /// <summary>
    /// Rebuilds service provider. Resets the Dear Dev Tools mod.
    /// </summary>
    public ServiceProvider RebuildServiceProvider()
    {
        // NOTE: multiple downstream dependents might call this method multiple times 
        Logger.LogDebug("Rebuilding service provider");

        var serviceCollection = new ServiceCollection();
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

        return serviceProvider;
    }

    private void ConfigureDefaults(ServiceCollection serviceCollection)
    {
        // TODO: need another way to configure
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
        serviceCollection.TryAddTransient<BadMenu>();
    }

    private void Initialize()
    {
        if (_instance == this) return;

        if (_instance != null) throw new InvalidOperationException("Another mod instance is already running");

        Logger.LogInformation("Initializing mod instance");

        try
        {
            _serviceProvider = RebuildServiceProvider();

            _modImGuiContext = _serviceProvider.GetRequiredService<ModImGuiContext>();
            _menuManager = _serviceProvider.GetRequiredService<MenuManager>();
            _endEscaperService = _serviceProvider.GetRequiredService<EndEscaperService>();

            _menuManager.CreateNew<DearDevToolsEnabledOverlay>();
            _menuManager.CreateNew<MainMenuBar>();

#if DEBUG
            IsActivated = true;
            IsMainUiVisible = true;
#else
            IsActivated = false;
            IsMainUiVisible = false;
#endif
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Fatal error during Dear Dev Tool initialization");
            throw;
        }

        _instance = this;

        Logger.LogInformation("Initialization complete");
    }
}