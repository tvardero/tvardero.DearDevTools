using Microsoft.Extensions.Logging;
using tvardero.DearDevTools.Components;

namespace tvardero.DearDevTools.Menus;

public class HelpMenu : ImGuiWindowWithLeftPanelBase
{
    /// <inheritdoc />
    public HelpMenu(ILogger<HelpMenu> logger) : base("Help", logger: logger) { }

    public void NavigateTo(string docId) { }

    /// <inheritdoc />
    protected override void OnDrawLeftPanel() { }

    /// <inheritdoc />
    protected override void OnDrawMiddleContent()
    {
        ImGui.EndDisabled();
    }
}