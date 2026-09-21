# Main production dashboard - 2026-09-21

## Delivered website workflow

The authenticated /production page now shows recorded well status, UWIs, latest test dates and water cut only when a dated test exists. It has explicit loading, no-selection, empty-list, denied/session and service-error states. Refresh no longer calls a lifecycle initialization method or announces success after failure. Field changes reload the page data and dispose unsubscribes listeners. Review links are keyboard-accessible links with escaped well identifiers, and the grid has paging.

Removed the unconditional Field Online badge, real-time/today claims, unimplemented historical-chart promise, inferred operational alerts, and aggregates of oil/gas values whose contract provides no units or consistent period. The service reads raw latest WELL_TEST amounts, so BOPD/MMSCFD claims were unsupported. The API summary actually counts active WELL and WELL_ACTIVITY records; cards now describe those counts accurately rather than calling active activities open work orders. Field metrics/alerts remain reachable through Production Operations. Analysis, allocation, forecasts and well-test navigation are preserved.

ProductionDashboardState publishes a result only after both summary and well reads succeed. It validates summary field identity, unique nonblank well IDs and the current field after loading, and ignores superseded/disposed requests. Failures clear prior results rather than displaying partial/stale summaries. ProductionServiceClient rejects a null well payload instead of converting it into an empty list. The forecasting consumer already catches client exceptions.

## Verification

159 website tests passed, zero failed. Nine new cases cover no field without requests, valid empty wells, mismatched summary, selection change between reads, failed wells clearing prior data, late responses after reset/disposal, and null versus empty client payloads. The real Razor page builds in the test run. Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/production-dashboard.trx and %TEMP%/beep-production-dashboard-tests.log. Whitespace check passed. Existing dependency/analyzer warnings remain.

API results remain the previous 954 passed/10 skipped; not rerun for this website-only slice. Live browser/OIDC, grid links and responsive/accessibility acceptance remain open.

## Remaining framework boundary

Well rows have no field identity or unit metadata. Before/after current-field checks and page request generations prevent observed selection races but do not provide an atomic snapshot or cross-session isolation guarantee. A future explicit field-scoped response should bind the full summary and rows to one request and expose validated units/periods before rate aggregates return. No synthetic rates or substitute values were introduced. Next browser acceptance: select fields during loading, verify all displayed rows update, fail either request and retry, then follow the well review links.
