# DevelopmentPlanning ModuleSetup Plan

## Current state
- DevelopmentPlanningModule registered; EntityTypes now has 4 table classes.
- Planning ownership correctly split from DrillingAndConstruction execution.

## Target state
- DevelopmentPlanning owns FDP lifecycle, well schedule, facility investment, and cost tracking.

## Phase tasks
- [x] Phase 1: define DevelopmentPlanning module ownership and ModuleId.
- [x] Phase 2: classify all planning data classes into Tables/Contracts/Projections.
- [x] Phase 3: implement new DevelopmentPlanningModule with local EntityTypes.
- [x] Phase 4: add idempotent planning seed scope.
- [x] Phase 5: build project validation completed — 0 errors, 0 warnings.
- [x] Phase 6: document ownership boundary vs DrillingAndConstruction.

## Key outcome
- This project is canonical owner of planning setup.

---

## Sub-Phase SP-A — Domain Audit (O&G Best-Practice Standards)

### Standards Applied
| Standard | Scope |
|---|---|
| SPE PRMS 2018 §3.4 | FDP as basis for Developed Reserves booking |
| SPE PRMS 2018 §2.1.2 | Well-by-well assignment for Developed Reserves |
| SPE PRMS 2018 §6 | Abandonment provision (ABEX) |
| AACE International | Cost estimate class 1-5 accuracy ranges |
| PPDM 3.9 FACILITY | Facility investment complement |

### Audit Findings
| Class | Finding | Action |
|---|---|---|
| DEVELOPMENT_COSTS | Missing FIELD_ID, FDP_ID, COST_YEAR (was decimal), COST_CATEGORY, COST_TYPE, COST_CURRENCY, ABANDONMENT_PROVISION | Revised |
| FIELD_DEVELOPMENT_PLAN | Not present — required as SPE PRMS basis document | Created |
| DEVELOPMENT_WELL_SCHEDULE | Not present — required for Developed Reserves well assignment | Created |
| FACILITY_INVESTMENT | Not present — required for surface CAPEX tracking | Created |

---

## Sub-Phase SP-B — Changes Applied

| File | Action | Key Fields |
|---|---|---|
| Data/Tables/DEVELOPMENT_COSTS.cs | Revised | Added FIELD_ID, FDP_ID, COST_YEAR (int), COST_CATEGORY, COST_TYPE, COST_CURRENCY, ABANDONMENT_PROVISION_MM |
| Data/Tables/FIELD_DEVELOPMENT_PLAN.cs | Created | FDP_ID, FIELD_ID, FDP_VERSION, FDP_STATUS, SUBMISSION_DATE, APPROVAL_DATE, FIRST_PRODUCTION_DATE, TOTAL_CAPEX_MM, RECOVERABLE_OIL_MMBBL, RECOVERABLE_GAS_BCF |
| Data/Tables/DEVELOPMENT_WELL_SCHEDULE.cs | Created | SCHEDULE_ID, FDP_ID, PLANNED_WELL_ID, WELL_TYPE, TARGET_RESERVOIR, AFE_NUMBER, AFE_COST_MM, PLANNED_SPUD_DATE, SCHEDULE_STATUS, INITIAL_RATE_OIL_BOPD |
| Data/Tables/FACILITY_INVESTMENT.cs | Created | FACILITY_INV_ID, FDP_ID, FACILITY_TYPE, INVESTMENT_PHASE, ESTIMATE_CLASS (AACE 1-5), CAPEX_PLANNED_MM, CAPEX_ACTUAL_MM, CAPACITY_PLANNED_BOPD |

---

## Sub-Phase SP-C — EntityTypes Update

DevelopmentPlanningModule._entityTypes updated:
```
typeof(DEVELOPMENT_COSTS)
typeof(FIELD_DEVELOPMENT_PLAN)
typeof(DEVELOPMENT_WELL_SCHEDULE)
typeof(FACILITY_INVESTMENT)
```

**Build result**: 0 errors, 0 warnings ✓
