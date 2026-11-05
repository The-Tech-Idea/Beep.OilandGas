# Implementation 101: Original intervention source revision

## Persona action

The existing engineer candidate review now carries a digest of its recorded activity history. The existing decision request requires ExpectedSourceRevision and the response identifies ReviewedSourceRevision. The screen freezes the source revision alongside well/FIELD/company/decision before transport, and refuses a returned result with a different review. It does not adopt a newer source while submitting approval, deferral or rejection.

## Existing API integration

ProductionInterventionSourceReviews hashes actual WELL_ACTIVITY records, sorted by observation number, together with company, actor, FIELD and well. It rejects absent history, mismatched well identity, null rows and duplicate observation identity. The full activity object is included, so edited original notes, dates, type, source, creator and active status change the review even if observation identity stays the same. UTC retrieval timestamps are not part of this source digest.

The existing list endpoint creates the source revision while projecting its original activity group. Candidate validation refuses missing/malformed revisions. The decision endpoint requires a selected authoritative company/account and re-reads that well's activity history through its existing FIELD service. It compares the original revision before orchestration, again before draft creation, before optional financial handoff and before recording the decision. Mismatch returns conflict with review guidance; conflicts after a draft may include its identity and explicitly warn earlier writes may exist. No automatic replacement source, SQL, store, schema, migration or business EF lane was added.

## Focused evidence

Actual isolated Data build: succeeded, zero warnings/errors, 1.93 seconds; `%TEMP%/oilgas-intervention-review-data-build-final.log`.

- Actual API helper with actual PPDM WELL_ACTIVITY/Data dependencies: **23 passed**, zero failures/skips, terminal exit 0, 0.9803 seconds; `%TEMP%/oilgas-intervention-source-review-final.log`. Covers independent materialization, provider ordering, edited original fields, appended decision, scope binding, absent/ambiguous history, malformed requested reviews and exact original match.
- Actual decision state/shared validation against current Data: **25 passed**, zero failures/skips, terminal exit 0, 0.9996 seconds; `%TEMP%/oilgas-intervention-decision-review-final.log`. Includes frozen source revision and rejecting a different returned reviewed revision.
- Actual candidate validation/static MudBlazor component against current Data: **20 passed**, zero failures/skips, terminal exit 0, 1.0224 seconds; `%TEMP%/oilgas-intervention-evidence-review-final.log`. Includes missing candidate source revision rejection and the previously recorded nullable evidence/rendering cases.

Whitespace diff check passed. These checks prove helper/state/component behavior, not execution of the whole controller, native parent page, HTTP/client/OIDC or provider.

## Open completion gates

This is optimistic preflight only. Reads and writes are not an atomic provider compare-and-swap; a concurrent change after the last check can still race a decision, and multi-step handoffs may leave partial writes. ReviewedSourceRevision is an API response assertion, not independent committed read-back or a durable command receipt. Durable original-context idempotency/reconciliation, native company/FIELD/well authority and physical provider binding/concurrency need implementation and verification.

Hashing all activity properties includes fields not displayed in the current review; materialization stability is covered by fixtures, not every real provider. Native provider serializers/data shapes remain a gate. Request intervention type/problem still need canonical content validation against reviewed source rather than trusting arbitrary submitted content. Independent approval authority, source-to-work-order/AFE marker read-back, original-context URL handoffs, separate decommissioning commands and complete planning/permit/execution/return-to-service/cost outcomes remain open. All accountant and other persona plans remain active; no phase or overall goal is closed.


Implementation 102: [Canonical content and independent intervention approval](Implementation-102-Independent-Intervention-Approval.md), 36 actual source-helper and 23 evidence/static-render cases pass. The current approval requires matching recorded content and a known different preparer; native authority, provider atomicity and durable complete persona handoffs remain open.
