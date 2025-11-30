using tvardero.DearDevTools.Components;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace tvardero.DearDevTools.Menus;

public class MainMenuBar : ImGuiDrawableBase
{
    /// <inheritdoc />
    public override bool IsVisible => true;

    /// <inheritdoc />
    public override bool IsBlockingWMEvent => false;

    /// <inheritdoc />
    public override bool RequiresMainUiShown => true;

    /// <inheritdoc />
    public override void Draw()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Menu"))
            {
                ImGui.MenuItem("Mod editor");
                ImGui.MenuItem("Region editor");
                ImGui.MenuItem("Map editor");
                ImGui.MenuItem("Dialog editor");
                ImGui.MenuItem("Palette editor");

                ImGui.Separator();

                ImGui.MenuItem("Settings");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                ImGui.MenuItem("Undo", "Ctrl+Z");
                ImGui.MenuItem("Redo", "Ctrl+Y");

                ImGui.Separator();

                // TODO: history
                ImGui.MenuItem("Clear history");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                ImGui.MenuItem("RW Debug");
                ImGui.MenuItem("ImGui Debug");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Navigate"))
            {
                ImGui.MenuItem("Warp to region/room");
                ImGui.MenuItem("Warp back");

                ImGui.Separator();

                ImGui.MenuItem("Sleep screen");
                ImGui.MenuItem("Death screen");
                ImGui.MenuItem("Main menu");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Tools"))
            {
                ImGui.MenuItem("Room settings");
                ImGui.MenuItem("Palette editor");
                ImGui.MenuItem("Room effects");
                ImGui.MenuItem("Room objects");
                ImGui.MenuItem("Room sounds");
                ImGui.MenuItem("Room triggers");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help"))
            {
                ImGui.MenuItem("How to use Dear Dev Tools?", "F1");
                ImGui.MenuItem("Whats new?");
                ImGui.MenuItem("Steam Workshop page");
                if (ImGui.MenuItem("GitHub page")) { Application.OpenURL("https://github.com/tvardero/tvardero.DearDevTools"); }
                if (ImGui.MenuItem("Report issue / suggest an idea")) { Application.OpenURL("https://github.com/tvardero/tvardero.DearDevTools/issues"); }
                if (ImGui.MenuItem("Support development")) { Application.OpenURL("https://ko-fi.com/tvardero"); }

                ImGui.Separator();

                if (ImGui.MenuItem("Escape the end", "Esc + End")) { Utils.ForceCrash(ForcedCrashCategory.Abort); }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}