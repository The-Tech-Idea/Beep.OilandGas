# Phase 3 — Documentation, packaging, and contracts

## Objectives

1. Add **`README.md`** (packaged if **`PackageReadmeFile`** is set — mirror **`GasProperties`**).
2. Clarify **scope**: black-oil screening vs EOS / compositional; link **`04`** for applicability.
3. Resolve **duplicate** **`IOilPropertiesService`** in **`Beep.OilandGas.Properties`** vs **`Models.Core.Interfaces`** (document which is authoritative; deprecate or align).

## TODO checklist

| # | Task | Target |
|---|------|--------|
| 3.1 | Add **`README.md`**: quick start, units, correlation list, link **`.plans`**, **`MASTER-TODO-TRACKER.md`**. | `Beep.OilandGas.OilProperties/README.md` |
| 3.2 | Set **`<PackageReadmeFile>README.md</PackageReadmeFile>`** + **`<None Include="README.md" Pack="true" .../>`** if shipping NuGet. | `Beep.OilandGas.OilProperties.csproj` |
| 3.3 | Optional **`IMPLEMENTATION_SUMMARY.md`** — inventory, DI registration pointer (`Program.cs`), known limitations. | New file |
| 3.4 | XML doc on **`IOilPropertiesService`** for **temperature/pressure units** and **correlation** string values. | `Beep.OilandGas.Models` |
| 3.5 | Remove unused **`Beep.OilandGas.GasProperties`** reference if still unused after audit. | `.csproj` |

## Verification criteria

- [ ] README builds with no broken relative links.
- [ ] `dotnet pack` (if enabled) includes readme without warning.

## Exit criteria

README + tracker + interface XML updated; packaging aligned with **`GasProperties`** pattern.
