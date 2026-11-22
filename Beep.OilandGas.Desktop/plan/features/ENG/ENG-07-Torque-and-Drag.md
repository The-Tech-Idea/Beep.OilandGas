# ENG-07 — Torque and drag

| Field | Value |
|---|---|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - see [evidence](../../evidence/STG-16-ENG-07/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Drilling` |
| Desktop project | `Modules/Engineering/TorqueDrag/` |
| External packages | `TheTechIdea.OilGas.Drilling` |
| Depends on | ENG-06 |
| Blocks | DEV-05 |
| Web reference | `OilandGasTools/docs/customer/drilling.html` |

## 1. Purpose

Predict hook load and torque along the string, and identify the limiting factor for the planned
operation.

## 2. Source components

| Source | Reuse |
|---|---|
| `Drilling/TorqueDrag.cs` | `Run(Trajectory, stringFromBit, TorqueDragCase)` |
| `Drilling/DrillingToolbox.cs` | friction factors and settings |
| `Wellbore.Data/Model/TubularCatalog.cs` | string components |

## 3. Screens

`Engineering → Torque & drag`

- Scenario: rotating / sliding / tripping; pick-up / slack-off / rotate-off.
- String editor from bit upward: component, OD, ID, weight, length, unit.
- Friction factor: cased / open hole per interval.
- Results grid per station: MD, inclination, axial load, torque, drag.
- Chart: load vs depth against string limits.
- Summary: hook load, surface torque, limiting factor, margin.

## 4. Data and contracts

`OgTorqueDragResult { Rows[] { Md, Load, Torque }, HookLoad, SurfaceTorque, Limit, Margin }`.

## 5. States

| State | Behaviour |
|---|---|
| Load above string limit | failed; the limiting component named |
| No trajectory | refused with the reason |
| Friction factor missing for an interval | refused with the interval |
| String not reaching TD | refused |
| Mixed units | refused |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run | `engineering.torquedrag.read` | no |
| Save run | `engineering.torquedrag.write` | yes |

## 7. Acceptance Criteria

- AC-1 A reference case reproduces hook load within tolerance.
- AC-2 The limiting factor is named with the component.
- AC-3 Friction factors are per interval and visible.
- AC-4 Rows are ordered by MD and cover the whole string.
- AC-5 Mixed units are refused.
- AC-6 A saved run records scenario and friction factors.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-07"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-16-ENG-07/observed-evidence.json).

**The open question, answered by neither reusing nor improvising inside a page.** *"Chart control: reuse `TimeDepthView` or draw a
new `ViewSurface`?"* `TimeDepthView` plots **time against depth**; this needs **load against depth**. So `OgTorqueDragChart` is a new
`ViewSurface` written to the tools' own conventions — `WellboreTheme` colours, the same fonts and axis placement, and the same
`SkiaViewSurfaceHost` — so it draws and exports like every other view instead of being a WPF drawing inside one page.

**AC-1 is proven against a closed form, not against the same code twice.** A vertical string has no side force, so the hook load
must be the **buoyed weight**: (19.5 lb/ft in kg/m) × g × (1 − mud/steel) × 2 000 m. The gateway asserts it within **1 %**, along
with **zero surface torque** and **zero side force on every station**.

| What the page shows | Recorded in |
|---|---|
| The trajectory the run follows | `WELL_PLAN` (ENG-06), recomputed by the owner's walk |
| String: OD, ID, weight, length, grade | `STRING_TEXT` on the run |
| Scenario, both friction factors, shoe, mud, element | recorded columns |
| Hook load, surface torque, limiting component | recorded columns |
| Stations (the `OgTorqueDragResult.Rows` contract) | recomputed, never stored |

**The trajectory is not stored twice.** The run rebuilds it from the saved plan's stations through the owner's own minimum-curvature
walk, so a run is always against the plan it names and a stored copy cannot go stale — which is what ENG-06's handoff is for.

**A limit is not invented.** A component's capacity is the **API 5C3 pipe-body yield** at its own wall and grade. A finding worth
recording: the **shipped grade table is casing grades**, so **S135 is not in it** — the run refuses an unconfigured grade by name,
and the gateway **adds S135 as an operator would** and removes it afterwards.

**AC-3 follows the interval, not a blended number.** The cased factor is demanded; the **open-hole factor only when the shoe is above
the bit**; with no shoe the whole string is cased, and the page says so. A first draft had that backwards, requiring an open-hole
factor whenever the shoe was *missing*.

**Two other corrections came out of the work:** stations' inclinations were being read from a throwaway single-station trajectory
(they now come from the real walk's bracketing stations), and the scenario was being re-derived from the operation with a string
comparison — it is now a named switch.

**Not built, and stated as gaps:** stiffness, buckling and stick-slip (this is the owner's soft-string model); torsional, burst,
collapse and connection limits (the capacity compared is tension only); a chart with drag and torque on the axes; a sub-grid string
editor (the string is text, and a mistyped line is refused naming the line); a run-by-run comparison (the screen offers the most
recent run); and paging.

## 9. Open questions and risks

- `TorqueDragCase` shape is undocumented; read the source and record it in §4.
