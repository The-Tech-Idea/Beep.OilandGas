# Implementation 106: Company/FIELD observation review provenance

## Canonical data and current screen

Data now owns WellTestReview: original company, actor, FIELD, well, UTC retrieval time and the existing canonical recorded test responses. The operator review consumes this envelope, compares every scope member with the company/account captured before FIELD lookup and validates retrieval. Missing/default/non-UTC retrieval or any company/account/FIELD/well mismatch cannot publish observations, coverage or selected detail. Clearing context also clears retrieval metadata. The screen labels retrieval as distinct from a committed source snapshot.

## Existing API/client reader

The current FIELD production API adds a review response endpoint, `/wells/{wellId}/tests/review`, and uses one ReadWellTestReviewAsync reader for it and its existing list response. That reader requires the selected company/account/FIELD, checks WELL READ, invokes the existing bound PPDMProductionService reader, refuses null or changed context and rechecks WELL rights/FIELD before returning original scope metadata. The existing FIELD filter still applies; this source inspection is not native enforcement evidence.

The existing typed production client uses the canonical review endpoint. Its existing GetWellTestsAsync delegates to that response and extracts rows for current callers; there is no second store/query/fallback. The review screen invokes the scoped response directly. Existing external list endpoint shape is retained through the same API reader rather than duplicated data logic.

## Focused evidence

Actual Data source isolated build succeeded, zero warnings/errors, 1.91 seconds; `%TEMP%/oilgas-well-test-provenance-data-build.log`.

Test-WellTestReviewState.ps1 now references actual compiled canonical Data review/response types with PPDM/base dependencies and no old cached Models DTO assembly or response-source override. **25 passed**, zero failures/skips, terminal exit 0, 1.1656 seconds; `%TEMP%/oilgas-well-test-provenance-state.log`. Eight new cases cover mismatched company/tenant/actor/FIELD/well, missing retrieval, non-UTC retrieval and null review, alongside 17 current identity/context/coverage/detail cases. UTC retrieval is asserted for valid publication. Whitespace diff check passed.

No native parent Razor/DI/HTTP/controller/filter/provider or browser/OIDC claims are made. The additive endpoint and typed-client orchestration remain source-level changes until those checks execute.

## Remaining operator completion gates

Capture through generic WELL_TEST insertion was inspected but not connected here: that editor does not yet prove independent approval, correction reason/version history or shift handover. Existing process testing payloads are still not presented as recorded observation insertion. Required original-context capture/correction, role/resource enforcement, durable command receipt/read-back, native parent interaction and supervisor/next-shift task handoff remain open.

The review envelope asserts provenance from the current reader; provider stable physical binding, well membership and authority revocation still need native proof. Retrieval time is not transactional as-of or optimistic source revision. Provider null-to-zero ambiguity and original/current unit semantics require real-data verification. Other production consumers extracting list rows need their own publication scope checks. All accountant and other persona plans remain active. No SQL, schema/migration, business EF lane, grants or phase/goal closure.
