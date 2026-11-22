using System.Threading;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-07 AC-6: the anti-collision scan observes a token per offset well, and changing the offset set cancels
/// the scan in flight so no superseded separation factor can be shown.
/// </summary>
public sealed class WellPathCancellationTests
{
    [Fact]
    public void The_scan_observes_the_token_between_offset_wells()
    {
        using var cancellation = new CancellationTokenSource();
        var progressed = 0;

        // AC-6: the cancel lands after the first well; the next per-well check must observe it.
        Assert.Throws<OperationCanceledException>(() => OgWellPath.ComputeOffsets(
            ManyOffsetWells(5),
            cancellation.Token,
            (done, _) =>
            {
                progressed = done;
                if (done == 1)
                {
                    cancellation.Cancel();
                }
            }));

        Assert.Equal(1, progressed);
    }

    [Fact]
    public void A_cancelled_scan_does_no_work_and_returns_nothing()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var progressed = 0;

        Assert.Throws<OperationCanceledException>(() => OgWellPath.ComputeOffsets(
            ManyOffsetWells(3),
            cancellation.Token,
            (_, _) => progressed++));

        Assert.Equal(0, progressed);
    }

    [Fact]
    public void A_completed_scan_concatenates_one_scan_per_offset_well()
    {
        var (result, _) = OgWellPath.ComputeOffsets(OgWellPath.TwoOffsetWells, CancellationToken.None);

        Assert.Equal(2, result.Offsets.Count);
        Assert.Equal(
            ["DEMO-OFF", "DEMO-OFF-2"],
            result.Offsets.Select(offset => offset.Name));
    }

    [Fact]
    public void Changing_the_offset_set_cancels_the_scan_in_flight_and_the_latest_wins()
    {
        RunOnSta(() =>
        {
            using var application = new OgBootstrapper().Create();
            var page = new WellPathPageView(application);

            // AC-6: a large set is still scanning when the set is replaced.
            var superseded = page.ComputeOffsetAsync(ManyOffsetWells(300));
            var latest = page.ComputeOffsetAsync(OgWellPath.DemoOffsetWells);

            var stale = superseded.GetAwaiter().GetResult();
            var current = latest.GetAwaiter().GetResult();

            Assert.Null(stale);
            Assert.True(page.LastComputationCancelled);

            Assert.NotNull(current);
            Assert.Single(current!.Value.Result.Offsets);
        });
    }

    private static IReadOnlyList<OgOffsetWell> ManyOffsetWells(int count) =>
        Enumerable.Range(1, count)
            .Select(index => new OgOffsetWell(
                $"OFF-{index}",
                OgWellPath.OffsetSurvey,
                29.0 + (index * 0.00001),
                48.0))
            .ToList();

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            throw failure;
        }
    }
}
