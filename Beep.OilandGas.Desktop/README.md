# Beep.OilandGas.Desktop

A Windows desktop (WPF, .NET 10) Oil & Gas workstation that brings **every project in
`Beep.OilandGas`** and **every project in `OilandGasTools`** into one shell — modelled on
how `Beep.DoitAll.Erp.Wpf` is built.

This folder is the delivery container. Nothing here runs yet; it holds the plan, the
solution skeleton layout, and the test plan.

## Folder map

| Folder | Purpose |
|---|---|
| `plan/` | The complete staged plan: master tracker, architecture, conventions, decisions, 19 stage documents, 127 feature documents. |
| `src/` | The WPF solution (`Beep.OilandGas.Desktop.slnx`) and its projects. Created in Stage 0. |
| `test/` | Unit, integration and smoke test projects. Created in Stage 0. |

## Start here

1. [`plan/00-MASTER-TRACKER.md`](plan/00-MASTER-TRACKER.md) — status of every stage and feature, and the module coverage matrix.
2. [`plan/01-ARCHITECTURE.md`](plan/01-ARCHITECTURE.md) — how the app is put together.
3. [`plan/07-LOGGING-AUDIT-AND-NOTIFICATIONS.md`](plan/07-LOGGING-AUDIT-AND-NOTIFICATIONS.md) — the one log/audit/exception pipeline and the toast service that shows it. **Mandatory reading before any `catch` is written.**
4. [`plan/02-DOCUMENT-CONVENTIONS.md`](plan/02-DOCUMENT-CONVENTIONS.md) — how plans and evidence are written.
5. [`plan/03-SOLUTION-LAYOUT.md`](plan/03-SOLUTION-LAYOUT.md) — the exact projects, namespaces and folder structure.
6. [`plan/04-DECISIONS.md`](plan/04-DECISIONS.md) — the decisions that must be settled before code.
7. [`plan/05-MODULE-MAP.md`](plan/05-MODULE-MAP.md) — every source project mapped to the stage and features that consume it.
8. [`plan/06-DEPENDENCY-GRAPH.md`](plan/06-DEPENDENCY-GRAPH.md) — build order and the critical path.
9. [`plan/stages/`](plan/stages/) — one document per stage.
10. [`plan/features/`](plan/features/) — one document per feature.

## Non-negotiables

- **No swallowed exception, ever.** Every `catch` reports through
  `IFailureReporter.ReportHandled(exception, operation, consequence, severity)` or rethrows.
  Enforced by analyzers `BEEP0003`/`BEEP0004` and by guard tests.
- **One log, audit and exception service.** All three arrive at a single pipeline; nothing writes to
  the store, the rolling log or a toast directly.
- **Every user-visible message is a toast from that pipeline**, carrying a failure reference and
  never exception text. Feature code never injects the toast service — only the notification hub may.
- **Every writing command writes exactly one audit row**, and every refusal is audited too.

## Scope

- **Data and business logic** come from the existing `Beep.OilandGas.*` projects. Nothing is re-implemented.
- **Wellbore, log, schematic, mapping and well-engineering capabilities** come from the
  `TheTechIdea.OilGas.*` packages published from `OilandGasTools`.
- **The desktop adds only** the host, the shell, the MVVM layer, the WPF view layer and the
  SkiaSharp hosting adapter.
- **Business data is reached through `Beep.OilandGas.Client` (`IBeepOilandGasApp`)**, never by
  opening a database connection from a view or view model. This keeps the AGENTS.md API-first
  rule intact and lets the same desktop run offline (local), against a company database, or
  against `Beep.OilandGas.ApiService`.

## Status

Stage 0 has not started. See the master tracker for the current gate.
