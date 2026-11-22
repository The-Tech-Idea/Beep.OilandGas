using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.WellSchematic;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-05: casing and completion are drawn with their depths, annuli say "not computed" when MAASP is
/// absent, an unverified barrier changes the picture, an unknown component type is warned about, validation
/// findings name their element, and the tally is the casing record.
/// </summary>
public sealed class WellSchematicViewTests
{
    [Fact]
    public void The_demo_schematic_carries_casing_and_completion_strings_with_ordered_depths()
    {
        var document = OgSchematicMapper.Demo();
        var strings = OgSchematicMapper.Strings(document);

        Assert.NotEmpty(strings);
        Assert.All(strings, row => Assert.True(row.Bottom > row.Top, $"{row.Name}: {row.Top}..{row.Bottom}"));

        // AC-1: both a casing string and a completion component are present.
        Assert.Contains(strings, row => row.Symbol.Contains("casing", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(strings, row => row.Symbol.Contains("tubing", StringComparison.OrdinalIgnoreCase)
                                        || row.Symbol.Contains("completion", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Annuli_report_MAASP_or_say_not_computed()
    {
        var document = OgSchematicMapper.Demo();
        var annuli = OgSchematicMapper.Annuli(document);

        Assert.NotEmpty(annuli);

        // AC-2: a value or the exact phrase — never a blank cell.
        Assert.All(annuli, row =>
        {
            Assert.False(string.IsNullOrWhiteSpace(row.Maasp));
            Assert.True(
                row.Maasp == OgSchematicMapper.NotComputed || row.Maasp.Contains(' '),
                $"'{row.Maasp}' is neither a pressure nor '{OgSchematicMapper.NotComputed}'.");
        });
    }

    [Fact]
    public void Every_barrier_says_whether_it_has_evidence()
    {
        var document = OgSchematicMapper.Demo();
        var barriers = OgSchematicMapper.Barriers(document);

        Assert.NotEmpty(barriers);
        Assert.All(barriers, row =>
        {
            Assert.False(string.IsNullOrWhiteSpace(row.Element));
            Assert.False(string.IsNullOrWhiteSpace(row.Evidence));
            Assert.Equal(row.Verified, row.Evidence != "no evidence recorded");
        });
    }

    [Fact]
    public void An_unverified_barrier_is_marked_in_the_panel_and_counted_in_the_summary()
    {
        UseDefaultEnvironment();

        var unverified = OgSchematicMapper.Demo();

        var element = unverified.Barriers?.Envelopes.FirstOrDefault()?.Elements.FirstOrDefault();
        Assert.NotNull(element);
        element!.Status = BarrierElementStatus.Unverified;
        element.VerifiedDate = null;
        element.VerificationMethod = null;
        element.AcceptanceRef = null;

        var rows = OgSchematicMapper.Barriers(unverified);
        var summary = OgSchematicMapper.Summarise(unverified);

        Assert.False(rows[0].Verified);
        Assert.Equal("no evidence recorded", rows[0].Evidence);

        // The tools' production barrier template ships without verification evidence, so the count is the
        // number of unverified rows — and it must be at least the one just cleared.
        Assert.Equal(rows.Count(row => !row.Verified), summary.UnverifiedBarriers);
        Assert.True(summary.UnverifiedBarriers >= 1);
    }

    [Fact]
    public void The_overlay_hook_paints_over_the_surface()
    {
        UseDefaultEnvironment();

        var view = new WellSchematicView { Schematic = OgSchematicMapper.Demo() };
        using var renderer = new ViewSurfaceRenderer { Surface = view };

        Assert.True(renderer.Render(200, 150));
        var plain = renderer.Frame!.ToArray();

        // The bluntest possible overlay: if this does not change the frame, the hook is broken, not the math.
        renderer.Overlay = canvas => canvas.Clear(new SkiaSharp.SKColor(255, 0, 0));
        Assert.True(renderer.Render(200, 150));

        Assert.False(plain.AsSpan().SequenceEqual(renderer.Frame!), "The overlay hook did not paint at all.");
    }

    [Fact]
    public void The_unverified_overlay_marks_each_barrier_without_evidence_and_leaves_verified_ones_alone()
    {
        UseDefaultEnvironment();

        var document = OgSchematicMapper.Demo();
        var view = new WellSchematicView { Schematic = document };
        using var renderer = new ViewSurfaceRenderer { Surface = view };

        // One renderer for every frame: the earlier two-instance comparison was inconclusive.
        Assert.True(renderer.Render(420, 320));
        var plain = renderer.Frame!.ToArray();

        var marks = OgSchematicBarrierOverlay.Unverified(document, view.LastPlan);
        Assert.NotEmpty(marks);
        Assert.All(marks, mark => Assert.True(mark.Bottom > mark.Top, $"{mark.Element}: {mark.Top}..{mark.Bottom}"));

        // A no-op overlay must not change the frame: any difference below comes from the hatch itself.
        renderer.Overlay = _ => { };
        Assert.True(renderer.Render(420, 320));
        Assert.True(plain.AsSpan().SequenceEqual(renderer.Frame!), "An empty overlay changed the frame.");

        var painted = 0;
        renderer.Overlay = canvas =>
            painted = OgSchematicBarrierOverlay.Draw(canvas, document, marks);
        Assert.True(renderer.Render(420, 320));

        Assert.True(painted > 0, $"Draw painted nothing from {marks.Count} mark(s).");
        Assert.False(
            plain.AsSpan().SequenceEqual(renderer.Frame!),
            "The hatched overlay did not change the frame.");

        // And with nothing unverified there is nothing to draw.
        foreach (var element in document.Barriers!.Envelopes.SelectMany(envelope => envelope.Elements))
        {
            element.VerificationMethod = "pressure test";
            element.VerifiedDate = DateTime.UtcNow;
        }

        Assert.Empty(OgSchematicBarrierOverlay.Unverified(document, view.LastPlan));
    }

    [Fact]
    public void A_verified_barrier_is_not_marked()
    {
        UseDefaultEnvironment();

        var document = OgSchematicMapper.Demo();

        foreach (var element in document.Barriers!.Envelopes.SelectMany(envelope => envelope.Elements))
        {
            element.VerificationMethod = "pressure test";
            element.VerifiedDate = new DateTime(2026, 1, 2);
        }

        var marks = OgSchematicBarrierOverlay.Unverified(document, plan: null);

        Assert.Empty(marks);
        Assert.All(OgSchematicMapper.Barriers(document), row => Assert.True(row.Verified));
    }

    [Fact]
    public void An_unknown_component_type_is_warned_about_and_the_well_still_renders()
    {
        UseDefaultEnvironment();

        var document = OgSchematicMapper.Demo();
        document.Equipment.Add(new SchematicEquipment
        {
            SymbolId = string.Empty,
            ComponentType = "NOT_A_REAL_COMPONENT",
            TopDepth = document.TopDepth + 100,
        });

        var warnings = OgSchematicMapper.Warnings(document);

        Assert.Contains(warnings, warning => warning.Contains("NOT_A_REAL_COMPONENT", StringComparison.Ordinal));

        using var renderer = new ViewSurfaceRenderer { Surface = new WellSchematicView { Schematic = document } };
        Assert.True(renderer.Render(420, 320));
        Assert.Contains(renderer.Frame!, value => value != 0);
    }

    [Fact]
    public void The_demo_document_has_no_unknown_symbols_of_its_own()
    {
        // A warning on the demo would mean the mapping is wrong, not that the check works.
        Assert.Empty(OgSchematicMapper.Warnings(OgSchematicMapper.Demo()));
    }

    [Fact]
    public void A_broken_string_is_reported_with_its_element()
    {
        var document = OgSchematicMapper.Demo();
        var broken = document.Strings[0];
        broken.BottomDepth = broken.TopDepth - 10;

        var findings = OgSchematicMapper.Findings(document);

        // AC-5: the finding is listed and names the element it concerns.
        Assert.Contains(findings, issue =>
            string.Equals(issue.ItemId, broken.Id, StringComparison.Ordinal));
    }

    [Fact]
    public void The_tally_is_the_casing_record()
    {
        var document = OgSchematicMapper.Demo();
        var tally = OgSchematicMapper.Tally(document);
        var strings = OgSchematicMapper.Strings(document);

        Assert.NotNull(tally);
        Assert.NotEmpty(tally.Rows);

        // AC-6: one tally row per string the program includes, keyed by that string's id, and its depth
        // cells are the string's own depths.
        Assert.Equal(tally.RowIds.Count, tally.Rows.Count);
        Assert.All(tally.RowIds, id => Assert.Contains(strings, row => row.Id == id));

        var topColumn = tally.Columns.ToList().FindIndex(column => column.key == "top");
        var bottomColumn = tally.Columns.ToList().FindIndex(column => column.key == "bottom");
        Assert.True(topColumn >= 0 && bottomColumn >= 0);

        for (var i = 0; i < tally.Rows.Count; i++)
        {
            var row = strings.Single(item => item.Id == tally.RowIds[i]);
            var cells = tally.Rows[i];

            Assert.Equal(row.Top.ToString("0.#"), cells[topColumn]);
            Assert.Equal(row.Bottom.ToString("0.#"), cells[bottomColumn]);
        }
    }

    [Fact]
    public void The_summary_counts_what_the_page_shows()
    {
        var document = OgSchematicMapper.Demo();
        var summary = OgSchematicMapper.Summarise(document);

        Assert.Equal(document.Strings.Count, summary.Strings);
        Assert.Equal(document.Equipment.Count, summary.Equipment);
        Assert.True(summary.Annuli > 0);
        Assert.True(summary.Barriers > 0);
        Assert.False(string.IsNullOrWhiteSpace(summary.DepthRange));
    }

    [Fact]
    public void The_schematic_view_builds_a_document_from_the_schematic()
    {
        UseDefaultEnvironment();

        var document = WellSchematicView.Build(OgSchematicMapper.Demo());

        Assert.NotEmpty(document.Tracks);
        Assert.True(document.Axis.Bottom > document.Axis.Top);
    }

    private static void UseDefaultEnvironment()
    {
        OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
    }
}
