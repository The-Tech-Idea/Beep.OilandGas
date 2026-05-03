# Phase 0 — Gas lift overview and baseline

## Purpose

**Gas lift** potential, **valve design**, **spacing**, and **performance** workflows for artificial lift. Calculations live in **`Beep.OilandGas.GasLift`**; shared wire types in **`Beep.OilandGas.Models.Data.GasLift`**.

## Inventory

| Area | Location |
|------|-----------|
| Contract | **`IGasLiftService`** (`Beep.OilandGas.Models.Core.Interfaces`) |
| Service | **`GasLiftService`** + **`GasLiftService.Advanced`** |
| Calculations | **`GasLiftCalculator`**, **`GasLiftValveDesignCalculator`**, **`GasLiftValveSpacingCalculator`**, **`GasLiftPotentialCalculator`**, … |
| Constants | **`GasLiftConstants`** |
| Extension LOV | **`R_GAS_LIFT_REFERENCE_CODE`** (`Beep.OilandGas.GasLift.Data`) |
| Reference seed | **`GasLiftReferenceSets`**, **`GasLiftReferenceCodeSeed`** |
| Module | **`GasLiftModule`** (**`Order` 74**) |

## Baseline gaps

| Gap | Notes | Phase |
|-----|--------|--------|
| Reference LOVs not persisted | Addressed by **`R_GAS_LIFT_REFERENCE_CODE`** + **`GasLiftModule.SeedAsync`** | 1 |
| README drift (wrong package names) | Align with **`GasLiftProperties`** / **`Models`** | 3 |
| Golden vectors for valve / spacing | Regression vs field spreadsheet | 2 |

## Exit criteria

- [x] **`GasLiftModule`** + **`R_GAS_LIFT_REFERENCE_CODE`** documented and seeded (idempotent).
- [x] Golden / regression tests for spacing + design path — **`GasLiftSpacingAndDesignRegressionTests`** (phase 2).
