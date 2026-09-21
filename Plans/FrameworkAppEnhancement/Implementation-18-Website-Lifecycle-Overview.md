# Website lifecycle overview - 2026-09-21

## Implemented workflow

The /ppdm39/field/dashboard route now requires authentication and subscribes to the shared website field-selection event. It reuses FieldDashboardState, clearing prior data during refresh, checking the dashboard field identity and ignoring late responses after a newer selection or page disposal. It no longer fetches a separate lifecycle summary for the header, which could display a different field name.

API failures now show persistent, safe error messages with the existing Refresh action. No-selected-field guidance appears only when there is no active field and points to the shared selector above. Removed inferred Active/Quiet/Inactive badges based on arbitrary activity-age thresholds. Phase counts are labelled Records, missing summaries are Not available, and performance values use shared formatting with API-supplied units.

## Verification

139 website tests passed, zero failed. Four new regressions cover clearing selection while a dashboard response is pending, stale current-field lookup, page disposal during loading, and authentication on the actual compiled lifecycle route. The test build also compiles the modified Razor page. Existing state tests cover service failures, mismatched fields and numeric formatting.

Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/lifecycle-overview-website.trx and %TEMP%/beep-lifecycle-overview-tests.log. Existing analyzer/dependency warnings remain. API tests were not rerun for this website-only change. Live browser/OIDC, keyboard and responsive acceptance remain open; tests validate shared state and route metadata rather than rendered component interaction.

## Next acceptance

Open the lifecycle overview while signed in, select two fields while data is loading, and verify header, counts, alerts and metrics correspond to the latest selection. Check denied access and an unavailable API, then refresh after service recovery. Other field-dependent workbenches still need the same event/subscription and failure-state audit.
