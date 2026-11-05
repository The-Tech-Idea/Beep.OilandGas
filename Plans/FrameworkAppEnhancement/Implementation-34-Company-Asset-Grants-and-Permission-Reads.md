# Implementation 34 — Company asset grants and permission reads

Date: 2026-10-05. Tracker: CA-03/05/06 and FG-11/12/20. Status: existing asset registry and authorization reader verified with focused checks; business row predicates and complete HTTP/provider integration remain open.

## Existing authority paths corrected

RepositoryAssetAccessStore previously accepted arbitrary reviewed-looking fingerprints and organization arguments, defaulted reads/grants to null organization, and revoked matching assets across organizations in one database. Its Administrator check read an unscoped Identity membership, ignoring company assignment approval/expiry. RepositoryApplicationAuthorizationReader likewise read all Identity memberships/claims for a subject without company scope or assignment lifecycle.

The existing CompanyModuleConnection now exposes its already-used fresh company authority check independently of module resolution. ResolveSubjectAsync permits the selected account to inspect its own access, or a current company Administrator to inspect another active member of that company. It delegates membership/role/permission resolution to the existing RepositoryCompanyContextResolver/RepositoryAccessService. Incoming IdentityServer roles do not participate. Permission/role queries do not require an already provisioned business binding; asset operations still require it.

RepositoryApplicationAuthorizationReader now uses this canonical subject resolution. Roles/permissions come only from live approved/effective company assignments joined to standard AspNetUserRoles and permission claims. Repository-wide assignments do not become company roles. Permission comparisons remain exact. Missing/inactive/expired target memberships return no grants; an unauthorized subject lookup or lost caller authority is refused. The old unscoped reader implementation was removed.

## Canonical asset registry narrowed

AppUserAssetAccess and IUserAssetAccessStore remain the single asset grant store/contracts in TheTechIdea.Data and the existing EF access repository. No parallel company asset permission table was added. The registry already has OrganizationId and DatabaseScope; this change enforces their meaning without adding a schema migration.

Every read/grant/revoke checks the current company-bound PPDM_CORE fingerprint. A supplied digest cannot select another database or bypass binding/configuration freshness. OrganizationId defaults to the selected BusinessAssociateId and is otherwise an exact assertion against it. An explicit foreign, differently cased or invalid organization is refused. Reads/grants require a current company member; inspecting another account requires current company Administrator authority. Grant/revoke independently require that same authoritative company role. The audit actor comes from the validated company account.

Reads and revocations constrain organization as well as subject/database, with exact in-memory identity comparisons after provider queries so case-insensitive collation cannot widen the grant match. Revocation changes only the selected company's record and preserves history; an administrator may clean up a former member's grant. Deterministic existing keys retain organization in their identity. Renewal preserves initial creation audit and replaces change audit/concurrency stamp. A stored key whose identity fields contradict its ownership is refused without changing it. TimeProvider supplies audit times.

Historical null-organization grants are hidden and cannot satisfy company access or be implicitly relabeled. An explicit new company grant creates its own owned record and leaves historical records intact. This requires reviewed ownership/access assignment during rollout rather than a guessed backfill. Binding/configuration fingerprint changes similarly invalidate old grants; reassignment/reconciliation is still an explicit operational gate.

Program now gives UserAssetAccessService and BoundAssetHierarchyService the same CompanyModuleConnection resolver used by PPDM CRUD/import paths. Their existing domain service implementations remain. The role-query organization-override refusal now describes validated request scope accurately. Actual controller role guards and independent API data checks continue to use standard ASP.NET roles; no new authentication/authorization mechanism or IdentityServer logic was introduced.

## Focused evidence

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --logger "trx;LogFileName=company.trx" --results-directory TestResults/company
```

**263 passed, zero failed/skipped**: previous 241 plus 22 company asset/subject/permission cases. Production store/reader/gate/context/access sources run against real SQLite Identity/access persistence and real Beep configuration contracts. Company membership/role/permission authority in these new cases uses actual RepositoryAccessService queries rather than mocked authority. No business datasource is opened by these cases.

Evidence includes identical asset IDs in two companies sharing one tenant database, identical company/asset IDs in different tenants, independent grant/revocation/audit/renewal, organization assertions, hidden historical ownership, corrupt stored-key refusal, active/expired/missing/foreign target membership, self-only ordinary member reads, Administrator grant/revoke checks, caller membership/role approval/expiry/Identity membership revocation, exclusion of repository-only grants, exact permissions, binding changes, authorization before business provisioning, exact asset revocation and former-member cleanup. The actual updated API fixture helper is also source-linked and exercised with the production gate/store.

Affected API/local-provider fixtures now seed explicit companies, memberships, scoped standard roles and pinned bindings. They no longer rely on caller-only Administrator claims or arbitrary fingerprints. Full API tests and LocalDB reopening remain source changes, not executed provider proof in this slice. Program, UserAssetAccessService/BoundAssetHierarchyService integration and profile/controller consumption were not hosted or fully built. Existing host dependency limitations remain outside the user-authorized focused gate. `git diff --check` passes.

## Next acceptance work

This closes a prerequisite grant/permission boundary; it does not filter PPDM business rows. Generic CRUD, batch/import/export and direct ID mutations still require authoritative table/column/resource policies and field/asset predicates through the existing PPDM repository path. Inherited relationships, joint-venture/operator ownership evidence, physical asset existence and same-tenant cross-company domain data need verification before claiming business isolation.

Review RepositoryApplicationRolePermissionStore/global role-definition administration separately: a company Administrator must not gain authority to change shared role definitions across tenants. Review legacy UserManagement field/JWT scope tables, profile primary-role writes, datasource/cache lifetime and other raw/default connection paths. These are recorded remaining requirements, not compatibility paths added here.

Actual HTTP/DI, provider concurrency/upgrade, browser/OIDC, stronger physical identity/provisioning and complete company/finance/engineering/operations/regional release gates remain open. Business schemas remain on Beep Migration; this slice changes no schema, adds no handwritten SQL and applies no live migration. No full phase is complete.
