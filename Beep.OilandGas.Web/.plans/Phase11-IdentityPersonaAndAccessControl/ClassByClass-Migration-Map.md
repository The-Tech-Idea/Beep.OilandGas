# W11-03/W11-04 Class-by-Class Migration Map

> Scope: Exact class-level migration planning from `Beep.OilandGas.Models` to canonical ownership in `Beep.OilandGas.UserManagement`.
> Baseline companion: `DataModel-And-Migration.md`

---

## Migration Actions Legend

- `Migrate+Enhance`: Move class to UserManagement canonical model and extend for new Phase 11 requirements.
- `Migrate`: Move class as-is first, then adapt in follow-up.
- `Bridge`: Keep in Models temporarily; add mapper/adapter to new canonical type.
- `Retire`: Replace by canonical type and deprecate legacy class.

---

## A. Security Model Classes (Models/Data/Security)

| Source Class | Source File | Target Class | Target Namespace | Target File | Action | Enhancement Notes |
|---|---|---|---|---|---|---|
| `USER` | `Beep.OilandGas.Models/Data/Security/USER.cs` | `AppUser` | `Beep.OilandGas.UserManagement.Models.Identity` | `Beep.OilandGas.UserManagement/Models/Identity/AppUser.cs` | Migrate+Enhance | Add lifecycle state, password policy metadata, lockout tracking, external identity key |
| `ROLE` | `Beep.OilandGas.Models/Data/Security/ROLE.cs` | `AppRole` | `Beep.OilandGas.UserManagement.Models.Identity` | `Beep.OilandGas.UserManagement/Models/Identity/AppRole.cs` | Migrate+Enhance | Add role type/category, SoD flags, system-role indicator |
| `PERMISSION` | `Beep.OilandGas.Models/Data/Security/PERMISSION.cs` | `AppPermission` | `Beep.OilandGas.UserManagement.Models.Identity` | `Beep.OilandGas.UserManagement/Models/Identity/AppPermission.cs` | Migrate+Enhance | Normalize to `resource.action.scope`, add risk level and policy mapping key |
| `USER_ROLE` | `Beep.OilandGas.Models/Data/Security/USER_ROLE.cs` | `AppUserRole` | `Beep.OilandGas.UserManagement.Models.Identity` | `Beep.OilandGas.UserManagement/Models/Identity/AppUserRole.cs` | Migrate+Enhance | Add effective date range, assignment reason, granted-by actor |
| `ROLE_PERMISSION` | `Beep.OilandGas.Models/Data/Security/ROLE_PERMISSION.cs` | `AppRolePermission` | `Beep.OilandGas.UserManagement.Models.Identity` | `Beep.OilandGas.UserManagement/Models/Identity/AppRolePermission.cs` | Migrate+Enhance | Add effective date range, source (seed/manual), approval stamp |
| `USER_PROFILE` | `Beep.OilandGas.Models/Data/Security/USER_PROFILE.cs` | `UserPersonaProfile` | `Beep.OilandGas.UserManagement.Models.Profile` | `Beep.OilandGas.UserManagement/Models/Profile/UserPersonaProfile.cs` | Migrate+Enhance | Add primary/secondary personas, default route, locale/timezone, UX preferences |
| `USER_ASSET_ACCESS` | `Beep.OilandGas.Models/Data/Security/USER_ASSET_ACCESS.cs` | `UserAssetAccess` | `Beep.OilandGas.UserManagement.Models.Scope` | `Beep.OilandGas.UserManagement/Models/Scope/UserAssetAccess.cs` | Migrate+Enhance | Add scoped constraints, access grant source, expiration, deny override support |
| `ORGANIZATION_HIERARCHY_CONFIG` | `Beep.OilandGas.Models/Data/Security/ORGANIZATION_HIERARCHY_CONFIG.cs` | `OrganizationScope` | `Beep.OilandGas.UserManagement.Models.Scope` | `Beep.OilandGas.UserManagement/Models/Scope/OrganizationScope.cs` | Migrate+Enhance | Normalize hierarchy depth/type, include tenant and org boundary metadata |
| `SETUP_WIZARD_LOG` | `Beep.OilandGas.Models/Data/Security/SETUP_WIZARD_LOG.cs` | `SetupWizardLog` | `Beep.OilandGas.UserManagement.Models.Audit` | `Beep.OilandGas.UserManagement/Models/Audit/SetupWizardLog.cs` | Bridge | Keep cross-domain setup linkage; evaluate final owner in later stream |
| `PermissionConstants` (+ nested groups) | `Beep.OilandGas.Models/Data/Security/PermissionConstants.cs` | `PermissionCatalog` | `Beep.OilandGas.UserManagement.Authorization` | `Beep.OilandGas.UserManagement/Authorization/PermissionCatalog.cs` | Migrate+Enhance | Expand to persona/domain coverage and policy-key metadata |

