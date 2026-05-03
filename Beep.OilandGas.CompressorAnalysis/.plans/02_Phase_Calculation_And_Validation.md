# Phase 2 — Calculation engine and validation

## Goal

Harden **numerical consistency**, **unit clarity** (psia, °R, Mscf/d, HP), **validation coverage**, and remove **technical debt** (obsolete backups, dead code paths).

## Target files

- `Beep.OilandGas.CompressorAnalysis/Calculations/*.cs`
- `Beep.OilandGas.CompressorAnalysis/Validation/CompressorValidator.cs`
- `Beep.OilandGas.CompressorAnalysis/Data/Constants/` — e.g. **`CompressorConstants.cs`**, seed payloads alongside **`CompressorAnalysisReferenceCodeSeed`**
- Legacy **`Beep.OilandGas.Models/Data/CompressorAnalysis/*.bak`** — removed with module relocation; no **`*.bak`** should remain under **`CompressorAnalysis`**

## TODO checklist

- [ ] Document **unit contract** in **`README.md`** (single source: suction/discharge pressure psia, suction temperature °R for thermodynamic paths, flow in **Mscf/day** where used).
- [ ] Cross-check **`CentrifugalCompressorCalculator`** vs **`ReciprocatingCompressorCalculator`** for shared constants (**k**, MW from **GAS_SPECIFIC_GRAVITY**, mechanical efficiency application order).
- [ ] **`CompressorPressureCalculator`**: confirm **`CalculateRequiredPressure`** / **`CalculateMaximumFlowRate`** bounds and exceptions align with **`CompressorValidator`**.
- [ ] **`MultistageCompressor`** / **`AdvancedCompressorCalculator`** / **`CompressorOptimization`**: identify dead code or wire into **`CompressorAnalysisService`** with explicit feature flags.
- [x] Delete legacy **`Models/Data/CompressorAnalysis/*.bak`** (completed with relocation); scan **`Beep.OilandGas.CompressorAnalysis`** for stray **`*.bak`** if any appear in PRs.
- [ ] Align **`IMPLEMENTATION_SUMMARY.md`** with actual file list (remove references to legacy **`Models/CompressorModels.cs`** if absent).

## Verification criteria

- `dotnet build Beep.OilandGas.CompressorAnalysis`
- Manual spot-check: one centrifugal + one reciprocating example matches spreadsheet/order-of-magnitude (document vectors in phase 5 tests)

## Edge cases to cover (defer detailed tests to phase 5)

- Compression ratio → 1 (no work)
- Discharge ≤ suction (already rejected in validator/service)
- Efficiency outside (0,1] — validator paths
