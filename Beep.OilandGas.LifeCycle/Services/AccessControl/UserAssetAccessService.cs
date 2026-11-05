using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.AccessControl
{
    /// <summary>
    /// Service for managing user asset access
    /// </summary>
    public class UserAssetAccessService : IAccessControlService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly Func<string, IPPDMRowAccessPolicy> _rowPolicies;
        private readonly IUserAssetAccessStore _assetStore;
        private readonly Func<Task<AssetDatabaseScope>> _resolveScope;
        private readonly IApplicationAuthorizationReader _authorization;
        private readonly IApplicationRolePermissionStore _rolePermissions;
        private readonly ICompanyFieldInterestReview _interestReview;

        public UserAssetAccessService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IApplicationAuthorizationReader authorization,
            IApplicationRolePermissionStore rolePermissions,
            IUserAssetAccessStore assetStore,
            Func<Task<AssetDatabaseScope>> resolveScope,
            Func<string, IPPDMRowAccessPolicy> rowPolicies,
            ICompanyFieldInterestReview interestReview)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _rowPolicies = rowPolicies ?? throw new ArgumentNullException(nameof(rowPolicies));
            _assetStore = assetStore ?? throw new ArgumentNullException(nameof(assetStore));
            _resolveScope = resolveScope ?? throw new ArgumentNullException(nameof(resolveScope));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _rolePermissions = rolePermissions ?? throw new ArgumentNullException(nameof(rolePermissions));
            _interestReview = interestReview ?? throw new ArgumentNullException(nameof(interestReview));
        }
        public Task<FieldInterestReview> GetFieldInterestReviewAsync(string fieldId) => _interestReview.ReadAsync(fieldId);

        /// <remarks>
        /// OILGAS-CATCH-01: a check that cannot be made reaches the caller (an authorization filter or the API's handler
        /// reports it with its reference, and the request is still refused). It had been answered as access denied
        /// ("could not be verified"), and the failure itself was told to no one.
        /// </remarks>
        public async Task<AccessCheckResponse> CheckAssetAccessAsync(string userId, string assetId, string assetType, string? requiredPermission = null, string? facilityType = null)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(assetId) || string.IsNullOrWhiteSpace(assetType))
                return new AccessCheckResponse { HasAccess = false, Reason = "User and asset identifiers are required." };
            if (assetType.Equals("FACILITY", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(facilityType))
                throw RefusalException.Invalid("A facility access check requires FACILITY_ID and FACILITY_TYPE.");
            if (!assetType.Equals("FACILITY", StringComparison.OrdinalIgnoreCase) && facilityType is not null)
                throw RefusalException.Invalid("Facility type applies only to facility access checks.");
            AssetAccess? Match(List<AssetAccess> assets) => assets.FirstOrDefault(candidate => candidate.Active && candidate.UserId == userId
                && candidate.AssetId == assetId && candidate.FacilityType == facilityType
                && string.Equals(candidate.AssetType, assetType, StringComparison.OrdinalIgnoreCase));
            var access = Match(await GetUserAccessibleAssetsAsync(userId, assetType, includeInherited: true));
            if (access is null || !AssetAccessLevelChecks.Meets(access.AccessLevel,requiredPermission))
                return new AccessCheckResponse { HasAccess = false, Reason = "No current asset grant permits the requested operation." };

            // READ/WRITE/DELETE are resource levels, not names in the app permission catalog.
            // Named app permissions still require the selected company's independent permission owner.
            var permitted = !AssetAccessLevelChecks.RequiresApplicationPermission(requiredPermission)
                || await HasPermissionAsync(userId, requiredPermission!);
            if (permitted && !string.IsNullOrWhiteSpace(requiredPermission))
            {
                // Permission resolution is another await boundary; the earlier asset snapshot cannot retain revoked access.
                access = Match(await GetUserAccessibleAssetsAsync(userId, assetType, includeInherited: true));
                permitted = access is not null && AssetAccessLevelChecks.Meets(access.AccessLevel,requiredPermission);
            }
            return new AccessCheckResponse
            {
                HasAccess = permitted,
                AccessLevel = access?.AccessLevel,
                Reason = permitted ? "Access granted for the requested asset." : "Current asset access and required application permission are both required."
            };
        }

        public async Task<List<AssetAccess>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, string? organizationId = null, bool includeInherited = true)
        {
            var requested = assetType?.ToUpperInvariant();
            if (requested is not null && requested is not ("FIELD" or "WELL" or "POOL" or "FACILITY" or "WORK_ORDER" or "EQUIPMENT"))
                throw RefusalException.Invalid("This asset type has no reviewed access path.");
            var scope = await ResolveScopeAsync();
            var grants = await _assetStore.ReadAsync(userId, scope.Fingerprint, organizationId);
            var policy = _rowPolicies(userId) ?? throw new InvalidOperationException("A validated subject row policy is required.");
            var rows = new List<object>();
            foreach (var type in new[] { typeof(Beep.OilandGas.PPDM39.Models.FIELD), typeof(Beep.OilandGas.PPDM39.Models.WELL),
                typeof(Beep.OilandGas.PPDM39.Models.POOL), typeof(Beep.OilandGas.PPDM39.Models.FACILITY), typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER), typeof(Beep.OilandGas.PPDM39.Models.EQUIPMENT) })
            {
                if (requested is not null && requested != type.Name) continue;
                if (!grants.Any(grant => grant.IsActive && (grant.AssetType == type.Name &&
                    (type.Name != "FACILITY" || !string.IsNullOrWhiteSpace(grant.FacilityType)) ||
                    includeInherited && type.Name is not ("FIELD" or "WORK_ORDER" or "EQUIPMENT") && grant.AssetType == "FIELD" && grant.Inherit))) continue;
                if ((await policy.GetReadConjunctionsAsync(type, type.Name)).Count == 0) continue;
                var source = _editor.GetDataSource(scope.ConnectionName)
                    ?? throw new InvalidOperationException("The asset datasource is unavailable.");
                Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Validate(_editor, source, scope.ConnectionName);
                var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    type, scope.ConnectionName, type.Name, rowAccess: policy);
                rows.AddRange(await repository.GetAsync([new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }]));
                Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Validate(_editor, source, scope.ConnectionName);
            }
            if (scope != await ResolveScopeAsync())
                throw RefusalException.Conflict("The asset database binding changed during this operation.");
            // Recompute results from fresh subject grants; the initial registry snapshot cannot retain revoked authority.
            grants = await _assetStore.ReadAsync(userId, scope.Fingerprint, organizationId);
            var output = new List<AssetAccess>();
            foreach (var row in rows)
            {
                var table = row.GetType().Name;
                var id = row switch { Beep.OilandGas.PPDM39.Models.FIELD value => value.FIELD_ID,
                    Beep.OilandGas.PPDM39.Models.WELL value => value.UWI, Beep.OilandGas.PPDM39.Models.POOL value => value.POOL_ID,
                    Beep.OilandGas.PPDM39.Models.FACILITY value => value.FACILITY_ID, Beep.OilandGas.PPDM39.Models.WORK_ORDER value => value.WORK_ORDER_ID,
                    Beep.OilandGas.PPDM39.Models.EQUIPMENT value => value.EQUIPMENT_ID, _ => throw RefusalException.Conflict("An asset read returned an unreviewed model.") };
                var field = row switch { Beep.OilandGas.PPDM39.Models.WELL value => value.ASSIGNED_FIELD,
                    Beep.OilandGas.PPDM39.Models.POOL value => value.FIELD_ID, Beep.OilandGas.PPDM39.Models.FACILITY value => value.PRIMARY_FIELD_ID, _ => null };
                var qualifier = row is Beep.OilandGas.PPDM39.Models.FACILITY facilityRow ? facilityRow.FACILITY_TYPE : null;
                var effectiveLevel = await policy.GetEffectiveAccessLevelAsync(row.GetType(), table, row, includeInherited);
                if (effectiveLevel is null) continue;
                var matched = grants.Where(grant => grant.IsActive && grant.UserId == userId &&
                    grant.AccessLevel is "READ" or "WRITE" or "DELETE" && (grant.AssetType == table && grant.AssetId == id && grant.FacilityType == qualifier &&
                    (table != "FACILITY" || qualifier is not null) ||
                    includeInherited && field is not null && grant.AssetType == "FIELD" && grant.Inherit && grant.AssetId == field)).ToArray();
                if (matched.Length == 0) continue;
                var direct = matched.FirstOrDefault(grant => grant.AssetType == table);
                output.Add(new AssetAccess { UserId = userId, AssetType = table, AssetId = id,
                    FacilityType = row is Beep.OilandGas.PPDM39.Models.FACILITY facility ? facility.FACILITY_TYPE : null,
                    AccessLevel = effectiveLevel,
                    Inherit = direct?.Inherit ?? false, OrganizationId = matched[0].OrganizationId, Active = true });
            }
            return output;
        }

        public Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _authorization.GetRolesAsync(userId);
        }

        public Task<bool> HasPermissionAsync(string userId, string permissionId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _authorization.HasPermissionAsync(userId, permissionId);
        }

        private static void RejectOrganizationRoleScope(string? organizationId)
        {
            if (!string.IsNullOrWhiteSpace(organizationId))
                throw new NotSupportedException("Application role queries use the validated request scope; organization overrides are not supported.");
        }

        public async Task<bool> GrantAssetAccessAsync(string userId, string assetId, string assetType, string accessLevel = "READ", bool inherit = true, string? organizationId = null, string? facilityType = null)
        {
            var scope = await ResolveScopeAsync();
            return await _assetStore.GrantAsync(userId, scope.Fingerprint, assetType, assetId, accessLevel, inherit, organizationId, facilityType);
        }

        public async Task<bool> RevokeAssetAccessAsync(string userId, string assetId, string assetType, string? facilityType = null)
        {
            var scope = await ResolveScopeAsync();
            return await _assetStore.RevokeAsync(userId, scope.Fingerprint, assetType, assetId, facilityType);
        }

        private async Task<AssetDatabaseScope> ResolveScopeAsync()
        {
            var scope = await _resolveScope();
            if (scope is null || string.IsNullOrWhiteSpace(scope.ConnectionName)
                || scope.Fingerprint is null || scope.Fingerprint.Length != 64
                || scope.Fingerprint.Any(c => !char.IsAsciiHexDigit(c))
                || scope.Fingerprint != scope.Fingerprint.ToUpperInvariant())
                throw new InvalidOperationException("A reviewed module database scope is required for asset access.");
            return scope;
        }

        public async Task<AssetGrantReviewPage> ReviewStoredGrantsAsync(string userId, int offset = 0, int limit = 50, CancellationToken token = default)
        {
            var scope = await ResolveScopeAsync();
            return await _assetStore.ReviewAsync(userId, scope.Fingerprint, offset, limit, token);
        }
        public async Task<AssetGrantRetirement> RetireStoredGrantAsync(string grantId, RetireAssetGrantRequest request, CancellationToken token = default)
        {
            var scope = await ResolveScopeAsync();
            return await _assetStore.RetireAsync(grantId, scope.Fingerprint, request, token);
        }
        public async Task<AssetGrantRetirementPage> GetGrantRetirementsAsync(string grantId, int offset = 0, int limit = 50, CancellationToken token = default)
        {
            var scope = await ResolveScopeAsync();
            return await _assetStore.ReadRetirementsAsync(grantId, scope.Fingerprint, offset, limit, token);
        }

        public Task<List<string>> GetRolePermissionsAsync(string roleId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _rolePermissions.GetPermissionsAsync(roleId);
        }

        public Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _rolePermissions.GrantAsync(roleId, permissionId);
        }

        public Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _rolePermissions.RevokeAsync(roleId, permissionId);
        }

    }
}
