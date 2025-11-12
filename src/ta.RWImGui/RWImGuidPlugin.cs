using System;
using UnityEngine;

namespace ta.RWImGui;

public class Class1 : BepInEx.BaseUnityPlugin
{
    private void OnGUI()
    {
        if (GUILayout.Button("Press Me")) { Debug.Log("Hello!"); }
    }
}
