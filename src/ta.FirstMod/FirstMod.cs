using System;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using JetBrains.Annotations;
using UnityEngine;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ta.FirstMod;

[BepInPlugin("ta.FirstMod", "ta.FirstMod", "1.0.0")]
public class FirstMod : BaseUnityPlugin
{
    private bool _initialized;

    [UsedImplicitly]
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (_initialized) return;

        try
        {
            _initialized = true;

            On.Player.Jump += PlayerOnJump;
        }
        catch (Exception ex) { Logger.LogError(ex); }
    }

    private void PlayerOnJump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);
        self.jumpBoost *= 4;
        
        Logger.LogInfo("Jomp! BepInEx");
        Debug.Log("Jomp! Unity");
    }
}
