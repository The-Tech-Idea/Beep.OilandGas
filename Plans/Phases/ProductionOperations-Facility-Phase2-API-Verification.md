# Phase 2 — API surface + verification

2026-09-21 review: cross-project verification is tracked by [FG-22 in the root tracker](../../MASTER-TODO-TRACKER.md). Mocked facility monitoring controller tests exist, but database/HTTP round-trip evidence remains pending. The current API build probe fails at `LifeCycle/Services/WellComparisonService.cs:475` with CS1519; the earlier Permits diagnosis below has been replaced by the current observed blocker.

## Objective

Expose facility operations through dedicated controllers and prove end-to-end behavior under CI.

## TODO checklist

- [x] Add `Beep.OilandGas.ApiService/Controllers/Facility/*` (six controller areas + `FacilityUserHelper`).
- [x] Wire controllers to `IFacilityManagementService`; `[Authorize]` on all; query/body validation on required keys.
- [x] Pass `CancellationToken` into service calls (implicit request token on actions); legacy `CreateOperationCompatibility` passes `HttpContext.RequestAborted` into `CreateProductionOperationAsync`.
- [ ] Integration or smoke tests for facility PDEN + volume round-trip.
- [ ] Resolve the current LifeCycle CS1519 failure, restore/rebuild, and address any subsequent failures across the intended solution. Do not exclude feature projects to claim a green full-solution build.

## Verification

- Full solution `dotnet build` succeeds.
- Swagger shows new facility routes (when implemented).
