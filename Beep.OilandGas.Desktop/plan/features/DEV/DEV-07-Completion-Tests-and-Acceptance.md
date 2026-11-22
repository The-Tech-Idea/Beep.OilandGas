# DEV-07 — Completion tests and acceptance

| Field | Value |
|---|---|
| Stage | STG-04 — Development and drilling |
| Area | Development |
| Status | Gate met - see [evidence](../../evidence/STG-04-DEV-07/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.DrillingAndConstruction` |
| Desktop project | `Modules/Development/Completion/` |
| External packages | `TheTechIdea.OilGas.WellSchematic` (completion schematic) |
| Depends on | DEV-06 |
| Blocks | PRD-01, WTA-01 |
| Web reference | `Pages/PPDM39/Drilling/DrillingCompletionAssigned.razor`, `DrillingCompletionHistory.razor`, and the 8 completion components under `Components/Workflow/DrillingCompletion*` |

## 1. Purpose

Prove the well is finished correctly: record completion tests, gather acceptance evidence, take an
independent decision, and hand the well to production.

## 2. Source components

| Source | Reuse |
|---|---|
| `DrillingAndConstruction/Services/DrillingOperationService.CompletionTests.cs` | test records |
| Tables `WELL_TEST`, `WELL_COMPLETION` | persistence |
| `Components/Workflow/DrillingCompletionSubmissionPanel.razor`, `…TestPanel`, `…EvidenceView`, `…DecisionPanel`, `…AcceptancePanel`, `…RoutingView`, `…ReportEditor` | behaviour to reproduce |
| `WellSchematic` completion rendering | evidence visual |

## 3. Screens

`Development → Completion`

- Test list: test type, interval, result, unit, performed by, date, attachment.
- Submission panel: summary, evidence checklist, report editor, submit.
- Review panel: assigned reviewer, evidence viewer, decision (accept / accept with conditions /
  return / reject) with rationale.
- Acceptance record: conditions, handover date, recipient.
- Completion schematic preview.

## 4. Data and contracts

`OgCompletionTest { TestType, TopDepth, BaseDepth, DepthUnit, Result, ResultUnit, PerformedBy,
PerformedAt, EvidenceRef }`
`OgCompletionAcceptance { WellId, AcceptedAt, AcceptedBy, Conditions[], HandoverTo }`.

## 5. States

| State | Behaviour |
|---|---|
| No passing test | acceptance is refused |
| Evidence missing | submission disabled with the missing list |
| Returned | reason shown on the failing item |
| Lost acknowledgement | retained original |
| Foreign well in the evidence | refused |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Record test | `drilling.completion.write` | yes |
| Submit | `drilling.completion.submit` | yes |
| Accept | `drilling.completion.approve` + SoD | yes |

## 7. Acceptance criteria

- AC-1 Acceptance requires at least one passed test record.
- AC-2 Evidence belongs to the same well; a foreign well is refused.
- AC-3 Author cannot accept their own submission.
- AC-4 Acceptance conditions appear on the production handover record.
- AC-5 Every test carries a result unit.
- AC-6 A lost acknowledgement does not duplicate the acceptance.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~Completion"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~CompletionGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-04-DEV-07/observed-evidence.json). Tests, submissions, evidence and
the acceptance are read and written through `UnitofWork<T>` over the DrillingAndConstruction-owned
`DEVELOPMENT_COMPLETION_TEST`, `DEVELOPMENT_COMPLETION_SUBMISSION` and `DEVELOPMENT_COMPLETION_EVIDENCE`; the wells come
from the field's `DEVELOPMENT_DRILLING_EXECUTION` rows. The plan-stage record is separate from the PPDM `WELL_COMPLETION`
and `WELL_TEST` interchange tables, which this screen does not read or write.

**The eight Web components, mapped (section 9):** `DrillingCompletionAssigned` and `DrillingCompletionHistory` are the
submission picker plus the test and evidence grids at route `completion`; the eight `DrillingCompletion*` workflow
components collapse to five behaviours in 1.0 - record a test, attach evidence, submit, decide (accept, accept with
conditions, return or reject), and read the handover record. The remaining component behaviours (a per-test attachment
browser past a reference string, and a separate assignee queue screen) are not built here and are named as limits in the
evidence rather than implied by the page.

## 9. Open questions and risks

- Eight Web components exist for completion; only five behaviours are needed in 1.0. Map them
  explicitly in this document before coding.
