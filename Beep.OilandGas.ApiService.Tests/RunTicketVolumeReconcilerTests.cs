using Beep.OilandGas.LifeCycle.Services.Accounting;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RunTicketVolumeReconcilerTests
{
    private static RUN_TICKET Ticket(string id, decimal? volume, string unit = "BBL") =>
        new() { RUN_TICKET_ID = id, NET_VOLUME = volume, VOLUME_OUOM = unit };

    private static Dictionary<string, List<ALLOCATION_RESULT>> Allocations(params (string Id, decimal? Volume)[] values) =>
        values.GroupBy(value => value.Id).ToDictionary(group => group.Key, group => group.Select(value =>
            new ALLOCATION_RESULT { ALLOCATION_REQUEST_ID = value.Id, ALLOCATED_VOLUME = value.Volume }).ToList());

    [Fact]
    public void ComparesMeasuredVolumeWithAllocatedVolume()
    {
        var result = RunTicketVolumeReconciler.Reconcile(new[] { Ticket("a", 100m) }, Allocations(("a", 90m)));
        Assert.Equal(ReconciliationStatus.VarianceDetected, result.Status);
        Assert.Equal(100m, result.FieldProductionVolume);
        Assert.Equal(90m, result.ALLOCATED_VOLUME);
        Assert.Equal(10m, result.DiscrepancyPercentage);
    }

    [Fact]
    public void MatchingTicketsProduceMatchedResult()
    {
        var result = RunTicketVolumeReconciler.Reconcile(new[] { Ticket("a", 100m), Ticket("b", 20m) },
            Allocations(("a", 100m), ("b", 20m)));
        Assert.Equal(ReconciliationStatus.Matched, result.Status);
        Assert.Equal(120m, result.OilVolume!.FieldVolume);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void OffsettingTicketVariancesDoNotProduceMatchedResult()
    {
        var result = RunTicketVolumeReconciler.Reconcile(new[] { Ticket("a", 100m), Ticket("b", 100m) },
            Allocations(("a", 90m), ("b", 110m)));
        Assert.Equal(0m, result.Discrepancy);
        Assert.Equal(ReconciliationStatus.VarianceDetected, result.Status);
        Assert.Equal(2, result.Issues.Count);
    }

    [Theory]
    [InlineData(null, "BBL")]
    [InlineData(-1, "BBL")]
    [InlineData(100, "MCF")]
    [InlineData(100, "")]
    public void InvalidMeasurementsCannotProducePartialSuccess(int? volume, string unit)
    {
        var result = RunTicketVolumeReconciler.Reconcile(new[] { Ticket("good", 10m), Ticket("bad", volume, unit) },
            Allocations(("good", 10m), ("bad", 100m)));
        Assert.Equal(ReconciliationStatus.Error, result.Status);
        Assert.Equal(0m, result.FieldProductionVolume);
        Assert.Null(result.OilVolume);
    }

    [Fact]
    public void EmptyTicketsAreMissingData()
    {
        Assert.Equal(ReconciliationStatus.Error, RunTicketVolumeReconciler.Reconcile(Array.Empty<RUN_TICKET>(), Allocations()).Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void MissingOrAmbiguousAllocationsAreErrors(int count)
    {
        var allocations = Allocations(Enumerable.Repeat(("a", (decimal?)100m), count).ToArray());
        Assert.Equal(ReconciliationStatus.Error, RunTicketVolumeReconciler.Reconcile(new[] { Ticket("a", 100m) }, allocations).Status);
    }

    [Fact]
    public void ZeroMeasuredVolumeHasNoDefinedPercentage()
    {
        var result = RunTicketVolumeReconciler.Reconcile(new[] { Ticket("a", 0m) }, Allocations(("a", 2m)));
        Assert.Equal(ReconciliationStatus.VarianceDetected, result.Status);
        Assert.Null(result.DiscrepancyPercentage);
    }
}
