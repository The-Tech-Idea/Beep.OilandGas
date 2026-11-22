# DEV-08 — Construction progress and handover

| Field | Value |
|---|---|
| Stage | STG-04 — Development and drilling |
| Area | Development |
| Status | Gate met - see [evidence](../../evidence/STG-04-DEV-08/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.DevelopmentPlanning`, `Beep.OilandGas.ProductionOperations` |
| Desktop project | `Modules/Development/Construction/` |
| External packages | none |
| Depends on | DEV-04 |
| Blocks | WRK-01, PRD-02 |
| Web reference | `Pages/PPDM39/Development/ConstructionProgress.razor`, `Components/FacilityEditDialog.razor`, `Components/Workflow/CompanyFacilityEvidencePanel.razor` |

## 1. Purpose

Track facility construction from award to handover, with recorded evidence per milestone, and hand
the asset to operations with a real acceptance record.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionOperations/Services/FacilityManagementService.*` (14 partials incl. Development creation/edit) | facility lifecycle |
| `DevelopmentPlanning` `FACILITY_INVESTMENT` | investment linkage |
| Tables `FACILITY`, `FACILITY_MEASUREMENT`, `FACILITY_EQUIPMENT_ACTIVITY` | persistence |

## 3. Screens

`Development → Construction`

- Facility list: name, type, state, planned vs actual milestone dates, spend vs budget.
- Milestone tracker: design → procurement → fabrication → installation → commissioning → handover.
- Evidence panel per milestone: document, date, reviewer, outcome.
- Equipment evidence reader: recorded equipment for the facility with active state.
- Handover panel: recipient, date, conditions, acceptance signature record.

## 4. Data and contracts

`OgConstructionMilestone { Name, PlannedDate, ActualDate, State, EvidenceRefs[] }`
`OgFacilityHandover { FacilityId, HandedOverAt, HandedOverTo, Conditions[], Accepted }`.

## 5. States

| State | Behaviour |
|---|---|
| Milestone overdue | red chip with days overdue |
| Evidence missing | milestone cannot complete |
| Unresolved original | creation and edit controls hidden until resolved (the Web establishes this rule) |
| Foreign facility selection | refused |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Update milestone | `development.construction.write` | yes |
| Record evidence | `development.construction.write` | yes |
| Handover | `development.construction.handover` | yes |

## 7. Acceptance criteria

- AC-1 A milestone cannot complete without evidence.
- AC-2 Overdue is computed from dates.
- AC-3 An unresolved prior submission hides create/edit controls.
- AC-4 Handover records recipient, date and conditions.
- AC-5 Recorded equipment does not by itself prove installation, testing or acceptance — say so.
- AC-6 Facility selection is revalidated before and after a write.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~Construction"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ConstructionGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-04-DEV-08/observed-evidence.json). Milestones, their evidence and the
handover are read and written through `UnitofWork<T>` over the DevelopmentPlanning-owned `FACILITY_MILESTONE`,
`FACILITY_MILESTONE_EVIDENCE` and `FACILITY_HANDOVER`, beside the `FACILITY_INVESTMENT` row they belong to; the equipment
panel reads the PPDM `FACILITY_EQUIPMENT` rows and writes nothing.

**The plan's contract, implemented:** `OgConstructionMilestone { Name, PlannedDate, ActualDate, State, EvidenceRefs[] }`
is `OgConstructionMilestone` with its `Evidence` list, and `OgFacilityHandover { FacilityId, HandedOverAt, HandedOverTo,
Conditions[], Accepted }` is `OgFacilityHandover` with `Accepted` always true because the row exists only when the asset
was accepted. `OgUpstreamHandover` shows DEV-07's accepted well completions as this screen's input, unchanged.

**"Unresolved original" (section 5):** the document names the rule without defining the original. This screen treats the
newest rejected evidence item of the facility as the unresolved submission and names it; the funded company flow's
`FACILITY_EDIT_INTENT` / `FACILITY_CREATE_INTENT` proofs are neither read nor written here. That is recorded as a limit in
the evidence rather than presented as the Web's own state.

## 9. Open questions and risks

- The Web explicitly warns that recorded active equipment is not evidence of acceptance. Preserve
  that wording; do not upgrade the claim.
