# Implementation 201 — Versioned correction command and source history

## Shared correction contract

TheTechIdea.Data now owns APInvoiceCorrectionCommand, submission/result contracts and bounded linear APInvoiceCorrectionHistory. This is the shared evidence contract required by the upcoming native save path; no correction writer or save endpoint is implemented in this slice.

A command captures the original accountant/company/invoice, exact explicit draft, displayed review revision, production binding, version, predecessor history revision and original intent. It must match the canonical current review and recorded history count. The first version has no predecessor; later versions require the exact hash of the preceding recorded history. Intents are unique within the history and versions are contiguous. The bounded contract allows at most eight versions and 262144 serialized characters; exceeding either bound refuses rather than dropping old evidence.

The history preserves one complete original header and line set. Each version captures its full command, UTC change audit and a revision covering that command, audit and complete resulting header/lines. It reconstructs successive corrections without nesting prior history documents. The original proposal and controller request remain byte-for-byte preserved. Only the original proposal preparer under the same REQUEST_CORRECTION may create a version. Explicit totals/arithmetic and complete existing line identities remain required; a no-op cannot add a version.

## Recorded source continuity

Reconstruction applies only the declared invoice total, existing billed line values/descriptions and accountant change audit. Currency, units, purchase references, ownership, status, payment/balance fields and all other source metadata must remain unchanged. Audit timestamps must be UTC and cannot precede the previous recorded change. This contract does not derive payment/balance changes or establish tax, book/period, matching or posting approval.

The existing AP_INVOICE entity gains nullable CORRECTION_HISTORY_JSON with bounded Beep metadata. Null history is omitted from serialization, preserving prior proposal/controller proof bytes. The current AP source digest includes history when present while retaining the existing digest for uncorrected sources. Complete current-source reads verify the actual header and line set against reconstructed history before publishing. Altered history, version/predecessor, command reason, source fields or line membership refuses.

Existing proposal/controller proof readers recover their preserved original header through verified correction history. Their header-only proof path does not claim current line validation; complete AP source reads independently require actual line equality. ForCompanyWorkflow recovers the preserved original line set as well as the header, retaining the existing proposal handoff revision through proven corrections. Current raw source revisions still change. No parallel workflow, source store or compatibility fallback is added.

Result verification requires the exact submitted command as the final history entry, exact reconstructed current header/lines, and the preceding complete source matching the displayed review. Historical evidence is not presented as current financial approval.

## Focused verification

The affected gate passes **1472 checks with zero failures/skips in 2.9375 minutes**, including 30 new history/command cases. After the final result guard was tightened to require non-null lines, **all 31 history cases pass against the final Data assembly in 21.1647 seconds**. These gates overlap and their scopes differ: the later targeted run covers the final result-only change. The overlapping source gate passes **519 checks in 9.4256 seconds**. Actual canonical Data builds with zero warnings/errors in 2.29 seconds. Focused composition reports 367 existing source/cache/package warnings; all gate handles reached terminal exit and whitespace verification passes. Earlier checkpoints passed 1469 in 2.2888 minutes and 1470 in 2.5330 minutes before the added malformed-root/result checks.

Cases cover first and successive versions, exact predecessor hashes, eight-version exhaustion, duplicate intents, removed/reordered versions, altered totals/lines/currency/references/status/audit/reason/revisions, foreign company, unknown JSON members, oversize history, null/empty original lines, missing result lines, addressed result verification, copied snapshots and preserved workflow/proposal/controller evidence. The schema case inspects Beep ClassCreator's nullable string field and exact size. Successive-version fixtures seed corrected test rows for fresh financial reviews; they do not invoke a production correction writer. Native writes remain the preceding two proposal/controller writes.

Live Beep migration/provider/transaction, HTTP save, browser/OIDC/accessibility and durable correction recovery remain unverified. No SQL, new business EF lane, live migration/grant or phase closure is introduced.

## Next implementation target

Implement the existing native AP owner’s correction command with owned Serializable PRODUCTION transaction, current standard roles/action grants and binding, fresh full source/contributor evidence, narrow header/line writes, history recording and durable exact read-back. Add independent API enforcement and typed-client submission, retain the original uncertain intent in the scoped editor, and require independent rereview plus confirmed corrected-invoice evidence before correction task completion. Live workflow provisioning, lifetime recovery, tax/non-PO/book/period/journal evidence, policy tasks, stock/returns, other personas/dashboards and multinational acceptance remain active.
