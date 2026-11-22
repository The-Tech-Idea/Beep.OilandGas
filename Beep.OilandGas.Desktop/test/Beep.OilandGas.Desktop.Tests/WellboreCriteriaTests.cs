using System.Diagnostics;
using System.Windows;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// The remaining VSB-03 criteria that do not need PPDM data: unit switching, mapping warnings, RTL, and a
/// large-log timing check.
/// </summary>
public sealed class WellboreCriteriaTests
{
    private const double FeetToMetres = 0.3048;

    [Fact]
    public void Switching_the_depth_unit_keeps_the_same_interval()
    {
        UseDefaultEnvironment();
        var view = new WellboreView { Document = OgDocumentMapper.Create(OgTrackTemplate.TripleComboDemo) };

        Assert.Equal(DepthUnit.Feet, view.Document!.Axis.Unit);
        view.View.SetRange(view.Document.Axis, 8200f, 8250f);
        view.View.CursorDepth = 8225f;

        Assert.True(OgDocumentUnits.Switch(view, DepthUnit.Meters));

        Assert.Equal(DepthUnit.Meters, view.Document.Axis.Unit);

        var (top, bottom) = view.View.Window(view.Document.Axis);
        Assert.Equal(8200 * FeetToMetres, top, 1);
        Assert.Equal(8250 * FeetToMetres, bottom, 1);
        Assert.Equal(8225 * FeetToMetres, view.View.CursorDepth!.Value, 1);

        // Asking for the unit it already has is a no-op, not a second conversion.
        Assert.False(OgDocumentUnits.Switch(view, DepthUnit.Meters));
        Assert.Equal(8200 * FeetToMetres, view.View.Window(view.Document.Axis).top, 1);
    }

    [Fact]
    public void The_toggle_alternates_between_the_two_units()
    {
        Assert.Equal(DepthUnit.Meters, OgDocumentUnits.Toggle(DepthUnit.Feet));
        Assert.Equal(DepthUnit.Feet, OgDocumentUnits.Toggle(DepthUnit.Meters));
    }

    [Fact]
    public void Mapping_warnings_are_reported_without_blocking_the_document()
    {
        // A well with no depth log set still produces a document — and says why it is empty.
        var empty = OgDocumentMapper.FromWellModel(new WellModel());
        var warnings = OgDocumentMapper.Warnings(empty);

        Assert.NotEmpty(warnings);
        Assert.Equal(0, OgDocumentMapper.Summarise(empty).TrackCount);

        // A populated document produces no notice.
        Assert.Empty(OgDocumentMapper.Warnings(OgDocumentMapper.Create(OgTrackTemplate.TripleComboDemo)));
        Assert.Empty(OgDocumentMapper.Warnings(OgDocumentMapper.Create(OgTrackTemplate.DepthOnly)));
    }

    [Fact]
    public void A_depth_only_document_has_a_range_and_no_warning()
    {
        var document = OgDocumentMapper.Create(OgTrackTemplate.DepthOnly);
        var summary = OgDocumentMapper.Summarise(document);

        Assert.Equal(1, summary.TrackCount);
        Assert.True(summary.Bottom > summary.Top);
        Assert.Empty(OgDocumentMapper.Warnings(document));
    }

    [Fact]
    public void Right_to_left_does_not_change_the_rendered_plot()
    {
        RunOnSta(() =>
        {
            UseDefaultEnvironment();

            using var host = new SkiaViewSurfaceHost
            {
                Surface = new WellboreView { Document = OgDocumentMapper.Create(OgTrackTemplate.TripleComboDemo) },
            };

            host.Measure(new Size(300, 240));
            host.Arrange(new Rect(0, 0, 300, 240));

            host.FlowDirection = FlowDirection.LeftToRight;
            Assert.True(host.Redraw());
            var leftToRight = host.Frame!.ToArray();

            host.FlowDirection = FlowDirection.RightToLeft;
            Assert.True(host.Redraw());
            var rightToLeft = host.Frame!.ToArray();

            // A depth plot is geological, not textual: mirroring it would put the well on its head.
            Assert.Equal(leftToRight.Length, rightToLeft.Length);
            Assert.True(
                leftToRight.AsSpan().SequenceEqual(rightToLeft),
                "The RTL render differs from the LTR render, so the plot is being mirrored.");
        });
    }

    [Fact]
    public void Twenty_thousand_samples_render_within_budget()
    {
        UseDefaultEnvironment();

        var well = BuildLargeWell(20_000);
        var document = OgDocumentMapper.FromWellModel(well);

        using var renderer = new ViewSurfaceRenderer
        {
            Surface = new WellboreView { Document = document },
        };

        var stopwatch = Stopwatch.StartNew();
        Assert.True(renderer.Render(800, 600));
        stopwatch.Stop();

        Assert.True(renderer.Frame!.Any(value => value != 0), "The large log produced an empty frame.");

        // A generous ceiling: the point is that a 20 000-sample log completes, and the measured value is
        // printed when it does not. REL-04 owns the real budget.
        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(10),
            $"A 20 000-sample log took {stopwatch.ElapsedMilliseconds} ms to render at 800x600.");
    }

    private static WellModel BuildLargeWell(int samples)
    {
        var model = new WellModel();
        model.Header.Name = $"LARGE-{samples}";

        var logSet = new LogSet
        {
            Name = "Large log",
            IndexType = LogIndexType.Depth,
            IndexMnemonic = "DEPT",
            IndexUnit = "m",
        };

        var index = new double[samples];
        var gamma = new double?[samples];

        for (var i = 0; i < samples; i++)
        {
            index[i] = 2000d + (i * 0.5);
            gamma[i] = 40 + (25 * Math.Sin(i / 40d));
        }

        logSet.Index = index;
        logSet.Curves.Add(new LogCurveData { Mnemonic = "GR", Unit = "API", Values = gamma });
        model.LogSets.Add(logSet);

        return model;
    }

    private static void UseDefaultEnvironment()
    {
        OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
    }

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
