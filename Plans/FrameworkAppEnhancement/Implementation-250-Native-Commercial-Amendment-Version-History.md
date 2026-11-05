# Implementation 250 — Native commercial amendment and version history

## Canonical versioned preparation

The existing native terms owner now accepts a SalesContractTermsAmendment after a recorded rejection. The command freezes the exact prior record/native proof revision, a distinct proposed version and intent, the original preparer, current source review and explicit amendment reason. Approved or undecided predecessors cannot be amended through this correction path. Company, contract, FIELD and revenue transaction remain bound to the original version. Proposed effective dates, currency, price, units/basis and method still require explicit valid terms; no values are inferred.

SalesContractTermsAmendment.NormalizeSource produces the reviewed original native business facts with the current revenue source capture. It validates the predecessor against the actual current proof and excludes the proof envelope itself from the new proposal's source contract, avoiding recursive duplication of earlier versions. Current source changes still require a fresh complete review; the command does not bypass source or maker evidence.

## Preserved native history

SalesContractTermsNativeProof adds optional flat PriorVersions and Amendment metadata in the existing TERMS_WORKFLOW_JSON column. Every preserved entry contains its full preparation/decision record, native row snapshot, original native proof revision and original amendment link/reason. Each entry is checked against its prefix history, audit actor/time, company/seller/contract business facts, rejection decision and chronology. Successive preparations retain the same original preparer and exact FIELD/transaction. All version labels and preparation intents are unique across the complete envelope.

History allows at most sixteen predecessors plus the current version and remains bounded by the existing 524288-character native evidence capacity. Exceeding either bound refuses; history is never truncated. Optional members are omitted for the original single-version format, preserving its serialization and digest shape. Every new decision retains the history and amendment origin instead of dropping it. The native base price, creator and other business facts remain unchanged.

## Existing command owner and transaction

ProductionAccountingService.AmendSalesContractTermsAsync extends its existing RecordCommercialTermsAsync path, owned Serializable Beep transaction, schema preflight, exact SALES_CONTRACT row policy, original source/authority callbacks, audit stamping and committed read-back. The first amendment append preserves the complete old proof entry and records an undecided new preparation. Its later decision uses the existing independently validated decision command and version-specific preparation digest.

The predecessor proof revision must match the actual native record before the append. Same-intent retries require the same proposed preparation, exact previous record, proof revision and amendment reason; changed reasons or predecessors refuse. Exact retries add no native write. Uncertain commits reconcile the original intent. Authority loss after the row update rolls back the complete native change. The result is copied evidence, not permission or an approved effective price.

This is verified native command delivery. No amendment HTTP endpoint or persona form is exposed yet. API authority must additionally require the original rejection-review process to be closed and must provide historical process/source reads before allowing a new version. Native command callers continue to supply authoritative source/access callbacks; no direct client database access was introduced.

## Focused verification

Canonical Data builds with zero warnings/errors. Eleven new actual native tests cover version-two append, independent approval of the amended version, a second rejection/amendment retaining both predecessors, uncertain commit/same-intent retry, changed predecessor, wrong maker, repeated version label, approved predecessor refusal, changed retry reason, altered historical evidence and in-transaction authority-loss rollback. They verify preserved original records/native snapshots/proof revisions/reason, undecided new preparations, independent new decisions, native write counts and unchanged base price.

The current exact-source/Razor regression passes 322 tests with zero failures/skips in 27 seconds: FieldRevenueSourceTests, CompanyProcessStart, CompanyStep and PersonaInbox. The preceding original-version preparation/decision/API/typed-client/rendered workflows remain green, including rejection routing and retained completion. Source/dependency composition warnings remain; full host/solution build is not claimed.

Reproduce using Scripts/Test-PurchaseOrderOwnership.ps1 with the actual canonical Data DLL, IncludeWorkflowInbox and filter FullyQualifiedName~FieldRevenueSourceTests|FullyQualifiedName~CompanyProcessStart|FullyQualifiedName~CompanyStep|FullyQualifiedName~PersonaInbox. Native writes occur only in fixtures. These tests do not prove real provider schema/isolation/concurrency, browser/OIDC or multinational acceptance.

## Next API and persona delivery

Implement a process-bound amendment review/submission API that proves the exact rejection-review workflow is closed, independently checks current company roles/grants/FIELD and binds the new native intent to that predecessor. Preserve historical process/source display against the stored prior version rather than resolving an old process to the new current proposal. Historical screens must be explicit and read-only; only the new version's configured independent handoff may decide that proposal.

Add retained amendment state/form and version history to the original-preparer workbench, then verify original-preparer inbox -> rejection review -> explicit amendment -> native read-back -> new independent task/decision through actual rendered screens and local HTTP. Continue durable circuit-loss/historical intent recovery, approved delivery/effective terms to invoice/recognition and ledger read-back, royalty/partner queues and the wider thirteen-persona/multinational plan.

No schema change, handwritten SQL, new business EF lane, live data write, permission assignment, commit/push or deployment was executed. Business schema remains on Beep Migration. No CA/FG phase or overall goal closes; provider/browser/keyboard/RTL/accessibility, concurrency/performance and broader purchase-order handoff rendering gaps remain open.
