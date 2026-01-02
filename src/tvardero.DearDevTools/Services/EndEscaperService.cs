using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    /// Escapes the End.
    /// </summary>
    /// <remarks>
    /// Crashes the game intentionally.
    /// </remarks>
    /// <param name="quick"> Crash the game without Unity crash handler (using <c>kill PID</c>). </param>
    [DoesNotReturn]
    public void EscapeTheEnd(bool quick = false)
    {
        _logger.LogCritical("Escaping the end");

        if (quick)
        {
            int pid = Process.GetCurrentProcess().Id;
            Process.GetProcessById(pid).Kill();
        }
        else { Utils.ForceCrash(ForcedCrashCategory.Abort); }

        throw null!; // unreachable
    }
}