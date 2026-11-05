# Implementation 205 — Independent corrected invoice review

## Persona handoff

After the original accountant records a source correction, an independent reviewer can retrieve current diagnostics through the existing AP controller and typed accounting client. The shared APInvoiceCorrectionDecisionReview contract lives in TheTechIdea.Data and addresses the latest verified correction version, intent, full recorded history, company, reviewer, invoice source revision and production binding. It copies the captured financial graph before exposing it.

The reviewer must differ from the original source maker, proposal preparer and latest correction preparer. Current standard app roles and Accounting.ReviewInvoiceVariance, invoice read and procurement read authority are required. Read authority is sufficient for this diagnostic endpoint; it does not require the accountant's correction write grant. IdentityServer remains authentication-only.

## Current evidence and source boundaries

The API captures current complete invoice/header/lines, purchase sources, receipt allocations, global receipt consumption and matching policies through the existing native read owners. Both complete captures must agree, with current authority checks before, during and after capture. Addressed raw invoice revision, verified full correction history, active DRAFT status, recorded correction request and original production binding are required. Changed role/grant/binding or source evidence refuses publication.

The diagnostic digest includes the complete current financial revision and full verified history. Receipt contributors that changed before the request are included as current facts; a prior financial capture is not silently reused. Changes during capture refuse the response. The typed client freezes selected company/account, escapes the addressed invoice ID and validates the canonical returned graph after a closing selection fence.

This is a read-only diagnostic path. It performs no writes and does not acknowledge the correction, approve matching/posting/payment, or complete a workflow task. The existing correction-work completion refusal remains active. Independent reviewer UI/scoped state and durable version-bound acknowledgement are subsequent work.

## Focused verification

Actual Data builds with zero warnings/errors in 2.15 seconds. The affected 1575-case run completed in 3.4321 minutes with 1574 passing and one new test expectation failure: changed binding correctly returned Forbidden through the existing authorization boundary, while the assertion expected Conflict. Only that assertion was corrected. The final targeted run passes all 22 new canonical/API/client review cases in 31.1947 seconds. These runs overlap; the broader run is not reported as entirely green. The overlapping source gate passes 519 checks in 9.3804 seconds. Composition reports 375 source/cache/package warnings; whitespace verification passes.

Ten canonical cases cover independent identities, exact binding/history/revision/source/address, explicit complete policies/receipt consumption, copying and retained controller request. Twelve actual Kestrel/typed-client/native-owner cases cover current grants/roles/binding, header/line changes, current contributor changes, late grant revocation, maker/preparer refusal, absent history and latest second-version review. All preserve write counts and DRAFT status.

Native provider/physical transactions and authoritative access capture remain fixture seams. Browser/OIDC/accessibility, live schema/grants/provider, circuit lifetime recovery and broad solution acceptance remain unverified. No SQL, new business EF lane, live migration/grant or phase closure is introduced.

## Next implementation target

Expose this independent current review in the existing reviewer screen/scoped owner, then record version-bound independent acknowledgement and confirmed evidence for configured correction workflow work. Source correction alone cannot authorize financial approval or task completion. Live definition provisioning, lifetime recovery, tax/non-PO/book/period/journal evidence, policy tasks, stock/returns, wider persona dashboards and multinational acceptance remain active.
