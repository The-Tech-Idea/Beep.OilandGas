# Implementation 208 — Return newer correction to the original preparer

## Complete correction return workflow

The existing reviewed company step command supports the explicit RETURN_CORRECTION_WORK action at a pending independent acknowledgement step when a newer corrected invoice version supersedes its completed work evidence. The current review stays ineligible for completion/acknowledgement and carries independently captured return evidence. The native owner verifies the earlier completed preparer work and requires the current correction version to be strictly newer. A return cannot be submitted as ordinary completion.

Return closes the current acknowledgement assignment, records CORRECTION_RETURNED audit, blocks that acknowledgement step and reopens the immediately preceding configured work step for the original preparer. It creates no acknowledgement approval and changes no AP values or invoice financial status. After the preparer confirms current source work, the normal configured handoff opens independent acknowledgement for that exact work/version. A second newer version can take the same declared return path.

## One existing command, store and router

The existing CompanyAccountingStepCommand carries a nullable explicit action; ordinary commands preserve their serialization. Current source/history/financial revisions, production/process binding, standard roles, review grant and Workflow.Approve gate return. Source maker and preparer cannot act as the independent reviewer. API and native owner independently verify the action and exact preceding work intent. No new endpoint, store, invoice column, SQL or business EF path is introduced.

CompanyAccountingCorrectionCycle is a shared Data contract stored in the existing process payload. It binds the company/process, original preparer, work/acknowledgement step identities, return intent and correction version/intent. The existing router derives new task identities/context from that declared cycle and restores the work deadline from the fresh start. Prior completed task identities, original audit and deadlines remain intact. Routing/read-back/completion checks count only the addressed cycle and reject pending assignments from other cycles. Cycles and their GUIDs are strictly validated; malformed, duplicate or noncanonical context refuses routing.

The existing retained native step proof preserves preceding proof when returning or progressing within a correction cycle. Return and next task, process/step updates, history and retained proof record together in the owned Serializable Beep lifecycle transaction. Exact retry confirms the original proof/cycle and adds no writes. Routing failure rolls back the new cycle, reset steps, task closure and history. Current PRODUCTION evidence remains outside that transaction; post-commit contributor drift cannot publish confirmation. Restoring exact original evidence permits reconciliation with the original intent and no additional writes.

## Screens and landing pages

The existing process detail displays the newer correction and offers Return newer correction to original preparer with a required reason. It uses the existing scoped step command owner/client/API and confirms a distinct recorded-return result through authoritative process reload. Unknown outcomes retain the original action/intent for reconciliation. Return wording does not claim acknowledgement or approval. Shared landing/inbox panels identify the need to return newer work and retain version/intent drill-downs.

Historical acknowledgement guards remain unchanged: the maker/preparer cannot acknowledge, current work intent/version must match, and source workflow acknowledgement cannot approve financial matching, posting or payment.

## Focused verification

Actual Data builds with zero warnings/errors in 1.28 seconds. The final related gate passes **106 checks in 2.6176 minutes**, covering return, acknowledgement, native company step commands, task routing and prior variance workflow/screens. It includes sixteen new cases: six canonical task-context cases and ten actual native/Kestrel/typed-client return cases. The valid case invokes the actual complete process screen's review and return handlers through its scoped owner and verifies the retained return receipt/intent and subsequent resumed work/acknowledgement. The second-cycle case confirms another return and final acknowledgement. The final overlapping source gate passes **519 checks in 9.4986 seconds**. Composition reports 382 source/cache/package warnings. All launched command handles reached terminal exit; whitespace verification passes.

The initial affected checkpoint passed 1608 checks in 6.5832 minutes before the new return cases. The expanded 1617-case checkpoint had 1612 passes and five failures in 6.4653 minutes: router closing-count validation still counted historical tasks. After that correction, the initial targeted run had 101 of 106 passes in 1.9265 minutes and exposed a read-back mismatch from clearing notes/outcome to null while the existing reader projects them as empty strings. The reopened state now uses that existing representation; the final 106-case run passes. The broader failed checkpoints are not reported as entirely green.

Unknown commit and post-commit drift recover with the original intent and no extra writes. Missing grants/binding, changed source and wrong action cannot mutate the lifecycle graph. Missing destination mapping after partial return work rolls back, leaving the original acknowledgement pending and no cycle/return history. Prior completed work tasks are checked byte-for-byte after return. AP status remains DRAFT; only prior source-correction writes exist.

Native provider/physical transactions and authoritative access capture remain fixture seams. Live provider/schema/role/definition provisioning, browser/OIDC/keyboard/accessibility, circuit lifetime recovery and broad solution acceptance remain open. No live migration/grant or phase closure is claimed.

## Next implementation target

Continue procurement/inventory stock and return workflows for purchasing, receiver and warehouse personas using existing source owners, current company roles, recorded receipt provenance and task/landing drill-downs. Keep financial approval separate until tax/non-PO/book/period/journal evidence is complete. Wider persona dashboards, multinational acceptance, live provider/provisioning and lifetime recovery remain active roadmap work.
