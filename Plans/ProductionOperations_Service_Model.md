# Production operations — service model (facility-aware)

## Roles

| Service | Responsibility |
|--------|----------------|
| **`IFacilityManagementService`** | PPDM39 facility lifecycle: `FACILITY`, `FACILITY_STATUS`, `FACILITY_LICENSE`, components, equipment links, maintenance, work orders, **`PDEN` / `PDEN_FACILITY`**, **`PDEN_VOL_SUMMARY`**, reliability metrics. Single place for facility PDEN identity (`EnsureFacilityPdenAsync`) and license-aware operational status. |
| **`IProductionOperationsService`** | Broad production operations (wells, costs, optimization, **legacy/summary DTOs** for facility production/status). Facility **volume and status persistence** is delegated to `IFacilityManagementService` so PDEN keys and `FACILITY_STATUS` rows stay consistent. |
| **`IProductionManagementService`** | PDEN- and facility-oriented **read/query** helpers via UoW (operations list, well PDENs, facility master row, **`ListFacilityPdenDeclarationsAsync`** for `PDEN_SUBTYPE = FACILITY`). Accepts **`CancellationToken`** on all methods for API/host cancellation. |

## Best practices (oil & gas facility management)

1. **Facility production is declared under `PDEN` with subtype `FACILITY`**, linked to `FACILITY` through `PDEN_FACILITY` — do not use raw `FACILITY_ID` as `PDEN_ID` without ensuring linkage (use `EnsureFacilityPdenAsync`).
2. **Operational status** belongs in **`FACILITY_STATUS`**, not ad hoc columns on `FACILITY`; enforce **active license** rules when marking active/operational states.
3. **Separation**: facility tables and PDEN volume writes live in **`IFacilityManagementService`**; **`ProductionOperationsService`** keeps thin adapters where it exposes `FacilityProduction` / `FacilityStatus` DTOs for existing callers.
4. **Cancellation**: long-running or list endpoints should pass **`HttpContext.RequestAborted`** into `IProductionManagementService` methods.

## Dependency order (DI)

Register **`IFacilityManagementService`** before **`IProductionOperationsService`** so the production operations implementation can take `IFacilityManagementService` in its constructor.

## HTTP API (ApiService)

Facility controllers live under `Beep.OilandGas.ApiService/Controllers/Facility/` and call **`IFacilityManagementService`** (and **`IProductionManagementService`** only for `GET api/facility/pden/facility-subtype`). See [FacilityManagementPlan.md](FacilityManagementPlan.md) § API Controllers.

## Related documents

- [FacilityManagementPlan.md](FacilityManagementPlan.md) — tables, lifecycle phases, API controller backlog.
