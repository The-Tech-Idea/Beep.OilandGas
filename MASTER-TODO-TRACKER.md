# MASTER-TODO-TRACKER — Oil & Gas facility + production operations

Single tracker for facility management and ProductionOperations service alignment. Each phase has a dedicated doc under `Plans/Phases/`.

## Phase index

| Phase | Doc | Goal |
|-------|-----|------|
| 1 | [Plans/Phases/ProductionOperations-Facility-Phase1-Services.md](Plans/Phases/ProductionOperations-Facility-Phase1-Services.md) | PPDM-native facility service, delegation from production operations, production management queries + cancellation |
| 2 | [Plans/Phases/ProductionOperations-Facility-Phase2-API-Verification.md](Plans/Phases/ProductionOperations-Facility-Phase2-API-Verification.md) | Facility API controllers, full solution build, integration tests |

## Current status (summary)

- Phase 1: **Mostly complete** — services, DI order, docs, and `Beep.OilandGas.ProductionOperations` build are done.
- Phase 2: **Controllers done** — facility HTTP surface is in `ApiService/Controllers/Facility/`. **Full ApiService build** still blocked on `Beep.OilandGas.PermitsAndApplications` compile errors; integration tests pending after Permits is green.
