# RES-02 — Oil properties

| Field | Value |
|---|---|
| Stage | STG-06 — Reservoir, PVT and forecasting |
| Area | Reservoir |
| Status | Gate met - see [evidence](../../evidence/STG-06-RES-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.OilProperties` |
| Desktop project | `Modules/Reservoir/OilProperties/` |
| External packages | none |
| Depends on | DAT-04 |
| Blocks | RES-03, LFT-01 |
| Web reference | `Pages/PPDM39/Calculations/OilProperties.razor` (`/analysis/oil-properties`) |

## 1. Purpose

Black-oil screening: bubble point, solution GOR, formation volume factor, viscosities — with the
correlation named and the screening nature of the result stated.

## 2. Source components

| Source | Reuse |
|---|---|
| `OilProperties/Services/OilPropertiesService.cs`, `.Advanced`, `.CompositionScreening` | `IOilPropertiesService` |
| `OilProperties/Calculations/OilPropertyCalculator.cs` | Standing bubble point, Rs, Bo; Beggs-Robinson viscosity |
| `OilProperties/Calculations/BlackOilScreening.cs` | screening helpers |
| `OilProperties/Constants/OilPropertyConstants.cs`, `OilPropertyUnits.cs` | defaults and unit conversion |
| Tables `OIL_COMPOSITION`, `OIL_PROPERTY_RESULT` | persistence |

## 3. Screens

`Reservoir → Oil properties`

- Inputs: API gravity or density, gas gravity, temperature, pressure, Rs (or bubble point),
  correlation selection.
- Results: Pb, Rs, Bo, dead and saturated viscosity, compressibility, with correlation names.
- Chart: Bo and Rs vs pressure.
- Warning banner: "screening correlations, not a PVT report" — always visible.
- History.

## 4. Data and contracts

Owning-service DTOs. `STOIIP`-style volumes outside scope.

## 5. States

| State | Behaviour |
|---|---|
| Inputs inconsistent (e.g. P < Pb with saturated correlation) | refused with the inconsistency named |
| Correlation out of validity range | warning plus result marked "outside range" |
| Missing gas gravity | the constant default is offered but must be accepted explicitly |
| History unavailable | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run | `properties.oil.read` | no |
| Save | `properties.oil.write` | yes |

## 7. Acceptance criteria

- AC-1 A reference case reproduces Pb, Rs and Bo within tolerance.
- AC-2 Results outside the correlation's validity range are marked.
- AC-3 A default gas gravity is never applied silently.
- AC-4 The screening banner is present on every state of the page.
- AC-5 Every saved result records inputs and correlation.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~OilProperties"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~OilPropertiesGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-06-RES-02/observed-evidence.json). The screening calls the owning
project's `OilPropertyCalculator` directly; the result is read and written through `UnitofWork<T>` over the
OilProperties-owned `OIL_PROPERTY_RESULT`.

**The reference case (AC-1) was computed by hand before the test existed**, from the published relations the engine
implements: 35 API oil, gas gravity 0.65, 200 degF, Rs 600 scf/STB gives Pb **2896.7799 psia**, Rs at 3000 psia
**625.5567 scf/STB**, Bo **1.327592 rb/STB**, dead viscosity **1.7854 cp** and saturated viscosity **0.5139 cp**. The
engine reproduces all five within half a percent, which is the precision the correlations themselves carry.

**The owning table was extended, not duplicated.** `OIL_PROPERTY_RESULT` already held pressure, temperature, Bo,
saturated viscosity, density, the correlation and the date, but none of the inputs AC-5 needs and no way to express
AC-2's range mark or AC-3's assumed default. Eight nullable columns were added rather than a parallel table for the same
concept, so the entity count stays at 387.

**AC-4 is a guard, not a promise:** the banner is a constant on `OilPropertyRules`, the page constructs its banner from
it once in the constructor, and a source-guard test asserts exactly one such construction and no `_banner.Text =`
assignment anywhere in the type - so no success, refusal or failure branch can blank it.

**Not built:** the Bo/Rs-vs-pressure chart (there is still no chart host for these calculation screens), compressibility
(the calculators expose no relation), and the composition-based path (`OilPropertiesService.CompositionScreening`).

## 9. Open questions and risks

- `STANDING_BEGGS` is a string literal in the service; surface the correlation name from a
  reference code or constant rather than hardcoding it in the view.
