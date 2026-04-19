# Phase 8 — Sprint Plan and RACI
## Compliance Module — 3 Sprints (~70 Story Points), RACI, Risks, DoD

---

## Story Table

| ID | Story | Points | Owner | Deliverable |
|---|---|---|---|---|
| 8-01 | `IComplianceService` + `IRoyaltyCalculationService` + `IGHGReportingService` interfaces | 3 | BD | All 3 interfaces in `Models/Core/Interfaces` |
| 8-02 | `ComplianceService`: OBLIGATION CRUD + auto-create on event triggers | 8 | BD | `ComplianceService.cs` |
| 8-03 | Escalation `IHostedService`: overdue detection + NOTIFICATION | 5 | BD | `ObligationEscalationService.cs` |
| 8-04 | `RoyaltyCalculationService`: ONRR GOR + Alberta Crown MRF | 8 | BD | `RoyaltyCalculationService.cs` |
| 8-05 | Royalty variance flagging: threshold > $500 or > 5% | 3 | BD | OBLIG_STATUS set; NOTIFICATION created |
| 8-06 | `GHGReportingService`: emission factor lookup + annual total | 8 | BD | `GHGReportingService.cs` + `EmissionFactors.json` |
| 8-07 | `JurisdictionRules` helper: obligation type matrix | 3 | BD | Static helper covering all 3 jurisdictions |
| 8-08 | `ComplianceController`: obligation + royalty + GHG endpoints | 5 | BD | Controller with all routes |
| 8-09 | Report export endpoints: EIA-914, OGOR, AER ST-39/ST-60 | 8 | BD | 4 export endpoints |
| 8-10 | Compliance API integration tests | 5 | QA | 15 test cases |
| 8-11 | `ComplianceDashboard.razor`: upcoming/overdue obligations grid | 5 | FE | Dashboard page |
| 8-12 | `ObligationDetail.razor`: obligation + payment + history | 5 | FE | Detail page |
| 8-13 | `RoyaltyVariancePanel.razor`: variance table with color coding | 3 | FE | Panel component |
| 8-14 | `GHGReportPage.razor`: year/jurisdiction picker + source table | 5 | FE | GHG page |
| 8-15 | CSV download buttons for all 4 report formats | 3 | FE | JS `downloadFile` invocation |
| 8-16 | bUnit tests for compliance components | 3 | QA | Tests for dashboard + detail |
| 8-17 | `EmissionFactors.json` configuration seeded | 2 | DA | JSON file with all jurisdiction/category combos |

**Total: ~83 SP across 3 sprints**

---

## Sprint Breakdown

| Sprint | Stories | Focus |
|---|---|---|
| 8.1 | 8-01 to 8-07 | Services + royalty + GHG (backend) |
| 8.2 | 8-08 to 8-11 | Controller + integration tests + dashboard UI |
| 8.3 | 8-12 to 8-17 | Detail pages, report exports, component tests |

---

## RACI Matrix

| Task | BD | FE | QA | DA | CS | PM |
|---|---|---|---|---|---|---|
| Service interfaces | R | I | I | I | C | A |
| OBLIGATION CRUD + triggers | R | I | C | I | C | A |
| Escalation service | R | I | C | I | A | I |
| Royalty calculation | R | I | I | C | A | I |
| Variance flagging | R | I | C | I | A | I |
| GHG emission factors | R | I | I | C | C | A |
| Jurisdiction routing | R | I | C | I | C | A |
| API controller | R | I | C | I | I | A |
| Report export formats | R | C | C | A | C | I |
| Integration tests | I | I | R | I | I | A |
| Dashboard + detail UI | C | R | C | I | I | A |
| CSV download buttons | C | R | C | I | I | A |
| Component tests | I | C | R | I | I | A |
| EmissionFactors.json | C | I | I | R | C | A |

**CS = Compliance Subject-Matter Expert**

---

## Risk Register

| ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R8-1 | ONRR royalty rate table changes annually; hard-coded values will become stale | High | High | Store rate tiers in database `R_ROYALTY_RATE` table; updatable without code deploy |
| R8-2 | Alberta MRF r-value calculation requires verified price benchmark source | Med | High | Accept input price from user or API integration (NYMEX/GLJ) in Sprint 8.1 |
| R8-3 | EIA / BSEE form column format changes; CSV structure brittle | Med | Med | Unit test with golden CSV fixture; add format version field |
| R8-4 | Compliance SME not available for obligation trigger review | Med | High | Create trigger matrix draft for review in Sprint 8.1 kickoff |
| R8-5 | GHG factor table coverage incomplete for edge cases (e.g., H2 combustion) | Low | Med | Log a warning and skip unconfigured source categories; flag for manual review |

---

## Definition of Done

- [ ] `OBLIGATION` rows auto-created on all documented production/incident triggers
- [ ] Overdue obligations detected daily; NOTIFICATION rows created
- [ ] Royalty calculation produces `OBLIG_PAYMENT` rows with variance for both USA and Canada
- [ ] Variance > $500 or > 5% sets `OBLIG_STATUS='VARIANCE_FLAGGED'` and notifies
- [ ] GHG annual report generated matching test golden values (within 0.1% rounding tolerance)
- [ ] EIA-914, OGOR, AER ST-39, AER ST-60 CSV exports pass format validation
- [ ] All compliance routes require `[Authorize]`; export routes require `ComplianceOfficer` or `Manager`
- [ ] `dotnet build Beep.OilandGas.sln` produces zero errors
- [ ] ≥ 15 integration test cases passing
