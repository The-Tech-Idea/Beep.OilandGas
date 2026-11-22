using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-06: the review comparison, the factor applicability rule, the unavailable-versus-zero text, the arithmetic refusal
/// and the export's agreement with the lines - the parts that need no database. The report over real rows is proven in the
/// gateway test.
/// </summary>
public sealed class HseGhgTests
{
    private const string PageFileName = "GhgPageView.cs";

    [Fact]
    public void A_report_needs_a_reviewed_catalog_and_says_why_when_it_has_none()
    {
        // AC-3: no selection is its own state, not "current".
        var (state, sentence) = GhgRules.ReviewOf(null, "rev-catalog-1", "rev-env-1");

        Assert.Equal(GhgRules.ReviewNotReviewed, state);
        Assert.Contains("not reviewed", sentence, StringComparison.OrdinalIgnoreCase);

        // No company environment at all is a different answer again.
        var (noEnvironment, noEnvironmentSentence) = GhgRules.ReviewOf(null, "rev-catalog-1", null);

        Assert.Equal(GhgRules.ReviewNoEnvironment, noEnvironment);
        Assert.Contains("no company environment", noEnvironmentSentence, StringComparison.OrdinalIgnoreCase);

        // The recorded revisions matching the current ones is the only "current" case.
        var selection = Selection("rev-catalog-1", "rev-env-1");
        var (current, currentSentence) = GhgRules.ReviewOf(selection, "rev-catalog-1", "rev-env-1");

        Assert.Equal(GhgRules.ReviewCurrent, current);
        Assert.Contains("reviewed", currentSentence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_changed_catalog_or_environment_forces_a_re_review()
    {
        // AC-3: a newer catalog revision must be reviewed before it is used, and both revisions are named.
        var (catalogState, catalogSentence) =
            GhgRules.ReviewOf(Selection("rev-catalog-1", "rev-env-1"), "rev-catalog-2", "rev-env-1");

        Assert.Equal(GhgRules.ReviewCatalogChanged, catalogState);
        Assert.Contains("rev-catalog-1", catalogSentence, StringComparison.Ordinal);
        Assert.Contains("rev-catalog-2", catalogSentence, StringComparison.Ordinal);
        Assert.Contains("Review it again", catalogSentence, StringComparison.Ordinal);

        // The same for an environment that moved after the review.
        var (environmentState, environmentSentence) =
            GhgRules.ReviewOf(Selection("rev-catalog-1", "rev-env-1"), "rev-catalog-1", "rev-env-2");

        Assert.Equal(GhgRules.ReviewEnvironmentChanged, environmentState);
        Assert.Contains("rev-env-1", environmentSentence, StringComparison.Ordinal);
        Assert.Contains("rev-env-2", environmentSentence, StringComparison.Ordinal);
    }

    [Fact]
    public void A_factor_applies_only_when_its_category_unit_and_period_all_match()
    {
        var factors = new List<GhgFactorDefinition>
        {
            new("F-1", "FLARE", "m3", "tCO2e", 0.002m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "EPA 40 CFR 98 Table C-1"),
            new("F-2", "FLARE", "scf", "tCO2e", 0.00005m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "EPA 40 CFR 98 Table C-1"),
            new("F-3", "VENT", "m3", "tCO2e", 0.001m,
                new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31), "EPA 40 CFR 98 Table W-1"),
        };

        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 12, 31);

        Assert.Equal("F-1", GhgRules.ApplicableFactor(factors, "FLARE", "m3", start, end)!.Id);

        // The category must match, the unit must match, and the period must be covered by the factor's window.
        Assert.Null(GhgRules.ApplicableFactor(factors, "VENT", "m3", start, end));
        Assert.Null(GhgRules.ApplicableFactor(factors, "FLARE", "bbl", start, end));
        Assert.Null(GhgRules.ApplicableFactor(factors, "FLARE", "m3",
            new DateTime(2027, 1, 1), new DateTime(2027, 12, 31)));

        // An activity with no recorded period cannot be matched, so nothing is assumed.
        Assert.Null(GhgRules.ApplicableFactor(factors, "FLARE", "m3", null, end));
        Assert.Null(GhgRules.ApplicableFactor(factors, "FLARE", "m3", start, null));
        Assert.Null(GhgRules.ApplicableFactor(factors, "", "m3", start, end));
    }

