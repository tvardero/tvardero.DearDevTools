using tvardero.DearDevTools.Components;

namespace tvardero.DearDevTools.Views;

public class HelpMenu : ImGuiWindowWithLeftPanelBase
{
    /// <inheritdoc />
    public HelpMenu() : base("Help") { }

    public void NavigateTo(string docId) { }

    /// <inheritdoc />
    protected override void OnDrawLeftPanel() { }

    /// <inheritdoc />
    protected override void OnDrawMiddleContent()
    {
        ImGui.EndDisabled();
    }
}