# Phase 2 — Sprint Plan & RACI
## Service Layer Delivery Schedule

> Total story points: ~89 SP  
> Sprint velocity assumption: 25–30 SP per 2-week sprint  
> Target delivery: 3–4 sprints (~6–8 weeks)

---

## Sprint Breakdown

### Sprint 2.1 — Foundation (Weeks 1–2)

| Story ID | Story | Points | Owner | Deliverable |
|---|---|---|---|---|
| P2-01 | Define `ProcessDefinition`, `ProcessStepDefinition`, `ProcessInstance` model classes | 3 | Backend Dev | Models in `Beep.OilandGas.Models/Data/Process/` |
| P2-02 | Define `ProcessGuardException`, `ProcessTransitionRecord`, `ProcessSummaryDto` | 2 | Backend Dev | 3 new model classes |
| P2-03 | Create `IProcessService` interface with all public method signatures | 3 | Backend Dev | Interface in `Beep.OilandGas.Models.Core.Interfaces` |
| P2-04 | Scaffold `ProcessStateMachine` with `_transitionRegistry`, `_guardRegistry`, `_initialStateRegistry` | 5 | Backend Dev | Empty registries compilable |
| P2-05 | Implement `RegisterWorkOrderStateMachine()` with all transitions and guards | 5 | Backend Dev + Domain SME | SM-01 complete |
| P2-06 | Implement `RegisterGateReviewStateMachine()` with document and approver guards | 5 | Backend Dev + Domain SME | SM-02 complete |
| P2-07 | Implement `ApplyTransitionAsync` core method | 5 | Backend Dev | Transitions apply + audit written |
| **Sprint total** | | **28 SP** | | |

---

### Sprint 2.2 — State Machines & Definition Seeding (Weeks 3–4)

| Story ID | Story | Points | Owner | Deliverable |
|---|---|---|---|---|
| P2-08 | Implement `RegisterHSEIncidentStateMachine()` (SM-03) | 5 | Backend Dev + HSE SME | API RP 754 Tier 1–4 flows |
| P2-09 | Implement `RegisterComplianceReportStateMachine()` (SM-04) | 4 | Backend Dev + Compliance SME | ONRR/NEB SM complete |
| P2-10 | Implement SM-05 Well Lifecycle + SM-06 Facility Lifecycle (no guards needed for phase 2) | 5 | Backend Dev | Transitions only; guards in Phase 6 |
| P2-11 | Implement SM-07 Reservoir Mgmt + SM-08 Pipeline Integrity transitions | 4 | Backend Dev | Transitions ready; guards Phase 7 |
| P2-12 | Create `ProcessDefinitionInitializer` partial class scaffold | 3 | Backend Dev | Registers all 12 `Initialize*` calls |
| P2-13 | Implement `InitializeExplorationProcessesAsync` (8 definitions) | 5 | Backend Dev + Domain SME | Category 1 seeded |
| P2-14 | Implement `InitializeWorkOrderProcessesAsync` (6 definitions) | 4 | Backend Dev + Domain SME | Category 5 seeded |
| **Sprint total** | | **30 SP** | | |

---

### Sprint 2.3 — Remaining Definitions & Service Methods (Weeks 5–6)

| Story ID | Story | Points | Owner | Deliverable |
|---|---|---|---|---|
| P2-15 | Implement remaining 10 `Initialize*ProcessesAsync` methods (Categories 2–4, 6–12) | 10 | Backend Dev | All 96 definitions seeded |
| P2-16 | Implement `StartProcessAsync` with entity validation | 5 | Backend Dev | New instances created correctly |
| P2-17 | Implement `GetProcessSummariesForFieldAsync` with available transitions | 5 | Backend Dev | Field-filtered list with transitions |
| P2-18 | Implement `GetProcessDefinitionByNameAsync` | 2 | Backend Dev | Definition lookup works |
| P2-19 | Register `IProcessService` in `Program.cs` using factory pattern | 3 | Backend Dev | DI compiles; service injectable |
| **Sprint total** | | **25 SP** | | |

---

### Sprint 2.4 — Testing & Hardening (Weeks 7–8)

