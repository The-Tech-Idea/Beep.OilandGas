# STG-15 — Wellbore visual suite

| Field | Value |
|---|---|
| Stage | STG-15 |
| Features | 9 (VSB-01 … VSB-09) |
| Depends on | STG-00, STG-02 |
| Status | Gate met - VSB-01..VSB-09 gate met ([VSB-01](../evidence/STG-15-VSB-01/observed-evidence.json), [VSB-02](../evidence/STG-15-VSB-02/observed-evidence.json), [VSB-03](../evidence/STG-15-VSB-03/observed-evidence.json), [VSB-04](../evidence/STG-15-VSB-04/observed-evidence.json), [VSB-05](../evidence/STG-15-VSB-05/observed-evidence.json), [VSB-06](../evidence/STG-15-VSB-06/observed-evidence.json), [VSB-07](../evidence/STG-15-VSB-07/observed-evidence.json), [VSB-08](../evidence/STG-15-VSB-08/observed-evidence.json), [VSB-09](../evidence/STG-15-VSB-09/observed-evidence.json)). The exit gate holds. |
| Exit gate | A well from the database opens as a wellbore view, a log view and a schematic view, and exports to PDF |

---

## 1. Objective

Bring the `OilandGasTools` drawing layer into the desktop. This is the stage that makes the
product visually distinct from the Web, and it is the foundation for STG-16 (engineering
toolboxes) and STG-17 (mapping and 3D).

**Every feature in this stage is blocked until the three SkiaSharp/package decisions are
recorded with probe evidence.** Do not start coding before that.

---

## 2. Scope

**In**

- The `ViewSurface → SKElement` host control with full input forwarding and accessibility.
- Environment bootstrap: register toolboxes, load `WellboreEnvironment`, `WellboreServices`.
- Wellbore composite view: depth track, tracks, zoom/pan, cursor, markers, annotations.
- Well log view: triple combo, curve fills, pay summary track, crossplots.
- Well schematic view: casing, completion, barriers, annulus, leak paths, abandonment.
- Correlation and cross-section: multi-well correlation panel, geological cross-section.
- Well path, ladder plot and travelling cylinder (anti-collision visuals).
- Symbol gallery and schematic editing.
- Print and export: PDF, SVG, PNG, with print planning and page templates.

**Out**

- Authoring new track types (extension API exists; not a 1.0 screen).
- Live WITSML streaming (INT-02 provides a static source; a live feed is deferred).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `TheTechIdea.OilGas.Wellbore` | `Hosting.ViewSurface`, `WellboreView` (+ `.Rendering/.Editing/.Input/.Accessibility`), `CorrelationView`, `CrossSectionView`, `MapView`, `LadderPlotView`, `TravellingCylinderView`, `WellboreDocument`, `WellboreViewState`, `DepthAxis`, `DepthMapper`, `WellboreTheme`, `WellTrack`, `WellTrackRegistry`, `WellboreExport`, `PanelExport`, `PrintPlanner`, `WellboreValidator`, `EnvironmentBridge` |
| `TheTechIdea.OilGas.WellLogs` | `WellLogView`, `WellLogDocuments`, `WellLogTemplates`, `WellLogTracks`, `Crossplots`, `CrossplotView`, `CementEvaluation`, `CurveEditing` |
| `TheTechIdea.OilGas.WellSchematic` | `WellSchematicView`, `WellSchematicSymbolView`, `WellModelConverter`, `WellSchematicLayoutEngine`, `WellSchematicValidator`, `SchematicTrack`, `TallyTrack`, `BarrierTableTrack`, `LeakPathTrack`, `CasingDesignTrack`, `AbandonmentTrack`, `BarrierTemplates`, `SchematicAnnuli`, `SymbolCatalog`, `WellSchematicTemplates` |
| `TheTechIdea.OilGas.Wellbore.Data` | `WellModel`, `WellboreEnvironment`, `WellboreServices`, `ToolboxRegistry`, `CountryPresets`, `LithologyLibrary`, `Units` |
| `Beep.OilandGas.Drawing` | `DrawingEngine`, `WellRenderer`, `LogRenderer`, `EnhancedWellSchematicRenderer`, `ImageExporter`, `SvgExporter`, `PdfExporter` — the *alternative* renderer path |
| `Beep.OilandGas.LifeCycle` | Well/bore/log/tubular/top records feeding the mapping |
| `Beep.OilandGas.PPDM39` | `IPPDMGenericRepository` for well data |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| VSB-01 | SkiaSharp view host control | [VSB-01](../features/VSB/VSB-01-SkiaSharp-View-Host-Control.md) |
| VSB-02 | Environment, toolboxes and configuration | [VSB-02](../features/VSB/VSB-02-Environment-Toolboxes-and-Configuration.md) |
| VSB-03 | Wellbore composite view | [VSB-03](../features/VSB/VSB-03-Wellbore-Composite-View.md) |
| VSB-04 | Well log view | [VSB-04](../features/VSB/VSB-04-Well-Log-View.md) |
| VSB-05 | Well schematic view | [VSB-05](../features/VSB/VSB-05-Well-Schematic-View.md) |
| VSB-06 | Correlation and cross-section | [VSB-06](../features/VSB/VSB-06-Correlation-and-Cross-Section.md) |
| VSB-07 | Well path, ladder plot and travelling cylinder | [VSB-07](../features/VSB/VSB-07-Well-Path-Ladder-Plot-and-Travelling-Cylinder.md) |
| VSB-08 | Symbol gallery and schematic editing | [VSB-08](../features/VSB/VSB-08-Symbol-Gallery-and-Schematic-Editing.md) |
| VSB-09 | Print and export | [VSB-09](../features/VSB/VSB-09-Print-and-Export.md) |

