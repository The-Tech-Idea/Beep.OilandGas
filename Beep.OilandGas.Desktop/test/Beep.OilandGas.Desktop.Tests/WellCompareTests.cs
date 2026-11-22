using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-06: the rules that hold without a database - a unit mismatch stops the delta, a missing value is not zero,
/// the export carries the matrix's own rows and columns, the overlay gate names what is missing, and a new
/// selection cancels the load in flight.
/// </summary>
public sealed class WellCompareTests
{
    [Fact]
    public void A_unit_mismatch_prevents_a_delta_and_a_missing_value_is_not_zero()
    {
        var facts = new[]
        {
            Facts("UWI-A", totalDepth: 3000m, depthUnit: "m", kb: 110m, latitude: 26m),
            Facts("UWI-B", totalDepth: 10000m, depthUnit: "ft", kb: 120m, latitude: 27m),
            Facts("UWI-C", totalDepth: null, depthUnit: null, kb: 0m, latitude: null),
        };

        var rows = OgWellComparison.BuildRows(facts);

        var td = Assert.Single(rows, row => row.PropertyKey == "final-td");
        Assert.True(td.UnitMismatch);
        Assert.Empty(td.Deltas);
        Assert.All(td.Values, cell => Assert.True(cell.UnitMismatch));

        // AC-2: the missing TD is "-" with no value, while the real zero KB is "0".
        Assert.True(td.Values[2].Missing);
        Assert.Null(td.Values[2].Value);
        Assert.Equal("-", td.Values[2].Text);

        var kb = Assert.Single(rows, row => row.PropertyKey == "kb");
        Assert.False(kb.UnitMismatch);
        Assert.False(kb.Values[2].Missing);
        Assert.Equal(0m, kb.Values[2].Value);
        Assert.StartsWith("0", kb.Values[2].Text, StringComparison.Ordinal);
        Assert.Equal(2, kb.Deltas.Count);
        Assert.Contains("+10", kb.Deltas[0], StringComparison.Ordinal);
        Assert.Contains("-110", kb.Deltas[1], StringComparison.Ordinal);
    }

    [Fact]
    public void The_export_uses_the_matrix_rows_and_columns()
    {
        var facts = new[]
        {
            Facts("UWI-A", totalDepth: 3000m, depthUnit: "m"),
            Facts("UWI-B", totalDepth: 3100m, depthUnit: "m"),
        };

        var matrix = new OgComparisonMatrix(
            [new OgCompareWell("UWI-A", "Alpha"), new OgCompareWell("UWI-B", "Bravo")],
            OgWellComparison.BuildRows(facts),
            true,
            null);

        var csv = matrix.ToCsv().Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // AC-3: one header plus one line per matrix row, with the wells as columns.
        Assert.Equal(matrix.Rows.Count + 1, csv.Length);
        Assert.StartsWith("Property,Category,Unit,UWI-A,UWI-B,Difference", csv[0], StringComparison.Ordinal);

        var tdLine = csv.First(line => line.StartsWith("Total depth", StringComparison.Ordinal));
        Assert.Contains("3000 m", tdLine, StringComparison.Ordinal);
        Assert.Contains("3100 m", tdLine, StringComparison.Ordinal);
    }

    [Fact]
    public void The_overlay_refuses_a_well_without_geometry_and_says_why()
    {
        var complete = new[] { Facts("UWI-A", 3000m, "m", latitude: 26m, longitude: 50m), Facts("UWI-B", 3100m, "m", latitude: 27m, longitude: 51m) };
        var incomplete = new[]
        {
            Facts("UWI-A", 3000m, "m", latitude: 26m, longitude: 50m),
            Facts("UWI-C", 2000m, "m", latitude: null, longitude: null),
        };

        var ok = OgWellComparison.OverlayDecision(complete);
        Assert.True(ok.CanOverlay);
        Assert.Null(ok.Reason);

        // AC-4
        var refused = OgWellComparison.OverlayDecision(incomplete);
        Assert.False(refused.CanOverlay);
        Assert.Contains("UWI-C", refused.Reason!, StringComparison.Ordinal);
        Assert.Contains("surface coordinates", refused.Reason!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Comparing_needs_two_wells_and_at_most_six()
    {
        var comparison = new OgWellComparison(() => null!, new Quiet());

        Assert.Equal(
            OgWellComparison.CodeNeedsTwo,
            (await comparison.CompareAsync(["UWI-A"])).Reference);

        var seven = Enumerable.Range(1, 7).Select(index => $"UWI-{index}").ToList();
        Assert.Equal(
            OgWellComparison.CodeTooMany,
            (await comparison.CompareAsync(seven)).Reference);
    }

    [Fact]
    public void A_selection_change_cancels_the_load_in_flight()
    {
        var gate = new OgCompareGate();
        var (first, version) = gate.BeginLoad();

        Assert.False(first.IsCancellationRequested);

        var (second, nextVersion) = gate.BeginLoad();

        // AC-5
        Assert.True(first.IsCancellationRequested);
        Assert.False(second.IsCancellationRequested);
        Assert.False(gate.IsCurrent(version));
        Assert.True(gate.IsCurrent(nextVersion));

        gate.SelectionChanged();
        Assert.True(second.IsCancellationRequested);
        Assert.False(gate.IsCurrent(nextVersion));
    }

    private static OgWellFacts Facts(
        string uwi,
        decimal? totalDepth = 3000m,
        string? depthUnit = "m",
        decimal? kb = 110m,
        decimal? latitude = 26m,
        decimal? longitude = 50m) =>
        new(
            uwi,
            Name: uwi,
            Field: "NORTH",
            Operator: "BA-1",
            GovernmentId: null,
            Status: "ACTIVE",
            WellClass: "DEVELOPMENT",
            TotalDepth: totalDepth,
            TotalDepthUnit: depthUnit,
            KellyBushing: kb,
            GroundElevation: 100m,
            ElevationUnit: "m",
            Latitude: latitude,
            Longitude: longitude,
            SpudDate: new DateTime(2020, 1, 1),
            CompletionDate: null,
            AbandonmentDate: null);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
