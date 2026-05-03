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

- [ ] Single **units** table in **README** (P, T, R, Z).
- [ ] Regression tests: trivial binary flash, near-critical behavior (smoke).
- [ ] Avoid silent **`NaN`** — **`FlashValidator`** on inputs.

## Verification

- `dotnet test Beep.OilandGas.FlashCalculations.Tests`
