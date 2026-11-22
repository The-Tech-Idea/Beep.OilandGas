# WTA-01 — Well test analysis

| Field | Value |
|---|---|
| Stage | STG-08 — Well test and pipeline |
| Area | Well test |
| Status | Gate met - see [evidence](../../evidence/STG-08-WTA-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.WellTestAnalysis` |
| Desktop project | `Modules/WellTest/Analysis/` |
| External packages | `TheTechIdea.OilGas.Production` (`HornerAnalysis`) |
| Depends on | RES-01, RES-02 |
| Blocks | WTA-02, PRD-06 |
| Web reference | `Pages/PPDM39/Calculations/WellTestAnalysis.razor` (`/analysis/well-test`) |

## 1. Purpose

Pressure transient analysis: build-up, drawdown, derivative, gas well, with reservoir model
identification and permeability/skin, and a second independent Horner implementation as a check.

## 2. Source components

| Source | Reuse |
|---|---|
| `WellTestAnalysis/Services/WellTestAnalysisService.cs`, `.Unimplemented` (`IWellTestAnalysisService`) | service |
| `WellTestAnalysis/WellTestAnalyzer.cs` | entry point |
| `WellTestAnalysis/Calculations/BuildUpAnalysis.cs`, `DrawdownAnalysis.cs`, `DerivativeAnalysis.cs`, `GasWellAnalysis.cs` | engine |
| `WellTestAnalysis/Rendering/WellTestRenderer.cs`, `.Configuration` | plot |
| `WellTestAnalysis/Interaction/WellTestInteractionHandler.cs` | plot interaction |
| `WellTestAnalysis/Constants/WellTestConstants.cs` | 162.6 coefficient and conversions |
| `Beep.OilandGas.Models` `WellTestAnalysisWellKnown` | analysis and method tokens |
| `Production` `HornerAnalysis.Analyze` | cross-check |
| Tables `WELL_TEST`, `WELL_TEST_FLOW_MEAS`, `WELL_TEST_ANALYSIS_RESULT`, `R_WELL_TEST_TYPE` | persistence |

## 3. Screens

`Well test → Analysis`

- Data tab: test selector or manual entry (time, pressure, rate), units per column, quality flags.
- Method tab: analysis type (build-up / drawdown), method (Horner / MDH), radial-flow window
  selection on the derivative plot.
- Plot tab: log-log diagnostic, semi-log straight line, derivative overlay, straight-line fit via
  the renderer and interaction handler.
- Results tab: permeability, skin, wellbore storage, reservoir pressure, confidence notes.
- Cross-check tab: the tools Horner result and the delta.
- Save and link to the test record (WTA-02).

## 4. Data and contracts

`OgWellTestResult { AnalysisType, Method, Permeability, Skin, WellboreStorage, ReservoirPressure,
FlowWindow, R2, Converged }`.

## 5. States

| State | Behaviour |
|---|---|
| No radial-flow window | result is refused; the plot explains what to select |
| Non-converged derivative | shown as unusable, not plotted as data |
| Mixed units | refused with the offending column |
| Cross-check disagreement | both values shown with the delta |
| Unimplemented analysis type | the option is disabled and labelled |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Analyse | `welltest.read` | no |
| Save result | `welltest.write` | yes |
| Link to test record | `welltest.write` | yes |

## 7. Acceptance criteria

- AC-1 A reference build-up yields permeability within 1 % of the published value.
- AC-2 Analysis type and method are recorded tokens.
- AC-3 A missing radial-flow window is refused, not guessed.
- AC-4 The tools Horner agrees within tolerance or the difference is shown.
- AC-5 Plots are drawn by `WellTestRenderer`.
- AC-6 Every saved result records the flow window and the method.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WTA-01"
```

## 9. Open questions and risks

- `WellTestAnalysisService.Unimplemented.cs` implies some analyses are stubs. Enumerate which work
  before designing the method list, and disable the rest.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-08-WTA-01/observed-evidence.json). Results are read and written
through `UnitofWork<T>` over the WellTestAnalysis-owned `WELL_TEST_ANALYSIS_RESULT`, extended additively with the window,
the derivative verdict and the cross-check.

**The open question above, answered by enumerating the file.** `WellTestAnalysisService.Unimplemented.cs` contains stubs
for type-curve matching, multi-rate analysis, deconvolution, boundary detection, reporting, plot generation and result
persistence. What is real is `BuildUpAnalysis` (Horner and MDH), `DrawdownAnalysis`, `DerivativeAnalysis`
(`CalculateDerivative`, `IdentifyModel`, `IdentifyFlowRegimes`) and `GasWellAnalysis`. The screen offers **only the real
ones** - BUILDUP with Horner or MDH, DRAWDOWN with MDH - and the workspace lists the rest as unavailable, so an engineer is
never handed a stub.

**Gas build-up is not surfaced, deliberately.** `AnalyzeGasBuildUp` works, but `WellTestAnalysisWellKnown` has only
`BUILDUP` and `DRAWDOWN` as analysis-type tokens; offering gas would mean inventing a token or labelling a gas test as an
oil build-up. It is listed as unavailable with that reason.

**The radial-flow window is the engineer's.** The readings inside the window are sliced and handed to the engine, so a
result always says which part of the test it came from (AC-6), and the four ways a window can be unusable - missing, empty,
too small, backwards - are four separate refusals rather than a guess.

**One limit stated plainly:** `BuildUpAnalysis` runs its own middle-time-region search and fits at most ten points from the
end of what it is given, without reporting which. `POINTS_IN_WINDOW` therefore records the window handed to the engine, not
the internal fit; the record does not claim more than it knows.

**Not built:** the plots (`WellTestRenderer` and the interaction handler need the chart host these calibration screens do
not have, so the straight line, the model and the derivative verdict are printed), the derivative series (only its verdict
and count are kept), and gas build-up. WTA-02's register link is the next feature.
