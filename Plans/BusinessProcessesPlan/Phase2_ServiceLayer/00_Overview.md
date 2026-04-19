# Phase 2 — Service Layer Enhancements: Overview
## Business Process Branch — Implementation Plan

> **Status**: 🔲 Not Started  
> **Depends on**: Phase 1 (Complete ✅)  
> **Blocks**: Phase 3 (API Layer)  
> **Owner**: Backend / Data Layer Team  
> **Estimated Total Effort**: 4 sprints × 2 weeks = 8 weeks  

---

## Document Index

| # | Document | Contents |
|---|---|---|
| 00 | **Overview** *(this file)* | Goals, milestones, dependencies, file map |
| 01 | [State Machines](01_StateMachines.md) | All state machine definitions, transition tables, guard logic, ASCII diagrams |
| 02 | [PPDM Table Mapping](02_PPDMTableMapping.md) | Column-level PPDM39 mapping for all 12 process categories |
| 03 | [Process Definition Catalog](03_ProcessDefinitionCatalog.md) | All 96 process definitions with full step inventory |
| 04 | [Data Model Extensions](04_DataModelExtensions.md) | New C# properties on `ProcessDefinition`, `ProcessStepDefinition`, `ProcessInstance` |
| 05 | [Code Templates](05_CodeTemplates.md) | Ready-to-use C# templates for seeding, state machines, repositories |
| 06 | [Test Plan](06_TestPlan.md) | Unit + integration test specifications with acceptance criteria |
| 07 | [Sprint Plan & RACI](07_SprintPlan_RACI.md) | 4-sprint breakdown, story points, RACI matrix, risk register |

---

## Goals

1. Extend `ProcessDefinitionInitializer` to cover all **12 business process categories** (currently only 4 of 12 are seeded).
2. Implement **8 new state machines** (or extend existing ones) to cover Work Orders, Gate Reviews, HSE, Compliance, Well Lifecycle, Facility Lifecycle, Reservoir Management, and Pipeline workflows.
3. Add PPDM39 table binding metadata to every `ProcessStepDefinition` so the UI and API can show which database tables are affected per step.
4. Add jurisdiction tagging (`USA`, `CANADA`, `INTERNATIONAL`) and regulatory reference citations to all process and step definitions.
5. Introduce `GetProcessDefinitionByNameAsync` so `BusinessProcessNode` (Phase 1) can resolve process definitions by the workflow names registered in `BusinessProcessCategoryNode`.

---

## Out of Scope for Phase 2

- **API endpoints** (Phase 3) — service methods are called directly from tests during this phase
- **UI pages** (Phase 4)
- **Analytics queries** (Phase 5)
- Changes to PPDM schema (no DDL allowed — use existing tables only)

---

## Key Files

| File | Action | Notes |
|---|---|---|
| `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.cs` | EXTEND | Add 8 new `Initialize*Async` methods |
| `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.cs` | EXTEND | Add `_transitionRegistry`, `_guardRegistry`, new SM registrations |
| `Beep.OilandGas.LifeCycle/Services/Processes/ProcessStateMachine.cs` | EXTEND | Refactor to use registry pattern (see ADR-2-02) |
| `Beep.OilandGas.Models/Data/Process/ProcessDefinition.cs` | MODIFY | New fields: `JurisdictionTags`, `ProcessCategory`, `RequiresApproval`, `MinApproverCount` |
| `Beep.OilandGas.Models/Data/Process/ProcessStepDefinition.cs` | MODIFY | New fields: `PrimaryPPDMTable`, `SecondaryPPDMTables`, `RegulatoryReference`, `IsDocumentRequired` |
| `Beep.OilandGas.Models/Data/Process/ProcessInstance.cs` | MODIFY | New fields: `JurisdictionTag`, `DueDate`, `ApproverUserIds`, `IsOverdue` |
| `Beep.OilandGas.Models/Core/Interfaces/IProcessService.cs` | MODIFY | Add `GetProcessDefinitionByNameAsync` |
| `Beep.OilandGas.LifeCycle/Services/Processes/Exceptions/ProcessGuardException.cs` | CREATE | New exception class for guard failures |

---

## Milestones

| Milestone | Target | Criteria |
|---|---|---|
| M2-1: Model extensions merged | End of Sprint 1, Week 1 | `ProcessDefinition`, `ProcessStepDefinition`, `ProcessInstance` compile with new fields; break-nothing build |
| M2-2: Work Order + Gate Review SMs operational | End of Sprint 2 | State machines registered; unit tests green for both |
| M2-3: HSE + Compliance SMs operational | End of Sprint 3 | Guard conditions pass SME review; unit tests green |
| M2-4: All 12 categories seeded + PPDM binding complete | End of Sprint 4 | `GetProcessDefinitionByNameAsync` returns correct result for all 96 names; integration test suite green |

---

## Dependencies

```
Phase 1 (Branch Infrastructure) ──────────────────────────────────────► [COMPLETE]
                                                                              │
                                                        ┌─────────────────────┘
                                                        ▼
                    Phase 2: Service Layer ──────────────────────────────────────────►
                    ├─ Sprint 1: Model Extensions + Work Order SMs
                    ├─ Sprint 2: Gate Review + HSE SMs
                    ├─ Sprint 3: Compliance + 4 Lifecycle SMs
                    └─ Sprint 4: PPDM Bindings + Jurisdiction Tags + Integration Tests
                                                        │
                                        ┌───────────────┘
                                        ▼
                        Phase 3: API Layer (blocked until M2-4)
```

---

## Standards Traceability Matrix

| Category | Standard | Where It Applies in Phase 2 |
|---|---|---|
| Exploration | SPE-PRMS 2018 §2 | Lead/Prospect state machine; `POOL.POOL_STATUS` values |
| Development | ISO 17779 / IPA FEL | FDP project step sequencing; Gate Review definitions |
| Production | API RP 19D; BSEE SEMS | Well start-up SM guards; emergency shutdown trigger |
| Decommissioning | API RP 100-2; OSPAR | P&A SM; `OBLIGATION` write on closure |
| Work Orders | ISO 55001:2018 | WO state machine design; `EQUIPMENT_MAINTAIN` binding |
| Work Orders | ISO 14224:2016 | Equipment failure taxonomy in WO Corrective type |
| Gate Reviews | SPE Stage-Gate; IPA FEL | Gate 0–5 definitions; approver count guards |
| HSE | API RP 754 | Tier 1–4 incident classification in HSE SM |
| HSE | IOGP 2022e | Incident closure criteria; TRIR/LTIR step tags |
| HSE | IEC 61882 | HAZOP step document requirement |
| Compliance | BSEE 30 CFR 250 | USA permit workflow; `BA_PERMIT` binding |
| Compliance | AER Directive 056 | Canada well permit step; `BA_LICENSE` binding |
| Compliance | EPA 40 CFR 98 Subpart W | GHG compliance SM trigger |
| Compliance | ONRR; NEB/CER | Royalty reporting step; `OBLIG_PAYMENT` binding |
| Well Lifecycle | API RP 100-1; ISO 16530-2 | Well design → spud → drill → complete steps |
| Facility Lifecycle | IEC 61511; IOGP RP 70 | PSSR step guard; commissioning state |
| Reservoir Mgmt | SPE PRMS §5 | Reserves certification SM; pool reclassification |
| Pipeline | ASME B31.8; CSA Z662 | Integrity SM; pressure test document guard |