    [Fact]
    public void An_unavailable_report_prints_the_reason_and_never_a_zero()
    {
        // AC-1 and AC-5.
        var workspace = Unavailable([GhgRules.ReasonNoFactors, GhgRules.ReasonNoActivity]);

        Assert.False(workspace.IsAvailable);
        Assert.Equal("unavailable", workspace.TotalText);
        Assert.Contains("not zero", workspace.StatusText, StringComparison.Ordinal);
        Assert.Contains(GhgRules.ReasonNoFactors, workspace.UnavailableText, StringComparison.Ordinal);
        Assert.Contains(GhgRules.ReasonNoActivity, workspace.UnavailableText, StringComparison.Ordinal);
        Assert.DoesNotContain("0", workspace.TotalText, StringComparison.Ordinal);
        Assert.Equal("unavailable", workspace.LinesText);
        Assert.Equal("unavailable", workspace.ExportText);

        // A produced report prints a real total, which is why the two can never be confused.
        var available = workspace with
        {
            Report = new OgGhgReport(
                2026,
                [
                    new OgGhgLine("Flare · FLARE · 2026", "m3", 1_000m, 0.002m, "tCO2e", "rev-1", 2m, "F-1",
                        "EPA 40 CFR 98 Table C-1"),
                ],
                [new OgGhgTotal("tCO2e", 2m)],
                "rev-1",
                GhgRules.ReviewCurrent),
            Unavailable = [],
        };

        Assert.True(available.IsAvailable);
        Assert.Equal("2 tCO2e", available.TotalText);
        Assert.Equal(string.Empty, available.UnavailableText);
        Assert.Equal("report produced", available.StatusText);
    }

