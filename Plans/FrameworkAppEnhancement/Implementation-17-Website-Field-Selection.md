# Website field selection - 2026-09-21

## User-visible changes

Mounted the existing FieldSelector in the shared signed-in layout, making active-field selection accessible across dashboards and field workbenches. The selector provides loading, saving, refresh, empty-catalog, expired-session, denied-access and service-failure feedback using existing MudBlazor components and theme. Selection remains on the previously confirmed field until the API confirms the new field. Concurrent input is disabled during saving.

## Supporting framework

FieldSelectionState validates catalog identities and guards stale loads and disposed components. DataManagementService reads current selection through the existing API without a stale local cache, serializes field reads/writes, checks the confirmed field ID and isolates field-change subscriber failures. An unconfirmed mutation invalidates displayed field context and requires refresh. A server-declared unsuccessful change preserves the previous selection. The current-field endpoint explicitly returns 404 for no active field; only that endpoint's 404 is interpreted as a valid empty selection.

ApiClient's typed POST preserves HTTP status codes and disposes responses, allowing safe user-facing authentication/access messages. Existing app-owned authorization and API resource checks remain authoritative. No new domain models, direct database access, CSS or JavaScript were introduced.

## Verification and open acceptance

135 website tests passed, zero failed, including 11 new cases covering first selection with no active field, valid empty versus invalid catalogs, delayed confirmation and concurrent input, rejected selection, unauthorized/forbidden/mismatched responses, subscriber isolation and fresh current-field reads. The test build compiles the shared layout and selector Razor. Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/field-selection-website.trx and %TEMP%/beep-field-selection-tests.log.

Existing dependency and analyzer warnings remain. API-suite results are previous evidence, not rerun for this website-only slice. Live browser/OIDC, keyboard/screen-reader and responsive acceptance remain open. Next acceptance: sign in, select two fields, verify each field-dependent page refreshes, simulate denied/service-error responses and recover with Refresh fields. Cross-session backend field-context isolation requires separate acceptance; this slice does not establish that guarantee.
