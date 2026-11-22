# PRD-06 — Intervention decisions and engineer workspace

| Field | Value |
|---|---|
| Stage | STG-05 — Production operations and allocation |
| Area | Production |
| Status | Gate met - see [evidence](../../evidence/STG-05-PRD-06/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionOperations` |
| Desktop project | `Modules/Production/Interventions/` |
| External packages | `TheTechIdea.OilGas.Production` (`Vogel`, `DeclineFitter`) |
| Depends on | PRD-01, WTA-01 |
| Blocks | LFT-01, WRK-01 |
| Web reference | `Pages/PPDM39/Production/WellPerformanceOptimizer.razor`, `InterventionDecisions.razor`, `ProductionEngineerWorkspace.razor`, `Components/Shared/LiftRecommendationCard.razor` |

## 1. Purpose

Decide what to do about an underperforming well, with the diagnosis and the options recorded, and
turn the decision into a work order.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionOperations/Services/IProductionOperationsAdvancedService.cs` | diagnostics |
| `NodalAnalysis/Services/NodalAnalysisService.cs` `.ArtificialLiftScoring` | lift ranking |
| `WellTestAnalysis` | recent test evidence |
| `Production` `Vogel`, `DeclineFitter` | quick screening |
| `Components/Shared/LiftRecommendationCard.razor` | recommendation presentation |

## 3. Screens

`Production → Interventions`

- Workspace: well list with performance flags.
- Diagnosis panel: symptoms, probable causes, supporting evidence, confidence.
- Options panel: candidate interventions with estimated gain, cost, risk, and a lift-method
  recommendation when artificial lift applies.
- Decision: chosen option, rationale, owner, target date.
- Action: create work order (WRK-01) or study (LFT-01).

## 4. Data and contracts

`OgInterventionOption { Method, EstimatedGain, GainUnit, EstimatedCost, Currency, Risk, Basis }`
`OgInterventionDecision { WellId, Option, Rationale, Owner, TargetDate, State }`.

## 5. States

| State | Behaviour |
|---|---|
| No evidence | diagnosis is "insufficient data" with the missing items |
| Conflicting evidence | both shown, no automatic resolution |
| Option without basis | not selectable |
| Work order creation lost | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `production.read` | no |
| Record decision | `production.intervention.write` | yes |
| Create work order | `work.write` | yes |

## 7. Acceptance criteria

- AC-1 A diagnosis without evidence says "insufficient data".
- AC-2 Each option states its basis (test, analogue, model).
- AC-3 Estimated gain carries a unit.
- AC-4 A decision records rationale and owner.
- AC-5 Creating a work order carries the decision reference and cannot be created twice for the
  same decision.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~Intervention"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~InterventionGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-05-PRD-06/observed-evidence.json). The diagnosis reads the PPDM
`WELL`, `WELL_STATUS`, `PDEN_WELL` and `PDEN_VOL_SUMMARY` rows and the `PRODUCTION_DEFERMENT` (PRD-03) and
`RECONCILIATION_VARIANCE` (PRD-05) rows; the decision, its options and the method code book are read and written through
`UnitofWork<T>` over the feature's own `INTERVENTION_DECISION`, `INTERVENTION_OPTION` and `R_INTERVENTION_METHOD`.

**The plan's contract, implemented:** `OgInterventionOption { Method, EstimatedGain, GainUnit, EstimatedCost, Currency,
Risk, Basis }` is `OgInterventionOption` with the same members plus its basis text, and `OgInterventionDecision { WellId,
Option, Rationale, Owner, TargetDate, State }` is `OgInterventionDecision` with the chosen option resolved from its
options. `R_INTERVENTION_METHOD` is not seeded: the methods are the operating company's data, and an empty set refuses
every option with a stated reason - the same rule PRD-03 applies to deferment reasons.

**How the confidence is computed (section 5):** one quarter for each of the four checks that produced evidence (rate,
deferment, variance, status), capped at one half while a conflict is unresolved. It is a stated rule, not a model score.

**The open question in section 9, answered:** nothing is computed on the client. This screen records the decision; a
lift-method recommendation belongs to LFT-01, and when the chosen method's reference row says it changes the lift
arrangement the page says exactly that. Vogel and DeclineFitter are not used, because no lift design is computed here.

**What is not this screen:** the work order is WRK-01 and the study is LFT-01. Neither is built, so `CreateWorkOrderAsync`
records the reference the decision was raised under and refuses a second one (AC-5) rather than creating a `WORK_ORDER`
row it does not own.

## 9. Open questions and risks

- `LiftRecommendationCard` may compute values on the client. Confirm it reads from
  `ArtificialLiftScoring`, which is the owning implementation.
