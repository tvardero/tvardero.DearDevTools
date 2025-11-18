using tvardero.DearDevTools.Components;

namespace tvardero.DearDevTools.Menus;

public class MapEditorMenu : ImGuiDrawableBase
{
    private bool _isVisible;

    /// <inheritdoc />
    public override bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    /// <inheritdoc />
    public override bool IsBlockingRWInput => true;

    /// <inheritdoc />
    protected override void OnDraw()
    {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();

        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);

        ImGui.Begin("Map editor", ref _isVisible, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings);

        // todo

        ImGui.End();
    }
}
