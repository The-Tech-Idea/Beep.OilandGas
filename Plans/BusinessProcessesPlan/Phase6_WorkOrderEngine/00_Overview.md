# Phase 6 — Work Order Engine Overview
## Scheduling, Contractor Management, Cost Capture, and Inspection

> **Status**: Not started  
> **Depends on**: Phase 2 (WORK_ORDER state machine), Phase 3 (API), Phase 4 (UI)  
> **Blocks**: Phase 10 (hardening requires WO records for compliance evidence)  
> **Owner**: Backend Dev + Domain SME

---

## Document Index

| # | Document | Purpose |
|---|---|---|
| This file | `00_Overview.md` | Goals, key files, milestones |
| [01](01_WorkOrderTypes.md) | `01_WorkOrderTypes.md` | 6 WO sub-types with jurisdiction variations |
| [02](02_SchedulingEngine.md) | `02_SchedulingEngine.md` | `ISchedulingService` and calendar-based scheduling |
| [03](03_ContractorManagement.md) | `03_ContractorManagement.md` | Contractor assignment, BA validation |
| [04](04_CostCapture.md) | `04_CostCapture.md` | AFE / cost code integration |
| [05](05_InspectionFramework.md) | `05_InspectionFramework.md` | Inspection items and BSEE SEMS §250.1920–.1932 |
| [06](06_SprintPlan_RACI.md) | `06_SprintPlan_RACI.md` | Sprint stories, RACI, risks, Definition of Done |

---

## Goals

1. Six WO sub-types: Preventive, Corrective, Safety, Environmental, Regulatory Inspection, Turnaround
2. Scheduling: calendar-based start/end with conflict detection across equipment
3. Contractor management: assign, qualify, validate license expiry
4. Cost capture: AFE variance tracking; cost code line items
5. Inspection framework: checklist items per WO type; BSEE SEMS compliance
6. All WO data stored in `PROJECT` + related PPDM tables

---

## Key Files

| File Path | Action |
|---|---|
| `Beep.OilandGas.Models/Core/Interfaces/IWorkOrderService.cs` | CREATE |
| `Beep.OilandGas.Models/Core/Interfaces/ISchedulingService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/SchedulingService.cs` | CREATE |
| `Beep.OilandGas.ApiService/Controllers/WorkOrder/WorkOrderController.cs` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/WorkOrder/WorkOrderDashboard.razor` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/WorkOrder/WorkOrderDetail.razor` | CREATE |

---

## Milestones

| ID | Target | Acceptance Criteria |
|---|---|---|
| M6-1 | Sprint 6.1 | All 6 WO types can be started; process definitions seeded |
| M6-2 | Sprint 6.2 | Scheduling calendar shows planned WOs; conflicts detected |
| M6-3 | Sprint 6.3 | Contractor assignment validates license; cost code lines saved |
| M6-4 | Sprint 6.4 | Inspection checklist items saved per WO; BSEE field tags present |

---

## PPDM Tables Used

| Table | Purpose |
|---|---|
| `PROJECT` | WO header |
| `PROJECT_PLAN` | Scheduled start/end dates |
| `PROJECT_STEP` | WO work steps / inspection items |
| `PROJECT_STEP_BA` | Contractor assignments per step |
| `PROJECT_STEP_TIME` | Actual time records per step |
| `PROJECT_STEP_CONDITION` | Inspection checklist items and results |
| `FINANCE` | AFE budget record |
| `FIN_COMPONENT` | Cost code line items |
| `BUSINESS_ASSOCIATE` | Contractor entity |
| `BA_LICENSE` | Contractor qualifications |
