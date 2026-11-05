# Implementation 102: Canonical source content and independent intervention approval

## Existing persona action

The engineer approval panel now displays the recorded original activity preparer and explains when independent approval cannot be established. The approval action is disabled and its imperative handler refuses when the current actor equals the preparer or the preparer is unknown. This predicate does not grant roles or permissions; current application/API authority remains required independently.

## Single existing source path

ProductionInterventionSourceReviews now owns original activity selection, decision-activity classification and workflow-marker removal, moved from the controller rather than duplicated. Both candidate projection and decision validation use this owner. A later decision row cannot replace the original source maker/content, and decision-only history cannot fabricate an original intervention.

At every existing source recheck, the actual API helper verifies original revision, exact submitted activity type and canonical recorded problem note. Only recorded workflow markers are removed from the note; a caller cannot submit a different engineering description while retaining the original revision. New decision rationale remains a separate user input.

APPROVED requires a known original ROW_CREATED_BY and a different current actor. Administrator status supplies no maker-check exception. Initial denial returns Forbid through the existing controller. Deferral/rejection can be recorded by the original maker, but still require original reviewed source, explicit rationale and current independent API authority. Candidate DTO adds actual SourcePreparer; the actual evidence component shows it or Unavailable.

## Focused evidence

Actual Data isolated build succeeded, zero warnings/errors, 1.86 seconds; `%TEMP%/oilgas-intervention-independent-data-build.log`.

Actual source helper with actual PPDM/Data dependencies: **36 passed**, zero failures/skips, terminal exit 0, 1.0107 seconds; `%TEMP%/oilgas-intervention-independent-review.log`. Thirteen new cases cover matching content, type/problem tampering with a valid original revision, maker/Administrator identity self-approval, null/blank maker, maker non-approval decisions, latest-decision versus original maker/content, decision-only history and canonical workflow-marker stripping, alongside 23 prior revision/history cases.

Actual Data validation/static MudBlazor evidence component: **23 passed**, zero failures/skips, terminal exit 0, 1.0072 seconds; `%TEMP%/oilgas-intervention-independent-evidence.log`. New cases verify known/unknown preparer display and independent actor eligibility alongside 20 prior evidence checks. Static JS calls fail explicitly rather than simulating a browser. Whitespace diff check passed.

## Remaining complete workflow gates

These are actual helper/shared-predicate/component checks, not native controller/filter/HTTP/parent screen/OIDC/provider execution. Approved company role admission, WELL-to-FIELD membership, current rights revocation and physical database binding remain provider/native gates. Original creator identities must map consistently across historical/provider data. Authority still needs actual role/catalog evidence; the independent-maker predicate is necessary but insufficient authorization.

The existing activity read/decision write sequence remains preflight, not atomic compare-and-swap. Durable idempotent original-context command receipts, partial handoff reconciliation and committed recorded read-back remain open. Separate decommissioning and URL handoffs, planner materials/permit/execution/supervisor return-to-service/AFE/cost outcomes and accountant gates remain in scope. No SQL, schema/migration, business EF lane, grant or phase/goal closure.

Implementation 101's canonical content and independent-maker gaps are advanced here; its native/atomic/durable gates remain open.
