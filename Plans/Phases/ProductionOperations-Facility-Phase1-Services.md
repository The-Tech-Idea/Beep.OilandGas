# Phase 1 — Core services (facility + production operations)

## Objective

Align `Beep.OilandGas.ProductionOperations` with PPDM39 facility management practices: one authoritative facility service, thin orchestration in production operations, query surface on production management.

## TODO checklist

- [x] `IFacilityManagementService` + partial `FacilityManagementService` (PPDM entities, no duplicate DTO layer).
- [x] `ProductionOperationsService` injects `IFacilityManagementService`; facility production + status delegate to it.
- [x] ApiService DI: register `IFacilityManagementService` before `IProductionOperationsService`; pass into ctor.
- [x] `IProductionManagementService`: `CancellationToken` on all methods; `ListFacilityPdenDeclarationsAsync`.
- [x] `ProductionManagementService` partial + `ProductionManagementService.PdenFacilityQueries.cs`.
- [x] Plans: `FacilityManagementPlan.md` service section; `Plans/ProductionOperations_Service_Model.md`.
- [x] `dotnet build` on `Beep.OilandGas.ProductionOperations` — succeeds.
- [ ] `dotnet build` on `Beep.OilandGas.ApiService` — blocked by pre-existing `Beep.OilandGas.PermitsAndApplications` compile errors (unchanged by this work).

## Target files

- `Beep.OilandGas.ProductionOperations/Services/*`
- `Beep.OilandGas.ApiService/Program.cs`
- `Plans/FacilityManagementPlan.md`, `Plans/ProductionOperations_Service_Model.md`

## Verification

- `Beep.OilandGas.ProductionOperations` builds with zero errors.
- `ProductionOperationsService` constructor resolves `IFacilityManagementService` from DI in ApiService.
