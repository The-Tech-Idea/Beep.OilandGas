# Beep.OilandGas.DrillingAndConstruction

## Snapshot

- Category: Operations
- Scan depth: Medium
- Current role: drilling and construction support slice
- Maturity signal: partial slice with service and calculation support but thin planning surfacing

## Observed Structure

- Top-level folders: `Calculations`, `Services`
- Calculation layer includes `HydraulicsCalculator` and `TorqueDragCalculator`
- Service layer includes `DrillingOperationService` and `DrillingEngineeringService`

## Representative Evidence

- Services: `Services/DrillingOperationService.cs`, `Services/DrillingEngineeringService.cs`
- Calculations: `Calculations/HydraulicsCalculator.cs`, `Calculations/TorqueDragCalculator.cs`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Operations/DrillingOperationController.cs`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Operations/DrillingOperations.razor`

## Planning Notes

- This project has enough structure to support the development slice, but not enough current surfacing to be treated as mature.
- Phase 8 should plan explicit handoffs from development planning and permits into this slice.
