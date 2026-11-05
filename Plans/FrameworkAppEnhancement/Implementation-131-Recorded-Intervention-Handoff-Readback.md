# Implementation 131: Recorded intervention handoff readback

## Production engineer to maintenance

The existing intervention decision page previously cleared its pending decision from a successful response and could navigate immediately to a returned order. It now reads the existing intervention-candidates endpoint after the response, validates the original company/actor/FIELD population, and confirms the original well's recorded activity before clearing that pending decision or following an order link. Missing or changed readback preserves the pending outcome and blocks repeat submission for the page lifetime.

The Data-owned candidate adds actual latest WELL_ACTIVITY type, raw remark and recorded-by identity. The existing API candidate projection supplies them from the latest recorded row. The existing production evidence component displays that activity, actor and rationale/links with Razor encoding; unavailable fields remain unavailable. This distinguishes original recommendation evidence from the latest recorded decision instead of displaying the original recommendation note as decision rationale.

ProductionInterventionDecisionChecks.RequireRecordedOutcome validates the response's original source revision, FIELD/well/decision and explicitly requested order/AFE handoff. Unrequested order or financial links refuse. It requires fresh recorded source revision, exact decision/status/activity type, actual current actor as recorded writer, exact rationale/marker content, response/readback link identities and a latest observation after the original recommendation. RecordedRemark now owns the existing note/marker format in Data; the API formatter delegates to it without changing output. The existing decision state delegates to the shared check and only then clears pending intent. No new schema, store, workflow owner, endpoint, SQL or business EF lane.

The page checks original company/FIELD before readback and captures its source-load version/selected record. A later selection cannot reuse an older click's return link/navigation context. The preexisting invalid class-level `_diagnosticsError=null` statement was removed; legitimate resets inside methods remain. Complete page compilation is still unverified.

Readback confirms the current recorded facts and a matching response. It is not a durable command receipt or proof that a particular submission exclusively caused those facts. Cross-session intent, stable command identity, concurrent changes and transaction consistency still require the existing durable workflow work. Independent approval/source/resource gates remain authoritative; no role or permission is inferred. Approval does not automatically create a work order in the UI, and a selected existing order still requires actual FIELD/UWI linkage at the API.

## Focused verification

Current actual Data source builds cleanly: zero warnings/errors, 1.64 seconds.

- Scripts/Test-ProductionInterventionDecisionState.ps1: **46 passed**, zero failed/skipped, terminal exit 0, 0.9584 seconds. Nineteen new cases cover twelve malformed/foreign/missing readback cases, three unrequested response links, exact recorded approval/deferral/rejection without handoff, and explicit existing-order confirmation. Existing 27 retained-intent cases now supply actual reviewed readback facts. Log: `%TEMP%/oilgas-intervention-readback-131.log`.
- Scripts/Test-ProductionInterventionSourceReviews.ps1: **36 passed**, zero failed/skipped, terminal exit 0, 1.2023 seconds. Existing actual source revision/independent approval rules remain green. Log: `%TEMP%/oilgas-intervention-source-131.log`.
- Scripts/Test-ProductionInterventionEvidence.ps1: **24 passed**, zero failed/skipped, terminal exit 0, 0.9112 seconds. The additional actual component case shows latest decision type/writer/rationale/link while encoding recorded markup. Existing evidence cases remain green. Log: `%TEMP%/oilgas-intervention-evidence-131.log`.

Actual current Data/state/source-review/component sources are tested with controlled boundaries and actual MudBlazor. These gates do not execute the full controller/projection, production typed client/HTTP, parent intervention page/host/browser, native PPDM write/readback or concurrency. Source navigation/version checks and the corrected parent-page declaration remain unverified by full Razor/host execution. Whitespace check passes. No live writes, migration execution or deployment.

## Remaining execution

Verify the actual production decision -> activity write -> original-context readback -> recorded maintenance detail journey through client/controller/host/browser/provider. Introduce durable command receipts and exact transactional revision enforcement through existing owners. Complete maintenance assignment, approved duty mapping, permits, supervisor acceptance, return-to-service and cost handoffs, plus the remaining company persona and multinational requirements. No persona/phase/goal closure.
