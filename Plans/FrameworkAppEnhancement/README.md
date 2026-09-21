# Framework and application enhancement plan

Review date: 2026-09-21. Source baseline: `cd2e4511`. Scope: the framework, API, typed client, Blazor application, data setup, and delivery checks in this checkout.

The [root master tracker](../../MASTER-TODO-TRACKER.md) owns execution status. These documents describe work and acceptance criteria; they are not a second tracker. This is a source-based review with a bounded build probe, not a certification of numerical accuracy, production security, or regulatory compliance.

## Read in order

1. [Review and evidence](01-Review-and-Evidence.md): strengths, findings, limitations, and traceability.
2. [Architecture and existing-plan alignment](02-Architecture-and-Plan-Alignment.md): ownership decisions and reuse rules.
3. [Phase 0: reproducible baseline](Phase-0-Baseline.md).
4. [Phase 1: API and authorization boundaries](Phase-1-Boundaries-and-Identity.md).
5. [Phase 2: PPDM data and setup reliability](Phase-2-Data-and-Setup.md).
6. [Phase 3: engineering calculation quality](Phase-3-Engineering-Quality.md).
7. [Phase 4: lifecycle and accounting workflows](Phase-4-Lifecycle-and-Accounting.md).
8. [Phase 5: application experience](Phase-5-App-Experience.md).
9. [Phase 6: release evidence](Phase-6-Delivery-and-Verification.md).

## Delivery sequence

Start with FG-00 through FG-02 to establish reproducibility and ownership. Address FG-10 through FG-13 before adding more user-facing workflows. Notification isolation and subscription authorization deserve immediate implementation attention even while the wider baseline is being established.

Data work follows ownership decisions. Calculation and workflow work can then progress independently against the same API/data contracts. App journeys integrate those verified services. Release acceptance requires all applicable gates; a passing library build does not establish application readiness.

Each task should produce one reviewable change with its relevant source, tests, and tracker evidence. Do not bulk-mark a phase complete because interfaces or pages exist. Assign a named owner when scheduling; the phase documents identify responsible project areas rather than inventing staff assignments. Estimate effort after the baseline, avoiding unsupported calendar promises.

## First delivery slice

Take FG-12: make notification state user/circuit scoped; derive user subscriptions from the authenticated principal; authorize persona/process subscriptions with the existing app RBAC and field access services; use the configured authenticated API hub URL. Prove with two concurrent identities that neither can subscribe to or receive the other's restricted messages. This closes a specific observed gap and provides a model for later app-to-API verification.

## Implementation evidence

Latest: [field-bound production API](Implementation-23-Field-Bound-Production-API.md). Previous: [production dashboard](Implementation-22-Production-Dashboard.md). Previous: [production workspace consolidation](Implementation-21-Production-Workspace-Consolidation.md). Previous: [production engineer workspace](Implementation-20-Production-Engineer-Workspace.md). Previous: [website well comparison](Implementation-19-Website-Well-Comparison.md). Previous: [website lifecycle overview](Implementation-18-Website-Lifecycle-Overview.md). Previous: [website field selection](Implementation-17-Website-Field-Selection.md). Previous: [website field dashboard](Implementation-16-Website-Field-Dashboard.md). Previous: [persona workspace navigation](Implementation-15-Persona-Workspace-Navigation.md). Previous: [website royalty details workflow](Implementation-14-Website-Royalty-Details.md). Previous: [atomic financial operation claims](Implementation-13-Financial-Operation-Claims.md). Previous: [structured royalty journal references](Implementation-12-Royalty-Source-References.md). Previous: [royalty posting evidence and accrual linkage](Implementation-11-Royalty-Posting-Review.md). Previous: [persisted royalty payments](Implementation-10-Royalty-Payments.md). Previous: [stored royalty accrual and retry reservations](Implementation-09-Royalty-Accrual.md). Previous: [field royalty reporting and bound preview](Implementation-08-Royalty-Reporting.md). Previous: [remote master reconciliation](Implementation-07-Remote-Master-Reconciliation.md). Previous: [royalty preview and development policy](Implementation-06-Royalty-Preview-and-Development-Policy.md). Previous: [direct cost calculation](Implementation-05-Direct-Cost-Calculation.md). Previous: [run-ticket reconciliation](Implementation-04-Run-Ticket-Reconciliation.md). Previous: [persona routing and seeding](Implementation-03-Persona-Routing-and-Seeding.md). Earlier slices: [notification isolation](Implementation-01-Notifications.md), [revocation and build recovery](Implementation-02-Revocation-and-Build.md).
