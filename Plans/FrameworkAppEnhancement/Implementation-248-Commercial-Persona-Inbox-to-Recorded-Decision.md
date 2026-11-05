# Implementation 248 — Commercial persona inbox to recorded decision and completion

## Delivered persona journey

The configured commercial persona can open an actual SALES_CONTRACT approval task from assigned work, review its recorded terms inside the existing company process screen, record an independent approval and complete the matching task. Reloading assigned work removes the completed task. This extends the existing source, process, inbox and workbench owners; no parallel commercial workflow store was introduced.

PersonaAssignedWorkPanel now supports sales-contract source counts and drill-down, commercial approval-needed, recorded-reviewer and approval-access filters, and explanations specific to commercial evidence. The canonical inbox validator accepts commercial step evidence while retaining its exact task/process/role/independence checks. Invoice variance explanations and regression continue to work under the shared accounting/commercial caption.

## API and source contract

GET api/accounting/processes/{id}/commercial-source returns the canonical CompanyCommercialProcessSourceReview. It includes selected company/current actor, recorded process/contract identity, original preparation digest, process digest and existing SalesContractTermsReadReview. The source FIELD, date window and revenue transaction are derived from the native stored preparation, never from guessed dates or a global selector.

CompanyAccountingProcessReads uses its existing company process/source admission and CompanyFieldRevenueReads owner. It checks the process before and after source resolution, repeats complete terms evidence and compares closing company/account/module binding and a frozen sorted role/grant set. The native source still requires exact active seller ownership, proof, current company Accounting.View and FIELD READ. Missing, foreign, changed or unauthorized sources refuse. The typed client validates original selected company/account, requested process identity, contract/preparation relation and canonical terms proof, rejects late context responses and returns copied evidence.

The company process page verifies the source contract and process digest against its displayed process before mounting CommercialTermsWorkbench. The assigned workbench checks the process source before each load or action, consumes its exact FIELD/window/transaction and suppresses a duplicate handoff panel. It does not change the global FIELD selection. Ordinary revenue-row workbench use continues to require its matching selected FIELD. Native decision commands independently enforce current FIELD WRITE, commercial approval grant and maker separation through the existing API.

After a confirmed native decision, the parent clears obsolete step eligibility and reloads the recorded process/source; the user reviews current step authority before task completion. Retained native and workflow command intents remain in their existing circuit owners. CommercialTermsWorkspaceState now tracks its presentation owner so disposal of an older panel cannot detach a newer panel's source during a render replacement. Disposal still clears that owner's visible evidence and preserves uncertain commands.

## Verification evidence

Canonical Data builds with zero warnings/errors. The current focused actual-source/Razor composition passes 306 tests with zero failures/skips. The filter includes FieldRevenueSourceTests, CompanyProcessStart, CompanyStep and PersonaInbox tests, including existing rendered invoice variance inbox cases. Source/dependency composition warnings remain; a clean full-host/solution result is not claimed.

Thirteen new cases cover four real API source denials (foreign selected company, FIELD access, missing company read grant and absent standard role), seven typed-client malformed/foreign/late/null source responses, presentation-owner replacement and the complete commercial persona screen journey. The complete journey uses actual SQLite app RBAC, current native Beep process/task and PRODUCTION reader/writer fixtures, authenticated local Kestrel controllers, typed clients and HtmlRenderer. The explicit persona-context presentation seam reflects the actual COMMERCIAL configuration used by the RBAC and task readers; this is not a live OIDC/browser test.

The journey prepares under one account, starts the configured approval process, switches to a distinct authorized reviewer, reads the actual pending inbox task, renders its sales-contract/approval-needed filters and process link, opens the process screen and verifies exact source facts. Its real workbench decision handler records native approval, clears stale workflow review, then the page's review/completion handlers record task/process completion. Actual inbox reload is empty. Exactly two native contract writes remain (preparation and decision); the test proves no global FIELD selection write. The shared retained-state and invoice/purchase-order process regressions stay green within this gate.

Reproduce using Scripts/Test-PurchaseOrderOwnership.ps1 with the actual canonical Data DLL, IncludeWorkflowInbox and filter FullyQualifiedName~FieldRevenueSourceTests|FullyQualifiedName~CompanyProcessStart|FullyQualifiedName~CompanyStep|FullyQualifiedName~PersonaInbox. Updated HtmlRenderer checks wait for the newly mounted child to finish its actual asynchronous source load before asserting outcome content. Native writes occur only in fixtures.

## Remaining plan work

Implement rejection disposition and return/correction routing rather than leaving rejected approval work pending. Continue commercial amendment/version history, durable intent recovery across circuit loss and historical uncertain-start reconciliation, then approved delivery/effective-terms to invoice/recognition and ledger read-back. Broader royalty/partner queues and the thirteen-persona and multinational roadmap remain in scope.

The module reads and command boundaries are sequential PRODUCTION/LIFECYCLE checks, not atomic cross-module transactions. Live Beep schema provisioning, provider/isolation/concurrency, OIDC, physical browser interaction, keyboard/RTL/accessibility, performance and multinational acceptance remain open. The three broader purchase-order handoff rendering fixture failures documented in Implementation 247 are not resolved by this gate. No CA/FG phase or overall implementation goal closes.

No handwritten SQL, new business EF lane, live schema/business operation, role/permission provisioning, commit/push or deployment was executed. Business schema ownership remains with existing Beep Migration.
