using BepInEx.Logging;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace tvardero.DearDevTools.Logging;

public sealed class BepInExLogger : ILogger, IDisposable
{
    private readonly Func<LogLevel> _minimumLogLevelEval;
    private readonly ManualLogSource _mls;

    public BepInExLogger(Func<LogLevel> minimumLogLevelEval, ManualLogSource mls)
    {
        _minimumLogLevelEval = minimumLogLevelEval;
        _mls = mls;
    }

    ~BepInExLogger()
    {
        _mls.Dispose();
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state)
    where TState : notnull
    {
        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _mls.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None && logLevel >= _minimumLogLevelEval();
    }

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        BepInEx.Logging.LogLevel matchedLevel = MatchLogLevel(logLevel);

        string? message = formatter(state, exception);
        if (exception != null) message += $"\n{exception}";

        _mls.Log(matchedLevel, message);
    }

    private static BepInEx.Logging.LogLevel MatchLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => BepInEx.Logging.LogLevel.Debug,
            LogLevel.Debug => BepInEx.Logging.LogLevel.Debug,
            LogLevel.Information => BepInEx.Logging.LogLevel.Info,
            LogLevel.Warning => BepInEx.Logging.LogLevel.Warning,
            LogLevel.Error => BepInEx.Logging.LogLevel.Error,
            LogLevel.Critical => BepInEx.Logging.LogLevel.Fatal,
            LogLevel.None => BepInEx.Logging.LogLevel.None,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };
    }
}