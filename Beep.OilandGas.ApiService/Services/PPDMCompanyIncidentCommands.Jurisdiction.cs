using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class PPDMCompanyIncidentCommands
{
    /// <summary>Reads the submitted jurisdiction text from the app-owned original, under current case authority.</summary>
    public async Task<string> ReadReportedJurisdictionAsync(FieldSelectionReview review, string incidentId)
    {
        ArgumentNullException.ThrowIfNull(review);
        if (!Identifier(incidentId) || review.ActorUserId != company.Company.UserId || review.Company != company.Company.Scope)
            throw RefusalException.Forbidden("The incident report is outside the selected company or account.");
        var metadataEntry = await metadata.GetTableMetadataAsync(nameof(HSE_INCIDENT_REPORT_INTENT));
        if (metadataEntry?.EntityTypeName != typeof(HSE_INCIDENT_REPORT_INTENT).FullName || metadataEntry.PrimaryKeyColumn != "HSE_INCIDENT_REPORT_INTENT_ID")
            throw RefusalException.Conflict("The incident report metadata is unavailable.");
        async Task<string> Authority()
        {
            var selected = await selections.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(selected, review.Company, review.ActorUserId);
            if (!selected.SelectionAvailable || selected.SelectionContext != review || await company.ResolveAsync() != review.Database ||
                (await company.ResolveSubjectAsync(review.ActorUserId))?.Permissions.Contains(PermissionConstants.HSE.View, StringComparer.Ordinal) != true)
                throw RefusalException.Forbidden("Current company incident view authority is required.");
            return await Canonical.CaptureHseIncidentAuthorityAsync(review, incidentId);
        }
        var authority = await Authority();
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        async Task<(string Jurisdiction, string Evidence)> Read()
        {
            var rows = await reader.ReadAsync(typeof(HSE_INCIDENT_REPORT_INTENT), [Equal("INCIDENT_ID", incidentId)], ["INCIDENT_ID"],
                ["HSE_INCIDENT_REPORT_INTENT_ID", "OWNER_TENANT_ID", "OWNER_COMPANY_BA_ID", "FIELD_ID", "COMMAND_JSON", "COMMAND_HASH", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]);
            if (rows.Length != 1 || rows[0] is not HSE_INCIDENT_REPORT_INTENT saved || !Identifier(saved.ROW_CREATED_BY))
                throw RefusalException.Conflict("The original incident report is unavailable or ambiguous.");
            var original = saved.Restore(review.Company, saved.ROW_CREATED_BY);
            if (original.IncidentId() != incidentId || original.Context.Database != review.Database || original.Context.FieldId != review.FieldId)
                throw RefusalException.Conflict("The original incident report belongs to another FIELD or database.");
            return (original.Jurisdiction, Fingerprint(rows));
        }
        var first = await Read();
        if (await Authority() != authority || await Read() != first || await Authority() != authority)
            throw RefusalException.Conflict("Incident authority or original report changed during review.");
        return first.Jurisdiction;
    }
}
