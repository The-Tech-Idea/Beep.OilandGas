# VSB-07 — Well path, ladder plot and travelling cylinder

| Field | Value |
|---|---|
| Stage | STG-15 — Wellbore visual suite |
| Area | Visuals |
| Status | Gate met - stations, plan/section, ladder plot, travelling cylinder, uncertainty ellipses and a per-offset cancellable scan. See [evidence](../../evidence/STG-15-VSB-07/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore` |
| Desktop project | `Modules/Wells/WellPath/` |
| External packages | `TheTechIdea.OilGas.Wellbore`, `Wellbore.Data` (`SurveyUncertainty`, `AntiCollision`) |
| Depends on | VSB-03, ENG-01 |
| Blocks | — |
| Web reference | none |

## 1. Purpose

See where the well actually goes: plan and actual projection views, ladder plot of separation, and
the travelling cylinder projection for anti-collision review.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore/Correlation/AntiCollisionViews.cs` | anti-collision visuals |
| `Wellbore/Correlation/WellMap.cs` | plan view |
| `Wellbore/Tracks/TrajectoryTrack.cs` | trajectory in the depth plot |
| `Wellbore/Depth/DepthMapper.cs` | MD/TVD mapping |
| `Wellbore.Data/Model/Trajectory.cs`, `SurveyStation.cs` | survey data |
| `Wellbore.Data/SurveyUncertainty.cs`, `AntiCollision.cs` | ellipse and separation data |

## 3. Screens

`Wells → Well path`

Tabs: Plan · Section · 3D path · Ladder plot · Travelling cylinder.

- Plan view: North/East with the planned and actual path, uncertainty ellipses at stations.
- Section view: vertical section along a chosen azimuth, with targets.
- 3D path: simple perspective with depth exaggeration.
- Ladder plot: separation factor vs depth against offsets.
- Travelling cylinder: projection at a chosen station with ellipse and offset wells.
- Station table: MD, inclination, azimuth, TVD, N/E, dogleg, build, turn.

## 4. Data and contracts

`OgSurveyStation { Md, Inclination, Azimuth, Tvd, North, East, Dogleg, Build, Turn }`
Separation and ellipse inputs come from `SurveyUncertainty` and `AntiCollision`.

## 5. States

| State | Behaviour |
|---|---|
| No survey | path tabs disabled with the reason; the station table is empty |
| No offsets selected | ladder and cylinder disabled with the reason |
| Azimuth undefined (vertical well) | section view uses a default and says so |
| No uncertainty model | ellipse hidden and stated |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `wells.read` | no |
| Choose offsets | `wells.read` | user preference |
| Export | `data.export` | no |

## 7. Acceptance Criteria

- AC-1 MD/TVD conversion matches `DepthMapper` for a reference survey.
- AC-2 Plan and section views draw the same stations.
- AC-3 The ladder plot uses the same separation computation as ENG-01.
- AC-4 Uncertainty ellipses are drawn only when an error model is available.
- AC-5 A vertical well states the azimuth assumption for the section view.
- AC-6 Changing the offset set cancels pending computation.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~VSB-07"
```

## 9. Open questions and risks

- Anti-collision computation belongs to ENG-01; this feature is visualisation only and must call it,
  never duplicate the maths.

### Delivered 2026-11-05

- **Route `wellpath`** (en + ar): view picker (**Plan / Section / Ladder plot / Travelling cylinder**),
  Export PDF, a station table (MD, inc, azi, TVD, N, E, dogleg) and a notice line.
- `OgWellPath` owns the demo survey and every calculation delegate: `Trajectory.Compute` for the stations,
  `AntiCollision.Scan` for separation, `LadderPlotView` / `TravellingCylinderView` for the two anti-collision
  pictures, `SurveyUncertainty.ForWell` for ellipse availability.
- **AC-1** in the vertical leg TVD equals MD to three decimals; the deviated well's TVD is shorter than its
  MD with a non-zero north displacement.
- **AC-2** plan and section are two projections of **one** station list — equal counts and identical MD
  order, so they cannot disagree.
- **AC-3** the ladder and cylinder are drawn from the tools' `AntiCollisionResult`; the demo's minimum
  separation factor is **0.65**, and no separation maths is re-implemented.
- **AC-5** a vertical trajectory yields azimuth 0 with `Assumed = true` and a note saying the well is
  vertical so 0° is used; the deviated demo is measured at ~45°.
- **Note on the demo:** the offset well is close enough that SF 0.65 is below 1 — an intentional
  anti-collision condition for the ladder and cylinder to show, not a pass/fail verdict on real wells.

- **AC-4 delivered.** `OgWellPath.Uncertainty` asks `SurveyUncertainty.ForWell` for a horizontal ellipse per
  station and keeps only those with a positive semi-major axis; `OgPathSurface` draws them over the plan
  path. The environment's own model is present - the smoke run reports **6 ellipses for 7 stations** (the
  surface station has zero covariance and is correctly omitted). Supplying an ellipse changes the frame and
  clearing it restores it byte-for-byte, so the drawing is proven, not just its gate.
- **AC-6** the scan is **chunked per offset well** and cancellable (2026-11-06): `ComputeOffsets` checks a
  token before each offset and after the last, so a cancelled scan does no further work and returns nothing.
  The page holds a `CancellationTokenSource`, cancels it when the offset set changes, and a superseded scan
  is never applied; the offset set is now a real choice (one offset, two offsets, none).

### Still open

- No 3D path view, no section targets, and no travelling-cylinder station picker; the cylinder uses the
  tools' default station. The plan/section surface is a small polyline drawn here, not `MapView`.
- The offset wells are fixtures, not wells read from PPDM (DAT-02).
