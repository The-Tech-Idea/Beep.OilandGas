using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

internal sealed class PPDMIncidentReportWriter(CompanyModuleConnection company, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, IPPDMRowAccessPolicy policy)
{
    private const string Source = "Beep.OilandGas.HSE";
    public async Task<string> CreateAsync(IncidentReportIntent intent, Func<Task> RequireCurrentAsync)
    {
        ArgumentNullException.ThrowIfNull(intent); ArgumentNullException.ThrowIfNull(RequireCurrentAsync); intent.Require();
        var review = intent.Context; var id = intent.IncidentId(); var actor = review.ActorUserId;
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        await reader.OpenAsync();
        var intentDefinition = await metadata.GetTableMetadataAsync(nameof(HSE_INCIDENT_REPORT_INTENT));
        if (intentDefinition?.EntityTypeName != typeof(HSE_INCIDENT_REPORT_INTENT).FullName || intentDefinition.PrimaryKeyColumn != "HSE_INCIDENT_REPORT_INTENT_ID")
            throw RefusalException.Conflict("The canonical incident intent metadata is unavailable.");
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT), ["INCIDENT_ID"]);
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_DETAIL), ["INCIDENT_ID", "DETAIL_OBS_NO"], ("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"), ("DETAIL_TYPE", "R_HSE_INCIDENT_DETAIL", "DETAIL_TYPE"));
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_COMPONENT), ["INCIDENT_ID", "COMPONENT_OBS_NO"], ("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"), ("FIELD_ID", "FIELD", "FIELD_ID"));
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_RESPONSE), ["INCIDENT_ID", "RESPONSE_OBS_NO"], ("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"), ("RESPONSE_TYPE", "R_HSE_RESPONSE_TYPE", "ACTION_TYPE"), ("RESPONSE_RESULT", "R_INCIDENT_RESP_RESULT", "RESPONSE_RESULT"));
        var source = editor.GetDataSource(review.Database.ConnectionName) ?? throw new InvalidOperationException("The incident datasource is unavailable.");

        async Task<string> ReferencesAsync()
        {
            var snapshots = new List<string>();
            async Task One(Type type, string key, string value)
            {
                await DefinitionAsync(metadata, type, [key]);
                var rows = await reader.ReadAsync(type, [Equal(key, value), Equal("ACTIVE_IND", "Y")], [key, "ACTIVE_IND"], ["EFFECTIVE_DATE", "EXPIRY_DATE"]);
                if (rows.Length != 1 || type.GetProperty("EFFECTIVE_DATE")!.GetValue(rows[0]) is DateTime { Kind: not DateTimeKind.Utc } ||
                    type.GetProperty("EXPIRY_DATE")!.GetValue(rows[0]) is DateTime { Kind: not DateTimeKind.Utc } ||
                    !Window(rows[0], TimeZoneInfo.Utc, TimeProvider.System, false))
                    throw RefusalException.Conflict("A required recorded incident reference is unavailable or ambiguous.");
                snapshots.Add(Fingerprint(rows));
            }
            await One(typeof(R_HSE_INCIDENT_DETAIL), "DETAIL_TYPE", "PRIMARY");
            await One(typeof(R_HSE_COMP_ROLE), "COMPONENT_ROLE", HseReferenceCodes.FieldContext.PrimaryRole);
            await One(typeof(R_HSE_INCIDENT_COMP_TYPE), "COMPONENT_TYPE", HseReferenceCodes.FieldContext.ComponentType);
            await One(typeof(R_HSE_RESPONSE_TYPE), "ACTION_TYPE", "STATUS");
            await One(typeof(R_INCIDENT_RESP_RESULT), "RESPONSE_RESULT", IncidentState.Reported);
            return FingerprintRecords(snapshots);
        }
        async Task<object[][]> ReadAsync()
        {
            async Task<object[]> Read(Type type, string key, string value, string[] columns) =>
                await reader.ReadAsync(type, [Equal(key, value)], [key], columns);
            return [await Read(typeof(HSE_INCIDENT_REPORT_INTENT), "HSE_INCIDENT_REPORT_INTENT_ID", intent.IntentId.ToString("N"), ["OWNER_TENANT_ID", "OWNER_COMPANY_BA_ID", "FIELD_ID", "INCIDENT_ID", "COMMAND_JSON", "COMMAND_HASH", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
                await Read(typeof(HSE_INCIDENT), "INCIDENT_ID", id, ["INCIDENT_DATE", "REMARK", "ACTIVE_IND", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
                await Read(typeof(HSE_INCIDENT_DETAIL), "INCIDENT_ID", id, ["DETAIL_OBS_NO", "DETAIL_TYPE", "INCIDENT_SET_ID", "INCIDENT_TYPE_ID", "INCIDENT_SEVERITY_ID", "INCIDENT_DATE", "REMARK", "ACTIVE_IND", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
                await Read(typeof(HSE_INCIDENT_COMPONENT), "INCIDENT_ID", id, ["COMPONENT_OBS_NO", "FIELD_ID", "COMPONENT_ROLE", "COMPONENT_TYPE", "REMARK", "JURISDICTION", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
                await Read(typeof(HSE_INCIDENT_RESPONSE), "INCIDENT_ID", id, ["RESPONSE_OBS_NO", "RESPONSE_TYPE", "RESPONSE_RESULT", "EFFECTIVE_DATE", "ACTIVE_IND", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"])];
        }
        bool Matches(object[][] rows)
        {
            if (rows.Any(group => group.Length != 1)) return false;
            if (rows[0][0] is not HSE_INCIDENT_REPORT_INTENT saved || saved.Restore(review.Company, actor) != intent) return false;
            var h = (HSE_INCIDENT)rows[1][0]; var d = (HSE_INCIDENT_DETAIL)rows[2][0]; var c = (HSE_INCIDENT_COMPONENT)rows[3][0]; var r = (HSE_INCIDENT_RESPONSE)rows[4][0];
            bool Audit(IPPDMEntity row) => row.ROW_CREATED_BY == actor && row.ROW_CHANGED_BY == actor &&
                row.ROW_CREATED_DATE is { Kind: DateTimeKind.Utc } created && row.ROW_CHANGED_DATE is { Kind: DateTimeKind.Utc } changed && changed >= created;
            if (!Audit(h) || !Audit(d) || !Audit(c) || !Audit(r)) return false;
            return h.INCIDENT_DATE == intent.IncidentUtc && h.REMARK == intent.Description && h.ACTIVE_IND == "Y" && h.SOURCE == Source && h.ROW_CREATED_BY == actor &&
                d.DETAIL_OBS_NO == 1 && d.DETAIL_TYPE == "PRIMARY" && d.INCIDENT_SET_ID == intent.Option.IncidentSetId && d.INCIDENT_TYPE_ID == intent.Option.IncidentTypeId && d.INCIDENT_SEVERITY_ID == intent.Option.SeverityId && d.INCIDENT_DATE == intent.IncidentUtc && d.REMARK == intent.Description && d.ACTIVE_IND == "Y" && d.SOURCE == Source && d.ROW_CREATED_BY == actor &&
                c.COMPONENT_OBS_NO == 1 && c.FIELD_ID == review.FieldId && c.COMPONENT_ROLE == HseReferenceCodes.FieldContext.PrimaryRole && c.COMPONENT_TYPE == HseReferenceCodes.FieldContext.ComponentType && c.REMARK == intent.Location && string.IsNullOrEmpty(c.JURISDICTION) && c.ACTIVE_IND == "Y" && c.ROW_CREATED_BY == actor &&
                r.RESPONSE_OBS_NO == 1 && r.RESPONSE_TYPE == "STATUS" && r.RESPONSE_RESULT == IncidentState.Reported && r.EFFECTIVE_DATE is { Kind: DateTimeKind.Utc } && r.ACTIVE_IND == "Y" && r.SOURCE == Source && r.ROW_CREATED_BY == actor;
        }
        await RequireCurrentAsync(); var references = await ReferencesAsync(); IncidentReportSchema.Validate(source, intent, Source);
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); await RequireCurrentAsync();
            if (await ReferencesAsync() != references) throw RefusalException.Conflict("The incident reference evidence changed before creation.");
            IncidentReportSchema.Validate(source, intent, Source);
            var before = await ReadAsync();
            if (before.Any(group => group.Length != 0))
            {
                if (!Matches(before)) throw RefusalException.Conflict("This report identity belongs to different or incomplete evidence.");
            }
            else
            {
                await Insert(new HSE_INCIDENT { INCIDENT_ID = id, INCIDENT_DATE = intent.IncidentUtc, REMARK = intent.Description, ACTIVE_IND = "Y", SOURCE = Source });
                await Insert(new HSE_INCIDENT_DETAIL { INCIDENT_ID = id, DETAIL_OBS_NO = 1, DETAIL_TYPE = "PRIMARY", INCIDENT_SET_ID = intent.Option.IncidentSetId, INCIDENT_TYPE_ID = intent.Option.IncidentTypeId, INCIDENT_SEVERITY_ID = intent.Option.SeverityId, INCIDENT_DATE = intent.IncidentUtc, REMARK = intent.Description, ACTIVE_IND = "Y", SOURCE = Source });
                await Insert(new HSE_INCIDENT_COMPONENT { INCIDENT_ID = id, COMPONENT_OBS_NO = 1, FIELD_ID = review.FieldId!, COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType, REMARK = intent.Location, ACTIVE_IND = "Y" });
                await Insert(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = id, RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Reported, EFFECTIVE_DATE = DateTime.UtcNow, ACTIVE_IND = "Y", SOURCE = Source });
                await Insert(HSE_INCIDENT_REPORT_INTENT.Create(intent));
            }
            await RequireCurrentAsync();
            if (await ReferencesAsync() != references) throw RefusalException.Conflict("The incident reference evidence changed during creation.");
            if (!Matches(await ReadAsync())) throw RefusalException.Conflict("The incident report records could not be confirmed before commit.");
            PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
        });
        await RequireCurrentAsync();
        if (await ReferencesAsync() != references) throw RefusalException.Conflict("The incident reference evidence changed before confirmation.");
        if (!Matches(await ReadAsync())) throw RefusalException.Conflict("The committed incident report is unconfirmed. Reconcile the original request.");
        await RequireCurrentAsync(); return id;

        async Task Insert(IPPDMEntity row)
        {
            var commandPolicy = new IncidentReportMutation(policy, source, row, actor, RequireCurrentAsync);
            await new PPDMGenericRepository(editor, columns, defaults, metadata, row.GetType(), review.Database.ConnectionName, row.GetType().Name, rowAccess: commandPolicy).InsertAsync(row, actor);
        }
    }

    internal sealed class IncidentReportMutation(IPPDMRowAccessPolicy owner, IDataSource source, IPPDMEntity expected, string actor, Func<Task> check) : IPPDMRowAccessPolicy
    {
        private readonly string payload = Payload(expected); private readonly string active = expected.ACTIVE_IND; private int used;
        private static string Payload(object row) => JsonSerializer.Serialize(row.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(p => p.CanRead && p.CanWrite && p.Name is not ("ROW_CREATED_BY" or "ROW_CREATED_DATE" or "ROW_CHANGED_BY" or "ROW_CHANGED_DATE" or "PPDM_GUID" or "ROW_EFFECTIVE_DATE" or "ROW_EXPIRY_DATE"))
            .OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => new object?[] { p.Name, p.GetValue(row) }));
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table) => owner.GetReadConjunctionsAsync(type, table);
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) => owner.GetReadConstraintsAsync(type, table);
        public Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows) => owner.ValidateRowsAsync(type, table, rows);
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool includeInherited = true) => owner.GetEffectiveAccessLevelAsync(type, table, row, includeInherited);
        public async Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation operation, string? assertedActor)
        {
            PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
            if (type != expected.GetType() || table != type.Name || entity.GetType() != type || previous is not null || operation != PPDMRowMutation.Insert ||
                actor != assertedActor || Payload(entity) != payload || Interlocked.Exchange(ref used, 1) != 0 || entity is not IPPDMEntity row || row.ACTIVE_IND != active || row.ROW_CREATED_BY != actor || row.ROW_CHANGED_BY != actor ||
                row.ROW_CREATED_DATE is not { Kind: DateTimeKind.Utc } created || row.ROW_CHANGED_DATE is not { Kind: DateTimeKind.Utc } changed || changed < created)
                throw RefusalException.Forbidden("The mutation differs from the reviewed incident request.");
            await check(); PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
        }
    }
}
