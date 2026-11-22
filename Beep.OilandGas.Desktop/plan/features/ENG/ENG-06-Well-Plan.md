# ENG-06 — Well plan

| Field | Value |
|---|---|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - see [evidence](../../evidence/STG-16-ENG-06/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Drilling` |
| Desktop project | `Modules/Engineering/WellPlan/` |
| External packages | `TheTechIdea.OilGas.Drilling` |
| Depends on | VSB-02, DEV-03 |
| Blocks | ENG-07, DEV-05 |
| Web reference | `OilandGasTools/docs/customer/drilling.html` (well planning) |

## 1. Purpose

Design a directional well plan: stations, build and turn rates, target, and the resulting path —
then hand it to torque & drag and hydraulics.

## 2. Source components

| Source | Reuse |
|---|---|
| `Drilling/WellPlan.cs` | `Start(unit, md, north, east, tvd, inclination, azimuth)` and station list |
| `Drilling/DrillingToolbox.cs` | settings |
| `Wellbore.Data/Model/Trajectory.cs`, `SurveyUncertainty.cs` | geometry validation |
| `Wellbore/Correlation/WellMap.cs` | plan view |

## 3. Screens

`Engineering → Well plan`

- Target: N/E, TVD, tolerance radius, units.
- Design methods: build-and-hold, S-shape, J-shape, horizontal; or explicit stations.
- Station table: MD, inclination, azimuth, TVD, N/E, dogleg, build, turn.
- Plan view: North/East with the trajectory and the target circle.
- Vertical section view.
- Checks: maximum dogleg, target hit, tortuosity.
- Handoff: "use in torque & drag" and "use in program" (DEV-05).

## 4. Data and contracts

`OgPlanStation { Md, Inclination, Azimuth, Tvd, North, East, Dogleg }`
Handoff passes the trajectory, not a copy of the plan record.

## 5. States

| State | Behaviour |
|---|---|
| Target not hit | shown with the miss distance |
| Dogleg above limit | refused or warned per the tool setting |
| Missing start datum | refused |
| Units mixed | refused |
| Handoff without a saved plan | refused with the reason |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Design | `engineering.wellplan.read` | no |
| Save plan | `engineering.wellplan.write` | yes |
| Handoff | `engineering.wellplan.write` | yes |

## 7. Acceptance Criteria

- AC-1 A build-and-hold plan closes on its target within tolerance.
- AC-2 Dogleg is computed per station and checked against the limit.
- AC-3 The station table and the plan view agree.
- AC-4 Units are explicit on every geometric value.
- AC-5 Handoff passes the computed trajectory.
- AC-6 A saved plan records the design method.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-06"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-16-ENG-06/observed-evidence.json).

**The open question, answered first.** *"`WellPlan` is a class with a `Start` factory; check whether it mutates or returns a new
instance."* It mutates: `Hold`, `Curve` and `CurveHoldToTarget` each append a node to the same internal list and return `this`.
So every design is a **fresh builder chain built from the request**, and an edit can never append to a plan that was already
saved.

| What the page shows | Recorded in |
|---|---|
| Tie-on, target, tolerance, unit, method, dogleg | the added `WELL_PLAN` |
| Explicit sections | `SECTIONS_TEXT` on the same row |
| Stations (the `OgPlanStation` contract) | recomputed from the inputs, never stored |
| Miss, target hit, largest station dogleg, tortuosity | the saved outcome |
| Handoff actor and time | `HANDED_OFF_BY` / `HANDED_OFF_AT_UTC` |
| The trajectory handed on | the plan's own `ToWellModel`, not a copy of the row |

**AC-1 is the owner's solve.** `CurveHoldToTarget` lands a surface tie-on on (N 800, E 0, TVD 2 000) at 3° per 30 m with a miss
**below 1e-6**, and the final station is asserted *on* the target.

**AC-2 needed a correction in its own framing.** The plan's stations carry the *requested* dogleg on a curve section, so checking
that against the limit would be a tautology. The column is instead the **minimum-curvature walk's `Dls`**, recomputed from the
station list; with `drilling.maxDogleg` set to 1.0 the same 3° plan reports that it is above the limit, and a plan with no limit
**says so rather than reporting a pass**.

**AC-4 reached the grid headers.** "MD" and "TVD" do not say what unit they are in, so the headers are rewritten from the plan's
own unit on load, the dogleg/build/turn columns are labelled with the course length, and a section written in another unit is
**refused rather than converted**.

**AC-6 drove a data decision.** The **stations are not stored**: the inputs reproduce them exactly, so a save keeps the **method,
the inputs and the outcome**, and the geometry is recomputed when the plan is opened — which is also what makes **AC-5 a
trajectory rather than a copy of the record**.

**A refusal had to avoid the exception's text.** An unreachable target throws inside the solver, so the exception is **reported
with a reference** and the refusal names the condition in the desktop's own words: behind the bit, or inside the turning circle at
that dogleg.

**The method list is honest about its limits.** **S-shape and horizontal are not offered**: the package has no drop-back or
horizontal solver. The refusal explains that a J-shape and a horizontal well are the target solver with a different target, while
an S-shape would need a solve the maths does not have — so listing it would promise a design that cannot be produced.

**Not built, and stated as gaps:** a drawn plan and section view (VSB-02's well-path screen renders the same stations, and this
page summarises the points and extents); revision history for a plan; the torque-and-drag consumer, which is ENG-07; a
tortuosity figure from the package (this is the desktop's stated definition); and any paging.

## 9. Open questions and risks

- `WellPlan` is a class with a `Start` factory; check whether it mutates or returns a new instance
  before designing the editor.
