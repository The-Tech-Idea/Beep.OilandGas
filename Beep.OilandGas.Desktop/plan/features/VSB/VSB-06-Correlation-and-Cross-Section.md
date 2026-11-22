# VSB-06 — Correlation and cross-section

| Field | Value |
|---|---|
| Stage | STG-15 — Wellbore visual suite |
| Area | Visuals |
| Status | Gate met - three-well correlation, datum flattening, missing-datum and pinch-out checks, coordinate cross-section, paginated PDF and a render cancelled between wells. See [evidence](../../evidence/STG-15-VSB-06/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore` (`Correlation`) |
| Desktop project | `Modules/Wells/Correlation/` |
| External packages | `TheTechIdea.OilGas.Wellbore` |
| Depends on | VSB-03, WEL-05 |
| Blocks | INT-07 (maps share coordinate handling) |
| Web reference | none |

## 1. Purpose

Put several wells side by side, align them on a shared marker or datum, and cut a geological
cross-section through them.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore/Correlation/CorrelationPanel.cs`, `CorrelationView.cs` | correlation |
| `Wellbore/Correlation/CrossSection.cs`, `CrossSectionView.cs` | section |
| `Wellbore/Correlation/AntiCollisionViews.cs` | related multi-well visuals |
| `Wellbore/Correlation/WellMap.cs` | well position map |
| `Wellbore/Output/PanelExport.cs` | `ToPdf(CorrelationPanel…)`, `ToPdf(CrossSectionPanel…)`, `CorrelationPages` |
| `Wellbore/MarkerMatching.cs`, `CorrelationChecks.cs`, `CorrelationLayout.cs`, `SectionGeometry.cs` | alignment and geometry |

## 3. Screens

`Wells → Correlation`

- Well picker (2–20 wells) with a map inset showing positions.
- Datum selector: a marker, a stratigraphic top, or a chosen depth.
- Main area: `SkiaViewSurfaceHost` hosting `CorrelationPanel` / `CrossSectionView`.
- Marker panel: which markers to show, colours, lock state.
- Alignment panel: per-well datum shift with a reset.
- Checks panel: pinch-outs, missing tops, crossing correlations.
- Export: multi-page PDF.

## 4. Data and contracts

`OgCorrelationSet { Wells[], Datum, Markers[], Shifts{} }`
`CorrelationChecks` provides the warnings; the desktop surfaces them.

## 5. States

| State | Behaviour |
|---|---|
| A well has no tops | that well is shown without markers and listed |
| Datum missing in a well | that well is offset and marked |
| Pinch-out detected | shown as a warning with the interval |
| Only one well | correlation is disabled with the reason |
| Export failure | retained original document, failure reported |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `wells.read` | no |
| Save alignment | `wells.interpretation.write` | yes |
| Export | `data.export` | no |

## 7. Acceptance criteria

- AC-1 Three wells align on a chosen marker.
- AC-2 A well missing the datum is marked and offset, not silently aligned.
- AC-3 Pinch-outs and missing tops appear in the checks panel.
- AC-4 A cross-section uses the real well positions when coordinates exist.
- AC-5 PDF export paginates correctly for the panel width.
- AC-6 Changing the well set cancels any pending render.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~VSB-06"
```

## 9. Open questions and risks

- Coordinate systems: use `TheTechIdea.OilGas.Mapping.CoordinateSystems` for the map inset so the
  section geometry and the map agree.

### Delivered 2026-11-05

- **Route `correlation`** (en + ar): datum picker (shared tops + none), **Reset alignment**, **Export PDF**,
  a checks grid, and the correlation panel hosted in `SkiaViewSurfaceHost`.
- `OgCorrelation` builds the document set and delegates every visual decision: `CorrelationPanel` for the
  wells and `FlattenOn`, `CorrelationLayout.Build` for the offsets, `CorrelationRenderer.Draw` for the
  picture, `PanelExport.CorrelationPages`/`ToPdf` for pagination.
- **Deterministic demo set:** DEMO-1 and DEMO-2 carry `Top B` at 8200 and 8230 ft; **DEMO-3 carries tops but
  not the datum** — the case AC-2 exists for.
- **AC-1** after flattening, DEMO-1's and DEMO-2's datum land on the same axis position (asserted to one
  decimal) while their offsets differ, because the marker sits at a different depth in each.
- **AC-2** DEMO-3 is named by `correlation-datum-missing`, reports `HasDatum=false`, keeps an offset of
  exactly zero and has **no** computable datum position — the view does not pretend it is aligned.
- **AC-3** a well with no markers is listed as `correlation-no-tops` instead of drawn blank, and
  **pinch-outs are now reported** (2026-11-06): `CrossSectionLayout.Horizons` exposes its terminations, so
  `SectionChecks` reads every `HorizonTermination.PinchOut` and names the horizon and both wells. The demo set
  produces **3**, and a control where every well carries every top produces none.
- **AC-4** the cross-section is built from the wells' **positions** and drawn in its own tab (2026-11-06):
  `BuildSection` places each well at its X/Y, draws the line through the outermost two, and
  `CrossSectionLayout.Paths` projects the wells onto it at true distance (0 / ~1516 / ~3245 ft for the demo
  positions). A well without a position is **refused** (`correlation-no-coordinates`) rather than drawn at
  zero.
- **AC-5** the planner's page count equals the exporter's, and the file starts with `%PDF-`.
- **AC-6** the render is **chunked and cancellable** (2026-11-06): `RenderChunked` checks a token between
  wells, before the frame and after it, so a cancel is observed at the next well boundary and a cancel during
  the draw discards the frame. The page holds a `CancellationTokenSource`, cancels it when a new render
  starts, and returns nothing for a superseded render, so a stale panel can never be applied. A cancel after
  the first well is observed (progress stops at 1), a pre-cancelled token does no work and produces no frame,
  and a 120-well render superseded by a 3-well render returns null for the first and the 3-well panel for the
  second.

### Still open

- A well picker (2-20 wells), the map inset, and the per-well shift editor are not built.
- The demo positions are deterministic fixtures, not coordinates read from PPDM (DAT-02).
- The draw itself is still one `CorrelationRenderer.Draw` call: the token is honoured at well boundaries and
  around the frame, not inside a single draw. A cancel that arrives during that call discards the frame.
