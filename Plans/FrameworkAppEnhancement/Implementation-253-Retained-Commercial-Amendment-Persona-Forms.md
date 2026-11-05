# Implementation 253 — Retained commercial amendment persona forms

## Original-preparer process workspace

The company process page now exposes CommercialAmendmentWorkbench after the original rejection review is completed. The selected account must be the original terms preparer and retain Accounting.View, ProductionAccounting.Adjust and a current standard company role. The component independently checks authentication, selected company/account, the exact reviewed process source and preparer before each operation. ApiService continues to enforce current configured roles, grants, FIELD and closed-process evidence independently.

The MudBlazor form shows the rejected version, independent reviewer/reason and fresh FIELD/transaction/source revision and gross revenue. New version, stored-calendar effective dates, currency, price, explicit unit/basis, method, notes and amendment reason are entered explicitly. Required form validation and canonical terms/command validation run before submission. No original values silently become a new proposal. Native prices, approval, invoice and recognition remain separate.

## Retained proposal and exact read-back

CommercialAmendmentWorkspaceState is circuit-scoped and registered alongside the existing terms, company step and handoff states. It retains one complete CompanyCommercialAmendmentSubmission, including reviewed predecessor/source, process revision, proposed terms, reason and UUID. Company/account/process/owner changes clear displayed evidence and fence late replies without deleting an uncertain intent. Getters copy evidence; disposing an old panel cannot detach its replacement.

Acknowledgement alone does not confirm an amendment. The state reloads the original owned process source, checks the same process revision and rejected predecessor, then finds the exact proposed preparation in validated current native history. Its predecessor proof/record, reason and closed-process origin must match the retained submission. Only that read-back clears pending state. If no amendment exists yet, a fresh amendment review must still equal the original frozen review before retrying the same UUID. Changed source/origin or another version refuses; a replacement command remains blocked.

Confirmed evidence is revalidated after remount/context return. A later visit without a circuit-held command can display a committed amendment whose preserved native origin identifies this exact original process revision and rejected record. That read-only discovery does not reconstruct an uncertain unsaved command. History entries retain version, preparer, decision and amendment reason; process-linked versions provide a predecessor rejection-review link.

## Independent handoff and history refresh

Only a confirmed undecided amendment exposes the existing CompanyAccountingHandoffPanel, subject to Workflow.Start. That panel uses the new preparation digest and its own retained configured-workflow intent. A successful amendment or manual reconciliation refreshes the parent process/native history; automatic remount loads avoid refresh loops. Old rejected terms remain read-only, while the new proposal's independent decision uses its distinct process.

The history panel and form use MudBlazor and existing theming. No custom CSS/JavaScript, direct client database access, duplicate business contracts or new schema/migration lane was introduced. Global FIELD selection remains unchanged throughout this assigned-source route.

## Original-preparer inbox admission fix

The actual preparer inbox test exposed an overbroad WorkflowInboxReview rule: it refused every eligible Workflow.Execute accounting task, including the configured commercial rejection-review step. The canonical validator now permits that combination only for SALES_CONTRACT evidence naming the current actor as its source maker. Foreign makers and AP invoice substitutions still refuse. Existing independent-review checks and API step authority remain intact.

## Focused verification

Two rendered journeys use the existing app RBAC/profile fixture and actual native task router to load the commercial preparer's rejection-review inbox, render its assigned-work route, close rejection review through the page and verify the task disappears. They then render fresh amendment inputs, set explicit terms, run actual form/domain validation and submit through local HTTP. One succeeds directly; the other loses the commit response, retains the same proposal, blocks replacement and confirms native history on reload without another write. Both render and submit the configured new handoff, then verify the new process's separate independent decision/completion through the typed API. The global FIELD is never changed.

Seven state cases cover failures before/after write, late company switch, failed read-back, changed fresh source, altered native origin and replacement-owner disposal. They assert exact UUID preservation, copy isolation, blocked replacement, context fencing, source/origin refusal and successful read-back reconciliation. The existing commercial, company start/step and persona inbox regression remains included. These are source/Razor/local HTTP/native fixture tests, not browser/OIDC or real provider acceptance.

Canonical Data builds with zero warnings/errors. Final focused source/Razor verification passes 355 tests with zero failures/skips in 53 seconds. Local temporary logs: oilgas-253-data.log and oilgas-253-final-source.log. Reproduce with Scripts/Test-PurchaseOrderOwnership.ps1 using the actual canonical Data DLL, IncludeWorkflowInbox and FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox. The focused composition now includes the amendment state and Razor component. Dependency/source-composition warnings remain scoped limitations; full solution verification is not claimed.

## Remaining delivery

Recover/discover an already-created amendment handoff from native lifecycle evidence after navigation or a new circuit, and verify the amended-version reviewer's own inbox and rendered decision/completion. Continue durable uncertain-command recovery across circuit loss and historical advancement. Extend approved delivery/effective terms to invoice/recognition and ledger read-back, royalty/partner queues and the wider thirteen-persona/multinational gates.

No CA/FG phase or the overall implementation goal closes. No handwritten SQL, new business EF migration lane, live business write, role grant, commit/push or deployment was performed. Beep Migration remains the schema owner. Browser/keyboard/accessibility/RTL, provider/isolation/concurrency, performance and multinational acceptance remain open.
