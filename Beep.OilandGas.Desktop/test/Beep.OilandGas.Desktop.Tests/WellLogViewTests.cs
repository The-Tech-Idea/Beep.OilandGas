using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.WellLogs;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-04: a triple-combo log with a curve list, an undoable shift, a splice that records both intervals,
/// crossplots produced by the tools, and an honest "no data" for image and cement logs.
/// </summary>
public sealed class WellLogViewTests
{
    [Fact]
    public void The_demo_log_has_the_curves_a_triple_combo_and_the_crossplots_need()
    {
        var model = OgLogDemo.BuildWell(samples: 200);
        var set = Assert.Single(model.LogSets);

        Assert.Contains(set.Curves, curve => curve.Mnemonic == "GR");
        Assert.Contains(set.Curves, curve => curve.Mnemonic == "RT");
        Assert.Contains(set.Curves, curve => curve.Mnemonic == "RHOB");
        Assert.Contains(set.Curves, curve => curve.Mnemonic == "NPHI");
        Assert.Equal(200, set.Index.Length);
    }

    [Fact]
    public void A_triple_combo_document_has_more_than_one_track_and_a_curve_list()
    {
        UseDefaultEnvironment();

        var document = WellLogTemplates.CreateTripleComboDemo();

        Assert.True(document.Tracks.Count > 1, $"Only {document.Tracks.Count} track(s).");
        Assert.True(document.Axis.Bottom > document.Axis.Top);

        var curves = document.Tracks.OfType<WellLogTrack>().SelectMany(track => track.Curves).ToList();
        Assert.NotEmpty(curves);
        Assert.All(curves, curve => Assert.False(string.IsNullOrWhiteSpace(curve.Mnemonic)));

        // AC-1: the triple combo carries a gamma ray, a resistivity and a porosity curve.
        Assert.Contains(curves, curve => curve.Mnemonic.StartsWith("GR", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(curves, curve =>
            curve.Mnemonic.Contains("RES", StringComparison.OrdinalIgnoreCase)
            || curve.Mnemonic.StartsWith("RT", StringComparison.OrdinalIgnoreCase)
            || curve.Mnemonic.Contains("ILD", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(curves, curve =>
            curve.Mnemonic.Contains("POR", StringComparison.OrdinalIgnoreCase)
            || curve.Mnemonic.Contains("NPHI", StringComparison.OrdinalIgnoreCase)
            || curve.Mnemonic.Contains("RHOB", StringComparison.OrdinalIgnoreCase)
            || curve.Mnemonic.Contains("PHI", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void A_curve_without_a_unit_is_flagged_rather_than_plotted_silently()
    {
        UseDefaultEnvironment();

        var curve = Curve("GR", "", 2100f);
        var row = new OgLogCurveRow(
            curve.Mnemonic,
            curve.Unit,
            curve.Name,
            curve.MinimumValue,
            curve.MaximumValue,
            curve.ScaleType.ToString(),
            curve.Samples.Count,
            !string.IsNullOrWhiteSpace(curve.Unit));

        Assert.False(row.UnitKnown);
        Assert.Equal("unit unknown", row.UnitLabel);
    }

    [Fact]
    public void A_depth_shift_moves_the_curve_and_is_undone_from_the_history()
    {
        UseDefaultEnvironment();

        var curve = Curve("GR", "API", 2100f);
        var history = new OgLogEditHistory();
        var before = curve.Samples.Select(sample => sample.Depth).ToArray();

        var result = OgLogEditing.Shift(curve, 2f, "m", 2000f, 2300f, history);

        Assert.True(result.Success, result.Message);
        Assert.Equal(before[0] + 2f, curve.Samples[0].Depth, 3);
        Assert.Single(history.Entries);
        Assert.Equal(OgLogEditKind.Shift, history.Entries[0].Kind);

        Assert.True(history.Undo());
        Assert.Equal(before[0], curve.Samples[0].Depth, 3);
        Assert.Empty(history.Entries);
        Assert.NotNull(history.LastUndone);
    }

    [Fact]
    public void A_shift_outside_the_log_range_is_refused_with_the_range()
    {
        UseDefaultEnvironment();

        var curve = Curve("GR", "API", 2100f);
        var history = new OgLogEditHistory();

        var result = OgLogEditing.Shift(curve, 500f, "m", 2000f, 2300f, history);

        Assert.False(result.Success);
        Assert.Equal(OgLogEditing.CodeOutsideRange, result.Reference);
        Assert.Contains("2000", result.Message!, StringComparison.Ordinal);
        Assert.Contains("2300", result.Message!, StringComparison.Ordinal);
        Assert.Empty(history.Entries);
    }

    [Fact]
    public void A_zero_shift_is_refused_rather_than_recorded_as_an_edit()
    {
        UseDefaultEnvironment();

        var history = new OgLogEditHistory();
        var result = OgLogEditing.Shift(Curve("GR", "API", 2100f), 0f, "m", 2000f, 2300f, history);

        Assert.False(result.Success);
        Assert.Equal(OgLogEditing.CodeZeroShift, result.Reference);
        Assert.Empty(history.Entries);
    }

    [Fact]
    public void A_splice_records_both_the_source_and_the_target_interval_and_is_undoable()
    {
        UseDefaultEnvironment();

        var upper = Curve("GR", "API", 2100f);
        var lower = Curve("GR2", "API", 2200f);
        var curves = new List<WellLogCurve> { upper, lower };
        var history = new OgLogEditHistory();

        var result = OgLogEditing.Splice(curves, 0, 1, 2150f, "GR_SPLICE", history);

        Assert.True(result.Success, result.Message);

        var edit = Assert.Single(history.Entries);
        Assert.Equal(OgLogEditKind.Splice, edit.Kind);
        Assert.Contains("GR ", edit.SourceInterval!, StringComparison.Ordinal);
        Assert.Contains("GR2 ", edit.TargetInterval!, StringComparison.Ordinal);
        Assert.Contains("2150", edit.Detail, StringComparison.Ordinal);

        // The list now holds the spliced curve only.
        var spliced = Assert.Single(curves);
        Assert.Equal("GR_SPLICE", spliced.Mnemonic);
        Assert.Contains(spliced.Samples, sample => sample.Depth >= 2150f);

        Assert.True(history.Undo());
        Assert.Equal(2, curves.Count);
        Assert.Same(upper, curves[0]);
        Assert.Same(lower, curves[1]);
    }

    [Fact]
    public void A_splice_outside_the_curves_range_is_refused()
    {
        UseDefaultEnvironment();

        var curves = new List<WellLogCurve> { Curve("GR", "API", 2100f), Curve("GR2", "API", 2200f) };
        var result = OgLogEditing.Splice(curves, 0, 1, 9000f, "X", new OgLogEditHistory());

        Assert.False(result.Success);
        Assert.Equal(OgLogEditing.CodeSpliceOutside, result.Reference);
        Assert.Equal(2, curves.Count);
    }

    [Fact]
    public void Crossplots_come_from_the_tools_and_carry_a_real_cloud()
    {
        UseDefaultEnvironment();

        var set = Assert.Single(OgLogDemo.BuildWell(samples: 200).LogSets);

        var (densityNeutron, densityWarnings) = OgLogCrossplots.DensityNeutron(set);
        var (pickett, pickettWarnings) = OgLogCrossplots.Pickett(set);

        Assert.NotEmpty(densityNeutron.Points);
        Assert.NotEmpty(pickett.Points);
        Assert.Empty(densityWarnings);

        // Pickett has no PHIT/PHID in this set, so the tools derive porosity from density and say so. That is
        // an answer, not a failure — but it must be the *only* thing they had to report.
        Assert.Contains(pickettWarnings, warning =>
            warning.Contains("density porosity computed", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(pickettWarnings, warning =>
            warning.Contains("needs a resistivity", StringComparison.OrdinalIgnoreCase));

        // VSB-04 AC-5: the panel is the tools' type, not a desktop re-implementation.
        Assert.Equal("TheTechIdea.OilGas.WellLogs.CrossplotPanel", densityNeutron.GetType().FullName);
    }

    [Fact]
    public void A_log_set_without_the_right_curves_says_what_is_missing()
    {
        UseDefaultEnvironment();

        var set = new LogSet { Name = "gamma only", Index = [2100d, 2101d] };
        set.Curves.Add(new LogCurveData { Mnemonic = "GR", Unit = "API", Values = [40d, 42d] });

        var (panel, warnings) = OgLogCrossplots.DensityNeutron(set);

        Assert.Empty(panel.Points);
        Assert.Contains(warnings, warning => warning.Contains("neutron", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void A_document_with_image_and_cement_tracks_renders_them_and_stops_saying_no_data()
    {
        UseDefaultEnvironment();

        var document = OgLogDemo.WithImageAndCement();

        // The document really carries both presentations.
        var imageTrack = Assert.Single(document.Tracks.OfType<ImageLogTrack>());
        Assert.NotEmpty(imageTrack.Image.Rows);
        Assert.Equal(imageTrack.Image.Rows.Count, imageTrack.Image.Depths.Count);

        Assert.True(OgLogAvailability.CementTrackCount(document) >= 2,
            $"Only {OgLogAvailability.CementTrackCount(document)} cement track(s).");

        // AC-6: the "no data" sentences are gone, and each says what is there.
        Assert.NotEqual(OgLogAvailability.NoImageData, OgLogAvailability.Image(document));
        Assert.NotEqual(OgLogAvailability.NoCementData, OgLogAvailability.Cement(document));
        Assert.Contains("present", OgLogAvailability.Image(document), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("present", OgLogAvailability.Cement(document), StringComparison.OrdinalIgnoreCase);

        // And it draws: a non-empty frame through the same renderer the host uses.
        using var renderer = new ViewSurfaceRenderer
        {
            Surface = new TheTechIdea.OilGas.WellLogs.WellLogView { Document = document },
        };

        Assert.True(renderer.Render(480, 360));
        Assert.Contains(renderer.Frame!, value => value != 0);

        // The plain triple combo still reports no image/cement data, so the check is not simply always true.
        var plain = WellLogTemplates.CreateTripleComboDemo();
        Assert.Equal(OgLogAvailability.NoImageData, OgLogAvailability.Image(plain));
        Assert.Equal(OgLogAvailability.NoCementData, OgLogAvailability.Cement(plain));
    }

    [Fact]
    public void Image_and_cement_displays_say_no_data_when_the_tracks_are_absent()
    {
        UseDefaultEnvironment();

        var document = WellLogTemplates.CreateTripleComboDemo();

        Assert.Equal(OgLogAvailability.NoImageData, OgLogAvailability.Image(document));
        Assert.Equal(OgLogAvailability.NoCementData, OgLogAvailability.Cement(document));
    }

    [Fact]
    public void The_default_standard_is_fgdc_and_an_unknown_one_falls_back_to_it()
    {
        Assert.NotEmpty(OgLogStandards.Available);

        var known = OgLogStandards.Resolve(OgLogStandards.Available[0]);
        Assert.Equal(OgLogStandards.Fgdc, known.Standard);
        Assert.Null(known.Notice);

        var unknown = OgLogStandards.Resolve("NOT-A-STANDARD");
        Assert.Equal(OgLogStandards.Fgdc, unknown.Standard);
        Assert.Contains("not a standard", unknown.Notice!, StringComparison.OrdinalIgnoreCase);

        Assert.Equal((OgLogStandards.Fgdc, null), OgLogStandards.Resolve(null));
    }

    [Fact]
    public void The_model_path_produces_a_log_document_with_tracks()
    {
        UseDefaultEnvironment();

        var model = OgLogDemo.BuildWell(samples: 120);
        var document = WellLogDocuments.FromModel(model);

        Assert.True(document.Tracks.Count > 1, $"Only {document.Tracks.Count} track(s).");
        Assert.Contains(document.Tracks.OfType<WellLogTrack>().SelectMany(track => track.Curves),
            curve => curve.Mnemonic == "GR");
    }

    private static WellLogCurve Curve(string mnemonic, string unit, float start)
    {
        var curve = new WellLogCurve { Mnemonic = mnemonic, Unit = unit, Name = mnemonic + " curve" };

        for (var i = 0; i < 50; i++)
        {
            curve.Samples.Add(new WellLogCurveSample { Depth = start + i, Value = 40 + (i % 7) });
        }

        curve.MinimumValue = 40;
        curve.MaximumValue = 50;

        return curve;
    }

    [Fact]
    public void The_shipped_lithology_standards_include_gost_and_a_company_sample()
    {
        // AC-2: what the page can choose from. FGDC is the built-in library; the other two are layers.
        Assert.Contains(OgLogStandards.Fgdc, OgLogStandards.Available);
        Assert.Contains(OgLogStandards.Gost, OgLogStandards.Available);
        Assert.Contains(OgLogStandards.SampleCompany, OgLogStandards.Available);
    }

    [Fact]
    public void Applying_the_sample_standard_changes_the_drawing_and_fgdc_restores_it()
    {
        UseDefaultEnvironment();
        var directory = Path.Combine(OgPaths.DataRoot, "standards-" + Guid.NewGuid().ToString("n")[..8]);

        try
        {
            OgLogStandards.Apply(OgLogStandards.Fgdc, directory);
            var before = OgLogStandards.Drawing("sand");
            Assert.Equal(OgLogStandards.Fgdc, OgLogStandards.Active);

            OgLogStandards.Apply(OgLogStandards.SampleCompany, directory);
            var after = OgLogStandards.Drawing("sand");

            // AC-2: the same lithology code draws differently under the company standard.
            Assert.NotEqual(before, after);
            Assert.Contains("sample-company", OgLogStandards.Active, StringComparison.OrdinalIgnoreCase);

            OgLogStandards.Apply(OgLogStandards.Fgdc, directory);
            Assert.Equal(before, OgLogStandards.Drawing("sand"));
        }
        finally
        {
            OgLogStandards.Apply(OgLogStandards.Fgdc, directory);
        }
    }

    [Fact]
    public void The_rendered_lithology_changes_between_standards_and_returns_on_restore()
    {
        UseDefaultEnvironment();
        var directory = Path.Combine(OgPaths.DataRoot, "standards-" + Guid.NewGuid().ToString("n")[..8]);

        try
        {
            OgLogStandards.Apply(OgLogStandards.Fgdc, directory);
            var fgdc = Render(WellLogTemplates.CreateTripleComboDemo());

            OgLogStandards.Apply(OgLogStandards.SampleCompany, directory);
            var sample = Render(WellLogTemplates.CreateTripleComboDemo());

            // The picture changes because the fills change, not because the data changed.
            Assert.False(fgdc.AsSpan().SequenceEqual(sample), "The lithology fills did not change the drawing.");

            OgLogStandards.Apply(OgLogStandards.Fgdc, directory);
            var restored = Render(WellLogTemplates.CreateTripleComboDemo());

            Assert.True(fgdc.AsSpan().SequenceEqual(restored), "Restoring FGDC did not restore the drawing.");
        }
        finally
        {
            OgLogStandards.Apply(OgLogStandards.Fgdc, directory);
        }
    }

    [Fact]
    public void Gost_is_accepted_and_says_its_drawings_are_not_supplied()
    {
        UseDefaultEnvironment();
        var directory = Path.Combine(OgPaths.DataRoot, "standards-" + Guid.NewGuid().ToString("n")[..8]);

        try
        {
            OgLogStandards.Apply(OgLogStandards.Fgdc, directory);
            var before = OgLogStandards.Drawing("sand");

            var (standard, notice) = OgLogStandards.Resolve(OgLogStandards.Gost);
            OgLogStandards.Apply(OgLogStandards.Gost, directory);

            Assert.Equal(OgLogStandards.Gost, standard);

            // Honest: GOST ships numbers and names, not drawings, so the built-in pattern stays.
            Assert.NotNull(notice);
            Assert.Contains("overrides", notice!, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(before, OgLogStandards.Drawing("sand"));
        }
        finally
        {
            OgLogStandards.Apply(OgLogStandards.Fgdc, directory);
        }
    }

    [Fact]
    public void An_unknown_standard_falls_back_to_fgdc_with_a_notice()
    {
        var (standard, notice) = OgLogStandards.Resolve("Not-A-Standard");

        Assert.Equal(OgLogStandards.Fgdc, standard);
        Assert.Contains("FGDC", notice!, StringComparison.Ordinal);
    }

    private static byte[] Render(WellboreDocument document)
    {
        using var renderer = new ViewSurfaceRenderer
        {
            Surface = new WellLogView { Document = document },
        };

        Assert.True(renderer.Render(480, 360));
        return renderer.Frame!.ToArray();
    }

    private static void UseDefaultEnvironment()
    {
        // Surfaces and templates read the environment when they are constructed (VSB-02).
        WellTools.Environment.OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
    }
}
