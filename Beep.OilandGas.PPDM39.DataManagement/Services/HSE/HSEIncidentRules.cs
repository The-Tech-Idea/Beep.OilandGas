using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;

/// <summary>
/// How an HSE incident's tier and state are read from its PPDM rows — the one rule for both, used by
/// <see cref="HSEIncidentService"/> (which writes them and builds the incident record) and <see cref="HSEKPIService"/>
/// (which counts by them).
/// </summary>
/// <remarks>
/// The tier is the INCIDENT_SEVERITY_ID on the incident's primary HSE_INCIDENT_DETAIL row ("TIER_1" … "TIER_4", a bare
/// number, or a PSE tier incident type), tier 4 when none is recorded. The state is the RESPONSE_RESULT of its latest
/// STATUS HSE_INCIDENT_RESPONSE row, "REPORTED" when there is none. HSE_INCIDENT.INCIDENT_CLASS_ID is written by nothing in
/// this system: counting tiers or open incidents by it (as two API aggregations did) counted nothing, or every incident.
/// </remarks>
internal static class HSEIncidentRules
{
    internal const string PrimaryDetailType = "PRIMARY";
    internal const string StatusResponseType = "STATUS";

    /// <summary>The tier an incident has when its primary detail records none.</summary>
    internal const int UnrecordedTier = 4;

    internal static string ToSeverityId(int tier) => $"TIER_{Math.Clamp(tier, 1, UnrecordedTier)}";

    internal static int TierOf(string? severityId)
    {
        if (string.IsNullOrWhiteSpace(severityId))
        {
            return UnrecordedTier;
        }

        if (severityId.StartsWith("TIER_", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(severityId[5..], out var parsedTier))
        {
            return parsedTier;
        }

        if (int.TryParse(severityId, out parsedTier))
        {
            return parsedTier;
        }

        if (string.Equals(severityId, IncidentType.PseTier1, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return string.Equals(severityId, IncidentType.PseTier2, StringComparison.OrdinalIgnoreCase) ? 2 : UnrecordedTier;
    }

    /// <summary>The detail row an incident's tier and type are read from: the PRIMARY one, else the first recorded.</summary>
    internal static HSE_INCIDENT_DETAIL? PrimaryDetail(IEnumerable<HSE_INCIDENT_DETAIL> details) =>
        details
            .OrderBy(detail => detail.DETAIL_TYPE == PrimaryDetailType ? 0 : 1)
            .ThenBy(detail => detail.DETAIL_OBS_NO)
            .FirstOrDefault();

    /// <summary>The latest STATUS response among an incident's responses, which says the incident's current state.</summary>
    internal static HSE_INCIDENT_RESPONSE? LatestStatus(IEnumerable<HSE_INCIDENT_RESPONSE> responses) =>
        responses
            .Where(response => string.Equals(response.RESPONSE_TYPE, StatusResponseType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(response => response.EFFECTIVE_DATE ?? DateTime.MinValue)
            .ThenByDescending(response => response.RESPONSE_OBS_NO)
            .FirstOrDefault();

    internal static string StateOf(HSE_INCIDENT_RESPONSE? latestStatus) =>
        string.IsNullOrWhiteSpace(latestStatus?.RESPONSE_RESULT) ? IncidentState.Reported : latestStatus.RESPONSE_RESULT;

    /// <summary>An incident is open until it is closed or cancelled.</summary>
    internal static bool IsOpen(string state) =>
        !string.Equals(state, IncidentState.Closed, StringComparison.OrdinalIgnoreCase)
        && !string.Equals(state, IncidentState.Cancelled, StringComparison.OrdinalIgnoreCase);
}
