using System.Numerics;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace tvardero.DearDevTools.Components;

[PublicAPI]
public abstract class ImGuiWindowBase : ImGuiDrawableBase
{
    private readonly Vector2 _initialSize;
    private readonly bool _disposeOnClose;
    private bool _isOpen = true;
    private bool _stealFocusNextFrame;

    /// <inheritdoc />
    protected ImGuiWindowBase(
        string title,
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.None,
        Vector2? initialSize = null,
        bool disposeOnClose = false,
        bool allowMultipleInstances = false,
        ILogger? logger = null) : base(allowMultipleInstances, logger)
    {
        Title = !title.Contains("##") && allowMultipleInstances ? $"{title}##{InstancesCounter}" : title;
        WindowFlags = windowFlags;
        _initialSize = initialSize ?? new Vector2(600, 400);
        _disposeOnClose = disposeOnClose;
    }

    /// <inheritdoc />
    public override bool IsVisible
    {
        get => _isOpen && field;

        set
        {
            ThrowIfDisposed();

            if (field == value) return;

            if (value && !_isOpen) _isOpen = true;
            field = value;
        }
    } = true;

    public bool IsOpen => _isOpen;

    public string Title
    {
        get;

        set
        {
            ThrowIfDisposed();

            if (field == value) return;

            if (!value.Contains("##") && AllowsMultipleInstances) value = value + "##" + InstancesCounter;
            field = value;
        }
    }

    public ImGuiWindowFlags WindowFlags { get; set; }

    public void Close()
    {
        _isOpen = false;
        if (_disposeOnClose) Dispose();
    }

    public void Focus()
    {
        ThrowIfDisposed();

        _stealFocusNextFrame = true;
    }

    public virtual void Reopen()
    {
        _isOpen = true;
        IsDisposed = false;
    }

    /// <inheritdoc />
    protected internal sealed override void Draw()
    {
        ThrowIfDisposed();

        if (!_isOpen && _disposeOnClose && !IsDisposed)
        {
            Dispose();
            return;
        }

        ImGuiCond sizeFlags = WindowFlags.HasFlag(ImGuiWindowFlags.NoResize) ? ImGuiCond.Always : ImGuiCond.Once;
        ImGui.SetNextWindowSize(_initialSize, sizeFlags);
        ImGui.Begin(Title, ref _isOpen, WindowFlags);

        if (_stealFocusNextFrame)
        {
            ImGui.SetWindowFocus();
            _stealFocusNextFrame = false;
        }

        OnDrawWindowContent();

        ImGui.End();
    }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        _isOpen = false;
        base.OnDispose();
    }

    protected abstract void OnDrawWindowContent();
}