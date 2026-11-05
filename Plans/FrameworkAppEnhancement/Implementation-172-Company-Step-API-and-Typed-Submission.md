# Implementation 172 — Company step API and typed submission

Date: 2026-10-06. Status: existing company API/native/typed-client completion and exact retained-intent reconciliation pass focused verification. Process-page submission controls and circuit/durable pending outcome UX remain open.

## Company command authority

The existing company process controller/service now exposes POST step-complete, using Data-owned CompanyAccountingStepCommand and CompanyAccountingStepReceipt. The actual native owner exposes its retained latest command through ICompanyAccountingProcessCommands; the API does not deserialize private native persistence envelopes or establish success from their presence alone.

The submission is frozen before awaits. Fresh authoritative app-role/permission sets, authenticated standard role claims, original company/actor/module binding, source-specific read access and Workflow.Approve or Workflow.Execute are required. The current assigned role must be both authoritative for this company and present in the principal, or the company's authoritative Administrator assignment must apply. No role, grant or persona is automatically assigned. IdentityServer remains authentication-only.

Owned process reads and the actual source/audit owner confirm the original source identity/revision/maker and workflow starter. The original definition must remain active and match its exact reviewed revision, current recorded step/template identity and independent-actor policy. A new command must equal the full fresh API step review, including displayed process and review revisions; frontend assertions do not create eligibility.

## Retry and persistence boundaries

Only an exact native retained command with the same intent can use historical completed-step authority checks. It must still have the original actor/role/permission/source/maker/definition/binding, recorded completed step and reason. The API checks the original historical step through existing standard ProcessStepRoleChecks after exact persisted evidence is established. It never substitutes a stale historical step for a new command. The native command independently verifies complete process/approval/history/task outcome proof and refuses missing, altered or progressed evidence without writes or repair.

The required native callback rechecks fresh app RBAC/configuration/company/actor/binding/permission authority inside the owned LIFECYCLE transaction. No PRODUCTION source query crosses that transaction. After native commit, exact receipt identity, fresh original owned source/audit/definition, retained command and actor/reason/history evidence are checked again before success.

Authority loss during mutation rolls back the graph and task closure. Source drift after commit returns unconfirmed conflict even though the recorded command exists. Restoring unchanged source evidence permits reconciliation using the exact same intent without extra writes. This remains separate preflight/transaction/post-commit evidence, not an atomic PRODUCTION/LIFECYCLE snapshot. Financial invoice/order changes, posting and payment are not performed.

## Complete typed client

BusinessProcessServiceClient now submits one frozen command through the company endpoint. It requires the original selected company/account, validates the full receipt against the retained command and discards late company results. It does not retry or regenerate intent automatically. The native outcome remains available for exact reconciliation even if the current client cannot accept a receipt.

The existing process page still offers read-only current-step eligibility. Reason input, pending command retention, explicit submission/reconciliation controls, confirmed history/next-recipient links and authoritative page reload are the next visible work. No complete command screen is claimed.

## Focused verification

Actual Data build: zero warnings/errors, 1.69 seconds. Log: %TEMP%/oilgas-step-api-172-data.log.

Final actual-source affected gate: Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox. **730 passed, zero failed/skipped, terminal exit 0, 13.7413 seconds**. Log: %TEMP%/oilgas-step-api-172-verified.log. Eighteen new cases cover actual typed AP/PO completion/read-back/unchanged retry; authentication/role/permission refusal; changed asserted company/actor/binding/source/definition/review/process/maker/step; in-transaction revocation/full rollback; post-commit source drift and original-intent reconciliation; changed retry reason; missing retained task set without repair; and discarded post-commit client receipt after company selection changes.

Whitespace verification passes. Expanded composition retains 319 dependency/source-precedence and nullable warnings; no new API/client warning is reported. Counts overlap previous affected gates and are not added. The first 728-case expansion passed. The 730-case checkpoint passed 729 and failed one transport fixture: forwarding the same already-sent HttpRequestMessage to a second HttpClient was rejected before mutation. The forwarding boundary now clones request headers/content; the final case proves actual commit before client receipt rejection.

These cases compile/run actual Data contracts, company controller/service, AP/PO owners, native process/approval/history/task command and complete typed client. Current account/company/binding/diagnostics seams and physical native provider transactions/records remain explicit fixtures; existing app RBAC/configuration SQLite checks run in the affected composition. The post-response transport boundary explicitly changes client selection after the real HTTP response. Full production host/DI/OIDC, live provider concurrency/durability, browser/RTL/accessibility, command-page interaction and whole-solution verification remain open.

## Remaining persona delivery

Connect reviewed completion and retained outcomes to the existing company process page. Keep browser/new-circuit and historical command recovery/outbox, configured validation/conditional routing/delegation, financial preparer/reviewer actions, receipt acceptance/concurrent consumption, approval/post/payment, full accounting dashboard composition, other personas and multinational/company requirements active. Beep MigrationManager remains the business schema owner; no handwritten SQL, new business EF lane, live migration/grant or phase closure.
