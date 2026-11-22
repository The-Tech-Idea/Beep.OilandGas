using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-06: three demo wells correlate on a chosen marker, the one that lacks the datum is marked rather than
/// silently aligned, the checks list it, the panel renders and the PDF export paginates.
/// </summary>
public sealed class CorrelationViewTests
{
    [Fact]
    public void The_demo_set_has_three_wells_and_the_datum_is_shared_by_two_of_them()
    {
        UseDefaultEnvironment();

        var wells = OgCorrelation.DemoWells();

        Assert.Equal(3, wells.Count);
        Assert.All(wells, well => Assert.NotEmpty(well.Document.Markers));

        var panel = OgCorrelation.Build(wells, OgCorrelation.DemoDatum);

        // AC-1: two wells carry the datum, so the panel can flatten on it.
        Assert.Equal(2, panel.Wells.Count(well =>
            MarkerMatching.Find(well.Document.Markers, OgCorrelation.DemoDatum) is not null));
        Assert.Equal(OgCorrelation.DemoDatum, panel.FlattenOn);
    }

    [Fact]
    public void Wells_that_carry_the_datum_land_on_the_same_axis_position()
    {
        UseDefaultEnvironment();

        var panel = OgCorrelation.Build(OgCorrelation.DemoWells(), OgCorrelation.DemoDatum);

        // AC-1: after flattening, each well's datum sits at the same axis value — that is what "aligned on a
        // marker" means, and it is the tools' own offset arithmetic.
        var first = OgCorrelation.DatumPositionAfterFlatten(panel, "DEMO-1");
        var second = OgCorrelation.DatumPositionAfterFlatten(panel, "DEMO-2");

        Assert.False(float.IsNaN(first));
        Assert.Equal(first, second, 1);

        // The offsets differ, because the datum sits at a different depth in each well.
        var columns = OgCorrelation.Columns(panel);
        var one = columns.Single(row => row.Title == "DEMO-1");
        var two = columns.Single(row => row.Title == "DEMO-2");
        Assert.NotEqual(one.Offset, two.Offset);
        Assert.True(one.HasDatum && two.HasDatum);
    }

    [Fact]
    public void A_well_without_the_datum_is_marked_and_not_aligned()
    {
        UseDefaultEnvironment();

        var panel = OgCorrelation.Build(OgCorrelation.DemoWells(), OgCorrelation.DemoDatum);
        var checks = OgCorrelation.Checks(panel, OgCorrelation.DemoDatum);

        // AC-2: named, explained, and its offset stays zero so the drawing shows it out of line.
        var issue = Assert.Single(checks, item => item.Code == OgCorrelation.CodeDatumMissing);
        Assert.Equal("DEMO-3", issue.Well);
        Assert.Contains(OgCorrelation.DemoDatum, issue.Message, StringComparison.Ordinal);

        var third = OgCorrelation.Columns(panel).Single(row => row.Title == "DEMO-3");
        Assert.False(third.HasDatum);
        Assert.Equal(0f, third.Offset, 3);

        // Its datum position is not computable, which is the honest answer for a well without the marker.
        Assert.True(float.IsNaN(OgCorrelation.DatumPositionAfterFlatten(panel, "DEMO-3")));
    }

    [Fact]
    public void A_well_without_any_tops_is_listed_rather_than_drawn_blank()
    {
        UseDefaultEnvironment();

        var wells = OgCorrelation.DemoWells().ToList();
        wells[0].Document.Markers.Clear();

        var panel = OgCorrelation.Build(wells, OgCorrelation.DemoDatum);
        var checks = OgCorrelation.Checks(panel, OgCorrelation.DemoDatum);

        Assert.Contains(checks, issue => issue.Code == OgCorrelation.CodeNoTops && issue.Well == "DEMO-1");
    }

    [Fact]
    public void One_well_cannot_be_correlated_and_says_so()
    {
        UseDefaultEnvironment();

        var panel = OgCorrelation.Build([OgCorrelation.DemoWells()[0]], OgCorrelation.DemoDatum);
        var checks = OgCorrelation.Checks(panel, OgCorrelation.DemoDatum);

        Assert.Contains(checks, issue => issue.Code == OgCorrelation.CodeSingleWell);
    }

    [Fact]
    public void The_panel_renders_a_non_empty_frame()
    {
        UseDefaultEnvironment();

        var panel = OgCorrelation.Build(OgCorrelation.DemoWells(), OgCorrelation.DemoDatum);

        var frame = OgCorrelation.Render(panel, 600, 400);

        Assert.NotNull(frame);
        Assert.Equal(600 * 400 * 4, frame!.Length);
        Assert.Contains(frame, value => value != 0);
    }

    [Fact]
    public void The_summary_counts_wells_shared_tops_and_problems()
    {
        UseDefaultEnvironment();

        var panel = OgCorrelation.Build(OgCorrelation.DemoWells(), OgCorrelation.DemoDatum);
        var checks = OgCorrelation.Checks(panel, OgCorrelation.DemoDatum);
        var summary = OgCorrelation.Summarise(panel, checks);

        Assert.Equal(3, summary.Wells);
        Assert.True(summary.SharedTops >= 2, $"Only {summary.SharedTops} shared top(s).");
        Assert.Equal(1, summary.MissingDatum);
        Assert.Equal(0, summary.WithoutTops);
        Assert.Equal(OgCorrelation.DemoDatum, summary.Datum);
    }

    [Fact]
    public void The_panel_exports_as_a_paginated_pdf()
    {
        UseDefaultEnvironment();

        var panel = OgCorrelation.Build(OgCorrelation.DemoWells(), OgCorrelation.DemoDatum);
        var pages = OgCorrelation.Pages(panel);

        // AC-5: the planner's page count is what the exporter writes.
        Assert.NotEmpty(pages);

        var directory = Path.Combine(OgPaths.DataRoot, "corr-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "correlation.pdf");

        int written;
        using (var stream = File.Create(path))
        {
            written = OgCorrelation.ExportPdf(panel, stream);
        }

        Assert.Equal(pages.Count, written);
        Assert.True(File.Exists(path));

        var header = new byte[5];
        using (var stream = File.OpenRead(path))
        {
            Assert.Equal(5, stream.Read(header, 0, 5));
        }

        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(header));
    }

    [Fact]
    public void Without_a_datum_the_panel_does_not_claim_to_be_aligned()
    {
        UseDefaultEnvironment();

        var panel = OgCorrelation.Build(OgCorrelation.DemoWells(), datum: null);
        var checks = OgCorrelation.Checks(panel, datum: null);

        Assert.Null(panel.FlattenOn);
        Assert.DoesNotContain(checks, issue => issue.Code == OgCorrelation.CodeDatumMissing);

        var columns = OgCorrelation.Columns(panel);
        Assert.All(columns, row => Assert.False(row.HasDatum));
    }

    private static void UseDefaultEnvironment()
    {
        OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
    }
}
