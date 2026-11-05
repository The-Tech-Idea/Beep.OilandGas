using System.Data;
using System.Text.Json.Nodes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Owns versioned installation-wide setup records on the dedicated Beep system target.</summary>
public sealed class SystemSetupRecordStore(SystemSetupTarget target, IDMEEditor editor, ICommonColumnHandler columns,
    IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata) : ISystemSetupRecordReader
{
    private const string Table = "SYSTEM_SETUP_RECORD";

    public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
    {
        _ = SystemSetupIdentity.ForKey(category, key); await authority(); var binding = target.Resolve();
        async Task Guard() { await authority(); RequireBinding(binding); RequireSchema(binding.Source); }
        var first = await ReadOne(category, key, binding.ConnectionName, Guard);
        var second = await ReadOne(category, key, binding.ConnectionName, Guard);
        await Guard(); if (first != second) throw new InvalidOperationException("The system setup record changed during review.");
        return second;
    }

    /// <summary>Reads a shared setup layer under the caller's serializable transaction on this exact system target.</summary>
    public Task<SystemSetupConfiguration?> ReadOwnedAsync(string category, string key, IDataSource ownedSource, Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(ownedSource);
        ArgumentNullException.ThrowIfNull(authority);
        async Task Guard()
        {
            await authority();
            var current = target.Resolve();
            if (!ReferenceEquals(current.Source, ownedSource))
                throw new InvalidOperationException("The shared setup record is not on the owned transaction target.");
            PPDMDataSourceTransaction.RequireOwned(ownedSource, IsolationLevel.Serializable);
        }
        return ReadAsync(category, key, Guard);
    }

    /// <summary>Rejects a draft whose shared inputs differ from those reviewed, within the owning write transaction.</summary>
    public async Task RequireReviewedDraftAsync(CompanyScope company, CompanyEnvironmentPreviewRequest draft,
        CompanyEnvironmentDraftReview review, IDataSource ownedSource, Func<Task> authority)
    {
        RequireReviewIdentity(company, draft, review);
        var baseline = await ReadOwnedAsync("TOOLS", "DEFAULT", ownedSource, authority);
        var country = await ReadOwnedAsync("COUNTRY", draft.CountryCode, ownedSource, authority);
        if (baseline is null || baseline.Revision != review.SystemRevision || country?.Revision != review.CountryRevision)
            throw new InvalidOperationException("The shared setup changed after the draft was reviewed.");
        var resolved = CompanyEnvironmentEffectiveResolver.BuildEnvironment(draft.CountryCode, draft.OverridesJson, baseline, country);
        if (CompanyEnvironmentEffectiveResolver.ReviewSettings(resolved).Length != 0)
            throw new InvalidOperationException("Resolve tool environment validation and source mapping issues before saving the company environment.");
        await authority();
        PPDMDataSourceTransaction.RequireOwned(ownedSource, IsolationLevel.Serializable);
    }

    public static void RequireReviewIdentity(CompanyScope company, CompanyEnvironmentPreviewRequest draft, CompanyEnvironmentDraftReview review)
    {
        ArgumentNullException.ThrowIfNull(review);
        if (review.Settings is null || review.Settings.Company != company || review.Settings.CountryCode != draft.CountryCode ||
            review.ToolsVersion != CompanyEnvironmentPreviewer.ToolsVersion ||
            review.DraftRevision != CompanyEnvironmentDraftIdentity.ForReview(company, draft, review.SystemRevision, review.CountryRevision, review.ToolsVersion))
            throw new InvalidOperationException("The reviewed company draft cannot be confirmed.");
    }

    public async Task<SystemSetupConfiguration> SaveAsync(SystemSetupRequest request, string actor, Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(request); ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        _ = SystemSetupIdentity.ForChange(request);
        var json = JsonNode.Parse(request.SettingsJson) as JsonObject ?? throw new ArgumentException("System setup settings must be a JSON object.");
        if (request.Category is "TOOLS" or "COUNTRY") CompanyEnvironmentPreviewer.RequireSafeSystemEnvironment(json);
        else { CompanyEnvironmentPreviewer.RequireUnambiguousKeys(json); CompanyEnvironmentPreviewer.RequireNoSensitiveReferences(json); }
        if (request.Category == "GHG_FACTORS") _ = GhgFactorCatalogValidation.Parse(request.Key, request.SettingsJson);
        await authority(); var binding = target.Resolve();
        async Task Guard(bool owned)
        {
            await authority(); RequireBinding(binding); RequireSchema(binding.Source);
            if (owned) PPDMDataSourceTransaction.RequireOwned(binding.Source, IsolationLevel.Serializable);
        }
        SystemSetupConfiguration? saved = null;
        await PPDMDataSourceTransaction.ExecuteAsync(binding.Source, async () =>
        {
            var existing = await ReadOne(request.Category, request.Key, binding.ConnectionName, () => Guard(true));
            var expected = SystemSetupIdentity.Prepare(request, existing);
            if (expected == existing) { saved = existing; return; }
            var row = ToRow(expected); var repository = Repository(binding.ConnectionName, new Mutation(request, actor, existing, expected, () => Guard(true)));
            if (existing is null) await repository.InsertAsync(row, actor);
            else
            {
                var original = await ReadRow(request.Category, request.Key, binding.ConnectionName, () => Guard(true));
                if (original is null || FromRow(original, request.Category, request.Key) != existing) throw new InvalidOperationException("The system setup record changed before update.");
                row.ROW_CREATED_BY = original.ROW_CREATED_BY; row.ROW_CREATED_DATE = original.ROW_CREATED_DATE;
                await repository.UpdateAsync(row, actor);
            }
            saved = await ReadOne(request.Category, request.Key, binding.ConnectionName, () => Guard(true));
            if (saved != expected) throw new InvalidOperationException("The system setup write could not be verified.");
            await Guard(true);
        });
        var first = await ReadOne(request.Category, request.Key, binding.ConnectionName, () => Guard(false));
        var second = await ReadOne(request.Category, request.Key, binding.ConnectionName, () => Guard(false));
        await Guard(false);
        if (saved is null || first != saved || second != saved) throw new InvalidOperationException("The system setup save outcome is unconfirmed.");
        return saved;
    }

    private void RequireBinding((string ConnectionName, IDataSource Source, string Fingerprint) original)
    {
        var current = target.Resolve();
        if (current.ConnectionName != original.ConnectionName || !ReferenceEquals(current.Source, original.Source) || current.Fingerprint != original.Fingerprint)
            throw new InvalidOperationException("The system setup target changed.");
    }
    private PPDMGenericRepository Repository(string connection, IPPDMRowAccessPolicy? policy = null) =>
        new(editor, columns, defaults, metadata, typeof(SYSTEM_SETUP_RECORD), connection, Table, rowAccess: policy);
    private async Task<SystemSetupConfiguration?> ReadOne(string category, string key, string connection, Func<Task> guard)
    { var row = await ReadRow(category, key, connection, guard); return row is null ? null : FromRow(row, category, key); }
    private async Task<SYSTEM_SETUP_RECORD?> ReadRow(string category, string key, string connection, Func<Task> guard)
    {
        var id = SystemSetupIdentity.ForKey(category, key); await guard();
        var rows = (await Repository(connection).GetAsync([new() { FieldName = "SYSTEM_SETUP_RECORD_ID", Operator = "=", FilterValue = id }]))?.ToArray();
        await guard();
        if (rows is null || rows.Length > 1 || rows.Any(row => row is not SYSTEM_SETUP_RECORD)) throw new InvalidOperationException("The system setup record is unavailable or ambiguous.");
        var result = rows.SingleOrDefault() as SYSTEM_SETUP_RECORD;
        if (result is not null) _ = FromRow(result, category, key);
        return result;
    }
    private static SystemSetupConfiguration FromRow(SYSTEM_SETUP_RECORD row, string category, string key)
    {
        if (row.SYSTEM_SETUP_RECORD_ID != SystemSetupIdentity.ForKey(category, key) || row.ACTIVE_IND != "Y" ||
            string.IsNullOrWhiteSpace(row.ROW_CREATED_BY) || row.ROW_CREATED_DATE is null) throw new InvalidOperationException("The system setup record audit is unconfirmed.");
        var value = new SystemSetupConfiguration(row.CATEGORY, row.SETUP_KEY, row.VERSION, row.SETTINGS_JSON, row.REVISION, row.PREVIOUS_REVISION ?? "", row.INTENT_ID);
        SystemSetupIdentity.RequireValid(value, category, key); return value;
    }
    private static SYSTEM_SETUP_RECORD ToRow(SystemSetupConfiguration value) => new()
    {
        SYSTEM_SETUP_RECORD_ID = SystemSetupIdentity.ForKey(value.Category, value.Key), CATEGORY = value.Category, SETUP_KEY = value.Key,
        VERSION = value.Version, SETTINGS_JSON = value.SettingsJson, REVISION = value.Revision,
        PREVIOUS_REVISION = value.PreviousRevision.Length == 0 ? null : value.PreviousRevision, INTENT_ID = value.IntentId, ACTIVE_IND = "Y"
    };
    private static void RequireSchema(IDataSource source)
    {
        var schema = source.GetEntityStructure(new EntityStructure { EntityName = Table, DatasourceEntityName = Table }, true);
        var keys = schema?.Fields?.Where(x => x.IsKey).ToArray();
        if (schema?.EntityName != Table || keys is not { Length: 1 } || keys[0].FieldName != Table + "_ID") throw new InvalidOperationException("Apply the system setup schema through Beep Migration.");
        foreach (var (name, size, nullable) in new[] { ("SYSTEM_SETUP_RECORD_ID", 64, false), ("CATEGORY", 64, false), ("SETUP_KEY", 128, false),
            ("VERSION", 128, false), ("SETTINGS_JSON", SystemSetupIdentity.MaximumSettingsLength, false), ("REVISION", 64, false),
            ("PREVIOUS_REVISION", 64, true), ("INTENT_ID", 64, false) })
        {
            var fields = schema.Fields.Where(x => x.FieldName == name).ToArray();
            if (fields.Length != 1 || fields[0].Fieldtype != typeof(string).FullName || fields[0].AllowDBNull != nullable || Math.Max(fields[0].Size, fields[0].Size1) < size)
                throw new InvalidOperationException("The system setup schema cannot preserve the required settings.");
        }
    }

    private sealed class Mutation(SystemSetupRequest request, string actor, SystemSetupConfiguration? existing, SystemSetupConfiguration expected, Func<Task> guard) : IPPDMRowAccessPolicy
    {
        private void RequireType(Type type, string table) { if (type != typeof(SYSTEM_SETUP_RECORD) || table != Table) throw new InvalidOperationException("Unexpected system setup record."); }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table)
        { await guard(); RequireType(type, table); return [new[] { new AppFilter { FieldName = "SYSTEM_SETUP_RECORD_ID", Operator = "=", FilterValue = SystemSetupIdentity.ForKey(request.Category, request.Key) } }]; }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) { await guard(); RequireType(type, table); return []; }
        public async Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows) { await guard(); RequireType(type, table); foreach (var row in rows) _ = FromRow((SYSTEM_SETUP_RECORD)row, request.Category, request.Key); }
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool includeInherited = true) => throw new InvalidOperationException();
        public async Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation operation, string? user)
        {
            await guard(); RequireType(type, table);
            if (user != actor || entity is not SYSTEM_SETUP_RECORD row ||
                (existing is null ? operation != PPDMRowMutation.Insert || previous is not null : operation != PPDMRowMutation.Update || previous is not SYSTEM_SETUP_RECORD old || FromRow(old, request.Category, request.Key) != existing) ||
                FromRow(row, request.Category, request.Key) != expected) throw new InvalidOperationException("The system setup mutation is unconfirmed.");
        }
    }
}
