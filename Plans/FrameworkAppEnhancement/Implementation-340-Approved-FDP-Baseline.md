# Implementation 340: Approved FDP baseline

2026-10-25. Approved FDP baseline creation, exact reload and original recovery are locally verified under CA-22 / FG-52. Continues [339: recorded independent review](Implementation-339-Development-Review-Source-and-Catalog.md), [the development persona](Persona-07-Exploration-Drilling-and-Development.md), [asset delivery](Feature-07-Asset-Delivery-and-Interventions.md) and [phase C](Company-Phase-C-Engineering.md). The full goal remains active.

## App outcome

The planner can rediscover the recorded review from the existing draft source and open review/FDP baseline history. `/development/fdp/history?processId=<encoded identity>` now contains an FDP baseline panel. Current original participants can inspect the baseline with View authority; a checked absence differs from failed or corrupt evidence.

After all configured stages approve, an independent current original recipient of the configured final role can inspect, enter a rationale, review a frozen confirmation and explicitly approve the FDP baseline. The API independently requires standard current app role membership, DevelopmentPlanning.View/Approve and Workflow.View/Approve, exact original FIELD WRITE grant and current resource access. Maker/requester, earlier roles and other companies cannot approve; Workflow.Start/Create permission is not required.

The canonical native header records a new **APPROVED version 2**, with deterministic company/source identity, direct original draft/review lineage, actual approver/UTC and exact reviewed data. The version 1 draft and lifecycle review/audit/task evidence stay unchanged. The draft list remains an actual DRAFT population. The confirmation shows recorded version, identity, source draft, approver, UTC and rationale; no local business identity/status is invented.

Encrypted tab storage retains the exact original baseline command before transport. An uncertain response blocks another approval and offers read-only original recovery, including across a new page owner. Failed retention/receipt/cleanup and changed company/persona/route preserve original recovery input and fence late results; another review cannot replace it. Original recovery needs current View/final-role/FIELD READ authority, without new approval or FIELD WRITE rights.

This approves the recorded FDP baseline. It does not approve an AFE, release investment funds, validate unrecorded engineering studies or accept operations handover.

## Canonical owners and persistence

- TheTechIdea.Data owns DevelopmentPlanRelease, baseline review/receipt and nullable lineage fields on the existing FIELD_DEVELOPMENT_PLAN model. Its original source/review evidence digest is stable across current authorized readers. Full stored-command/header comparison checks the canonical row; no duplicate business entity or alternate approval engine.
- DevelopmentPlanService retains one shared owned Beep transaction implementation for draft and task-bound baseline scopes. No active FIELD preference is fabricated for an assigned reviewer. Exact source draft, baseline identity, scope, original command, schema widths, status/currency references, SDK results and closing readbacks are required.
- The existing DEVELOPMENT_PLANNING manifest still owns the same seven entity types. Nullable SOURCE_DRAFT_FDP_ID, SOURCE_REVIEW_PROCESS_ID, RELEASE_INTENT_ID, RELEASE_REQUEST_REVISION and bounded RELEASE_COMMAND_JSON evolve the existing header through Beep Migration metadata. Null lineage does not change original draft serialization. No SQL, new business EF lane, live migration or older-row relabelling.
- DevelopmentAssignedReviewService shares current company/account/standard-role/workspace/grant/binding checks with review/source readers. Only an original independent final-role recipient can approve. Current native FIELD WRITE/resource checks and exact completed lifecycle/source evidence bracket the planning transaction; fresh app authority is rechecked inside it. A changed closing lifecycle audit leaves the original outcome unconfirmed; no distributed transaction or silent repair is claimed.
- Native approval inserts one next-version header, preserving the reviewed draft. Another original intent cannot create another baseline for that draft. Recovery/read never inserts or repairs. Changed/inactive/ambiguous baseline evidence cannot be reported absent. The existing generic repository guard is retained; the focused gate verifies refusal of unqualified raw FDP inserts.
- Existing controller, typed DevelopmentServiceClient, ProtectedSessionStorage journal and recorded-history page are extended. DevelopmentPlanReleaseState and its MudBlazor panel own inspect/prepare/confirm/recover; existing source screens link to that exact owner. Existing shared failure reporting handles denied/unavailable evidence without success substitution or private exception disclosure. Program registers the baseline interface to the same canonical concrete API owner.

## API paths

All paths are under `/api/field/current/development-planning` and require authenticated app authority with independent API checks.

| Method/path | Meaning |
| --- | --- |
| GET `review-handoffs/{processId}/baseline?expectedReviewRevision=...` | Exact displayed review -> approved baseline or checked absence |
| POST `baselines` | Retained exact approved review, intent and rationale -> canonical next FDP version and confirmed receipt |
| POST `baselines/recover` | Original command -> confirmed original baseline or checked absence; no new write |

## Verified evidence

**693 focused tests pass**, zero failures/skips, **116.532 seconds**, terminal exit 0. Actual Data builds with zero errors and one existing missing IdentityServer-analyzer warning. [Exact command, class counts, final logs, source/runtime hashes and limits](evidence/340-approved-fdp-baseline/observed-evidence.json). The previous 645-check review/history gate is retained in this gate; counts overlap and are not added.

Actual typed HTTP -> controller -> current authority -> real SQLite app role/registry/workspace/grant owners -> controlled native Beep planning/lifecycle sources verifies inspect/approve/reload/recovery, current final role/FIELD authority, company isolation, maker/earlier-role refusal, unchanged draft/history, exact currency/calendar values, retry/lost native commit response, every affected write/readback/refusal boundary, changed closing evidence and no duplicate version. Baseline row/schema/reference corruption refuses or rolls back as appropriate.

Actual rendered history parent drives the complete baseline form and shows recorded version/lineage/audit. State checks retention before transport, uncertainty, page-owner recovery, other reviews, permission changes and late/context changes. Actual ProtectedSessionStorage checks encryption, scope/kind separation and mismatched overwrite/clear refusal. Retained source/catalog/review/decision/routing/landing/inbox/schema regressions pass. Native business sources, FIELD resource checks and external auth remain controlled boundaries; no browser/provider/host or deployed migration acceptance is claimed.

## Remaining app and delivery work

1. Continue through [341: approved FDP to company AFE request](Implementation-341-Development-AFE-Request.md), which replaces the obsolete AFE surface and connects current company/FIELD finance authority, configured books/currency and a native DRAFT request. Its current verification status is owned by that document. Independent investment decisions and fund release remain open; an FDP is never treated as a work order.
2. Continue authoritative design/program/scenario evidence, drilling/construction actuals, approved changes and operations acceptance/handoff under the existing persona/feature owners. Baseline approval does not complete those journeys.
3. Qualify Beep migration preflight/apply/recovery and provider column/LOB/constraint support, real concurrency/reconciliation, hosted role/company/OIDC composition and browser keyboard/accessibility/RTL/localization/multinational acceptance separately. No full feature/phase/goal closure or commit/push.
