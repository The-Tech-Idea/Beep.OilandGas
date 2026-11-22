# `src/` — solution root

The solution is created in `STG-00`. This file records what belongs here so nothing is invented
later. Full detail: [`../plan/03-SOLUTION-LAYOUT.md`](../plan/03-SOLUTION-LAYOUT.md).

## Projects

| Project | Output | TFM | Purpose |
|---|---|---|---|
| `Beep.OilandGas.Desktop` | WinExe | `net10.0-windows` | Host: `App`, startup, shell, setup, sign-in, theme, localization |
| `Beep.OilandGas.Desktop.Core` | library | `net10.0` | Contracts, results, session, page catalog, permissions |
| `Beep.OilandGas.Desktop.Data` | library | `net10.0` | Connection/module administration, PPDM adapters, local composition |
| `Beep.OilandGas.Desktop.Controls` | library | `net10.0-windows` | Shared WPF controls and the SkiaSharp view host |
| `Beep.OilandGas.Desktop.Modules` | library | `net10.0-windows` | One folder per area: Views + ViewModels |
| `Beep.OilandGas.Desktop.WellTools` | library | `net10.0-windows` | `OilandGasTools` integration: environment, mapping, export, toolboxes |

## Building

```powershell
dotnet build Beep.OilandGas.Desktop.slnx -c Debug -p:SkipMinVer=true
```

Two facts about the build:

- **`OilGasToolsRoot`** (ADR-008). `WellTools` references the sibling `OilandGasTools` checkout because
  the 13 `TheTechIdea.OilGas.*` packages are not published. Default is
  `..\..\..\OilandGasTools`; override it when the checkout lives elsewhere.
- **`-p:SkipMinVer=true`**. That checkout versions itself with MinVer, which needs `git` on PATH. With
  git available the flag is unnecessary; without it the tools projects fail with `MINVER1007`. The
  desktop's own version comes from `OgProductVersion`, so skipping MinVer changes nothing here.

## Reference direction

```
Host → Core, Data, Controls, Modules, WellTools
Modules → Core, Controls
WellTools → Core, Controls
Data → Core
Controls → Core
Core → (nothing desktop)
```

Forbidden: `Modules → Data`, `WellTools → Modules`, `Controls → Modules`, `Core → any WPF`.

## Rules

1. `Core` and `Data` must not reference WPF.
2. Only `Data` may reference `IDMEEditor`, `UnitofWork<T>`, `IPPDMGenericRepository` or driver types.
3. Only `WellTools` and `Controls` may reference SkiaSharp types.
4. `Directory.Build.props` here imports the repository one, so `BEEP0001..0004` analyzers and
   `CS8509` apply to every project.
5. Central package versions live in `Directory.Packages.props`; no version in a `.csproj`.

## First files to create (STG-00)

1. `Directory.Build.props`, `Directory.Packages.props`
2. `Beep.OilandGas.Desktop.slnx`
3. `Beep.OilandGas.Desktop.Core` with `OgResult`, `OgModuleKeys`, `OgPageDefinition`,
   `OgModuleCatalog`
4. `Beep.OilandGas.Desktop` with `App.xaml`, an empty `OgShellWindow`, and the `--smoke` verb
5. `Beep.OilandGas.Desktop.Controls` with `PageFrame` and `StatePresenter`
6. `Beep.OilandGas.Desktop.Modules` with `Foundation/`
7. `Beep.OilandGas.Desktop.Data` with `OgConnectionCatalog`
8. `Beep.OilandGas.Desktop.WellTools` — created but not referenced by the host until ADR-001,
   ADR-002 and ADR-008 are accepted.
