using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-10: pay summary against the library's reference interval, the refusals the screen must make, the zone
/// exclusion that names a missing curve, and CSV/XLSX exports that match the result.
/// </summary>
public sealed class PaySummaryTests
{
    [Fact]
    public void The_reference_interval_reproduces_the_library_hand_calculation()
    {
        var run = OgPaySummaryWorkspace.Compute(
            OgPaySummaryWorkspace.ReferenceWell(),
            OgPaySummaryWorkspace.ReferenceZones(),
            OgPaySummaryWorkspace.ReferenceParameters());

        Assert.True(run.Success, run.Message);
        var result = run.Value!;

        var a = Assert.Single(result.Zones, zone => zone.Name == "A");
        var b = Assert.Single(result.Zones, zone => zone.Name == "B");

        // AC-1: the same values the library's own test asserts.
        Assert.Equal(5.0, a.Gross, 9);
        Assert.Equal(5.0, a.Logged, 9);
        Assert.Equal(3.0, a.NetReservoir, 9);
        Assert.Equal(2.0, a.NetPay, 9);
        Assert.Equal(0.6, a.NetToGross, 9);
        Assert.Equal(0.1875, a.Porosity!.Value, 9);
        Assert.Equal(0.1475 / 0.375, a.Sw!.Value, 9);
        Assert.Equal(0.25, a.Vsh!.Value, 9);
        Assert.Equal(0.2275, a.HydrocarbonColumn, 9);
        Assert.Null(a.GrossTvd);

        Assert.Equal(4.0, b.Gross, 9);
        Assert.Equal(0.5, b.NetReservoir, 9);
        Assert.Equal(0.5, b.NetPay, 9);

        Assert.False(a.Excluded);
        Assert.Equal("Vsh <= 0.4, PHI >= 0.1, Sw <= 0.6", a.Cutoffs);
        Assert.Contains("N/G 0.389", result.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_curve_excludes_the_zone_and_names_the_curve()
    {
        // A well with porosity and saturation but neither GR nor VSH: Vsh cannot be computed.
        var well = OgPaySummaryWorkspace.ReferenceWell();
        var set = well.LogSets[0];
        set.Curves.RemoveAll(curve => curve.Mnemonic is "VSH" or "GR");

        var run = OgPaySummaryWorkspace.Compute(
            well,
            OgPaySummaryWorkspace.ReferenceZones(),
            OgPaySummaryWorkspace.ReferenceParameters());

        Assert.True(run.Success, run.Message);

        // AC-2: excluded, and the reason names the curve.
        Assert.All(run.Value!.Zones, zone =>
        {
            Assert.True(zone.Excluded);
            Assert.Equal(0, zone.NetReservoir);
            Assert.Equal(0, zone.NetPay);
            Assert.Contains("GR", zone.Note!, StringComparison.Ordinal);
        });

        Assert.Contains(run.Value!.Warnings, warning => warning.Contains("GR", StringComparison.Ordinal));
    }

    [Fact]
    public void Overlapping_zones_are_refused_with_both_names()
    {
        var run = OgPaySummaryWorkspace.Compute(
            OgPaySummaryWorkspace.ReferenceWell(),
            [new("A", 1000, 1006), new("B", 1005, 1009)],
            OgPaySummaryWorkspace.ReferenceParameters());

        Assert.False(run.Success);
        Assert.Equal(OgPaySummaryWorkspace.CodeOverlap, run.Reference);
        Assert.Contains("A", run.Message!, StringComparison.Ordinal);
        Assert.Contains("B", run.Message!, StringComparison.Ordinal);
        Assert.Contains("overlap", run.Message!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_gap_between_zones_is_refused_rather_than_silently_skipped()
    {
        var run = OgPaySummaryWorkspace.Compute(
            OgPaySummaryWorkspace.ReferenceWell(),
            [new("A", 1000, 1003), new("B", 1005, 1009)],
            OgPaySummaryWorkspace.ReferenceParameters());

        Assert.False(run.Success);
        Assert.Equal(OgPaySummaryWorkspace.CodeGap, run.Reference);
        Assert.Contains("not contiguous", run.Message!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_run_without_a_cutoff_is_refused_and_names_the_cutoff()
    {
        var run = OgPaySummaryWorkspace.Compute(
            OgPaySummaryWorkspace.ReferenceWell(),
            OgPaySummaryWorkspace.ReferenceZones(),
            new OgPayParameters(new OgPayCutoffs(VshMax: null, PorosityMin: 0.1, SwMax: 0.6)));

        Assert.False(run.Success);
        Assert.Equal(OgPaySummaryWorkspace.CodeCutoffs, run.Reference);
        Assert.Contains("Vsh cutoff", run.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_last_zone_must_end_at_the_log_bottom()
    {
        // One zone that stops short of the log's last sample: the boundary cannot come from a top.
        var run = OgPaySummaryWorkspace.Compute(
            OgPaySummaryWorkspace.ReferenceWell(),
            [new("A", 1000, 1007)],
            OgPaySummaryWorkspace.ReferenceParameters());

        Assert.False(run.Success);
        Assert.Equal(OgPaySummaryWorkspace.CodeGap, run.Reference);
    }

    [Fact]
    public void The_csv_uses_the_library_column_order_and_values()
    {
        var run = OgPaySummaryWorkspace.Compute(
            OgPaySummaryWorkspace.ReferenceWell(),
            OgPaySummaryWorkspace.ReferenceZones(),
            OgPaySummaryWorkspace.ReferenceParameters());

        var csv = OgPaySummaryWorkspace.ToCsv(run.Value!, OgPaySummaryWorkspace.ReferenceZones());
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.StartsWith("zone,top,base,gross", lines[0], StringComparison.Ordinal);
        Assert.Contains("A,1000,1005,5,5,3,2,", lines[1], StringComparison.Ordinal);
        Assert.Contains("Vsh <= 0.4", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_xlsx_export_reads_back_with_the_grids_values()
    {
        var run = OgPaySummaryWorkspace.Compute(
            OgPaySummaryWorkspace.ReferenceWell(),
            OgPaySummaryWorkspace.ReferenceZones(),
            OgPaySummaryWorkspace.ReferenceParameters());

        var directory = Path.Combine(OgPaths.DataRoot, "pay-" + Guid.NewGuid().ToString("n")[..8]);
        var path = Path.Combine(directory, "pay.xlsx");

        var written = OgPaySummaryWorkspace.ExportXlsx(
            run.Value!,
            OgPaySummaryWorkspace.ReferenceZones(),
            path);

        Assert.True(written.Success, written.Message);
        Assert.True(File.Exists(path));

        // AC-6: the workbook carries the same rows and numbers as the grid.
        var rows = await new ExcelTabularSource(File.ReadAllBytes(path))
            .QueryAsync("Pay summary", new Dictionary<string, object?>());

        Assert.Equal(run.Value!.Zones.Count, rows.Count);
        Assert.Equal("A", Convert.ToString(rows[0]["Zone"], System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(
            3.0,
            Convert.ToDouble(rows[0]["Net reservoir"], System.Globalization.CultureInfo.InvariantCulture),
            9);

        // A second write without overwrite is refused.
        var again = OgPaySummaryWorkspace.ExportXlsx(
            run.Value!,
            OgPaySummaryWorkspace.ReferenceZones(),
            path);

        Assert.False(again.Success);
        Assert.Equal("pay-export-exists", again.Reference);
    }

    [Fact]
    public void A_depth_shift_is_audited_with_the_actor_and_can_be_undone()
    {
        var well = OgPaySummaryWorkspace.ReferenceWell();
        var set = well.LogSets[0];
        Assert.False(string.IsNullOrWhiteSpace(set.Id));

        var session = new LogEditSession(well, "tester");
        var before = set.Index.ToArray();

        session.DepthShift(set.Id, [(1000.0, 1002.0)], reason: "tie to the reference run");

        Assert.Contains(
            well.Audit,
            entry => entry.Author == "tester"
                     && entry.EntityType == "LogSet"
                     && entry.Field == "depthShift"
                     && entry.Reason == "tie to the reference run");

        Assert.True(session.CanUndo);
        Assert.True(session.Undo());
        Assert.Equal(before, well.LogSets[0].Index);
    }

    [Fact]
    public void A_splice_records_both_runs_and_the_actor_and_can_be_undone()
    {
        var well = new WellModel();

        var first = Run("run 1", 1000, 1010, depth => depth - 1000);
        var second = Run("run 2", 1006, 1020, depth => 100 + depth - 1006);
        well.LogSets.Add(first);
        well.LogSets.Add(second);

        var session = new LogEditSession(well, "tester");
        var composite = session.Splice([first.Id, second.Id], reason: "tie the runs");

        // AC-5: both intervals, splice depths and the actor are recorded.
        Assert.Contains("run 1", composite.Extra["splicedFrom"], StringComparison.Ordinal);
        Assert.Contains("run 2", composite.Extra["splicedFrom"], StringComparison.Ordinal);
        Assert.Contains(
            well.Audit,
            entry => entry.Author == "tester"
                     && entry.EntityType == "LogSet"
                     && entry.After!.Contains("run 1", StringComparison.Ordinal)
                     && entry.After.Contains("run 2", StringComparison.Ordinal)
                     && entry.Reason == "tie the runs");

        Assert.True(session.Undo());
        Assert.DoesNotContain(well.LogSets, set => set.Id == composite.Id);
    }

    private static LogSet Run(string name, double top, double bottom, Func<double, double> gr)
    {
        var index = Enumerable.Range(0, (int)(bottom - top) + 1).Select(i => top + i).ToArray();
        var set = new LogSet { Name = name, IndexUnit = "m", Index = index };

        set.Curves.Add(new LogCurveData
        {
            Mnemonic = "GR",
            Unit = "gAPI",
            Values = index.Select(depth => (double?)gr(depth)).ToArray(),
        });

        return set;
    }
}
