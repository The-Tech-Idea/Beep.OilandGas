using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
using Xunit;
using Ppdm = Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.PPDM39.Tests;

/// <summary>
/// DAT-02's HSE count rule: total, open (latest STATUS neither CLOSED nor CANCELLED) and the tiers from each
/// incident's primary detail, tier 4 when none is recorded. Pure, so the rule is tested without a database; the
/// service decides which rows are in scope (field, date range, active) and then calls this.
/// </summary>
public class HSEIncidentCountsTests
{
    private static IncidentCounts Count(
        Ppdm.HSE_INCIDENT[] incidents,
        Ppdm.HSE_INCIDENT_DETAIL[] details,
        Ppdm.HSE_INCIDENT_RESPONSE[] responses) =>
        HSEIncidentRules.Count(incidents, details, responses);

    [Fact]
    public void Tiers_come_from_the_primary_detail_and_open_from_the_latest_status()
    {
        var incidents = new[]
        {
            new Ppdm.HSE_INCIDENT { INCIDENT_ID = "A", ACTIVE_IND = "Y" },
            new Ppdm.HSE_INCIDENT { INCIDENT_ID = "B", ACTIVE_IND = "Y" },
            // The class column is written by nothing; it must not decide the tier.
            new Ppdm.HSE_INCIDENT { INCIDENT_ID = "C", ACTIVE_IND = "Y", INCIDENT_CLASS_ID = "TIER_1" },
        };

        var details = new[]
        {
            new Ppdm.HSE_INCIDENT_DETAIL { INCIDENT_ID = "A", DETAIL_OBS_NO = 2, DETAIL_TYPE = "SUPPLEMENTARY", INCIDENT_SEVERITY_ID = "TIER_3" },
            new Ppdm.HSE_INCIDENT_DETAIL { INCIDENT_ID = "A", DETAIL_OBS_NO = 1, DETAIL_TYPE = "PRIMARY", INCIDENT_SEVERITY_ID = "TIER_1" },
            new Ppdm.HSE_INCIDENT_DETAIL { INCIDENT_ID = "B", DETAIL_OBS_NO = 1, DETAIL_TYPE = "PRIMARY", INCIDENT_SEVERITY_ID = IncidentType.PseTier2 },
        };

        var responses = new[]
        {
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "A", RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Reported, EFFECTIVE_DATE = new DateTime(2026, 9, 1) },
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "A", RESPONSE_OBS_NO = 2, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Closed, EFFECTIVE_DATE = new DateTime(2026, 9, 20) },
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "B", RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.UnderInvestigation, EFFECTIVE_DATE = new DateTime(2026, 9, 5) },
        };

        var counts = Count(incidents, details, responses);

        Assert.Equal(3, counts.Total);
        // A is closed; B is under investigation; C has no status, so it stands as reported (open).
        Assert.Equal(2, counts.Open);
        // Tiers cover every incident in scope, not only the open ones.
        Assert.Equal(1, counts.Tier1);
        Assert.Equal(1, counts.Tier2);
        Assert.Equal(0, counts.Tier3);
        Assert.Equal(1, counts.Tier4);
        Assert.Equal(counts.Total, counts.Tier1 + counts.Tier2 + counts.Tier3 + counts.Tier4);
    }

    [Fact]
    public void A_cancelled_incident_is_not_open_and_a_closed_one_is_not_either()
    {
        var incidents = new[]
        {
            new Ppdm.HSE_INCIDENT { INCIDENT_ID = "A", ACTIVE_IND = "Y" },
            new Ppdm.HSE_INCIDENT { INCIDENT_ID = "B", ACTIVE_IND = "Y" },
            new Ppdm.HSE_INCIDENT { INCIDENT_ID = "C", ACTIVE_IND = "Y" },
        };

        var responses = new[]
        {
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "A", RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Cancelled },
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "B", RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Closed },
            // C has a non-status response only: its state is REPORTED, which is open.
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "C", RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "ACTION", RESPONSE_RESULT = IncidentState.Closed },
        };

        var counts = Count(incidents, [], responses);

        Assert.Equal(3, counts.Total);
        Assert.Equal(1, counts.Open);
        Assert.Equal(3, counts.Tier4);
    }

    [Fact]
    public void The_latest_status_by_effective_date_and_sequence_wins()
    {
        var incidents = new[] { new Ppdm.HSE_INCIDENT { INCIDENT_ID = "A", ACTIVE_IND = "Y" } };

        var responses = new[]
        {
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "A", RESPONSE_OBS_NO = 3, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Reported, EFFECTIVE_DATE = new DateTime(2026, 9, 1) },
            // Later sequence on the same date: this one decides, and it is closed.
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "A", RESPONSE_OBS_NO = 4, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Closed, EFFECTIVE_DATE = new DateTime(2026, 9, 1) },
            new Ppdm.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "A", RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.PendingRCA, EFFECTIVE_DATE = new DateTime(2026, 8, 1) },
        };

        Assert.Equal(0, Count(incidents, [], responses).Open);
    }

    [Fact]
    public void An_empty_scope_counts_nothing()
    {
        var counts = Count([], [], []);

        Assert.Equal(new IncidentCounts(0, 0, 0, 0, 0, 0), counts);
    }
}
