# Implementation 133: Configured accountant landing catalog

## Accountant landing and company context

The existing /accounting/dashboard and /ppdm39/accounting landing was a fixed catalog of process IDs and reporting-standard labels. It now loads the existing API definition catalog for the current company/FIELD and shows active FINANCIAL definitions with actual name/identity, source entity, reported version, description and configured reference. No reporting regime is assumed from a landing label. The existing assigned-work inbox, cost/revenue workbenches and standard Accountant/Administrator-gated executed-period-check link remain. Fixed seed process links were removed instead of retained as a fallback.

FinancialProcessCatalogState is a UI projection of existing ProcessDefinition records, not a new domain contract or persistence owner. It captures the selected company/version/FIELD before reading, rejects missing/ambiguous definitions, freezes displayed fields and accepts only active financial rows. Company/FIELD changes and failed reloads clear previous content; late responses cannot populate a new context. Metadata accepts actual strings or JSON string values; missing/unsupported configured references stay unavailable. The component includes responsive labels, pagination, encoded definition routes/text and explicit catalog-versus-transaction context. The display/load timestamp is an actual retrieval time, not a fabricated source modification time.

The API's existing known-category list omitted FINANCIAL even though financial definitions are already seeded through the existing process owner. ProcessDefinitionCatalog centralizes the existing list plus FINANCIAL, validates each returned category and unique definition identity, and refuses a missing category rather than returning a partial success. The existing controller resolves its operation service once, checks original FIELD at the end and reports source conflicts. This extends the current endpoint; it does not add another catalog store or role mechanism. Categories outside this existing list remain outside this endpoint's coverage.

The existing Web client now refuses a null catalog response. PPDMProcessService no longer substitutes version 1.0 for a missing recorded VERSION; the component shows that absence as unavailable. Full client/controller/PPDM mapping compilation and execution remain unverified in this slice. Domain ProcessDefinition ownership currently remains in the existing Models owner; broader canonical Data consolidation is still open, and this UI projection does not introduce a duplicate business DTO.

Configured references and seed descriptions are displayed as recorded metadata; they do not certify company applicability or compliance. Catalog visibility grants no execution or approval rights. Existing app-owned standard role/resource/process-step checks remain independently authoritative. No schema change, SQL, business EF lane, migration execution or live writes.

## Focused verification

Scripts/Test-ProcessInstanceDetailState.ps1: **76 passed**, zero failed/skipped, terminal exit 0, 0.9408 seconds. Twelve new catalog-state cases cover active financial selection, frozen displayed values, inactive/other-category exclusion, malformed/duplicate sources, empty response, failed reload, late company/FIELD changes and metadata representation. The preceding 64 process source/JSON/state cases remain green. Log: `%TEMP%/oilgas-finance-catalog-state-133.log`.

Scripts/Test-JournalSourceRendering.ps1: **13 passed**, zero failed/skipped, terminal exit 0, 1.1465 seconds. Five new actual financial-catalog component cases cover actual names/source/encoded route, missing references/version without a default regime, empty response, unconfirmed/company-cleared content and encoded source metadata. Eight journal/maintenance component cases remain green. Actual MudBlazor is used with a test-only popover host and no-browser JS boundary. Log: `%TEMP%/oilgas-finance-catalog-render-133.log`.

Scripts/Test-ProcessTransitionReviews.ps1: **46 passed**, zero failed/skipped, terminal exit 0, 0.9925 seconds. Five new actual API catalog-helper cases include FINANCIAL and reject missing/wrong-category/empty-identity/duplicate results. Existing 41 source-action review cases remain green. Log: `%TEMP%/oilgas-finance-catalog-api-133.log`.

No Data source changes; the clean actual Implementation 132 Data assembly is reused. Whitespace check passes. Gates execute current API helper/UI state/Razor component against actual process-model dependencies. They do not compile/execute the complete accountant page, client/controller/PPDM mapper/full host, live HTTP/OIDC, browser events, real provider reads or role assignments.

## Remaining accountant and full-plan work

Verify parent accountant landing -> assigned source detail -> independent approval/post -> recorded readback -> identical dashboard drill-down through the actual host/client/controller/browser/provider. Invoice matching/payment/reconciliation, currencies/books/periods, authoritative period locks/close/reopen, duty assignment, durable transaction outcomes, configured jurisdiction applicability and native providers remain required. Continue every other persona and multinational requirement under the existing owners. No persona, phase or goal closes.
