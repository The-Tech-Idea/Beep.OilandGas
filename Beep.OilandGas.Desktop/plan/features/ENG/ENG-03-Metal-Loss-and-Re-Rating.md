# ENG-03 — Metal loss and re-rating

| Field | Value |
|---|---|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - see [evidence](../../evidence/STG-16-ENG-03/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Integrity` |
| Desktop project | `Modules/Engineering/MetalLoss/` |
| External packages | `TheTechIdea.OilGas.Integrity` |
| Depends on | VSB-02, ENG-02 |
| Blocks | ENG-04, ENG-05 |
| Web reference | `OilandGasTools/docs/customer/integrity.html` |

## 1. Purpose

Measure wall loss from multi-finger caliper data, re-rate the string, and state the remaining life
basis.

## 2. Source components

| Source | Reuse |
|---|---|
| `Integrity/MfcAnalysis.cs` | `Analyze(WellModel, LogSet, joints, warnings)`, `JointsCsv` |
| `Integrity/Rerating.cs` | `Rerate(WellModel, MfcResult, loadCase, warnings)`, `ToCsv` |
| `Integrity/CorrosionTrack.cs` | wall-loss track overlay |
| `Integrity/IntegrityToolbox.cs` | `Settings` incl. damage class |
| `Wellbore.Data/Model/CasingDesign.cs` | resistance after loss |

## 3. Screens

`Engineering → Metal loss`

- Log selector (MFC log set) and joint definition (automatic or manual).
- Joint results grid: joint, depth, nominal wall, measured minimum, loss %, damage class.
- Re-rating panel: load case, remaining resistance, new rating, utilisation.
- Track overlay: wall-loss track alongside the schematic.
- Remaining-life panel: rate of loss, basis, projection.
- Export CSV.

## 4. Data and contracts

`OgJointResult { Joint, Depth, NominalWall, MinWall, LossPercent, DamageClass }`
`OgRerateResult { Case, Resistance, Rating, Utilisation, Passes }`.

## 5. States

| State | Behaviour |
|---|---|
| No MFC log | analysis disabled with the reason |
| Joint definition incomplete | refused with the uncovered interval |
| Loss beyond the damage limit | failed; the joint is named |
| Remaining-life basis absent | "basis not recorded" and no projection |
| Noisy data | flagged per joint with the deviation |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Analyse | `integrity.mfc.read` | no |
| Save result | `integrity.mfc.write` | yes |
| Export | `data.export` | no |

## 7. Acceptance Criteria

- AC-1 Joint wall loss matches a reference MFC dataset.
- AC-2 Damage classes come from the tool settings.
- AC-3 Re-rating uses the measured wall and states the load case.
- AC-4 Remaining life is only projected when a basis is recorded.
- AC-5 CSV export matches the grid.
- AC-6 A saved result links to the log set and joint definition used.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-03"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-16-ENG-03/observed-evidence.json).

**The open question is answered without a second import path.** *"MFC analysis needs a log set with the right curves; the import
path (INT-01) must land first or the screen must accept a manual joint table."* The caliper file is read with the **same LAS
reader INT-01 uses**, so the `LogSet` the analysis consumes already exists - no second import path and no manual joint table.

**The nominal pipe comes from the well's own record.** `WELL_TUBULAR` carries the outside and inside diameters, grade, weight and
depths, so no wall is typed in or resolved from a catalogue. The weight matters: the first gateway run produced **burst and
collapse but no tension** until `TUBING_WEIGHT` was carried into the string the tools see.

| What the page shows | Recorded in |
|---|---|
| Nominal pipe (OD, ID, grade, weight, top, shoe) | `WELL_TUBULAR` |
| Caliper curves and the joints they form | the LAS file, through the tools' `MfcAnalysis` |
| Wall loss, remaining wall, areal loss, damage class | the analysis, per joint |
| Re-rated burst, collapse and tension at the measured wall | the tools' `Rerating` |
| Remaining-life rate and projection | the difference between two recorded results |
| The recorded result | the added `MFC_RESULT` and `MFC_JOINT` |

**AC-1 is exact by construction.** Eight identical 3.1 in radii on a 7 x 6 in pipe give penetration `(2 x 3.1 - 6) / (7 - 6) =
0.2` and a remaining wall of `(7 - 2 x 3.1) / 2 = 0.4` - asserted to four decimals along with the areal loss, so the number is
checked against the formula rather than against a previous run of the same code.

**AC-2 refuses to invent a scale.** The owner ships **no damage classes**, because they are company practice. With none
configured a joint is unclassified and the page says that a class name there would be a threshold this build invented. Configure
Moderate and Severe and the same joint is classified, and the classes are recorded with the run - so a later change to the
settings does not rewrite what the inspection concluded.

**AC-3 is the tools' own re-rating** at the wall used (minimum wall by default, or the uniform average wall if the basis is
configured that way), and each row names its load case and unit. Allowed load = resistance / the configured design factor;
utilisation = load / allowed, so the burst case at a 5 000 psi test against an 8 000 psi rating and a 1.25 factor is 0.78.

**AC-4 is the sharpest rule.** A rate of loss needs two dated inspections whose worst joint is **at the same depth**, and a
projection additionally needs a **configured damage limit**. So one inspection, a moved depth, a zero loss, no limit and an already
exceeded limit are each **stated rather than projected** - and an exceeded limit reports **zero years, never a negative one**.

**A gap closed in the owning module.** The owner computes the interpretation in memory and keeps nothing, so `MFC_RESULT` and
`MFC_JOINT` now record the measured figures, the re-rated resistances and the rules as they were applied.

**A quota the feature asks for and the owner cannot supply is recorded as a limit:** noisy joints are **not flagged**, because no
noise threshold or noise statistic exists; only the per-joint figures are shown.

**Not built, and stated as gaps:** automatic joint detection only (the CCL threshold ships unset, so one segment per nominal
interval); the wall-loss track overlay, which belongs to the schematic (VSB-05); per-joint load cases; a rate that follows each
joint rather than the worst one; and external-corrosion wall loss, which a caliper cannot see.

## 9. Open questions and risks

- MFC analysis needs a log set with the right curves; the import path (INT-01) must land first or the
  screen must accept a manual joint table.
