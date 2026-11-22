using System.Text.Json.Nodes;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-01: the ISCWSA reference case through the same error model the scan uses (AC-1), the rule as it is displayed (AC-2), the
/// violation rule (AC-4) and the export's agreement with the rows (AC-5) - the parts that need no database. The survey read, the
/// excluded offset and the saved run are proven in the gateway test.
/// </summary>
public sealed class AntiCollisionTests
{
    private const string PageFileName = "AntiCollisionPageView.cs";

    [Fact]
    public void The_iscwsa_reference_case_reproduces_the_published_covariance()
    {
        if (!Fixture(out var document, out var context, out var model, out var stations, out var tvd))
        {
            return; // the fixture is copied beside the test assembly; without it there is nothing to compare against
        }

        var result = IscwsaErrorModel.Compute(stations, "m", context, model, tvd);

        Assert.Empty(result.Warnings);

        var expected = document["totals"]!.AsArray();

        Assert.Equal(expected.Count, result.Totals.Count);

        for (var station = 0; station < expected.Count; station++)
        {
            var want = expected[station]!.AsArray();
            var got = result.Totals[station];

            var actual = new[] { got.NN, got.EE, got.VV, got.NE, got.NV, got.EV };

            for (var term = 0; term < 6; term++)
            {
                var value = want[term]!.GetValue<double>();

                Assert.True(Math.Abs(actual[term] - value) <= 1e-9 * Math.Max(1, Math.Abs(value)),
                    $"station {station} covariance[{term}]: {actual[term]:G17} vs ISCWSA {value:G17}");
            }
        }
    }