---

## B. Access Control Classes (Models/Data/AccessControl)

| Source Class | Source File | Target Class | Target Namespace | Target File | Action | Enhancement Notes |
|---|---|---|---|---|---|---|
| `AccessCheckRequest` | `Beep.OilandGas.Models/Data/AccessControl/AccessCheckRequest.cs` | `AccessCheckRequestDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/AccessCheckRequestDto.cs` | Migrate | Add optional field/asset scope hints and correlation ID |
| `AccessCheckResponse` | `Beep.OilandGas.Models/Data/AccessControl/AccessCheckResponse.cs` | `AccessCheckResponseDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/AccessCheckResponseDto.cs` | Migrate+Enhance | Add policy name, evaluated scopes, denial reason code |
| `GrantAccessRequest` | `Beep.OilandGas.Models/Data/AccessControl/AccessControlRequests.cs` | `GrantAccessRequestDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/GrantAccessRequestDto.cs` | Migrate+Enhance | Add justification and approval marker for sensitive grants |
| `RevokeAccessRequest` | `Beep.OilandGas.Models/Data/AccessControl/AccessControlRequests.cs` | `RevokeAccessRequestDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/RevokeAccessRequestDto.cs` | Migrate | Add revocation reason code |
| `RoleDefinitions` | `Beep.OilandGas.Models/Data/AccessControl/AccessControlRequests.cs` | `RoleCatalog` | `Beep.OilandGas.UserManagement.Authorization` | `Beep.OilandGas.UserManagement/Authorization/RoleCatalog.cs` | Migrate+Enhance | Expand to full O&G persona role catalog |
| `AssetAccess` | `Beep.OilandGas.Models/Data/AccessControl/AssetAccess.cs` | `AssetAccessDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/AssetAccessDto.cs` | Migrate | Include source role/grant metadata |
| `AssetHierarchyNode` | `Beep.OilandGas.Models/Data/AccessControl/AssetHierarchyNode.cs` | `AssetHierarchyNodeDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/AssetHierarchyNodeDto.cs` | Migrate | Add path and ancestor IDs for efficient checks |
| `GetAccessibleAssetsRequest` | `Beep.OilandGas.Models/Data/AccessControl/GetAccessibleAssetsRequest.cs` | `GetAccessibleAssetsRequestDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/GetAccessibleAssetsRequestDto.cs` | Migrate | Add pagination and includeInherited flag normalization |
| `GetAssetHierarchyRequest` | `Beep.OilandGas.Models/Data/AccessControl/GetAssetHierarchyRequest.cs` | `GetAssetHierarchyRequestDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/GetAssetHierarchyRequestDto.cs` | Migrate | Add depth and traversal mode |
| `HierarchyConfig` | `Beep.OilandGas.Models/Data/AccessControl/HierarchyConfig.cs` | `HierarchyConfigDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/HierarchyConfigDto.cs` | Migrate | Normalize hierarchy type/value fields |
| `RolePermission` | `Beep.OilandGas.Models/Data/AccessControl/RolePermission.cs` | `RolePermissionDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/RolePermissionDto.cs` | Retire | Replace by canonical `AppRolePermission` + mapper DTO |
| `UserProfile` | `Beep.OilandGas.Models/Data/AccessControl/UserProfile.cs` | `UserProfileDto` | `Beep.OilandGas.UserManagement.Contracts.Profile` | `Beep.OilandGas.UserManagement/Contracts/Profile/UserProfileDto.cs` | Retire | Replace by canonical `UserPersonaProfile` + mapper DTO |
| `UpdatePreferencesRequest` | `Beep.OilandGas.Models/Data/AccessControl/UpdatePreferencesRequest.cs` | `UpdatePreferencesRequestDto` | `Beep.OilandGas.UserManagement.Contracts.Profile` | `Beep.OilandGas.UserManagement/Contracts/Profile/UpdatePreferencesRequestDto.cs` | Migrate+Enhance | Add validation envelope for allowed profile preference keys |
| `UpdatePreferredLayoutRequest` | `Beep.OilandGas.Models/Data/AccessControl/UpdatePreferredLayoutRequest.cs` | `UpdatePreferredLayoutRequestDto` | `Beep.OilandGas.UserManagement.Contracts.Profile` | `Beep.OilandGas.UserManagement/Contracts/Profile/UpdatePreferredLayoutRequestDto.cs` | Migrate | Add route policy compatibility checks |
| `UpdatePrimaryRoleRequest` | `Beep.OilandGas.Models/Data/AccessControl/UpdatePrimaryRoleRequest.cs` | `UpdatePrimaryRoleRequestDto` | `Beep.OilandGas.UserManagement.Contracts.Profile` | `Beep.OilandGas.UserManagement/Contracts/Profile/UpdatePrimaryRoleRequestDto.cs` | Migrate+Enhance | Enforce SoD and approved role catalog |
| `ValidateAccessRequest` | `Beep.OilandGas.Models/Data/AccessControl/ValidateAccessRequest.cs` | `ValidateAccessRequestDto` | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | `Beep.OilandGas.UserManagement/Contracts/AccessControl/ValidateAccessRequestDto.cs` | Migrate+Enhance | Add action + resource + scope evaluation tuple |

