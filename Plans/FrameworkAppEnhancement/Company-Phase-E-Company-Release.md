# Company phase E: company integration and release

Current executive continuity checkpoint: [108 - account/role-bound executive source journey](Implementation-108-Executive-Company-Portfolio-Evidence.md) validates the typed company/account/FIELD envelope, guards actual portfolio/inbox rendering and clears stale auth/source generations. The protected API now compares company/account/authority values across normal context refreshes, rechecks owned PPDM_CORE/source and current FIELD access, and returns no-store portfolio/assets. Native initial gate passes 13 (20.3411 seconds), final actual typed-client/protected HTTP gate 12 (9.0825 seconds); current app RBAC/row policy/native repository are real, provider/schema/auth/target-admission seams explicit. Prior rendered executive 50 and shared landing/inbox 137 pass, counts overlap. All executive jobs are terminal; mixed-return work remains separate and unverified. No new model/role/endpoint/schema/SQL/domain EF/business write/KPI or theme. Full application/provider/browser/OIDC, approved management KPI/decision/outcome/consolidation and broader goal acceptance remain open. Commit/push follows full goal/plan completion.

Current implementation: [77 — company persona state and landing](Implementation-77-Company-Persona-State-and-Landing.md). Six state checks pass; current Razor wiring is source-only. Role eligibility and the persona journeys remain open. Installed substitute UI/auth/form guidance is approved.

Deliver this phase through [company persona data workflows](Company-Persona-Data-Workflows.md): authorized company/role landing, source-backed work queues and dashboards, business actions, cross-role handoff and authoritative reload. Backend gates support the promoted journey; they do not substitute for its screens.

Mandatory extension: CA-45/46 in [multinational requirements](Company-Multinational-Requirements.md). Release requires regional deployment/residency evidence and a pilot covering independent tenants plus multinational legal entities, locales, currencies and timezones.

Depends on applicable A-D journey gates and FG-51/60..62. Responsible areas: shared UX, reporting, integrations, platform administration and delivery.

| ID | Deliverable | Acceptance |
|---|---|---|
| CA-40 | Unified role workspace, search, task inbox, notifications and cross-module asset history. | Every promoted role completes daily tasks without table-name knowledge; persona switching grants no rights; inaccessible search results do not leak data; history links measurements, studies, orders and journals. |
| CA-41 | Executive dashboards and governed reports. | Production/cost/cash/risk metrics declare period, unit/currency, source and freshness; totals reconcile with source reports; missing data cannot appear as zero; exports preserve access restrictions. |
| CA-42 | Company integration catalog: ERP/payroll/bank, telemetry, documents and regulatory exchange as established by CA-01. | Each integration declares authoritative owner, mapping, credentials, replay/idempotency, reconciliation and failure queue; no parallel master-data authority. Deferred requirements have explicit rationale and release scope. |
| CA-43 | BeepDiA governance, branding, accessibility, localization and operational readiness. | Master-data administration API-first; roles independently enforced; Mud palette and central CSS/JS rules pass; keyboard, dark/RTL and failure states inspected; backup/restore and operator runbook exercised. |
| CA-44 | End-to-end company pilot and release evidence. | Accountant, engineer, operator, HSE, manager and admin journeys pass with persisted data; cross-role handoffs, rejection/correction/recovery, provider concurrency and app isolation proven; release evidence records SHA/environment and remaining scope. |

Release requires clean reproducible CI, supported-provider migration/restore, diagnostics configuration, OIDC browser journeys, transaction/recovery checks and domain-reviewed calculation/financial samples. Measure performance against an agreed representative dataset and concurrency target established during baseline; do not invent capacity figures. Each promoted capability must have UI, API, persistence, authorization, audit and recovery evidence. A source inventory or passing unit suite alone cannot satisfy this gate.

Current persona implementation: [Implementation 78](Implementation-78-Company-Role-Workspace-Review.md). 17 state and 14 SQLite store tests pass. API/host/rendered screens and full persona journeys remain open; next is mapping administration and actual landing/source workflow.

Latest persona slice: [Implementation 79](Implementation-79-Reviewed-Role-Workspace-Administration.md): existing role screen gains reviewed workspace assignments/readback; 20 SQLite/store tests pass, including canonical company-role resolution. Actual HTTP/host/rendered UI and accountant daily workflow remain open.
