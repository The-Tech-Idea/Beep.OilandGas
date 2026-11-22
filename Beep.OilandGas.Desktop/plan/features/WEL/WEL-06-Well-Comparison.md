# WEL-06 — Well comparison

| Field | Value |
|---|---|
| Stage | STG-02 — Wells, wellbores and stratigraphy |
| Area | Wells |
| Status | Gate met - AC-1..AC-5: unit mismatch suppresses the delta (each cell shows its own unit), missing is "-" and never zero, export built from the displayed matrix, overlay refused with the well and the missing geometry named, selection change cancels the pending load. See [evidence](../../evidence/STG-02-WEL-06/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Wells/WellCompare/` |
| External packages | `TheTechIdea.OilGas.WellSchematic` (`WellComparison`, `ChangeOverlay`) |
| Depends on | WEL-02 |
| Blocks | — |
| Web reference | `Pages/PPDM39/CompareWells.razor` |

## 1. Purpose

Put two or more wells side by side on named properties, with units made explicit, and with the
schematic overlay from `OilandGasTools` when geometry is available.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle/Services/WellComparisonService.cs` | property comparison |
| `TheTechIdea.OilGas.WellSchematic/Comparison.cs` (`WellComparison`, `ChangeOverlay`) | schematic difference overlay |
| `PPDM.Models/39/WELL*` | source properties |

## 3. Screens

`Wells → Compare`

- Well picker (2–6 wells).
- Property matrix: rows = properties, columns = wells, unit column, difference column.
- Grouping by category (identity, geometry, completion, status).
- Schematic overlay panel (only when all selected wells have the needed geometry).
- Export to CSV/XLSX.

## 4. Data and contracts

`OgComparisonRow { PropertyKey, LabelKey, Unit, Values[], Deltas[] }`.

## 5. States

| State | Behaviour |
|---|---|
| One well selected | "select at least two" |
| Unit mismatch across wells | the cell is marked and no delta is shown |
| Missing value | shown as "—" with a tooltip, never as zero |
| No geometry | overlay panel is disabled with the reason |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Compare | `wells.read` |
| Export | `data.export` |

## 7. Acceptance criteria

- AC-1 A unit mismatch prevents a numeric delta.
- AC-2 A missing value is distinct from zero.
- AC-3 The matrix is exportable with the same rows and columns as displayed.
- AC-4 The overlay refuses wells without the required geometry and says why.
- AC-5 Selection changes cancel pending loads.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WEL-06"
```

## 9. Open questions and risks

- `WellComparisonService` API shape must be inspected before designing the matrix; it may already
  return a comparison record rather than raw properties.
