# Beep.OilandGas.UserManagement

The active OilGas application authenticates through the shared IdentityServer client integration. Application account, company membership, role and permission authority lives in the existing Identity/access repository and is enforced by ApiService. Standard ASP.NET roles are synthesized from app-owned assignments. This project does not issue application JWTs, handle passwords/MFA or supply token field claims as authorization.

## Active account and company paths

- Shared entities and API contracts belong to TheTechIdea.Data.
- RepositoryAccessService and RepositoryRoleScope resolve current repository authority; company access uses RepositoryCompanyContextResolver.
- ApiService's RepositoryUserService, RepositoryRoleAssignmentService, RepositoryUserProfileService and canonical permission/asset stores independently enforce current authority.
- The existing `api/identity/security` routes now use CompanyRowLevelSecurityService, which is registered by ApiService. They require a validated selected company and the freshly resolved active caller.
- Row reads use the existing CompanyPPDMRowAccessPolicy and PPDM39GenericDataService. A diagnostic row check reads the complete key from the stored row through that policy; caller-supplied ownership does not prove access.
- Scope queries report current explicit company asset grants. Connection/database checks report whether a target matches the selected reviewed core binding; they do not grant row visibility.

## Row-security contract

IRowLevelSecurityService and its five request/result contracts now live in TheTechIdea.Data, retaining their namespaces. Request DTOs have no USER_ID selector; the controller passes the resolved actor separately. Row filter requests accept `Filters` as Beep AppFilter values. Responses return `ReadPredicates`, `Filters` and `DenyAll`; empty predicates deny all rows. Predicates form an OR, and each supplied filter narrows each predicate through AND. The actual CRUD path always regenerates current authority; a diagnostic response is not a permission token. No SQL expression is generated or consumed.

Database checks accept `DatabaseFingerprint`, the exact current reviewed binding digest. Source/datasource checks accept the selected binding's connection name. Generic row checks accept TABLE_NAME and ENTITY_DATA_JSON containing every canonical primary-key column. They report read visibility of an existing row, not write/delete permission or legal asset ownership.

The old PPDM RowLevelSecurityService, FieldAccessService and JWT FieldScopeAuthorizationHandler have been removed, along with their obsolete field-scope tests and unused JWT package references. The current scoped tests exercise the actual canonical authority/read path instead. No alternate PPDM scope-table reader or global wildcard claim handler is registered for these routes.

## Remaining library work

AddUserManagement is not called by the active API; SecurityModule is excluded from its setup discovery. Remaining historical PPDM user/role/persona/hierarchy/elevation/bootstrap services in this library have not been promoted as client-facing authority. Their dependencies and domain reuse must be reviewed before retirement or integration into the existing owners. They are not a second supported authentication/RBAC setup for OilGas.

Focused evidence, limitations and next work are recorded in [Implementation 38](../Plans/FrameworkAppEnhancement/Implementation-38-Company-Row-Security-Diagnostics.md) and the [master tracker](../MASTER-TODO-TRACKER.md). Full HTTP/OIDC/DI, provider and browser verification remain open. Business/module schemas use Beep Migration; the existing Identity/access repository uses EF. This slice adds no schema migration.
