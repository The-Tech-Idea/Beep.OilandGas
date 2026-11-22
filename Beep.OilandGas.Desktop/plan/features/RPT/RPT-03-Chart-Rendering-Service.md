# RPT-03 — Chart rendering service

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Reporting |
| Status | Gate met - one renderer behind the domain's `IRenderingService`, six chart types, theme palette, "no data", reported failures, point selection and a pixel-identical export. See [evidence](../../evidence/STG-18-RPT-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Models` (`IRenderingService`) |
| Desktop project | `src/Beep.OilandGas.Desktop.Controls/ChartHost.cs`, `Modules/Reporting/Charts/` |
| External packages | `SkiaSharp` |
| Depends on | VSB-01 |
| Blocks | RPT-01 charts, PRD-01/05 charts |
| Web reference | Web renders charts to data URLs; ERP uses Skia renderers |

## 1. Purpose

One chart component for the whole product, driven by the domain's `IRenderingService`, so every KPI
and trend looks the same and the colours follow the skin.

## 2. Source components

| Source | Reuse |
|---|---|
| `Models/Core/Interfaces/IRenderingService.cs` | `RenderChartRequest` → `RenderChartResult`, persists `CHART_CONFIGURATION` |
| `Models/Data/Rendering/*` | request/result contracts |
| Per-domain renderers already in use elsewhere | `NodalRenderer`, `PumpPerformanceRenderer`, `WellTestRenderer`, `EconomicRenderer`, `HeatMapRenderer` |
| `TheTechIdea.OilGas.Mapping` `ContourMapView` | maps in reports |

## 3. Screens / API

`ChartHost` control:

```csharp
public sealed class ChartHost : ContentControl
{
    public IRenderingService? Renderer { get; set; }
    public RenderChartRequest? Request { get; set; }
    public event EventHandler<ChartPointSelected>? PointSelected;
}
```

- Uses the same `SkiaViewSurfaceHost` paint path.
- Reads palette from the theme tokens (`BeepChartBrush1..8`) at draw time.
- Optional toolbar: legend toggle, export, full screen.

## 4. Data and contracts

Chart requests come from `IRenderingService`; the desktop must not define its own chart request type.

## 5. States

| State | Behaviour |
|---|---|
| No renderer registered | placeholder with the reason |
| Empty data series | "no data" inside the chart area |
| Render exception | failure panel, chart does not blank silently |
| Skin change | palette updated on next draw |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | the host page's permission |
| Export | `data.export` |

## 7. Acceptance Criteria

- AC-1 Every chart type supported by `IRenderingService` renders.
- AC-2 Colours come from theme tokens and follow a skin change.
- AC-3 An empty series shows "no data", not an empty axis.
- AC-4 A render failure is reported, not swallowed.
- AC-5 Point selection raises an event with the series and value.
- AC-6 Exported chart matches the on-screen chart.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~RPT-03"
```

## 9. Open questions and risks

- The domain renderers and `IRenderingService` may overlap. Decide: `IRenderingService` for generic
  charts, domain renderers for purpose-built plots. Record the split here.

### Decision - the split

`IRenderingService` is for **generic** charts (line, area, bar, scatter, pie, donut) whose data is series and
labels. Purpose-built plots - nodal analysis, pump performance, well-test, economics, heat maps - stay with
their own renderers, because they encode domain geometry (pressure vs rate, pump curves) that a generic
series chart would have to fake. A purpose-built renderer can still be hosted by `ChartHost` when it draws to
the same canvas.

### Delivered 2026-11-06

- **Route `charts`** (en + ar): type picker, an **Empty series** switch, **Export PNG**, the chart control,
  and a status line that names the selected series/value when a point is clicked.
- `OgSkiaChartRenderer` is the implementation of the domain's `IRenderingService` (the owning repository ships
  only the interface): six chart types, palette from `BeepChartBrush1..8`, axes/grid/labels, legends for
  pie/donut, and `LastPoints` for hit testing.
- `ChartSurface` paints through the same `ViewSurface` path as the well visuals, so it inherits device
  scaling, the failure boundary and the tools' trademark footnote; `ChartHost` is the WPF wrapper.
- **AC-1** line, area, bar, scatter, pie and donut all render (theory over six types); an unknown type is a
  failure that names it (`radar`).
- **AC-2** the palette is a property, set by the host from the theme tokens; a test renders red then green and
  asserts the old colour is gone. A skin change is a repaint.
- **AC-3** an empty series draws **"No data"** inside the chart area (`LastOutcome == "no data"`) instead of
  empty axes.
- **AC-4** a throwing renderer produces a failure panel **and** one failure-reporter row whose reference is
  shown on the surface.
- **AC-5** a click within 14 px of a drawn point raises `PointSelected` with series, label and value; a click
  far away raises nothing.
- **AC-6** the export renders through the same `ChartSurface`, so the PNG is **pixel-identical** to the frame
  on screen - including the trademark footnote that `ViewSurface.Draw` adds. That footnote was the first
  mismatch the test found, and it is why the exporter goes through the surface rather than drawing directly.
- **ADR-001 follow-through:** referencing `Beep.OilandGas.Models` brings `SkiaSharp.Svg 1.60.0` (used by one
  Models helper). The desktop pins and **excludes its assets**, so only SkiaSharp 4.154.0 compiles and loads;
  a guard test fails if desktop source touches `SkiaSharp.Extended.Svg`/`SKSvg`.

### Still open

- **Chart configurations are session-only.** The interface's save/get/delete work in memory; persisting
  `CHART_CONFIGURATION` needs the database path (DAT-02).
- The optional toolbar (legend toggle, full screen) is not built; the page has a type picker and export.
- Point selection is nearest-marker on cartesian charts and slice-midpoint on pie; there is no hover readout.
- Only PNG export; SVG/PDF chart export is not offered (ADR-001 keeps one SVG path, and this is not it).
