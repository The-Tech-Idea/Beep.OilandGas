# Production workspaces and KPI retirement - 2026-09-21

## Implemented

Migrated /production/operations to the existing field dashboard path. Its old ProductionMgrKpi model did not match the endpoint response, so successful requests still yielded default figures. Removed that duplicate model, raw HTTP call, zero fallback, assumed daily production/uptime/efficiency figures, and hardcoded WELL-042/intervention notices. Added standard authentication on the route.

Extracted ProductionFieldOverview from the engineer workspace. Both production workspaces now render this one component for field data, refresh, missing/error states, reported metrics/units/timestamps and active field alerts. The component owns field-change subscription cleanup and uses the existing FieldDashboardState response-generation and field-matching guards. Each page retains its own analysis/operations navigation. No additional domain DTO, alternate data store or compatibility path was added.

The operations page was the last source consumer of /api/production/engineer/kpi. Removed ProductionEngineerController, ProductionEngineerAggregationService, its API-local KPI class and DI registration. This retires the implementation that ignored fieldId, suppressed failures, assumed conversion/aggregation semantics and defaulted unimplemented counters to zero. Repository source search found no remaining references to the route, service or duplicate models. This is an intentional development-time endpoint removal under the user's no-legacy requirement; no redirect or replacement KPI contract is introduced.

## Verification

- Website: 150 passed, zero failed. Compiles both pages and the shared component; authenticated-route coverage now includes operations. Navigation checks cover the 17 distinct engineer/operations destinations.
- API: 954 passed, 10 skipped, zero failed (964 total), including the actual API build after controller/service/registration removal. Existing LocalDB skips remain open.
- Whitespace check passed; existing dependency and analyzer warnings remain.

Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/production-workspaces.trx; Beep.OilandGas.ApiService.Tests/TestResults/production-kpi-retirement.trx; %TEMP%/beep-production-workspaces-tests.log and %TEMP%/beep-production-kpi-retirement-tests.log.

## Remaining acceptance

Live browser/OIDC, responsive/keyboard checks and cross-session field-context isolation remain open. Verify both routes while switching fields, failing and retrying requests, and following their tools. Tests establish route metadata, build compatibility and shared state behavior, not rendered interactions or accuracy of the existing field-dashboard aggregation. Any future daily production, uptime or efficiency feature needs explicit source data, units, periods and ownership rather than reinstating inferred/default KPIs.
