# `test/` — test projects

Three projects, created in `STG-00`. Rule for the whole programme: **run focused tests only.**
Never run the full suite, never re-run automatically, and never claim a broad regression from a
focused result.

## Projects

| Project | TFM | Scope |
|---|---|---|
| `Beep.OilandGas.Desktop.Tests` | `net10.0-windows` | Contracts, catalog, route guard, localization keys, view-model logic with substituted services |
| `Beep.OilandGas.Desktop.IntegrationTests` | `net10.0-windows` | Local composition against a real SQLite PPDM database; module setup; PPDM read/write round-trips |
| `Beep.OilandGas.Desktop.SmokeTests` | `net10.0-windows` | Drives `--smoke`; constructs every catalog page once |

## Naming

`<FEATURE-ID>/T<NN>` — for example `LFT-03/T03`. The test method name starts with the same token
so a focused run is a single filter:

```
dotnet test test/Beep.OilandGas.Desktop.Tests `
  --filter "FullyQualifiedName~LFT-03_T03"
```

## What a test must prove

| Kind | Proves | Does not prove |
|---|---|---|
| Unit | A rule, a guard, a state transition, a refusal | Anything about a real database, provider or browser |
| Integration | The adapter works against a real SQLite PPDM database | Other providers, concurrency, latency |
| Smoke | Every page constructs and the headless run completes | Business correctness |
| Manual (recorded) | A pixel, a layout, an export file, an RTL mirror | Repeatability |

Every test result recorded as evidence must state which of these it is, and what it does **not**
prove. `observed-evidence.json` has a mandatory `limits` field for exactly this.

## Guard tests worth writing early

| Guard | Why |
|---|---|
| Every catalog route has exactly one view factory case | Prevents a navigable dead end |
| Every catalog route has an icon resource | Prevents the "missing icon" regression the ERP avoids with `nav.<route>.svg` |
| No user-visible literal in `Modules/**/*.xaml` or `*.cs` without `Loc.Get` | Prevents localization drift |
| No `#RRGGBB` or `Color="..."` in `Modules/**/*.xaml` | Enforces theme tokens |
| No `catch` without a report or rethrow | Enforces the failure rule (in addition to the analyzer) |
| No `catch` whose body is only a log call | The skill's rule: reporting is not logging |
| `IOgToastService` is injected only into `OgNotificationHub` | Keeps one message pipeline |
| No `MessageBox.Show` outside `Controls/Notifications/Dialogs` and the startup dialog | Keeps one visible surface |
| Every `OgToast` with `Kind == Failure` has a non-empty reference | Guarantees a copyable reference |
| A sentinel exception message never appears in a toast, panel or result | Proves redaction |
| Every writing command writes exactly one audit row | Proves audit cardinality |
| No writable path resolves under `AppContext.BaseDirectory` | Keeps the failure store, log and local database out of the install folder |
| `OgPaths` is the only type that composes a writable path | Prevents a second, wrong location appearing later |
| Every `*.csproj` has no `PackageReference` with `Version` | Enforces central package management |
| Every view model has a parameterless-free constructor taking interfaces only | Prevents a view model reaching for a concrete DB |

## Reference material for assertions

The `OilandGasTools` test projects already contain reference vectors:

- `tests/TheTechIdea.OilGas.Mapping.Tests/vectors/*.json` — geodesic, projection, kriging
- `tests/TheTechIdea.OilGas.Tests/IscwsaConformanceTests.cs` — survey uncertainty
- `tests/TheTechIdea.OilGas.Tests/CasingDesignTests.cs` — API 5C3
- `tests/TheTechIdea.OilGas.Tests/ProductionToolboxTests.cs` — decline, Vogel, nodal
- `tests/TheTechIdea.OilGas.Production.Tests/*` — Horner, vertical lift

Port the specific assertions; do not re-derive the expected values.
