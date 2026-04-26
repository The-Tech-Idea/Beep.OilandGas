# ProspectIdentification ModuleSetup Plan

## Current state
- Module file: Beep.OilandGas.ProspectIdentification/Modules/ExplorationModule.cs
- ModuleSetupBase exists and EntityTypes is populated with project-owned exploration tables.
- Data classes are already split across Contracts/Core/Projections/Tables.

## Target state
- ExplorationModule EntityTypes includes only ProspectIdentification Data/Tables persisted classes.
- All data classes are complete per PPDM 3.9, SPE PRMS, and SEC reserves best practices.
- SeedAsync seeds only exploration/project-owned workflow/reference bootstrap data.

## Phase tasks
- [x] Phase 1: confirm exploration-owned table list under Data/Tables.
- [x] Phase 2: verify all table classes are persisted-only shape.
- [x] Phase 3: repopulate EntityTypes from local table classes only.
- [x] Phase 4: isolate exploration bootstrap seeds from shared PPDM seeds.
- [x] Phase 5: build project validation completed.
- [ ] Phase 6: add CI guard for forbidden PPDM39.Models in module.

---

## Sub-Phase SP-A — O&G Best Practice Domain Audit (completed April 2026)

### Standards applied
| Standard | Domain |
|---|---|
| PPDM 3.9 | Prospect, play, lead, source rock data model |
| SPE PRMS (2018) | Probabilistic volume estimates (P10/P50/P90/Mean), STOIIP/GIIP/EUR classification |
| Lahee Classification | Prospect/well outcome classification A/B1/B2/C1/C2/D1/D2 |
| SEC Reg S-X Rule 4-10 | Proved/probable/possible reserves basis, dry-hole cost accounting |
| FASB ASC 932 | Exploration cost classification; dry-hole, deferred classification |

### Audit findings — missing / incorrect fields per class

| Class | Missing fields | Issue |
|---|---|---|
| PROSPECT | LEAD_ID, PROSPECT_CATEGORY, PRIMARY_FLUID_TYPE, PROSPECT_CLASSIFICATION (Lahee), SEISMIC_COVERAGE_IND, RANK_VALUE, ACTIVE_IND | No lifecycle traceability back to originating LEAD; no Lahee class |
| PROSPECT_RISK_ASSESSMENT | CHARGE_RISK, POS_GEOLOGICAL, POS_COMMERCIAL, POS_OVERALL | Missing charge/migration risk; missing explicit PoS fields required for EMV calculation |
| PROSPECT_VOLUME_ESTIMATE | ESTIMATE_TYPE (STOIIP/GIIP/EUR), FLUID_TYPE, OIL_VOLUME_MEAN, GAS_VOLUME_MEAN, CONDENSATE_VOLUME_P50, BOE_EQUIVALENT_P50 | No SPE PRMS resource category; no mean (EV) estimate; no condensate/BOE equivalent |
| PROSPECT_RESERVOIR | RESERVOIR_NAME, FLUID_TYPE, TEMPERATURE/OUOM, PRESSURE/OUOM, WATER_SATURATION, OIL_API_GRAVITY, GAS_GRAVITY, DEPTH_TO_TOP/BASE/OUOM, GAS_OIL_RATIO/OUOM, FORMATION_VOLUME_FACTOR | Missing critical PVT and depth data for volumetric calculations |
| PROSPECT_TRAP | TRAP_CONFIDENCE, STRUCTURAL_STYLE, DEPTH_TO_CLOSURE/OUOM, SPILL_POINT_DEPTH/OUOM | No structural style classification; no spill-point geometry data |
| PROSPECT_SOURCE_ROCK | S1, S2, S3, THICKNESS/OUOM | Missing Rock-Eval pyrolysis parameters (industry standard for source quality) |
| PLAY | BASIN_ID, PROVINCE_NAME, SEAL_TYPE, FLUID_TYPE, MATURITY_STATUS, PLAY_RISK, ACTIVE_IND | No basin context; no play maturity lifecycle; no seal-type characterisation |
| LEAD | LEAD_TYPE, ESTIMATED_AREA/OUOM, GEOLOGIST_ID, CONFIDENCE_LEVEL, ACTIVE_IND | No seismic/gravity data origin type; no confidence level; no soft delete |
| EXPLORATION_BUDGET | BUDGET_ID (PK), BUDGET_YEAR (wrong type: decimal→int), APPROVAL_STATUS, APPROVED_BY, APPROVAL_DATE, BUDGET_VERSION, AFE_NUMBER | BUDGET_YEAR is decimal (must be int); no AFE number; no approval workflow |
| PROSPECT_DISCOVERY | DISCOVERY_WELL_UWI, VOLUMES_DISCOVERED_CONDENSATE, FLOW_RATE_OIL/GAS/OUOM, DISCOVERY_SIGNIFICANCE, RESOURCE_CLASS | Missing well test rates; no PRMS resource classification |

