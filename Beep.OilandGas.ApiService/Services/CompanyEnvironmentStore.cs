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

/// <summary>Company environment owner on the installation-wide Beep system setup datasource.</summary>
public sealed class CompanyEnvironmentStore(IDMEEditor editor, ICommonColumnHandler columns,
    IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, Func<Task<string>> resolveConnection,
    Func<CompanyScope, CompanyEnvironmentPreviewRequest, CompanyEnvironmentDraftReview, IDataSource, Func<Task>, Task> requireReviewedDraft) : ICompanyEnvironmentStore
{
    private const string Table = "COMPANY_OILGAS_ENVIRONMENT";

    public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority)
    {
        var source = await BindAsync(authority);
        async Task Guard() { await authority(); await source.RequireBinding(); RequireSchema(source.DataSource); }
        var first = await ReadOne(company, source.Connection, Guard);
        var second = await ReadOne(company, source.Connection, Guard);
        await Guard();
        if (first != second) throw new InvalidOperationException("The company environment changed during review.");
        return second;
    }

    public async Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor,
        CompanyEnvironmentReviewedSaveRequest command, Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(command);
        var request = command.Change ?? throw new ArgumentException("The company environment change is required.");
        var review = command.Review ?? throw new ArgumentException("A draft review is required.");
        SystemSetupRecordStore.RequireReviewIdentity(company, new(request.CountryCode, request.OverridesJson), review);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        _ = CompanyEnvironmentPreviewer.ValidateForSave(company, request);
        var version = CompanyEnvironmentPreviewer.ToolsVersion;
        var source = await BindAsync(authority);
        async Task Guard(bool owned)
        {
            await authority(); await source.RequireBinding();
            if (owned) PPDMDataSourceTransaction.RequireOwned(source.DataSource, IsolationLevel.Serializable);
            RequireSchema(source.DataSource);
        }
        CompanyEnvironmentConfiguration? saved = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source.DataSource, async () =>
        {
            await Guard(true);
            var existing = await ReadOne(company, source.Connection, () => Guard(true));
            var expected = CompanyEnvironmentIdentity.Prepare(company, request, version, existing, review.SystemRevision, review.CountryRevision);
            if (existing == expected) { saved = existing; return; }
            await requireReviewedDraft(company, new(request.CountryCode, request.OverridesJson), review, source.DataSource, () => Guard(true));
            var row = ToRow(expected);
            var policy = new MutationPolicy(company, actor, existing, expected, () => Guard(true));
            var repo = Repository(source.Connection, policy);
            if (existing is null) await repo.InsertAsync(row, actor);
            else
            {
                var original = await ReadRow(company, source.Connection, () => Guard(true));
                if (original is null || ToConfiguration(original, company) != existing)
                    throw new InvalidOperationException("The environment changed before update.");
                row.ROW_CREATED_BY = original.ROW_CREATED_BY;
                row.ROW_CREATED_DATE = original.ROW_CREATED_DATE;
                await repo.UpdateAsync(row, actor);
            }
            var stored = await ReadOne(company, source.Connection, () => Guard(true));
            if (stored != expected) throw new InvalidOperationException("The company environment write could not be verified.");
            saved = stored;
            await requireReviewedDraft(company, new(request.CountryCode, request.OverridesJson), review, source.DataSource, () => Guard(true));
            await Guard(true);
        });
        var first = await ReadOne(company, source.Connection, () => Guard(false));
        var second = await ReadOne(company, source.Connection, () => Guard(false));
        await Guard(false);
        if (saved is null || first != saved || second != saved)
            throw new InvalidOperationException("The environment save outcome is unconfirmed. Reload the original company settings.");
        return saved;
    }

    private async Task<(string Connection, IDataSource DataSource, Func<Task> RequireBinding)> BindAsync(Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(authority);
        await authority();
        var connection = await resolveConnection();
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("The system setup binding is unavailable.");
        var source = editor.GetDataSource(connection) ?? throw new InvalidOperationException("The system setup datasource is unavailable.");
        async Task RequireBinding()
        {
            if (await resolveConnection() != connection || !ReferenceEquals(editor.GetDataSource(connection), source))
                throw new InvalidOperationException("The system setup binding changed.");
        }
        return (connection, source, RequireBinding);
    }

    private PPDMGenericRepository Repository(string connection, IPPDMRowAccessPolicy? policy = null) =>
        new(editor, columns, defaults, metadata, typeof(COMPANY_OILGAS_ENVIRONMENT), connection, Table, rowAccess: policy);

    private async Task<CompanyEnvironmentConfiguration?> ReadOne(CompanyScope company, string connection, Func<Task> guard)
    {
        var row = await ReadRow(company, connection, guard);
        return row is null ? null : ToConfiguration(row, company);
    }

    private async Task<COMPANY_OILGAS_ENVIRONMENT?> ReadRow(CompanyScope company, string connection, Func<Task> guard)
    {
        var id = CompanyEnvironmentIdentity.ForCompany(company);
        await guard();
        var rows = (await Repository(connection).GetAsync([new() { FieldName = "COMPANY_OILGAS_ENVIRONMENT_ID", Operator = "=", FilterValue = id }]))?.ToArray();
        await guard();
        if (rows is null || rows.Length > 1 || rows.Any(x => x is not COMPANY_OILGAS_ENVIRONMENT))
            throw new InvalidOperationException("The company environment record is unavailable or ambiguous.");
        var row = rows.SingleOrDefault() as COMPANY_OILGAS_ENVIRONMENT;
        if (row is not null) _ = ToConfiguration(row, company);
        return row;
    }

    private static CompanyEnvironmentConfiguration ToConfiguration(COMPANY_OILGAS_ENVIRONMENT row, CompanyScope company)
    {
        if (row.COMPANY_OILGAS_ENVIRONMENT_ID != CompanyEnvironmentIdentity.ForCompany(company) ||
            row.OWNER_TENANT_ID != company.TenantId || row.OWNER_COMPANY_BA_ID != company.BusinessAssociateId || row.ACTIVE_IND != "Y" ||
            string.IsNullOrWhiteSpace(row.ROW_CREATED_BY) || row.ROW_CREATED_DATE is null)
            throw new InvalidOperationException("The company environment ownership or audit is unconfirmed.");
        var result = new CompanyEnvironmentConfiguration(company, row.COUNTRY_CODE, row.OVERRIDES_JSON, row.TOOLS_VERSION,
            row.REVISION, row.PREVIOUS_REVISION ?? "", row.INTENT_ID, row.REVIEWED_DRAFT_REVISION, row.REVIEWED_SYSTEM_REVISION, row.REVIEWED_COUNTRY_REVISION);
        CompanyEnvironmentIdentity.RequireValid(result, company);
        return result;
    }

    private static COMPANY_OILGAS_ENVIRONMENT ToRow(CompanyEnvironmentConfiguration value) => new()
    {
        COMPANY_OILGAS_ENVIRONMENT_ID = CompanyEnvironmentIdentity.ForCompany(value.Company), OWNER_TENANT_ID = value.Company.TenantId,
        OWNER_COMPANY_BA_ID = value.Company.BusinessAssociateId, COUNTRY_CODE = value.CountryCode, OVERRIDES_JSON = value.OverridesJson,
        TOOLS_VERSION = value.ToolsVersion, REVISION = value.Revision, PREVIOUS_REVISION = value.PreviousRevision.Length == 0 ? null : value.PreviousRevision,
        INTENT_ID = value.IntentId, REVIEWED_DRAFT_REVISION = value.ReviewedDraftRevision,
        REVIEWED_SYSTEM_REVISION = value.ReviewedSystemRevision, REVIEWED_COUNTRY_REVISION = value.ReviewedCountryRevision, ACTIVE_IND = "Y"
    };

    private static void RequireSchema(IDataSource source)
    {
        var schema = source.GetEntityStructure(new EntityStructure { EntityName = Table, DatasourceEntityName = Table }, true);
        var keys = schema?.Fields?.Where(x => x.IsKey).ToArray();
        if (schema?.EntityName != Table || keys is not { Length: 1 } || keys[0].FieldName != Table + "_ID")
            throw new InvalidOperationException("Apply the company environment schema through the system setup Beep migration.");
        foreach (var (name, size, nullable) in new[] { ("COMPANY_OILGAS_ENVIRONMENT_ID", 64, false), ("OWNER_TENANT_ID", 64, false),
            ("OWNER_COMPANY_BA_ID", 128, false), ("COUNTRY_CODE", 2, false), ("OVERRIDES_JSON", CompanyEnvironmentIdentity.MaxOverridesLength, false),
            ("REVISION", 64, false), ("PREVIOUS_REVISION", 64, true), ("INTENT_ID", 64, false), ("TOOLS_VERSION", 128, false), ("REVIEWED_DRAFT_REVISION", 64, false),
            ("REVIEWED_SYSTEM_REVISION", 64, false), ("REVIEWED_COUNTRY_REVISION", 64, true) })
        {
            var fields = schema.Fields.Where(x => x.FieldName == name).ToArray();
            if (fields.Length != 1 || fields[0].Fieldtype != typeof(string).FullName || fields[0].AllowDBNull != nullable || Math.Max(fields[0].Size, fields[0].Size1) < size)
                throw new InvalidOperationException("The company environment schema cannot preserve the required settings.");
        }
    }

    private sealed class MutationPolicy(CompanyScope company, string actor, CompanyEnvironmentConfiguration? existing,
        CompanyEnvironmentConfiguration expected, Func<Task> guard) : IPPDMRowAccessPolicy
    {
        private static AppFilter Filter(string field, string value) => new() { FieldName = field, Operator = "=", FilterValue = value };
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table)
        { await guard(); RequireType(type, table); return [new[] { Filter("OWNER_TENANT_ID", company.TenantId), Filter("OWNER_COMPANY_BA_ID", company.BusinessAssociateId) }]; }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) { await guard(); RequireType(type, table); return []; }
        public async Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows)
        { await guard(); RequireType(type, table); foreach (var row in rows) _ = ToConfiguration((COMPANY_OILGAS_ENVIRONMENT)row, company); }
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool includeInherited = true) => throw new InvalidOperationException();
        public async Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation operation, string? user)
        {
            await guard(); RequireType(type, table);
            if (user != actor || entity is not COMPANY_OILGAS_ENVIRONMENT row ||
                (existing is null ? operation != PPDMRowMutation.Insert || previous is not null :
                    operation != PPDMRowMutation.Update || previous is not COMPANY_OILGAS_ENVIRONMENT old || ToConfiguration(old, company) != existing) ||
                ToConfiguration(row, company) != expected)
                throw new InvalidOperationException("The original company environment mutation is unconfirmed.");
        }
        private static void RequireType(Type type, string table)
        { if (type != typeof(COMPANY_OILGAS_ENVIRONMENT) || table != Table) throw new InvalidOperationException("Unexpected environment record type."); }
    }
}
