using System.Text.RegularExpressions;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-02: the report validation, the ISO timestamp rule, the null-population refusal, the recorded state vocabulary, the
/// workflow refusal and the stale-load guard - the parts that need no database. The register and the report's rows are
/// proven in the gateway test.
/// </summary>
public sealed class HseIncidentTests
{
    private const string PageFileName = "IncidentsPageView.cs";

    private static readonly OgHseScope Scope = new("SMOKE", "F-1");

    [Fact]
    public void A_report_needs_its_type_tier_jurisdiction_timestamp_and_description()
    {
        Assert.Null(IncidentRules.ValidateReport(Complete(), Scope));

        // AC-1: each required part is named.
        Assert.Equal(IncidentRules.CodeInputRequired, IncidentRules.ValidateReport(Complete() with { Type = null }, Scope)!.Value.Code);
        Assert.Equal(IncidentRules.CodeInputRequired, IncidentRules.ValidateReport(Complete() with { Tier = null }, Scope)!.Value.Code);
        Assert.Equal(IncidentRules.CodeInputRequired, IncidentRules.ValidateReport(Complete() with { Description = "" }, Scope)!.Value.Code);
        Assert.Equal(IncidentRules.CodeInputRequired, IncidentRules.ValidateReport(Complete() with { IntentId = null }, Scope)!.Value.Code);
        Assert.Equal(IncidentRules.CodeInputInvalid, IncidentRules.ValidateReport(Complete() with { Tier = "TIER_9" }, Scope)!.Value.Code);

        // AC-2: a blank jurisdiction is refused and nothing is assumed in its place.
        var noJurisdiction = IncidentRules.ValidateReport(Complete() with { Jurisdiction = "  " }, Scope);

        Assert.Equal(IncidentRules.CodeJurisdictionRequired, noJurisdiction!.Value.Code);
        Assert.Contains("None is assumed", noJurisdiction.Value.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("USA", noJurisdiction.Value.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_timestamp_without_an_offset_is_refused()
    {
        // AC-1: an ISO 8601 timestamp with an explicit offset is required.
        Assert.True(IncidentRules.TryParseOccurred("2026-11-08T10:00:00Z", out var utc));
        Assert.Equal(DateTimeOffset.Parse("2026-11-08T10:00:00Z"), utc);

        Assert.True(IncidentRules.TryParseOccurred("2026-11-08T13:00:00+03:00", out var offset));
        Assert.Equal(TimeSpan.FromHours(3), offset.Offset);

        Assert.True(IncidentRules.TryParseOccurred("2026-11-08T13:00:00.500+0300", out _));

        // A local date and time without an offset is not an instant, so it is refused.
        foreach (var bad in new[] { "2026-11-08T10:00:00", "2026-11-08 10:00", "08/11/2026 10:00", "", null })
        {
            Assert.False(IncidentRules.TryParseOccurred(bad, out _));
            Assert.Equal(IncidentRules.CodeTimestampNeedsOffset,
                IncidentRules.ValidateReport(Complete() with { OccurredAtIso = bad }, Scope)!.Value.Code);
        }
    }

    [Fact]
    public void An_instant_is_shown_in_utc_and_the_company_zone()
    {
        var utc = new DateTime(2026, 11, 8, 10, 0, 0, DateTimeKind.Utc);

        Assert.Contains("2026-11-08 10:00 UTC", OgIncidents.DescribeOccurred(utc, null), StringComparison.Ordinal);
        Assert.Contains("no company zone selected", OgIncidents.DescribeOccurred(utc, null), StringComparison.Ordinal);

        // A zone this machine knows is shown beside the UTC instant, so a reader never mistakes one for the other.
        var zone = TimeZoneInfo.Local.Id;
        var inZone = OgIncidents.DescribeOccurred(utc, zone);

        Assert.Contains("10:00 UTC", inZone, StringComparison.Ordinal);
        Assert.Contains(zone, inZone, StringComparison.Ordinal);

        // An unknown zone is named rather than shown as a local time.
        Assert.Contains("not known", OgIncidents.DescribeOccurred(utc, "Not/AZone"), StringComparison.Ordinal);
    }

    [Fact]
    public void A_report_reviewed_against_another_field_is_refused()
    {
        // The reviewed field must match the session's field when the report is filed.
        var moved = IncidentRules.ValidateReport(Complete() with { FieldId = "F-2" }, Scope);

        Assert.Equal(IncidentRules.CodeFieldChanged, moved!.Value.Code);
        Assert.Contains("F-2", moved.Value.Message, StringComparison.Ordinal);
        Assert.Contains("F-1", moved.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_null_population_is_refused_rather_than_shown_as_empty()
    {
        // AC-4.
        var refusal = IncidentRules.CheckPopulation<object>(null, "incident register");

        Assert.Equal(IncidentRules.CodePopulationNull, refusal!.Value.Code);
        Assert.Contains("rather than shown as empty", refusal.Value.Message, StringComparison.Ordinal);

        Assert.Null(IncidentRules.CheckPopulation(Array.Empty<object>(), "incident register"));
    }

    [Fact]
    public void The_state_vocabulary_is_exactly_the_recorded_one()
    {
        // AC-6: the filter offers only the owner's IncidentState values.
        Assert.Equal(
            [
                IncidentState.Reported,
                IncidentState.UnderInvestigation,
                IncidentState.PendingRCA,
                IncidentState.PendingCorrectiveAction,
                IncidentState.PendingCloseOut,
                IncidentState.Closed,
                IncidentState.Cancelled,
            ],
            IncidentRules.States);

        Assert.Equal(HseReferenceCodes.Severity.All, IncidentRules.Tiers);

        // An unknown filter value is refused and the refused message lists what is offered.
        var refusal = IncidentRules.ValidateFilter(new OgIncidentFilter(State: "PENDING_RCA_TYPO"));

        Assert.Equal(IncidentRules.CodeFilterInvalid, refusal!.Value.Code);
        Assert.Contains(IncidentState.PendingRCA, refusal.Value.Message, StringComparison.Ordinal);

        Assert.Null(IncidentRules.ValidateFilter(new OgIncidentFilter(State: IncidentState.Closed)));
        Assert.Null(IncidentRules.ValidateFilter(OgIncidentFilter.None));
    }

    [Fact]
    public async Task A_state_change_is_refused_because_the_workflow_is_not_wired()
    {
        // AC-5: no direct update, and the refusal is the same whatever the data - so it is checked before any read.
        var incidents = new OgIncidents(() => null, new Quiet());

        var outcome = await incidents.TransitionAsync("INC-1", "investigate", "started", "sara");

        Assert.False(outcome.Success);
        Assert.Null(outcome.Value);
        Assert.Equal(IncidentRules.CodeWorkflowNotWired, outcome.Reference);
        Assert.Contains("keeps its recorded state", outcome.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void A_later_load_invalidates_the_earlier_one()
    {
        // AC-3, the version half: a late response for a superseded load is discarded.
        var incidents = new OgIncidents(() => null, new Quiet());

        var first = incidents.BeginLoad();
        var second = incidents.BeginLoad();

        Assert.False(incidents.IsCurrent(first));
        Assert.True(incidents.IsCurrent(second));
    }

    [Fact]
    public void The_incident_id_is_derived_from_the_intent_so_a_retry_finds_it()
    {
        Assert.Equal("INC-intent-1", IncidentRules.IdFor("intent-1"));
        Assert.Equal(IncidentRules.IdFor("intent-1"), IncidentRules.IdFor(" intent-1 "));
        Assert.NotEqual(IncidentRules.IdFor("intent-1"), IncidentRules.IdFor("intent-2"));
    }

    [Fact]
    public void The_page_has_no_default_jurisdiction_and_offers_no_state_change()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-2: no country is named anywhere in the view.
        Assert.DoesNotMatch(new Regex("\\b(USA|United States|US|Canada|UK)\\b"), page);

        // AC-1: the report binds an ISO timestamp from the dialog.
        Assert.Contains("Occurred ISO with offset", page, StringComparison.Ordinal);
        Assert.Contains("Report()", page, StringComparison.Ordinal);

        // AC-5: the page states the workflow block instead of offering a write.
        Assert.Contains("TransitionNotice", page, StringComparison.Ordinal);

        // AC-6: the filters are bound from the workspace's recorded lists.
        Assert.Contains("workspace.States", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Tiers", page, StringComparison.Ordinal);

        // AC-3/AC-4: the failed branch clears the grid.
        Assert.Contains("_grid.ItemsSource = null", page, StringComparison.Ordinal);
    }

    private static OgIncidentReport Complete() =>
        new(
            "intent-1",
            HseReferenceCodes.IncidentType.Spill,
            HseReferenceCodes.Severity.Tier2,
            "TX",
            "2026-11-08T10:00:00Z",
            "F-1",
            "FAC-1",
            "A small spill was contained at the separator.");

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
