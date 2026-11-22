# VSB-02 — Environment, toolboxes and configuration

| Field | Value |
|---|---|
| Stage | STG-15 — Wellbore visual suite |
| Area | Visuals |
| Status | Gate met — register-before-load ordering, findings, country preset, reload, per-toolbox usability. See [evidence](../../evidence/STG-15-VSB-02/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Data` |
| Desktop project | `src/Beep.OilandGas.Desktop.WellTools/Environment/` |
| External packages | `TheTechIdea.OilGas.Wellbore.Data`, `Wellbore.Setup` |
| Depends on | VSB-01 |
| Blocks | every VSB, ENG, INT feature |
| Web reference | `OilandGasTools/docs/customer/configuring.html`, `onboarding.html`, `countries.html` |

## 1. Purpose

Load the customer's wellbore environment correctly and in the correct order, so every view, unit and
threshold behaves as configured — and prove which environment is active on every screen.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Data/Configuration/WellboreEnvironment.cs` | `Global`, `Current`, `Load`, `FromJson`, `Validate`, `Template`, `Provenance`, `ContrastRatio` |
| `Wellbore.Data/Configuration/Contracts.cs` (`WellboreServices`) | `Load`, `Global`, `Scope`, `Get<T>`, `Set<T>`, `Issues`, `Warnings` |
| `Wellbore.Data/Configuration/Toolboxes.cs` (`ToolboxRegistry`) | `Register`, `EffectiveSettings`, `CheckResources` |
| `Wellbore.Data/Configuration/CountryPresets.cs` | 62 country presets, `Load`, `TryGet` |
| `Wellbore.Data/Configuration/ConfigurationArchive.cs`, `ConfiguredFiles.cs`, `EnvironmentReport.cs` | save, resolve, diff |
| `Integrity/IntegrityToolbox.cs`, `Drilling/DrillingToolbox.cs`, `Production/ProductionToolbox.cs`, `DataAI/DataAIToolbox.cs`, `Training/TrainingToolbox.cs`, `Mapping/MappingToolbox.cs` | `Register()` |
| `Wellbore.Setup/DataSourcePlugins.cs` | plugin loading |
| `tools/setup-app/Program.cs` | the canonical startup order |

## 3. Screens

`Settings → Wellbore environment` and a status-bar indicator:

- Active environment file, source, revision, company, country.
- Validation findings (`Issues`, `Warnings`) with severity.
- Resource check results from `ToolboxRegistry.CheckResources`.
- Sections viewer using `EnvironmentReport.Flatten`, with a `Diff` against the last saved revision.
- Toolbox settings viewer (read-only unless the section is user-editable).
- "Open folder", "Reload", "Validate", "Export archive".

## 4. Data and contracts

```csharp
public sealed class OgWellboreEnvironment
{
    public string? ActivePath { get; }
    public int Revision { get; }
    public string? CountryCode { get; }
    public IReadOnlyList<OgEnvironmentFinding> Findings { get; }
    public Task<OgResult> LoadAsync(string? path, CancellationToken ct);
    public Task<OgResult> ReloadAsync(CancellationToken ct);
}
```

Startup order (mandatory, in this sequence):

1. `IntegrityToolbox.Register()`, `DrillingToolbox.Register()`, `ProductionToolbox.Register()`,
   `DataAIToolbox.Register()`, `TrainingToolbox.Register()`, `MappingToolbox.Register()`.
2. `WellboreServices.Global = WellboreServices.Load(path)` (or `new WellboreServices()`).
3. `DataSourcePlugins.FromFolder(...).Register(WellboreServices.Global)`.
4. Only now construct views.

## 5. States

| State | Behaviour |
|---|---|
| No environment file | built-in defaults with a prominent "defaults" badge |
| Validation errors | views still open; the indicator is red and lists them |
| Country preset only | shown as "preset: KW", not as a full customer environment |
| Reload with open views | offers to rebuild views; never leaves stale geometry |
| Resource missing | the affected toolbox is disabled with the reason |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Reload environment | `settings.wellbore.reload` | no |
| Edit environment | `settings.wellbore.write` | file |
| Export archive | `data.export` | file (zip) |

## 7. Acceptance criteria

- AC-1 Toolboxes are registered before the environment loads; a deliberately invalid toolbox section
  produces a validation warning (proves the ordering).
- AC-2 The active environment path is visible on every visual screen.
- AC-3 Validation findings are listed with section and key.
- AC-4 A country preset can be applied and is labelled as a preset.
- AC-5 Reload rebuilds open views and reports what changed.
- AC-6 A missing configured resource disables the dependent toolbox with the reason.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~VSB-02"
```

## 9. Open questions and risks

- **Blocked on ADR-008** (package consumption).
- `WellboreEnvironment` has ~30 sections; the viewer must be virtualised and grouped.
