# Phase 2 — API surface + verification

## Objective

Expose facility operations through dedicated controllers and prove end-to-end behavior under CI.

## TODO checklist

- [x] Add `Beep.OilandGas.ApiService/Controllers/Facility/*` (six controller areas + `FacilityUserHelper`).
- [x] Wire controllers to `IFacilityManagementService`; `[Authorize]` on all; query/body validation on required keys.
- [x] Pass `CancellationToken` into service calls (implicit request token on actions); legacy `CreateOperationCompatibility` passes `HttpContext.RequestAborted` into `CreateProductionOperationAsync`.
- [ ] Integration or smoke tests for facility PDEN + volume round-trip.
- [ ] Resolve `Beep.OilandGas.PermitsAndApplications` build failures **or** exclude from solution temporarily so full solution build is green.

## Verification

- Full solution `dotnet build` succeeds.
- Swagger shows new facility routes (when implemented).
