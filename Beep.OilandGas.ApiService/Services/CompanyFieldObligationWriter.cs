using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Owns the paired native PPDM records for an explicitly reviewed FIELD obligation.</summary>
public sealed class CompanyFieldObligationWriter(CompanyModuleConnection company, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, IPPDMRowAccessPolicy rowPolicy)
{
    public async Task<FieldObligationCreationOptions> OptionsAsync(CompanyFieldObligationCommandScope authority)
    {
        ArgumentNullException.ThrowIfNull(authority); var review = authority.Context;
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await authority.ConfirmAsync(); await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(FIELD), ["FIELD_ID"]);
        await DefinitionAsync(metadata, typeof(R_OBLIG_TYPE), ["LAND_OBLIG_TYPE"]);
        await DefinitionAsync(metadata, typeof(R_OBLIG_COMPONENT_TYPE), ["OBLIGATION_COMPONENT_TYPE"]);
        async Task<(object[] Fields, object[] Types, object[] Components)> Read()
        {
            await authority.CheckCurrentAsync();
            var field = await reader.ReadAsync(typeof(FIELD), [Equal("FIELD_ID", review.FieldId!), Equal("ACTIVE_IND", "Y")], ["FIELD_ID", "ACTIVE_IND"]);
            if (field.Length != 1 || (await rowPolicy.GetEffectiveAccessLevelAsync(typeof(FIELD), "FIELD", field[0])) is not ("WRITE" or "DELETE"))
                throw RefusalException.Forbidden("The recorded FIELD must permit current company writes.");
            var types = await reader.ReadAsync(typeof(R_OBLIG_TYPE), [Equal("ACTIVE_IND", "Y")], ["LAND_OBLIG_TYPE", "ACTIVE_IND"], ["EFFECTIVE_DATE", "EXPIRY_DATE"]);
            var components = await reader.ReadAsync(typeof(R_OBLIG_COMPONENT_TYPE), [Equal("ACTIVE_IND", "Y")], ["OBLIGATION_COMPONENT_TYPE", "ACTIVE_IND"], ["EFFECTIVE_DATE", "EXPIRY_DATE"]);
            if (types.Any(row => !Identifier(((R_OBLIG_TYPE)row).LAND_OBLIG_TYPE)) || components.Any(row => !Identifier(((R_OBLIG_COMPONENT_TYPE)row).OBLIGATION_COMPONENT_TYPE)) ||
                types.Select(row => ((R_OBLIG_TYPE)row).LAND_OBLIG_TYPE).Distinct(StringComparer.Ordinal).Count() != types.Length ||
                components.Select(row => ((R_OBLIG_COMPONENT_TYPE)row).OBLIGATION_COMPONENT_TYPE).Distinct(StringComparer.Ordinal).Count() != components.Length)
                throw RefusalException.Conflict("The recorded obligation type catalog is invalid or ambiguous.");
            await authority.CheckCurrentAsync(); return (field, types, components);
        }
        var first = await Read(); var second = await Read();
        if (Fingerprint(first.Fields) != Fingerprint(second.Fields) || Fingerprint(first.Types) != Fingerprint(second.Types) || Fingerprint(first.Components) != Fingerprint(second.Components))
            throw RefusalException.Conflict("FIELD or obligation reference evidence changed during review.");
        await authority.ConfirmAsync();
        return new(review, first.Types.Where(row => Window(row, TimeZoneInfo.Utc, TimeProvider.System, false)).Cast<R_OBLIG_TYPE>().Select(row => row.LAND_OBLIG_TYPE).Order(StringComparer.Ordinal).ToArray(),
            first.Components.Where(row => Window(row, TimeZoneInfo.Utc, TimeProvider.System, false)).Cast<R_OBLIG_COMPONENT_TYPE>().Select(row => row.OBLIGATION_COMPONENT_TYPE).Order(StringComparer.Ordinal).ToArray());
    }

    public async Task<CreatedFieldObligation> CreateAsync(CreateFieldObligation request, CompanyFieldObligationCommandScope authority)
    {
        ArgumentNullException.ThrowIfNull(request); ArgumentNullException.ThrowIfNull(authority);
        var review = authority.Context;
        FieldSelectionReviewValidator.RequireReview(request.Context, review.Company, review.ActorUserId);
        if (!FieldObligationIdentity.SameTarget(request.Context, review) || request.IntentId == Guid.Empty || !Identifier(request.ObligationType) || !Identifier(request.ComponentType) ||
            string.IsNullOrWhiteSpace(request.Description) || request.Description != request.Description.Trim() || request.Description.Length > 255 ||
            request.Description.Any(char.IsControl) || request.CriticalDate is { Kind: not DateTimeKind.Unspecified })
            throw RefusalException.Invalid("An original FIELD review, request ID, recorded types, description and date without a time zone are required.");
        var id = "FO-" + request.IntentId.ToString("N").ToUpperInvariant();
        var marker = "OILGAS-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {
            review.Company, review.ActorUserId, review.Database, review.FieldId })) ))[..24];
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await authority.ConfirmAsync(); await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(FIELD), ["FIELD_ID"]);
        await DefinitionAsync(metadata, typeof(R_OBLIG_TYPE), ["LAND_OBLIG_TYPE"]);
        await DefinitionAsync(metadata, typeof(R_OBLIG_COMPONENT_TYPE), ["OBLIGATION_COMPONENT_TYPE"]);
        await DefinitionAsync(metadata, typeof(OBLIGATION), ["OBLIGATION_ID", "OBLIGATION_SEQ_NO"]);
        await DefinitionAsync(metadata, typeof(OBLIGATION_COMPONENT), ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "COMPONENT_OBS_NO"]);
        var source = editor.GetDataSource(review.Database.ConnectionName) ?? throw new InvalidOperationException("The obligation datasource is unavailable.");

        void RequireSchema()
        {
            void Validate(string table, string[] keys, (string Name, Type Type, int Width)[] required)
            {
                var schema = source.GetEntityStructure(new EntityStructure { EntityName = table, DatasourceEntityName = table }, true)
                    ?? throw new NativeSchemaCompatibilityException($"The {table} schema is unavailable. Review the Beep migration.");
                if (schema.Fields is null || !schema.Fields.Where(field => field.IsKey).Select(field => field.FieldName)
                    .Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal)))
                    throw new NativeSchemaCompatibilityException($"The {table} physical primary key does not match the canonical PPDM key.");
                foreach (var (name, type, width) in required)
                {
                    var fields = schema.Fields.Where(field => string.Equals(field.FieldName, name, StringComparison.Ordinal)).ToArray();
                    if (fields.Length != 1 || fields[0].Fieldtype != type.FullName ||
                        width > 0 && Math.Max(fields[0].Size, fields[0].Size1) < width)
                        throw new NativeSchemaCompatibilityException($"The {table}.{name} physical column cannot preserve this obligation. Review the Beep migration.");
                }
            }
            var actorWidth = review.ActorUserId.Length;
            Validate("OBLIGATION", ["OBLIGATION_ID", "OBLIGATION_SEQ_NO"],
                [("OBLIGATION_ID", typeof(string), id.Length), ("OBLIGATION_SEQ_NO", typeof(decimal), 0), ("ACTIVE_IND", typeof(string), 1),
                 ("OBLIGATION_TYPE", typeof(string), request.ObligationType.Length), ("DESCRIPTION", typeof(string), request.Description.Length),
                 ("CRITICAL_DATE", typeof(DateTime), 0), ("SOURCE", typeof(string), marker.Length), ("ROW_CREATED_BY", typeof(string), actorWidth),
                 ("ROW_CREATED_DATE", typeof(DateTime), 0), ("ROW_CHANGED_BY", typeof(string), actorWidth), ("ROW_CHANGED_DATE", typeof(DateTime), 0)]);
            Validate("OBLIGATION_COMPONENT", ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "COMPONENT_OBS_NO"],
                [("OBLIGATION_ID", typeof(string), id.Length), ("OBLIGATION_SEQ_NO", typeof(decimal), 0), ("COMPONENT_OBS_NO", typeof(decimal), 0),
                 ("FIELD_ID", typeof(string), review.FieldId!.Length), ("ACTIVE_IND", typeof(string), 1),
                 ("OBLIGATION_COMPONENT_TYPE", typeof(string), request.ComponentType.Length), ("SOURCE", typeof(string), marker.Length),
                 ("ROW_CREATED_BY", typeof(string), actorWidth), ("ROW_CREATED_DATE", typeof(DateTime), 0),
                 ("ROW_CHANGED_BY", typeof(string), actorWidth), ("ROW_CHANGED_DATE", typeof(DateTime), 0)]);
        }

        async Task Evidence()
        {
            await authority.CheckCurrentAsync();
            var field = await reader.ReadAsync(typeof(FIELD), [Equal("FIELD_ID", review.FieldId!), Equal("ACTIVE_IND", "Y")], ["FIELD_ID", "ACTIVE_IND"]);
            if (field.Length != 1 || (await rowPolicy.GetEffectiveAccessLevelAsync(typeof(FIELD), "FIELD", field[0])) is not ("WRITE" or "DELETE"))
                throw RefusalException.Forbidden("The recorded FIELD must permit current company writes.");
            var types = await reader.ReadAsync(typeof(R_OBLIG_TYPE), [Equal("LAND_OBLIG_TYPE", request.ObligationType), Equal("ACTIVE_IND", "Y")],
                ["LAND_OBLIG_TYPE", "ACTIVE_IND"], ["EFFECTIVE_DATE", "EXPIRY_DATE"]);
            var components = await reader.ReadAsync(typeof(R_OBLIG_COMPONENT_TYPE), [Equal("OBLIGATION_COMPONENT_TYPE", request.ComponentType), Equal("ACTIVE_IND", "Y")],
                ["OBLIGATION_COMPONENT_TYPE", "ACTIVE_IND"], ["EFFECTIVE_DATE", "EXPIRY_DATE"]);
            if (types.Length != 1 || components.Length != 1 || !Window(types[0], TimeZoneInfo.Utc, TimeProvider.System, false) || !Window(components[0], TimeZoneInfo.Utc, TimeProvider.System, false))
                throw RefusalException.Conflict("The selected recorded obligation and component types must be active and unambiguous.");
            await authority.CheckCurrentAsync();
        }
        async Task<(OBLIGATION[] Parents, OBLIGATION_COMPONENT[] Links)> Read()
        {
            var parents = (await reader.ReadAsync(typeof(OBLIGATION), [Equal("OBLIGATION_ID", id)],
                ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "ACTIVE_IND"], ["OBLIGATION_TYPE", "DESCRIPTION", "CRITICAL_DATE", "SOURCE", "ROW_CREATED_BY"])).Cast<OBLIGATION>().ToArray();
            var links = (await reader.ReadAsync(typeof(OBLIGATION_COMPONENT), [Equal("OBLIGATION_ID", id)],
                ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "COMPONENT_OBS_NO", "FIELD_ID", "ACTIVE_IND"], ["OBLIGATION_COMPONENT_TYPE", "SOURCE", "ROW_CREATED_BY"])).Cast<OBLIGATION_COMPONENT>().ToArray();
            return (parents, links);
        }
        bool Matches((OBLIGATION[] Parents, OBLIGATION_COMPONENT[] Links) rows) => rows.Parents.Length == 1 && rows.Links.Length == 1 &&
            rows.Parents[0] is var parent && parent.OBLIGATION_SEQ_NO == 1 && parent.ACTIVE_IND == "Y" && parent.OBLIGATION_TYPE == request.ObligationType &&
            parent.DESCRIPTION == request.Description && parent.CRITICAL_DATE == request.CriticalDate && parent.SOURCE == marker && parent.ROW_CREATED_BY == review.ActorUserId &&
            rows.Links[0] is var link && link.OBLIGATION_SEQ_NO == 1 && link.COMPONENT_OBS_NO == 1 && link.FIELD_ID == review.FieldId &&
            link.ACTIVE_IND == "Y" && link.OBLIGATION_COMPONENT_TYPE == request.ComponentType && link.SOURCE == marker && link.ROW_CREATED_BY == review.ActorUserId;

        await Evidence(); RequireSchema();
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); await Evidence(); RequireSchema();
            var before = await Read();
            if (before.Parents.Length != 0 || before.Links.Length != 0)
            {
                if (!Matches(before)) throw RefusalException.Conflict("The request ID already belongs to different or incomplete recorded evidence.");
            }
            else
            {
                var parent = new OBLIGATION { OBLIGATION_ID = id, OBLIGATION_SEQ_NO = 1, ACTIVE_IND = "Y", OBLIGATION_TYPE = request.ObligationType,
                    DESCRIPTION = request.Description, CRITICAL_DATE = request.CriticalDate, SOURCE = marker };
                var link = new OBLIGATION_COMPONENT { OBLIGATION_ID = id, OBLIGATION_SEQ_NO = 1, COMPONENT_OBS_NO = 1, FIELD_ID = review.FieldId!,
                    ACTIVE_IND = "Y", OBLIGATION_COMPONENT_TYPE = request.ComponentType, SOURCE = marker };
                await Insert(parent); await Insert(link);
            }
            await Evidence();
            if (!Matches(await Read())) throw RefusalException.Conflict("The paired obligation records could not be confirmed before commit.");
            PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
        });
        await authority.ConfirmAsync(); await Evidence();
        if (!Matches(await Read())) throw RefusalException.Conflict("The committed obligation outcome is unconfirmed. Retry the same request.");
        await authority.ConfirmAsync();
        return new(review, id, 1, 1);

        async Task Insert(IPPDMEntity row)
        {
            var policy = new MutationPolicy(rowPolicy, source, row, review.ActorUserId, authority.CheckCurrentAsync);
            var repository = new PPDMGenericRepository(editor, columns, defaults, metadata, row.GetType(), review.Database.ConnectionName, row.GetType().Name, rowAccess: policy);
            await repository.InsertAsync(row, review.ActorUserId);
        }
    }

    private sealed class MutationPolicy(IPPDMRowAccessPolicy owner, IDataSource source, IPPDMEntity expected, string actor, Func<Task> check) : IPPDMRowAccessPolicy
    {
        private readonly string payload = Payload(expected); private int used;
        private static string Payload(object row) => JsonSerializer.Serialize(row.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(p => p.CanRead && p.CanWrite && p.Name is not ("ROW_CREATED_BY" or "ROW_CREATED_DATE" or "ROW_CHANGED_BY" or "ROW_CHANGED_DATE" or "PPDM_GUID" or "ROW_EFFECTIVE_DATE"))
            .OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => new object?[] { p.Name, p.GetValue(row) }));
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table) => owner.GetReadConjunctionsAsync(type, table);
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) => owner.GetReadConstraintsAsync(type, table);
        public Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows) => owner.ValidateRowsAsync(type, table, rows);
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool includeInherited = true) => owner.GetEffectiveAccessLevelAsync(type, table, row, includeInherited);
        public async Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation operation, string? assertedActor)
        {
            PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
            if (type != expected.GetType() || table != type.Name || entity.GetType() != type || previous is not null || operation != PPDMRowMutation.Insert ||
                actor != assertedActor || Payload(entity) != payload || Interlocked.Exchange(ref used, 1) != 0 || entity is not IPPDMEntity row || row.ROW_CREATED_BY != actor)
                throw RefusalException.Forbidden("The mutation differs from the original obligation command.");
            await check(); PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
        }
    }
}
