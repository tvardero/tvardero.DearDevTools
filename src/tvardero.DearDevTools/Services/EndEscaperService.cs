using Microsoft.Extensions.Logging;
using UnityEngine.Diagnostics;

namespace tvardero.DearDevTools.Services;

/// <summary>
/// Service to escape the end.
/// </summary>
internal sealed class EndEscaperService
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of service.
    /// </summary>
    /// <param name="logger"> Logger. </param>
    public EndEscaperService(ILogger<EndEscaperService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Escapes the end.
    /// </summary>
    public void EscapeTheEnd()
    {
        _logger.LogCritical("Escaping the end");
        Utils.ForceCrash(ForcedCrashCategory.Abort);
    }
}