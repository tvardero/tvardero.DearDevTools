using System.Numerics;

namespace tvardero.DearDevTools.Components;

public abstract class SimpleWindowBase : ImGuiDrawableBase
{
    private bool _isVisible;

    /// <inheritdoc />
    public override bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    public abstract string Name { get; }

    /// <inheritdoc />
    protected sealed override void OnDraw()
    {
        ImGui.SetNextWindowSize(new Vector2(600, 400), ImGuiCond.FirstUseEver);
        ImGui.Begin(Name, ref _isVisible);

        OnDrawWindowContent();

        ImGui.End();
    }

    protected abstract void OnDrawWindowContent();
}
