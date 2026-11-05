using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class PPDMCompanyCorrectiveActionCommands
{
    public async Task<IncidentClosureReview> ReviewClosureAsync(FieldSelectionReview context, string incidentId, string projectId, string actor)
    {
        var authority = await RequireAuthorityAsync(context, incidentId, projectId, actor);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, context.Database, context.Company, actor);
        await reader.OpenAsync();
        var definition = await metadata.GetTableMetadataAsync(nameof(HSE_CA_VERIFICATION));
        if (definition?.EntityTypeName != typeof(HSE_CA_VERIFICATION).FullName || definition.PrimaryKeyColumn != nameof(HSE_CA_VERIFICATION.HSE_CA_VERIFICATION_ID))
            throw RefusalException.Conflict("The recorded verification metadata is unavailable.");
        async Task<CAVerificationDecision[]> Read()
        {
            var rows = await reader.ReadAsync(typeof(HSE_CA_VERIFICATION), [Equal("OWNER_TENANT_ID", context.Company.TenantId), Equal("OWNER_COMPANY_BA_ID", context.Company.BusinessAssociateId),
                Equal("FIELD_ID", context.FieldId!), Equal("INCIDENT_ID", incidentId), Equal("PROJECT_ID", projectId)], ["HSE_CA_VERIFICATION_ID"],
                ["HSE_CA_VERIFICATION_ID", "OWNER_TENANT_ID", "OWNER_COMPANY_BA_ID", "FIELD_ID", "INCIDENT_ID", "PROJECT_ID", "SOURCE_REVISION", "COMMAND_JSON", "COMMAND_HASH", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]);
            if (rows.Length > 256) throw RefusalException.Conflict("The verification history exceeds the closure review limit.");
            return rows.Cast<HSE_CA_VERIFICATION>().Select(row => row.Restore(context.Company)).OrderBy(item => item.IntentId).ToArray();
        }
        var actions = await ReviewVerificationAsync(context, incidentId, projectId, actor); var decisions = await Read();
        var result = new IncidentClosureReview(actions, decisions); result.Require();
        if (JsonSerializer.Serialize(await ReviewVerificationAsync(context, incidentId, projectId, actor)) != JsonSerializer.Serialize(actions) ||
            JsonSerializer.Serialize(await Read()) != JsonSerializer.Serialize(decisions) || await RequireAuthorityAsync(context, incidentId, projectId, actor) != authority)
            throw RefusalException.Conflict("The incident closure evidence changed during review.");
        return result;
    }

    public async Task<CAVerificationDecision> RecordVerificationAsync(CAVerificationDecision original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        var frozen = JsonSerializer.Deserialize<CAVerificationDecision>(JsonSerializer.Serialize(original))!; frozen.Require();
        var review = frozen.Review; var context = review.Context; var actor = context.ActorUserId;
        var authority = await RequireAuthorityAsync(context, review.IncidentId, review.PlanId, actor);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, context.Database, context.Company, actor);
        await reader.OpenAsync();
        var definition = await metadata.GetTableMetadataAsync(nameof(HSE_CA_VERIFICATION));
        if (definition?.EntityTypeName != typeof(HSE_CA_VERIFICATION).FullName || definition.PrimaryKeyColumn != nameof(HSE_CA_VERIFICATION.HSE_CA_VERIFICATION_ID))
            throw RefusalException.Conflict("Corrective-action verification metadata is unavailable.");
        var source = editor.GetDataSource(context.Database.ConnectionName) ?? throw new InvalidOperationException("The verification datasource is unavailable.");
        var expected = HSE_CA_VERIFICATION.Create(frozen);
        void ValidateSchema()
        {
            var shape = source.GetEntityStructure(new EntityStructure { EntityName = nameof(HSE_CA_VERIFICATION), DatasourceEntityName = nameof(HSE_CA_VERIFICATION) }, true);
            if (shape?.Fields is null || !shape.Fields.Where(x => x.IsKey).Select(x => x.FieldName).SequenceEqual([nameof(HSE_CA_VERIFICATION.HSE_CA_VERIFICATION_ID)]))
                throw new InvalidOperationException("The verification primary key is unavailable or incompatible.");
            foreach (var (name, type, width) in new[] {
                ("HSE_CA_VERIFICATION_ID", typeof(string), 32), ("OWNER_TENANT_ID", typeof(string), context.Company.TenantId.Length),
                ("OWNER_COMPANY_BA_ID", typeof(string), context.Company.BusinessAssociateId.Length), ("FIELD_ID", typeof(string), context.FieldId!.Length),
                ("INCIDENT_ID", typeof(string), review.IncidentId.Length), ("PROJECT_ID", typeof(string), review.PlanId.Length),
                ("SOURCE_REVISION", typeof(string), 64), ("COMMAND_JSON", typeof(string), expected.COMMAND_JSON.Length), ("COMMAND_HASH", typeof(string), 64),
                ("ACTIVE_IND", typeof(string), 1), ("ROW_CREATED_BY", typeof(string), actor.Length), ("ROW_CHANGED_BY", typeof(string), actor.Length),
                ("ROW_CREATED_DATE", typeof(DateTime), 0), ("ROW_CHANGED_DATE", typeof(DateTime), 0) })
            {
                var field = shape.Fields.Where(x => x.FieldName == name).ToArray();
                if (field.Length != 1 || field[0].Fieldtype != type.FullName || width > 0 && Math.Max(field[0].Size, field[0].Size1) < width)
                    throw new InvalidOperationException("The verification schema cannot preserve its original evidence.");
            }
        }
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(editor.GetDataSource(context.Database.ConnectionName), source) || await RequireAuthorityAsync(context, review.IncidentId, review.PlanId, actor) != authority)
                throw RefusalException.Conflict("The verification authority or database changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task RequireReviewedEvidence()
        {
            var current = await ReviewVerificationAsync(context, review.IncidentId, review.PlanId, actor);
            if (JsonSerializer.Serialize(current) != JsonSerializer.Serialize(review) || !current.CanReviewIndependently)
                throw RefusalException.Conflict("The recorded corrective actions changed. Review them again.");
        }
        async Task<HSE_CA_VERIFICATION[]> Read()
        {
            var rows = await reader.ReadAsync(typeof(HSE_CA_VERIFICATION), [Equal("OWNER_TENANT_ID", context.Company.TenantId), Equal("OWNER_COMPANY_BA_ID", context.Company.BusinessAssociateId),
                Equal("FIELD_ID", context.FieldId!), Equal("INCIDENT_ID", review.IncidentId), Equal("PROJECT_ID", review.PlanId)], ["HSE_CA_VERIFICATION_ID"],
                ["HSE_CA_VERIFICATION_ID", "OWNER_TENANT_ID", "OWNER_COMPANY_BA_ID", "FIELD_ID", "INCIDENT_ID", "PROJECT_ID", "SOURCE_REVISION", "COMMAND_JSON", "COMMAND_HASH", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]);
            var records = rows.Cast<HSE_CA_VERIFICATION>().ToArray();
            if (records.Select(x => x.HSE_CA_VERIFICATION_ID).Distinct(StringComparer.Ordinal).Count() != records.Length)
                throw RefusalException.Conflict("The recorded verification population is ambiguous.");
            foreach (var record in records) _ = record.Restore(context.Company);
            return records;
        }
        bool Matches(HSE_CA_VERIFICATION row) => JsonSerializer.Serialize(row.Restore(context.Company)) == JsonSerializer.Serialize(frozen);
        await Guard(false); ValidateSchema(); await Guard(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true); var records = await Read();
            var sameId = records.Where(row => row.HSE_CA_VERIFICATION_ID == expected.HSE_CA_VERIFICATION_ID).ToArray();
            if (sameId.Length > 0)
            {
                if (sameId.Length != 1 || !Matches(sameId[0])) throw RefusalException.Conflict("The verification identity belongs to another request.");
            }
            else
            {
                if (records.Any(row => row.SOURCE_REVISION == review.Revision)) throw RefusalException.Conflict("A decision for this exact action evidence is already recorded.");
                await RequireReviewedEvidence(); await Guard(true);
                var permit = new PPDMIncidentReportWriter.IncidentReportMutation(rowPolicy, source, expected, actor, async () => { await Guard(true); await RequireReviewedEvidence(); });
                await new PPDMGenericRepository(editor, columns, defaults, metadata, typeof(HSE_CA_VERIFICATION), context.Database.ConnectionName, nameof(HSE_CA_VERIFICATION), rowAccess: permit).InsertAsync(expected, actor);
            }
            await Guard(true); var saved = (await Read()).Where(row => row.HSE_CA_VERIFICATION_ID == expected.HSE_CA_VERIFICATION_ID).ToArray();
            if (saved.Length != 1 || !Matches(saved[0])) throw RefusalException.Conflict("The verification outcome is unconfirmed.");
        });
        await Guard(false); var confirmed = (await Read()).Where(row => row.HSE_CA_VERIFICATION_ID == expected.HSE_CA_VERIFICATION_ID).ToArray();
        if (confirmed.Length != 1 || !Matches(confirmed[0])) throw RefusalException.Conflict("The verification commit is unconfirmed; retain the original decision.");
        await Guard(false); return frozen;
    }
}
