# W11-03/W11-04 Namespace and File Target Matrix

> Scope: Concrete namespace/file targets under `Beep.OilandGas.UserManagement` for migrated and enhanced identity/access code.
> Companion: `ClassByClass-Migration-Map.md`

---

## Target Folder Topology (UserManagement)

```text
Beep.OilandGas.UserManagement/
  Authorization/
    PermissionCatalog.cs
    RoleCatalog.cs
    PolicyNames.cs
  Contracts/
    AccessControl/
      AccessCheckRequestDto.cs
      AccessCheckResponseDto.cs
      GrantAccessRequestDto.cs
      RevokeAccessRequestDto.cs
      ValidateAccessRequestDto.cs
      AssetAccessDto.cs
      AssetHierarchyNodeDto.cs
      GetAccessibleAssetsRequestDto.cs
      GetAssetHierarchyRequestDto.cs
      HierarchyConfigDto.cs
      RolePermissionDto.cs
    Profile/
      UserProfileDto.cs
      UpdatePreferencesRequestDto.cs
      UpdatePreferredLayoutRequestDto.cs
      UpdatePrimaryRoleRequestDto.cs
    Services/
      IUserManagementService.cs
      IRoleManagementService.cs
      IPermissionManagementService.cs
      IAuthorizationEvaluationService.cs
      IAuthenticationService.cs
  Models/
    Identity/
      AppUser.cs
      AppRole.cs
      AppPermission.cs
      AppUserRole.cs
      AppRolePermission.cs
    Profile/
      UserPersonaProfile.cs
      PersonaDefinition.cs
      PersonaViewPreference.cs
    Scope/
      UserAssetAccess.cs
      UserScopeAssignment.cs
      OrganizationScope.cs
    Audit/
      UserAccessAuditEvent.cs
      UserProfileAuditEvent.cs
      AuthorizationDecisionTrace.cs
      SetupWizardLog.cs
    Requests/
      UserManagement/
        ApplyRowFiltersRequest.cs
        CheckDatabaseAccessRequest.cs
        CheckDataSourceAccessRequest.cs
        CheckPermissionRequest.cs
        CheckPermissionAnyRequest.cs
        CheckPermissionAllRequest.cs
        CheckRoleRequest.cs
        CheckRowAccessRequest.cs
        CheckSourceAccessRequest.cs
  Mappers/
    LegacySecurityModelMapper.cs
    LegacyAccessControlContractMapper.cs
  Services/
    UserManagementService.cs
    RoleManagementService.cs
    PermissionManagementService.cs
    AuthorizationEvaluationService.cs
    ProfileManagementService.cs
  Security/
    PermissionRequirement.cs
    PermissionHandler.cs
  DependencyInjection/
    UserManagementServiceCollectionExtensions.cs
```

---

## Namespace Matrix

| Layer | Namespace | Notes |
|---|---|---|
| Canonical identity models | `Beep.OilandGas.UserManagement.Models.Identity` | Replacement for `Models.Data.Security` entity set |
| Canonical profile models | `Beep.OilandGas.UserManagement.Models.Profile` | Persona/profile ownership |
| Canonical scope models | `Beep.OilandGas.UserManagement.Models.Scope` | Field/asset/org access scoping |
| Canonical audit models | `Beep.OilandGas.UserManagement.Models.Audit` | Access/profile decision traces |
| User-management request models | `Beep.OilandGas.UserManagement.Models.Requests.UserManagement` | Migration-manager compatible request entities inheriting `ModelEntityBase` |
| API-facing contracts | `Beep.OilandGas.UserManagement.Contracts.AccessControl` | Request/response DTOs for AccessControl controller family |
| Profile contracts | `Beep.OilandGas.UserManagement.Contracts.Profile` | Profile update/read DTOs |
| Service contracts | `Beep.OilandGas.UserManagement.Contracts.Services` | Canonical service interfaces |
| Authorization catalog | `Beep.OilandGas.UserManagement.Authorization` | Policy names, role and permission catalogs |
| Compatibility mappers | `Beep.OilandGas.UserManagement.Mappers` | Legacy model <-> canonical bridges |

---

## Source-to-Target Namespace Rewrite Rules

1. `Beep.OilandGas.Models.Data.Security.*` -> `Beep.OilandGas.UserManagement.Models.Identity|Profile|Scope|Audit`.
2. `Beep.OilandGas.Models.Data.AccessControl.*` -> `Beep.OilandGas.UserManagement.Contracts.AccessControl|Profile`.
3. `Beep.OilandGas.Models.Core.Interfaces.Security.*` -> `Beep.OilandGas.UserManagement.Contracts.Services`.
4. Existing `Beep.OilandGas.UserManagement.Security` remains the enforcement adapter layer, now backed by canonical services.

---

## Transitional Compatibility Targets

| Compatibility Item | Target File | Purpose |
|---|---|---|
| Legacy security mapper | `Beep.OilandGas.UserManagement/Mappers/LegacySecurityModelMapper.cs` | Convert `USER/ROLE/PERMISSION/...` to canonical model classes |
| Legacy access-control mapper | `Beep.OilandGas.UserManagement/Mappers/LegacyAccessControlContractMapper.cs` | Preserve API compatibility while moving DTO ownership |
| Legacy namespace aliases (temporary) | `Beep.OilandGas.Models` compatibility wrappers | Allow staged refactor without breaking all projects at once |

---

## Implementation Checkpoints

1. Create target folders and namespaces in UserManagement.
2. Add canonical models and contracts with minimal compile-safe fields.
3. Wire mappers and update `UserManagementService` to canonical types.
4. Switch controller/service call sites to Contracts namespaces.
5. Remove compatibility wrappers after full dependency scan passes.
