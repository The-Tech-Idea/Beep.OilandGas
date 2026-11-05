# Company selection and client request scope

Date: 2026-10-05. CA-03/05/06 remain partial. Extends [tenant routing and module ownership](Implementation-27-Tenant-Routing-and-Module-Ownership.md). The approved business-schema boundary remains Beep MigrationManager, with EF for the existing access repository; this slice adds no schema migration or handwritten SQL.

## Implemented behavior

The canonical CompanyScope/RepositoryCompanyAccess contracts remain in TheTechIdea.Data. CompanyRequestPolicy now supplies the same account/setup routing classification to the API middleware and client transport, including segment boundaries: similarly named business routes do not inherit a control-plane exemption.

CompanyClientContext is scoped to one client session/circuit, not a pooled HTTP handler or singleton. It holds an immutable copy of the API-confirmed selected company, account, roles and permissions. Starting a selection immediately clears previous access, increments the scope version and cancels outstanding business HTTP requests. Only the matching latest gateway result can commit. Scope identifiers are selection requests, not authorization authority; the API continues to revalidate membership and module ownership independently on every request.

CompanySelectionService reads the existing `me/companies` and `me/company-access` API gateways through CompanyAccessClient. It requires an authoritative active account, rejects mismatched responses and refuses a result that arrived after an account or selection change. Failure leaves the previous selection cleared. It does not choose the first company or invent a membership. The MudBlazor selector renders the current authoritative tenant/business-associate IDs. PPDM organization names and tenant/company provisioning remain separate work; no second organization record was introduced for labels.

CompanyAuthenticationStateProvider decorates the existing authentication provider and supports its host-environment state contract. On business routes it removes incoming roles and permissions from every identity, then exposes the selected company's API-granted ClaimTypes.Role/permission claims for the matching account. Standard `[Authorize(Roles = ...)]`, `AuthorizeView` and `User.IsInRole` continue to work. Repository/account/setup routes retain the independently resolved repository principal. A company-only Administrator cannot open repository administration, and a repository Administrator is not implicitly a business-company Administrator. IdentityServer stays authentication-only. Account changes clear selection; late original authentication-state results cannot replace a newer account.

The existing Web ApiClient and Beep facade's ServiceBase both use CompanyClientContext for business HTTP scope. Requests receive per-message tenant/company headers; HttpClient.DefaultRequestHeaders is not mutated. The facade captures scope before token lookup, so a pending operation cannot adopt another company's selectors. Responses are checked after transport and body/JSON reads. CompanyResponseStream owns its HTTP response and refuses further reads after switching. The facade no longer silently appends `connectionName=PPDM39` to every request; remaining explicit connection parameters and server consumers still need the isolation audit.

The existing main layout exposes company selection, asks for selection before rendering business workspaces, and keys the body/persona subtree by scope version to dispose stale page state. Account and repository setup remain accessible independently. Notifications clear immediately on scope change. Notification and progress SignalR clients carry company headers, bind tokens/callbacks to the captured account/scope version and dispose old connections; progress connection/disconnection is serialized. These integrated consumer changes are source-reviewed, not live hub/browser proof.

Cancellation prevents stale client results from being used. It does not claim to reverse a server mutation that already completed. Mutating workflows still require their existing transaction/idempotency evidence and the pending tenant audit.

## Verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --logger "trx;LogFileName=company.trx" --results-directory TestResults/company
```

**104 passed, zero failed/skipped**: previous 82 checks plus 22 client-context, gateway-selection and standard Blazor/ASP.NET role cases. Actual CompanyClientContext, CompanyResponseStream, CompanySelectionService, CompanyAuthenticationStateProvider and routing-policy sources are compiled in the focused target, alongside the previous production persistence/routing sources. Tests use real framework ClaimsPrincipal/AuthenticationStateProvider/IAuthorizationService and an HTTP message handler; they do not reconstruct missing SDK/diagnostics libraries.

New cases prove independent sessions, no shared default headers, missing selection before transport, account/setup exemptions and strict prefixes, captured scope before awaits, inflight cancellation, rejection of late uncancellable responses, stale download rejection, immutable role copies, cross-session request refusal, exact gateway response matching, failed/concurrent selection, account changes during lookup, company/repository Administrator separation, standard RequireRole authorization, removal of other-identity roles/permissions and late authentication-state rejection.

Published BeepDM dependencies remain used; only the previously missing analyzer warning remains in this focused run. No provider database or live server was contacted. Existing Web/Auth test callers and SignalR fixtures were updated for required scope/tenant arguments, but those integrated suites were not rerun. Web Razor rendering, Program registration, ApiClient/facade wiring and complete SignalR consumers are source-only evidence. `git diff --check` passes.

## Remaining acceptance gates

- Server SignalR groups and publishers must include tenant/company scope and revalidate membership throughout long-lived connections. Client headers/version checks do not isolate global server group names or broadcasts.
- Reconcile physical database aliases and configuration changes; connection-name ownership alone cannot establish independent databases.
- Complete tenant/company provisioning, explicit historical binding ownership reconciliation and PPDM organization labels through the existing owners.
- Audit direct startup connections, explicit connection query parameters, field/company filters, caches, user preferences, stored plans/jobs, exports/imports and workers. Legal entities within one tenant still need their domain ownership filters.
- Verify browser switching, actual cookie/OIDC claims transformation, API/hub authorization, pending writes, multiple simultaneous circuits, reconnect and revocation with the complete host.
- CA-04 period-close records and the finance/engineering/operations/regional roadmap remain open behind the foundation gates.

This slice verifies the shared client selection/transport/role boundary in focused tests. It does not mark CA-03/05/06 or the multinational release complete.
