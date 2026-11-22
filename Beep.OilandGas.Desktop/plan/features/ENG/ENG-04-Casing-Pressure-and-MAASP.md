# ENG-04 — Casing pressure and MAASP

| Field | Value |
|-----|-----|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - see [evidence](../../evidence/STG-16-ENG-04/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Integrity`, `TheTechIdea.OilGas.WellSchematic` |
| Desktop project | `Modules/Engineering/CasingPressure/` |
| External packages | `TheTechIdea.OilGas.Integrity`, `WellSchematic` |
| Depends on | ENG-03 |
| Blocks | ENG-05 |
| Web reference | `OilandGasTools/docs/customer/integrity.html` (casing pressure section) |

## 1. Purpose

Classify annular casing pressure against API RP 90 and compute MAWOP/MAASP so an operator knows
what pressure is safe and what must be bled off.

## 2. Source components

| Source | Reuse |
|---|---|
| `Integrity/CasingPressure.cs` | `CasingPressureDiagnostic.Classify(test, mawop)` |
| `Integrity/Mawop.cs` | `Mawop.For(well, annulus)` |
| `WellSchematic/Annuli.cs`, `SchematicAnnuli`, `AnnulusOverlay` | MAASP per annulus |
| `Integrity/IntegrityToolbox.cs` | `CasingPressureSettings` (API RP 90) |
| `Wellbore.Data/Model/Annuli.cs`, `Pressure.cs` | inputs |

## 3. Screens

`Engineering → Casing pressure`

- Annulus selector with current pressure, MAWOP and MAASP.
- Test record: date, pressure, duration, bleed-off behaviour, temperature.
- Classification panel: API RP 90 class with the rule that produced it, and the recommended action.
- History: pressure trend with the threshold line.
- Schematic overlay: annulus coloured by class.

## 4. Data and contracts

`OgCasingPressureTest { Annulus, Date, Pressure, Unit, Duration, BleedOff, Temperature }`
`OgClassification { Class, Rule, Mawop, Maasp, Action }`.

## 5. States

| State | Behaviour |
|---|---|
| MAWOP cannot be computed | classification refused with the missing input |
| Pressure above MAASP | critical state with the exceedance |
| Test without duration | accepted but marked incomplete |
| Class rule not configured | "unclassified" with the reason |
| No annulus data | screen disabled with the reason |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Record test | `integrity.casingpressure.write` | yes |
| Classify | `integrity.casingpressure.read` | no |

## 7. Acceptance Criteria

- AC-1 Classification names the API RP 90 class and the deciding rule.
- AC-2 MAWOP and MAASP are shown per annulus with their basis.
- AC-3 Pressure above MAASP is critical, never a warning.
- AC-4 A test missing duration is marked incomplete.
- AC-5 The trend uses recorded tests only.
- AC-6 Every threshold comes from settings, not literals.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-04"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-16-ENG-04/observed-evidence.json).

**The open question is answered.** *"API RP 90 class thresholds must live in `CasingPressureSettings`; verify they are
configurable."* They do and they are: `DiagnosticThreshold` (100 psig), `BleedHours` (24), `BuildUpHours` (24), `ZeroTolerance`
(0 psi), the MAWOP fractions (0.5 casing, 0.8 next outer casing, 0.3 outermost, 0.75 inner collapse) and the MAASP deratings
(0.8 burst, 1.0 collapse). The gateway moves the diagnostic threshold to 50 psi for one run, after which the same 60 psi test
classifies as above the threshold where the shipped 100 psi calls it below it — so nothing in the desktop keeps its own copy.

**The limits are the owner's, not the desktop's.**

| Limit | Rule | Worked example (the gateway's data) |
|---|---|---|
| MAWOP | least of the casing MIYP fraction, the next outer casing's, the outer collapse fraction and the inner collapse fraction | 3 435 / **2 061** (outermost) / 5 625 → **2 061 governs** |
| MAASP | outer burst × derating, inner collapse × derating, formation at the outer shoe, wellhead rating | 5 496 / 7 500 / 5 325 / **5 000** → **the wellhead governs** |

Every term and every limit is printed with its value and its arithmetic, because the feature asks for the basis and because the
screening is a flag rather than a design — which the owner's own comment says, and the page repeats.

**AC-3 separates the safety state from the API class.** The class says what the bleed-down showed; the **severity** says what to
do. A pressure above the MAASP is **CRITICAL with the exceedance in psi**, even when the test is below the diagnostic threshold —
the unit test asserts exactly that case. A sustained pressure or a departure is a warning, and a clean test is information.

**AC-4 refuses to invent a verdict.** A test with no valve-closed time is **accepted and stored**, marked **incomplete**, class
**UNCLASSIFIED**, with the reason: without a bleed-down window nothing is judged, and no class is guessed from one reading.

**A gap was closed in the owning module.** The tools name annuli from the strings that reach surface and compute the limits, but
nothing recorded an annulus's **fluid, fracture gradient, wellhead rating** — or a single casing-pressure test. `WELL_ANNULUS` and
`CASING_PRESSURE_TEST` now do, and each test keeps the limits it was judged against so a later settings change does not rewrite a
past conclusion.

**Not built, and stated as gaps:** the annulus schematic overlay (VSB-05 draws annuli from the same records); a plotted pressure
trend with the threshold line (the history is a list of the recorded tests); a full ISO 16530-1 / API RP 90 analysis, since the
MAASP here is the owner's surface-point screening; manufacturer ratings, because the tubular record carries none; and any use of
the recorded temperature, which the screening does not model.

## 9. Open questions and risks

- API RP 90 class thresholds must live in `CasingPressureSettings`; verify they are configurable.
