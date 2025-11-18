namespace tvardero.DearDevTools.Internal;

/// <summary>
/// This class holds internal state of the mod.
/// </summary>
internal class ModContext : IDisposable
{
    private bool _disposed;
    private readonly DearDevToolsIMGUIContext _imguiContext;

    ~ModContext()
    {
        Dispose(); // if unity forgets to call OnDisable
    }

    private ModContext(DearDevToolsIMGUIContext imguiContext)
    {
        _imguiContext = imguiContext;
    }

    public bool QuickToolsEnabled { get; set; }

    public bool MainUiVisible { get; set; } = true;

    public static ModContext Create()
    {
        var imguiContext = new DearDevToolsIMGUIContext();
        var ctx = new ModContext(imguiContext);
        imguiContext.AttachToModContext(ctx);

        return ctx;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        _imguiContext.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
