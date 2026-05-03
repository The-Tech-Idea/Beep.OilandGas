# Consolidated execution checklist — FlashCalculations

## Architecture

- [x] **CLAUDE.md** — table vs projection; **no** `DTO` namespaces (unchanged contract)
- [x] **`PPDM39.DataManagement`** does **not** own flash domain modules
- [x] **`FlashCalculationsModule`** lists **only** extension **`R_FLASH_CALCULATION_REFERENCE_CODE`**

## Data

- [x] **`SeedAsync`** remains idempotent
- [x] New codes added to **`FlashReferenceCodeSeed`** + **`FlashReferenceSets`** together (use **`FlashEquationOfStateMapping`** for wire EOS alignment)

## Quality

- [x] `dotnet build Beep.OilandGas.FlashCalculations` — **`Beep.OilandGas.PPDM.Models`** is a **project reference** across the solution (no NuGet **1.0.21** feed required for local builds).
- [x] `dotnet test Beep.OilandGas.FlashCalculations.Tests`

## Docs

- [x] **README** — units table + EOS / **`FlashEquationOfStateMapping`** note
