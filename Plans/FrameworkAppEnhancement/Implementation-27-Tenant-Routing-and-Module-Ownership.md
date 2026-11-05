# Tenant routing and module ownership

Date: 2026-10-05. CA-03/05/06 remain partial. Extends [role lifecycle and access context](Implementation-26-Company-Role-Lifecycle-and-Context.md). The approved persistence boundary remains Beep Migration for business/module schemas and EF for the existing Identity/access repository. No handwritten SQL was added.

## Implemented behavior

The existing ModuleDatabaseBinding registry now uses a generated BindingId primary key and a unique tenant/module index. Independent tenants can bind the same module ID. TenantId remains nullable solely to preserve historical records without guessing their owner. Unowned records never satisfy domain connection resolution.

AppModuleConnection, owned by TheTechIdea.Data, records exclusive ownership of each normalized BeepDM connection name. A tenant can use its connection for several modules; another tenant cannot claim that name. EF primary/alternate keys and a composite binding foreign key enforce this boundary independently of service checks. This establishes connection-name ownership; physical database aliases are a separate, open acceptance gate. [Implementation 31](Implementation-31-Configured-Database-Target-Ownership.md) now adds configured namespace target pinning and known alias protection; server-proven physical identity remains open.

ModuleBindingService extends the canonical registry with active-tenant checks, transactional connection claims, optimistic binding versions and explicit ownership assignment. Existing rows require their BindingId, current stamp, target tenant and recorded evidence; ownership cannot be reassigned through that operation. Repository administrators can inspect unowned rows through GET `api/setup/modules/unowned` and assign through POST `api/setup/modules/{bindingId}/owner`. This API exists; a reconciliation UI and tenant/company provisioning remain open.

ModuleConnectionResolver requires request/worker tenant scope, rechecks active tenancy and resolves only that tenant's owned binding and uniquely named configured connection. Migration fingerprints include tenant, binding ID and version, so a plan cannot be reused after a tenant or binding change. Module planning and seeding consult that same resolver before touching business schema/data.

CompanyRequestMiddleware runs after authentication and before asset middleware/authorization. Business API and hub requests require exactly one `X-OilGas-Tenant` and `X-OilGas-Company`, resolve authoritative account membership and apply the selected company's standard ASP.NET role/permission claims. Roles and permissions from every incoming identity are removed before scoped authority is installed. Headers alone do not grant authority. Account role-gateway endpoints retain their existing independent membership checks.

Repository/setup control-plane requests use independently resolved repository-administrator authority. They accept a tenant header or `tenantId` query selector, reject ambiguous/disagreeing selectors and recheck active tenancy. The existing setup controller exposes active tenants. Its typed Web client and MudBlazor screen explicitly select a tenant and carry it through binding, planning, approval, execution and seeding. Tenant changes discard the reviewed plan, binding, approval and seed confirmation; no tenant is selected implicitly.

The queue captures validated request tenancy and restores it in each fresh execution scope. Lifecycle SLA work enumerates active tenants with a lifecycle binding and resolves each in a fresh scope; escalation keys include tenancy. These worker/host consumers were updated in source, without focused runtime proof for the complete background workflows.

## EF registry migration

EF tooling generated TenantModuleBindings and snapshots for SQL Server (`20261005054834`), PostgreSQL (`20261005054852`) and Oracle (`20261005054856`). The upgrade adds a generated binding ID, nullable ownership fields, the connection-owner table, uniqueness and restrictive foreign keys. It preserves existing binding rows and does not invent ownership. No application database was migrated during this slice.

Downgrading to the former module-only primary key requires reconciliation when multiple tenants use the same module. Provider upgrades on populated databases, identity-column behavior and restore/recovery remain unverified. These generated C# migrations concern the repository registry; domain schema execution remains Beep MigrationManager.

## Focused verification

Command:

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --logger "trx;LogFileName=company.trx" --results-directory TestResults/company
```

Result: **82 passed, zero failed/skipped**: 43 company/access/lifecycle, 17 tenant-binding/request-middleware, 15 provider migration/schema/snapshot and seven bootstrap checks. The target compiles actual production persistence, access, binding, connection resolver and middleware sources with published BeepDM 3.1.1 packages. It does not substitute copied implementations or missing host libraries. Aligned Roslyn workspace references satisfy the combined EF design/Beep package dependencies. CI uses the same published-package path.

Tests cover identical module IDs in two tenants, case-insensitive exclusive connection-name ownership, same-tenant sharing, database key/foreign-key constraints, explicit evidence/versioned ownership, inactive/missing tenancy, unowned rows, missing ownership/configuration, migration fingerprint changes, missing/forged/ambiguous company selectors, stripped incoming roles across identities, independent setup administrator checks and immutable scope. Provider checks verify generated EF operations, no SqlOperation, retained nullable historical ownership, matching migration order and current snapshots; provider script generation does not connect to a database. Runtime persistence cases use SQLite with foreign keys enabled.

The missing analyzer produces MSB9008; the focused build/test still succeeds. Full host, Web Razor, setup-controller HTTP wiring and background consumer execution were not verified by this target. Updated existing integration fixtures are source changes, not fresh passing evidence. `git diff --check` passes.

## Next gates

- Add the shared business-company selector and pass headers through typed business API and SignalR clients. Until then, scoped business endpoints refuse requests lacking selection; this is an integration gap, not a completed app journey.
- Finish tenant/company provisioning and administrative ownership reconciliation using authoritative PPDM business associates and the existing registry.
- Prevent distinct BeepDM connection aliases or later configuration edits from pointing independent tenants at one physical database. Connection-name uniqueness alone does not prove physical isolation.
- Audit services still constructed with the startup connection, shared module/datasource caches, field/company filters, stored plans/jobs, exports, imports, notifications and worker recovery. Request middleware does not by itself isolate those channels or legal entities within one tenant.
- Exercise authentication/claims transformation and real HTTP/hub authorization, concurrent tenants, queued Beep execution, provider upgrades on populated databases, and browser tenant switching.
- Only after those foundation gates, promote CA-04 authoritative period close and subsequent finance, engineering, operations and multinational release phases.

This slice proves scoped registry/routing behavior in focused tests. It does not establish complete multinational isolation or release readiness.
