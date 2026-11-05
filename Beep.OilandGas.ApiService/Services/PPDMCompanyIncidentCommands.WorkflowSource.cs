using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class PPDMCompanyIncidentCommands
{
    /// <summary>Captures current case/write authority and the reported status for a workflow start.</summary>
    public async Task<string> ReviewWorkflowSourceRevisionAsync(FieldSelectionReview review, string incidentId)
    {
        ArgumentNullException.ThrowIfNull(review);
        if (!Identifier(incidentId)) throw RefusalException.Invalid("A recorded incident is required.");
        var authority = await RequireAuthorityAsync(review, incidentId, review.ActorUserId);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        async Task<string> ReadStatus()
        {
            var rows = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE),
                [Equal("INCIDENT_ID", incidentId), Equal("RESPONSE_TYPE", "STATUS"), Equal("ACTIVE_IND", "Y")],
                ["INCIDENT_ID", "RESPONSE_OBS_NO", "RESPONSE_TYPE", "RESPONSE_RESULT", "ACTIVE_IND", "EFFECTIVE_DATE"],
                ["INCIDENT_ID", "RESPONSE_OBS_NO", "RESPONSE_TYPE", "RESPONSE_RESULT", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]))
                .Cast<HSE_INCIDENT_RESPONSE>().ToArray();
            if (rows.Length == 0 || rows.Any(row => !Observation(row.RESPONSE_OBS_NO) || row.EFFECTIVE_DATE is not { Kind: DateTimeKind.Utc } ||
                row.EFFECTIVE_DATE > DateTime.UtcNow || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) ||
                rows.Select(row => row.RESPONSE_OBS_NO).Distinct().Count() != rows.Length)
                throw RefusalException.Conflict("The recorded incident status is unavailable or ambiguous.");
            var latest = rows.Where(row => row.EFFECTIVE_DATE == rows.Max(value => value.EFFECTIVE_DATE)).ToArray();
            if (latest.Length != 1 || latest[0].RESPONSE_RESULT != IncidentState.Reported || !Window(latest[0], TimeZoneInfo.Utc, TimeProvider.System, false))
                throw RefusalException.Conflict("Only a uniquely recorded reported incident can start this workflow.");
            return Fingerprint(rows);
        }
        var status = await ReadStatus();
        if (await RequireAuthorityAsync(review, incidentId, review.ActorUserId) != authority || await ReadStatus() != status ||
            await RequireAuthorityAsync(review, incidentId, review.ActorUserId) != authority)
            throw RefusalException.Conflict("The incident or current authority changed during workflow review.");
        return ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(new { Context = review, IncidentId = incidentId, Authority = authority, Status = status }));
    }
}