---

## C. Security Interfaces (Models/Core/Interfaces/Security)

| Source Interface | Source File | Target Interface | Target Namespace | Target File | Action | Notes |
|---|---|---|---|---|---|---|
| `IUserService` | `Beep.OilandGas.Models/Core/Interfaces/Security/IUserService.cs` | `IUserManagementService` | `Beep.OilandGas.UserManagement.Contracts.Services` | `Beep.OilandGas.UserManagement/Contracts/Services/IUserManagementService.cs` | Migrate+Enhance | Use canonical models/DTOs and remove placeholders |
| `IRoleService` | `Beep.OilandGas.Models/Core/Interfaces/Security/IRoleService.cs` | `IRoleManagementService` | `Beep.OilandGas.UserManagement.Contracts.Services` | `Beep.OilandGas.UserManagement/Contracts/Services/IRoleManagementService.cs` | Migrate | Split role CRUD vs assignment APIs |
| `IPermissionService` | `Beep.OilandGas.Models/Core/Interfaces/Security/IPermissionService.cs` | `IPermissionManagementService` | `Beep.OilandGas.UserManagement.Contracts.Services` | `Beep.OilandGas.UserManagement/Contracts/Services/IPermissionManagementService.cs` | Migrate | Add policy-catalog query methods |
| `IAuthorizationService` | `Beep.OilandGas.Models/Core/Interfaces/Security/IAuthorizationService.cs` | `IAuthorizationEvaluationService` | `Beep.OilandGas.UserManagement.Contracts.Services` | `Beep.OilandGas.UserManagement/Contracts/Services/IAuthorizationEvaluationService.cs` | Migrate+Enhance | Return rich decision traces |
| `IAuthService` | `Beep.OilandGas.Models/Core/Interfaces/Security/IAuthService.cs` | `IAuthenticationService` | `Beep.OilandGas.UserManagement.Contracts.Services` | `Beep.OilandGas.UserManagement/Contracts/Services/IAuthenticationService.cs` | Bridge | Coordinate with existing auth stack; avoid duplicate credential pipeline |

---

## D. User Management Request Classes (Models/Data/UserManagement)

