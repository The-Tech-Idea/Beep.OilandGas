# Sheet 9: Implementation Trigger Sheet

## Purpose

Define the exact conditions that must be true before a `Stage Later` slice is allowed to move into active code.

## Global Trigger Rule

A slice may move from `Stage Later` into active code only when all four conditions below are true:

1. A real business workflow or user-visible process for the slice is approved.
2. At least one API, lifecycle service, or UI flow is explicitly consuming the slice.
3. The data boundary for the slice is settled: persisted PPDM-backed classes versus transient projections are identified.
4. The slice has a named owner in the plan bundle and its classes are no longer ambiguous in the disposition matrix.

If any one of these is false, the slice stays `Stage Later`.

## Slice Triggers

### `LeadToProspect`

Move to active code only when:

- the `LEAD_TO_PROSPECT` workflow is explicitly part of the approved live scope
- there is a real lead intake, screening, or promotion endpoint or UI flow
- `LEAD` and `PLAY` are confirmed as active workflow data rather than passive inventory tables

### `ProspectToDiscovery`

Move to active code only when:

- the `PROSPECT_TO_DISCOVERY` workflow is explicitly part of the approved live scope
- there is a real discovery-handoff, drill-candidate, or technical maturation flow in API or UI
- the persisted versus transient boundary is approved for `PROSPECT_DISCOVERY`, `PROSPECT_WELL`, `PROSPECT_RISK_ASSESSMENT`, `PROSPECT_VOLUME_ESTIMATE`, `PROSPECT_ANALOG`, and `PROSPECT_SEIS_SURVEY`

### `GateReviewAndRanking`

Move to active code only when:

- the `GATE_EXPLORATION_REVIEW` workflow is explicitly part of the approved live scope
- there is a real ranking, comparison, report, or drill/defer decision flow in API or UI
- it is confirmed whether any gate-review state must be persisted beyond `PROSPECT_WORKFLOW_STAGE`

### `SeismicEvidence`

Move to active code only when:

- a real seismic evidence workflow is approved beyond shared PPDM evidence-table usage
- there is a real seismic request, interpretation, or review flow in API or UI
- it is confirmed which seismic models remain transient and which, if any, require local persisted linkage beyond shared PPDM tables

## Result When Triggered

When a slice is triggered:

- its classes may move from `Stage Later` into active code planning
- the module approval sheet may be updated if the slice adds new module entity types
- the disposition matrix should be revised so the slice classes no longer remain `Stage Later`

## Result When Not Triggered

- the slice remains documented only
- no code changes should be made for that slice
- its classes remain outside the minimum live module list