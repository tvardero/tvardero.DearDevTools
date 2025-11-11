using System;
using BepInEx.Logging;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ta.UIKit.Logging;

public class BepInExLogger : ILogger
{
    private readonly ManualLogSource _logSource;

    public BepInExLogger(ManualLogSource logSource)
    {
        _logSource = logSource;
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
    where TState : notnull
    {
        return NullScope.Instance;
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        BepInEx.Logging.LogLevel bepInExLogLevel = ToBepInExLogLevel(logLevel);
        string message = FormatMessage(state, exception, formatter);

        _logSource.Log(bepInExLogLevel, message);
    }

    private static string FormatMessage<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string? message = formatter(state, exception);
        if (exception != null) message += Environment.NewLine + exception;
        return message;
    }

    private static BepInEx.Logging.LogLevel ToBepInExLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => BepInEx.Logging.LogLevel.Debug,
            LogLevel.Debug => BepInEx.Logging.LogLevel.Info,
            LogLevel.Information => BepInEx.Logging.LogLevel.Message,
            LogLevel.Warning => BepInEx.Logging.LogLevel.Warning,
            LogLevel.Error => BepInEx.Logging.LogLevel.Error,
            LogLevel.Critical => BepInEx.Logging.LogLevel.Fatal,
            LogLevel.None => BepInEx.Logging.LogLevel.None,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null),
        };
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose() { }
    }
}
