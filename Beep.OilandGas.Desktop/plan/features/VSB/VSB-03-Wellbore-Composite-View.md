# VSB-03 — Wellbore composite view

| Field | Value |
|---|---|
| Stage | STG-15 — Wellbore visual suite |
| Area | Visuals |
| Status | Gate met - see [evidence](../../evidence/STG-15-VSB-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` (data), `TheTechIdea.OilGas.Wellbore` (rendering) |
| Desktop project | `Modules/Wells/WellboreView/`, `WellTools/Bridging/OgWellModelMapper.cs` |
| External packages | `TheTechIdea.OilGas.Wellbore`, `Wellbore.Data` |
| Depends on | VSB-02, WEL-03 |
| Blocks | VSB-04, VSB-05, VSB-06, VSB-07 |
| Web reference | `Pages/PPDM39/DataManagement/DrawingSampleHost.razor` (data-URL rendering only) |

## 1. Purpose

The general-purpose depth plot: a depth track plus arbitrary tracks (schematic, logs, tops,
pressure), with zoom, pan, cursor readout, markers and annotations — fed from a real PPDM well.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore/WellboreView.cs` (+ `.Rendering`, `.Editing`, `.Input`, `.Accessibility`) | the view |
| `Wellbore/WellboreDocument.cs`, `Depth/WellboreViewState.cs`, `Depth/DepthAxis.cs`, `DepthMapper.cs`, `DepthWarp.cs` | document and depth model |
| `Wellbore/Rendering/WellboreRenderer.cs`, `TrackChrome.cs`, `Paths.cs`, `WellboreTheme.cs` | rendering |
| `Wellbore/Tracks/*` (`WellTrack`, `TrackLayout`, `WellMarkers`, `TrajectoryTrack`, `BuiltInTracks`) | tracks |
| `Wellbore/Tracks/TrackLayout.cs`, `Rendering/TrackChrome.cs` | layout |
| `Wellbore/Lithology/*` | lithology fills |
| `Wellbore/Performance/CurveDecimation.cs` | large-log handling |
| `Wellbore/WellboreValidation.cs` | validation |
| `Wellbore/Output/*` | export (VSB-09) |
| `Beep.OilandGas.LifeCycle` well/bore/log/top records | source data |
| `Beep.OilandGas.Drawing` `DrawingEngine`, `WellRenderer` | **alternative** renderer, see §6 |

## 3. Screens

`Wells → Wellbore view`

- Toolbar: well/wellbore selector, depth unit, track set (template), zoom in/out/fit, cursor toggle,
  markers, annotations, export, print.
- Main area: `SkiaViewSurfaceHost` hosting `WellboreView`.
- Left rail: track visibility list with reorder.
- Right rail (collapsible): cursor readout at the current depth (all visible tracks), selection info,
  validation findings.
- Bottom: depth range indicator and a mini depth scale.
- Empty/denied/failed states rendered around the host, never inside the canvas.

## 4. Data and contracts

`OgWellModelMapper.ToDocument(well, wellbore, logs, tops) → (WellboreDocument, warnings)`.
Warnings are surfaced; a well without a trajectory or tops still opens with the available tracks.

## 5. States

| State | Behaviour |
|---|---|
| No well selected | empty state with a picker |
| No depth data | "no depth-indexed data for this wellbore" |
| Very large log | decimation applied; a badge states the decimation ratio |
| Mapped with warnings | amber badge listing the warnings |
| Render exception | failure panel around the host |
| Denied | denied state, no canvas |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `wells.read` | no |
| Add/edit marker, annotation | `wells.interpretation.write` | yes |
| Change track template | `wells.read` | user preference |

## 7. Acceptance criteria

- AC-1 A real well from the database renders with a depth track and at least one data track.
- AC-2 20 000 samples render without freezing the UI.
- AC-3 Cursor readout reports values for each visible track at the current depth.
- AC-4 A marker can be added, undone and redone.
- AC-5 Mapping warnings are visible without blocking the view.
- AC-6 The depth unit follows the environment and can be switched without losing position.
- AC-7 RTL does not mirror the depth plot (recorded decision: geological plot stays LTR).

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~VSB-03"
```

Manual: open three wells of increasing data complexity and capture screenshots.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-15-VSB-03/observed-evidence.json).

**The recorded blocker was the wrong diagnosis.** AC-1 was blocked on *"real PPDM data"*, and the page drew the tools' demo
documents and said so. The database side was available all along: `WELL`, `WELLBORE` and `WELL_COMPLETION` are declared by the
owner, and `WELL_COMPLETION` carries real `TOP_DEPTH`/`BASE_DEPTH` intervals per UWI. What was missing was the **read path** -
nothing mapped a database well into a `WellboreDocument`.

| What the view shows | Recorded in |
|---|---|
| Well identity and name | `WELL.UWI`, `WELL_NAME` |
| Depth axis | `WELLBORE.TOTAL_DEPTH` (or `WELL.BASE_DEPTH`), with its UOM |
| Data track (completion intervals) | `WELL_COMPLETION` top, base, unit and type |
| Warnings | what the records did not supply |

**AC-1 is met from records.** The document is built with the owner's own `DepthTrack`, `IntervalTrack` and `DepthInterval`
types, so the renderer draws exactly what it expects; the gateway asserts the interval values, the axis and that an **inactive
completion is not drawn**.

**AC-5 stays honest about what the schema cannot do.** `WELL_LOG`, `WELL_LOG_CURVE` and `WELL_LOG_AXIS_COORD` hold log headers,
curve metadata and **index coordinates but no measured value per sample**, and the LAS path is file-based rather than stored. So
no curve track can be drawn from this schema, and the view carries a warning saying exactly that instead of plotting a demo curve
as real data. Inventing a row-per-sample PPDM table is a data-model decision for the owner, not something to guess at here.

**The axis refuses to overstate.** A well with no total depth ends at its deepest recorded interval and *says so*; a well with no
depth-indexed data at all is **refused** with its reason rather than drawn as a bare axis; an unknown well and a blank identity
are refused before any read.

**A small trap worth recording:** `DepthTrack`, `IntervalTrack` and `DepthInterval` live in the `TheTechIdea.OilGas.Wellbore`
namespace although their file sits in a `Tracks` folder, so a folder-based `using` does not compile.

**Not built, and stated as gaps:** a curve track (no value store exists); more than one bore per well; unit conversion between
feet and metres; interval descriptions beyond the completion type; persisted markers and annotations (as recorded for VSB-02); and
the manual screenshot pass the feature asks for - the automated run proves the document, the tracks and the readout, not the
pixels.

## 9. Open questions and risks

- **Two renderer families.** `OilandGasTools` `WellboreView` and `Beep.OilandGas.Drawing`
  `DrawingEngine` both draw wellbores. Decision: use `OilandGasTools` for this feature (it has the
  interaction handlers, accessibility and print planner), and reserve `DrawingEngine` for the sample
  gallery in INT-04. Record any change here.
- 20 000-sample performance must be measured, not assumed; `CurveDecimation` exists for this reason.