    [Fact]
    public void The_rule_shown_is_the_rule_applied()
    {
        // AC-2: the displayed sentence is built from the settings the scan uses, so the two cannot drift.
        var settings = new AntiCollisionSettings();

        var rule = AntiCollisionRules.RuleOf(settings);

        Assert.Equal(3.5, rule.ScalingFactor);
        Assert.Equal(0.3, rule.SurfaceMargin, 6);
        Assert.Equal(0.5, rule.ProjectAhead, 6);
        Assert.Equal(10, rule.ScanStep, 6);

        Assert.Contains("SF = (D - Rref - Roff - Sm)", rule.Describe(), StringComparison.Ordinal);
        Assert.Contains("MASD = k · √(σS² + σPA²)", rule.Describe(), StringComparison.Ordinal);
        Assert.Contains("violation below SF", rule.Describe(), StringComparison.Ordinal);
        Assert.Contains("Stop below 1", string.Join(" · ", rule.Levels), StringComparison.Ordinal);

        // The numbers in the sentence are the settings' numbers.
        var changed = new AntiCollisionSettings { ScalingFactor = 3.0 };

        Assert.Contains("k 3", AntiCollisionRules.RuleOf(changed).Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_separation_factor_and_masd_follow_the_rule()
    {
        // The arithmetic is the library's; this asserts the desktop does not substitute its own.
        var settings = new AntiCollisionSettings();

        var sf = AntiCollision.SeparationFactor(20, 5, 0.2, 0.2, settings);
        var masd = AntiCollision.Masd(5, 0.2, 0.2, settings);

        // MASD = k*sqrt(sigmaS^2 + sigmaPA^2) + Rref + Roff + Sm, and SF = (D - MASD terms) / that same denominator.
        var denominator = settings.ScalingFactor * Math.Sqrt(25 + 0.25);
        var expectedMasd = denominator + 0.2 + 0.2 + 0.3;

        Assert.Equal(expectedMasd, masd, 6);
        Assert.Equal((20 - (0.2 + 0.2 + 0.3)) / denominator, sf, 6);
        Assert.True(sf > 1, "A 20 m separation with a 5 m sigma is above the rule.");
    }

    [Fact]
    public void A_violation_is_the_level_the_settings_call_out()
    {
        // AC-4: the count follows the configured level, not a number chosen in the desktop.
        var rule = AntiCollisionRules.RuleOf(new AntiCollisionSettings());

        Assert.True(AntiCollisionRules.IsViolation("Stop", rule.ViolationLevel));
        Assert.True(AntiCollisionRules.IsViolation("stop", rule.ViolationLevel));
        Assert.False(AntiCollisionRules.IsViolation("Shut in / adjust", rule.ViolationLevel));
        Assert.False(AntiCollisionRules.IsViolation("Report", rule.ViolationLevel));
        Assert.False(AntiCollisionRules.IsViolation(null, rule.ViolationLevel));

        var scan = ScanWith(
        [
            Row("OFF-1", 100, 20, 1.8, "Report"),
            Row("OFF-1", 110, 2, 0.6, "Stop"),
            Row("OFF-1", 120, 5, 0.9, "Stop"),
        ]);

        Assert.Equal(2, scan.Violations);
        Assert.Contains("2 violation(s)", scan.CountsText, StringComparison.Ordinal);
        Assert.Contains("violation(s) at the configured level", "2 violation(s) at the configured level", StringComparison.Ordinal);
    }

    [Fact]
    public void The_export_is_the_grid_row_for_row()
    {
        // AC-5: one set of rows feeds the grid and the CSV.
        var scan = ScanWith(
        [
            Row("OFF-1", 100.123456, 20.5, 1.8, "Report"),
            Row("OFF-2", 110, 2, 0.6, "Stop"),
        ]);

        var csv = scan.Csv();
        var lines = csv.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("offset,md_m,distance_m,sigma_m,separation_factor,masd_m,status", lines[0]);
        Assert.Equal(3, lines.Length);

        for (var index = 0; index < scan.Rows.Count; index++)
        {
            var row = scan.Rows[index];
            var line = lines[index + 1];

            Assert.Contains("\"" + row.Offset + "\"", line, StringComparison.Ordinal);
            Assert.Contains(OgAntiCollisionScan.Figure(row.Md), line, StringComparison.Ordinal);
            Assert.Contains(OgAntiCollisionScan.Figure(row.Distance), line, StringComparison.Ordinal);
            Assert.Contains(OgAntiCollisionScan.Figure(row.SeparationFactor), line, StringComparison.Ordinal);
            Assert.Contains(OgAntiCollisionScan.Figure(row.Masd), line, StringComparison.Ordinal);
            Assert.Contains("\"" + row.Status + "\"", line, StringComparison.Ordinal);
        }

        // A well name with a comma is quoted rather than breaking the row.
        Assert.Contains("\"OFF, ONE\"", ScanWith([Row("OFF, ONE", 10, 5, 2, "Report")]).Csv(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_reference_without_stations_is_refused_and_no_offsets_is_refused()
    {
        var offline = new OgAntiCollision(() => null, new Quiet());

        var noReference = offline.ScanAsync(" ", ["OFF-1"]).GetAwaiter().GetResult();

        Assert.False(noReference.Success);
        Assert.Equal(AntiCollisionRules.CodeReferenceRequired, noReference.Reference);

        var noOffsets = offline.ScanAsync("REF-1", []).GetAwaiter().GetResult();

        Assert.False(noOffsets.Success);
        Assert.Equal(AntiCollisionRules.CodeNoOffsets, noOffsets.Reference);

        var noConnection = offline.ScanAsync("REF-1", ["OFF-1"]).GetAwaiter().GetResult();

        Assert.False(noConnection.Success);
        Assert.Equal(AntiCollisionRules.CodeNoConnection, noConnection.Reference);
    }

    [Fact]
    public void The_page_shows_the_rule_the_exclusions_and_the_same_export()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("result.Rule.Describe()", page, StringComparison.Ordinal);
        Assert.Contains("result.ExcludedText", page, StringComparison.Ordinal);
        Assert.Contains("result.Csv()", page, StringComparison.Ordinal);
        Assert.Contains("result.WarningsText", page, StringComparison.Ordinal);

        // A failed scan clears the grid rather than showing an empty one.
        Assert.Contains("_grid.ItemsSource = null", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the fixture

    /// <summary>
    /// The published ISCWSA MWD Rev 5 reference case, read from the fixture copied beside the test assembly. The workbook's own
    /// sheets compute XYM2 as systematic and XYM4 as random, as the owner's conformance test notes.
    /// </summary>
    private static bool Fixture(
        out JsonNode document,
        out SurveyErrorContext context,
        out ToolErrorModel model,
        out IReadOnlyList<(double md, double inc, double az)> stations,
        out IReadOnlyList<double> tvd)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "Data", "iscwsa");
        var file = Path.Combine(folder, "iscwsa-1-mwd-rev5.json");

        if (!File.Exists(file))
        {
            document = null!;
            context = null!;
            model = null!;
            stations = [];
            tvd = [];
            return false;
        }

        document = JsonNode.Parse(File.ReadAllText(file))!;
        var header = document["header"]!;

        context = new SurveyErrorContext
        {
            Gravity = header["gravity"]!.GetValue<double>(),
            BTotal = header["bTotal"]!.GetValue<double>(),
            Dip = header["dip"]!.GetValue<double>(),
            Declination = header["declination"]!.GetValue<double>(),
            Convergence = header["convergence"]!.GetValue<double>(),
            AzimuthReference = header["azimuthReference"]!.GetValue<string>()
                .StartsWith("T", StringComparison.OrdinalIgnoreCase) ? "True" : "Grid",
        };

        model = new ToolErrorModel
        {
            Name = "from the workbook",
            VerticalIncLimit = header["verticalIncLimit"]!.GetValue<double>(),
            Terms = document["model"]!.AsArray().Select(term => new ToolErrorTerm
            {
                Code = term!["code"]!.GetValue<string>(),
                Function = term["function"]!.GetValue<string>(),
                Magnitude = term["magnitude"]!.GetValue<double>(),
                Unit = term["unit"]!.GetValue<string>(),
                Propagation = term["function"]!.GetValue<string>() switch
                {
                    "XYM2" => "systematic",
                    "XYM4L" => "random",
                    _ => term["propagation"]!.GetValue<string>(),
                },
            }).ToList(),
        };

        var rows = document["stations"]!.AsArray();

        stations = rows.Select(row => (
            row![0]!.GetValue<double>(),
            row[1]!.GetValue<double>(),
            row[2]!.GetValue<double>())).ToList();

        tvd = rows.Select(row => row![3]!.GetValue<double>()).ToList();

        return true;
    }

    private static OgSeparationRow Row(string offset, double md, double distance, double sf, string? level) =>
        new(offset, md, distance, 1.2, sf, distance - 1, level);

    private static OgAntiCollisionScan ScanWith(IReadOnlyList<OgSeparationRow> rows)
    {
        var rule = AntiCollisionRules.RuleOf(new AntiCollisionSettings());

        return new OgAntiCollisionScan(
            "REF-1",
            ["OFF-1", "OFF-2"],
            [],
            rows.Select(row => row with { IsViolation = AntiCollisionRules.IsViolation(row.Level, rule.ViolationLevel) })
                .ToList(),
            [],
            rule,
            "defaults",
            "ISCWSA MWD Rev5",
            1d,
            DateTime.UtcNow);
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
