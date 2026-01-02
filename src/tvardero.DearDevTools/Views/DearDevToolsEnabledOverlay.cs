using System.Numerics;
using tvardero.DearDevTools.Components;
using tvardero.DearDevTools.Services;

namespace tvardero.DearDevTools.Views;

public class DearDevToolsEnabledOverlay : ImGuiDrawableBase
{
    private readonly DearDevToolsPlugin _plugin;
    private readonly GameStateService _gameStateService;

    public DearDevToolsEnabledOverlay(DearDevToolsPlugin plugin, GameStateService gameStateService)
    {
        _plugin = plugin;
        _gameStateService = gameStateService;
    }

    /// <inheritdoc />
    public override bool IsVisible => !_plugin.IsMainUiVisible;

    /// <inheritdoc />
    public override bool RequiresMainUiVisible => false;

    /// <inheritdoc />
    public override bool IsBlockingWMEvent => false;

    /// <inheritdoc />
    protected internal override void Draw()
    {
        const float _PADDING = 10f;
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 workPos = viewport.WorkPos;
        Vector2 workSize = viewport.WorkSize;
        var anchor = new Vector2(1, 1);
        var pos = new Vector2(workPos.X + workSize.X - _PADDING, workPos.Y + workSize.Y - _PADDING);
        ImGui.SetNextWindowPos(pos, ImGuiCond.Always, anchor);

        ImGui.SetNextWindowBgAlpha(0.15f);

        ImGui.Begin("##DearDevToolsEnabled",
            ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoSavedSettings
          | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove);

        ImGui.Text("Dear Dev Tools are enabled");
        ImGui.Separator();

        ImGui.Text("Ctrl+O to enable/disable Dear Dev Tools");
        ImGui.Text("Ctrl+H to open/close UI");

#if DEBUG
        ImGui.Separator();
        ImGui.Text("DEBUG BUILD");
#endif

        ImGui.End();
    }
}