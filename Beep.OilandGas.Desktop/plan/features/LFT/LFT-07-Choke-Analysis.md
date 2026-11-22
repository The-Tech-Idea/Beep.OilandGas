# LFT-07 — Choke analysis

| Field | Value |
|---|---|
| Stage | STG-07 — Well performance and artificial lift |
| Area | Lift |
| Status | Gate met - see [evidence](../../evidence/STG-07-LFT-07/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ChokeAnalysis` |
| Desktop project | `Modules/Lift/Choke/` |
| External packages | none |
| Depends on | RES-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Calculations/ChokeAnalysis.razor` (`/analysis/choke`) |

## 1. Purpose

Size a choke and predict the resulting rate or pressure, stating the correlation and flow regime
used, and screening for erosion.

## 2. Source components

| Source | Reuse |
|---|---|
| `ChokeAnalysis/Services/ChokeAnalysisService.cs`, `.Advanced`, `.Multiphase` | `IChokeAnalysisService` |
| `ChokeAnalysis/Calculations/GasChokeCalculator.cs` | Gilbert, Ros, Achong, Pilehvari, Sachdeva, Baxendell |
| `ChokeAnalysis/Constants/ChokeConstants.cs`, `ChokeAnalysisReferenceCodes.cs`, `ChokeAnalysisReferenceSets.cs`, `ChokeFlowRegime.cs`, `ChokeType.cs` | constants, codes, enums |
| Tables `CHOKE_PROPERTIES`, `GAS_CHOKE_PROPERTIES`, `CHOKE_FLOW_RESULT`, `R_CHOKE_ANALYSIS_REFERENCE_CODE` | persistence |

## 3. Screens

`Lift → Choke`

- Inputs: upstream/downstream pressure, temperature, GOR, gas gravity, choke size, choke type,
  correlation.
- Result: flow rate or downstream pressure, flow regime (sonic / subsonic), correlation used.
- Sensitivity: rate vs choke size curve with the chosen size marked.
- Erosion panel: predicted erosion rate and the threshold with its source.
- History.

## 4. Data and contracts

`OgChokeResult { Rate, RateUnit, DownstreamPressure, Regime, Correlation, ErosionRate,
ErosionThreshold, ThresholdSource }`.

## 5. States

| State | Behaviour |
|---|---|
| Sonic/subsonic undetermined | refused with the input that could not decide |
| Correlation chosen but out of range | warning plus result marked |
| Erosion above threshold | red chip with the threshold source |
| Missing GOR | refused |
| History unavailable | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Analyse | `lift.choke.read` | no |
| Save result | `lift.choke.write` | yes |

## 7. Acceptance criteria

- AC-1 A reference case reproduces the expected rate within tolerance.
- AC-2 Flow regime and correlation are displayed with every result.
- AC-3 An erosion threshold always names its source.
- AC-4 Choke types and regimes come from the owning enums, not strings.
- AC-5 Results are deterministic for the same inputs.
- AC-6 Saved results record the correlation and regime.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LFT-07"
```

## 9. Open questions and risks

- `ChokeAnalysisService.Advanced.cs` has uncentralized magic numbers (choke adjustment factors,
  bounds, efficiency bases) recorded in `ENGINEERING_PROJECTS_ANALYSIS.md` §3. Do not surface those
  values in the UI as if they were published constants.
