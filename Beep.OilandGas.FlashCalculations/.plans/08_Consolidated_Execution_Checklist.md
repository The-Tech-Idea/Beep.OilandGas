# Consolidated execution checklist — FlashCalculations

## Architecture

- [ ] **CLAUDE.md** — table vs projection; **no** `DTO` namespaces
- [ ] **`PPDM39.DataManagement`** does **not** own flash domain modules
- [ ] **`FlashCalculationsModule`** lists **only** extension **`R_FLASH_CALCULATION_REFERENCE_CODE`**

## Data

- [ ] **`SeedAsync`** remains idempotent
- [ ] New codes added to **`FlashReferenceCodeSeed`** + **`FlashReferenceSets`** together

## Quality

- [ ] `dotnet build Beep.OilandGas.FlashCalculations`
- [ ] `dotnet test Beep.OilandGas.FlashCalculations.Tests`

## Docs

- [ ] **README** units and EOS assumptions when behavior changes
