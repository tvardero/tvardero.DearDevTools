using System.Numerics;
using Microsoft.Extensions.Logging;
using tvardero.DearDevTools.Components;
using tvardero.DearDevTools.Services;

namespace tvardero.DearDevTools.Menus;

public class DearDevToolsEnabledOverlay : ImGuiDrawableBase
{
    private readonly DearDevToolsPlugin _plugin;

    public DearDevToolsEnabledOverlay(DearDevToolsPlugin plugin, ILogger<DearDevToolsEnabledOverlay> logger)
        : base(logger: logger)
    {
        _plugin = plugin;
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
        ImGui.TextColored(new Vector4(1, 0, 0, 1), "DEBUG BUILD");
#endif

        ImGui.End();
    }
}