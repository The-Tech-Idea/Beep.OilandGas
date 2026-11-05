# Company membership and role-access foundation

Date: 2026-10-05. Tasks CA-05/06. User authorized focused tests despite unavailable unrelated projects. This delivers persistence/access resolution; complete tenant isolation and the remaining roadmap stay open.

## Implementation

TheTechIdea.Data owns tenant, company and membership contracts. Company links reference existing PPDM business associates without copying organization records. Composite EF keys/foreign keys connect tenant, company, membership and scoped role metadata. Membership records include granting user, effective interval, change timestamp and concurrency stamp. Existing profile TenantId never grants company access.

The existing RepositoryAccessService resolves active company memberships and approved/current roles. AspNetUserRoles remains authority: scoped metadata narrows an existing grant and cannot invent one. Membership alone and unscoped administrator roles grant no company permissions; scoped-only roles cannot become repository-wide authority. Disabled owners, expired/future memberships, missing grants and cancellation fail closed.

RepositoryRoleAssignmentService limits repository-management lookup to unscoped assignments, refuses company-assignment revocation through that operation and handles multiple scopes for one role without duplicate-key errors. TimeProvider enables deterministic effective-date tests.

RepositoryAccountController adds read-only GET api/auth/repository/me/companies and GET api/auth/repository/me/company-access (tenantId/businessAssociateId). Account identity comes from the principal. HTTP/controller integration is outside the current focused tests.

## EF-only persistence

EF-generated CompanyAccess C# migrations and updated snapshots exist for SQL Server, PostgreSQL and Oracle. Reads use EF LINQ. No handwritten SQL, direct SQL execution or .sql artifacts were added. Scaffolding uses the actual production model/design-time factories compiled by the focused target. Existing roles remain unscoped; there is no inferred ownership backfill or privilege grant. Live-provider deployment is not exercised.

## Evidence

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -v quiet --logger "trx;LogFileName=company-access.trx" --results-directory TestResults/company
```

**36 passed, zero failed/skipped**: 20 company/role tests, nine provider schema/snapshot checks and seven bootstrap regressions. Real SQLite persistence covers tenant collisions, profile non-authority, role isolation, disabled owners, exact effective boundaries, unapproved grants, revoked Identity membership, forged foreign keys, cancellation and management separation. Provider checks verify generated schemas and matching snapshots without connecting to live providers.

FocusedCompanyTests links active production files without copying/stubbing implementations. Separate bin/company-focused and obj/company-focused artifacts prevent replacing integrated test outputs. The default remains the integrated project. CI runs focused tests independently and uploads TRX. Missing analyzer is an MSB9008 warning; full-host/analyzer completeness is outside this proof.

## Next

Continue request context, company-aware standard roles, tenant module bindings, approved provisioning, asset ownership and company switching. Global bindings/legacy controllers are not tenant-safe merely because membership exists. Cross-channel isolation, provider deployment/races, HTTP/OIDC/browser checks and finance/engineering/operations phases stay open.

User constraint: no handwritten SQL. Three additional provider checks assert both UpOperations and DownOperations contain no SqlOperation and company tables are created through EF CreateTableOperation.
