# Implementation 138: Procurement assigned-work landing

## Company buyer and receiver entry point

The existing `/procurement` source workbench now embeds the existing PersonaTaskInbox above supplier selection. A configured company persona whose exact DefaultRoute is `/procurement` can review its assigned approval, review and data-entry work, overdue/critical counts, and the exact contributing process records on this landing. Direct PO detail retains the source review; assigned work is on the workbench root. No new task store, workflow owner, dashboard API or role catalog is introduced.

The queue is governed by current company/persona review and the existing independently protected task API. Procurement source access continues to require current standard company roles and Procurement.View. Persona preference grants neither source access nor a mutation right. The implementation does not seed a BUYER role/persona or change company workspace mappings: BUYER in the tests is an explicit fixture. Configured roles and catalog mappings must be reviewed through the existing administration owner before use.

## Shared screen and source drill-down

PersonaTaskInbox retains loading, failure, missing-company, missing-persona and wrong-workspace states and its existing subscription/reload owner. Its recorded summary/filter/row presentation now lives in one shared PersonaAssignedWorkPanel, reused by that inbox across all experience roots. There is no parallel buyer-only task renderer.

The panel shows selected company, workspace, recorded task counts, priority/SLA, source identity, process/step, due date and recorded handoff workspace/sender. Missing sender/workspace/task creation time is explicitly unavailable. Task creation uses the recorded timestamp; no current timestamp is substituted. Source links continue to open the exact encoded process instance. A task's source label does not establish an approved PO, accepted receipt, stock movement or AP matching outcome. Counts describe loaded pending tasks and are not procurement inventory or value KPIs.

Summary controls select exact contributing rows without changing loaded source totals. Ordinary MudBlazor controls preserve the shared theme; there is no custom CSS/JS. Static rendering verifies text, links, encoding and states, not keyboard, screen-reader, responsive, RTL or browser event behavior.

## Stable reviewed task evidence

PersonaTaskInboxState previously retained the mutable transport response, and consumers could mutate its counts or source links after validation. The state now snapshots the response before validation/publication and returns independent copies of shared Data contracts from Current and VisibleRows. Mutating the transport or exposed objects cannot change the next displayed drill-down. Shared domain DTOs remain in TheTechIdea.Data; no duplicate task model is introduced.

The selected profile must match both actor and persona before the queue is requested. Existing company generation, persona/route changes, failed reload and disposal still clear prior evidence and discard late results. Company-switch behavior is verified through the existing state gate; this is page-session evidence, not a durable cross-session snapshot or transaction guarantee.

## Focused verification

`Scripts/Test-PersonaTaskInboxState.ps1` now compiles actual PersonaContextService, CompanyClientContext, the existing inbox interface/state, the actual shared Razor panel and explicit test-only render host. It obtains MudBlazor's version from the actual Web project and references the current actual Data assembly. The typed inbox/persona, static navigation and no-browser JS fixtures are explicit. Missing production diagnostics/identity projects are not replaced.

Final-source result: **47 passed**, zero failed/skipped, terminal exit 0, 1.3613 seconds. Log: `%TEMP%/oilgas-procurement-landing-138.log`. This includes the prior 38 state cases, two mutation cases, two configured buyer/mismatched-landing cases, one stale-profile case and four actual render cases covering complete/filtered/empty/cleared evidence, encoded source/sender text and missing creation provenance.

The first render checkpoint failed because the static fixture lacked MudChip's NavigationManager dependency. Adding a test-only static NavigationManager corrected the fixture; final production sources pass. The parent procurement page and full PersonaTaskInbox orchestration are still source-only because required diagnostics dependencies are missing. The gate does not claim complete parent-page compilation, HTTP/OIDC, browser events, real provider acceptance or task assignment/mutation delivery. The prior procurement source gate remains recorded separately in Implementation 137; counts are not summed across overlapping scopes.

## Remaining workflow delivery

Review and configure actual buyer/receiver/persona role mappings without granting ledger access. Verify parent component/host, two-company HTTP, browser filter/reload/company-switch and keyboard/RTL journeys. Establish units/currency/material/location evidence; implement approved demand-to-PO, controlled receipt/stock and invoice/AP handoffs through existing owners, independent duties and recorded outcomes. Beep MigrationManager remains the business schema owner. No SQL, business EF migration, migration execution, role grant or live write was performed. All remaining personas, multinational requirements and the full roadmap remain active.
