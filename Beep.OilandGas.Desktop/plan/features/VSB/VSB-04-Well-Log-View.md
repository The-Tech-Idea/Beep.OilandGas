# VSB-04 — Well log view

| Field | Value |
|---|---|
| Stage | STG-15 — Wellbore visual suite |
| Area | Visuals |
| Status | Gate met - triple combo, curve list, shift/splice with undo, crossplots, image/cement tracks and a real lithology-standard fill change. See [evidence](../../evidence/STG-15-VSB-04/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.WellLogs`, `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Wells/WellLogView/` |
| External packages | `TheTechIdea.OilGas.WellLogs` |
| Depends on | VSB-03 |
| Blocks | ENG-10 |
| Web reference | none (Web log page is a "data has moved" stub) |

## 1. Purpose

Read a well log properly: triple combo, curve fills, pay summary, image logs, cement bond, crossplots
— with curve editing that is audited and undoable.

## 2. Source components

| Source | Reuse |
|---|---|
| `WellLogs/WellLogView.cs` | view subclass; `LoadModel`, `PrimaryStandard` |
| `WellLogs/Documents/WellLogDocuments.cs`, `WellLogTemplates.cs`, `WellLogModels.cs` | documents and templates |
| `WellLogs/Tracks/WellLogTrack*.cs` (7 partials), `WellLogTracks.cs` | curve tracks |
| `WellLogs/Tracks/PressureTrack.cs`, `ImageLogTrack.cs`, `CementBondTracks.cs`, `WaveformTrack.cs`, `CuttingsTrack.cs`, `DipTrack.cs`, `FlagTrack.cs`, `PaySummaryTrack.cs` | specialised tracks |
| `WellLogs/Analysis/Crossplots.cs`, `CrossplotView.cs` | density-neutron and Pickett |
| `WellLogs/Analysis/CementEvaluation.cs` | bond index, classification |
| `WellLogs/Analysis/CurveEditing.cs` | shift and splice |
| `WellLogs/Analysis/LiveLogFeed.cs` | live feed (deferred screen) |
| `Wellbore/Lithology/*` | lithology library and standards |

## 3. Screens

`Wells → Logs`

- Template selector: triple combo, lithology, cement, image, custom.
- Standard selector: lithology standard (including GOST 21302), which changes the fill patterns.
- Main area: `SkiaViewSurfaceHost` hosting `WellLogView`.
- Curve list: mnemonic, unit, colour, scale (linear/log), range, visibility.
- Crossplot tab: density-neutron and Pickett with the interaction handler.
- Cement tab: bond index curve, intervals, classification.
- Edit tab: depth shift and splice with preview and undo.

## 4. Data and contracts

`OgLogCurve { Mnemonic, Unit, Description, Min, Max, Scale, IsIndex, Samples }`
Editing produces `TrackEditHistory` entries; the desktop surfaces undo/redo rather than hiding it.

## 5. States

| State | Behaviour |
|---|---|
| No log data | "no log sets for this wellbore" |
| Curve without unit | plotted but flagged; scale label shows "unit unknown" |
| Missing lithology standard | falls back to the default pattern set with a notice |
| Shift would move data outside the log range | refused with the range |
| Render exception | failure panel |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `wells.read` | no |
| Edit curve | `wells.logs.edit` | yes |
| Save template | `wells.read` | user preference |

## 7. Acceptance criteria

- AC-1 A triple-combo log opens with GR, resistivity and porosity tracks.
- AC-2 A lithology standard changes the fill patterns and states which standard is active.
- AC-3 A depth shift is undoable and recorded in history.
- AC-4 A splice records both source and target intervals.
- AC-5 Crossplots are produced by `Crossplots`, not re-implemented.
- AC-6 Image log and cement bond tracks render when data exists, and say "no data" when not.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~VSB-04"
```

## 9. Open questions and risks

- Lithology colours exist in both `OilandGasTools` (`LithologyColorPalette`) and the theme. Choose the
  tools palette for log fills (it encodes the standard) and record that decision here.

### Delivered 2026-11-05

- **Route `logs`** in the catalog (with an Arabic title), hosting `WellLogView` in `SkiaViewSurfaceHost`.
- `OgLogWorkspace` is the whole bridge: the curve list (`OgLogCurveRow`, with **`unit unknown`** for a curve
  that has no unit), `OgLogEditing` + `OgLogEditHistory`, `OgLogCrossplots`, `OgLogAvailability`,
  `OgLogStandards` and a deterministic `OgLogDemo` log set with GR, RT, RHOB and NPHI.
- **AC-1** the triple combo has 6 tracks and a GR, a resistivity and a porosity curve; the smoke run lists
  4 curves over 8200–8300 ft.
- **AC-3** a shift goes through `CurveEditing.Shift`, records the before/after interval, and is refused when
  it is zero or would move a sample outside the axis range; `Undo` restores the original depths.
- **AC-4** a splice goes through `CurveEditing.Splice`, records **source and target** intervals, replaces
  both curves, and `Undo` restores them in their original positions.
- **AC-5** crossplots come from the tools' `Crossplots`; the test asserts the panel type and a non-empty
  cloud rather than re-implementing the maths.
- **AC-6 delivered.** `OgLogDemo.WithImageAndCement` builds a document with an **8-column image log (60 rows)**
  and the tools' cement presentation — CBL, bond index and bond flags, **3 tracks**. Availability now detects
  them by track type and curve mnemonic instead of a name fragment (the cement tracks are named "CBL",
  "Bond index" and "Bond", so the old name check would have missed all three). The "no data" sentences
  disappear when the tracks exist and remain for the plain triple combo; a non-empty frame renders through
  the same `ViewSurfaceRenderer` the host uses. The log page gained a **"Demo with image & cement"** source.
- **Lithology palette decision:** log fills use the **tools'** palette, because it encodes the standard;
  the theme colours the chrome around the plot. Recorded here per this section's request.

### AC-2 delivered 2026-11-06

**The mechanism is the lithology standards layer, not a stratigraphic lexicon.** A lexicon (`StandardLexicons`,
including the NCS sample) names *stratigraphic units*; the fills come from `LithologyLibrary` drawings, which
`LithologyStandards.Apply` layers per `resources.lithologyStandards`. That is why loading the NCS lexicon cannot
change a fill pattern — it was the wrong layer.

- `OgLogStandards` now offers **FGDC** (the built-in library), **GOST 21.302-2013** (the shipped layer) and a
  **sample company standard**; `Apply` sets `resources.lithologyStandards` and the library is rebuilt on the
  next draw, so the picture changes without reloading the data.
- **The sample company standard** (`sample-company.lithology.json`, written under the data root) overrides
  sand, shale, limestone and sandstone so the change is visible. It is labelled a demo and says so in its
  `source`; a real company supplies its own file with entries, overrides and `.pat` hatches.
- **GOST is honest about its gap:** the shipped file carries numbers and names but *no drawings* ("supply them
  as overrides"), so selecting it states that the built-in patterns remain.
- Tests: the drawing for `sand` differs under the sample and returns exactly on FGDC; the **rendered frame**
  differs and returns byte-for-byte; an unknown name falls back to FGDC with a notice.

### Still open

- The shift/splice actions act on the first curve(s); a curve-selection UI is still to come, along with the
  crossplot/cement tabs and the curve scale editor.
- The company standard is a demo fixture, not a customer's real drawings; GOST's drawings are not reproduced.
