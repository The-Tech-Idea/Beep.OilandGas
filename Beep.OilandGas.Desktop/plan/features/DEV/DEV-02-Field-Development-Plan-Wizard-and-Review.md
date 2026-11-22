# DEV-02 — Field development plan wizard and review

| Field | Value |
|---|---|
| Stage | STG-04 — Development and drilling |
| Area | Development |
| Status | Gate met - see [evidence](../../evidence/STG-04-DEV-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.DevelopmentPlanning` |
| Desktop project | `Modules/Development/FdpWizard/` |
| External packages | none |
| Depends on | DEV-01 |
| Blocks | DEV-03, DEV-04, ECO-01 |
| Web reference | `Pages/PPDM39/Development/FDPWizard.razor`, `DevelopmentPlanReview.razor`, `DevelopmentAssignedReview.razor`, `DevelopmentReviewHistory.razor`, `Components/Development/MilestoneTracker.razor` |

## 1. Purpose

Author a field development plan as a structured, versioned document with a baseline, then review
it with a recorded decision.

## 2. Source components

| Source | Reuse |
|---|---|
| `DevelopmentPlanning/Services/DevelopmentPlanService.cs`, `.Advanced`, `.Baseline` | draft, submit, baseline |
| `DevelopmentPlanning/Services/IDevelopmentPlanService.cs` | contract |
| Tables `FIELD_DEVELOPMENT_PLAN`, `DEVELOPMENT_COSTS`, `DEVELOPMENT_WELL_SCHEDULE`, `FACILITY_INVESTMENT` | persistence |
| `LifeCycle` approval engine | review decision |

## 3. Screens

`Development → FDP`

Wizard steps: 1 Scope · 2 Reservoirs and well count · 3 Well schedule · 4 Facilities and
capacity · 5 Costs and schedule · 6 Risk and mitigations · 7 Review and submit.

- Each step saves a draft and can be resumed.
- `WizardHost` shows completion state per step.
- Review page: side-by-side current vs baseline, comments, decision panel with rationale.
- History: every baseline with its approval and conditions.

## 4. Data and contracts

`OgFdpDraft { Id, FieldId, Version, State, Steps[], BaselineId? }`
`OgFdpBaseline { Id, Version, ApprovedAt, ApprovedBy, Conditions[] }`

## 5. States

| State | Behaviour |
|---|---|
| Draft resumable | "resume where you left off" with the last step |
| Submitted | read-only; revision creates a new draft |
| Review returned | shows the return reason on the named step |
| Lost acknowledgement on submit | retained original |
| Cost figures incomplete | submit disabled, listing missing steps |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit draft | `development.fdp.write` | yes |
| Submit | `development.fdp.submit` | yes |
| Review decision | `development.fdp.approve` + SoD | yes |

## 7. Acceptance criteria

- AC-1 A draft saves, submits, and cannot be edited after submission.
- AC-2 A revision creates a new baseline without mutating the old one.
- AC-3 Review refusal names the step to correct.
- AC-4 Author cannot approve their own plan.
- AC-5 Costs are `decimal` with currency.
- AC-6 The baseline is readable after any number of revisions.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FdpWizard"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FdpWizardGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-04-DEV-02/observed-evidence.json). The wizard reads and writes
`FIELD_DEVELOPMENT_PLAN` and the DevelopmentPlanning-owned `FDP_DRAFT`, `FDP_BASELINE` and `FDP_REVIEW` through
`UnitofWork<T>`; it contains no statement text.

## 9. Open questions and risks

- Five Web pages cover this area inconsistently; this feature consolidates them and records the
  mapping from old routes.
