# Decommissioning ModuleSetup Plan

## Current state
- Module file: Beep.OilandGas.Decommissioning/Modules/DecommissioningModule.cs
- Two table classes: `DECOMMISSIONING_STATUS`, `ABANDONMENT_STATUS`.
- EntityTypes registered with both local table classes.

## Target state
- DecommissioningModule registers only Decommissioning-owned table classes.
- No PPDM39.Models table registrations.
- SeedAsync currently returns explicit skip (no reference seed needed).

## Phase tasks
- [x] Phase 1: confirm decommissioning schema ownership.
- [x] Phase 2: classify Data classes as table vs projection.
- [x] Phase 3: audit table classes against O&G standards.
- [x] Phase 4: revise table classes with best-practice fields.
- [x] Phase 5: build and validate.
- [ ] Phase 6: add CI ownership checks.

## Audit snapshot
- Local table classes: 2 (`DECOMMISSIONING_STATUS`, `ABANDONMENT_STATUS`).
- Projection classes: 10+ under `Data/Projections/` — correctly excluded from EntityTypes.

---

## SP-A — Domain Audit (Standards Applied)

**Standards referenced**: PPDM 3.9 (`WELL_ABANDONMENT`, `FACILITY_DECOMMISSIONING`), BSEE 30 CFR Part 250 (US OCS abandonment regulations), BSEE NTL 2010-N06 (well plugging requirements), ISO 16530-2 (well integrity — decommissioning phase), FASB ASC 410 (Asset Retirement Obligations — ARO accounting), API RP 5CT (casing for plug placement).

### Audit Findings Matrix

| Class | Gap identified | Standard violated |
|---|---|---|
| `DECOMMISSIONING_STATUS` | Missing `FIELD_ID`, `FACILITY_ID` (PPDM 3.9 FK); missing `DECOM_PHASE` (PLANNING/ENGINEERING/EXECUTION/VERIFICATION/COMPLETED — ISO 16530-2 lifecycle); missing `REGULATORY_AUTHORITY`, `REGULATORY_APPROVAL_NUMBER`; missing `ESTIMATED_COST`, `ACTUAL_COST`, `COST_CURRENCY` (FASB ASC 410 ARO); redundant inherited backing fields `REMARKValue`, `SOURCEValue` | PPDM 3.9 FACILITY_DECOMMISSIONING; ISO 16530-2; FASB ASC 410 |
| `ABANDONMENT_STATUS` | Missing `UWI` (PPDM 3.9 WELL PK — correct column is UWI not WELL_ID); missing `WELLBORE_ID`; missing `ABANDONMENT_TYPE` (TEMPORARY/PERMANENT — BSEE 30 CFR §250.1715); missing `REGULATORY_PERMIT_NUMBER`; missing `PLUG_BACK_DEPTH_MD` / `PLUG_BACK_DEPTH_OUOM` (BSEE NTL 2010-N06 plug depth requirement); missing `VERIFIED_BY`, `VERIFICATION_DATE` (independent verification required by regulators); redundant inherited backing fields | PPDM 3.9 WELL_ABANDONMENT; BSEE 30 CFR §250.1715; BSEE NTL 2010-N06 |

---

## SP-B — Code Revisions

| File | Changes made |
|---|---|
| `Data/Tables/DECOMMISSIONING_STATUS.cs` | Removed redundant inherited backing fields (`REMARKValue`, `SOURCEValue`). Added: `FIELD_ID`, `FACILITY_ID`, `DECOM_PHASE`, `REGULATORY_AUTHORITY`, `REGULATORY_APPROVAL_NUMBER`, `ESTIMATED_COST`, `ACTUAL_COST`, `COST_CURRENCY`. |
| `Data/Tables/ABANDONMENT_STATUS.cs` | Removed redundant inherited backing fields (`REMARKValue`, `SOURCEValue`). Added: `UWI`, `WELLBORE_ID`, `ABANDONMENT_TYPE`, `REGULATORY_PERMIT_NUMBER`, `PLUG_BACK_DEPTH_MD`, `PLUG_BACK_DEPTH_OUOM`, `VERIFIED_BY`, `VERIFICATION_DATE`. |

---

## SP-C — EntityTypes Module

- Module file: `Modules/DecommissioningModule.cs`
- No new entity types added — all revised classes already registered.
- EntityTypes count: 2 (unchanged).

---

## Build Result

```
dotnet build Beep.OilandGas.Decommissioning.csproj -v q
Build succeeded. 0 Warning(s). 0 Error(s).
```
