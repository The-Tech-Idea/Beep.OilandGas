# Phase 3 — Documentation, packaging, and API alignment

## Objectives

1. Fix **README** and any **`COMPLETE_IMPLEMENTATION_SUMMARY` / `ENHANCEMENT_PLAN`** drift: correct assembly name **`Beep.OilandGas.PumpPerformance`**, namespaces **`Beep.OilandGas.PumpPerformance.*`**, and sample `using` statements.
2. Align **NuGet** metadata: **`PackageReadmeFile`**, packed **`README.md`** (mirror **`GasProperties`** / **`OilProperties`** pattern).
3. Clean **`.csproj`**: duplicate property groups, confirm **Skia** packages are justified by **`Rendering/`** usage only.
4. **ApiService / Web** — If endpoints exist or are planned, document route → service mapping; avoid duplicating **`HydraulicPumps`** or **SRP** lift calculators in this library.

## TODO checklist

| # | Task | Target |
|---|------|--------|
| 3.1 | Rewrite **README** quick start with correct namespaces + links to **`.plans`** and **`MASTER-TODO-TRACKER.md`**. | `README.md` |
| 3.2 | Add **`<PackageReadmeFile>`** + packed readme item. | `.csproj` |
| 3.3 | Reconcile **`ENHANCEMENT_PLAN.md`** with actual code or mark sections **Done / Superseded**. | Root docs |
| 3.4 | Add **`IMPLEMENTATION_SUMMARY.md`** (short) if product wants a single executive snapshot. | Optional new file |
| 3.5 | Document **Rendering** as optional subsystem (Skia dependency). | `README.md` + `Rendering/RENDERING_IMPLEMENTATION.md` link |

## Exit criteria

- [ ] New developer can copy-paste README samples **without** namespace errors.
- [ ] `dotnet pack` produces package with readme (if packing enabled).
