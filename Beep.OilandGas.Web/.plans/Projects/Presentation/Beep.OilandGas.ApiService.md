# Beep.OilandGas.ApiService

## Snapshot

- Category: Presentation/API
- Scan depth: Heavy
- Current role: primary HTTP boundary for the solution
- Maturity signal: broad controller surface with very large DI graph

## Observed Structure

- Top-level folders: `Controllers`, `Services`, `Middleware`, `Config`, `Exceptions`, `Attributes`, `Properties`
- Controller families include `Field`, `PPDM39`, `Accounting`, `Calculations`, `Operations`, `Production`, `Properties`, `Pumps`, `BusinessProcess`, `HSE`, `Compliance`, `LifeCycle`, `WorkOrder`, and `AccessControl`
- Services folder is small compared with the controller and DI surface, which indicates the project is primarily an orchestration layer over referenced domain services
- `Program.cs` contains extensive service registration across PPDM, lifecycle, accounting, calculation, operations, compliance, and work-order domains

## Representative Evidence

- Startup and DI: `Program.cs`
- Generic boundary controllers: `Controllers/CalculationsController.cs`, `Controllers/DataManagementController.cs`, `Controllers/ConnectionController.cs`
- Operational controller families: `Controllers/Field/`, `Controllers/Operations/`, `Controllers/Production/`
- PPDM controller families: `Controllers/PPDM39/`
- Work-order split: `Controllers/WorkOrder/WorkOrderController.cs`, `Controllers/LifeCycle/WorkOrderController.cs`
- Minimal local services: `Services/PPDM39GenericDataService.cs`, `Services/ProgressTrackingService.cs`

## Planning Notes

- This project is the primary owner for controller-family consolidation and route normalization in phases 8-10.
- The work-order controller split must be resolved before workflow handoff planning is considered stable.
- Permit/compliance surfacing is weaker here than the underlying permits project suggests, so phase 9 should treat that as an API build-out task.
- DI registration breadth means phase 10 validation needs to distinguish missing domain maturity from actual startup regressions.
