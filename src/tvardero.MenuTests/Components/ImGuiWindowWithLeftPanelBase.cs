using System.Numerics;
using ImGuiNET;
using Microsoft.Extensions.Logging;

namespace tvardero.MenuTests.Components;

public abstract class ImGuiWindowWithLeftPanelBase : ImGuiWindowBase
{
    /// <inheritdoc />
    protected ImGuiWindowWithLeftPanelBase(
        string title,
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.MenuBar,
        Vector2? initialSize = null,
        bool disposeOnClose = false,
        bool allowMultipleInstances = false,
        ILogger? logger = null)
        : base(title, windowFlags, initialSize, disposeOnClose, allowMultipleInstances, logger) { }

    public bool IsLeftPanelCollapsed { get; set; }

    protected abstract void OnDrawLeftPanel();

    protected abstract void OnDrawMiddleContent();

    /// <inheritdoc />
    protected sealed override void OnDrawWindowContent()
    {
        ImGui.BeginMenuBar();

        string title = IsLeftPanelCollapsed ? "Show left panel###Collapse" : "Hide left panel###Collapse";
        if (ImGui.MenuItem(title)) IsLeftPanelCollapsed = !IsLeftPanelCollapsed;

        ImGui.EndMenuBar();

        if (!IsLeftPanelCollapsed)
        {
            ImGui.BeginChild("Left pane", new Vector2(150, 0), ImGuiChildFlags.Border | ImGuiChildFlags.ResizeX);
            OnDrawLeftPanel();
            ImGui.EndChild();

            ImGui.SameLine();
        }

        OnDrawMiddleContent();
    }
}