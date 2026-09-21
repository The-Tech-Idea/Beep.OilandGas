# Production engineer website workspace - 2026-09-21

## Delivered

The authenticated /production/engineer-workspace page now uses the existing LifeCycleService and FieldDashboardState. It refreshes with active-field changes, rejects mismatched/late field data, clears previous results on reload and disposal, and distinguishes no selection from authentication, access and service failures. A Refresh action is available. No new domain DTO or data path was added.

Removed the page-local ProductionEngineerKpi duplicate, raw HttpClient call, exception-to-zero fallback, assumed BOE/d display, arbitrary severity thresholds and three hardcoded attention notices (including the invented three-wells-below-target claim). The workspace displays production record counts and reported field metrics with supplied phases, units and timestamps. Alerts are labelled field alerts and come from active alerts in the dashboard response; missing data is explicit. The analysis and monitoring links remain usable while field data loads or fails. Metrics have paging.

## Verification

149 website tests passed, zero failed. Two new compiled-route checks verify workspace authentication and existence of all 13 analysis/monitoring destinations. Existing shared dashboard state tests cover field mismatch, errors, late requests, clearing selection and disposal. The test build compiles the actual Razor workspace. Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/production-engineer-workspace.trx and %TEMP%/beep-engineer-workspace-tests.log. Whitespace check passed. Existing analyzer/dependency warnings remain.

No live browser/OIDC, responsive layout, screen-reader or backend data-accuracy acceptance was performed. API tests were not rerun because this slice changed the website only. Route-existence tests do not certify the destination workflows.

## Open framework finding and next acceptance

ProductionEngineerAggregationService.GetKpiAsync accepts fieldId but does not filter by it, swallows repository failures, assumes gas conversion and aggregation semantics, and leaves downtime/workover counters as default zeroes. The website no longer consumes that endpoint. The endpoint remains a separate open backend issue; do not describe its KPIs as field-scoped or verified. Resolve its contracts, units, aggregation periods and field ownership before any future client consumes it.

Browser acceptance: select two fields during loading, confirm latest field identity and data, exercise missing metrics and denied/service errors, then navigate each engineering tool. Metrics and alerts retain the accuracy limits of the existing dashboard API.
