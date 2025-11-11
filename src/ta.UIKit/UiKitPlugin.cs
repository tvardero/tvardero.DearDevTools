using System;
using System.Diagnostics;
using BepInEx;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ta.UIKit.Inputs;
using ta.UIKit.Logging;
using ta.UIKit.Nodes;

namespace ta.UIKit;

[BepInPlugin("ta.UIKit", "ta.UIKit", "0.0.1")]
public class UiKitPlugin : BaseUnityPlugin
{
    private readonly Stopwatch _drawSw = new();
    private readonly Stopwatch _updateSw = new();
    private ServiceProvider? _serviceProvider;

    public static bool IsInitialized => Instance != null;

    public static UiKitPlugin? Instance { get; private set; }

    public static UtilReferences InstanceUtils => Instance?.Utils ?? throw new InvalidOperationException("UiKit is not initialized");

    public UtilReferences Utils { get; private set; } = null!;

    [UsedImplicitly]
    public void Update()
    {
        if (!IsInitialized) return;

        Utils.SceneManager.ProcessInputEvents();

        TimeSpan elapsed = _updateSw.Elapsed;
        _updateSw.Restart();

        Utils.SceneManager.ProcessUpdate(elapsed);
    }

    [UsedImplicitly]
    public void OnEnable()
    {
        if (IsInitialized) return;

        RebuildServiceProvider();
        Instance = this;
    }

    [UsedImplicitly]
    public void OnDisable()
    {
        _serviceProvider?.Dispose();
        Instance = null;
    }

    [UsedImplicitly]
    public void OnPreRender()
    {
        if (!IsInitialized) return;

        TimeSpan elapsed = _drawSw.Elapsed;
        _drawSw.Restart();

        Utils.SceneManager.ProcessDraw(elapsed);
    }

    public static event Action<IServiceCollection>? OnConfigureServices;

    public static void RebuildServiceProvider()
    {
        if (Instance == null) throw new InvalidOperationException("UiKit is not initialized");

        Instance.RebuildServiceProvider_Impl();
    }

    private void RebuildServiceProvider_Impl()
    {
        _serviceProvider?.Dispose();

        var collection = new ServiceCollection();

        OnConfigureServices?.Invoke(collection);
        ConfigureDefaultServices(collection);

        _serviceProvider = collection.BuildServiceProvider();
        Utils = new UtilReferences(_serviceProvider);

        return;

        static void ConfigureDefaultServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddBepInEx());

            services.AddOptions<InputManagerOptions>();

            services.AddSingleton<SceneManager>();
            services.AddSingleton<InputManager>();
            services.AddSingleton<InputEventPool>();
            services.TryAddSingleton<NodeFactory>();
            services.TryAddSingleton(typeof(NodeFactory<>), typeof(ServiceProviderNodeFactory<>));
            services.TryAddTransient(typeof(NodePool<>));
        }
    }

    public class UtilReferences
    {
        public SceneManager SceneManager { get; }

        public ILoggerFactory LoggerFactory { get; }

        public InputManager InputManager { get; }

        public NodeFactory NodeFactory { get; }

        public UtilReferences(IServiceProvider serviceProvider)
        {
            SceneManager = serviceProvider.GetRequiredService<SceneManager>();
            LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            InputManager = serviceProvider.GetRequiredService<InputManager>();
            NodeFactory = serviceProvider.GetRequiredService<NodeFactory>();
        }

        public ILogger<T> GetLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
        
        public ILogger GetLogger(Type type)
        {
            return LoggerFactory.CreateLogger(type);
        }
    }
}
