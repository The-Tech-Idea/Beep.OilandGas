using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class PPDMCompanyIncidentCommands
{
    private static bool MatchesRecordedRca(HSE_INCIDENT_RCA_INTENT saved, HSE_INCIDENT_CAUSE cause, IncidentRcaIntent original)
    {
        var context = original.Source.Context;
        var retained = saved.Restore(context.Company, context.ActorUserId);
        if (JsonSerializer.Serialize(retained) != JsonSerializer.Serialize(original)) return false;
        return cause.INCIDENT_ID == original.Source.IncidentId && cause.CAUSE_OBS_NO == original.Source.NextObservation && cause.ACTIVE_IND == "Y" &&
            cause.CAUSE_TYPE == CauseType.Root && cause.CAUSE_CODE == original.Option.CauseCode && cause.REMARK == original.Summary &&
            cause.SOURCE == "Beep.OilandGas.HSE" && cause.EFFECTIVE_DATE is { Kind: DateTimeKind.Utc } && cause.ROW_CREATED_BY == context.ActorUserId &&
            cause.ROW_CHANGED_BY == context.ActorUserId && cause.ROW_CREATED_DATE is { Kind: DateTimeKind.Utc } created &&
            cause.ROW_CHANGED_DATE is { Kind: DateTimeKind.Utc } changed && changed >= created;
    }

    /// <summary>Recover this actor's recorded originals only after confirming each incident cause.</summary>
    public async Task<IReadOnlyList<IncidentRcaIntent>> ReadRecordedRcaAsync(FieldSelectionReview review, string incidentId)
        => (await ReadRcaProofsAsync(review, incidentId, true)).Select(item => item.Original).ToArray();

    public async Task<IReadOnlyList<IncidentRcaRecordedCause>> ReadSharedRcaAsync(FieldSelectionReview review, string incidentId)
        => (await ReadRcaProofsAsync(review, incidentId, false)).Select(item => item.Shared).ToArray();

    private async Task<IReadOnlyList<(IncidentRcaIntent Original, IncidentRcaRecordedCause Shared)>> ReadRcaProofsAsync(FieldSelectionReview review, string incidentId, bool ownOnly)
    {
        ArgumentNullException.ThrowIfNull(review);
        if (!Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        var actor = review.ActorUserId; var authority = await Canonical.CaptureHseRecordedEvidenceAuthorityAsync(selections, review, incidentId, actor);
        var definition = await metadata.GetTableMetadataAsync(nameof(HSE_INCIDENT_RCA_INTENT));
        if (definition?.EntityTypeName != typeof(HSE_INCIDENT_RCA_INTENT).FullName || definition.PrimaryKeyColumn != nameof(HSE_INCIDENT_RCA_INTENT.HSE_INCIDENT_RCA_INTENT_ID))
            throw RefusalException.Conflict("The root-cause intent metadata is unavailable.");
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_CAUSE), ["INCIDENT_ID", "CAUSE_OBS_NO"]);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        await reader.OpenAsync();
        var source = editor.GetDataSource(review.Database.ConnectionName) ?? throw new InvalidOperationException("The incident datasource is unavailable.");
        async Task Guard()
        {
            if (!ReferenceEquals(source, editor.GetDataSource(review.Database.ConnectionName)) || await Canonical.CaptureHseRecordedEvidenceAuthorityAsync(selections, review, incidentId, actor) != authority)
                throw RefusalException.Conflict("Incident authority or datasource changed during cause recovery.");
            PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task<(string Evidence, (IncidentRcaIntent Original, IncidentRcaRecordedCause Shared)[] Proofs)> Capture()
        {
            await Guard();
            var filters = new List<TheTechIdea.Beep.Report.AppFilter> { Equal("OWNER_TENANT_ID", review.Company.TenantId), Equal("OWNER_COMPANY_BA_ID", review.Company.BusinessAssociateId),
                Equal("FIELD_ID", review.FieldId!), Equal("INCIDENT_ID", incidentId), Equal("ACTIVE_IND", "Y") };
            if (ownOnly) filters.Add(Equal("ROW_CREATED_BY", actor));
            var rows = (await reader.ReadAsync(typeof(HSE_INCIDENT_RCA_INTENT),
                filters,
                ["HSE_INCIDENT_RCA_INTENT_ID"], ["HSE_INCIDENT_RCA_INTENT_ID", "OWNER_TENANT_ID", "OWNER_COMPANY_BA_ID", "FIELD_ID", "INCIDENT_ID", "CAUSE_OBS_NO", "COMMAND_JSON", "COMMAND_HASH", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]))
                .Cast<HSE_INCIDENT_RCA_INTENT>().ToArray();
            if (rows.Select(row => row.HSE_INCIDENT_RCA_INTENT_ID).Distinct(StringComparer.Ordinal).Count() != rows.Length || rows.Select(row => row.CAUSE_OBS_NO).Distinct().Count() != rows.Length)
                throw RefusalException.Conflict("Recorded cause originals are ambiguous.");
            var evidence = new List<string>(); var proofs = new List<(IncidentRcaIntent Original, IncidentRcaRecordedCause Shared)>();
            foreach (var row in rows.OrderBy(item => item.HSE_INCIDENT_RCA_INTENT_ID, StringComparer.Ordinal))
            {
                var frozenRow = JsonSerializer.Deserialize<HSE_INCIDENT_RCA_INTENT>(JsonSerializer.Serialize(row))!;
                var author = frozenRow.ROW_CREATED_BY;
                var original = frozenRow.Restore(review.Company, author); var context = original.Source.Context;
                if (context.Company != review.Company || (ownOnly && context.ActorUserId != actor) || context.Database != review.Database || context.FieldId != review.FieldId || original.Source.IncidentId != incidentId)
                    throw RefusalException.Conflict("A retained cause belongs to another company, FIELD or datasource.");
                var causes = (await reader.ReadAsync(typeof(HSE_INCIDENT_CAUSE), [Equal("INCIDENT_ID", incidentId), Equal("CAUSE_OBS_NO", original.Source.NextObservation)],
                    ["INCIDENT_ID", "CAUSE_OBS_NO"], ["INCIDENT_ID", "CAUSE_OBS_NO", "ACTIVE_IND", "CAUSE_TYPE", "CAUSE_CODE", "REMARK", "EFFECTIVE_DATE", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]))
                    .Cast<HSE_INCIDENT_CAUSE>().ToArray();
                if (causes.Length != 1 || !MatchesRecordedRca(frozenRow, causes[0], original)) throw RefusalException.Conflict("A recorded cause no longer matches its retained original.");
                var rowJson = JsonSerializer.Serialize(frozenRow); var causeJson = JsonSerializer.Serialize(causes[0]);
                evidence.Add(rowJson); evidence.Add(causeJson);
                var shared = new IncidentRcaRecordedCause(review.Company, review.Database, review.FieldId!, incidentId, original.IntentId,
                    original.Source.NextObservation, original.Option.CauseCode, original.Summary, author,
                    ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new[] { rowJson, causeJson })));
                shared.Require(); proofs.Add((original, shared));
            }
            await Guard(); return (FingerprintRecords(evidence), proofs.ToArray());
        }
        var first = await Capture(); var second = await Capture();
        if (first.Evidence != second.Evidence ||
            JsonSerializer.Serialize(first.Proofs.Select(p => p.Original)) != JsonSerializer.Serialize(second.Proofs.Select(p => p.Original)) ||
            JsonSerializer.Serialize(first.Proofs.Select(p => p.Shared)) != JsonSerializer.Serialize(second.Proofs.Select(p => p.Shared)))
            throw RefusalException.Conflict("Recorded causes changed during recovery.");
        await Guard(); return Array.AsReadOnly(second.Proofs);
    }

    /// <summary>Records one reviewed root cause and its private original in one owned PPDM transaction.</summary>
    public async Task<IncidentRcaReceipt> RecordRcaAsync(IncidentRcaIntent original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        var frozen = JsonSerializer.Deserialize<IncidentRcaIntent>(JsonSerializer.Serialize(original))!; frozen.Require();
        var review = frozen.Source.Context; var actor = review.ActorUserId; var incident = frozen.Source.IncidentId;
        var authority = await RequireAuthorityAsync(review, incident, actor);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        await reader.OpenAsync();
        var definition = await metadata.GetTableMetadataAsync(nameof(HSE_INCIDENT_RCA_INTENT));
        if (definition?.EntityTypeName != typeof(HSE_INCIDENT_RCA_INTENT).FullName || definition.PrimaryKeyColumn != nameof(HSE_INCIDENT_RCA_INTENT.HSE_INCIDENT_RCA_INTENT_ID))
            throw RefusalException.Conflict("The root-cause intent metadata is unavailable.");
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_CAUSE), ["INCIDENT_ID", "CAUSE_OBS_NO"]);
        var source = editor.GetDataSource(review.Database.ConnectionName) ?? throw new InvalidOperationException("The incident datasource is unavailable.");
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(editor.GetDataSource(review.Database.ConnectionName), source) || await RequireAuthorityAsync(review, incident, actor) != authority)
                throw RefusalException.Conflict("Incident authority or datasource changed during root-cause recording.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task RequireReviewedSource()
        {
            var current = await ReviewRcaSourceAsync(review, incident);
            if (current.Revision != frozen.Source.Revision || current.NextObservation != frozen.Source.NextObservation ||
                !current.Options.Contains(frozen.Option) || !current.Findings.SequenceEqual(frozen.Source.Findings))
                throw RefusalException.Conflict("The incident causes or available root-cause options changed. Review again.");
        }
        async Task<(object[] Intents, object[] Causes)> ReadAsync() => (
            await reader.ReadAsync(typeof(HSE_INCIDENT_RCA_INTENT), [Equal("HSE_INCIDENT_RCA_INTENT_ID", frozen.IntentId.ToString("N"))], ["HSE_INCIDENT_RCA_INTENT_ID"],
                ["HSE_INCIDENT_RCA_INTENT_ID", "OWNER_TENANT_ID", "OWNER_COMPANY_BA_ID", "FIELD_ID", "INCIDENT_ID", "CAUSE_OBS_NO", "COMMAND_JSON", "COMMAND_HASH", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
            await reader.ReadAsync(typeof(HSE_INCIDENT_CAUSE), [Equal("INCIDENT_ID", incident), Equal("CAUSE_OBS_NO", frozen.Source.NextObservation)], ["INCIDENT_ID", "CAUSE_OBS_NO"],
                ["INCIDENT_ID", "CAUSE_OBS_NO", "ACTIVE_IND", "CAUSE_TYPE", "CAUSE_CODE", "REMARK", "EFFECTIVE_DATE", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]));
        bool Matches((object[] Intents, object[] Causes) rows)
        {
            if (rows.Intents.Length != 1 || rows.Causes.Length != 1 || rows.Intents[0] is not HSE_INCIDENT_RCA_INTENT saved || rows.Causes[0] is not HSE_INCIDENT_CAUSE cause)
                return false;
            return MatchesRecordedRca(saved, cause, frozen);
        }
        void ValidateSchema()
        {
            void Table(Type type, string[] keys, params (string Name, Type Type, int Width)[] fields)
            {
                var shape = source.GetEntityStructure(new EntityStructure { EntityName = type.Name, DatasourceEntityName = type.Name }, true);
                if (shape?.Fields is null || !shape.Fields.Where(x => x.IsKey).Select(x => x.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal)))
                    throw new NativeSchemaCompatibilityException("The root-cause primary key is unavailable or incompatible.");
                foreach (var (name, typeOfField, width) in fields)
                {
                    var found = shape.Fields.Where(x => x.FieldName == name).ToArray();
                    // A signed 64-bit integral column preserves every bounded Int32 observation exactly.
                    var preservesType = found.Length == 1 && (found[0].Fieldtype == typeOfField.FullName ||
                        typeOfField == typeof(int) && found[0].Fieldtype == typeof(long).FullName);
                    if (!preservesType || width > 0 && Math.Max(found[0].Size, found[0].Size1) < width)
                        throw new NativeSchemaCompatibilityException($"The root-cause field {type.Name}.{name} cannot preserve the request: expected {typeOfField.FullName}, reported {(found.Length == 1 ? found[0].Fieldtype : "missing or ambiguous")}, required width {width}, reported width {(found.Length == 1 ? Math.Max(found[0].Size, found[0].Size1) : 0)}.");
                }
            }
            var saved = HSE_INCIDENT_RCA_INTENT.Create(frozen);
            Table(typeof(HSE_INCIDENT_RCA_INTENT), ["HSE_INCIDENT_RCA_INTENT_ID"], ("HSE_INCIDENT_RCA_INTENT_ID", typeof(string), 32),
                ("OWNER_TENANT_ID", typeof(string), review.Company.TenantId.Length), ("OWNER_COMPANY_BA_ID", typeof(string), review.Company.BusinessAssociateId.Length),
                ("FIELD_ID", typeof(string), review.FieldId!.Length), ("INCIDENT_ID", typeof(string), incident.Length), ("CAUSE_OBS_NO", typeof(int), 0),
                ("COMMAND_JSON", typeof(string), saved.COMMAND_JSON.Length), ("COMMAND_HASH", typeof(string), 64), ("ACTIVE_IND", typeof(string), 1),
                ("ROW_CREATED_BY", typeof(string), actor.Length), ("ROW_CHANGED_BY", typeof(string), actor.Length), ("ROW_CREATED_DATE", typeof(DateTime), 0), ("ROW_CHANGED_DATE", typeof(DateTime), 0));
            Table(typeof(HSE_INCIDENT_CAUSE), ["INCIDENT_ID", "CAUSE_OBS_NO"], ("INCIDENT_ID", typeof(string), incident.Length), ("CAUSE_OBS_NO", typeof(decimal), 0),
                ("CAUSE_TYPE", typeof(string), CauseType.Root.Length), ("CAUSE_CODE", typeof(string), frozen.Option.CauseCode.Length), ("REMARK", typeof(string), frozen.Summary.Length),
                ("SOURCE", typeof(string), "Beep.OilandGas.HSE".Length), ("ACTIVE_IND", typeof(string), 1), ("EFFECTIVE_DATE", typeof(DateTime), 0),
                ("ROW_CREATED_BY", typeof(string), actor.Length), ("ROW_CHANGED_BY", typeof(string), actor.Length), ("ROW_CREATED_DATE", typeof(DateTime), 0), ("ROW_CHANGED_DATE", typeof(DateTime), 0));
        }
        await Guard(false); ValidateSchema(); await Guard(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true); var before = await ReadAsync();
            if (before.Intents.Length > 0 || before.Causes.Length > 0)
            {
                if (!Matches(before)) throw RefusalException.Conflict("The original root-cause request differs from recorded evidence.");
            }
            else
            {
                await RequireReviewedSource(); await Guard(true);
                async Task Insert(IPPDMEntity row)
                {
                    var restriction = new PPDMIncidentReportWriter.IncidentReportMutation(policy, source, row, actor, async () => { await Guard(true); await RequireReviewedSource(); });
                    await new PPDMGenericRepository(editor, columns, defaults, metadata, row.GetType(), review.Database.ConnectionName, row.GetType().Name, rowAccess: restriction).InsertAsync(row, actor);
                }
                await Insert(HSE_INCIDENT_RCA_INTENT.Create(frozen));
                await Insert(new HSE_INCIDENT_CAUSE { INCIDENT_ID = incident, CAUSE_OBS_NO = frozen.Source.NextObservation, ACTIVE_IND = "Y", CAUSE_TYPE = CauseType.Root,
                    CAUSE_CODE = frozen.Option.CauseCode, REMARK = frozen.Summary, SOURCE = "Beep.OilandGas.HSE", EFFECTIVE_DATE = DateTime.UtcNow });
            }
            await Guard(true); if (!Matches(await ReadAsync())) throw RefusalException.Conflict("The recorded root cause could not be confirmed."); await Guard(true);
        });
        await Guard(false); if (!Matches(await ReadAsync())) throw RefusalException.Conflict("The root-cause commit is unconfirmed. Retain the original request."); await Guard(false);
        var receipt = new IncidentRcaReceipt(review.Company, actor, review.FieldId!, frozen.IntentId, incident, frozen.Source.NextObservation, HSE_INCIDENT_RCA_INTENT.Create(frozen).COMMAND_HASH);
        receipt.Require(frozen); return receipt;
    }
}