| Source Class | Source File | Target Class | Target Namespace | Target File | Action | Notes |
|---|---|---|---|---|---|---|
| `ApplyRowFiltersRequest` | `Beep.OilandGas.Models/Data/UserManagement/ApplyRowFiltersRequest.cs` | `ApplyRowFiltersRequest` | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | `Beep.OilandGas.UserManagement/Models/Requests/UserManagement/ApplyRowFiltersRequest.cs` | Migrate | Keep `ModelEntityBase` inheritance for migration manager compatibility |
| `CheckDatabaseAccessRequest` | `Beep.OilandGas.Models/Data/UserManagement/CheckDatabaseAccessRequest.cs` | `CheckDatabaseAccessRequest` | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | `Beep.OilandGas.UserManagement/Models/Requests/UserManagement/CheckDatabaseAccessRequest.cs` | Migrate | Keep `ModelEntityBase` inheritance for migration manager compatibility |
| `CheckDataSourceAccessRequest` | `Beep.OilandGas.Models/Data/UserManagement/CheckDataSourceAccessRequest.cs` | `CheckDataSourceAccessRequest` | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | `Beep.OilandGas.UserManagement/Models/Requests/UserManagement/CheckDataSourceAccessRequest.cs` | Migrate | Keep `ModelEntityBase` inheritance for migration manager compatibility |
| `CheckPermissionRequest` | `Beep.OilandGas.Models/Data/UserManagement/CheckPermissionRequest.cs` | `CheckPermissionRequest` | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | `Beep.OilandGas.UserManagement/Models/Requests/UserManagement/CheckPermissionRequest.cs` | Migrate | Keep `ModelEntityBase` inheritance for migration manager compatibility |
| `CheckPermissionAnyRequest` | `Beep.OilandGas.Models/Data/UserManagement/CheckPermissionAnyRequest.cs` | `CheckPermissionAnyRequest` | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | `Beep.OilandGas.UserManagement/Models/Requests/UserManagement/CheckPermissionAnyRequest.cs` | Migrate | Keep `ModelEntityBase` inheritance for migration manager compatibility |
| `CheckPermissionAllRequest` | `Beep.OilandGas.Models/Data/UserManagement/CheckPermissionAllRequest.cs` | `CheckPermissionAllRequest` | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | `Beep.OilandGas.UserManagement/Models/Requests/UserManagement/CheckPermissionAllRequest.cs` | Migrate | Keep `ModelEntityBase` inheritance for migration manager compatibility |
| `CheckRoleRequest` | `Beep.OilandGas.Models/Data/UserManagement/CheckRoleRequest.cs` | `CheckRoleRequest` | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | `Beep.OilandGas.UserManagement/Models/Requests/UserManagement/CheckRoleRequest.cs` | Migrate | Keep `ModelEntityBase` inheritance for migration manager compatibility |
| `CheckRowAccessRequest` | `Beep.OilandGas.Models/Data/UserManagement/CheckRowAccessRequest.cs` | `CheckRowAccessRequest` | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | `Beep.OilandGas.UserManagement/Models/Requests/UserManagement/CheckRowAccessRequest.cs` | Migrate | Keep `ModelEntityBase` inheritance for migration manager compatibility |
| `CheckSourceAccessRequest` | `Beep.OilandGas.Models/Data/UserManagement/CheckSourceAccessRequest.cs` | `CheckSourceAccessRequest` | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | `Beep.OilandGas.UserManagement/Models/Requests/UserManagement/CheckSourceAccessRequest.cs` | Migrate | Keep `ModelEntityBase` inheritance for migration manager compatibility |

---

## E. Existing UserManagement Classes (retain/adjust)

| Existing Class | File | Action | Notes |
|---|---|---|---|
| `UserManagementService` | `Beep.OilandGas.UserManagement/Services/UserManagementService.cs` | Refactor | Remove role placeholders, switch to canonical models, harden password handling |
| `PermissionRequirement` | `Beep.OilandGas.UserManagement/Security/PermissionRequirement.cs` | Keep+Enhance | Support policy keys and scope-aware checks |
| `PermissionHandler` | `Beep.OilandGas.UserManagement/Security/PermissionHandler.cs` | Refactor | Move from simple claims-only check to service-backed authorization evaluator |
| `UserManagementServiceCollectionExtensions` | `Beep.OilandGas.UserManagement/DependencyInjection/UserManagementServiceCollectionExtensions.cs` | Refactor | Register canonical services, mappers, policy evaluators, and audit services |

---

## Migration Sequence for W11-03/W11-04

1. Create canonical model/contract namespaces and target files in UserManagement.
2. Move or copy classes with compile-safe namespace changes and add adapters.
3. Refactor service and handlers to canonical interfaces.
4. Mark legacy Models classes as compatibility-only in planning tracker.
5. Remove compatibility layer after dependency matrix reaches zero active references.
