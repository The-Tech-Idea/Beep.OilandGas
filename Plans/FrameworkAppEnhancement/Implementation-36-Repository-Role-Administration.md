# Implementation 36: repository role administration

Date: 2026-10-05. Owners: CA-03/05/06 and FG-11. Extends the company authority and row authorization work in Implementations 34–35. Status: focused service/persistence verification; complete HTTP/DI and provider concurrency remain open.

## Problem and resulting behavior

RepositoryApplicationRolePermissionStore previously accepted an authenticated principal's Administrator role as permission to mutate globally shared Identity role claims. On company business routes, that role comes from the selected company's assignments. This let company authority reach shared role definitions. RepositoryRoleAssignmentService's global role/permission mutations also trusted any nonempty actor ID, unlike its company assignment methods, which already rechecked repository authority.

The existing RequireRepositoryAdministratorAsync check is now the common administration gate. It resolves the active account's current repository roles through RepositoryAccessService and RepositoryRoleScope, using the service's TimeProvider. Standard Identity memberships remain authoritative; company-only, partial-scope, revoked, pending, future and expired metadata do not grant repository administration. Administrator names use the existing RoleManager normalizer. A forged/stale principal role cannot supply the required membership. No roles/permissions are moved into IdentityServer.

Global AssignRoleAsync, RevokeRoleAsync, GrantPermissionToRoleAsync and RevokePermissionFromRoleAsync call that gate before resource lookup or mutation. The existing company assignment methods continue using the same gate. Catalog/assignment/permission controller reads independently require current repository authority as well as their existing standard ASP.NET role attributes.

Role creation now uses RepositoryRoleAssignmentService.CreateRoleAsync, which checks the actor and delegates to the existing RepositoryRoleCatalogService and RoleManager. RepositoryRolesController consumes that method. The existing catalog/store implementation is reused; role creation does not assign the new role to anybody or grant it permissions. Catalog library reads/creation remain trusted server operations; client-facing creation uses the guarded API orchestration. The controller's catalog read is separately guarded.

## Canonical permission store and account boundary

The existing permission store now independently checks fresh repository authority for reads, grants and revocations. It requires an authenticated HTTP principal and the application account resolved by the API's existing FindActingUserId function. It does not accept incoming Administrator claims as authority, a request actor selector, the token subject or a fabricated system account.

Program registers one stateless account-resolution delegate and injects it into both the existing company middleware and permission store. This preserves the auth owner's party/account resolution rather than adding another claim parser. The focused tests supply an explicit resolved-account boundary; they do not compile or replace the absent shared auth projects. Production account-resolution wiring remains source evidence until the API host is verified.

Actual grants/revocations continue through the canonical assignment service, Identity role claims and existing APP permission/history metadata. Regrant/history, permission ID/key resolution and duplicate-grant refusal remain on the same path. A repository administrator may intentionally change a shared role's permissions, which affects its explicit company assignments across tenants. A company-only administrator cannot perform that operation. This does not add tenant-local role definitions or a parallel permission table.

The company AccessControl controller retains its standard company role guards and resource access routes. Its shared-definition store now requires repository authority too. Repository-wide role management is available through the existing identity control-plane routes. A database target or selected company is not required to administer repository definitions.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false
```

**324 passed, zero failed/skipped**: previous 294 plus 30 repository administration cases. Production assignment/permission-store/access/catalog sources run with actual UserManager, RoleManager, EF Identity stores and SQLite foreign keys. No business datasource is opened and no auth/SDK implementation stub was introduced.

The cases cover company-only/ordinary/missing actors despite incoming administrator claims; every global mutation's independent refusal before resource lookup; missing/anonymous/unresolved/empty/subject-only account resolution; valid repository authority without an incoming role claim; audit/history/regrant; inactive/missing-membership/expired/future/revoked/pending/company/partial-scope authority; cross-company effects only after repository approval; definition ID/key reuse and duplicate refusal; role revocation preserving company assignments; refused/approved actual catalog creation; and standard Identity role normalization.

The existing focused company/row/client/hub/queue/migration tests remain green. The API role-extension fixtures now explicitly seed administrator authority before service mutations and use the actual actor separately from their subjects. The LocalDB permission fixture retains an explicit repository grant when company metadata is added and supplies the existing account resolver. These broader fixtures/controller/Program changes are source updates, not executed HTTP/LocalDB proof. Historical broad test expectations/results are not promoted into current verification. `git diff --check` passes; missing analyzer project warnings remain outside the requested focused gate.

## Remaining work

Verify actual controller authorization, auth-owner resolution, DI and client role-management journeys in the complete host. Prove concurrent administrator revocation versus a shared definition write on the supported providers; the current fresh check is not an atomic lock across every administrator/target-role write. Complete the older host/provider regression once its dependencies are available; no full regression claim is made here.

Next audit the account/profile/primary-role and legacy JWT/field administration paths for independent authority and scope, and continue authoritative asset assignment/existence, remaining table/reference policies, domain connection/cache/provisioning and physical database identity. Company row isolation and finance/engineering/operations/regional release gates remain open.

Business/module schema changes remain on Beep Migration. Existing access persistence uses EF. This slice adds no schema migration, handwritten SQL or live database mutation. No company phase or release is complete.
