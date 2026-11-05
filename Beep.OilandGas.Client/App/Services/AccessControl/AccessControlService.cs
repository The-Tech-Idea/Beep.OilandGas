using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Client.Exceptions;
using System.Text.Json;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Client.App.Services.AccessControl;

/// <summary>The canonical access/profile API contract over the captured-company HTTP transport.</summary>
internal sealed class AccessControlService(ClientApiTransport transport) : IAccessControlService
{
    private readonly ClientApiTransport api = transport ?? throw new ArgumentNullException(nameof(transport));

    public async Task<bool> CheckAssetAccessAsync(string userId, string assetId, string assetType,
        CancellationToken cancellationToken = default, string? facilityType = null, string? requiredPermission = null)
    {
        Identifier(userId); Identifier(assetId);
        var type = AssetType(assetType);
        if (type == "FACILITY") Identifier(facilityType);
        else if (facilityType is not null) throw new ArgumentException("Facility type applies only to facilities.", nameof(facilityType));
        if (requiredPermission is not null) Identifier(requiredPermission);
        var request = new AccessCheckRequest { UserId = userId, AssetId = assetId, AssetType = type,
            FacilityType = facilityType, RequiredPermission = requiredPermission };
        var result = Required(await api.PostAsync<AccessCheckRequest, AccessCheckResponse>("/api/AccessControl/check-access", request, cancellationToken));
        if (result.HasAccess && (result.AccessLevel is not ("READ" or "WRITE" or "DELETE") ||
            !AssetAccessLevelChecks.Meets(result.AccessLevel, requiredPermission)))
            throw new ApiClientException("The access API returned an invalid access decision.");
        return result.HasAccess;
    }

