# Repository Scan Summary

> Purpose: summarize the broad repository scan used to correct and ground the phase 6-10 planning set.  
> Scope: representative file-level scan across web pages/components/services, API controllers, and domain/support project structures in the Beep.OilandGas solution.

---

## What Was Scanned

- routed Razor files under `Beep.OilandGas.Web/Pages/**`
- routed Razor files under `Beep.OilandGas.Web/Components/**`
- web services under `Beep.OilandGas.Web/Services/**`
- app shell, router, layout, and shared component areas in the web project
- controller families under `Beep.OilandGas.ApiService/Controllers/**`
- representative service/model/DTO/repository folders in operational, support, finance, and engineering projects across the solution
- the solution project list in `Beep.OilandGas.sln`

---

## High-Value Findings

### Web structure

- routed files exist in both `Pages/` and `Components/`
- duplicate page areas are real, especially `Pages/Data/*` vs `Pages/PPDM39/Data/*`
- calculation pages exist for only part of the engineering stack
- work-order pages exist as a dedicated page family, while lifecycle also owns work-order logic

### API structure

- API controller coverage is broad across field, PPDM39, accounting, calculations, operations, properties, pumps, business process, HSE, compliance, and work order domains
- work-order API ownership is duplicated between `Controllers/WorkOrder/WorkOrderController.cs` and `Controllers/LifeCycle/WorkOrderController.cs`
- permit/compliance logic is not surfaced as a first-class permit controller family despite the depth of the permits project

### Domain maturity asymmetry

- materially present slices: `ProspectIdentification`, `LeaseAcquisition`, `Decommissioning`, `Accounting`, `ProductionAccounting`, `PPDM39.DataManagement`, `LifeCycle`
- partial or thin slices: `DevelopmentPlanning`, `ProductionOperations`, `EnhancedRecovery`, `DrillingAndConstruction`
- support/infrastructure projects: `DataManager`, `UserManagement`, `Branchs`, `Drawing`
- validation-heavy support domain: `PermitsAndApplications`

### Surfacing asymmetry

- finance projects are service heavy but web thin
- several engineering projects exist in the solution without first-class UI or API surfacing
- calculation client coverage is not obviously aligned with the full set of engineering modules

---

## Planning Consequences

1. Phase 8 must include service maturity work, not just integration cleanup.
2. Phase 8 and 9 must explicitly address work-order ownership and permit surfacing gaps.
3. Phase 9 must treat finance/compliance surfacing as a build-out task, not only rationalization.
4. Phase 10 must validate both duplication cleanup and incomplete domain maturity so those concerns are not conflated.
5. Project matrices for phases 8, 9, and 10 must distinguish `mature`, `partial`, `thin`, and `support-only` roles where relevant.
