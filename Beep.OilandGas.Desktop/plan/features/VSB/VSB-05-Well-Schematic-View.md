# VSB-05 — Well schematic view

| Field | Value |
|---|---|
| Stage | STG-15 — Wellbore visual suite |
| Area | Visuals |
| Status | Gate met - AC-1 to AC-6, including a desktop overlay that hatches unverified barriers. See [evidence](../../evidence/STG-15-VSB-05/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.WellSchematic` |
| Desktop project | `Modules/Wells/WellSchematic/`, `WellTools/Bridging/OgSchematicMapper.cs` |
| External packages | `TheTechIdea.OilGas.WellSchematic` |
| Depends on | VSB-03 |
| Blocks | VSB-08, DEC-02, DEV-07 |
| Web reference | none |

## 1. Purpose

The engineering picture of a well: casing strings, completion, packers, barriers, annuli, leak paths
and abandonment state — drawn from PPDM tubular and component records.

## 2. Source components

| Source | Reuse |
|---|---|
| `WellSchematic/WellSchematicView.cs` | view (`Schematic`, `Track`, `LastPlan`, `ValidateSchematic`) |
| `WellSchematic/WellModelConverter.cs` | `ToSchematic`, `ToWellboreView`, `ApplyEdits`, `SymbolFor` |
| `WellSchematic/Models/WellSchematicModels.cs` | document model |
| `WellSchematic/Layout/*` (`WellSchematicLayoutEngine`, `SchematicRenderPlan`, `RadialMap`) | layout |
| `WellSchematic/Rendering/*` (`WellSchematicRenderer`, `SymbolRenderer`, `BarrierOverlay`) | rendering |
| `WellSchematic/Validation/WellSchematicValidator.cs` | validation |
| `WellSchematic/Tally.cs` | tally |
| `WellSchematic/Annuli.cs`, `Barriers.cs`, `LeakPaths.cs`, `Abandonment.cs`, `Comparison.cs` | tracks |
| `WellSchematic/CasingDesignTrack.cs`, `WellPath.cs` | casing and path tracks |
| `WellSchematic/Catalog/SymbolCatalog.cs` | symbols |
| `WellSchematic/Resources/*.json` | symbol and barrier template data |

## 3. Screens

`Wells → Schematic`

- Toolbar: well/wellbore, scheme version, toggle casing / completion / barriers / annuli / leak paths /
  abandonment, tally toggle, fit, export.
- Main area: `SkiaViewSurfaceHost` hosting `WellSchematicView`.
- Annulus panel: each annulus with MAASP and current pressure when available.
- Barrier panel: element, interval, status, evidence.
- Leak path panel: path, severity, status.
- Validation panel: `WellSchematicValidator` findings.
- Tally tab: joint-level length and grade list.

## 4. Data and contracts

`OgSchematicMapper.ToDocument(well, tubulars, components, barriers) → (WellSchematicDocument,
warnings)`. Missing component types get a generic symbol and a warning, never a blank.

## 5. States

| State | Behaviour |
|---|---|
| No tubulars | schematic shows the well path only, with a notice |
| Unknown component type | generic symbol plus a warning listing the type |
| Barrier evidence absent | barrier drawn hatched, marked unverified |
| Validation errors | listed in the panel; drawing continues |
| Render exception | failure panel |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `wells.read` | no |
| Edit schematic layout | `wells.schematic.write` | yes |
| Edit component symbols | `settings.wellbore.write` | yes |

## 7. Acceptance criteria

- AC-1 A well with casing and completion renders both, with correct depths.
- AC-2 Annuli show MAASP when the inputs exist, and "not computed" when not.
- AC-3 An unverified barrier is visually distinct from a verified one.
- AC-4 An unknown component type produces a warning, not a blank.
- AC-5 Validation findings are listed with the offending element.
- AC-6 The tally matches the casing record lengths.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~VSB-05"
```

## 9. Open questions and risks

- The schematic is the product's most persuasive screen. Invest in the mapping quality before adding
  any other visual feature.

### Delivered 2026-11-05

- **Route `schematic`** (en + ar) hosting `WellSchematicView` in `SkiaViewSurfaceHost`, with Fit and Reload,
  and four panels: **Annuli**, **Barriers**, **Validation** and **Tally**.
- `OgSchematicMapper` is the whole mapping: strings, annuli, barriers, findings, warnings, tally and the
  summary. Nothing on the page computes a well fact itself.
- **AC-1** the tools' gas-lift producer has 7 strings with ordered depths, including casing and tubing; the
  smoke run builds the schematic track and renders it.
- **AC-2** each annulus reports its MAASP with unit or the exact **`not computed`**, plus the screening
  result — never a blank cell.
- **AC-4** an unknown component type produces `unknown component type 'X' — a generic symbol is drawn`, and
  the well still renders; the demo document itself produces no warnings, so the check is exercised by a
  deliberately unknown row rather than by a broken demo.
- **AC-5** a string whose bottom is above its top produces a finding whose `ItemId` is that string.
- **AC-6** the tally is `TallyBuilder.CasingProgram`: one row per included string, `RowIds` are the string
  ids, and its `top`/`bottom` cells equal the strings' own depths.
- **AC-3 delivered by a desktop overlay.** The tools' `BarrierOverlay` draws a cross for `Failed` and nothing
  for `Unverified`, so `OgSchematicBarrierOverlay` now draws a hatched amber band over each unverified
  interval. The host gained `Overlay`, invoked over the finished surface before the snapshot, so the mark is
  drawn on top of the schematic rather than the schematic being reimplemented. Proven with **one** renderer
  instance: a no-op overlay leaves the frame byte-identical, the hatch changes it, and giving every barrier
  evidence yields no marks. Scope: only elements with a resolvable interval are hatched (1 of the demo's 12);
  the rest carry no interval and target nothing with geometry, and every unverified barrier is still named
  in the panel and counted in the summary.

### Still open

- Barrier/annulus/leak-path toggles, the export command and the tally CSV export.
- The production barrier template ships without verification evidence, so the demo's barriers all read
  "no evidence recorded"; a verified example would need a document that carries evidence.
- The hatch is a depth-aligned margin band rather than a box over the casing column: the committed
  `SchematicRenderPlan` exposes no usable column or depth mapping after the draw (its depth area collapses
  to 1 px), so the band uses the canvas bounds and the document's own depths. Drawing inside the column
  needs the layout plan made available after a render.
