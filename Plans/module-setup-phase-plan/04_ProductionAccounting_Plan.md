# ProductionAccounting ModuleSetup Plan

## Current state
- Module file: Beep.OilandGas.ProductionAccounting/Modules/ProductionModule.cs
- ModuleSetupBase exists; EntityTypes now populated with local production-accounting table classes.

## Target state
- ProductionModule should register only ProductionAccounting-owned table classes.
- No PPDM39.Models table registrations.
- SeedAsync limited to production accounting bootstrap/reference defaults owned by this project.

## Phase tasks
- [x] Phase 1: confirm production accounting ownership set.
- [x] Phase 2: classify Data/ProductionAccounting classes by table/contract/projection.
- [x] Phase 3: update EntityTypes with local table classes only.
- [x] Phase 4: isolate local seed defaults from shared PPDM reference seed.
- [x] Phase 5: build and validate setup flow.
- [ ] Phase 6: add forbidden-usage CI scan for module file.

## Audit snapshot
- Local table ownership currently registered: 106 entity types.
- Note: persisted table classes are stored under `Data/ProductionAccounting` (not a literal `Data/Tables` folder), which is valid for this project.

---

## SP-A — Domain Audit (Standards Applied)

**Standards referenced**: PPDM 3.9 (PDEN / PDEN_VOL_SUMMARY), API MPMS (Manual of Petroleum Measurement Standards), COPAS (Council of Petroleum Accountants Societies), SEC Reg S-X Rule 4-10, FASB ASC 932, state royalty regulations.

### Audit Findings Matrix

| Class | Gap identified | Standard violated |
|---|---|---|
| `RUN_TICKET` | Missing `VOLUME_OUOM`, `TICKET_STATUS`, `GRAVITY_CORRECTION_FACTOR`, `FIELD_ID`, `PIPELINE_TICKET_NUMBER`; duplicate `BSWVOLUME`/`BSWPERCENTAGE` properties (no-underscore duplicates of `BSW_VOLUME`/`BSW_PERCENTAGE`) | API MPMS Chapter 11 (volume correction to 60°F); API MPMS Chapter 6 (LACT custody transfer) |
| `PRODUCTION_ALLOCATION` | Missing `FLUID_TYPE`, `PRODUCTION_PERIOD_START`, `PRODUCTION_PERIOD_END`, `ALLOCATED_VOLUME`, `ALLOCATED_VOLUME_OUOM`, `ALLOCATION_FACTOR`, `ALLOCATION_STATUS`; `ALLOCATION_RESULTS_JSON` is a JSON blob in a persisted table class (anti-pattern — queries impossible) | PPDM 3.9 PDEN allocation model; API MPMS allocation methodology |
| `PROVED_RESERVES` | Missing `FIELD_ID`, `FISCAL_YEAR` (int), `RESERVES_CATEGORY` (1P/2P/3P), `PRICE_DECK_TYPE`, `OIL_VOLUME_OUOM`, `GAS_VOLUME_OUOM`, `PROVED_DEVELOPED_PRODUCING_OIL`, `PROVED_DEVELOPED_NONPRODUCING_OIL`, `ENGINEER_CERTIFICATION_ID`; redundant inherited backing fields `EFFECTIVE_DATEValue`, `EXPIRY_DATEValue`, `REMARKValue`, `SOURCEValue` | SEC Reg S-X Rule 4-10; FASB ASC 932-235 (proved reserve roll-forward disclosure); SPE PRMS 2018 §2.1.1 |
| `ROYALTY_CALCULATION` | Missing `ROYALTY_TYPE` (MINERAL/OVERRIDING/WORKING_INTEREST/NPI/ORRI), `ROYALTY_BASIS` (GROSS/NET_BACK/WELLHEAD), `FLUID_TYPE`, `PRODUCTION_PERIOD_START`, `GROSS_VOLUME`, `GROSS_VOLUME_OUOM`, `PRICE_PER_UNIT`, `PAYMENT_STATUS`; has navigation property `DEDUCTIONS` (object reference in table class — forbidden); redundant inherited backing fields | COPAS accounting procedures (JIB/royalty); state royalty regulations (severance tax basis) |

### Other findings (noted, not blocking)
- `ALLOCATION_RESULTS_JSON` in `PRODUCTION_ALLOCATION`: JSON blob in a persisted table class prevents SQL-level query. Left in place (breaking change to remove), documented as technical debt.
- `CompatibilityAliases.cs` BSW shim referenced removed duplicate fields — fixed to use canonical `BSW_VOLUME` / `BSW_PERCENTAGE`.

---

## SP-B — Code Revisions

| File | Changes made |
|---|---|
| `Data/ProductionAccounting/PROVED_RESERVES.cs` | Removed redundant inherited backing fields (`EFFECTIVE_DATEValue`, `EXPIRY_DATEValue`, `REMARKValue`, `SOURCEValue`). Added: `FIELD_ID`, `FISCAL_YEAR` (int), `RESERVES_CATEGORY`, `PROVED_DEVELOPED_PRODUCING_OIL`, `PROVED_DEVELOPED_NONPRODUCING_OIL`, `OIL_VOLUME_OUOM`, `GAS_VOLUME_OUOM`, `PRICE_DECK_TYPE`, `ENGINEER_CERTIFICATION_ID`. |
| `Data/ProductionAccounting/ROYALTY_CALCULATION.cs` | Removed redundant inherited backing fields (`REMARKValue`, `SOURCEValue`). Removed navigation property `DEDUCTIONS` (table class shape violation). Added: `ROYALTY_TYPE`, `ROYALTY_BASIS`, `FLUID_TYPE`, `PRODUCTION_PERIOD_START`, `GROSS_VOLUME`, `GROSS_VOLUME_OUOM`, `PRICE_PER_UNIT`, `PAYMENT_STATUS`. |
| `Data/ProductionAccounting/RUN_TICKET.cs` | Removed duplicate `BSWVOLUME` / `BSWPERCENTAGE` properties (already existed as `BSW_VOLUME` / `BSW_PERCENTAGE`). Added: `FIELD_ID`, `VOLUME_OUOM`, `TICKET_STATUS`, `GRAVITY_CORRECTION_FACTOR`, `PIPELINE_TICKET_NUMBER`. |
| `Data/ProductionAccounting/PRODUCTION_ALLOCATION.cs` | Added: `FLUID_TYPE`, `PRODUCTION_PERIOD_START`, `PRODUCTION_PERIOD_END`, `ALLOCATED_VOLUME`, `ALLOCATED_VOLUME_OUOM`, `ALLOCATION_FACTOR`, `ALLOCATION_BASIS`, `ALLOCATION_STATUS`. |
| `Data/ProductionAccounting/CompatibilityAliases.cs` | Fixed `BSWVolume` / `BSWPercentage` shim getters/setters to use canonical `BSW_VOLUME` / `BSW_PERCENTAGE` (removed fallback to now-deleted duplicate fields). |
| `Services/ProductionAccountingService.ControllerFacade.cs` | Updated object initializer and local variable to use `BSW_PERCENTAGE` instead of `BSWPERCENTAGE`. |

---

## SP-C — EntityTypes Module

- Module file: `Modules/ProductionModule.cs`
- No new entity types were added — all revised classes were already registered.
- EntityTypes count: 106 (unchanged).

---

## Build Result

```
dotnet build Beep.OilandGas.ProductionAccounting.csproj -v q
Build succeeded. 0 Warning(s). 0 Error(s).
```

