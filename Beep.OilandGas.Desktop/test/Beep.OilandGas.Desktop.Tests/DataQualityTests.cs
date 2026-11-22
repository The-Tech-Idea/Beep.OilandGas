using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-07: a finding names table, key, column and rule; "no rules" is distinct from "no findings"; a failed run
/// keeps the last successful timestamp; a resolution records the actor; severity order is stable.
/// </summary>
public sealed class DataQualityTests
{
    private static OgFormDefinition Form() => OgEntityForm.Build(
        "T_WELL",
        [
            new("WELL_ID", typeof(string), AllowNull: false, MaxLength: 10),
            new("WELL_NAME", typeof(string), AllowNull: false, MaxLength: 8),
            new("DEPTH_MD", typeof(decimal?), AllowNull: true, Scale: 2),
        ],
        ["WELL_ID"]);

    private static OgRow Row(string id, string? name, string? depth = "100") =>
        new(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["WELL_ID"] = id,
            ["WELL_NAME"] = name,
            ["DEPTH_MD"] = depth,
        });

    [Fact]
    public void A_finding_carries_the_table_the_key_the_column_and_the_rule()
    {
        var run = OgQualityReviewer.Run(Form(), [Row("W-1", null)], OgQualityReviewer.RulesFor(Form()));

        var finding = Assert.Single(run.Findings);

        // AC-1
        Assert.Equal("T_WELL", finding.Table);
        Assert.Equal("W-1", finding.Key);
        Assert.Equal("WELL_NAME", finding.Column);
        Assert.Equal("required", finding.Rule);
        Assert.Equal(OgQualitySeverity.High, finding.Severity);
        Assert.Contains("WELL_NAME", finding.Message, StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(finding.Id));
    }

    [Fact]
    public void A_table_with_no_rules_is_told_apart_from_a_table_with_no_findings()
    {
        var clean = OgQualityReviewer.Run(Form(), [Row("W-1", "Alpha")], OgQualityReviewer.RulesFor(Form()));
        var noRules = OgQualityReviewer.Run(Form(), [Row("W-1", "Alpha")], []);

        // AC-2
        Assert.True(clean.HasRules);
        Assert.False(clean.HasFindings);
        Assert.Contains("no findings", clean.Describe(), StringComparison.OrdinalIgnoreCase);

        Assert.False(noRules.HasRules);
        Assert.False(noRules.HasFindings);
        Assert.Contains("No quality rules apply", noRules.Describe(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Findings_are_ordered_worst_first_and_the_order_is_stable()
    {
        var rules = new List<OgQualityRule>
        {
            new("low", "WELL_NAME", OgQualityRuleKind.ValueTooLong, OgQualitySeverity.Low, Maximum: 3),
            new("critical", null, OgQualityRuleKind.DuplicateKey, OgQualitySeverity.Critical),
            new("medium", "DEPTH_MD", OgQualityRuleKind.NotANumber, OgQualitySeverity.Medium),
            new("high", "WELL_NAME", OgQualityRuleKind.Required, OgQualitySeverity.High),
        };

        var rows = new[] { Row("W-1", null, "nope"), Row("W-1", "A much longer name") };

        var first = OgQualityReviewer.Run(Form(), rows, rules);
        var second = OgQualityReviewer.Run(Form(), rows, rules);

        // AC-5: the numeric order is the contract, and two runs list them the same way.
        Assert.Equal(
            [OgQualitySeverity.Critical, OgQualitySeverity.High, OgQualitySeverity.Medium, OgQualitySeverity.Low],
            first.Findings.Select(finding => finding.Severity));

        Assert.Equal(
            first.Findings.Select(finding => (finding.Severity, finding.Rule, finding.Key)),
            second.Findings.Select(finding => (finding.Severity, finding.Rule, finding.Key)));
    }

    [Fact]
    public void A_failed_run_keeps_the_timestamp_of_the_last_successful_run()
    {
        var journal = new OgQualityJournal();
        var successful = OgQualityReviewer.Run(
            Form(),
            [Row("W-1", "Alpha")],
            OgQualityReviewer.RulesFor(Form()),
            startedAt: new DateTimeOffset(2026, 11, 1, 9, 0, 0, TimeSpan.Zero));

        journal.Record(successful);

        var failed = successful with
        {
            Succeeded = false,
            Error = "the connection dropped",
            CompletedAtUtc = new DateTimeOffset(2026, 11, 7, 9, 0, 0, TimeSpan.Zero),
            Findings = [],
        };

        journal.Record(failed);

        // AC-3: the failed run did not move the timestamp.
        Assert.Equal(successful.CompletedAtUtc, journal.LastSuccess("T_WELL"));
        Assert.Contains("failed", failed.Describe(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolving_a_finding_records_the_actor_and_refuses_an_empty_one()
    {
        var finding = Assert.Single(
            OgQualityReviewer.Run(Form(), [Row("W-1", null)], OgQualityReviewer.RulesFor(Form())).Findings);

        var resolved = OgQualityReviewer.Resolve(finding, "  fahad ", "Corrected in the source system");

        // AC-4
        Assert.True(resolved.Success);
        Assert.Equal("fahad", resolved.Value!.Actor);
        Assert.Equal(finding.Id, resolved.Value.FindingId);
        Assert.Equal("Corrected in the source system", resolved.Value.Note);

        var refused = OgQualityReviewer.Resolve(finding, "   ", "no name");

        Assert.False(refused.Success);
        Assert.Equal("quality-actor-required", refused.Reference);
    }

    [Fact]
    public void Duplicate_keys_are_found_once_per_key_with_the_row_count()
    {
        var run = OgQualityReviewer.Run(
            Form(),
            [Row("W-1", "Alpha"), Row("W-1", "Bravo"), Row("W-2", "Charlie")],
            OgQualityReviewer.RulesFor(Form()));

        var duplicate = Assert.Single(run.Findings, finding => finding.Rule == "duplicate-key");

        Assert.Equal("W-1", duplicate.Key);
        Assert.Contains("2 rows", duplicate.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_value_longer_than_the_column_and_a_number_that_is_not_one_are_both_reported()
    {
        var run = OgQualityReviewer.Run(
            Form(),
            [Row("W-1", new string('x', 9), "not-a-number")],
            OgQualityReviewer.RulesFor(Form()));

        Assert.Contains(run.Findings, finding => finding.Rule == "too-long" && finding.Column == "WELL_NAME");
        Assert.Contains(run.Findings, finding => finding.Rule == "not-a-number" && finding.Column == "DEPTH_MD");
    }
}
