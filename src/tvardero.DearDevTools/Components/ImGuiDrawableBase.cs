using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace tvardero.DearDevTools.Components;

[PublicAPI]
public abstract class ImGuiDrawableBase : IDisposable
{
    protected ImGuiDrawableBase(ILogger? logger = null)
    {
        InstancesCounter++;
        Logger = logger ?? NullLogger.Instance;
    }

    public bool IsDisposed
    {
        get;

        protected set
        {
            if (field == value) return;

#pragma warning disable CA1816
            if (value) GC.SuppressFinalize(this);
            else if (field) GC.ReRegisterForFinalize(this);
#pragma warning restore CA1816

            field = value;
        }
    }

    public virtual bool IsVisible { get; set; } = true;

    public virtual bool RequiresMainUiVisible { get; protected set; } = true;

    public virtual bool IsBlockingWMEvent { get; protected set; }

    protected static int InstancesCounter { get; private set; }

    protected ILogger Logger { get; }

#pragma warning disable CA1816
    /// <summary>
    /// Disposes (destroys / closes) current drawable instance.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;

        OnDispose();

        IsDisposed = true;
    }
#pragma warning restore CA1816

    public void Hide() => Show(false);

    public virtual void Show(bool show = true)
    {
        ThrowIfDisposed();

        IsVisible = show;
    }

    protected internal abstract void Draw();

    protected virtual void OnDispose() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
    }
}