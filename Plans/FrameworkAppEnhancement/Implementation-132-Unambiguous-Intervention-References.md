# Implementation 132: Unambiguous intervention references

## Production and maintenance link integrity

The existing activity format stores workflow references as whitespace-delimited remark tokens. The controller previously selected the first matching token, including marker-like text inside a word, while the independent source-note remover used another expression. Duplicate references could silently select one order or financial link, and rationale text could introduce an unintended handoff marker.

Data-owned ProductionInterventionMarkers is now the single reader for that existing format. It recognizes complete supported tokens, preserves opaque punctuation and international characters, and refuses empty or duplicate references, including repeated identical values. The current source-note reader delegates to it; embedded marker-like word fragments remain rationale. The controller reads the entire reference set once per recorded row and removes its old first-match extraction helper. Ambiguous original activity/reference projection becomes an explicit source conflict instead of a successful empty list or guessed first link; the actual conflict handling remains source-only in this slice.

ProductionInterventionDecisionChecks validates rationale and explicit reference fields before retained intent or decision write. Rationale cannot inject reserved workflow marker tokens. IDs containing whitespace cannot be represented unambiguously by the current token format and refuse; no encoding reinterpretation or fallback was added. RecordedRemark uses the same guards and existing output format. A future structured relationship owner is still needed for references outside this format; no live records are rewritten.

This extends existing Data/API/source/state owners. It adds no schema, business EF lane, SQL, endpoint, role store, handoff owner or automatic repair. Source/resource/independent actor checks and exact recorded-outcome review remain mandatory.

## Focused verification

Current actual Data source builds cleanly: zero warnings/errors, 1.62 seconds.

Scripts/Test-ProductionInterventionSourceReviews.ps1: **51 passed**, zero failed/skipped, terminal exit 0, 1.0195 seconds. Fifteen new cases cover international/punctuated complete identities, five empty/duplicate-reference cases, embedded marker-like words retained as notes, six reserved rationale tokens refused through the actual API source helper, and formatter/reader round-trip. Existing 36 source review cases remain green. Log: `%TEMP%/oilgas-intervention-markers-132.log`.

Scripts/Test-ProductionInterventionDecisionState.ps1: **49 passed**, zero failed/skipped, terminal exit 0, 0.9584 seconds. Three new cases refuse reserved rationale and whitespace-containing order/AFE identities before they become pending. Existing 46 decision/readback cases remain green. Log: `%TEMP%/oilgas-intervention-marker-state-132.log`.

These checks execute actual current Data/source-helper/Web decision state with controlled inputs. Full controller projection/conflict handling, typed transport/HTTP, parent page/host/browser and native persisted activity behavior remain unverified. No live writes/migrations/deployment. Whitespace check passes. Parser coverage is not completion of multilingual UX or multinational policy.

## Remaining plan

Verify the actual recorded production decision/source/link journey through controller/client/browser/provider. Replace remark-only relationships with the existing authoritative structured owners where required; retain exact link identities and audit history. Durable command receipts and transactional consistency, maintenance duties/assignment/planning/permits/independent acceptance/return-to-service/cost, other company personas and all multinational gates remain open. No persona/phase/goal closes.
