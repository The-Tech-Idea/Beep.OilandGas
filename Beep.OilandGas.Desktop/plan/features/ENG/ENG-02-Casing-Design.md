# ENG-02 — Casing design

| Field | Value |
|---|---|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - see [evidence](../../evidence/STG-16-ENG-02/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Data` |
| Desktop project | `Modules/Engineering/CasingDesign/` |
| External packages | `TheTechIdea.OilGas.Wellbore.Data` |
| Depends on | VSB-02, DEV-03 |
| Blocks | DEV-05, ENG-03 |
| Web reference | `OilandGasTools/docs/customer/casing-design.html` |

## 1. Purpose

Design and check casing to API 5C3: burst, collapse and tension with design factors, grade
selection, and a per-load-case verdict.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Data/Model/CasingDesign.cs` | `Rate(TubularString, settings)`, `Check(WellModel, loadCase, settings, warnings)`, `Burst/Collapse/Tension(od, wall, yield)`, `YieldOf(grade)` |
| `Wellbore.Data/Model/TubularCatalog.cs` | grades, weights, wall thicknesses |
| `Wellbore.Data/Model/Annuli.cs`, `Pressure.cs` | load inputs |
| `Wellbore.Data/Configuration/tool-models.json` | settings |
| `WellSchematic/CasingDesignTrack.cs` | schematic overlay |

## 3. Screens

`Engineering → Casing design`

- String editor: section, OD, wall, grade, weight, top, bottom (units per field).
- Load cases: burst (internal), collapse (external), tension (string weight + overpull); editable
  case set saved with the design.
- Design factors per load case, from configuration.
- Results grid per section: load, resistance, utilisation, verdict.
- Summary: governing case and utilisation ratio.
- Schematic overlay preview.

## 4. Data and contracts

`OgCasingSection { Od, Wall, Grade, Weight, Top, Bottom, Unit }`
`OgCasingVerdict { Section, Case, Load, Resistance, Utilisation, Passes }`.

## 5. States

| State | Behaviour |
|---|---|
| Grade not in the catalog | refused with the grade |
| Missing wall thickness | refused |
| Utilisation > 1 | failed, governing section named |
| No load case | check refused with the reason |
| Mixed units | refused |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Check | `engineering.casing.read` | no |
| Save design | `engineering.casing.write` | yes |

## 7. Acceptance Criteria

- AC-1 Burst, collapse and tension reproduce API 5C3 worked examples.
- AC-2 Utilisation is computed per section and per case.
- AC-3 The governing case is named.
- AC-4 Design factors come from configuration, not the view.
- AC-5 A grade absent from the catalog is refused.
- AC-6 A saved design records grades, walls and design factors.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-02"
```

Port the assertions from `OilandGasTools/tests/TheTechIdea.OilGas.Tests/CasingDesignTests.cs`.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-16-ENG-02/observed-evidence.json).

**The open question, answered from the owner's source.** `CasingLoadCase` carries exactly two inputs - `FluidGradient` (the mud
outside the casing) and `TestPressure` (the surface test pressure). The desktop adds the **overpull**, which the owner's simple
check does not model, and records all three on the run rather than widening the owner's type.

**The owner had already said whose job this is.** Its own comment on `DEVELOPMENT_CASING_STRING` states that the burst, collapse
and tension arithmetic belongs to ENG-02, so the ratings are the tools' `CasingDesign.Rate`, `Burst`, `Collapse` and `Tension` -
Barlow at 87.5 % wall, four collapse regimes by D/t, pipe-body yield, API rounding of 10 psi and 1 000 lbf. **The desktop
implements no second curve.**

**But the table could not supply the inputs.** A 5C3 rating is a function of **OD and wall**, and the tension load is the buoyed
weight of the length between the **top** and the shoe - and the outline recorded neither. `WALL_THICKNESS` and `TOP_DEPTH` are
additive on the table's own partial class, so a DEV-03 row stays valid. A row without them still loads: the top is measured from
surface with a **stated warning**, and the wall is resolved from the owner's **nominal-weight catalogue** - or the section is
**refused**, never rated on a guessed wall.

| What the page shows | Recorded in |
|---|---|
| Sections, OD, weight, grade, set depth | `DEVELOPMENT_CASING_STRING` |
| Wall and top | the added `WALL_THICKNESS` and `TOP_DEPTH` |
| Load case | `CASING_DESIGN_RUN`: mud gradient, test pressure, overpull |
| Design factors | the environment's `CasingDesign.DesignFactors`, stored per verdict |
| Verdicts | `CASING_DESIGN_VERDICT`: load, resistance, utilisation, required factor, pass |
| Governing case | the highest utilisation, named `SECTION · CASE` |

**A second check table was not reused.** DEV-03's `DEVELOPMENT_CASING_CHECK` records that a design's **inputs** were valid - it has
an outcome and a findings text and no load, resistance, utilisation or factor. Writing ENG-02 results into it would have put two
meanings on one row and made DEV-03's "last check" display a verdict it never computed.

**AC-4 is structural.** The factors are read from the configuration and stored per verdict; the page has no field for one, which
the unit test asserts against the page source rather than trusting the review.

**Two corrections came out of the work.** The first draft computed one true vertical depth from the shallowest shoe and applied it
to every section - harmless in a vertical test well, wrong in a deviated one - so each section now asks the trajectory for its own
shoe. And a unit assertion claimed the achieved factor was the reciprocal of the utilisation, whereas utilisation times the
achieved factor is the required factor.

**A typo was caught by tightening a check, not by the check passing.** The gateway compared the collapse utilisation against
5 179.8 instead of 5 194.8 and passed at two decimal places because both sides rounded to 1.20; it now compares at four.

**Not built, and stated as gaps:** gas-filled-annulus, circulating and kick load cases; overpull and test pressure validated
against the wellhead or hanger; ISO/TR 10400 ductile-rupture burst (the owner does not implement it, and it is refused rather than
substituted); a wall outside the six-row nominal-weight catalogue without recording it; a survey-based collapse load when the
planned well records no survey; liner-top overlap validation; per-section load cases; and the schematic overlay, which belongs to
VSB-05.

## 9. Open questions and risks

- `CasingLoadCase` shape is not documented; read the source and record it in §4 before coding.
