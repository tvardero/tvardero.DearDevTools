using System.Numerics;
using RWIMGUI.API;

namespace tvardero.DearDevTools.Internal;

internal class DearDevToolsIMGUIContext : IMGUIContext, IDisposable
{
    private bool _disposed;
    private ModContext _modContext = null!;

    public void AttachToModContext(ModContext modContext)
    {
        _modContext = modContext;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposed = true;
        _modContext = null!;
    }

    /// <inheritdoc />
    public override bool BlockWMEvent()
    {
        return true;
    }

    /// <inheritdoc />
    public override void Render(ref IntPtr IDXGISwapChain, ref uint SyncInterval, ref uint Flags)
    {
        if (_disposed) return;
        if (!_modContext.MainUiVisible) return;

        ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.FirstUseEver);
        ImGui.Begin("Dear Dev Tools");

        ImGui.Text("Hello");

        ImGui.End();
    }
}