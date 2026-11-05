using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class PPDMCompanyCorrectiveActionCommands
{
    private static bool OpenIncidentState(string? state) => state is IncidentState.Reported or IncidentState.UnderInvestigation or IncidentState.PendingRCA or IncidentState.PendingCorrectiveAction or IncidentState.PendingCloseOut;
    private async Task RequireOpenIncidentMutationAsync(FieldSelectionReview context, string incidentId)
    {
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, context.Database, context.Company, context.ActorUserId); await reader.OpenAsync();
        var states = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("INCIDENT_ID", incidentId), Equal("RESPONSE_TYPE", "STATUS"), Equal("ACTIVE_IND", "Y")], ["INCIDENT_ID", "RESPONSE_OBS_NO", "RESPONSE_RESULT"]))
            .Cast<HSE_INCIDENT_RESPONSE>().ToArray();
        if (states.Length == 0 || states.Any(s => !OpenIncidentState(s.RESPONSE_RESULT))) throw RefusalException.Conflict("A closed, cancelled or unrecognized incident cannot accept corrective-action changes.");
    }
    private async Task<HSE_INCIDENT_RESPONSE[]> ReadClosureResponsesAsync(CompanyPPDMEvidenceReader reader, string incidentId)
    {
        var rows = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("INCIDENT_ID", incidentId)], ["INCIDENT_ID", "RESPONSE_OBS_NO"],
            ["INCIDENT_ID", "RESPONSE_OBS_NO", "RESPONSE_TYPE", "RESPONSE_RESULT", "ACTIVE_IND", "REMARK", "SOURCE", "PROJECT_ID", "EFFECTIVE_DATE", "EXPIRY_DATE", "PPDM_GUID", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE", "ROW_QUALITY"]))
            .Cast<HSE_INCIDENT_RESPONSE>().OrderBy(r => r.RESPONSE_OBS_NO).ToArray();
        if (rows.Length is 0 or > 512 || rows.Any(r => r.INCIDENT_ID != incidentId || r.ACTIVE_IND is not ("Y" or "N") || r.RESPONSE_OBS_NO <= 0 || r.RESPONSE_OBS_NO != decimal.Truncate(r.RESPONSE_OBS_NO)) || rows.Select(r => r.RESPONSE_OBS_NO).Distinct().Count() != rows.Length)
            throw RefusalException.Conflict("The native incident response population is unavailable or ambiguous.");
        return rows;
    }
    private static string ClosureResponseRevision(HSE_INCIDENT_RESPONSE[] responses) => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(responses));
    public async Task<string> ReviewNativeClosureRevisionAsync(FieldSelectionReview context, string incidentId, string projectId)
    {
        var authority = await RequireAuthorityAsync(context, incidentId, projectId, context.ActorUserId);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, context.Database, context.Company, context.ActorUserId); await reader.OpenAsync();
        var population = await ReadClosureResponsesAsync(reader, incidentId);
        var statuses = population.Where(r => r.ACTIVE_IND == "Y" && r.RESPONSE_TYPE == "STATUS").ToArray();
        if (statuses.Length == 0 || statuses.Any(r => !OpenIncidentState(r.RESPONSE_RESULT) || r.EFFECTIVE_DATE is not { Kind: DateTimeKind.Utc } || r.EFFECTIVE_DATE > DateTime.UtcNow) || statuses.Count(r => r.EFFECTIVE_DATE == statuses.Max(x => x.EFFECTIVE_DATE)) != 1)
            throw RefusalException.Conflict("Only an active, uniquely recorded open incident can be reviewed for closure.");
        var first = ClosureResponseRevision(population);
        if (first != ClosureResponseRevision(await ReadClosureResponsesAsync(reader, incidentId)) || await RequireAuthorityAsync(context, incidentId, projectId, context.ActorUserId) != authority)
            throw RefusalException.Conflict("The native incident responses changed during closure review.");
        return first;
    }
    public async Task<IncidentClosedReceipt> RecordIncidentClosedAsync(CompanyHseClosureCommand original, CompanyHseClosureReceipt lifecycle, Func<Task> confirmLifecycle)
        => await RecordIncidentClosedCoreAsync(original, lifecycle, null, confirmLifecycle);
    public async Task<IncidentClosedReceipt> ResolveIncidentClosureAsync(CompanyHseClosureCommand original, CompanyHseClosureReceipt lifecycle, IncidentClosureResolution resolution, Func<Task> confirmLifecycle)
    {
        resolution.Require(); resolution.Review.RequireAgainst(original, lifecycle);
        resolution = JsonSerializer.Deserialize<IncidentClosureResolution>(JsonSerializer.Serialize(resolution))!;
        return await RecordIncidentClosedCoreAsync(original, lifecycle, resolution, confirmLifecycle);
    }
    public async Task<IncidentClosureResolutionReview> ReviewClosureResolutionAsync(FieldSelectionReview context, CompanyHseClosureCommand original, CompanyHseClosureReceipt lifecycle)
    {
        original.Require(); lifecycle.Require(original); var r = original.Review;
        var revision = await ReviewNativeClosureRevisionAsync(context, r.Evidence.Actions.IncidentId, r.Evidence.Actions.PlanId);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, context.Database, context.Company, context.ActorUserId); await reader.OpenAsync();
        var rows = await ReadClosureResponsesAsync(reader, r.Evidence.Actions.IncidentId);
        var result = new IncidentClosureResolutionReview(context, r.LifecycleDatabase, r.Evidence.Actions.IncidentId, r.Evidence.Actions.PlanId, original.IntentId, lifecycle.GraphRevision, r.NativeRevision, revision,
            rows.Select(row => new IncidentResponseEvidence(checked((long)row.RESPONSE_OBS_NO), row.RESPONSE_TYPE, row.RESPONSE_RESULT, row.PROJECT_ID, row.REMARK, row.ACTIVE_IND, row.EFFECTIVE_DATE, row.SOURCE, row.ROW_CHANGED_BY, row.ROW_CHANGED_DATE)).ToArray());
        result.RequireAgainst(original, lifecycle);
        if (ClosureResponseRevision(rows) != revision || await ReviewNativeClosureRevisionAsync(context, result.IncidentId, result.PlanId) != revision) throw RefusalException.Conflict("The native history changed during resolution review.");
        return result;
    }
    private async Task<IncidentClosedReceipt> RecordIncidentClosedCoreAsync(CompanyHseClosureCommand original, CompanyHseClosureReceipt lifecycle, IncidentClosureResolution? resolution, Func<Task> confirmLifecycle)
    {
        original.Require(); lifecycle.Require(original); ArgumentNullException.ThrowIfNull(confirmLifecycle);
        if (original.Review.Stage != "INC_CLOSURE") throw RefusalException.Invalid("A final lifecycle closure is required.");
        original = JsonSerializer.Deserialize<CompanyHseClosureCommand>(JsonSerializer.Serialize(original))!;
        lifecycle = JsonSerializer.Deserialize<CompanyHseClosureReceipt>(JsonSerializer.Serialize(lifecycle))!;
        var review = original.Review; var prior = review.Context;
        var context = (await selections.GetCurrentFieldSelectionAsync()).SelectionContext ?? throw new UnauthorizedAccessException("A current FIELD selection is required.");
        if (prior.Company != context.Company || prior.ActorUserId != context.ActorUserId || prior.Database != context.Database || prior.FieldId != context.FieldId) throw new UnauthorizedAccessException("The native closure original belongs to another scope.");
        var actor = context.ActorUserId; var incident = review.Evidence.Actions.IncidentId; var project = review.Evidence.Actions.PlanId;
        if (resolution is not null) resolution.Review.RequireAgainst(original, lifecycle);
        var expectedRevision = resolution?.Review.Revision ?? review.NativeRevision;
        var reason = resolution?.Reason ?? original.Reason;
        var authority = await RequireAuthorityAsync(context, incident, project, actor);
        var source = editor.GetDataSource(context.Database.ConnectionName) ?? throw new InvalidOperationException("The incident datasource is unavailable.");
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, context.Database, context.Company, actor); await reader.OpenAsync();
        var definition = await metadata.GetTableMetadataAsync(nameof(HSE_INCIDENT_CLOSURE));
        if (definition?.EntityTypeName != typeof(HSE_INCIDENT_CLOSURE).FullName || definition.PrimaryKeyColumn != nameof(HSE_INCIDENT_CLOSURE.HSE_INCIDENT_CLOSURE_ID)) throw RefusalException.Conflict("The incident closure metadata is unavailable.");
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(source, editor.GetDataSource(context.Database.ConnectionName)) || await RequireAuthorityAsync(context, incident, project, actor) != authority) throw RefusalException.Conflict("The incident closure authority or datasource changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task RequireEvidence()
        {
            var current = await ReviewClosureAsync(context, incident, project, actor);
            current = current with { Actions = current.Actions with { Context = prior } };
            if (JsonSerializer.Serialize(current) != JsonSerializer.Serialize(review.Evidence) || !current.VerificationSatisfied) throw RefusalException.Conflict("The verified incident actions changed before native closure.");
        }
        async Task<IncidentClosureStoredProof[]> Read()
        {
            var rows = await reader.ReadAsync(typeof(HSE_INCIDENT_CLOSURE), [Equal("OWNER_TENANT_ID", context.Company.TenantId), Equal("OWNER_COMPANY_BA_ID", context.Company.BusinessAssociateId), Equal("FIELD_ID", context.FieldId!), Equal("INCIDENT_ID", incident)],
                ["HSE_INCIDENT_CLOSURE_ID"], ["HSE_INCIDENT_CLOSURE_ID", "OWNER_TENANT_ID", "OWNER_COMPANY_BA_ID", "FIELD_ID", "INCIDENT_ID", "PROJECT_ID", "COMMAND_JSON", "COMMAND_HASH", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]);
            if (rows.Length > 1) throw RefusalException.Conflict("The native incident closure is ambiguous.");
            return rows.Cast<HSE_INCIDENT_CLOSURE>().Select(r => r.Restore(context.Company)).ToArray();
        }
        async Task Confirm(IncidentClosureStoredProof proof)
        {
            proof.Require();
            if (JsonSerializer.Serialize(proof.Receipt.Original) != JsonSerializer.Serialize(original) || JsonSerializer.Serialize(proof.Receipt.Lifecycle) != JsonSerializer.Serialize(lifecycle)) throw RefusalException.Conflict("The recorded incident closure belongs to another original.");
            if (resolution is not null && JsonSerializer.Serialize(proof.Receipt.Resolution) != JsonSerializer.Serialize(resolution)) throw RefusalException.Conflict("The recorded native resolution belongs to another original.");
            var rows = await ReadClosureResponsesAsync(reader, incident); var status = rows.Where(r => r.RESPONSE_OBS_NO == proof.Receipt.StatusObservation).ToArray();
            if (status.Length != 1 || JsonSerializer.Serialize(status[0]) != proof.StatusJson || ClosureResponseRevision(rows) != proof.ResponseRevision || status[0].RESPONSE_RESULT != IncidentState.Closed ||
                status[0].RESPONSE_TYPE != "STATUS" || status[0].SOURCE != "Beep.OilandGas.HSE" || status[0].ACTIVE_IND != "Y" || status[0].ROW_CREATED_BY != actor || status[0].ROW_CHANGED_BY != actor || status[0].REMARK != (proof.Receipt.Resolution?.Reason ?? original.Reason))
                throw RefusalException.Conflict("The native closed incident status is unconfirmed.");
        }
        void Schema(Type type, object row, string key)
        {
            var shape = source.GetEntityStructure(new EntityStructure { EntityName = type.Name, DatasourceEntityName = type.Name }, true);
            var keys = type == typeof(HSE_INCIDENT_CLOSURE) ? new[] { key } : new[] { "INCIDENT_ID", "RESPONSE_OBS_NO" };
            if (shape?.Fields is null || !shape.Fields.Where(f => f.IsKey).Select(f => f.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal))) throw new InvalidOperationException("The native closure primary key schema is incompatible.");
            foreach (var p in type.GetProperties().Where(p => p.PropertyType == typeof(string) && p.CanRead))
            {
                if (p.GetValue(row) is not string value || value.Length == 0) continue;
                var fields = shape.Fields.Where(f => f.FieldName == p.Name).ToArray();
                if (fields.Length != 1 || fields[0].Fieldtype != typeof(string).FullName || Math.Max(fields[0].Size, fields[0].Size1) < value.Length) throw new InvalidOperationException($"The connector cannot preserve native closure field {type.Name}.{p.Name}: required length {value.Length}, matching columns {fields.Length}, available width {(fields.Length == 1 ? Math.Max(fields[0].Size, fields[0].Size1) : 0)}.");
            }
            foreach (var name in new[] { "ROW_CREATED_DATE", "ROW_CHANGED_DATE" }) if (shape.Fields.Count(f => f.FieldName == name && f.Fieldtype == typeof(DateTime).FullName) != 1) throw new InvalidOperationException("Native closure requires preserved UTC audit dates.");
        }
        // Lifecycle confirmation is outside the PPDM transaction so separate bindings never share an ambient transaction.
        await confirmLifecycle(); await Guard(false); await RequireEvidence(); await Guard(false); IncidentClosureStoredProof? proof = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true); var recorded = await Read();
            if (recorded.Length == 1) { proof = recorded[0]; await Confirm(proof); return; }
            await RequireEvidence(); await Guard(true);
            if (resolution is not null)
            {
                var currentReview = await ReviewClosureResolutionAsync(context, original, lifecycle);
                currentReview = currentReview with { Context = resolution.Review.Context };
                if (JsonSerializer.Serialize(currentReview) != JsonSerializer.Serialize(resolution.Review)) throw RefusalException.Conflict("The reviewed native resolution evidence changed.");
                await Guard(true);
            }
            var responses = await ReadClosureResponsesAsync(reader, incident);
            if (ClosureResponseRevision(responses) != expectedRevision) throw RefusalException.Conflict("The reviewed native incident responses changed. Retain the approved closure for reconciliation.");
            var active = responses.Where(r => r.ACTIVE_IND == "Y" && r.RESPONSE_TYPE == "STATUS").ToArray();
            if (active.Length == 0 || active.Any(r => !OpenIncidentState(r.RESPONSE_RESULT) || r.EFFECTIVE_DATE is not { Kind: DateTimeKind.Utc } || r.EFFECTIVE_DATE > DateTime.UtcNow) || active.Where(r => r.EFFECTIVE_DATE == active.Max(x => x.EFFECTIVE_DATE)).Count() != 1)
                throw RefusalException.Conflict("The native incident status cannot be closed from this reviewed population.");
            var observation = checked((long)responses.Max(r => r.RESPONSE_OBS_NO) + 1); var now = DateTime.UtcNow;
            if (now <= active.Max(r => r.EFFECTIVE_DATE)) throw RefusalException.Conflict("A strictly later native closure time is required.");
            var status = new HSE_INCIDENT_RESPONSE { INCIDENT_ID = incident, RESPONSE_OBS_NO = observation, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Closed, REMARK = reason, EFFECTIVE_DATE = now, ACTIVE_IND = "Y", SOURCE = "Beep.OilandGas.HSE" };
            Schema(typeof(HSE_INCIDENT_RESPONSE), status, "INCIDENT_ID");
            var statusPermit = new PPDMIncidentReportWriter.IncidentReportMutation(rowPolicy, source, status, actor, () => Guard(true));
            await new PPDMGenericRepository(editor, columns, defaults, metadata, typeof(HSE_INCIDENT_RESPONSE), context.Database.ConnectionName, nameof(HSE_INCIDENT_RESPONSE), rowAccess: statusPermit).InsertAsync(status, actor);
            var after = await ReadClosureResponsesAsync(reader, incident);
            if (JsonSerializer.Serialize(after) != JsonSerializer.Serialize(responses.Append(status).OrderBy(r => r.RESPONSE_OBS_NO))) throw RefusalException.Conflict("The native incident status population changed during closure.");
            proof = new(new IncidentClosedReceipt(original, lifecycle, observation) { Resolution = resolution }, JsonSerializer.Serialize(status), ClosureResponseRevision(after)); proof.Require();
            var row = HSE_INCIDENT_CLOSURE.Create(proof); Schema(typeof(HSE_INCIDENT_CLOSURE), row, "HSE_INCIDENT_CLOSURE_ID");
            var permit = new PPDMIncidentReportWriter.IncidentReportMutation(rowPolicy, source, row, actor, () => Guard(true));
            await new PPDMGenericRepository(editor, columns, defaults, metadata, typeof(HSE_INCIDENT_CLOSURE), context.Database.ConnectionName, nameof(HSE_INCIDENT_CLOSURE), rowAccess: permit).InsertAsync(row, actor);
            var saved = await Read(); if (saved.Length != 1 || JsonSerializer.Serialize(saved[0]) != JsonSerializer.Serialize(proof)) throw RefusalException.Conflict("The native closure original is unconfirmed.");
            await Confirm(proof); await Guard(true);
        });
        if (proof is null) throw RefusalException.Conflict("The native incident closure result is unconfirmed.");
        await Guard(false); var final = await Read(); if (final.Length != 1 || JsonSerializer.Serialize(final[0]) != JsonSerializer.Serialize(proof)) throw RefusalException.Conflict("The native incident closure commit is unconfirmed.");
        await Confirm(proof); await confirmLifecycle(); await RequireEvidence(); await Guard(false); await Confirm(proof); return proof.Receipt;
    }
}
