# DrillingAndConstruction ModuleSetup Plan

## Current state
- Module file: Beep.OilandGas.DrillingAndConstruction/Modules/DevelopmentModule.cs
- ModuleSetupBase exists; EntityTypes populated with 7 execution table classes (see SP-B).
- Module name implies broad Development ownership, but project scope is execution/construction.
- This is a legacy naming mismatch, not intended final architecture.

## Target state
- Rename module concept to DrillingExecutionModule.
- Update module metadata to execution ownership (ModuleId/ModuleName), even if filename transition is staged.
- Register only DrillingAndConstruction-owned table classes.
- No PPDM39.Models table registrations.

## Phase tasks
- [x] Phase 1: mark current DevelopmentModule as legacy and approve DrillingExecution ownership.
- [x] Phase 2: identify execution-owned tables in this project.
- [x] Phase 3: update EntityTypes with local execution tables only and remove planning semantics.
- [x] Phase 4: keep seed behavior limited to execution bootstrap data.
- [x] Phase 5: build project validation completed.
- [ ] Phase 6: enforce ownership guardrails in review checklist.

## Decision status
- Decision confirmed: this project remains execution ownership; planning ownership moves to DevelopmentPlanning.

---

## Sub-Phase SP-A — Domain Audit (O&G Best-Practice Standards)

### Standards Applied
| Standard | Scope |
|---|---|
| PPDM 3.9 | Drilling activity, wellbore status, well tubular |
| IADC DDR | Daily Drilling Report structure, NPT classification codes |
| IADC Dull-Grade | Bit grading system (8-column dull-grade string) |
| API RP 13B | Drilling fluid measurement and reporting |
| API RP 10D / ISO 10426 | Cementing job record, CBL evaluation |
| API Spec 5CT | Casing steel grades (J-55, K-55, N-80, P-110, Q-125) |

### Audit Findings
| Class | Finding | Action |
|---|---|---|
| (none existed) | Project had zero persisted table classes at start of this phase | Create all required execution classes |
| DRILLING_PROGRAM (needed) | No planned-well program record existed | Create: PK, WELL_ID, AFE, rig details, well profile, planned depths/dates |
| DRILLING_DAILY_REPORT (needed) | No DDR record existed | Create: depth range, footage, rotating/NPT hours, daily cost, cumulative cost |
| DRILLING_ACTIVITY (needed) | No fine-grained IADC time-log existed | Create: activity code, NPT code, start/end time, NPT_IND |
| DRILLING_FLUID (needed) | No mud-property log existed | Create: mud type, weight, PV, YP, gel, fluid loss, pH, chlorides |
| DRILLING_BIT (needed) | No bit record existed | Create: IADC code, size, run number, footage, rotating hours, dull grade |
| CASING_PROGRAM (needed) | No casing string design record existed | Create: string type, OD, weight, grade, connection, setting depth, ratings |
| CEMENT_JOB (needed) | No cementing job record existed | Create: job type, slurry volume/weight, TOC, max pressure, CBL quality |

---

## Sub-Phase SP-B — Changes Applied

| File Created | Class | Key Fields Added | Standard |
|---|---|---|---|
| Data/Tables/DRILLING_PROGRAM.cs | DRILLING_PROGRAM | PROGRAM_ID, WELL_ID, FIELD_ID, PROGRAM_STATUS, WELL_TYPE, WELL_PROFILE, PLANNED_MD, PLANNED_TVD, DEPTH_OUOM, PLANNED_DURATION_DAYS, AFE_COST, AFE_NUMBER, RIG_NAME, RIG_TYPE, PLANNED_SPUD_DATE | PPDM 3.9 / IADC |
| Data/Tables/DRILLING_DAILY_REPORT.cs | DRILLING_DAILY_REPORT | DDR_ID, PROGRAM_ID, WELL_ID, REPORT_DATE, REPORT_NUMBER, DEPTH_START_MD, DEPTH_END_MD, FOOTAGE_DRILLED, ROTATING_HOURS, NPT_HOURS, NPT_REASON, DAILY_COST, CUMULATIVE_COST, COST_CURRENCY, DRILLER_NAME, REPORT_STATUS | IADC DDR |
| Data/Tables/DRILLING_ACTIVITY.cs | DRILLING_ACTIVITY | ACTIVITY_ID, DDR_ID, WELL_ID, START_TIME, END_TIME, DURATION_HOURS, ACTIVITY_CODE, NPT_CODE_IADC, NPT_IND, DEPTH_AT_START_MD | IADC time-log |
| Data/Tables/DRILLING_FLUID.cs | DRILLING_FLUID | FLUID_ID, DDR_ID, WELL_ID, MUD_TYPE, MUD_WEIGHT, PLASTIC_VISCOSITY, YIELD_POINT, GEL_10SEC_STRENGTH, GEL_10MIN_STRENGTH, FLUID_LOSS, PH, CHLORIDE_CONCENTRATION, TOTAL_ACTIVE_VOLUME | API RP 13B |
| Data/Tables/DRILLING_BIT.cs | DRILLING_BIT | BIT_ID, PROGRAM_ID, WELL_ID, BIT_RUN_NUMBER, IADC_CODE, BIT_SIZE, DEPTH_IN_MD, DEPTH_OUT_MD, FOOTAGE_DRILLED, ROTATING_HOURS, WEIGHT_ON_BIT, ROP_AVG, PULL_REASON_CODE, DULL_GRADE | IADC bit record |
| Data/Tables/CASING_PROGRAM.cs | CASING_PROGRAM | CASING_ID, PROGRAM_ID, WELL_ID, CASING_STRING, NOMINAL_OD_SIZE, WEIGHT_PER_FT, GRADE_STEEL, CONNECTION_TYPE, PLANNED_SET_DEPTH_MD, ACTUAL_SET_DEPTH_MD, BURST_RATING, COLLAPSE_RATING, TENSION_RATING, STATUS_CASING | API Spec 5CT |
| Data/Tables/CEMENT_JOB.cs | CEMENT_JOB | CEMENT_JOB_ID, CASING_ID, WELL_ID, JOB_TYPE, JOB_DATE, SLURRY_VOLUME_MIXED, SLURRY_WEIGHT, TOP_OF_CEMENT_MD, MAX_PUMP_PRESSURE, SQUEEZE_RESULT_CODE, CBL_LOG_IND, CBL_QUALITY_SCORE | API RP 10D / ISO 10426 |

---

## Sub-Phase SP-C — EntityTypes Update

DevelopmentModule._entityTypes updated to register all 7 new classes:
```
typeof(DRILLING_PROGRAM)
typeof(DRILLING_DAILY_REPORT)
typeof(DRILLING_ACTIVITY)
typeof(DRILLING_FLUID)
typeof(DRILLING_BIT)
typeof(CASING_PROGRAM)
typeof(CEMENT_JOB)
```
Using added: `Beep.OilandGas.Models.Data.DrillingAndConstruction`

**Build result**: 0 errors, 0 warnings ✓

