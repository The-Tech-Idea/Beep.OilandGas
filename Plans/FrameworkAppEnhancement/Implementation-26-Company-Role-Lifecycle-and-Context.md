# Company role lifecycle and immutable access context

Date: 2026-10-05. Tasks CA-03/05/06, extending [membership foundation](Implementation-25-Company-Membership-and-Access.md). Implementation remains partial at the company-isolation level. User approved Beep Migration for business/module schemas and EF for the existing Identity/access repository, with focused testing while unrelated source projects are unavailable.

## Delivered behavior

The existing RepositoryRoleAssignmentService now assigns, revokes and lists company grants. TheTechIdea.Data owns AssignCompanyRoleRequest and the extended RepositoryUserRole contract; no second role store or authorization engine was introduced. AspNetUserRoles remains required for every grant, with APP_USER_ROLE describing company scope, approval, reason, actor and effective interval.

Repository administrators manage these control-plane operations. The existing controller requires standard `[Authorize(Roles = "Administrator")]`; the service independently resolves active repository-administrator authority from the application's own tables. A company-only Administrator role does not permit repository administration. IdentityServer remains authentication-only.

Assignment requires an active membership in the exact tenant/company, a recorded reason and an expiry within that membership's interval. Disabled accounts, missing memberships, duplicate grants, pending/future repository grants and ambiguous unannotated Identity membership are refused. An existing unannotated membership must be explicitly classified; company assignment never silently changes its repository authority. Both Identity membership and company metadata are saved in the same EF transaction.

Revocation preserves another company's grant and a repository grant for the same Identity role. Only the final approved, non-expired grant removes Identity membership; future approved grants retain that membership without becoming current authority. Revocation records its actor and effective end, preserving historical records. A company assignment cannot be revoked through another company's selector or the repository-only operation. Time-limited repository grants can be revoked before expiry.

RepositoryRoleScope is the shared EF query for repository-administration membership. Access resolution, repository assignment listings, last-administrator checks and readiness use it. Expired/revoked metadata cannot reappear as implicit Identity authority. Company-only administrators do not satisfy repository recovery or last-administrator protection. User-management and readiness consumers were updated in source; those complete host paths are not compiled/executed by the focused target.

RepositoryCompanyContextResolver returns an immutable snapshot of the selected company, current account and scoped roles/permissions. Every call rechecks the existing access service; it stores no mutable selection across requests. Returned DTO arrays are copies. The existing account company-access endpoint uses this resolver. This is the read-only role gateway foundation; domain routing and UI claims/switching are not enabled by this slice.

## API surface

The existing `api/identity/roles` controller adds:

- GET `tenants/{tenantId}/companies/{businessAssociateId}/users/{targetUserId}/assignments`: scoped grant history, including revoked records.
- POST the same path: AssignCompanyRoleRequest with RoleId, required Reason, optional EffectiveToUtc. The actor comes from the principal, not the request body.
- DELETE `tenants/{tenantId}/companies/{businessAssociateId}/assignments/{userRoleId}`: revoke in the exact selected scope; wrong/missing/already-revoked records return not found.

Typed refusals remain handled by the existing API exception boundary. Concurrent EF/unique-key refusals use the controller's existing conflict/reporting path. Endpoint wiring and HTTP authorization are source-reviewed; focused tests execute the production service, not the full API pipeline.

## Persistence and verification

EF-generated CompanyRoleRevocation C# migrations and snapshots for SQL Server, PostgreSQL and Oracle add nullable REVOKED_BY_USER_ID to existing APP_USER_ROLE. No existing actor is invented or backfilled. No handwritten SQL, direct SQL or SQL files were added. Business/module migrations remain on Beep Migration. No migration was applied to a deployed database.

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -v quiet --no-restore --logger "trx;LogFileName=company-access.trx" --results-directory TestResults/company
```

**62 passed, zero failed/skipped**: 43 company/access/lifecycle checks, 12 provider migration/schema checks and seven bootstrap regressions. Source-linked tests execute the actual EF model, access service, immutable resolver and role service against real SQLite with foreign keys. Tests cover colliding tenant identifiers, membership/role expiry, company-only administrator refusal, separate revocation lifecycles, history/actor preservation, retry, pending grants, immutable response snapshots and revalidation after membership revocation. Injected metadata-save failure proves transaction rollback removes the new Identity membership and prevents accidental repository authority.

Provider checks confirm matching EF snapshots and generated schema operations without live connections. Company migrations' Up and Down operations contain no SqlOperation. CI's existing focused company job includes these tests automatically. The missing shared analyzer remains an explicit MSB9008 warning; focused results do not establish full-host/analyzer readiness.

## Remaining gates and next implementation

CA-05/06 remain partial. ModuleDatabaseBinding is still globally keyed by ModuleId; domain services and background workers still use global routing. Next, implement explicit tenant ownership migration/preflight and validated request routing together, updating setup, workers and clients. Do not mint company roles into domain requests while those requests can reach globally bound data. Unowned existing records must not be assigned a guessed tenant.

Approved tenant/company provisioning, PPDM business-associate verification, membership-management audit/regrant, company switching, asset ownership, caches/files/exports/hub isolation and BeepDiA admin consumption remain open. Live-provider migration/concurrency, direct HTTP, OIDC and browser checks are not covered by these focused tests. Finance/engineering/operations phases retain their existing acceptance gates.

Period-close source audit also found inconsistent markers: ProductionAccountingService's status read searches PC/ENTRY_TYPE, while PeriodClosingService posts CLOSE/REFERENCE_NUMBER/SOURCE_MODULE. Align this existing owner path when implementing CA-04; it is not repaired or verified in this slice.
