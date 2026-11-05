# Implementation 202 — Native accountant correction recording

## Existing native AP owner

APInvoiceService now records the shared versioned correction command through the existing PPDM repository and owned Beep Serializable PRODUCTION transaction. It freezes the command and displayed review before awaiting current submission authority. Connection name, cached datasource identity, physical transaction ownership and isolation remain guarded throughout reads and mutations.

Fresh Beep metadata must confirm the bounded nullable history column and typed header/line amount, description and audit fields. Missing, duplicate, wrong-type, undersized or non-nullable history metadata refuses before a correction write. No SQL or new business EF path is introduced; no live migration runs.

The native owner reads the complete financial evidence and globally identified header/lines twice before mutation. The financial capture must match those raw rows exactly. Foreign/duplicate invoice or line identities, changed source, purchase, allocations, global contributors, receiver proof or policy evidence refuse. The caller supplies the server-owned fresh financial capture and current authority callback; independent HTTP role/grant enforcement is the next slice, not proved by a native callback fixture.

## Narrow owned writes and history

Correction writes only the declared invoice total, every existing billed line's explicit values/description, accountant change audit and canonical correction history. Header and lines use one frozen UTC correction timestamp. The narrow existing row-policy boundary validates the exact original row and immutable intended serialized row after standard common-column preparation. It cannot insert/delete, grant reads, change unrelated fields or access other tables.

All line updates and the header/history update participate in the same owned transaction. Partial provider failures, altered read-back, lost authority/binding and contributor drift after line writes roll back the whole correction. Original proposal/controller evidence, creator, ownership, currency, units, purchase references, status and payment/balance fields remain recorded. This is source correction, not financial matching, tax/ledger reconciliation, posting or payment.

## Exact retry and durable confirmation

APInvoiceFinancialReview.OriginalCorrectionEvidence reconstructs the verified preceding header and complete line set for the latest exact recorded correction command. It reprojects only that invoice in purchase/allocation/receipt-consumption evidence. All other current purchase, allocation, contributor, receiver and policy facts remain current. Rebuilding the original correction review must reproduce the submitted revision before retry or post-write confirmation can succeed.

Exact retry adds no update or history version. Another intent, changed reason/history or changed current contributor cannot overwrite or repair the recorded correction. A later explicitly reviewed version appends to the bounded existing history and preserves its predecessor.

After owned read-back and commit, the owner again captures full current financial evidence and globally identified header/lines. The addressed result must verify the exact original submission and match the previously confirmed serialized result. Commit failure, missing durable header/lines or post-commit authority/contributor drift publishes no success. An already committed original intent can be reconciled after exact evidence and authority return, without additional writes. Scoped editor retention of that uncertain intent is still pending HTTP/client integration.

## Focused verification

The affected gate passes **1515 checks with zero failures/skips in 2.8047 minutes**, including the initial 42 new native cases. All **44 final native cases pass in 28.9596 seconds**, including two additional post-commit cases. The gates overlap and have different scopes; the final targeted run covers the added cases. All command handles reached terminal exit and whitespace verification passes. The overlapping source gate passes **519 checks in 7.6913 seconds**. Actual canonical Data builds with zero warnings/errors in 1.73 seconds. Focused composition has 371 warnings: the prior 367 plus four source/cache conflicts at the new references to existing repository/transaction owners.

Native cases cover exact recording/retry, successive reviewed versions, invalid submission before transaction, full fresh source/contributor drift, global identity collisions, Beep metadata, partial failures/rollback, borrowed or insufficient transactions, uncertain commit, changed retry intents/proofs, missing durable rows, initial revoked authority and post-commit authority/contributor changes. Fixtures use actual native AP/PPDM owners with explicit datasource/transaction/authority seams. The shared AP fixture now supplies its canonical AP_INVOICE_LINE primary-key metadata for the row policy.

No HTTP save endpoint, client submission, browser/OIDC/accessibility proof, live provider/schema/transaction or correction-task completion is claimed. No live grant/migration or phase closure is introduced.

## Next implementation target

Expose correction submission through the existing AP API with independent current standard roles, company action grants, original account and binding, and server-owned complete financial captures. Extend the existing typed client and scoped editor with frozen original intent, unknown-outcome recovery and fresh exact source/history confirmation. Require independent rereview and confirmed corrected-invoice evidence before correction task completion. Live definition provisioning, lifetime recovery, tax/non-PO/book/period/journal evidence, policy tasks, stock/returns, other personas/dashboards and multinational acceptance remain active.
