# Implementation 199 - Original accountant correction review

Date: 2026-10-07. Status: canonical correction inputs/review and the existing AP API/typed client now support an addressed, fresh review of explicit accountant edits. The correction editor, native versioned write/history and correction task completion remain open; no persona phase is closed.

## Canonical inputs and immutable recorded history

TheTechIdea.Data owns APInvoiceCorrectionLine, APInvoiceCorrectionDraft and APInvoiceCorrectionReview. Proposed fields are quantity, unit price, line total, description and declared invoice total, with an explicit reason. Inputs address the displayed invoice/financial revisions and exact recorded controller decision intent/JSON revision. They contain the complete existing line identities; adding, deleting or substituting a line is refused.

Explicit line arithmetic and declared header total must agree. Decimal overflow, incomplete values, duplicate/foreign line identities, bounded/control-invalid text and a no-op correction refuse. Currency precision or rounding is not silently inferred. Currency/unit/reference changes, tax/non-PO and payment/balance/ledger semantics require their own later source contracts and gates; this review does not claim those concerns are resolved.

Only the original recorded proposal preparer can review correction, and only for the verified independent controller REQUEST_CORRECTION disposition. Acceptance cannot be used as a correction request. Exact current source, complete receipt/allocation/global consumption and policy evidence, active DRAFT status and original production binding are required. Current source and proposed values are separately copied: the proposal is not presented as an updated recorded invoice.

The review digest includes the full controller proof and exact proposed values. A structurally valid changed controller reason cannot reuse the prior displayed decision revision, even though the existing invoice/financial digest omits decision JSON. Consumer mutation, a tampered review digest or substituted request cannot become valid reviewed evidence.

## Existing API and client path

POST api/accounting/traditional/ap/{id}/correction-review extends the existing AP controller. It captures the original selected company/account/binding, checks current standard roles, read grants and Accounting.CorrectInvoice, and freezes the full role/permission sets. Complete native financial evidence is read twice and two correction reviews must have equal revisions before final access confirmation. Changed source, decision, contributor, binding or grant refuses publication.

The existing AccountingServiceClient freezes the input and company snapshot, escapes the exact route identity and validates the returned canonical review against the original company/account/invoice/draft. No database path or client entity/store is added. Accounting.CorrectInvoice is a permission identifier; no live role/grant is seeded.

Review is read-only: it records no correction, modifies no source/financial values and creates or completes no workflow task. Implementation 198's API/native correction-completion refusal remains active until the versioned correction command/history is implemented.

## Focused verification

Final affected gate passes 1428 checks with zero failures/skips in 2.3109 minutes, including 26 new correction cases. The two gates overlap. All command handles reached terminal exit and whitespace verification passes. These focused gates do not prove a full solution or live-provider pass. New cases exercise canonical exact values, source/request/binding/actor membership, no-op/duplicate/missing/foreign line refusal, arithmetic and text validation, copied snapshots and tampered reviews. Actual API/typed-client cases cover successful original accountant review, changed source/decision/global allocation evidence, absent/revoked grants, role/binding changes and another preparer. A decision changed during capture must also refuse. Source writes stay at the fixture's two preceding proposal/controller writes.

Actual canonical Data builds with zero warnings/errors in 1.80 seconds. The overlapping source gate passes 519 checks in 8.7205 seconds. The first affected runtime passed 1426 of 1427; its only failure assumed a collection expression was an array in the isolation fixture, now corrected to an explicit mutable input array. The initial partial controller compile required the logging extension import. Composition has one additional existing-style PermissionConstants source/cache conflict, for 367 warnings; this is separate from the clean actual Data build.

Native provider/transactions and current production read authority remain explicit test seams. Browser/OIDC/accessibility, live Beep provider/schema, editor behavior and durable correction/history are unverified. No SQL, new business EF lane, live migration/grant or phase closure was introduced.

## Next implementation target

Wire the existing invoice workbench/scoped owner to explicit correction editing and fresh review, then implement the native versioned correction command with bounded history, original request/proposal preservation, current source/contributor/authority gates, durable exact read-back and retained original uncertain intent. Require independent rereview and confirmed correction data before correction task completion. Workflow definition provisioning, cancellation/lifetime recovery, policy tasks, financial tax/non-PO/book/period/journal evidence, stock/returns, other personas/dashboards and multinational acceptance remain active.
