# Implementation 31 — Configured database target ownership

Date: 2026-10-05. Tracker: CA-05/06 and FG-12/20/21. Status: configured target identity/binding behavior verified with focused checks; complete physical isolation and the wider company phases remain open.

## Existing ownership registry extended

AppModuleConnection remains the canonical connection-owner record in TheTechIdea.Data and the existing EF repository. It now stores a credential-free PhysicalTargetKey digest plus the account/time of target verification. The nullable fields preserve historical name-only owners without inventing target ownership. Such owners cannot satisfy routing or migration-plan lookup until a repository administrator explicitly rebinds with the current module version. Initial name ownership audit remains intact.

There is one canonical connection name for each recognized configured database target. Different names, credentials, driver versions and schemas cannot create another claim for that target, including another alias in the same tenant. A tenant shares its canonical connection across its modules. A unique repository index arbitrates target claims independently of service checks.

ModuleBindingService now requires the active IDMEEditor. It verifies a unique configured connection, derives the target, checks tenant/name ownership and duplicate target claims inside the existing transaction, and records the target only on first explicit verification. Binding versions are validated before ownership mutations. A previously pinned target cannot be changed by editing configuration and rebinding; deployment reconciliation is required. Historical binding ownership assignment uses the same target gate.

ModuleConnectionResolver rechecks pinned target identity against current configuration on every resolution and migration fingerprint request. A changed endpoint/database is refused rather than routed to the new target. Migration fingerprints additionally include the physical target digest while preserving the existing full configuration fingerprint for credentials, driver/schema changes and cached migration target validation. Ownership identity and migration-plan freshness have different purposes and use the existing registry/planning paths accordingly.

## Supported configured identities

ModuleDatabaseTarget parses concrete SQL Server/PostgreSQL/SQLite connection strings with the real provider builders, and explicit Oracle host/port/service strings with the connection-string/URI builders. It normalizes endpoint case, supported local SQL Server spellings, default ports and absolute SQLite paths, and excludes credentials, display names, driver versions and schemas from the digest. PostgreSQL database-name case remains significant. SQL Server database-name casing is conservatively collapsed, preventing claims that could name the same database under a case-insensitive deployment.

Beep's ConnectionStringProcessingHelper handles recognized templates on a copied ConnectionProperties object. Reading identity never mutates ConfigEditor or writes another config store. File paths resolve against an explicit absolute Beep root; existing symbolic path components are resolved by the implementation. The focused file tests prove relative/absolute normalization, not every filesystem alias case.

In-memory/composite targets, generic parameter overrides, unresolved targets, implicit login databases, PostgreSQL multi-host/socket endpoints, SQL Server attached files, Oracle TNS aliases/descriptors and currently unsupported providers are refused. Parser errors return an explicit refusal without connection-string credentials. No datasource is opened by this configured identity/binding gate. Provider builders are parsers here; they introduce neither domain EF access nor handwritten SQL.

## Repository schema changes

EF 10.0.9 generated PhysicalModuleTargetOwnership and snapshots for:

- SQL Server: `20261005072608`.
- PostgreSQL: `20261005072634`.
- Oracle: `20261005072638`.

Each adds three nullable columns and a unique target index with provider-generated null handling. No guessed backfill, business data copy or SqlOperation is added. The source-linked focused test project was used as the tooling host; EF's generated snapshots were placed back in the canonical provider migration folders. No generated snapshot copy remains in the test project. No live database migration was applied. Business/module schemas remain on Beep Migration; this migration changes only the existing EF Identity/access/module-ownership repository.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --logger "trx;LogFileName=company.trx" --results-directory TestResults/company
```

**189 passed, zero failed/skipped**: previous 155 plus 31 configured target/binding cases and three provider migration cases. Production target parsing, binding service and resolver are source-linked alongside the canonical repository/data models. Persistence cases use real SQLite EF constraints; provider migration/script/snapshot checks do not connect to servers. Existing access/client/progress/queue/transport checks continue to pass.

Evidence includes credential/schema/endpoint spellings, local SQL Server aliases, provider/endpoint/database differences, template non-mutation, relative files, unknown/ambiguous configurations, sanitized parser refusal, cross-tenant and same-tenant duplicate alias refusal, shared canonical module bindings, configuration redirection refusal during routing/planning/rebinding, permitted credential rotation, explicit historical target revalidation, verification audit and direct database constraint rejection. All provider snapshots match current models, migration ordering remains aligned and operations contain no handwritten SQL. The pre-existing missing analyzer warning remains.

Wider API/local-provider test fixtures were updated for the required editor, concrete target metadata, pinned owners and changed refusal behavior. Those integrated targets are source changes, not executed evidence in this slice. `git diff --check` passes.

## Acceptance boundaries and next work

This digest identifies a configured namespace, not a server-proven physical database identifier. Different DNS names/IPs, SQL instance aliases, proxies/redirectors, alternate Oracle services, filesystem hard links and provider-specific catalog redirects can still identify one database through different namespaces. A provider-supported stable identity or reviewed deployment reconciliation is required before claiming complete physical alias isolation. Generated provider migrations likewise need real upgrade/concurrency checks before deployment readiness is claimed.

The existing binding gate now prevents known configured aliases and target drift. Continue with stronger physical identity/reconciliation and canonical tenant/company provisioning. Audit and replace domain services still using startup/default connections or raw connection-name overrides, including PPDM data/import/export and queued CSV execution. Apply authoritative company/field filters and review shared datasource/cache scope. Stored ownership does not secure a caller that bypasses the canonical resolver.

Full API DI/HTTP, provider, browser/OIDC, host, distributed worker/transport and remaining finance/engineering/operations/regional gates remain open. No complete multinational isolation or phase closure is asserted.
