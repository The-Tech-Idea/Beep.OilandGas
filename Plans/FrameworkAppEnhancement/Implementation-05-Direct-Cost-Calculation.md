# Implementation 05: direct cost calculation

Date: 2026-09-21. Continues [implementation 04](Implementation-04-Run-Ticket-Reconciliation.md). Authoritative status: [master tracker](../../MASTER-TODO-TRACKER.md).

## Implemented behavior

The field cost adapter no longer invokes the production-volume allocator. It reads active ACCOUNTING_COST rows by FIELD_ID and inclusive calendar dates, then computes a direct breakdown by each cost's recorded WELL_ID, or PROPERTY_ID when no well assignment exists. Capitalized and expensed costs remain separate. Duplicate source IDs, missing targets, negative adjustment amounts and ambiguous capital/expense classifications are rejected before a result is returned.

Only DirectAllocation is supported by this field/date contract. Step-down, reciprocal and activity-based methods require allocation bases that the request does not provide; they return an explicit unsupported-method error rather than silently using direct allocation.

This is a **read-only calculation**, not an accounting posting. The controller no longer inserts a synthetic COST_ALLOCATION summary without a source COST_TRANSACTION_ID or target allocation. It forwards the selected connection and rejects client-supplied total overrides. The existing posting service ICostAllocationService remains the owner of explicit per-cost allocations; it is not called by this calculation endpoint.

The page now labels this behavior as a direct cost breakdown, offers only the supported method, removes total override inputs, clears stale results before requests and rejects a null response. External callers using the existing route must account for this corrected read-only behavior and new validation responses before release.

The current ACCOUNTING_COST contract has no currency/unit field. Amounts are therefore treated as recorded ledger amounts; no FX conversion or claim of cross-currency compatibility is introduced. Currency ownership/normalization remains part of the accounting data gate.

## Verification

**43 focused tests pass**, including 10 new direct-cost calculation cases. They exercise the production calculator source for conserved totals, explicit well/property targets, capital/operating separation, unsupported methods, invalid classification, missing/duplicate records and negative adjustments. Existing 33 cases remain green.

API build: **2 errors / 42 warnings**, 6.24 seconds. Both errors are in the royalty adapter: nonexistent IRoyaltyService.GetRoyaltyCalculationsAsync and unsupported ProductionRoyaltyCalculationResult.RoyaltyAmount. Full controller/Razor compilation, database filter integration and actual posting workflows remain unverified. Focused calculation tests do not establish API or browser readiness.

Evidence: `%TEMP%/beep-direct-cost-tests.log`, temporary harness `TestResults/direct-costs.trx`, and `%TEMP%/beep-oilgas-build-current.log`. Harness locator: `%TEMP%/beep-notification-harness-path.txt`. Repository tests: `Beep.OilandGas.ApiService.Tests/DirectCostAllocationCalculatorTests.cs`.

## Royalty decision (resolved in implementation 06)

The existing field royalty action conflates calculating new royalty obligations from allocation details with summarizing stored calculations, then attempts to save fields absent from the canonical royalty entity. Clarification requested: calculate from recorded lease interests, or summarize existing calculations. The answer determines whether this action invokes the existing royalty posting service with an authenticated actor or remains a read-only query. Neither field IDs nor pool IDs may be substituted for lease IDs or royalty-interest IDs without an authoritative mapping.

FG-00 remains open until that flow is corrected and the normal application/test gates pass. No royalty fallback or fabricated financial result was added.
