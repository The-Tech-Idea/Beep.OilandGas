# Implementation 252 — Process-bound commercial amendment API

## Closed rejection review as the amendment boundary

The existing company process controller now exposes GET /api/accounting/processes/{id}/commercial-amendment-review and POST /api/accounting/processes/{id}/commercial-amendment. The API requires a completed SALES_CONTRACT process with REJECTION_REVIEWED, explicit company ownership, the original deterministic start identity, unchanged lifecycle binding and configured definition, and the original preparer as its completed review actor.

Both configured steps must retain their exact identities, names, approval requirements, roles and completed rejection outcomes. Stored UTC dates, independent rejection reviewer, original-preparer completion, matching history actors/times/notes and the exact retained preparer completion command are verified. The command must still identify the same company, definition, contract, original preparation and full rejected native record revision. Active, approved, altered or foreign workflows refuse.

Current authenticated standard roles must include the configured preparer role or the existing Administrator override, while the account must still be the original preparer. Accounting.View and ProductionAccounting.Adjust remain mandatory. PRODUCTION independently requires current FIELD WRITE, its fresh source reader requires FIELD READ, and current company/actor/roles/grants/bindings are rechecked. A role or admin override never substitutes another account for the original maker.

## Canonical review and retained command

CompanyCommercialAmendmentReview in TheTechIdea.Data freezes company/actor/process revision/lifecycle binding, the exact rejected native contract/record and a fresh normalized source. CompanyCommercialAmendmentSubmission must match that review's predecessor, native proof revision, source, account and process origin exactly. New version, effective terms, currency, price, units/method, notes and amendment reason remain explicit; no new defaults are inferred.

The typed process client validates received reviews against its original selected company/account/process, copies them and refuses changed/late/null evidence. Submission freezes the complete review/intent, fences company/account before and after HTTP and requires an exact returned preparation. Retaining that original submission is necessary for uncertain-outcome retries; obtaining a new review after the amendment is already recorded is deliberately not a replacement-intent recovery path. Circuit-retained persona forms and durable recovery are the next delivery.

SalesContractTermsAmendment and its native origin add optional process identity/revision metadata. The actual native proof records that closed-process origin beside predecessor and reason, and carries it through subsequent decisions and preserved versions. Optional null metadata is omitted to preserve existing native serialization. Exact retry checks include the original process link and reason. Native history remains flat, bounded and validated; no schema column or business EF migration was added.

## Fresh amendment data and captured process display

An amendment review captures fresh revenue against the predecessor's exact FIELD/transaction/window and restores only its original contract business facts. Changed revenue since the rejection is permitted as explicitly reviewed new-proposal data; changes after that review refuse at the native source fence. The closed original process now uses captured rejected evidence even before a successor version exists, so the preparer can open that process when live revenue has changed. Its historical view retains old rows and terms. Active/current decision work continues to require live matching source evidence.

After the amendment, the original process remains read-only and bound to its rejected version. The new proposal is undecided and must receive a separate configured workflow and independent native decision. No rejection-review task is reopened and no financial approval, price activation, invoice or recognition is implied by amendment submission.

## Beep transaction and sequential verification

Full closed-process evidence is read before and after the native PRODUCTION command. Inside its owned Serializable transaction, guards capture current RBAC and production/lifecycle bindings only; they do not open LIFECYCLE repositories. Production source/native/row-policy checks, audit stamping, rollback and committed read-back remain in the existing command owner.

The first HTTP test caught an attempted cross-source read inside the owned transaction; that path was removed. The API does not claim atomicity across lifecycle and production. If closed-process evidence changes after native commit, submission refuses confirmation and the original process-linked intent remains the reconciliation evidence. This optimistic cross-module gap requires provider/concurrency acceptance and is not hidden by a success response.

## Focused verification

Actual native/local HTTP/typed-client journeys now use the public amendment review and submission endpoints. They complete rejection routing and original-preparer review, reject other preparers, obtain fresh changed revenue, submit/retry one process-linked proposal, retain original historical evidence, start a distinct new workflow, record an independent new-version approval and complete it. Existing rendered historical and commercial approval pages remain in the focused regression.

New command cases cover uncertain commit followed by exact retry, in-transaction authority-loss rollback, altered process revision, wrong predecessor, changed retry reason, changed definition, lost role/grant, source drift and altered rejection-review outcome. Active-process amendment review refuses. Seven typed-review cases reject foreign company/actor/process, altered source/predecessor, late company switch and null evidence. Prior current-role/FIELD/company/binding/native historical denials, native version-history and unrelated company start/step/inbox tests remain included.

Canonical Data builds with zero warnings/errors. Final focused source/Razor verification passes 346 tests with zero failures/skips in 47 seconds. Local temporary logs: oilgas-252-data-final.log and oilgas-252-closed-capture.log. Reproduce with Scripts/Test-PurchaseOrderOwnership.ps1, the actual canonical Data DLL, IncludeWorkflowInbox and FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox. Native writes occur only in fixtures. Focused source/dependency composition warnings remain; full solution/provider/browser/OIDC acceptance is not claimed.

## Next persona delivery

Add retained amendment state/forms and version history navigation to the original-preparer company process workspace. Preserve one complete reviewed submission across responses, context changes and reconciliation, and require confirmed native/process read-back before creating a new independent handoff. Verify the preparer persona inbox -> rejection review -> rendered amendment form -> recorded version -> independent reviewer journey.

Continue durable circuit-loss/historical-intent recovery, approved delivery/effective terms to invoice/recognition and ledger read-back, royalty/partner queues and the wider thirteen-persona/multinational plan. No CA/FG phase or overall goal closes. No handwritten SQL, new business EF migration lane, live data write, permission assignment, commit/push or deployment was performed. Schema remains on Beep Migration.
