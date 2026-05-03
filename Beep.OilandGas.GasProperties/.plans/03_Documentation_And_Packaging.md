# Phase 3 — Documentation and packaging

## Goal

**`README.md`** is the public contract: correct **`using`** namespaces (**`Beep.OilandGas.Models.Data.GasProperties`**, not a non-existent **`Beep.OilandGas.GasProperties.Models`**), link **`.plans`** + **`MASTER-TODO-TRACKER.md`**, and optionally ship **`PackageReadmeFile`** on the **`.csproj`** (NuGet best practice — mirror **`Beep.OilandGas.GasLift`**).

## TODO checklist

- [ ] Fix **Quick Start** samples: replace **`Beep.OilandGas.GasProperties.Models`** with **`Beep.OilandGas.Models.Data.GasProperties`** (and **`GasComposition`** location).
- [ ] Add **Planning** subsection: link **[`.plans/README.md`](README.md)** and **[`../MASTER-TODO-TRACKER.md`](../MASTER-TODO-TRACKER.md)**.
- [ ] Add **Applicability & basis** (short form of phase **4**): single-phase gas **Z** / **μg** / **m(p)** scope; **well vs separator vs pipeline** basis; when to use **`FlashCalculations`** / EOS instead.
- [ ] Link **[`04_Industry_Scenarios_Wells_Facilities_Reservoirs.md`](04_Industry_Scenarios_Wells_Facilities_Reservoirs.md)** from **`README.md`** for full scenario matrices.
- [ ] Trim or archive **`IMPLEMENTATION_SUMMARY.md`** claims that contradict the repo (file counts / `Models/` folder under GasProperties).
- [ ] Keep **`ENHANCEMENT_PLAN.md`** as backlog; add one-line pointer from **`README.md`** for long-range correlations / Bg.
- [ ] Optional: `<PackageReadmeFile>README.md</PackageReadmeFile>` in **`Beep.OilandGas.GasProperties.csproj`** + verify **`dotnet pack`** includes readme.

## Verification

```bash
dotnet pack Beep.OilandGas.GasProperties/Beep.OilandGas.GasProperties.csproj
```

## Exit criteria

- [ ] **`README.md`** builds as copy-paste without wrong namespaces.
- [ ] Pack produces a package with readme when **`PackageReadmeFile`** is enabled.
