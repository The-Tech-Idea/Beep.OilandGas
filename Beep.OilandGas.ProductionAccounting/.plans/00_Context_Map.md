# ProductionAccounting Context Map

## Project Scope

`Beep.OilandGas.ProductionAccounting` provides production-accounting orchestration and accounting-domain compatibility surfaces on top of PPDM entities and Beep accounting services.

## Core Anchors

- `Services/ProductionAccountingService.cs`
  - Main orchestrator and aggregate status calculator.
- `Services/ProductionAccountingService.ControllerFacade.cs`
  - Compatibility manager surface (traditional accounting, production manager, pricing, ownership, royalty, reporting, storage, lease, trading, costing flavors).
- `Services/ProductionAccountingService.Compatibility.cs`
  - Shared compatibility helpers (`GetRepository`, static calculators/converters).
- `Modules/ProductionAccountingModuleSetup.cs`
  - Module entity registration and seed entrypoint.

## Interface Anchors

- `Beep.OilandGas.Models.Core.Interfaces/IProductionAccountingService`
  - Current minimal orchestrator contract (`ProcessProductionCycleAsync`, `GetAccountingStatusAsync`, `ClosePeriodAsync`).
- `Core/Interfaces/*`
  - Broad accounting sub-domain interface set (allocation, royalty, inventory, pricing, tax, reporting, etc.).

## API Touchpoints

- `Beep.OilandGas.ApiService/Controllers/Field/AccountingController.cs`
  - Uses `ProductionAccountingService` directly for:
    - `GetRevenueTransactionsAsync`
    - `GetAccountingStatusAsync`
    - `ClosePeriodAsync`

## Initial Observations

- The service layer is feature-rich, but maturity is uneven between orchestration methods and compatibility wrappers.
- Module setup currently has broad entity declaration but no reference-data seed sets.
- Planning focus should prioritize canonical persistence patterns, reference-data governance, and API/interface alignment.

