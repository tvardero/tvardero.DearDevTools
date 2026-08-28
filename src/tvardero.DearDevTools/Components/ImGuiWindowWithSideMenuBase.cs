using System.Numerics;
using Microsoft.Extensions.Logging;

namespace tvardero.DearDevTools.Components;

public abstract class ImGuiWindowWithSideMenuBase : ImGuiWindowBase
{
    /// <inheritdoc />
    protected ImGuiWindowWithSideMenuBase(
        string title,
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.MenuBar,
        Vector2? initialSize = null,
        bool disposeOnClose = false,
        ILogger? logger = null)
        : base(title, windowFlags, initialSize, disposeOnClose, logger) { }

    public bool IsSideMenuCollapsed { get; set; }

    protected abstract void OnDrawBody();

    protected abstract void OnDrawSideMenu();

    /// <inheritdoc />
    protected sealed override void OnDrawWindowContent()
    {
        ImGui.BeginMenuBar();

        string title = IsSideMenuCollapsed ? "Show side menu###Collapse" : "Hide side menu###Collapse";
        if (ImGui.MenuItem(title)) IsSideMenuCollapsed = !IsSideMenuCollapsed;

        ImGui.EndMenuBar();

        if (!IsSideMenuCollapsed)
        {
            ImGui.BeginChild("Left pane", new Vector2(150, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);

            OnDrawSideMenu();

            ImGui.EndChild();

            ImGui.SameLine();
        }

        OnDrawBody();
    }
}