# DEV-05 — Drilling program authoring and review

| Field | Value |
|---|---|
| Stage | STG-04 — Development and drilling |
| Area | Development |
| Status | Gate met - see [evidence](../../evidence/STG-04-DEV-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.DrillingAndConstruction` |
| Desktop project | `Modules/Development/DrillingProgram/` |
| External packages | `TheTechIdea.OilGas.Drilling` (`WellPlan`, `Hydraulics`, `TorqueDrag`) |
| Depends on | DEV-03 |
| Blocks | DEV-06 |
| Web reference | `Pages/PPDM39/Drilling/DrillingProgramAssignedReview.razor`, `Pages/PPDM39/Operations/DrillingOperations.razor` |

## 1. Purpose

Author the drilling program: hole sections, casing, mud, bits, cement, time and cost — then have it
independently reviewed before a rig is mobilised.

## 2. Source components

| Source | Reuse |
|---|---|
| `DrillingAndConstruction/Services/DrillingOperationService.cs` and `Program*` partials | program lifecycle |
| `DrillingAndConstruction/Services/DrillingEngineeringService.cs`, `PipeSection` | engineering inputs |
| Tables `DRILLING_PROGRAM`, `CASING_PROGRAM`, `CEMENT_JOB`, `DRILLING_FLUID`, `DRILLING_BIT` | persistence |
| `TheTechIdea.OilGas.Drilling/WellPlan.cs` | trajectory stations (ENG-06) |
| `TheTechIdea.OilGas.Drilling/Hydraulics.cs`, `TorqueDrag.cs` | checks (ENG-07, ENG-08) |

## 3. Screens

`Development → Drilling program`

- Header: well, plan version, prepared by, state, approvals.
- Sections grid: hole size, top, bottom, casing, mud, bit, cement, duration, cost.
- Checks panel: hydraulics ECD, torque & drag, casing design — each with a status and a link to
  the detailed screen.
- Risks and mitigations list.
- Review panel: assigned reviewer, comments, decision with rationale.
- Version history.

## 4. Data and contracts

`OgDrillProgram { Id, WellId, Version, State, Sections[], TotalCost, Currency, Checks[] }`
`OgDrillSection { HoleSize, HoleSizeUnit, Top, Bottom, DepthUnit, CasingOd, MudWeight, DurationDays, Cost }`.

## 5. States

| State | Behaviour |
|---|---|
| Check failed | the program can be submitted but the failure is recorded and shown to the reviewer |
| Check not run | shown as "not run", not "passed" |
| Submitted | read-only; revision creates a new version |
| Reviewer is the author | approval refused |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit program | `drilling.program.write` | yes |
| Submit | `drilling.program.submit` | yes |
| Review decision | `drilling.program.approve` + SoD | yes |

## 7. Acceptance criteria

- AC-1 A program cannot be submitted with an incomplete section.
- AC-2 Design checks that were not run are shown as not run.
- AC-3 The author cannot approve.
- AC-4 A revision never mutates the approved version.
- AC-5 Depths and diameters always carry units.
- AC-6 Total cost equals the sum of section costs.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DrillingProgram"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DrillingProgramGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-04-DEV-05/observed-evidence.json). The program is read and written
through `UnitofWork<T>` over the DrillingAndConstruction-owned `DEVELOPMENT_DRILLING_PROGRAM`, `DRILLING_PROGRAM_SECTION`,
`DRILLING_PROGRAM_CHECK` and `DRILLING_PROGRAM_REVIEW`, with the DEV-03 completeness rule consumed rather than copied.

**Storage decision (recorded here because it changes the source components in section 2):** the module's
`DRILLING_PROGRAM` is the *funded company* program. Its schema requires an AFE id, an investment handoff id and the
bounded original draft proof, and `DrillingOperationService` reads it by AFE; a plan-stage program has none of those, and
the framework's `[Required]` validation refuses empty values. DEV-05 therefore authors the plan-stage program in
`DEVELOPMENT_DRILLING_PROGRAM` (with section/check/review children), and the company program is created from it by the
module's funding flow. `CASING_PROGRAM`, `CEMENT_JOB`, `DRILLING_FLUID` and `DRILLING_BIT` remain the execution-side
records and are not written here.


## 9. Open questions and risks

- Engineering checks call into STG-16 features; until they exist the panel shows "not run" with a
  link, never a fabricated pass.
