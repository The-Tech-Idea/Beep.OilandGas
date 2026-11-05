# Implementation 89: recorded journal preparer in action review

Updated 2026-10-06. Continues accountant source -> independent approval, following [Implementation 86](Implementation-86-Source-Action-Review-and-Screen.md). Supporting source/action integration; finance approval/posting and complete persona journey remain open.

## Workflow correction

A journal workflow may be started by someone other than the journal preparer. Process creator independence alone does not prevent that preparer approving their own journal. The existing action-review factory now takes the recorded journal from the existing accounting owner, checks exact JOURNAL_ENTRY identity, and excludes journal preparers from independent review/approval choices even when they have the Administrator role. Missing journal or unknown preparer evidence cannot offer approval. Existing role and process-creator checks still apply.

The action revision includes the full captured journal record alongside company, account, roles, source, definition and history. A changed preparer or status changes the reviewed revision. The existing API review captures the journal twice through its normal two-pass review, and transition POST repeats that review before mutation. This remains a preflight check, not atomic approval/posting certification.

## Active owners and API wiring

BusinessProcessController consumes the existing registered IJournalEntryService, whose existing JournalEntryService.GetEntryByIdAsync is now exposed through that interface. No second journal, repository, transport or authorization owner is introduced. Program already registers the implementation with a request PRODUCTION ModuleConnectionResolver callback; no caller-selected database is introduced here. Existing process/FIELD/entity READ checks precede journal capture.

The existing current-step gate also checks recorded journal preparer independence for journal approval/financial review steps before update/close/transition operations that use that gate. It rechecks source resource authority before reading the preparer; changed/refused sources report through the existing failure reporter and refuse the step. RequiresIndependentActor is shared by the gate and review factory, preserving the standard app-role pattern and authentication-only IdentityServer boundary.

The existing source screen obtains the revised eligible choices through its current typed API review path. Journal records remain in their existing legacy domain owner for this change; no duplicate shared DTO/entity or claim of complete domain ownership migration is introduced.

## Focused evidence

- Test-ProcessTransitionReviews.ps1: **14 passed**, zero failures/skips, terminal exit 0, 1.2951 seconds. Actual factory/role-check sources with actual Data/Models assemblies. New cases cover original journal preparer with Reviewer and Administrator roles, missing/unknown preparer, mismatched journal identity, and changed preparer/status revision. Previous eight factory cases now supply explicit recorded journal evidence. Log `%TEMP%/oilgas-journal-preparer-review-final.log`.
- Test-ProcessStepRoles.ps1: **23 passed**, zero failures/skips, terminal exit 0, 1.6161 seconds after extracting the shared independence rule. Actual role-check source and fixtures. Log `%TEMP%/oilgas-journal-step-roles.log`.
- Whitespace diff check passed. Existing controller test constructor sources were updated for the required existing journal service dependency; those native suites are unexecuted here.

## Remaining journey gates

Native controller/DI/typed HTTP and provider tests must prove that each selected company's journal read binds to its actual accounting database, rejects colliding IDs and revoked source access, denies preparer step/transition actions, and preserves recorded evidence under concurrent changes. The full journal hash does not prove transactional consistency. Direct accounting posting APIs/services and alternate process mutation paths still require their own authority, maker/checker, version and durable operation review; this change does not certify them.

Render invoice/journal data, company/book/period/currency and approval outcomes through the existing source screen; then verify actual post -> persisted ledger read-back -> financial KPI drill-down. No financial totals or posting success are inferred from action availability. Durable command receipt/reconciliation, provider atomicity, missing production dependencies and rendered browser verification remain open.

No SQL, schema change, business EF lane, role grants or phase/goal closure.
