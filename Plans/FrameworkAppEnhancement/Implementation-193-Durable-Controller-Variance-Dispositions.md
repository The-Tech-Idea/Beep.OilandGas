# Implementation 193 - Durable first controller variance disposition

Date: 2026-10-07. Status: canonical command/proof, native invoice recording, API/typed client and scoped read-only history display implemented. Controller screen submission/retained intent and configured persona task handoff remain incomplete. No persona phase is closed.

## Controller decision and persisted history

An independently authorized controller can record REQUEST_CORRECTION or an eligible ACCEPT_WITHIN_POLICY_VARIANCE through the existing AP invoice owner. The command binds the exact company/account/invoice, original accountant proposal intent, independently reviewed invoice/review revisions, original production binding, explicit disposition/reason and one original GUID intent. Invalid choices, altered identity/revisions, unconfirmed required evidence or out-of-policy acceptance refuse before transaction. The controller must differ from both source creator and proposal preparer.

Canonical AP_INVOICE in TheTechIdea.Data gains nullable notifying VARIANCE_DECISION_JSON, bounded to 65536 characters. Existing PRODUCTION manifest already owns AP_INVOICE, so Beep reflection migration owns this column; live schema evolution has not been applied. A fresh typed/nullable/sufficient-size metadata check is mandatory before writing. No handwritten SQL or business EF path is added.

APInvoiceVarianceDecisionProof stores the complete original command and predecision invoice, including original proposal JSON and accountant change audit, plus the complete recorded row revision. The controller can update only decision proof and standard change audit. Invoice source/financial fields, original proposal bytes and DRAFT status remain unchanged. This is an immutable first disposition; replacement, correction, cancellation, retirement and subsequent history require separate lifecycle work. A within-policy variance disposition does not establish invoice matching, financial approval, journal posting or payment.

The new optional decision property is omitted from JSON when null. An invoice without a decision retains its existing canonical serialized shape and proposal proof bytes/revisions. Proposal verification on a decided invoice resolves the verified predecision invoice through the same canonical proof path. Nested decision history is refused; the original decision invoice must have no prior decision. Altered current row or history cannot confirm either controller history or the underlying proposal. No compatibility or alternate parser is introduced.

## Native transaction and original-intent reconciliation

RecordVarianceDecisionAsync extends the existing APInvoiceService and uses existing PPDM repository/common audit/owned Beep transaction boundaries. Displayed command/review are frozen and required before admission. The original physical PRODUCTION datasource/resolver and current recording authority must remain unchanged. An owned Serializable transaction is required; borrowed/read-committed transactions refuse.

Each review reads full current financial evidence and the globally identified invoice, requires exactly one matching owned row with identical header, and recreates the exact independently addressed controller review. The narrow mutation policy admits only the original invoice/update/actor/previous row, then creates its canonical decision proof after standard audit stamping. Stored row, repeated complete evidence and final authority are verified before commit; an exact durable global row/proof/current-authority read is required afterward.

Exact original retry verifies the stored command, reconstructs only its proven predecision invoice header, and retains current invoice lines, purchase data, allocation/receipt contributors and matching policy. It opens no additional invoice update. Changed evidence, reason, intent, binding or proof cannot replace or silently repair a recorded decision. Commit uncertainty and postcommit authority/durable-read failure propagate; callers must retain and reconcile the same original submission.

## API, SDK and visible recorded outcome

POST api/accounting/traditional/ap/{id}/variance-decision-commands uses the canonical submission/result. The API independently requires current standard app roles, Accounting.View, Procurement.View, Accounting.ReviewInvoiceVariance and Accounting.RecordInvoiceVarianceDecision, with unchanged full role/permission sets, company, actor and binding across fresh evidence boundaries. Lost authority or original route/context mismatch returns 403; changed evidence/schema or uncertain outcome returns 409. No live grant is assigned. IdentityServer remains authentication-only.

