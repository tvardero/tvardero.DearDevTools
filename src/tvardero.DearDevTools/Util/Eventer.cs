using tvardero.JetBrains.Annotations;

namespace tvardero.DearDevTools.Util;

/// <summary>
/// Event producer without parameters, that manages subscription and unsubscription via disposable tokens.
/// </summary>
public class Eventer
{
    private readonly List<Action> _handlers = [];

    /// <summary>
    /// Fires the event.
    /// </summary>
    /// <exception cref="AggregateException"> One or more subscribed handlers threw an exception. </exception>
    public void Fire()
    {
        List<Exception> exceptions = [];
        foreach (Action? handler in _handlers)
        {
            try { handler(); }
            catch (Exception ex) { exceptions.Add(ex); }
        }

        if (exceptions.Count > 0) throw new AggregateException(exceptions);
    }

    /// <summary>
    /// Subscribes a handler to run when event fires.
    /// To unsubscribe the handler, <see cref="IDisposable.Dispose"> dispose </see> the token that you get in return.
    /// </summary>
    /// <param name="handler"> Handler. </param>
    /// <returns> Subscription token, which on dispose unsubscribes the handler. </returns>
    /// <exception cref="ArgumentNullException"> Handler is null. </exception>
    [MustDisposeResource]
    public IDisposable Register(Action handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        _handlers.Add(handler);
        return new Unsubscriber(new WeakReference<List<Action>>(_handlers), handler);
    }

    private class Unsubscriber : IDisposable
    {
        private readonly WeakReference<List<Action>> _handlersWeak;
        private readonly Action _handler;
        private bool _disposed;

        public Unsubscriber(WeakReference<List<Action>> handlersWeak, Action handler)
        {
            _handlersWeak = handlersWeak;
            _handler = handler;
        }

        ~Unsubscriber()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (!_handlersWeak.TryGetTarget(out List<Action>? handlers)) return;

            handlers.Remove(_handler);
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

/// <summary>
/// Event producer with a strongly-typed parameter, that manages subscription and unsubscription via disposable tokens.
/// </summary>
/// <typeparam name="TArgument"> Type of event argument. </typeparam>
public class Eventer<TArgument>
{
    private readonly List<Action<TArgument>> _handlers = [];

    /// <summary>
    /// Fires the event.
    /// </summary>
    /// <param name="argument"> Argument of the event. </param>
    /// <exception cref="AggregateException"> One or more subscribed handlers threw an exception. </exception>
    public void Fire(TArgument argument)
    {
        List<Exception> exceptions = [];
        foreach (Action<TArgument>? handler in _handlers)
        {
            try { handler(argument); }
            catch (Exception ex) { exceptions.Add(ex); }
        }

        if (exceptions.Count > 0) throw new AggregateException(exceptions);
    }

    /// <summary>
    /// Subscribes a handler to run when event fires.
    /// To unsubscribe the handler, <see cref="IDisposable.Dispose"> dispose </see> the token that you get in return.
    /// </summary>
    /// <param name="handler"> Handler. </param>
    /// <returns> Subscription token, which on dispose unsubscribes the handler. </returns>
    /// <exception cref="ArgumentNullException"> Handler is null. </exception>
    [MustDisposeResource]
    public IDisposable Register(Action<TArgument> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        _handlers.Add(handler);
        return new Unsubscriber(new WeakReference<List<Action<TArgument>>>(_handlers), handler);
    }

    private class Unsubscriber : IDisposable
    {
        private readonly WeakReference<List<Action<TArgument>>> _handlersWeak;
        private readonly Action<TArgument> _handler;
        private bool _disposed;

        public Unsubscriber(WeakReference<List<Action<TArgument>>> handlersWeak, Action<TArgument> handler)
        {
            _handlersWeak = handlersWeak;
            _handler = handler;
        }

        ~Unsubscriber()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (!_handlersWeak.TryGetTarget(out List<Action<TArgument>>? handlers)) return;

            handlers.Remove(_handler);
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}