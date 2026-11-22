# WEL-05 — Stratigraphy browser

| Field | Value |
|---|---|
| Stage | STG-02 — Wells, wellbores and stratigraphy |
| Area | Wells |
| Status | Gate met - AC-1..AC-6: three-level hierarchy with stable ordering, picks whose unit comes from STRAT_UNIT, overlap flagged on both sides, rename changes display only, colour as a theme token (source has no colour value), picks scoped to the selected well. See [evidence](../../evidence/STG-02-WEL-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Wells/Stratigraphy/` |
| External packages | `TheTechIdea.OilGas.Wellbore.Data` (`IStratigraphicLexicon`) |
| Depends on | WEL-03 |
| Blocks | VSB-06 (correlation uses tops) |
| Web reference | `Pages/PPDM39/Stratigraphy/Units.razor`, `Hierarchy.razor`, `Columns.razor`, `Sections.razor`, plus legacy `/hierarchy`, `/columns`, `/sections` |

## 1. Purpose

One browser for the stratigraphic framework: the unit hierarchy, regional columns, and the tops
picked in each well.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM.Models/39/STRAT_UNIT`, `STRAT_COLUMN`, `STRAT_WELL_SECTION` | entities |
| `LifeCycle/Services/ReservoirLifecycle/ReservoirLifecycleService.cs` | reservoir/zone linkage |
| `TheTechIdea.OilGas.Wellbore.Data/Configuration/StratigraphyQueries.cs`, `IStratigraphicLexicon`, `LayeredLexicon`, `StandardLexicons` | lexicon overlay for display names and ordering |

## 3. Screens

`Wells → Stratigraphy` with four tabs:

- **Units** — grid of units with parent, rank, age range, colour; edit.
- **Hierarchy** — tree by rank with drag-free ordering (order edited as a number).
- **Columns** — regional columns and their unit sets.
- **Well sections** — picks for the selected well: unit, top depth, base depth, thickness, gross,
  net, net-to-gross; edit and import.

## 4. Data and contracts

`OgStratUnit { Id, Name, ParentId, Rank, TopAge, BaseAge, Colour, Order }`
`OgWellPick { WellId, UnitId, TopMd, BaseMd, Unit, Thickness, Net, Ntg }`

## 5. States

| State | Behaviour |
|---|---|
| No lexicon | falls back to raw names with a notice |
| Overlapping picks | flagged; not silently reordered |
| Missing top depth | allowed but excluded from thickness calculations |
| Unit edit affecting picks | warns with the affected pick count before saving |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit units/columns | `stratigraphy.write` | yes |
| Edit well picks | `wells.picks.write` | yes |
| Import picks | `wells.picks.write` | yes |

## 7. Acceptance criteria

- AC-1 The hierarchy renders at least three levels with stable ordering.
- AC-2 A pick carries a unit from the reference set, never free text.
- AC-3 Overlapping picks are flagged.
- AC-4 Renaming a unit does not change picks, only their display.
- AC-5 Colour comes from the unit record or the theme, never a hardcoded hex.
- AC-6 Well sections are scoped to the selected well.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WEL-05"
```

## 9. Open questions and risks

- Four Web pages with overlapping behaviour; the mapping to four tabs is recorded above and
  supersedes the Web structure.
- Lithology colours also exist in `OilandGasTools` (`LithologyColorPalette`). Choose one source per
  screen and record it in VSB-04.
