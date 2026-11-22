# EXP-01 — Exploration workspace

| Field | Value |
|---|---|
| Stage | STG-03 — Exploration and seismic |
| Area | Exploration |
| Status | Gate met - AC-1..AC-4: counts from records with a failed read showing no counts, every source link decided by the shell's page permission (missing pages named with the feature that adds them), the incomplete-risk warning counting only prospects missing elements, and the exploration persona's landing route derived from the RBAC `DEFAULT_LANDING_ROUTE` and guarded by a test. See [evidence](../../evidence/STG-03-EXP-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProspectIdentification` |
| Desktop project | `Modules/Exploration/Workspace/` |
| External packages | none |
| Depends on | WEL-01 |
| Blocks | EXP-02 … EXP-05 |
| Web reference | `Pages/PPDM39/Exploration/ExplorationDashboard.razor`, `Pages/Lifecycle/.../ProspectBoard.razor` (`/lifecycle/exploration/dashboard`) |

## 1. Purpose

The exploration persona's landing page: what is in the funnel, what is waiting on me, and where
the gaps are.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProspectIdentification/Services/ProspectIdentificationService.cs` | counts by stage |
| `ProspectIdentification/Services/PPDMExplorationService.cs` | field-scoped reads |
| `UserManagement/Services/DefaultSecuritySeedService.cs` | persona `EXPLORATION_GEOLOGIST` and landing route |
| `LifeCycle` persona inbox | assigned work |

## 3. Screens

- Header: field context, persona name, period.
- KPI cards: prospects by stage, plays, leads, wells planned, wells drilling.
- Persona inbox panel (reused `PersonaAssignedWorkPanel` behaviour).
- "Sources" links: prospect board, prospects, seismic, well program approval, lease (LSE-01).
- Data-quality warning strip when the funnel has prospects with missing risk elements.

## 4. Data and contracts

`OgFunnelCounts { Stage, Count }`, plus the shared inbox item shape from WFL-05.

## 5. States

| State | Behaviour |
|---|---|
| Failed read | failure panel; no zero counts |
| Empty field | "no exploration activity recorded for this field" |
| Incomplete risk data | warning strip with a count and a link |
| No persona | landing falls back to the dashboard |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | `exploration.read` |
| Open a source page | that page's gate |

## 7. Acceptance criteria

- AC-1 Counts come from records; a failed read is never zero.
- AC-2 Every source link is gated by its own page permission.
- AC-3 The incomplete-risk warning counts only prospects with missing elements.
- AC-4 The persona landing route matches the RBAC `DEFAULT_LANDING_ROUTE`.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~EXP-01"
```

## 9. Open questions and risks

- The Web landing was rewritten after removing invented chance/stage values; do not reintroduce
  any computed probability here.
