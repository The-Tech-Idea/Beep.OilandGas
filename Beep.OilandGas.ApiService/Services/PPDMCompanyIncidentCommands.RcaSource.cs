using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class PPDMCompanyIncidentCommands
{
    /// <summary>Reviews all cause observation keys under current incident and FIELD write authority.</summary>
    public async Task<IncidentRcaSourceReview> ReviewRcaSourceAsync(FieldSelectionReview review, string incidentId)
    {
        ArgumentNullException.ThrowIfNull(review);
        if (!Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        var authority = await RequireAuthorityAsync(review, incidentId, review.ActorUserId);
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_CAUSE), ["INCIDENT_ID", "CAUSE_OBS_NO"]);
        await DefinitionAsync(metadata, typeof(R_INCIDENT_CAUSE_TYPE), ["CAUSE_TYPE"]);
        await DefinitionAsync(metadata, typeof(R_INCIDENT_CAUSE_CODE), ["CAUSE_TYPE", "CAUSE_CODE"], ("CAUSE_TYPE", "R_INCIDENT_CAUSE_TYPE", "CAUSE_TYPE"));
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        async Task<(string Evidence, IncidentRcaFinding[] Findings, int Next, IncidentRcaOption[] Options)> ReadAsync()
        {
            var rows = (await reader.ReadAsync(typeof(HSE_INCIDENT_CAUSE), [Equal("INCIDENT_ID", incidentId)],
                ["INCIDENT_ID", "CAUSE_OBS_NO"], ["INCIDENT_ID", "CAUSE_OBS_NO", "ACTIVE_IND", "CAUSE_TYPE", "CAUSE_CODE", "REMARK", "EFFECTIVE_DATE", "EXPIRY_DATE"]))
                .Cast<HSE_INCIDENT_CAUSE>().ToArray();
            if (rows.Any(row => !Observation(row.CAUSE_OBS_NO) || row.CAUSE_OBS_NO >= int.MaxValue || row.ACTIVE_IND is not ("Y" or "N") ||
                row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) ||
                rows.Select(row => row.CAUSE_OBS_NO).Distinct().Count() != rows.Length)
                throw RefusalException.Conflict("The recorded incident cause observations are invalid or ambiguous.");
            var findings = rows.OrderBy(row => row.CAUSE_OBS_NO).Select(row => new IncidentRcaFinding(row.CAUSE_OBS_NO,
                row.CAUSE_TYPE ?? "", row.CAUSE_CODE ?? "", row.REMARK ?? "", row.ACTIVE_IND == "Y")).ToArray();
            var parents = (await reader.ReadAsync(typeof(R_INCIDENT_CAUSE_TYPE), [Equal("CAUSE_TYPE", CauseType.Root), Equal("ACTIVE_IND", "Y")],
                ["CAUSE_TYPE", "ACTIVE_IND"], ["CAUSE_TYPE", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"])) .Cast<R_INCIDENT_CAUSE_TYPE>().ToArray();
            var codes = (await reader.ReadAsync(typeof(R_INCIDENT_CAUSE_CODE), [Equal("CAUSE_TYPE", CauseType.Root), Equal("ACTIVE_IND", "Y")],
                ["CAUSE_TYPE", "CAUSE_CODE", "ACTIVE_IND"], ["CAUSE_TYPE", "CAUSE_CODE", "ACTIVE_IND", "LONG_NAME", "EFFECTIVE_DATE", "EXPIRY_DATE"])) .Cast<R_INCIDENT_CAUSE_CODE>().ToArray();
            if (parents.Length > 1 || codes.Any(row => !Identifier(row.CAUSE_CODE) || row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) ||
                parents.Any(row => row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) ||
                codes.Select(row => row.CAUSE_CODE).Distinct(StringComparer.Ordinal).Count() != codes.Length)
                throw RefusalException.Conflict("Recorded root-cause reference data is invalid or ambiguous.");
            var available = parents.Length == 1 && Window(parents[0], TimeZoneInfo.Utc, TimeProvider.System, false);
            var options = available ? codes.Where(row => Window(row, TimeZoneInfo.Utc, TimeProvider.System, false))
                .OrderBy(row => row.CAUSE_CODE, StringComparer.Ordinal).Select(row => new IncidentRcaOption(row.CAUSE_TYPE, row.CAUSE_CODE, row.LONG_NAME ?? "")).ToArray() : [];
            return (FingerprintRecords([Fingerprint(rows), Fingerprint(parents), Fingerprint(codes)]), findings,
                rows.Length == 0 ? 1 : Math.Max(1, checked((int)rows.Max(row => row.CAUSE_OBS_NO) + 1)), options);
        }
        var first = await ReadAsync();
        if (await RequireAuthorityAsync(review, incidentId, review.ActorUserId) != authority || (await ReadAsync()).Evidence != first.Evidence ||
            await RequireAuthorityAsync(review, incidentId, review.ActorUserId) != authority)
            throw RefusalException.Conflict("Incident authority or cause evidence changed during review.");
        var result = new IncidentRcaSourceReview(review, incidentId,
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Context = review, IncidentId = incidentId, Authority = authority, first.Evidence })),
            first.Next, Array.AsReadOnly(first.Findings), Array.AsReadOnly(first.Options));
        result.Require(); return result;
    }
}
