using Microsoft.Extensions.Logging;
using RWIMGUI.API;
using tvardero.DearDevTools.Components;

namespace tvardero.DearDevTools.Services;

internal sealed class ModImGuiContext : IMGUIContext, IDisposable
{
    private readonly List<ImGuiDrawableBase> _renderList = [];
    private readonly DearDevToolsPlugin _plugin;
    private readonly ILogger<ModImGuiContext> _logger;
    private ImGuiDrawableBase[]? _renderListSnapshot;
    private bool _disposed;

    public ModImGuiContext(DearDevToolsPlugin plugin, ILogger<ModImGuiContext> logger)
    {
        _plugin = plugin;
        _logger = logger;
        RenderList = _renderList.AsReadOnly();
    }

    public IReadOnlyList<ImGuiDrawableBase> RenderList { get; }

    public bool IsActive => ImGUIAPI.CurrentContext == this;

    public void Activate()
    {
        if (!IsActive)
        {
            _logger.LogDebug($"Activating {nameof(ModImGuiContext)}");
            ImGUIAPI.SwitchContext(this);
        }
    }

    public void AddDrawable(ImGuiDrawableBase drawable)
    {
        if (_renderList.Contains(drawable)) return;

        _logger.LogDebug("Adding drawable {Drawable} to render list", drawable);
        _renderList.Add(drawable);
        _renderListSnapshot = null;
    }

    /// <inheritdoc />
    public override bool BlockWMEvent()
    {
        if (_disposed) return false;
        if (!_plugin.AreDearDevToolsActive) return false;

        bool isMainUiVisible = _plugin.IsMainUiVisible;
        return _renderList
            .Where(drawable => drawable is { IsDisposed: false, IsVisible: true })
            .Where(drawable => isMainUiVisible || !drawable.RequiresMainUiVisible)
            .Any(drawable => drawable.IsBlockingWMEvent);
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            _logger.LogDebug($"Deactivating {nameof(ModImGuiContext)}");
            ImGUIAPI.SwitchContext(null);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _logger.LogDebug($"Disposing {nameof(ModImGuiContext)}");

        Deactivate();

        _disposed = true;
        _renderList.Clear();
        _renderListSnapshot = null;
    }

    public void RemoveDrawable(ImGuiDrawableBase drawable)
    {
        if (!_renderList.Contains(drawable)) return;

        _logger.LogDebug("Removing drawable {Drawable} from render list", drawable);
        _renderList.Remove(drawable);
        _renderListSnapshot = null;
    }

    /// <inheritdoc />
    public override void Render(ref IntPtr IDXGISwapChain, ref uint SyncInterval, ref uint Flags)
    {
        if (_disposed) return;
        if (!_plugin.AreDearDevToolsActive) return;

        SanitizeRenderList();

        bool isMainUiVisible = _plugin.IsMainUiVisible;
        IEnumerable<ImGuiDrawableBase> toDraw = GetRenderListSnapshot()
            .Where(drawable => drawable.IsVisible)
            .Where(drawable => isMainUiVisible || !drawable.RequiresMainUiVisible);

        foreach (ImGuiDrawableBase drawable in toDraw)
        {
            try { drawable.Draw(); }
            catch (Exception e)
            {
                _logger.LogError(e, "Drawable {Drawable} threw an exception during rendering, removing drawable from render list", drawable);
                _renderList.Remove(drawable);
            }
        }
    }

    public void SanitizeRenderList()
    {
        int countBefore = _renderList.Count;

        // remove null or disposed instances
        _renderList.RemoveAll(d => d is not { IsDisposed: false });

        if (countBefore != _renderList.Count) _renderListSnapshot = null;
    }

    private ImGuiDrawableBase[] GetRenderListSnapshot()
    {
        _renderListSnapshot ??= _renderList.ToArray();
        return _renderListSnapshot;
    }
}