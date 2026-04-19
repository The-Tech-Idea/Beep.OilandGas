# Phase 7 — Sprint Plan and RACI
## HSE Module — 4 Sprints (~85 Story Points), RACI, Risk Register, Definition of Done

---

## Story Table

| ID | Story | Points | Owner | Deliverable |
|---|---|---|---|---|
| 7-01 | Create `IHSEService` + `ICorrectiveActionService` interfaces | 2 | BD | Interfaces in `Models/Core/Interfaces` |
| 7-02 | `HSEIncidentService`: CRUD + state machine transitions | 8 | BD | `HSEIncidentService.cs` passing unit tests |
| 7-03 | Auto-create OBLIGATION rows on UNDER_INVESTIGATION transition | 5 | BD | OPA 90/AER obligations auto-generated |
| 7-04 | `IRCAService`: cause chain CRUD | 5 | BD | `RCAService.cs` with 5-Why storage |
| 7-05 | RCA completion guard wiring | 3 | BD | Guard blocks advance when ROOT cause missing |
| 7-06 | `ICorrectiveActionService`: CA lifecycle + deadline escalation | 8 | BD | `CorrectiveActionService.cs` |
| 7-07 | Close-out guard: all CAs completed | 3 | BD | Guard prevents premature close |
| 7-08 | `IHAZOPService`: study + node + deviation CRUD | 8 | BD | `HAZOPService.cs` |
| 7-09 | `IBarrierManagementService`: Bow-Tie barriers | 5 | BD | `BarrierManagementService.cs` |
| 7-10 | API RP 754 Tier upgrade suggestion logic | 3 | BD | Auto-notify when ≥ 2 barriers failed |
| 7-11 | `IEmergencyResponseService`: activation + obligation generation | 8 | BD | `EmergencyResponseService.cs` |
| 7-12 | `HSEController`: incident + RCA + CA + HAZOP endpoints | 8 | BD | Controller with all CRUD + transition routes |
| 7-13 | HSE API integration tests (WebApplicationFactory) | 5 | QA | 20 test cases covering incident lifecycle |
| 7-14 | `IncidentList.razor`: filterable, field-scoped | 5 | FE | Page at `/ppdm39/hse/incidents` |
| 7-15 | `IncidentDetail.razor`: state + RCA + CA + barriers tabs | 8 | FE | Detail page with 4 tabs |
| 7-16 | `HAZOPStudy.razor`: study + node accordion + deviation table | 8 | FE | Page at `/ppdm39/hse/hazop/{studyId}` |
| 7-17 | `EmergencyResponseStatus.razor`: active ER + obligations | 5 | FE | Page at `/ppdm39/hse/emergency/{id}` |
| 7-18 | `IHSEKPIService` + HSE KPI dashboard (Tier rates, TRIR) | 8 | DA | Service + `HSEKPIDashboard.razor` |
| 7-19 | Exposure hours entry form | 3 | FE | `ExposureHoursEntry.razor` |
| 7-20 | HSE bUnit component tests | 5 | QA | Tests for all HSE components |

**Total: ~118 SP across 4 sprints (29–30 SP/sprint, ~2-week sprints)**

---

## Sprint Breakdown

| Sprint | Stories | Focus |
|---|---|---|
| 7.1 | 7-01 to 7-07 | Incident management service + RCA + CA (backend) |
| 7.2 | 7-08 to 7-12 | HAZOP, barriers, ER service + controller |
| 7.3 | 7-13 to 7-17 | Integration tests + UI pages |
| 7.4 | 7-18 to 7-20 | KPI reporting + exposure hours + UI polish |

---

## RACI Matrix

| Task | BD | FE | QA | DA | HS | PM |
|---|---|---|---|---|---|---|
| Service interfaces | R | I | I | I | C | A |
| Incident state machine wiring | R | I | C | I | C | A |
| Obligation auto-creation | R | I | C | I | A | I |
| HAZOP service + CRUD | R | I | C | I | C | A |
| Barrier management logic | R | I | I | I | A | I |
| API RP 754 Tier suggestion | R | I | I | I | A | I |
| Emergency response service | R | I | I | I | A | I |
| API controller | R | I | C | I | I | A |
| Integration tests | I | I | R | I | I | A |
| Incident/HAZOP UI pages | C | R | C | I | I | A |
| KPI dashboard + charts | C | C | I | R | C | A |
| Exposure hours form | I | R | C | C | I | A |

**HS = HSE Subject-Matter Expert**

---

## Risk Register

| ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R7-1 | PPDM `HSE_INCIDENT_CAUSE` CAUSE_DESC length not sufficient for RCA | Med | Med | Verify column DDL; extend if CLOB/VARCHAR(MAX) |
| R7-2 | OPA 90 immediate notification requires real-time alerting (not batch) | High | High | Schedule `IHostedService` heartbeat every 5 min; add push notification |
| R7-3 | Exposure hours data not yet captured → KPI denominators = 0 | High | Med | Add manual entry form (story 7-19) from Day 1 of sprint 7.4 |
| R7-4 | HSE SME not available for HAZOP guide word review | Med | Med | Use IEC 61882 defaults; capture SME review as acceptance gate in M7-3 |
| R7-5 | Blazor ER page must show real-time data (not page refresh) | Med | Med | Add SignalR subscription to ProgressTrackingClient |

---

## Definition of Done

- [ ] All state machine transitions guarded as per Phase 2 guard specs
- [ ] OPA 90 / AER obligations auto-created on `UNDER_INVESTIGATION` for relevant jurisdictions
- [ ] RCA requires ROOT cause before advancing to CA state
- [ ] All CAs must be COMPLETED/DEFERRED before close-out
- [ ] HAZOP study nodes/deviations saved and retrievable
- [ ] Tier upgrade suggestion fires when ≥ 2 barriers failed
- [ ] Emergency response notification obligations created within the same transaction
- [ ] KPI dashboard shows Tier 1/2 rates, TRIR, LTIF for current field
- [ ] All routes under `/ppdm39/hse/` require `[Authorize]`
- [ ] `dotnet build Beep.OilandGas.sln` produces zero errors
- [ ] ≥ 20 integration tests passing
