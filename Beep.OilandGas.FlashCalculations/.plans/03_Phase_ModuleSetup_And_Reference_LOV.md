# Phase 3 — ModuleSetup and reference LOV

## Goal (delivered baseline)

**`FlashCalculationsModule`** registers extension table **`R_FLASH_CALCULATION_REFERENCE_CODE`** and seeds LOVs for:

| `REFERENCE_SET` | Purpose |
|-----------------|--------|
| **`FLASH_EOS_MODEL`** | PR, SRK, SRK_MODIFIED, IDEAL_K |
| **`FLASH_CALC_CATEGORY`** | ISOTHERMAL_FLASH, MULTISTAGE_FLASH, PHASE_ENVELOPE, RIGOROUS_FLASH |
| **`FLASH_SOLVER_PRESET`** | DEFAULT, STRICT, FAST |
| **`FLASH_SPECIFICATION`** | PT_SPECIFIED, PH_SPECIFIED, TP_SPECIFIED |
| **`FLASH_PHASE_STATE`** | OVERALL, VAPOR, LIQUID, AQUEOUS |
| **`FLASH_PROPERTY_KIND`** | COMPRESSIBILITY_Z, MOLAR_VOLUME, FUGACITY_COEFF, K_VALUE |

**Do not** register standard PPDM core tables on **`EntityTypes`** — only this extension **`R_*`** table.

## Target files

- **`Modules/FlashCalculationsModule.cs`**
- **`Data/Tables/R_FLASH_CALCULATION_REFERENCE_CODE.cs`**
- **`Data/Constants/FlashReferenceSets.cs`**, **`FlashReferenceCodeSeed.cs`**

## TODO checklist

- [x] **`SeedAsync`** idempotent by **`REFERENCE_SET` + `REFERENCE_CODE`**.
- [ ] UI/API: bind picklists to **`R_FLASH_CALCULATION_REFERENCE_CODE`** where appropriate.
- [ ] Optional: add **binary interaction** default reference set when EOS BIPs are modeled.

## Verification

- Run **`PPDM39SetupService`** / module orchestrator seed on dev DB — rows appear without duplicates.
