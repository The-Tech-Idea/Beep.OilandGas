# Website field dashboard - 2026-09-21

## User-visible changes

Reworked `/dashboard` around the selected field's reported data. Removed invented trend messages, the hardcoded production unit and the active-well value labelled Active Fields. Lifecycle summary cards now explicitly say exploration/development/production/decommissioning records. Performance cards use each API metric's label, value and supplied unit. Missing summaries and values say Not available; a real reported zero remains zero.

Added loading, no-selected-field, expired-session, denied-access, missing-field and service-error states with a refresh action. Previously displayed figures are cleared when refreshing or switching fields. A response for a different field is rejected and a late request cannot replace a newer field's dashboard. Recent activity is drawn only from that field's dashboard, with a distinct empty state and paging.

Start Work shortcuts are now actual MudBlazor links, supporting keyboard activation and ordinary browser link behavior. The persona badge and theme remain on the shared app infrastructure. No new CSS/JS or invented colors were introduced.

## Supporting website framework

LifeCycleService.GetFieldDashboardAsync now propagates HTTP errors and rejects a null response rather than returning a fabricated empty dashboard. The main dashboard no longer falls back to unrelated audit activity when its data source fails. The other existing dashboard consumer already catches errors and clears its payload.

FieldDashboardState owns request-generation checks, selected-field matching and display-safe metric conversion. Numeric JsonElement values from object-valued API properties are formatted without invalid decimal casts. Complex/missing values are not displayed as zero. The page subscribes to field changes and cleans up subscriptions and state on disposal.

## Validation

**124 website tests passed**, zero failed. Twelve new cases cover missing field without requests, clearing previous figures on failures, 401/403/service messages, late responses, wrong-field payloads, JSON numeric/missing/complex values and typed-client failure propagation. The test build compiles the actual dashboard Razor page. Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/field-dashboard-website.trx; log `%TEMP%/beep-field-dashboard-tests.log`. Whitespace check passed.

Existing dependency/analyzer warnings remain. This slice changed the website/client only; previous API-suite results remain historical evidence and were not rerun. Live browser/OIDC, responsive layout, keyboard and screen-reader acceptance remain open. Next website validation should select two fields while loading, simulate an outage, verify retry, and compare displayed metrics with the actual API response. Backend completeness/accuracy of reported metrics is outside this website display change.
