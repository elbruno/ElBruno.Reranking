namespace ElBruno.Reranking.Utils;

using System.Diagnostics;

/// <summary>
/// Helper for measuring elapsed time during reranking operations.
/// </summary>
public class TimingHelper : IDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    /// <summary>
    /// Gets the elapsed time in milliseconds since the helper was created.
    /// </summary>
    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    /// <summary>
    /// Disposes the helper and stops the timer.
    /// </summary>
    public void Dispose()
    {
        _stopwatch.Stop();
    }
}
