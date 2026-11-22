# DEV-01 — Development workspace

| Field | Value |
|---|---|
| Stage | STG-04 — Development and drilling |
| Area | Development |
| Status | Gate met - see [evidence](../../evidence/STG-04-DEV-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.DevelopmentPlanning` |
| Desktop project | `Modules/Development/Workspace/` |
| External packages | none |
| Depends on | WEL-01 |
| Blocks | DEV-02 … DEV-08 |
| Web reference | `Pages/PPDM39/Development/ConstructionProgress.razor` (shared landing), `Pages/lifecycle/development/dashboard` |

## 1. Purpose

The development persona's landing: which plans are in progress, which wells are scheduled, which
reviews are waiting.

## 2. Source components

| Source | Reuse |
|---|---|
| `DevelopmentPlanning/Services/DevelopmentPlanService.cs` and `.Baseline` | plan counts and states |
| `DevelopmentPlanning/Services/IFieldDevelopmentService.cs` | field scope |
| `LifeCycle` persona inbox | assigned work |
| `DefaultSecuritySeedService` | `DEVELOPMENT_PLANNER` and `FACILITIES_ENGINEER` landings |

## 3. Screens

- KPI cards: plans by state, wells planned, wells drilling, facilities in construction, spend to date.
- Persona inbox panel.
- Source links: FDP wizard, plan review, well design, pools and facilities, drilling programs,
  construction.
- Attention strip: plans past their review date, wells without an approved program.

## 4. Data and contracts

`OgDevelopmentCounts { PlanState, Count }`, plus shared inbox items.

## 5. States

| State | Behaviour |
|---|---|
| Failed read | failure panel; no zero counts |
| No plans | "no development plans for this field" with a create action |
| Attention data unavailable | strip hidden with a tooltip, not shown empty |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | `development.read` |
| Open a source page | that page's gate |

## 7. Acceptance criteria

- AC-1 Counts come from records; a failed read is never zero.
- AC-2 Attention items are derived from dates, not stored flags.
- AC-3 Each source link is independently gated.
- AC-4 The landing route matches the persona's configured route.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DevelopmentWorkspace"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DevelopmentWorkspaceGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-04-DEV-01/observed-evidence.json). The workspace reads the
DevelopmentPlanning-owned tables through `UnitofWork<T>`; it contains no statement text.

## 9. Open questions and risks

- The Web reuses `ConstructionProgress.razor` as several personas' landing; the desktop should not.
  Each persona gets its own landing built from this template.
