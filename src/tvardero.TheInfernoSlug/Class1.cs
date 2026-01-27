using System.Diagnostics.CodeAnalysis;
using BepInEx;
using JetBrains.Annotations;

namespace tvardero.TheInfernoSlug;

[BepInPlugin("tvardero.TheInfernoSlug", "The Inferno slugcat", "0.0.1")]
[BepInDependency("slime-cubed.slugbase")]
[PublicAPI]
public class TheInfernoSlugPlugin : BaseUnityPlugin
{
    private static bool _rwModsInitCalled; 

    public static TheInfernoSlugPlugin? Instance { get; private set; }

    [MemberNotNullWhen(true, nameof(Instance))]
    public static bool IsInitialized => Instance is not null;

    private void OnEnable()
    {
        if (_rwModsInitCalled) Initialize();
        else On.RainWorld.OnModsInit += OnModsInit;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        Initialize();
        
        _rwModsInitCalled = true;
        On.RainWorld.OnModsInit -= OnModsInit;
    }

    private void OnDisable()
    {
        Deinitialize();
    }

    private void Initialize()
    {
        if (IsInitialized) return;

        Instance = this;
    }

    private void Deinitialize()
    {
        if (!IsInitialized) return;

        Instance = null;
    }
}