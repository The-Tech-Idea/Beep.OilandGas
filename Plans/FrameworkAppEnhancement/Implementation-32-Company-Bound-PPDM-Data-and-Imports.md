# Implementation 32 — Company-bound PPDM data and imports

Date: 2026-10-05. Tracker: CA-03/05/06 and FG-12/20/21. Status: production connection gate and import runner verified with focused checks; API/Web wiring is source-only and full company data isolation remains open.

## Canonical core connection gate

CompanyModuleConnection is a scoped API service over the existing ModuleConnectionResolver, RepositoryTenantContext and RepositoryCompanyContextResolver. It requires the validated selected company, refreshes that account's authoritative membership and captured roles/permissions, and resolves the tenant's PPDM_CORE binding with the existing ownership/configuration fingerprint checks. Missing selection, inactive tenant, lost access, missing binding and redirected configuration are refused before a business datasource is opened.

Optional query/body connection names are assertions against this bound connection. They cannot route an operation to another configured database. Explicit empty assertions are invalid; names match case-insensitively. The first resolved AssetDatabaseScope is retained for the lifetime of the scoped service. A later resolution with a changed binding/configuration fingerprint is refused. A new request may capture a newly reviewed binding. This does not establish an atomic transaction between configuration lookup and provider access.

PPDM39GenericDataService now receives this gate instead of the startup connection string. Its five existing CRUD methods resolve the canonical binding before metadata/repository construction; mutations additionally require the selected company's authoritative account. Interface/controller defaults no longer inject PPDM39. The existing entity lookup now asks the actual PPDM model assembly, consistent with the repository's model ownership, rather than searching the API assembly for an unqualified type name. No second domain store or domain EF path was added.

## Existing CSV admission/execution extended

The import/export controller resolves the company-bound core connection before reading an upload or constructing an export repository. Query/body assertions are both checked for exports; exports require server-assigned operation IDs. Startup/first-configured/default routing is removed from these paths.

CsvImportJob now captures the existing immutable CompanyScope and AssetDatabaseScope plus the authoritative account and owned upload bytes. Its ConnectionName is derived from the captured database scope. There is no name-only job constructor. The existing queue continues to capture validated request authority and restore/revalidate it in the worker scope.

CsvImportJobRunner checks company/account/database identity and fresh access before creating its temporary input or invoking ICsvImportExecutor. CsvImportExecutor independently performs the same check before constructing PPDMGenericRepository. A changed company, account, binding, physical target or configuration fingerprint refuses queued execution. Credential rotation also changes this fingerprint and requires a newly submitted import. Successful execution awaits repository completion before deleting input; row errors or execution failures mark the existing owned operation failed. Partial writes remain possible and are reported as such. The repository has no cancellation token; cancellation cannot interrupt an already executing provider import.

The active Core FileImportResult/FileImportError contracts moved unchanged from PPDMGenericRepository.cs into TheTechIdea.Data/OilGas/FileImportModels.cs. Their existing namespace remains; there is one active definition and no forwarding/compatibility model. PPDM39 now explicitly references the canonical data assembly. The existing ICsvImportExecutor/runner were separated from the concrete executor file so focused tests compile actual execution orchestration without substituting SDK or repository implementations.

The Web data-management service's CRUD/import/export defaults now omit connection assertions unless explicitly supplied. Insert/update send the raw entity dictionary expected by the existing API actions, with any assertion in the query; the old GenericEntityRequest wrapper was incompatible with these actions. All access remains through the existing typed API client. No client database connection was added.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --logger "trx;LogFileName=company.trx" --results-directory TestResults/company
```

**216 passed, zero failed/skipped**: previous 189 plus 27 connection/capture/runner checks. The tests source-link the production gate, job and runner alongside the existing resolver, access/context sources and canonical data/repository models. SQLite persists real owner/binding constraints; mocked access responses exercise revocation through the production context resolver. Existing real access-service checks continue to pass. Actual Beep packages supply configuration contracts and provider parsers.

Evidence covers two tenant bindings, no startup fallback, foreign/conflicting/empty assertions, missing selection/membership/roles/permissions, inactive tenant, cancellation/missing binding, forged actor, same-request binding changes, captured shared scope/fingerprint, substituted tenant/company/account, changed binding/target/credentials, refreshed grants and real runner refusal/completion/file ownership/cleanup for success, row errors and executor errors. The gate never calls GetDataSource/OpenDataSource in these checks. Executor tests use the existing ICsvImportExecutor boundary; they do not execute a real PPDM import or simulate it as provider proof.

API test fixtures were updated for required gate/job metadata, real SQLite binding fixtures and pinned configured target ownership. Those full API tests, concrete CsvImportExecutor, generic data service, controller actions, Program DI and Web changes were not compiled/executed as an integrated host in this slice. No replacement missing-library contracts or SDK stubs were introduced. The existing missing analyzer warning remains. `git diff --check` passes.

## Remaining acceptance gates

Tenant binding ownership does not enforce company/field row predicates within a tenant's database. Authoritative company/field filters, allowed table/column policies, direct ID/mutation authorization and cache/datasource lifetime review remain mandatory before claiming complete business isolation. Other domain services still take startup/default/raw connection names. Validation, quality, versioning, defaults and background domain paths require the same routing audit.

The facade client's DataManagementService.CRUD.cs still uses the historical `/api/ppdm39data` routes and generic response shapes, while the actual controller exposes `/api/ppdm39/data` and response envelopes. Align that existing client with the canonical contract; do not add a second API route or parallel DTO implementation. Review UI callers that still send explicit global connection preferences. Broader shared contract ownership remains open beyond the active import contracts moved here.

Stronger server-proven physical identity/reconciliation, canonical provisioning, configuration/provider concurrency, actual provider CRUD/import/export, HTTP/DI, browser/OIDC and distributed/durable recovery remain open. Existing generated EF migrations concern only the Identity/access/module-ownership repository. Business/module schemas continue through Beep Migration. This slice adds no migration, handwritten SQL or live database mutation, and closes no complete company phase.
