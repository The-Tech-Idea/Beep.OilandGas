# Field-bound production dashboard API - 2026-09-21

## Design and implementation

Replace the main production dashboard's separate mutable-current-field reads with one explicit GET /api/fields/{fieldId}/production/dashboard request. The route requires authentication and applies the existing RequireAssetAccess field guard. IdentityServer remains authentication-only; the app's existing access service decides access to the requested field. No new authorization mechanism or identity role source is introduced.

ProductionDashboardController uses IPPDMProductionService to request summary and wells with the same route field ID. It rejects a mismatched summary and returns an error if either read fails, so no partial success is published. It does not resolve or mutate IFieldOrchestrator.CurrentFieldId. Existing production repository methods now appear on the existing service interface for injection/testing.

ProductionDashboardResponse, ProductionDashboardSummary and ProductionWellStatusDto have one owner in TheTechIdea.Data. The latter two were moved from Beep.OilandGas.Models rather than copied, retaining their namespace for existing consumers. Models references Data; no type forwarder, duplicate definition or compatibility model was added.

The Web typed client encodes the field identifier and rejects null responses. ProductionDashboardState now consumes the complete field-bound response, validates its summary and well identities, and retains generation/disposal protection. Removed the unused current-field summary endpoint and its client method. The separate well-list endpoint remains because forecasting still consumes it; it is not a fallback for this dashboard.

## Scope and remaining acceptance

Both repository reads use the same explicit field, but run sequentially without a database snapshot transaction. This solves field binding, not concurrent-write snapshot consistency. Other pages still using current-field endpoints and cross-session selection behavior require separate work. Existing unit/period limitations remain; rate aggregates are not restored.

Live browser/OIDC and real-provider acceptance remain open. Verify switching fields during loading, denied field access and recovery after either repository read fails. Current tests exercise controller orchestration, the actual endpoint guard with mocked app access, typed-client encoding and state behavior; they do not constitute a live authentication/provider integration test.

## Verification

160 website tests passed; 959 API tests passed, 10 skipped, zero failed (969 API total). Five new API cases cover explicit-field reads, mismatched summary, failed wells, and allow/deny decisions through the attached field guard. Updated dashboard state regressions and added a typed-client field-encoding/null-response case. Builds compile the moved contracts and affected consumers. Whitespace check passed; source search confirms one definition of each production dashboard contract. Existing analyzer/dependency warnings and LocalDB skips remain.

Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/field-bound-production-dashboard.trx; Beep.OilandGas.ApiService.Tests/TestResults/field-bound-production-dashboard.trx; %TEMP%/beep-field-bound-web-tests.log and %TEMP%/beep-field-bound-api-tests.log.
