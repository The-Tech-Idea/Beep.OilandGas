# Beep.OilandGas.Models

## Snapshot

- Category: Core/shared contracts
- Scan depth: Heavy
- Current role: main shared contract and DTO/model surface for the solution
- Maturity signal: broad and cross-cutting, with many domain areas represented under `Data`

## Observed Structure

- Top-level folders: `Core`, `Data`, `Enums`, `Graphics`, `HeatMap`, `Reservoir`, `Schematics`, `Scripts`, `Survey`, `WellData`
- `Core` contains `Interfaces`
- `Data` contains many domain folders including `AccessControl`, `Accounting`, `Calculations`, `Compliance`, `Development`, `Drilling`, `EconomicAnalysis`, `EnhancedRecovery`, `FieldOrchestrator`, `FlashCalculations`, `GasLift`, `GasProperties`, `HSE`, `LeaseManagement`, `LifeCycle`, `Operations`, `Production`, `ProductionAccounting`, `ProductionOperations`, `ProspectIdentification`, `PumpPerformance`, `WorkOrder`, and many more
- `WellData` contains multiple well-shape models such as `WellData.cs`, `WellData_Borehole.cs`, and `WellData_Tubing.cs`

## Representative Evidence

- Interface surface: `Core/Interfaces/IWorkOrderService.cs`, `Core/Interfaces/IProductionOperationsService.cs`, `Core/Interfaces/IEconomicAnalysisService.cs`, `Core/Interfaces/IGasLiftService.cs`, `Core/Interfaces/IDecommissioningService.cs`
- Data breadth: `Data/ProductionOperations/`, `Data/ProspectIdentification/`, `Data/Accounting/`, `Data/Compliance/`, `Data/WorkOrder/`
- Base entity: `Data/ModelEntityBase.cs`

## Planning Notes

- This project is the first place to look when phases 8 and 9 need new handoff contracts or typed request/response models.
- Because the data surface is already broad, new phase plans should prefer extending existing contract areas over introducing parallel model locations.
- The phase plans should assume this project is a dependency risk area when changing domain boundaries or typed-client contracts.
