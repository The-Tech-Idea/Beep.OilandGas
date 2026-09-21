# Phase 4: durable lifecycle and accounting journeys

Dependencies: FG-11, FG-20/22; FG-30 when consuming engineering results. Responsible areas: LifeCycle, UserManagement, ProductionOperations, ProductionAccounting, Accounting and domain modules. Reuse existing workflow/RBAC and accounting revision plans.

| Task | Deliverable | Acceptance |
|---|---|---|
| FG-40 | Verify and harden the existing approval/workflow engine for concurrency, retry, restart, delegation expiry and field-scoped tasks. | Repeated delivery cannot approve or execute a business transition twice; competing approvals resolve deterministically; restart preserves state/history; revoked or expired authority is rejected. |
| FG-41 | Prove production allocation -> accounting posting -> reconciliation -> period close/reopen through existing services. | Volumes and monetary totals reconcile under declared rounding rules; duplicate submission does not duplicate postings; closed periods reject ordinary changes; authorized reopen/reversal leaves an audit trail. |
| FG-42 | Connect remaining lifecycle modules through explicit handoff contracts and stage gates. | Required domain evidence, authorized actor and transition outcome are persisted; failed downstream work is visible/recoverable and does not leave a misleading completed stage. |

## Journey rollout

1. Production pilot: capture/validate facility measurements, approve production, allocate volumes, post/reconcile and close. Exercise correction and reversal as well as the successful path.
2. Asset pilot: prospect -> development approval -> drilling/construction -> handover to production. Reuse current field/asset identifiers and existing process definitions.
3. Governance pilot: HSE incident or permit action -> assigned task -> evidence -> approval -> audited closure. Attachments and task visibility honor asset/field access.
4. Retirement pilot: decommissioning proposal -> engineering/cost evidence -> authorization -> closure/restoration records. Model required permit and lease dependencies explicitly.

Keep financial accounting and production accounting responsibilities as defined in their existing plans. Do not conflate operational volume allocation with general-ledger posting or duplicate calculation logic. This review does not certify IFRS, GAAP, tax or jurisdictional compliance; individual standards and jurisdictions need separate domain validation during implementation.

## Failure behavior and checks

Inspect existing transaction/retry support before adding anything. For each multi-step mutation, declare its transaction boundary and recovery behavior. Use existing persistence and workflow history, adding idempotency/version checks there where missing. External side effects need a persisted delivery/retry record with duplicate protection if the current system lacks one; do not add an alternative workflow engine.

Extend `CrossModuleWorkflowTests`, production accounting process/period-close tests and relevant module tests. Add real persistence tests for two simultaneous approvals, timeout after commit, replayed submission, revoked delegation, unauthorized field, restart mid-process and partial downstream failure. Exit requires both a success and recovery demonstration for each promoted journey.