    [Fact]
    public void Every_line_names_its_factor_unit_source_and_revision()
    {
        // AC-2.
        var line = new OgGhgLine("Flare · FLARE · 2026-01-01 to 2026-12-31", "m3", 1_000m, 0.002m, "tCO2e",
            "rev-catalog-1", 2m, "F-1", "EPA 40 CFR 98 Table C-1");

        Assert.Contains("factor F-1", line.Describe(), StringComparison.Ordinal);
        Assert.Contains("tCO2e", line.Describe(), StringComparison.Ordinal);
        Assert.Contains("revision rev-catalog-1", line.Describe(), StringComparison.Ordinal);
        Assert.Contains("source EPA 40 CFR 98 Table C-1", line.Describe(), StringComparison.Ordinal);
        Assert.Contains("1000 m3", line.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_owners_arithmetic_refuses_an_unrepresentable_value()
    {
        // AC-4: the report multiplies through GhgArithmetic.Check, so its refusal is the report's refusal.
        var factor = new GhgFactorDefinition("F-1", "FLARE", "m3", "tCO2e", decimal.MaxValue,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "EPA 40 CFR 98 Table C-1");

        var fact = new GhgDispositionFact("METERED", "FLARE", "GAS", "ANNUAL", 1m, 0m, 1_000m, "m3",
            new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));

        var refused = Assert.Throws<InvalidOperationException>(() => GhgArithmetic.Check(fact, factor));

        Assert.Contains("exceed the supported calculation range", refused.Message, StringComparison.Ordinal);

        // A representable pair is multiplied exactly, without rounding it to a display width.
        var fine = GhgArithmetic.Check(fact, factor with { Factor = 0.002m });

        Assert.Equal(2m, fine.Result);
        Assert.Equal("tCO2e", fine.OutputUnit);
    }

    [Fact]
    public void The_export_matches_the_lines_and_the_total_exactly()
    {
        // AC-6: one formatting helper feeds the grid text and the CSV alike.
        var lines = new List<OgGhgLine>
        {
            new("Flare · FLARE · 2026", "m3", 1_234.5678m, 0.002m, "tCO2e", "rev-1", 2.4691356m, "F-1", "Table C-1"),
            new("Vent · VENT · 2026", "m3", 10m, 0.001m, "tCO2e", "rev-1", 0.01m, "F-2", "Table W-1"),
        };

        var report = new OgGhgReport(2026, lines, [new OgGhgTotal("tCO2e", 2.4791356m)], "rev-1",
            GhgRules.ReviewCurrent);

        var csv = report.Csv();

        foreach (var line in lines)
        {
            Assert.Contains(GhgRules.Figure(line.Quantity), csv, StringComparison.Ordinal);
            Assert.Contains(GhgRules.Figure(line.Factor), csv, StringComparison.Ordinal);
            Assert.Contains(GhgRules.Figure(line.Emissions), csv, StringComparison.Ordinal);
            Assert.Contains("\"" + line.FactorId + "\"", csv, StringComparison.Ordinal);
            Assert.Contains("\"" + line.SourceReference + "\"", csv, StringComparison.Ordinal);
        }

        Assert.Contains("total,\"tCO2e\",2.4791356", csv, StringComparison.Ordinal);
        Assert.Contains("2.4791356 tCO2e", report.TotalText, StringComparison.Ordinal);
        Assert.Equal(2.4791356m, report.TotalCo2e);

        // The screen's line text uses the same numbers as the export.
        foreach (var line in lines)
        {
            Assert.Contains(GhgRules.Figure(line.Emissions), report.LinesText, StringComparison.Ordinal);
        }

        // Two output units are never summed, and the total then has no single CO2e figure.
        var mixed = report with
        {
            Totals = [new OgGhgTotal("tCO2e", 2.4691356m), new OgGhgTotal("kgCO2e", 10m)],
        };

        Assert.Null(mixed.TotalCo2e);
        Assert.Contains("not summed", mixed.TotalText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_catalog_selection_needs_its_field_its_jurisdiction_and_its_rationale()
    {
        Assert.Null(GhgRules.ValidateSelection("F-1", "TX", "Chosen for the 2026 filing."));

        Assert.Equal(GhgRules.CodeFieldRequired, GhgRules.ValidateSelection(null, "TX", "why")!.Value.Code);
        Assert.Equal(GhgRules.CodeJurisdictionRequired,
            GhgRules.ValidateSelection("F-1", " ", "why")!.Value.Code);

        var noRationale = GhgRules.ValidateSelection("F-1", "TX", " ");
        Assert.Equal(GhgRules.CodeRationaleRequired, noRationale!.Value.Code);
        Assert.Contains("reviewed later", noRationale.Value.Message, StringComparison.Ordinal);

        // AC-3's neighbour: the jurisdiction is never defaulted.
        var noCountry = GhgRules.ValidateSelection("F-1", null, "why");
        Assert.DoesNotContain("USA", noCountry!.Value.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("None is assumed", noCountry.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_shows_the_reason_the_export_and_no_default_country()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-1 and AC-5: the status and the reasons are bound from the workspace.
        Assert.Contains("workspace.StatusText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.UnavailableText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.TotalText", page, StringComparison.Ordinal);

        // AC-6: the export is the report's own text.
        Assert.Contains("workspace.ExportText", page, StringComparison.Ordinal);

        // AC-3: the review sentence is shown.
        Assert.Contains("workspace.Review.Sentence", page, StringComparison.Ordinal);

        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex("\\b(USA|United States|Canada|UK)\\b"), page);
    }

    private static COMPANY_GHG_CATALOG_SELECTION Selection(string catalogRevision, string environmentRevision) =>
        new()
        {
            COMPANY_GHG_CATALOG_SELECTION_ID = "GHG-SMOKE-TX",
            OWNER_TENANT_ID = "SMOKE",
            OWNER_COMPANY_BA_ID = "SMOKE",
            JURISDICTION_KEY = "TX",
            CATALOG_REVISION = catalogRevision,
            ENVIRONMENT_REVISION = environmentRevision,
            SELECTED_BY = "sara",
            RATIONALE = "Chosen for the 2026 filing.",
            REVISION = "rev-selection-1",
            INTENT_ID = "rev-selection-1",
        };

    private static OgGhgWorkspace Unavailable(IReadOnlyList<string> reasons) =>
        new(
            new OgHseScope("SMOKE", "F-1"),
            2026,
            "TX",
            new OgGhgCatalog("TX", null, [], GhgRules.ReasonNoFactors),
            new OgGhgReview(GhgRules.ReviewNotReviewed, "the company has not reviewed a catalog", null, null, null,
                "rev-env-1", null),
            [],
            null,
            reasons,
            DateTime.UtcNow);
}
