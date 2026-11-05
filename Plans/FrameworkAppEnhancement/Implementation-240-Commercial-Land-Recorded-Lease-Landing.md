# Implementation 240 — Commercial and land recorded lease landing

## Visible company persona workflow

The existing `/accounting/revenue-workbench` now includes a commercial/land records panel alongside its existing assigned-work component and revenue navigation. It reuses `ILeaseServiceClient`, the canonical field-scoped lease API and `LeasePortfolioState`; it introduces no parallel lease models, persistence, company selection or field selection store.

The panel identifies the selected company and FIELD, shows a client retrieval UTC timestamp and explicitly names the selected FIELD lease API response as its source. It provides an all-recorded-leases card and cards generated from the exact status codes present in the returned records. A card filters the same recorded rows by exact status; no hardcoded status translation, legal approval or unbilled-delivery KPI is inferred.

Rows show lease identity/name/number, recorded status, land-right subtype, date zone and raw recorded interest formatted in the user's current culture. Review opens the exact lease and FIELD through the existing typed client and state owner. The result retains the complete record interpretation, recorded status/interest and effective/expiration dates, with explicit unavailable dates. The existing lease workflow route remains available for its separately authorized operations. This panel provides no create/status-write or financial-posting action.

## Access and context behavior

The existing revenue page requires authentication. The panel additionally verifies that the current principal's standard ASP.NET role set matches an authoritative selected-company role and that NameIdentifier/sub identifies the selected account; no persona-specific hardcoded role or IdentityServer business role is introduced. Selection is context rather than authority. The existing API independently governs lease reads and FIELD access.

Company, FIELD and authentication changes immediately clear the component's current lease data, selected status, review and retrieval timestamp. Queued reloads verify current scope and authentication before reading; existing state generation/cancellation/snapshot guards discard late responses. The panel creates and disposes an instance of the existing portfolio state for its own display lifecycle, avoiding disposal or clearing of another page's scoped state. No authorization or entity model is duplicated.

The portfolio owner now exposes retrieval UTC only after a valid current response and clears it when selection, load or a confirmed status command invalidates that response. `ClearSelection` invalidates the existing state and its pending reads while retaining its original uncertain-command tracking. The commercial panel does not use the state owner's mutation methods.

Missing company/FIELD, denied role, API failure, loading and confirmed empty response have separate visible states. Failed reads do not create zero-count cards. Retrieved UTC is when this client received validated records; it is not a database version, legal effective time or approval timestamp. Existing field selection and date-zone interpretation are retained.

## Focused verification

`Scripts/Test-CommercialLandPortfolio.ps1` compiles the exact current Razor component, current portfolio state and existing/new state tests against the actual canonical Data build and existing dependency assemblies. It uses the app's configured MudBlazor package version. Forty checks pass with zero failures/skips; final test duration is 415 milliseconds. Whitespace checks pass. Focused duplicate-type composition warnings remain because actual source state overrides the older cached test assembly state.

Eleven HtmlRenderer cases cover recorded rows, confirmed empty, API failure, missing company, missing FIELD, denied standard role, status-card drill-down, exact-record review, late company response, FIELD switch and authentication change. These verify encoded recorded text, matching card/row filters, identity/interpretation display, exact lease/FIELD review calls and discarded stale records. Twenty-nine current portfolio-state cases cover the underlying scope/validation/command behavior, including a new retrieval-time and selection-clear/late-response case.

An attempted broader repository focused build encountered duplicate generated assembly attributes in a transitive BeepDM project. The dedicated source/component gate above avoids that unrelated project build and passes against actual canonical Data. No full host, live HTTP/OIDC, browser/keyboard/RTL or real-provider acceptance is claimed by these mocked typed-client/rendered checks. Production registration for the lease client and field selection already exists and is unchanged.

## Remaining commercial and wider persona delivery

Recorded leases are the rights/terms source entry point, not proof of legal title, approved effective commercial terms, delivery readiness or financial settlement. Continue the source-to-approved-terms-to-invoice path with explicit versioned evidence, independent approval and actual persisted outcome/task read-back. Reconcile existing royalty and partner surfaces before displaying their source-backed company queues. Their older navigation links and screens are not certified complete by this slice.

Company persona/catalog/native authority, multinational currency/units/time zones/localization, provider/browser/accessibility and the remaining twelve persona workflows stay active in the overall plan. No CA/FG phase closes. No SQL, business EF lane, direct frontend database access, live schema migration, permission assignment, deployment or commit/push is executed.
