# Implementation 170 — Company assigned step review

Date: 2026-10-06. Status: actual API/typed-client/current process-page eligibility review is focused-tested. Atomic step submission and authoritative outcome recovery remain open.

## Assigned recipient experience

The existing company accounting process detail page now offers an explicit current-step authority review alongside its recorded source/history. It shows the exact step, assigned role, source maker, workflow starter, required permission and independent-actor requirement. Eligible, missing workflow permission, role/independence refusal and unavailable/changed evidence have separate presentations. Company/source reload clears visible review; late company or changed displayed-process responses are discarded.

This review grants no authority and performs no action. The page explicitly states that atomic recorded step commands/recovery are still required before submission. Financial approval, matching, posting and payment are separate work. No submit control calls the existing unsafe generic completion path.

## Existing ownership and standard roles

TheTechIdea.Data owns CompanyAccountingStepReview. The complete existing company controller/service exposes GET {id}/step-review through the same typed BusinessProcessServiceClient. Exact escaped process identity, company and actor are validated; client company changes refuse publication. The page compares the review's full ProcessRevision with its displayed frozen process before showing eligibility.

The API uses the existing owned process/source reader, original current company/module binding, actual AP/PO source owner and shared consistent unmodified source-maker audit. Original recorded start intent must match company, source, workflow starter, module binding and definition revision. Current source revision/maker must still match that intent. No missing source audit, legacy company scope or alternate maker is inferred.

The current active process has exactly one matching pending/in-progress step and template with the same required role, step name and approval flag. Existing ProcessStepRoleChecks determines role and independent-actor eligibility. Its standard ASP.NET roles are restricted to the intersection of the authenticated principal and the company's current authoritative app-role set. Approval steps require the existing Workflow.Approve permission; other steps require Workflow.Execute. No grants are seeded or IdentityServer authorization added. Read access alone can return an explicit negative eligibility result.

Two complete captures agree on process, definition, source, eligibility and revision. Frozen original roles/permissions and company/actor/binding must remain unchanged through the final check. Changed/ambiguous evidence returns fixed conflict; authentication/current access loss refuses. Malformed stored start evidence is an unconfirmed conflict, rather than treating it as malformed caller input or exposing provider details.

## Why command submission remains open

Inspection of ProcessServiceBase.CompleteStepAsync confirms it calls SaveProcessInstanceAsync and AddHistoryEntryAsync separately. It does not provide the company command's required atomic process/history/task handoff, frozen intent and reconciled receipt. This slice implements the actual recipient review and screen without labeling that legacy path an atomic action. The next implementation must extend the existing native command owner, retain one intent, commit the current/next step and complete persona-task changes together, recheck authority and confirm recorded outcome before the page can submit.

## Focused evidence

Actual Data build: zero warnings/errors, 1.58 seconds. Log: %TEMP%/oilgas-step-review-170-data-final.log.

Final actual-source affected gate: Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox. **690 passed, zero failed/skipped, terminal exit 0, 10.8744 seconds**. Log: %TEMP%/oilgas-step-review-170-verified.log. Eighteen new cases cover actual native AP/PO handoff-to-typed review, exact displayed process digest, starter/maker/permission refusal, changed definition/source/audit/provider/step/company/role, authority revocation, malformed retained intent and four complete process-page handler/render cases. No additional native writes occur during review. Whitespace verification passes; 315 existing dependency/source-precedence and nullable warnings remain, with no warning from the new review/page/test sources.

The first expanded HTTP gate passed 684 and failed one expectation (685 total): foreign company ownership follows the existing fixed conflict boundary, not the test's assumed 403. A page-test compile then required qualification of System.Uri because NavigationManager.Uri shadows the type. The 689-case checkpoint passed 688 and failed one diagnostics-wording expectation: the actual API conflict sentence is propagated rather than the caller's fallback. Those expectations were corrected to existing owner behavior; the final gate includes the added malformed-intent case and supersedes the checkpoints.

Native handoffs in these cases are prepared by the actual writer under an explicit test authority callback, using actual API-reviewed source evidence and a distinct recorded starter. The HTTP reviewer, native process/task owner, source readers, controller/service, complete typed client and complete process page are actual sources. Page tests invoke the actual bound review handler and rerender through HtmlRenderer; they do not certify browser clicks. Current company/binding/account/diagnostics seams and physical native records/transactions remain explicit fixtures. Existing app RBAC/configuration SQLite checks run in this affected composition. Production host/DI/OIDC, native provider/concurrency/durability, browser/RTL/accessibility and whole-solution verification remain open.

## Remaining delivery

Atomic reviewed step commands, durable pending recovery/outbox, financial preparer/reviewer action evidence, receipt acceptance/concurrent consumption, approval/post/payment, full accounting dashboard composition, other personas and multinational/company requirements remain active. Beep MigrationManager remains the business schema owner. No handwritten SQL, new business EF lane, live migration/grant or phase closure.
