# Beep.OilandGas.LifeCycle

## Snapshot

- Category: Operations
- Scan depth: Heavy
- Current role: cross-domain orchestration, mapping, work-order, and lifecycle hub
- Maturity signal: broad orchestration project with many service areas

## Observed Structure

- Top-level folders: `Core`, `Models`, `Services`, plus existing implementation and migration notes
- Services are broad and include `AccessControl`, `Accounting`, `Calculations`, `DataMapping`, `Development`, `Exploration`, `FieldLifecycle`, `Permits`, `Processes`, `Production`, `WellManagement`, and `WorkOrder`
- This project appears to mediate across many domain slices rather than represent just one business domain

## Representative Evidence

- Work-order ownership: `Services/WorkOrder/WorkOrderManagementService.cs`
- Service breadth: `Services/DataMapping/`, `Services/Processes/`, `Services/Production/`, `Services/Exploration/`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/LifeCycle/WorkOrderController.cs`
- Web dependency evidence: `Beep.OilandGas.Web/Services/LifeCycleService.cs`, `Pages/PPDM39/WorkOrder/WorkOrderDashboard.razor`

## Planning Notes

- This is the most likely canonical orchestration owner for cross-module state transitions.
- The work-order duality between lifecycle and standalone work-order APIs must be resolved around this project.
