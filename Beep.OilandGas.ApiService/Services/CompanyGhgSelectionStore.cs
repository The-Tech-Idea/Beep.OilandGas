using System.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Owns company catalog selections on the installation-wide system setup datasource.</summary>
public sealed class CompanyGhgSelectionStore(SystemSetupTarget target, IDMEEditor editor, ICommonColumnHandler columns,
    IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, CompanyGhgCatalogReader reviews) : ICompanyGhgSelectionStore
{
    private const string Table = "COMPANY_GHG_CATALOG_SELECTION";

    public async Task<CompanyGhgSelection?> ReadAsync(CompanyScope company, Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(authority);
        await authority();
        var binding = target.Resolve();
        async Task Guard() { await authority(); RequireBinding(binding); RequireSchema(binding.Source); }
        var first = await ReadOne(company, binding.ConnectionName, Guard);
        var second = await ReadOne(company, binding.ConnectionName, Guard);
        await Guard();
        if (first != second) throw new InvalidOperationException("The company catalog selection changed during review.");
        return second;
    }

    public async Task<CompanyGhgSelection> SaveAsync(CompanyScope company, string actor, CompanyGhgSelectionRequest request, Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(authority);
        await authority();
        var binding = target.Resolve();
        async Task Guard(bool owned)
        {
            await authority(); RequireBinding(binding); RequireSchema(binding.Source);
            if (owned) PPDMDataSourceTransaction.RequireOwned(binding.Source, IsolationLevel.Serializable);
        }
        async Task RequireReviewed()
        {
            await Guard(true);
            var review = await reviews.ReviewAsync(company, request.Jurisdiction, () => Guard(true));
            if (review.Company != company || review.Catalog.Revision != request.CatalogRevision || review.EffectiveRevision != request.EnvironmentRevision)
                throw new InvalidOperationException("The catalog or company environment changed since review.");
            await Guard(true);
        }
        CompanyGhgSelection? saved = null;
        await PPDMDataSourceTransaction.ExecuteAsync(binding.Source, async () =>
        {
            await Guard(true);
            var existing = await ReadOne(company, binding.ConnectionName, () => Guard(true));
            var expected = CompanyGhgSelectionIdentity.Prepare(company, actor, request, existing);
            if (existing == expected) { saved = existing; return; }
            await RequireReviewed();
            var row = ToRow(expected);
            var repository = Repository(binding.ConnectionName, new Mutation(company, actor, existing, expected, () => Guard(true)));
            if (existing is null) await repository.InsertAsync(row, actor);
            else
            {
                var original = await ReadRow(company, binding.ConnectionName, () => Guard(true));
                if (original is null || ToSelection(original, company) != existing) throw new InvalidOperationException("The company selection changed before update.");
                row.ROW_CREATED_BY = original.ROW_CREATED_BY; row.ROW_CREATED_DATE = original.ROW_CREATED_DATE;
                await repository.UpdateAsync(row, actor);
            }
            saved = await ReadOne(company, binding.ConnectionName, () => Guard(true));
            if (saved != expected) throw new InvalidOperationException("The company selection write is unconfirmed.");
            await RequireReviewed();
        });
        var first = await ReadOne(company, binding.ConnectionName, () => Guard(false));
        var second = await ReadOne(company, binding.ConnectionName, () => Guard(false));
        await Guard(false);
        if (saved is null || first != saved || second != saved) throw new InvalidOperationException("The company selection outcome is unconfirmed.");
        return saved;
    }

    private void RequireBinding((string ConnectionName, IDataSource Source, string Fingerprint) original)
    {
        var current = target.Resolve();
        if (current.ConnectionName != original.ConnectionName || !ReferenceEquals(current.Source, original.Source) || current.Fingerprint != original.Fingerprint)
            throw new InvalidOperationException("The system setup target changed.");
    }
    private PPDMGenericRepository Repository(string connection, IPPDMRowAccessPolicy? policy = null) =>
        new(editor, columns, defaults, metadata, typeof(COMPANY_GHG_CATALOG_SELECTION), connection, Table, rowAccess: policy);
    private async Task<CompanyGhgSelection?> ReadOne(CompanyScope company, string connection, Func<Task> guard)
    { var row = await ReadRow(company, connection, guard); return row is null ? null : ToSelection(row, company); }
    private async Task<COMPANY_GHG_CATALOG_SELECTION?> ReadRow(CompanyScope company, string connection, Func<Task> guard)
    {
        await guard();
        var rows = (await Repository(connection).GetAsync([new() { FieldName = Table + "_ID", Operator = "=", FilterValue = CompanyGhgSelectionIdentity.ForCompany(company) }]))?.ToArray();
        await guard();
        if (rows is null || rows.Length > 1 || rows.Any(x => x is not COMPANY_GHG_CATALOG_SELECTION)) throw new InvalidOperationException("The company selection is unavailable or ambiguous.");
        var row = rows.SingleOrDefault() as COMPANY_GHG_CATALOG_SELECTION;
        if (row is not null) _ = ToSelection(row, company);
        return row;
    }
    private static CompanyGhgSelection ToSelection(COMPANY_GHG_CATALOG_SELECTION row, CompanyScope company)
    {
        if (row.COMPANY_GHG_CATALOG_SELECTION_ID != CompanyGhgSelectionIdentity.ForCompany(company) || row.OWNER_TENANT_ID != company.TenantId ||
            row.OWNER_COMPANY_BA_ID != company.BusinessAssociateId || row.ACTIVE_IND != "Y" || string.IsNullOrWhiteSpace(row.ROW_CREATED_BY) || row.ROW_CREATED_DATE is null)
            throw new InvalidOperationException("The selection ownership or audit is unconfirmed.");
        var value = new CompanyGhgSelection(company, row.JURISDICTION_KEY, row.CATALOG_REVISION, row.ENVIRONMENT_REVISION,
            row.SELECTED_BY, row.RATIONALE, row.REVISION, row.PREVIOUS_REVISION ?? "", row.INTENT_ID);
        CompanyGhgSelectionIdentity.RequireValid(value, company); return value;
    }
    private static COMPANY_GHG_CATALOG_SELECTION ToRow(CompanyGhgSelection value) => new()
    {
        COMPANY_GHG_CATALOG_SELECTION_ID = CompanyGhgSelectionIdentity.ForCompany(value.Company), OWNER_TENANT_ID = value.Company.TenantId,
        OWNER_COMPANY_BA_ID = value.Company.BusinessAssociateId, JURISDICTION_KEY = value.Jurisdiction, CATALOG_REVISION = value.CatalogRevision,
        ENVIRONMENT_REVISION = value.EnvironmentRevision, SELECTED_BY = value.SelectedBy, RATIONALE = value.Rationale, REVISION = value.Revision,
        PREVIOUS_REVISION = value.PreviousRevision.Length == 0 ? null : value.PreviousRevision, INTENT_ID = value.IntentId, ACTIVE_IND = "Y"
    };
    private static void RequireSchema(IDataSource source)
    {
        var schema = source.GetEntityStructure(new EntityStructure { EntityName = Table, DatasourceEntityName = Table }, true);
        var keys = schema?.Fields?.Where(x => x.IsKey).ToArray();
        if (schema?.EntityName != Table || keys is not { Length: 1 } || keys[0].FieldName != Table + "_ID") throw new InvalidOperationException("Apply the system setup migration for company selections.");
        foreach (var (name, size, nullable) in new[] { (Table + "_ID", 64, false), ("OWNER_TENANT_ID", 64, false), ("OWNER_COMPANY_BA_ID", 128, false),
            ("JURISDICTION_KEY", 128, false), ("CATALOG_REVISION", 64, false), ("ENVIRONMENT_REVISION", 64, false), ("SELECTED_BY", 128, false),
            ("RATIONALE", 2000, false), ("REVISION", 64, false), ("PREVIOUS_REVISION", 64, true), ("INTENT_ID", 64, false) })
        {
            var fields = schema.Fields.Where(x => x.FieldName == name).ToArray();
            if (fields.Length != 1 || fields[0].Fieldtype != typeof(string).FullName || fields[0].AllowDBNull != nullable || Math.Max(fields[0].Size, fields[0].Size1) < size)
                throw new InvalidOperationException("The company selection schema cannot preserve the required evidence.");
        }
    }
    private sealed class Mutation(CompanyScope company, string actor, CompanyGhgSelection? existing, CompanyGhgSelection expected, Func<Task> guard) : IPPDMRowAccessPolicy
    {
        private static AppFilter Filter(string field, string value) => new() { FieldName = field, Operator = "=", FilterValue = value };
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table)
        { await guard(); RequireType(type, table); return [new[] { Filter("OWNER_TENANT_ID", company.TenantId), Filter("OWNER_COMPANY_BA_ID", company.BusinessAssociateId) }]; }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) { await guard(); RequireType(type, table); return []; }
        public async Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows) { await guard(); RequireType(type, table); foreach (var row in rows) _ = ToSelection((COMPANY_GHG_CATALOG_SELECTION)row, company); }
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool includeInherited = true) => throw new InvalidOperationException();
        public async Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation operation, string? user)
        {
            await guard(); RequireType(type, table);
            if (user != actor || entity is not COMPANY_GHG_CATALOG_SELECTION row ||
                (existing is null ? operation != PPDMRowMutation.Insert || previous is not null : operation != PPDMRowMutation.Update || previous is not COMPANY_GHG_CATALOG_SELECTION old || ToSelection(old, company) != existing) ||
                ToSelection(row, company) != expected) throw new InvalidOperationException("The original company selection mutation is unconfirmed.");
        }
        private static void RequireType(Type type, string table) { if (type != typeof(COMPANY_GHG_CATALOG_SELECTION) || table != Table) throw new InvalidOperationException("Unexpected company selection record."); }
    }
}
