using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.LeaseAcquisition.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Options;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

// Reuses the existing Beep evidence reader and FIELD policy for the canonical lease owner.
public sealed class PPDMCompanyLeaseDataAccess(CompanyModuleConnection company, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata,
    IPPDMRowAccessPolicy policy, IOptions<PPDMFieldEligibilityConfiguration> configuration, TimeProvider? clock = null) : ICompanyLeaseDataAccess
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    private static readonly Dictionary<Type, string[]> Keys = new()
    {
        [typeof(LAND_RIGHT)] = ["LAND_RIGHT_SUBTYPE", "LAND_RIGHT_ID"],
        [typeof(LAND_AGREEMENT)] = ["LAND_RIGHT_SUBTYPE", "LAND_RIGHT_ID"],
        [typeof(LAND_STATUS)] = ["LAND_RIGHT_SUBTYPE", "LAND_RIGHT_ID", "STATUS_TYPE", "LAND_RIGHT_STATUS", "STATUS_SEQ_NO"],
        [typeof(LAND_RIGHT_FIELD)] = ["LAND_RIGHT_SUBTYPE", "LAND_RIGHT_ID", "FIELD_ID"],
        [typeof(LAND_RIGHT_COMPONENT)] = ["LAND_RIGHT_SUBTYPE", "LAND_RIGHT_ID", "COMPONENT_OBS_NO"]
    };
    public async Task<LeaseDataScope> ResolveAsync()
    {
        var database = await company.ResolveAsync(); var captured = company.Company;
        var rules = Rules(captured.Scope);
        var scope = new LeaseDataScope(captured.Scope, database, captured.UserId, rules.LeaseLandRightSubtype, rules.LeaseCompanyComponentType, rules.LeaseStatusType, rules.TimeZoneId);
        _ = Zone(scope);
        foreach (var entry in Keys) await DefinitionAsync(metadata, entry.Key, entry.Value);
        await DefinitionAsync(metadata, typeof(LAND_RIGHT_COMPONENT), Keys[typeof(LAND_RIGHT_COMPONENT)],
            ("BUSINESS_ASSOCIATE_ID", "BUSINESS_ASSOCIATE", "BUSINESS_ASSOCIATE_ID"),
            ("FIELD_ID", "FIELD", "FIELD_ID"), ("LAND_COMPONENT_TYPE", "R_LAND_COMPONENT_TYPE", "LAND_COMPONENT_TYPE"));
        await DefinitionAsync(metadata, typeof(LAND_RIGHT_FIELD), Keys[typeof(LAND_RIGHT_FIELD)], ("FIELD_ID", "FIELD", "FIELD_ID"));
        await DefinitionAsync(metadata, typeof(R_LAND_COMPONENT_TYPE), ["LAND_COMPONENT_TYPE"]);
        await Reader(scope).OpenAsync();
        await RequireCurrentAsync(scope); return scope;
    }
    public async Task RequireCurrentAsync(LeaseDataScope scope)
    {
        await Reader(scope).RequireCurrentAsync(); var rules = Rules(scope.Company);
        if (rules.LeaseLandRightSubtype != scope.LandRightSubtype || rules.LeaseCompanyComponentType != scope.CompanyComponentType || rules.LeaseStatusType != scope.StatusType || rules.TimeZoneId != scope.DateZoneId)
            throw RefusalException.Conflict("Company lease data interpretation changed during this operation.");
        await ReferenceAsync(scope, typeof(R_LAND_COMPONENT_TYPE), ["LAND_COMPONENT_TYPE"], [Equal("LAND_COMPONENT_TYPE", scope.CompanyComponentType)]);
    }
    public async Task RequireFieldAsync(LeaseDataScope scope, string fieldId, string level, string? actor = null)
    {
        if (!Identifier(fieldId) || level is not ("READ" or "WRITE" or "DELETE")) throw RefusalException.Invalid("An exact FIELD and supported lease access level are required.");
        await RequireCurrentAsync(scope);
        if ((level != "READ" || actor is not null) && actor != scope.ActorUserId) throw RefusalException.Forbidden("Lease changes require the authenticated application account.");
        if (level != "READ" && !company.Company.Roles.Contains("Administrator", StringComparer.Ordinal))
            throw RefusalException.Forbidden("A current company Administrator is required to change lease records.");
        var repository = new PPDMGenericRepository(editor, columns, defaults, metadata, typeof(FIELD), scope.Database.ConnectionName, "FIELD", rowAccess: policy);
        var fields = (await repository.GetAsync([Equal("FIELD_ID", fieldId), Equal("ACTIVE_IND", "Y")])).ToArray();
        if (fields.Length != 1 || PPDMCompanyFieldEligibility.Rank(await policy.GetEffectiveAccessLevelAsync(typeof(FIELD), "FIELD", fields[0])) < PPDMCompanyFieldEligibility.Rank(level))
            throw RefusalException.Forbidden("The account needs current FIELD access at the requested lease operation level.");
        await RequireCurrentAsync(scope);
    }
    public async Task<object[]> ReadAsync(LeaseDataScope scope, Type type, List<AppFilter> filters)
    {
        if (!Keys.TryGetValue(type, out var keys) || !Has(filters, "LAND_RIGHT_SUBTYPE", scope.LandRightSubtype))
            throw RefusalException.Invalid("Lease reads require the canonical table and complete reviewed subtype.");
        if (type == typeof(LAND_RIGHT_COMPONENT))
        {
            if (!Has(filters, "BUSINESS_ASSOCIATE_ID", scope.Company.BusinessAssociateId) || !Has(filters, "LAND_COMPONENT_TYPE", scope.CompanyComponentType) ||
                !Has(filters, "ACTIVE_IND", "Y") || !ExactIdentifier(filters, "FIELD_ID") && !ExactIdentifier(filters, "LAND_RIGHT_ID"))
                throw RefusalException.Invalid("Lease association reads require exact company, component and resource predicates.");
        }
        else if (!ExactIdentifier(filters, "LAND_RIGHT_ID")) throw RefusalException.Invalid("Lease evidence reads require an exact complete land-right identity.");
        await RequireCurrentAsync(scope);
        var rows = await Reader(scope).ReadAsync(type, filters, type == typeof(LAND_RIGHT) ? [..keys, "CALCULATED_INTEREST"] : keys);
        if (rows.Any(row => keys.Any(key => type.GetProperty(key)!.GetValue(row) is decimal number ? !Observation(number) : !Identifier(type.GetProperty(key)!.GetValue(row) as string))))
            throw RefusalException.Conflict("The provider returned incomplete or invalid lease keys.");
        var identities = rows.Select(row => System.Text.Json.JsonSerializer.Serialize(keys.Select(key => type.GetProperty(key)!.GetValue(row) is decimal number
            ? number.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) : type.GetProperty(key)!.GetValue(row) as string))).ToArray();
        if (identities.Distinct(StringComparer.Ordinal).Count() != rows.Length)
            throw RefusalException.Conflict("The provider returned duplicate complete lease keys.");
        return rows;
    }
    public async Task ValidateCodesAsync(LeaseDataScope scope, string? agreementType, string status)
    {
        await RequireCurrentAsync(scope);
        if (!Identifier(status) || agreementType is not null && !Identifier(agreementType)) throw RefusalException.Invalid("Explicit lease agreement/status codes are required.");
        if (agreementType is not null) await ReferenceAsync(scope, typeof(R_LAND_AGREE_TYPE), ["LAND_AGREE_TYPE"], [Equal("LAND_AGREE_TYPE", agreementType)]);
        await ReferenceAsync(scope, typeof(R_LAND_RIGHT_STATUS), ["STATUS_TYPE", "LAND_RIGHT_STATUS"], [Equal("STATUS_TYPE", scope.StatusType), Equal("LAND_RIGHT_STATUS", status)]);
        await RequireCurrentAsync(scope);
    }
    private async Task ReferenceAsync(LeaseDataScope scope, Type type, string[] keys, List<AppFilter> filters)
    {
        await DefinitionAsync(metadata, type, keys); filters.Add(Equal("ACTIVE_IND", "Y"));
        var rows = await Reader(scope).ReadAsync(type, filters, keys);
        if (rows.Length != 1 || !Window(rows[0], Zone(scope), time, required: false)) throw RefusalException.Conflict("A reviewed lease reference is missing, ambiguous or noncurrent.");
    }
    public bool IsCurrent(LeaseDataScope scope, object row) => Window(row, Zone(scope), time, required: true);
    public async Task ExecuteCommandAsync(LeaseDataScope scope, string fieldId, Func<Task> command, Func<Task> confirm)
    {
        await RequireFieldAsync(scope, fieldId, "WRITE", scope.ActorUserId);
        var source = editor.GetDataSource(scope.Database.ConnectionName) ?? throw new InvalidOperationException("The lease command datasource is unavailable.");
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () => {
            await RequireFieldAsync(scope, fieldId, "WRITE", scope.ActorUserId);
            await command();
            await confirm();
            await RequireFieldAsync(scope, fieldId, "WRITE", scope.ActorUserId);
            await RequireCurrentAsync(scope);
        });
        // A commit acknowledgment alone is not a successful command response.
        await confirm();
        await RequireFieldAsync(scope, fieldId, "WRITE", scope.ActorUserId);
        await RequireCurrentAsync(scope);
    }
    public IPPDMRowAccessPolicy ForMutation(LeaseDataScope scope, string fieldId, string leaseId, object expectedEntity, bool creating)
        => new LeaseMutationPolicy(this, scope, fieldId, leaseId, expectedEntity, creating);

    private sealed class LeaseMutationPolicy(PPDMCompanyLeaseDataAccess access, LeaseDataScope scope, string field, string id, object expected, bool creating) : IPPDMRowAccessPolicy
    {
        private readonly Type expectedType = expected.GetType();
        private readonly object?[] expectedKeys = Keys[expected.GetType()].Select(key => expected.GetType().GetProperty(key)!.GetValue(expected)).ToArray();
        private void Validate(Type type, string table, object row)
        {
            if (type != expectedType || !Keys.ContainsKey(type) || type.Name != table || !type.IsInstanceOfType(row) ||
                type.GetProperty("LAND_RIGHT_SUBTYPE")!.GetValue(row) as string != scope.LandRightSubtype || type.GetProperty("LAND_RIGHT_ID")!.GetValue(row) as string != id ||
                type.GetProperty("FIELD_ID") is { } fieldColumn && fieldColumn.GetValue(row) as string != field ||
                row is LAND_RIGHT_COMPONENT component && (component.BUSINESS_ASSOCIATE_ID != scope.Company.BusinessAssociateId || component.LAND_COMPONENT_TYPE != scope.CompanyComponentType) ||
                row is LAND_STATUS status && status.STATUS_TYPE != scope.StatusType)
                throw RefusalException.Forbidden("Lease mutation keys or resource association changed outside the reviewed command.");
            if (!Keys[type].Select(key => type.GetProperty(key)!.GetValue(row)).SequenceEqual(expectedKeys))
                throw RefusalException.Forbidden("A complete lease mutation key changed during preparation.");
            _ = access.IsCurrent(scope, row);
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table)
        {
            if (!Keys.ContainsKey(type) || type.Name != table) throw RefusalException.Forbidden("The lease mutation policy requires a canonical reviewed table.");
            await access.RequireCurrentAsync(scope);
            return new IReadOnlyList<AppFilter>[] { new AppFilter[] { Equal("LAND_RIGHT_SUBTYPE", scope.LandRightSubtype), Equal("LAND_RIGHT_ID", id) } };
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) { await GetReadConjunctionsAsync(type, table); return []; }
        public async Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows)
        { foreach (var row in rows) Validate(type, table, row); await access.RequireFieldAsync(scope, field, "READ"); }
        public async Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool includeInherited = true)
        { await ValidateRowsAsync(type, table, [row]); return "READ"; }
        public async Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation operation, string? actor)
        {
            if (operation == PPDMRowMutation.Delete || operation != PPDMRowMutation.Insert && previous is null)
                throw RefusalException.Forbidden("The lease command does not authorize this mutation shape.");
            Validate(type, table, entity); if (previous is not null) Validate(type, table, previous);
            await access.RequireFieldAsync(scope, field, "WRITE", actor);
            if (entity is LAND_STATUS proposedStatus) await access.ValidateCodesAsync(scope, null, proposedStatus.LAND_RIGHT_STATUS);
            if (entity is LAND_AGREEMENT proposedAgreement) await access.ReferenceAsync(scope, typeof(R_LAND_AGREE_TYPE), ["LAND_AGREE_TYPE"], [Equal("LAND_AGREE_TYPE", proposedAgreement.LAND_AGREE_TYPE)]);
            if (!creating)
            {
                var components = (await access.ReadAsync(scope, typeof(LAND_RIGHT_COMPONENT), [Equal("LAND_RIGHT_SUBTYPE", scope.LandRightSubtype), Equal("LAND_RIGHT_ID", id),
                    Equal("BUSINESS_ASSOCIATE_ID", scope.Company.BusinessAssociateId), Equal("LAND_COMPONENT_TYPE", scope.CompanyComponentType), Equal("ACTIVE_IND", "Y")])).Cast<LAND_RIGHT_COMPONENT>().Where(row => access.IsCurrent(scope, row)).ToArray();
                if (components.Length == 0 || components.Any(row => row.FIELD_ID != field)) throw RefusalException.Forbidden("The company's lease association was revoked or reparented during mutation preparation.");
                var links = await access.ReadAsync(scope, typeof(LAND_RIGHT_FIELD), [Equal("LAND_RIGHT_SUBTYPE", scope.LandRightSubtype), Equal("LAND_RIGHT_ID", id), Equal("FIELD_ID", field), Equal("ACTIVE_IND", "Y")]);
                if (links.Length != 1 || !access.IsCurrent(scope, links[0])) throw RefusalException.Forbidden("The lease FIELD link was revoked during mutation preparation.");
                await access.RequireFieldAsync(scope, field, "WRITE", actor);
            }
            Validate(type, table, entity); // Also reject keys changed during awaited authority checks.
        }
    }
    private CompanyFieldEligibilityRules Rules(CompanyScope scope)
    {
        var matches = configuration.Value.Companies.Where(rule => rule.TenantId == scope.TenantId && rule.BusinessAssociateId == scope.BusinessAssociateId).ToArray();
        if (matches.Length != 1 || !Identifier(matches[0].LeaseLandRightSubtype) || !Identifier(matches[0].LeaseCompanyComponentType) || !Identifier(matches[0].LeaseStatusType) || !Identifier(matches[0].TimeZoneId))
            throw RefusalException.Conflict("Configure the selected company's reviewed lease subtype, company component code and date zone.");
        return matches[0];
    }
    private static TimeZoneInfo Zone(LeaseDataScope scope)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(scope.DateZoneId); }
        catch (Exception failure) when (failure is TimeZoneNotFoundException or InvalidTimeZoneException)
        { throw RefusalException.Conflict("The reviewed lease date zone is unavailable or invalid."); }
    }
    private CompanyPPDMEvidenceReader Reader(LeaseDataScope scope) => new(company, editor, columns, defaults, metadata, scope.Database, scope.Company, scope.ActorUserId);
    private static bool Has(List<AppFilter> filters, string column, string value) => filters.Count(filter => filter.FieldName == column && filter.Operator == "=" && filter.FilterValue == value) == 1;
    private static bool ExactIdentifier(List<AppFilter> filters, string column) => filters.Count(filter => filter.FieldName == column && filter.Operator == "=" && Identifier(filter.FilterValue)) == 1;
}