| Story ID | Story | Points | Owner | Deliverable |
|---|---|---|---|---|
| P2-20 | Implement `WorkOrderStateMachineTests` (12 test cases) | 5 | QA + Backend Dev | 12/12 pass green |
| P2-21 | Implement `GateReviewStateMachineTests` (8 test cases) | 4 | QA + Backend Dev | 8/8 pass green |
| P2-22 | Implement `HSEIncidentStateMachineTests` (8 test cases) | 4 | QA + Backend Dev | 8/8 pass green |
| P2-23 | Implement `ComplianceReport` + `PPDMProcessServiceTests` (9 test cases) | 4 | QA + Backend Dev | 9/9 pass green |
| P2-24 | Implement `ProcessDefinitionInitializerTests` (6 test cases) + idempotency | 3 | QA + Backend Dev | Idempotency proven |
| P2-25 | Code review and tech debt clearance | 3 | All | No P1 review comments open |
| **Sprint total** | | **23 SP** | | |

---

## RACI Matrix

### Roles

| Code | Role |
|---|---|
| **BD** | Backend Developer |
| **DA** | Data Architect (PPDM specialist) |
| **QA** | QA Engineer |
| **DS** | Domain SME (oil & gas subject matter expert) |
| **CS** | Compliance SME (regulatory specialist) |
| **HS** | HSE SME |
| **PM** | Project Manager |

### RACI

| Task | BD | DA | QA | DS | CS | HS | PM |
|---|---|---|---|---|---|---|---|
| Model class design (`ProcessDefinition`, `ProcessInstance`) | **R** | C | — | C | — | — | I |
| State machine transitions (Work Order, Gate Review) | **R** | — | C | **A** | — | — | I |
| State machine transitions (Compliance, GHG) | **R** | — | C | — | **A** | — | I |
| State machine transitions (HSE Incident) | **R** | — | C | — | — | **A** | I |
| PPDM table column mapping | C | **R/A** | — | C | C | C | I |
| `ProcessDefinitionInitializer` seeding logic | **R** | C | — | **A** | — | — | I |
| `ApplyTransitionAsync` implementation | **R/A** | — | C | — | — | — | I |
| Guard condition logic — regulatory references | R | — | — | A | A | A | I |
| Unit test authoring | **R** | — | **A** | C | C | C | I |
| `Program.cs` DI registration | **R/A** | — | — | — | — | — | I |
| Sprint planning & acceptance of Definition of Done | C | C | C | C | C | C | **R/A** |

---

## Risk Register

| Risk ID | Risk Description | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R2-01 | PPDM column mapping incomplete for a category — guard queries fail at runtime | Medium | High | DA reviews all 12 category pages in `02_PPDMTableMapping.md` before Sprint 2.2; add fallback logging if column not found |
| R2-02 | `ProcessType` mismatch between `ProcessDefinition` and `_transitionRegistry` key causes silent transition failures | Low | High | Unit test `TC-SM` suite covers every `ProcessType` used in definitions; add startup validation asserting all used types have registered SMs |
| R2-03 | Regulatory guard condition is too strict for a regional jurisdiction edge case | Medium | Medium | DS + CS review guard logic during code review in Sprint 2.4; jurisdiction override mechanism planned for Phase 8 |
| R2-04 | `InitializeAllProcessDefinitionsAsync` called before `IDMEEditor` connection established | Low | High | Registration order in `Program.cs` enforced by CLAUDE.md rule; startup test validates connection before seeding |
| R2-05 | 96 process definitions bloat startup time significantly | Low | Medium | Idempotency check (Template 2) short-circuits on first run after seeding; benchmark in Sprint 2.3 |

---

## Definition of Done (Phase 2)

- [ ] All 96 `ProcessDefinition` rows seedable idempotently in clean test DB
- [ ] All 8 state machines (`SM-01` through `SM-08`) have full transition tables registered in `_transitionRegistry`
- [ ] Every guard function has been reviewed and signed off by the relevant SME (DS, CS, or HS)
- [ ] `dotnet build Beep.OilandGas.sln` exits with 0 errors, 0 new warnings
- [ ] All 43 unit tests in `Beep.OilandGas.LifeCycle.Tests` pass
- [ ] `IProcessService` registered in `Program.cs` and injectable in integration test
- [ ] `02_PPDMTableMapping.md` has been cross-checked by Data Architect
- [ ] No `TODO` or `FIXME` comments remain in new files
- [ ] Commit message includes `[Phase2-Complete]` tag
