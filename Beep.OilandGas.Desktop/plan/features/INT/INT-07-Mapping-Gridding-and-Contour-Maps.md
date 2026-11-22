# INT-07 — Mapping, gridding and contour maps

| Field | Value |
|---|---|
| Stage | STG-17 — Data interop, mapping and 3D |
| Area | Interop |
| Status | Gate met - see [evidence](../../evidence/STG-17-INT-07/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Mapping` |
| Desktop project | `Modules/Interop/Mapping/` |
| External packages | `TheTechIdea.OilGas.Mapping` |
| Depends on | VSB-01, WEL-05 |
| Blocks | INT-08 |
| Web reference | `OilandGasTools/docs/customer/mapping.html`; Web `Components/Visualization/FieldMap.razor`, `Components/Shared/PPDMMapView.razor` |

## 1. Purpose

Turn well values into a map: structure, isochore, net pay, bubble maps, with correct projections and
contours — and export it.

## 2. Source components

| Source | Reuse |
|---|---|
| `Mapping/Gridding/Gridding.cs` | `Run(points, method, spec)` |
| `Mapping/Gridding/Kriging.cs` | `Predict(data, variogram, x, y, neighbours)` |
| `Mapping/Gridding/Contouring.cs` | `Levels`, `Interval`, `NiceInterval`, `Bands`, `Trace` |
| `Mapping/Gridding/Grid.cs`, `GridQc.cs`, `ContourBands.cs` | grid primitives |
| `Mapping/Wells/WellMaps.cs` | `Structure`, `Thickness`, `NetPay`, `Values` |
| `Mapping/Wells/MapPoints.cs` | points and bubbles |
| `Mapping/CoordinateSystems/*` | projections and geodesy |
| `Mapping/Rendering/ContourMapView.cs`, `LabelLayout.cs`, `MapLabelBox.cs` | rendering and labels |
| `Mapping/MapExport.cs` | PDF / PNG / SVG |
| `Beep.OilandGas.HeatMap/Rendering/HeatMapRenderer.cs` | optional heat-map layer (ADR-006) |

## 3. Screens

`Data → Mapping`

- Data tab: value source (structure top, isochore, net pay, custom expression), well set, unit.
- Projection tab: coordinate system, datum, ellipsoid, axis names from `AxesHelp`.
- Gridding tab: method (minimum curvature, inverse distance, kriging), cell size, variogram.
- Display tab: contour interval, index contours, colour ramp, labels, wells, bubbles, scale bar,
  north arrow, legend.
- Main area: `ContourMapView` inside `SkiaViewSurfaceHost`.
- QC panel: `GridQc` output — empty cells, extrapolation distance, outlier wells.
- Data table: well, X, Y, value, used/excluded.

## 4. Data and contracts

`OgMapRequest { ValueKind, Wells[], Projection, GridSpec, Display }`
`OgMapQc { EmptyCells, MaxExtrapolation, OutlierWells[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Fewer than three wells | map disabled with the rule |
| Missing coordinates | those wells are excluded and listed |
| Value missing for a well | excluded and counted |
| Projection undefined | refused, projection required |
| QC outliers | shown, exclusion is the user's decision |
| Export failure | retained request, failure reported |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Build map | `mapping.read` | no |
| Save map | `mapping.write` | yes |
| Export | `data.export` | file |

## 7. Acceptance Criteria

- AC-1 A three-well structure map renders with a legend and scale bar.
- AC-2 Projection round-trips: WGS-84 → projection → WGS-84 within tolerance.
- AC-3 Wells without coordinates are excluded and listed.
- AC-4 QC reports empty cells and maximum extrapolation distance.
- AC-5 Contour intervals come from `NiceInterval` unless overridden.
- AC-6 PDF export paginates and includes legend, scale bar and north arrow.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~INT-07"
```

Port vector assertions from `OilandGasTools/tests/TheTechIdea.OilGas.Mapping.Tests/vectors/*.json`.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-17-INT-07/observed-evidence.json).

**Every geometric step is the tools'.** A pick is placed where the well really is — the wellhead **projected**, plus the trajectory's
offset at the pick's measured depth — by `WellMaps.Structure` (one top) or `WellMaps.Thickness` (two tops); the surface is
`Gridding.Run`, the levels are `Contouring`'s, the bands are `ContourBands`', and the drawing and the three exports are
`ContourMapView` and `MapExport`.

| What the page shows | Where it comes from |
|---|---|
| The map, with legend, scale bar and north arrow | `ContourMapView`, hosted in the Skia surface host |
| Wells and their values | `WellMapSet.Rows` and `Points()`, as wells and bubbles |
| Grid quality | the gridding result's own `GridQc` |
| Contour levels and interval | `Contouring`, and which interval was used |
| Projections, with their round-trip error | `CoordinateSystems` and each system's own two methods |
| The PDF, PNG or SVG | `MapExport` |

**AC-2 exposed a real gap.** `mapping.coordinateSystems` is an **empty dictionary by default**, so `DefinedNames()` returns nothing
and **no map could be drawn at all** — the feature's "projection undefined, refused" state would have been the only reachable one. The
projection list now adds **the UTM zone each wellhead falls in** (`utm:<zone><N|S>`), which the engine generates from its own
definition and needs no configuration. A field straddling a zone boundary is offered **both**, so neither half is mapped in the other's
zone, and a customer's national grid appears beside them once it is defined in the environment.

**The round trip is measured, not asserted.** Each system is taken through `FromGeographic` and back through `ToGeographic` at the
wells' own positions, the differences are converted to **metres at each latitude**, and a system that returns a point further than
**0.05 m** is marked unusable — with the figure on the page so the choice can be seen.

**AC-3 is the engine's own exclusion.** The test fixture's fourth well has a survey and a pick but **no wellhead position**, and it is
listed **"✗ Alpha 4: no wellhead latitude and longitude"** with X and Y at zero — **not given a made-up coordinate** to keep it on the
map. Deleting a placed well's pick then excludes it with **"no top named 'Hod'"** while the others stay.

**Two type findings shaped the page.** `Contouring.Levels` returns **(Level, IsIndex) pairs** — the service keeps the levels and passes
the interval to the view, which re-derives the index flags from the same setting, so the heavier lines cannot disagree between the
table and the drawing. And **`MapWell` carries only a label and a position** — the value belongs to a **`MapBubble`**, which is exactly
the bubble map the feature asks for beside the contours.

**A structure map needs the datum elevation**, because it is drawn in tvdss by default: `WELL.DEPTH_DATUM_ELEV` is read with its unit
and added to the header, and a well without one is excluded with the engine's reason rather than mapped at an invented subsea depth.

**Two wells is not a surface.** Fewer than three placed wells yields a workspace with `MapPossible = false`, no view and a sentence
naming the count — the feature's own "map disabled with the rule" state, rather than a failed read that would lose the well table.

**The open question is answered by the engine.** `LabelLayout` already places every label — inside the frame, clear of the symbols and
of the labels already placed — because the page hosts the view rather than drawing one. Overlap is **not separately measured** on the
page, and that is recorded as a limit rather than claimed as a measurement that was not made.

**Not built, and stated as gaps:** net pay and arbitrary per-well values (the pay-zone intervals are PAY's data and are not read
here); a **saved map** (this build writes nothing but the export); a per-well exclude toggle (outliers are shown, and the feature says
exclusion is a person's decision); kriging's standard-deviation grid; label-overlap measurement; and paging.

## 9. Open questions and risks

- Label placement can be poor with dense wells; `LabelLayout` exists — use it and measure label
  overlap rather than eyeballing.
