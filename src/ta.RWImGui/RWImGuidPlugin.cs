using System;
using BepInEx;
using JetBrains.Annotations;
using UnityEngine;

namespace ta.RWImGui;

[BepInPlugin("ta.RWImGUI", "ta.RWImGUI", "0.0.1")]
public class RWImGUIPlugin : BaseUnityPlugin
{
    private bool _enabled;

    [UsedImplicitly]
    public void OnEnable()
    {
        _enabled = true;
    }

    [UsedImplicitly]
    public void OnDisable()
    {
        _enabled = false;
    }

    [UsedImplicitly]
    public void OnGUI()
    {
        if (!_enabled) return;
        
        if (GUILayout.Button("Press Me")) { Debug.Log("Hello!"); }
    }
}
