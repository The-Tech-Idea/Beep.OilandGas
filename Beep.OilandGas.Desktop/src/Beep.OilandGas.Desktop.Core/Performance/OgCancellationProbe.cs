using System.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// REL-04 AC-4: measures how long a cancellable operation takes to stop after a cancel is requested.
/// </summary>
/// <remarks>
/// <para>
/// The probe runs a chunked loop that observes a <see cref="CancellationToken"/> between chunks — the shape
/// every long operation must have. It reports the delay between the cancel request and the loop stopping, which
/// is the number the budget is about.
/// </para>
/// <para>
/// It deliberately does not pretend a single synchronous render can be interrupted: a call that does not
/// observe a token cannot be stopped mid-frame, and this probe cannot make it stop. That limit is recorded in
/// the evidence rather than hidden by a number.
/// </para>
/// </remarks>
public static class OgCancellationProbe
{
    /// <summary>The budget for a cancel to be observed.</summary>
    public const double BudgetMilliseconds = 500;

    /// <summary>
    /// Runs a cancellable loop, cancels it, and returns the observed latency in milliseconds.
    /// </summary>
    /// <param name="runMilliseconds">How long the loop runs before the cancel is requested.</param>
    /// <param name="chunkMilliseconds">How long each chunk takes; this is the coarsest cancellation granularity.</param>
    /// <param name="outer">Cancels the probe early; not the measurement's cancel.</param>
    public static double MeasureLatency(
        int runMilliseconds = 120,
        int chunkMilliseconds = 5,
        CancellationToken outer = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(runMilliseconds);
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkMilliseconds, 1);

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(outer);
        var clock = Stopwatch.StartNew();
        var stoppedAt = -1d;

        var worker = new Thread(() =>
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    Thread.Sleep(chunkMilliseconds);
                }
            }
            finally
            {
                stoppedAt = clock.Elapsed.TotalMilliseconds;
            }
        })
        {
            IsBackground = true,
            Name = "og-cancellation-probe",
        };

        worker.Start();

        Thread.Sleep(runMilliseconds);

        var requestedAt = clock.Elapsed.TotalMilliseconds;
        cancellation.Cancel();
        worker.Join(TimeSpan.FromSeconds(5));

        // The worker not stopping is itself a result: report "never", not a number that looks fine.
        return stoppedAt < 0 ? double.PositiveInfinity : Math.Max(0, stoppedAt - requestedAt);
    }
}
