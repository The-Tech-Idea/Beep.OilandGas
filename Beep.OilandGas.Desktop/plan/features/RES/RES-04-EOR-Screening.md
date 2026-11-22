# RES-04 — EOR screening

| Field | Value |
|---|---|
| Stage | STG-06 — Reservoir, PVT and forecasting |
| Area | Reservoir |
| Status | Gate met - see [evidence](../../evidence/STG-06-RES-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.EnhancedRecovery` |
| Desktop project | `Modules/Reservoir/Eor/` |
| External packages | none |
| Depends on | RES-03 |
| Blocks | RES-05, ECO-01 |
| Web reference | `Pages/PPDM39/Reservoir/EORScreening.razor`, `Pages/PPDM39/Operations/EnhancedRecovery.razor` |

## 1. Purpose

Screen enhanced recovery candidates against reservoir and fluid conditions, with the screening
method and its assumptions shown rather than buried.

## 2. Source components

| Source | Reuse |
|---|---|
| `EnhancedRecovery/Services/EnhancedRecoveryService.cs`, `.Advanced` | `IEnhancedRecoveryOperationsService`, `IEnhancedRecoveryService` |
| `EnhancedRecovery/Data/Constants/EnhancedRecoveryConstants.cs`, `EnhancedRecoveryReferenceSets.cs`, `EnhancedRecoveryReferenceCodeSeed.cs` | method codes and constants |
| Tables `PDEN`, `PDEN_FLOW_MEASUREMENT`, `R_ENHANCED_RECOVERY_REFERENCE_CODE` | persistence |

## 3. Screens

`Reservoir → EOR screening`

- Reservoir and fluid inputs (from the selected pool where available, editable).
- Method checklist: water flood, gas injection, CO₂ miscible, polymer, ASP/chemical, steam,
  thermal — each with eligibility, score, and the rule that decided it.
- Economics panel: incremental recovery, cost per barrel, incremental NPV (delegates to ECO-01).
- Recommendation panel with caveats.

## 4. Data and contracts

`OgEorCandidate { Method, Eligible, Score, DecidingRule, IncrementalRecovery, CostPerBbl,
Currency }`.

## 5. States

| State | Behaviour |
|---|---|
| Missing reservoir input | method marked "insufficient data", never ineligible |
| Method outside its depth/gravity window | ineligible with the window stated |
| Economics unavailable | the engineering screen still works; the economics panel says unavailable |
| Two sources disagree (screening vs recorded scheme) | both shown |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Screen | `reservoir.eor.read` | no |
| Save a scheme | `reservoir.eor.write` | yes |

## 7. Acceptance criteria

- AC-1 Every method states the rule that decided eligibility.
- AC-2 Insufficient data is distinct from ineligible.
- AC-3 Constants come from `EnhancedRecoveryConstants`, not from the view.
- AC-4 The economics panel never invents a value when economics is unavailable.
- AC-5 A saved scheme links to the pool and the screening inputs.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~EorScreening"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~EorScreeningGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-06-RES-04/observed-evidence.json). The screens read the
EnhancedRecovery module's constants; the run and its candidates are read and written through `UnitofWork<T>` over the
EnhancedRecovery-owned `EOR_SCREENING_RUN` and `EOR_SCREENING_CANDIDATE`.

**Where the thresholds came from.** They are not recalled: they are Taber, Martin & Seright (1997), Table 3, plus that
paper's own depth-versus-gravity table for CO2. They were added to `EnhancedRecoveryConstants` - the type AC-3 names -
as a `ScreeningWindow` record and a `ScreeningWindows` dictionary keyed by the `EorMethodConstants` codes, and every
recovery factor and chemical cost was already there. A guard test compares each window and recovery factor to the
owner's dictionary and asserts the page contains none of the threshold literals and never mentions the constants type.

**Insufficient data is its own state (AC-2).** A method whose window needs a value the inputs do not carry is
`INSUFFICIENT_DATA`, names the missing value, and says it is "not judged rather than called ineligible". The workspace
counts the three states separately, and with no viscosity at all nothing is ineligible while water flooding - which
needs no property - is still judged.

**The economics is only what the module states (AC-4, SG-06-5).** The panel reports the module's screening recovery
factor labelled `% OOIP (screening default)`, and a cost only for the chemical methods, which is
`ChemicalTypeConstants`' cost **per unit of chemical** with that basis in words. A cost per incremental barrel and an
incremental NPV belong to ECO-01, which is not built, so the panel says they are "not shown rather than estimated".

**The score** is this screen's own ranking heuristic - the mean relative margin inside the window, 1 at the ideal end -
and is labelled as a way to order eligible methods, not a Taber value.

**A scope boundary, stated:** the screen uses the module's constants and reference sets but does not call
`EnhancedRecoveryService.Advanced`. Those are instance methods that need an `IDMEEditor` and answer different questions
(waterflood performance, gas-injection miscibility, chemical and thermal recovery) from the screening this feature
defines. That is recorded as a limit rather than presented as wired.

## 9. Open questions and risks

- `EnhancedRecoveryService.Advanced.cs` contains uncentralized recovery factors and chemical costs
  (recorded in `ENGINEERING_PROJECTS_ANALYSIS.md`). Do not mirror those numbers into the view;
  file a constants task instead.
