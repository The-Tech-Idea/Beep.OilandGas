# ENG-08 — Hydraulics

| Field | Value |
|---|---|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - see [evidence](../../evidence/STG-16-ENG-08/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Drilling` |
| Desktop project | `Modules/Engineering/Hydraulics/` |
| External packages | `TheTechIdea.OilGas.Drilling` |
| Depends on | ENG-06 |
| Blocks | DEV-05 |
| Web reference | `OilandGasTools/docs/customer/drilling.html` (hydraulics) |

## 1. Purpose

Compute circulating pressures, ECD, bit pressure loss and annular velocities, and select nozzles.

## 2. Source components

| Source | Reuse |
|---|---|
| `Drilling/Hydraulics.cs` | `Circulate(mud, flowRate, sections, nozzles, bitTvd, surfaceLoss)`, `Gradient`, `BitLoss`, `Velocity` |
| `Drilling/DrillingToolbox.cs` | rheology and ECD settings |
| `Wellbore.Data/Units/Quantity.cs` | units |

## 3. Screens

`Engineering → Hydraulics`

- Mud: model (Bingham / power law), density, PV, YP, n, K.
- Flow rate and unit; surface loss.
- Sections: hole/casing, ID, length (from the well plan).
- Nozzle editor: count and size in 32nds.
- Results: annular velocity per section, ECD, bit pressure loss, total system pressure.
- Chart: ECD vs depth with pore and fracture gradient lines when available.
- Optimizer: recommend nozzle sizes for a target ECD.

## 4. Data and contracts

`OgHydraulicsResult { Sections[] { Velocity, Ecd, PressureLoss }, BitLoss, SystemPressure, Tfa }`.

## 5. States

| State | Behaviour |
|---|---|
| ECD above fracture gradient | critical, with the depth |
| Rheology parameters missing | refused with the missing parameter |
| No nozzle | bit loss computed as zero and flagged |
| Section geometry missing | refused with the interval |
| Optimizer finds no solution | stated as infeasible |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run | `engineering.hydraulics.read` | no |
| Save run | `engineering.hydraulics.write` | yes |

## 7. Acceptance Criteria

- AC-1 A reference case reproduces ECD within tolerance.
- AC-2 Annular velocity and pressure loss are per section.
- AC-3 ECD above fracture gradient is critical.
- AC-4 Nozzle sizes are in 32nds and shown as such.
- AC-5 The optimizer's recommendation improves ECD monotonically in its search range.
- AC-6 A saved run records mud model and flow rate.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-08"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-16-ENG-08/observed-evidence.json).

**The open question is answered from the data.** *"Pore and fracture gradients may not be stored; if absent, omit the threshold
lines rather than inventing them."* A **fracture gradient is** recorded — on the annulus, from ENG-04 — so it is joined to the shoe
depth through the string id and drawn. A **pore pressure or pore gradient is recorded nowhere** in either repository, so **no pore
line is drawn** and the page says so rather than plotting a second invented threshold.

**AC-1 is checked against the textbook, not against the code.** The laminar annular and pipe gradients are computed from the
Bingham field relations **written out independently in the test** and matched to nine decimals; the bit loss is matched against
`8.311e-5·ρ·q²/(Cd²·A²)` for the nozzle area; and the ECD against the owner's own relation to six.

| What the page shows | Recorded in |
|---|---|
| Mud model, density and both viscometer readings | `HYDRAULICS_RUN` |
| PV, YP, n and K as the model derived them | the same row, kept beside the readings |
| Rate, unit, surface loss, bit TVD | the same row |
| Nozzles and flow paths | `NOZZLES_TEXT` / `SECTIONS_TEXT` |
| Standpipe, annular loss, ECD, bit loss, horsepower | the same row |
| The fracture gradient and whether it was exceeded | the same row, from the annulus record |

**The profile is not a second model.** The owner returns **one ECD at the bit**, and the chart and the per-depth fracture comparison
both need a profile. So the profile accumulates the **owner's own `Gradient`** down the annulus and applies the owner's own ECD
relation at each depth — and the service **refuses to publish** it unless the deepest point equals `HydraulicsResult.Ecd`, which the
gateway asserts to four decimals.

**AC-5 needed its search variable corrected.** The feature asks for **nozzle sizes for a target ECD**, but in this model the nozzle
never enters the ECD: the annular loss depends on the rate and the annulus only. So the optimiser **solves the rate** — the lever
that does move the ECD, in every regime the model covers — and reports the bit hydraulics at that rate. The gateway **proves the ECD
is identical with and without nozzles**, so the correction is by measurement rather than by comment.

**The solver had to report a step.** The owner's laminar and turbulent correlations **do not meet** at the configured transition, so
the ECD **jumps** there and a bisection lands on the jump — measured at **0.019 ppg** above a 10.4 ppg target. The recommendation
therefore states the rate that first reaches the target, the ECD actually reached, and why the two differ, rather than reporting an
exact hit that does not exist.

**A divide-by-zero was caught before it shipped.** `Hydraulics.BitLoss` divides by the summed nozzle area, so an empty nozzle list
gives an **infinite** bit loss and standpipe pressure. That state is handled first: zero bit loss, flagged, and the standpipe
pressure restated without the bit's share — never published as infinity.

**A fixture bug found a real gap.** Two annulus sections sharing a depth range **doubled that depth's loss**; overlapping annuli are
now refused, naming both sections and the interval they share.

**A finding for the owner.** The business layer carries a **second hydraulics calculator** whose velocity is in feet per minute fed
to formulas that take feet per second, with pipe coefficients for annuli and a 3000 Reynolds threshold. Only the tools' package is
called here, so a number on this page cannot be the other implementation's — recorded because two hydraulics implementations in one
solution will eventually be compared.

**Not built, and stated as gaps:** a per-run rheology model (it is configuration, because the owner's `Gradient` reads the global
setting); API RP 13D's Herschel-Bulkley method (the owner does not implement it); turbulent power-law flow (reported as not
computed, with the ECD withheld); a pore-pressure line; depth-band fracture gradients (one per string is recorded); a sub-grid flow
path editor; a computed surface loss; and paging.

## 9. Open questions and risks

- Pore and fracture gradients may not be stored; if absent, omit the threshold lines rather than
  inventing them.
