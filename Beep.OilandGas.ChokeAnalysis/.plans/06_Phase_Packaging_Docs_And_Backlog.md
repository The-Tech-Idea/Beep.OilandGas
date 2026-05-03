# Phase 6 — Packaging, documentation, and backlog

## Goal

Ship a **consumable NuGet package** and **accurate docs**, and capture **future enhancements** without blocking current releases.

## Packaging (`Beep.OilandGas.ChokeAnalysis.csproj`)

Baseline expectations (align with sister engineering libraries):

| Setting | Purpose |
|---------|---------|
| `GeneratePackageOnBuild` | Produce `.nupkg` on build |
| `PackageReadmeFile` + packed `README.md` | NuGet gallery / restore UX |
| `PackageIcon`, license, repository URL | Metadata parity |
| `Nullable` | Enabled; avoid broad `NoWarn` |

## TODO checklist

- [ ] Confirm `README.md` at project root matches API surface (samples compile against current types).
- [ ] Trim `IMPLEMENTATION_SUMMARY.md` if outdated (file counts, “Properties” vs `GasProperties` naming).
- [ ] Verify `dotnet pack` succeeds and package contains README (inspect `.nupkg` or build log).
- [ ] `CopyPackage` / `PostBuild` targets: confirm paths still valid for team local NuGet drop.

## Documentation touchpoints

- Project **README.md** — installation, quick start, units.
- **CLAUDE.md** — only if adding repo-wide rules triggered by choke work.
- **Web** — `Beep.OilandGas.Web/.plans/Projects/Engineering/Beep.OilandGas.ChokeAnalysis.md` if UI integration is planned (cross-link only).

## Backlog (examples — prioritize with product)

See also [07_Scenarios_Best_Practices_And_Industry_Reference.md](07_Scenarios_Best_Practices_And_Industry_Reference.md) for correlation gaps (e.g. explicit **Gilbert/Ros/Baxendell** variants, **Sachdeva**/**Ashford–Pierce** subcritical paths, single-phase liquid orifice).

| Item | Notes |
|------|------|
| Cancellation tokens | Thread through `IChokeAnalysisService` + calculators |
| Multiphase breadth | Expand Multiphase partial coverage + tests; align **GLR/WC** applicability with 07 |
| Regime diagnostics | Surface **critical vs subcritical** and \(r_c\) basis in results or logs (avoid single hard-coded ratio without docs) |
| Persisted choke runs | PPDM table choice + idempotent upserts |
| Field-scoped choke history | API list endpoint + `FIELD_ID` filters |
| Calibration / provenance | Store optional **tuned \(C_D\)** or correlation id for audit (field-fit per 07) |

## Exit criteria

- [ ] Package metadata complete; no “missing readme” warnings from pack.
- [ ] Backlog items triaged (in module `TODO.md` or issue tracker).

## References

- Root `README.md` in this project.
- `NuGet` packaging patterns used in `Beep.OilandGas.NodalAnalysis`, `Beep.OilandGas.Models`.