    public async Task<List<AssetAccess>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null,
        CancellationToken cancellationToken = default, bool includeInherited = true, string? organizationId = null)
    {
        var endpoint = User(userId) + "/assets?includeInherited=" + (includeInherited ? "true" : "false");
        var type = assetType is null ? null : AssetType(assetType);
        if (type is not null) endpoint += "&assetType=" + Uri.EscapeDataString(type);
        if (organizationId is not null) { Identifier(organizationId); endpoint += "&organizationId=" + Uri.EscapeDataString(organizationId); }
        cancellationToken.ThrowIfCancellationRequested();
        var selected = api.SelectedCompanyScope;
        var assets = Required(await api.GetAsync<List<AssetAccess>>(endpoint, cancellationToken));
        if (assets.Any(asset => asset is null || asset.UserId != userId || !asset.Active ||
            string.IsNullOrWhiteSpace(asset.AssetId) || (type is not null && asset.AssetType != type) ||
            asset.AssetType is not ("FIELD" or "WELL" or "POOL" or "FACILITY" or "WORK_ORDER") ||
            asset.AssetType == "WORK_ORDER" && asset.Inherit ||
            (asset.AssetType == "FACILITY" ? string.IsNullOrWhiteSpace(asset.FacilityType) : asset.FacilityType is not null) ||
            asset.AccessLevel is not ("READ" or "WRITE" or "DELETE") || asset.OrganizationId != selected.BusinessAssociateId ||
            (organizationId is not null && asset.OrganizationId != organizationId)))
            throw new ApiClientException("The access API returned inconsistent accessible assets.");
        return assets;
    }

    public async Task<List<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var roles = Required(await api.GetAsync<List<string>>(User(userId) + "/roles", cancellationToken));
        if (roles.Any(string.IsNullOrWhiteSpace)) throw new ApiClientException("The access API returned an invalid role list.");
        return roles;
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        Identifier(permission);
        return await api.GetAsync<bool?>(User(userId) + "/permission/" + Uri.EscapeDataString(permission), cancellationToken)
            ?? throw new ApiClientException("The access API returned no permission result.");
    }

    public async Task<UserProfile> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        Identifier(userId);
        var profile = Required(await api.GetAsync<UserProfile>("/api/UserProfile/" + Uri.EscapeDataString(userId), cancellationToken));
        if (profile.UserId != userId || profile.Roles is null || profile.Roles.Any(string.IsNullOrWhiteSpace))
            throw new ApiClientException("The profile API returned an inconsistent profile.");
        return profile;
    }

    private static T Required<T>(T? result) where T : class => result ?? throw new ApiClientException("The access API returned no result.");
    public async Task<FieldInterestReview> GetFieldInterestReviewAsync(string fieldId, CancellationToken cancellationToken = default)
    {
        Identifier(fieldId); cancellationToken.ThrowIfCancellationRequested(); var selected = api.SelectedCompanyScope;
        var wire = await api.GetAsync<JsonElement?>("/api/AccessControl/fields/" + Uri.EscapeDataString(fieldId) + "/interest-review", cancellationToken)
            ?? throw new ApiClientException("The API returned no FIELD interest review.");
        try
        {
            var sets = Property(wire, "Sets");
            foreach (var set in sets.EnumerateArray())
            {
                Numbers(Property(set, "InterestSet"), "INTEREST_SET_SEQ_NO");
                foreach (var row in Property(set, "Components").EnumerateArray()) Numbers(row, "INTEREST_SET_SEQ_NO", "COMPONENT_OBS_NO");
                foreach (var row in Property(set, "CompanyPartners").EnumerateArray()) Numbers(row, "INTEREST_SET_SEQ_NO", "PARTNER_OBS_NO", "GROSS_PERCENT_INTEREST", "NET_PERCENT_INTEREST");
            }
            var result = JsonSerializer.Deserialize<FieldInterestReview>(wire.GetRawText(), new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? throw new ApiClientException("The API returned no FIELD interest review.");
            FieldInterestReviewValidation.RequireValid(result, selected, fieldId);
            return result;
        }
        catch (Exception failure) when (failure is JsonException or InvalidOperationException or ArgumentException)
        { throw new ApiClientException("The API returned malformed FIELD interest evidence.", failure); }
    }
    private static JsonElement Property(JsonElement value, string name) => value.EnumerateObject().Single(pair => pair.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
    private static void Numbers(JsonElement value, params string[] names)
    {
        if (names.Any(name => !Property(value, name).TryGetDecimal(out _))) throw new ApiClientException("The API returned incomplete numeric interest evidence.");
    }
    public async Task<AssetGrantReviewPage> ReviewStoredGrantsAsync(string userId, int offset = 0, int limit = 50, CancellationToken cancellationToken = default)
    {
        Page(offset, limit); var selected = api.SelectedCompanyScope;
        var result = Required(await api.GetAsync<AssetGrantReviewPage>(User(userId) + "/stored-grants?offset=" + offset + "&limit=" + limit, cancellationToken));
        if (result.Company != selected || result.UserId != userId || result.Offset != offset || result.Limit != limit || result.Total < 0 || result.Items is null ||
            !Fingerprint(result.DatabaseScope) || result.Items.Count > limit || result.Items.Count > result.Total || result.Items.Any(row => row is null || row.UserId != userId || row.OrganizationId != selected.BusinessAssociateId || row.DatabaseScope != result.DatabaseScope) ||
            result.Items.Select(row => row.Id).Distinct(StringComparer.Ordinal).Count() != result.Items.Count)
            throw new ApiClientException("The stored-grant review has inconsistent ownership or paging.");
        return result;
    }
    public async Task<AssetGrantRetirement> RetireStoredGrantAsync(string grantId, RetireAssetGrantRequest request, CancellationToken cancellationToken = default)
    {
        Identifier(grantId); ArgumentNullException.ThrowIfNull(request); var selected = api.SelectedCompanyScope;
        if (string.IsNullOrWhiteSpace(request.ExpectedRevision) || request.ExpectedRevision.Length > 36 || request.ExpectedRevision != request.ExpectedRevision.Trim() || request.ExpectedRevision.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 1000 || request.Reason != request.Reason.Trim() || request.Reason.Any(char.IsControl))
            throw new ArgumentException("A reviewed revision and a valid retirement reason of at most 1000 characters are required.");
        var result = Required(await api.PostAsync<RetireAssetGrantRequest, AssetGrantRetirement>("/api/AccessControl/stored-grants/" + Uri.EscapeDataString(grantId) + "/retire", request, cancellationToken));
        if (result.GrantId != grantId || result.TenantId != selected.TenantId || result.OrganizationId != selected.BusinessAssociateId ||
            result.PreviousRevision != request.ExpectedRevision || result.Reason != request.Reason || !Guid.TryParseExact(result.Revision, "D", out _))
            throw new ApiClientException("The grant retirement API returned inconsistent command evidence.");
        ValidateRetirement(result, selected, result.DatabaseScope, grantId);
        return result;
    }
    public async Task<AssetGrantRetirementPage> GetGrantRetirementsAsync(string grantId, int offset = 0, int limit = 50, CancellationToken cancellationToken = default)
    {
        Identifier(grantId); Page(offset, limit); var selected = api.SelectedCompanyScope;
        var result = Required(await api.GetAsync<AssetGrantRetirementPage>("/api/AccessControl/stored-grants/" + Uri.EscapeDataString(grantId) + "/retirements?offset=" + offset + "&limit=" + limit, cancellationToken));
        if (result.Company != selected || result.GrantId != grantId || result.Offset != offset || result.Limit != limit || result.Total < 0 || result.Items is null ||
            !Fingerprint(result.DatabaseScope) || result.Items.Count > limit || result.Items.Count > result.Total || result.Items.Any(row => row is null || row.GrantId != grantId || row.TenantId != selected.TenantId || row.OrganizationId != selected.BusinessAssociateId || row.DatabaseScope != result.DatabaseScope) ||
            result.Items.Select(row => row.Revision).Distinct(StringComparer.Ordinal).Count() != result.Items.Count)
            throw new ApiClientException("The grant retirement history has inconsistent ownership or paging.");
        foreach (var receipt in result.Items) ValidateRetirement(receipt, selected, result.DatabaseScope, grantId);
        return result;
    }
    private static void Page(int offset, int limit) { if (offset < 0 || limit is < 1 or > 100) throw new ArgumentException("Use a nonnegative offset and a page size from 1 to 100."); }
    private static bool Fingerprint(string value) => value is { Length: 64 } && value.All(char.IsAsciiHexDigit) && value == value.ToUpperInvariant();
    private static void ValidateRetirement(AssetGrantRetirement result, CompanyScope selected, string database, string id)
    {
        if (!Fingerprint(database) || result.GrantId != id || result.TenantId != selected.TenantId || result.OrganizationId != selected.BusinessAssociateId || result.DatabaseScope != database ||
            !Guid.TryParseExact(result.Revision, "D", out _) || string.IsNullOrWhiteSpace(result.ActorUserId) || string.IsNullOrWhiteSpace(result.Reason) ||
            result.Reason.Length > 1000 || string.IsNullOrWhiteSpace(result.SnapshotJson))
            throw new ApiClientException("The retirement API returned invalid command evidence.");
        try
        {
            var before = JsonSerializer.Deserialize<AppUserAssetAccess>(result.SnapshotJson);
            if (before is null || before.Id != id || before.OrganizationId != selected.BusinessAssociateId || before.DatabaseScope != database || before.ConcurrencyStamp != result.PreviousRevision || !before.IsActive)
                throw new ApiClientException("The retirement API returned an inconsistent reviewed snapshot.");
        }
        catch (JsonException failure) { throw new ApiClientException("The retirement API returned an invalid reviewed snapshot.", failure); }
    }
    private static string User(string userId) { Identifier(userId); return "/api/AccessControl/user/" + Uri.EscapeDataString(userId); }
    private static void Identifier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value != value.Trim() || value.Any(char.IsControl)) throw new ArgumentException("A canonical nonempty identifier of at most 128 characters without surrounding whitespace or control characters is required.");
    }
    private static string AssetType(string value)
    {
        Identifier(value);
        return value.ToUpperInvariant() is "FIELD" or "WELL" or "POOL" or "FACILITY" or "WORK_ORDER" ? value.ToUpperInvariant() :
            throw new ArgumentException("This asset type has no reviewed access path.", nameof(value));
    }
}
