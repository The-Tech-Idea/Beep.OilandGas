using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Requests.UserManagement;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Read diagnostics over the same company grants, Beep filters and actual PPDM read path as CRUD.</summary>
public sealed class CompanyRowLevelSecurityService(RepositoryRequestAccount account, CompanyModuleConnection company,
    IUserAssetAccessStore assets, IPPDMRowAccessPolicy rows, IPPDMMetadataRepository metadata,
    IPPDM39DataService data) : IRowLevelSecurityService
{
    public async Task<RowAccessResult> CheckRowAccessAsync(string userId, CheckRowAccessRequest request)
    {
        await RequireScopeAsync(userId);
        var type = await EntityTypeAsync(request.TABLE_NAME);
        // This validates the reviewed ownership path before accepting any caller-supplied key.
        await rows.GetReadConjunctionsAsync(type, request.TABLE_NAME);
        if (string.IsNullOrWhiteSpace(request.ENTITY_DATA_JSON) || request.ENTITY_DATA_JSON.Length > 65536)
            throw RefusalException.Invalid("Supply a JSON object with the complete PPDM row key, up to 65536 characters.");
        using var entity = ParseKey(request.ENTITY_DATA_JSON);
        var keys = (await metadata.GetPrimaryKeyColumnsAsync(request.TABLE_NAME)).ToArray();
        if (keys.Length == 0 || keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
            throw RefusalException.Conflict("The PPDM primary key metadata is incomplete.");
        var filters = new List<AppFilter>();
        foreach (var key in keys)
        {
            if (type.GetProperty(key)?.PropertyType != typeof(string) ||
                !entity.RootElement.TryGetProperty(key, out var value) || value.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(value.GetString()) || value.GetString()!.Length > 128)
                throw RefusalException.Invalid("Supply every canonical PPDM primary key column as a nonempty string up to 128 characters.");
            filters.Add(new() { FieldName = key, Operator = "=", FilterValue = value.GetString() });
        }
        // Ownership values in the request cannot prove access: query the stored row through the mandatory policy.
        var result = await data.GetEntitiesAsync(request.TABLE_NAME, filters);
        if (!result.Success || result.Entities.Count > 1)
            throw RefusalException.Conflict("The scoped PPDM row read did not return a unique result.");
        await RequireScopeAsync(userId);
        return new() { HasAccess = result.Entities.Count == 1, ScopeType = "COMPANY_ASSET",
            Reason = result.Entities.Count == 1 ? "The stored row is currently readable in the selected company." : "No stored row is visible under current company grants." };
    }

    public async Task<RowFilterResult> ApplyRowFiltersAsync(string userId, ApplyRowFiltersRequest request)
    {
        await RequireScopeAsync(userId);
        var type = await EntityTypeAsync(request.TABLE_NAME);
        var filters = PPDMGenericRepository.CopyAndValidateFilters(type, request.Filters);
        var predicates = await rows.GetReadConjunctionsAsync(type, request.TABLE_NAME);
        var constraints = await rows.GetReadConstraintsAsync(type, request.TABLE_NAME);
        await RequireScopeAsync(userId);
        return new() { ReadConjunctions = predicates, CompanyConstraints = constraints, Filters = filters, ScopeType = "COMPANY_ASSET" };
    }

    public async Task<AccessResult> CheckSourceAccessAsync(string userId, CheckSourceAccessRequest request) =>
        await CheckConnectionAsync(userId, request.TARGET_SOURCE);

    public async Task<AccessResult> CheckDataSourceAccessAsync(string userId, CheckDataSourceAccessRequest request) =>
        await CheckConnectionAsync(userId, request.DATASOURCE_NAME);

    public async Task<AccessResult> CheckDatabaseAccessAsync(string userId, CheckDatabaseAccessRequest request)
    {
        var scope = await RequireScopeAsync(userId);
        return Result(string.Equals(scope.Fingerprint, request.DatabaseFingerprint, StringComparison.Ordinal));
    }

    public async Task<string[]> GetUserAccessibleFieldsAsync(string userId) =>
        (await GrantsAsync(userId)).Where(grant => grant.AssetType == "FIELD").Select(grant => grant.AssetId)
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

    // Explicit grants only; FIELD eligibility comes from the same canonical policy as actual row reads.
    public async Task<string[]> GetUserAccessibleAssetsAsync(string userId) =>
        (await GrantsAsync(userId)).Select(grant => grant.AssetId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

    public async Task<string[]> GetUserAccessibleOrganizationsAsync(string userId)
    {
        await RequireScopeAsync(userId);
        return [company.Company.Scope.BusinessAssociateId];
    }

    private async Task<AccessResult> CheckConnectionAsync(string userId, string requested)
    {
        var scope = await RequireScopeAsync(userId);
        return Result(string.Equals(scope.ConnectionName, requested, StringComparison.OrdinalIgnoreCase));
    }

    private static AccessResult Result(bool matches) => new() { HasAccess = matches,
        Reason = matches ? "This is the selected company's reviewed core binding; row grants are checked separately." : "The target does not match the selected company's reviewed core binding." };

    private async Task<List<AppUserAssetAccess>> GrantsAsync(string userId)
    {
        var scope = await RequireScopeAsync(userId);
        var grants = await assets.ReadAsync(userId, scope.Fingerprint);
        var selected = grants.Where(grant => grant.IsActive && grant.AccessLevel is "READ" or "WRITE" or "DELETE").ToList();
        if (selected.Any(grant => grant.AssetType == "FIELD"))
        {
            var approved = (await rows.GetReadConjunctionsAsync(typeof(Beep.OilandGas.PPDM39.Models.FIELD), "FIELD"))
                .Where(group => group.Count == 1 && group[0].FieldName == "FIELD_ID" && group[0].Operator == "=")
                .Select(group => group[0].FilterValue).ToHashSet(StringComparer.Ordinal);
            selected = selected.Where(grant => grant.AssetType != "FIELD" || approved.Contains(grant.AssetId)).ToList();
        }
        await RequireScopeAsync(userId);
        return selected;
    }

    private async Task<AssetDatabaseScope> RequireScopeAsync(string userId)
    {
        await account.RequireSelfAsync(userId);
        company.RequireActor(userId);
        return await company.ResolveAsync();
    }

    private async Task<Type> EntityTypeAsync(string table)
    {
        if (string.IsNullOrWhiteSpace(table)) throw RefusalException.Invalid("A PPDM table is required.");
        var definition = await metadata.GetTableMetadataAsync(table);
        return definition is null ? throw RefusalException.Invalid("The PPDM table is not recognized.") :
            typeof(Beep.OilandGas.PPDM.Models.IPPDMEntity).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + definition.EntityTypeName)
                ?? throw RefusalException.Conflict("The PPDM table has no canonical entity model.");
    }

    private static JsonDocument ParseKey(string json)
    {
        try
        {
            var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.EnumerateObject().Select(property => property.Name).Distinct(StringComparer.Ordinal).Count() ==
                    document.RootElement.EnumerateObject().Count()) return document;
            document.Dispose();
            throw RefusalException.Invalid("The row key must be a JSON object without duplicate properties.");
        }
        catch (JsonException invalid) { throw new RefusalException(RefusalKind.Invalid, "The row key must be valid JSON.", invalid); }
    }
}
