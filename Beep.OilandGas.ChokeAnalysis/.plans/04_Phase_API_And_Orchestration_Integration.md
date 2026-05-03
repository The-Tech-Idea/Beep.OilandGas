# Phase 4 — API and orchestration integration

## Goal

Choke analysis exposed via the shared **calculation orchestration** API remains stable, authorized, and consistent with **field context** patterns used elsewhere.

## Current integration (baseline)

| Layer | Responsibility |
|-------|------------------|
| HTTP | `Beep.OilandGas.ApiService/Controllers/CalculationsController.cs` — `POST api/calculations/choke` |
| Orchestration | `ICalculationService.PerformChokeAnalysisAsync(ChokeAnalysisRequest)` (implementation aggregates choke + other engines) |
| Domain | `ChokeAnalysisService` — pure choke math |

Verify actual registration in **`Program.cs`**: `IChokeAnalysisService` → `ChokeAnalysisService`, and `ICalculationService` wiring includes choke delegation.

## Field orchestrator

Many calculation endpoints merge **`IFieldOrchestrator.CurrentFieldId`** into requests when field id is empty. Choke endpoint should follow the **same pattern** as DCA/Nodal if choke requests carry `FieldId` / well scope:

- [ ] Compare choke action with `PerformDCAAnalysis` field injection pattern.
- [ ] If choke requests support field linkage, ensure orchestrator fills `FieldId` when missing (document in XML on action).

## Controller conventions

From [CLAUDE.md](../../CLAUDE.md) / architecture commands:

- `[ApiController]`, meaningful route, **`[Authorize]`** where required (controller-level or global policy).
- Use `User.FindFirst(ClaimTypes.NameIdentifier)` for audit user id when persisting results.
- Return **`BadRequest`** for validation failures with safe messages (avoid leaking internals).

## TODO checklist

- [ ] Trace `ChokeAnalysisRequest` → service mapping inside `ICalculationService` implementation; ensure no duplicate choke logic outside `IChokeAnalysisService`.
- [ ] Confirm DI: `AddBeepServices` / metadata available before any service needing `IDMEEditor` (read `Program.cs` lines 1–120).
- [ ] Progress tracking: if choke runs become long-running, align with `IProgressTrackingService` pattern used by DCA (optional enhancement).
- [ ] Error contract: align choke errors with other calculation endpoints (consistent JSON shape).

## Verification

```bash
dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
```

Manual smoke (when server running): `POST /api/calculations/choke` with minimal valid body from README examples.

## References

- [07_Scenarios_Best_Practices_And_Industry_Reference.md](07_Scenarios_Best_Practices_And_Industry_Reference.md) §1 — operational scenarios (allocation, testing, gas lift interaction) that drive API response shape and disclaimers.
- `.cursor/commands/architecture-patterns.md`
- `Beep.OilandGas.Web/MudBlazor_Docs/README.md` — only if adding/choking-related UI later.