---

## Sub-Phase SP-B — Data Class Revisions (completed April 2026)

### Changes applied

| Class | Added fields | Fixed |
|---|---|---|
| PROSPECT | LEAD_ID, PROSPECT_CATEGORY, PRIMARY_FLUID_TYPE, PROSPECT_CLASSIFICATION, SEISMIC_COVERAGE_IND, RANK_VALUE, ACTIVE_IND | — |
| PROSPECT_RISK_ASSESSMENT | CHARGE_RISK, POS_GEOLOGICAL, POS_COMMERCIAL, POS_OVERALL | — |
| PROSPECT_VOLUME_ESTIMATE | ESTIMATE_TYPE, FLUID_TYPE, OIL_VOLUME_MEAN, GAS_VOLUME_MEAN, CONDENSATE_VOLUME_P50, BOE_EQUIVALENT_P50 | — |
| PROSPECT_RESERVOIR | RESERVOIR_NAME, FLUID_TYPE, TEMPERATURE, TEMPERATURE_OUOM, PRESSURE, PRESSURE_OUOM, WATER_SATURATION, OIL_API_GRAVITY, GAS_GRAVITY, DEPTH_TO_TOP, DEPTH_TO_BASE, DEPTH_OUOM, GAS_OIL_RATIO, GOR_OUOM, FORMATION_VOLUME_FACTOR | — |
| PROSPECT_TRAP | TRAP_CONFIDENCE, STRUCTURAL_STYLE, DEPTH_TO_CLOSURE, DEPTH_OUOM, SPILL_POINT_DEPTH, SPILL_POINT_DEPTH_OUOM | — |
| PROSPECT_SOURCE_ROCK | S1, S2, S3, THICKNESS, THICKNESS_OUOM | — |
| PLAY | BASIN_ID, PROVINCE_NAME, SEAL_TYPE, FLUID_TYPE, MATURITY_STATUS, PLAY_RISK, ACTIVE_IND | — |
| LEAD | LEAD_TYPE, ESTIMATED_AREA, ESTIMATED_AREA_OUOM, GEOLOGIST_ID, CONFIDENCE_LEVEL, ACTIVE_IND | — |
| EXPLORATION_BUDGET | BUDGET_ID, APPROVAL_STATUS, APPROVED_BY, APPROVAL_DATE, BUDGET_VERSION, AFE_NUMBER | BUDGET_YEAR: decimal → int |
| PROSPECT_DISCOVERY | DISCOVERY_WELL_UWI, VOLUMES_DISCOVERED_CONDENSATE, FLOW_RATE_OIL, FLOW_RATE_GAS, FLOW_RATE_OUOM, DISCOVERY_SIGNIFICANCE, RESOURCE_CLASS | — |

---

## Sub-Phase SP-C — EntityTypes Update
- ExplorationModule.EntityTypes already includes all revised classes (no new classes added).
- No registration changes needed — revisions are in-place additions to existing table classes.

---

## Audit snapshot
- Local table ownership currently registered: 26 entity types.
- Status: SP-A and SP-B revisions applied; build validated — 0 errors, 0 warnings.

## Notes
- Keep shared PPDM reference seeding delegated to PPDM39.DataManagement modules.
- OIL_PRICE / GAS_PRICE / DEVELOPMENT_COST in PROSPECT_RISK_ASSESSMENT are misplaced (belong in PROSPECT_ECONOMIC); retained for now to avoid breaking changes — flag for future migration.
- BUDGET_YEAR type corrected from decimal to int; downstream code must be updated if it assigns a decimal value.
