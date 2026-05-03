# Phase 2 — Numerics, validation, and units

## Goal

**Rankine / psia** (or documented SI mode) consistency; **Wilson** / **Rachford–Rice** stability; **`FlashValidator`** aligned with **`FlashConstants`** bounds (**K-value**, iterations, tolerance).

## Target files

- **`Calculations/*.cs`**, **`Validation/FlashValidator.cs`**, **`Constants/FlashConstants.cs`**

## Industry practices

| Topic | Practice |
|-------|------------|
| **Specified variables** | PT flash is primary; document PH/UV as future |
| **K-values** | Clamp with **`MinimumKValue`** / **`MaximumKValue`**; log oscillation |
| **EOS** | PR/SRK α correlation assumptions documented in **README** |

## TODO checklist

- [x] Single **units** table in **README** (P, T, R, Z) — see **README** *Units (screening defaults)*.
- [x] Regression tests: trivial binary flash — **`FlashIsothermalFlashGoldenVectorTests`** (Wilson + RR vs external reference); existing Rachford–Rice / interior tests remain.
- [x] Avoid silent non-finite numerics — **`FlashValidator`** **`AssertFinite`** on P, T, mole-fraction sums, and component scalars (double overflow to infinity).

## Verification

- `dotnet test Beep.OilandGas.FlashCalculations.Tests`