---

## 5. New code introduced

| Project | Key types |
|---|---|
| `.Controls` | `SkiaViewSurfaceHost`, `ViewInputMapper`, `ViewKeyMap`, `AccessibilityPeer`, `ChartHost` |
| `.WellTools/Environment` | `OgWellboreEnvironment`, `OgToolboxBootstrap`, `OgEnvironmentStore` |
| `.WellTools/Bridging` | `OgWellModelMapper`, `OgDocumentMapper`, `OgSchematicMapper` |
| `.WellTools/Export` | `OgExportService` |
| `.Modules/Wells` (extension) | Wellbore/log/schematic/correlation/path views |

---

## 6. Architecture notes

- **`ViewSurface` is the contract; nothing is re-implemented.** Every screen is
  `new XxxView { Document = ... }` inside `SkiaViewSurfaceHost`.
- **Environment before views.** Register toolboxes → set `WellboreServices.Global` → construct
  views. Getting this order wrong silently disables validation and theming.
- **Two renderer families exist.** `OilandGasTools` (`ViewSurface`) and `Beep.OilandGas.Drawing`
  (`DrawingEngine`). Choose per screen and record why; never mix them in one canvas.
- **Mapping is explicit.** `OgWellModelMapper` maps PPDM `WELL`/`WELLBORE`/`WELL_LOG`/
  `WELL_TUBULAR` records into `WellModel`. Missing data produces warnings shown to the user, not
  a silently empty view.
- **No SkiaSharp type escapes `.WellTools` and `.Controls`.** View models hold documents and
  settings, never canvases.

---

## 7. Dependencies and sequencing

ADR-001, ADR-002 and ADR-008 probes → `VSB-01` → `VSB-02` → (`VSB-03`, `VSB-04`, `VSB-05`,
`VSB-06`, `VSB-07`) → `VSB-08`, `VSB-09`.

`VSB-01` and `VSB-02` are the only true blockers for the rest of the programme's visual work.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-15-1 | `SkiaViewSurfaceHost` forwards paint, mouse, wheel, key, pinch and pan | test `VSB-01/T02…T06` |
| SG-15-2 | `Surface.Invalidated` causes exactly one redraw, on the UI thread | test `VSB-01/T07` |
| SG-15-3 | Accessibility peer exposes name and children | test `VSB-01/T09` |
| SG-15-4 | Toolboxes register before the environment loads, proven by a validation warning being produced | test `VSB-02/T02` |
| SG-15-5 | A real well from the database renders in the wellbore view | screenshot |
| SG-15-6 | A log renders with ≥ 20 000 samples without freezing the UI | timing capture |
| SG-15-7 | A schematic renders with casing, completion and barriers | screenshot |
| SG-15-8 | Correlation of three wells aligns on a shared marker | screenshot |
| SG-15-9 | PDF export produces a paginated document with header and legend | exported file |
| SG-15-10 | Exported SVG opens in a browser and in `Svg.Skia` | file check |

---

## 9. Test plan

| Test | What it proves |
|---|---|
| `VSB-01/T02` | Paint produces non-empty pixels |
| `VSB-01/T03` | Mouse press/move/release reach the surface |
| `VSB-01/T04` | Wheel delta reaches the surface |
| `VSB-01/T05` | Key mapping covers arrows, page, home/end, +/-, tab, enter, escape, delete, Z, Y |
| `VSB-01/T06` | Pinch and two-finger pan |
| `VSB-01/T07` | Single redraw, UI thread |
| `VSB-01/T09` | Accessibility |
| `VSB-02/T02` | Toolbox-before-environment ordering |
| `VSB-03/T02` | Depth axis, track chrome, zoom |
| `VSB-04/T03` | Curve rendering and fill |
| `VSB-05/T03` | Casing and barrier track |
| `VSB-06/T02` | Marker alignment across wells |
| `VSB-07/T02` | Ladder plot separation factor |
| `VSB-08/T02` | Symbol catalogue count and filter |
| `VSB-09/T02` | PDF/SVG/PNG produced |

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| SkiaSharp unification breaks native loading | ADR-001 probe with a render assertion |
| `SkiaSharp.Views.WPF` unavailable at the unified version | ADR-002 fallback `HwndHost` |
| `OilandGasTools` package restore requires a published feed | ADR-008 probe; local feed from `dotnet pack` |
| Two renderer families confuse implementers | A decision table in `VSB-03` naming which renderer each screen uses |
| Very large logs freeze the UI | `CurveDecimation` is already provided by the tools; enforce it and measure |
