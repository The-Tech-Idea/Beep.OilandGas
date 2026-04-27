# Phase 1 Canonical Type/File Map

## Scope

Verified all `Services/*.cs` files under `Beep.OilandGas.ProductionOperations`.

## Canonical Type Ownership

- `IFacilityManagementService` -> `Services/IFacilityManagementService.cs`
- `IProductionManagementService` -> `Services/IProductionManagementService.cs`
- `IProductionOperationsService` (local expanded surface) -> `Services/IProductionOperationsService.cs`
- `ProductionOperationsService` -> `Services/ProductionOperationsService.cs` + `Services/ProductionOperationsService.ModelsCoreImpl.cs` (partial class split, single type)
- `ProductionManagementService` -> `Services/ProductionManagementService.cs` + `Services/ProductionManagementService.PdenFacilityQueries.cs` (partial class split, single type)
- `FacilityManagementService` -> `Services/FacilityManagementService.cs` + `Services/FacilityManagementService.Operations.cs` (partial class split, single type)

## Findings

- No duplicate physical type files were found in `Services`.
- Duplicate search results observed previously were path-separator variants from tooling output (`/` vs `\`) for the same files, not duplicate files on disk.

## Phase 1 Decision

- Keep current partial-class splits (they are clean and responsibility-oriented).
- No file deletions are required for duplicate cleanup in this phase.