AccountingServiceClient freezes/validates the exact submission, uses the original escaped invoice route and refuses late account/company drift. It validates the complete returned command/proof/original invoice against the supplied review; it creates no new intent for retry. Controller screen action/retained intent remains the next step; the SDK caller currently owns the original submission.

Existing APInvoiceWorkspaceState reads/copies verified controller history with the same detail application path, clears it with source/company invalidation and distinguishes malformed recorded proof from absence. APInvoiceSourcePanel shows controller, disposition, reason, original/proposal intents, reviewed evidence/binding, row revision and UTC audit, with explicit separate task/financial obligations. Invalid history is unavailable. The complete payables page suppresses preparation of another independent review when any decision source is already recorded. Mud/theme conventions and the existing read owner are reused; no new client database path, custom CSS/JS or duplicate domain/store is introduced.

## Focused verification

Final affected gate: **1340 passed, zero failed/skipped, terminal exit 0**, 1.5809 minutes; log %TEMP%/oilgas-controller-193-screen-final.log. Smaller final source gate: **519 passed, zero failed/skipped, terminal exit 0**, 9.7717 seconds; log %TEMP%/oilgas-controller-193-source-final.log. Adds 53 cases over Implementation 192. Composition has 366 source/cached dependency warnings; Data is warning-free. Whitespace checks, including new source/document files, pass. Actual Data build passes with zero warnings/errors, 1.80 seconds; log %TEMP%/oilgas-controller-193-data.log.

Initial affected run: 1329 passed, two failed, 1331 total, terminal exit 1, 1.4534 minutes; log %TEMP%/oilgas-controller-193-focused.log. Failures were test-fixture assumptions: foreign ownership produced an unchecked null dereference, and the duplicate-row test called a single-row fixture getter after intentionally duplicating identity. The helper now explicitly refuses a missing owned invoice and the assertion inspects every identified row. Production refusal checks were not weakened. Initial Data compilation also needed a global namespace qualifier, repaired before executing tests.

Corrected backend checkpoint: 1337 passed, zero failed/skipped, terminal exit 0, 1.4123 minutes; log %TEMP%/oilgas-controller-193-final.log. Adds native postcommit/durable-read and actual HTTP unknown-commit cases. The subsequent final source adds three real source-panel rendering cases for valid history, malformed proof and company clear.

Coverage includes both dispositions, preserved full financial/source/proposal fields and audit, exact retry with no extra write, independent reviewer and exact command shape/revisions, out-of-policy refusal, changed full evidence/global identity, fresh bounded metadata, write/readback/late-authority/source/binding rollback, borrowed/insufficient isolation, uncertain commit/changed retry refusal, missing durable row and lost postcommit authority, actual HTTP/typed-client current-grant/role failures and original-intent reconciliation. Recorded proposal retries remain read-only after controller disposition. History returned by the scoped owner is copied; malformed or cleared evidence cannot display a verified controller outcome.

Focused counts overlap; physical provider schema/isolation/durability, hosted DI/OIDC, concurrency, real data, browser/JS and broader application acceptance remain unverified or fixture seams. The composition uses current source with cached dependency assemblies and retains type-conflict warnings. No broad missing-project build is claimed.

## Next persona delivery

Wire controller disposition/reason submission, one frozen original intent, unknown-outcome retention, fresh result/source/line confirmation and exact reconciliation into the existing scoped workspace/payables screen. Connect configured accountant-to-controller handoff and controller inbox/landing status through existing lifecycle/task owners; task completion must follow separately verified disposition evidence. Keep durable cancellation/correction/retirement and lifetime/new-circuit recovery explicit.

Live Beep proposal/decision-column evolution, policy-specific tasks, tax/non-PO and book/period/journal evidence, stock/returns, broader persona dashboards, multinational requirements and live host/provider/browser/release gates remain active. No live migration/grant, commit, push, deployment or phase closure.
