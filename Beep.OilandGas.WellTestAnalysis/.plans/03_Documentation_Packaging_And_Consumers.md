# Phase 3 — Documentation, packaging, and consumers

## Objectives

1. Replace legacy **`Beep.WellTestAnalysis`** / **`Beep.WellTestAnalysis.Analysis`** samples with **`Beep.OilandGas.WellTestAnalysis`** and **`Beep.OilandGas.Models.Data.WellTestAnalysis`** in **README**, **USAGE_EXAMPLES**, and **ENHANCEMENT_PLAN** (or mark sections **Superseded**).
2. **NuGet** — Add **`PackageReadmeFile`**, pack **`README.md`** (mirror **PumpPerformance** / **GasProperties**).
3. **`.csproj`** — Deduplicate property groups if any creep in; confirm **Skia** only required for **`Rendering/`**.
4. **ApiService / Web** — Document routes that call **`IWellTestAnalysisService`**; link to **`WellServices`** / field context when tests are field-scoped.

## TODO checklist

| # | Task | Target |
|---|------|--------|
| 3.1 | Rewrite **README** quick start with **`WELL_TEST_DATA`** construction or minimal DTO sample. | `README.md` |
| 3.2 | Fix **installation** section package id (**`Beep.OilandGas.WellTestAnalysis`** vs legacy name). | `README.md` |
| 3.3 | Reconcile **`ENHANCEMENT_PLAN.md`** with implemented surface (derivative, gas, renderer). | Root |
| 3.4 | Verify **`API.md`** exists or remove link; add link to **`.plans`** and **`MASTER-TODO-TRACKER.md`**. | `README.md` |

## Exit criteria

- [ ] New developer can copy-paste samples without namespace errors.
- [ ] `dotnet pack` includes readme when packing is enabled.
