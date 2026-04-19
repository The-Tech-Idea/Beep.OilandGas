# Phase 6 — Sprint Plan & RACI
## Work Order Engine Delivery Schedule

> Total story points: ~78 SP  
> Target: 4 sprints (~8 weeks)

---

## Sprint Breakdown

### Sprint 6.1 — WO Types & Service Scaffold (Weeks 1–2)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| Seed all 6 WO process definitions (`WO-PREVENTIVE` through `WO-TURNAROUND`) | 3 | BD | Definitions in DB on startup |
| `IWorkOrderService` interface + `WorkOrderService` scaffold | 3 | BD | Service injectable |
| `ISchedulingService` + `SchedulingService.ScheduleWorkOrderAsync` | 5 | BD | WO scheduled; returns conflict result |
| `SchedulingService.GetConflictsAsync` | 3 | BD | Conflicts detected for overlap dates |
| Calendar API endpoint `GET /workorder/calendar` | 2 | BD | Returns `List<CalendarSlot>` |
| Basic `WorkOrderDashboard.razor` (skeleton + grid) | 3 | FE | Page loads without error |
| **Sprint total** | **19 SP** | | |

---

### Sprint 6.2 — Contractor & Cost (Weeks 3–4)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| `IContractorManagementService` + `ValidateContractorAsync` | 4 | BD | License validation rejects expired certs |
| `AssignContractorAsync` + `RemoveContractorAsync` | 3 | BD | `PROJECT_STEP_BA` rows written |
| `ICostCaptureService` + `UpsertAFEAsync` | 3 | BD | `FINANCE` row created |
| `AddCostLineAsync` + `UpdateActualCostAsync` | 3 | BD | `FIN_COMPONENT` rows written |
| `GetVarianceSummaryAsync` | 3 | BD | Variance % computed correctly |
| `CostVariancePanel.razor` component | 3 | FE | Variance table renders |
| Contractor assignment page section in `WorkOrderDetail.razor` | 3 | FE | Assign/remove contractor works |
| **Sprint total** | **22 SP** | | |

---

### Sprint 6.3 — Inspection Framework (Weeks 5–6)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| `IInspectionService` + `SeedChecklistAsync` | 3 | BD | 12 SEMS items seeded on WO-TURNAROUND |
| `RecordResultAsync` + `AllConditionsPassedAsync` | 4 | BD | Guard check works end-to-end |
| Inspection checklist component `InspectionChecklist.razor` | 4 | FE | Items render; pass button marks PASS |
| Guard wiring: SEMS close-out guard on COMPLETE trigger | 3 | BD | WO cannot complete without all PASS |
| `WorkOrderDetail.razor` — inspection tab | 3 | FE | Tab shows checklist items |
| Integration tests: schedule conflict (3), contractor (3), inspection guard (3) | 5 | QA | 9 tests pass |
| **Sprint total** | **22 SP** | | |

---

### Sprint 6.4 — Calendar UI & Polish (Weeks 7–8)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| `WorkOrderCalendar.razor` week-grid view | 4 | FE | Calendar shows 7-day events |
| Cost variance export (XLSX) | 3 | BD | Per-WO cost report downloads |
| Nav links for WO pages | 1 | FE | Sidebar updated |
| Smoke test checklist | 4 | QA | All 10 items green |
| **Sprint total** | **12 SP** | | |

---

## RACI Matrix

| Task | BD | FE | QA | DA | HS | PM |
|---|---|---|---|---|---|---|
| WO type process definitions | **R/A** | — | C | C | C | I |
| Scheduling logic | **R/A** | — | C | — | — | I |
| Contractor validation | **R** | — | C | — | **A** | I |
| Cost capture | **R/A** | — | C | C | — | I |
| Inspection checklist seed | **R** | — | C | — | **A** | I |
| SEMS guard implementation | **R** | — | C | — | **A** | **I** |
| UI components | — | **R/A** | C | — | — | I |
| Integration tests | R | — | **R/A** | — | C | I |

---

## Risk Register

| Risk ID | Description | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R6-01 | `BA_LICENSE` table not populated (no contractors loaded) | Medium | High | Seed 3 test contractors with valid licenses in Sprint 6.1 |
| R6-02 | Turnaround BSEE notification obligation not automated | Low | Medium | Track as manual obligation in OBLIGATION table (Phase 8) |
| R6-03 | 12 SEMS conditions hard to maintain if requirements change | Low | Low | Checklist seeded from configurable JSON; not hardcoded |

---

## Definition of Done (Phase 6)

- [ ] All 6 WO types start successfully from ProcessDashboard
- [ ] Schedule conflict detection returns 409 when equipment has overlap
- [ ] Contractor assignment rejects expired licenses
- [ ] AFE finance record created for WOs > $50,000
- [ ] All 12 SEMS conditions seeded on WO-TURNAROUND
- [ ] WO-TURNAROUND cannot complete without all conditions PASS
- [ ] Calendar view shows weekly planned WOs
- [ ] 9 integration tests pass
- [ ] `dotnet build Beep.OilandGas.sln` exits 0 errors
