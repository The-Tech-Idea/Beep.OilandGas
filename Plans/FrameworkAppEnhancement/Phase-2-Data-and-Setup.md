# Phase 2: PPDM data, setup and facility persistence

Dependencies: FG-00/01 and API boundary for client-facing setup. Responsible areas: PPDM39.DataManagement, DataManager, domain module registrations, ApiService and external admin client. Findings: R01, R11. Recovery work below is a proposed verification/hardening scope, not an assertion that every mechanism is absent.

| Task | Deliverable | Acceptance |
|---|---|---|
| FG-20 | Audit module discovery, dependency order, table ownership and reference-code seeds using existing `ModuleSetupOrchestrator`. | Each table/seed family is registered once by its owner. Fresh install and repeated setup converge to the same valid state. Cancellation produces a truthful outcome. |
| FG-21 | Add setup preflight, durable progress/recovery where needed, provider matrix and data-quality reports to the existing wizard/API pipeline. | An injected failure mid-install can be diagnosed and resumed or rolled back without duplicate reference rows; unsupported providers fail preflight. Required credentials never return to browser logs. |
| FG-22 | Complete facility/production HTTP-to-repository integration coverage using existing service/controller implementations. | Create facility, associate production entity, write/read volume or measurement, update and cancel across actual persistence; unauthorized field access is rejected; read-back survives a service restart. |

## Implementation sequence

Inventory actual supported providers from registration, scripts and test fixtures. Select one primary provider for the first gate; do not claim every provider is supported because a driver package exists. For each additional supported provider, run schema creation, seed idempotency, transactions, parameterized filtering and precision/date round trips.

Use metadata to validate required identifiers, references, units and effective dates before writes. Return row-level import diagnostics with accepted/rejected counts and provenance. Add explicit preview/validation before bulk changes in the existing management flow. Map resulting admin operations to BeepDiA under the canonical boundary decision.

Use disposable test databases with deterministic seed data. Never run setup/recovery experiments on production data. Verify existing databases as well as fresh installation; generated PPDM model files remain generated artifacts, while approved extensions follow the FG-01 owner decision.

## Exit evidence

Record schema version, provider/version, migration/seed identifiers, fixture data and test command. Show repeated setup, interrupted setup recovery, failed validation with no partial business write, and facility read-back through authenticated HTTP. Extend existing `ModuleSetupOrchestratorTests` and `FacilityMonitoringControllerTests` rather than replacing their fast unit coverage with database-only tests.
