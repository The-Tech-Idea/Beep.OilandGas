# ProductionOperations Context Map

## Scope

`Beep.OilandGas.ProductionOperations` owns production-operations application flows with heavy dependency on PPDM39 tables and repository infrastructure.

## Current Structure

- `Services/IProductionOperationsService.cs`
  - Broad, DTO-oriented facade.
  - Includes well production, maintenance, facility operations, cost, optimization, and reporting.
- `Services/IProductionManagementService.cs`
  - PPDM-focused operations centered on `PDEN` and `FACILITY`.
- `Services/IFacilityManagementService.cs`
  - Lifecycle operations directly on `FACILITY` and related PPDM entities.
- `Services/ProductionOperationsService*.cs`
  - Partial class; some methods implemented with PPDM repositories, several methods still stubs or placeholder outputs.
- `Services/ProductionManagementService*.cs`
  - UnitOfWork-centric implementation for `PDEN`/`FACILITY`.
- `Modules/FacilityManagementModuleSetup.cs`
  - Module metadata and idempotent `CAT_EQUIPMENT` seeding.

## Key Risks

- Overlapping service interfaces can cause drift in API usage and implementation ownership.
- Mixed data-access patterns increase maintenance cost and behavior inconsistency.
- Stub methods can appear production-ready but return placeholders.
- Facility reference-data seeding is narrower than workflow requirements.

## Desired Direction

- Keep one canonical service boundary per workflow capability.
- Make repository/data-access strategy explicit per service.
- Expand module seeding for required facility reference tables.
- Ensure API-facing contracts map to implemented behavior only.

