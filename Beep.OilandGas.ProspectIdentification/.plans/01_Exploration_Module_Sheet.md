# Sheet 1: Exploration Module

## Goal

Make the ProspectIdentification module own the actual exploration inventory used by prospect work, organized around the real exploration business process rather than around a flat class list.

## Planning-Only Rule

- This sheet defines the target module class layout only.
- Do not change `Modules/ExplorationModule.cs` from this sheet until the planned class list is approved.
- Any add/remove action below is a planned module change, not a code change already made.

## Exploration Process First

The module should reflect the normal exploration sequence:

1. Lead screening
2. Prospect maturation
3. Technical evidence gathering
4. Risk and volume estimation
5. Gate review and drill-candidate selection
6. Discovery or dry-hole outcome capture

That means module ownership should prioritize the few entities that support this chain end to end.

## Revised Entity Ownership

### Core exploration master data

- `LEAD`
- `PLAY`
- `PROSPECT`

These are the only classes that can credibly act as top-level exploration inventory. Even here, `LEAD` and `PLAY` should stay only if they are part of active workflows rather than passive catalog tables.

## Concrete Target Class Layout

### `Core`

Purpose:
canonical exploration identity and the smallest persisted state needed across workflows.

Planned module entity types:

- `PROSPECT`
- `PROSPECT_WORKFLOW_STAGE`
- `SEIS_ACQTN_SURVEY`
- `SEIS_LINE`
- `WELL`
- `WELL_STATUS`

Why this is the minimum live slice:

- `PROSPECT` is the real business object already used by the live service.
- `PROSPECT_WORKFLOW_STAGE` is the only clearly justified persisted workflow-state class.
- seismic and well tables are shared evidence tables already used to support exploration decisions.

### `LeadToProspect`

Purpose:
lead intake, lead screening, and promotion into a prospect.

Planned module entity types only if this workflow is active:

- `LEAD`
- `PLAY`

Planned non-module process shapes:

- `CreateProspect`
- `UpdateProspect`
- validation and screening projections

Rule:

- if lead screening is not part of the active API slice yet, keep these out of the minimum live module list.

### `ProspectToDiscovery`

Purpose:
technical maturation of a prospect into a drill-ready or discovery candidate.

Planned module entity types when this workflow is active:

- `PROSPECT_DISCOVERY`
- `PROSPECT_WELL`
- `PROSPECT_RISK_ASSESSMENT`
- `PROSPECT_VOLUME_ESTIMATE`
- `PROSPECT_ANALOG`
- `PROSPECT_SEIS_SURVEY`

Rule:

- these are second-wave module additions, not part of the minimum live slice unless the discovery handoff and technical maturation workflow is implemented end to end.

### `GateReviewAndRanking`

Purpose:
rank, compare, review, and decide drill, defer, monitor, or abandon.

Planned module entity types:

- none by default beyond `PROSPECT_WORKFLOW_STAGE`

Planned process-owned transient classes:

- `ProspectRanking`
- `ProspectComparison`
- `ProspectReport`
- `ProspectValidation`
- `EconomicEvaluation`

Rule:

- gate-review behavior should stay mostly projection-driven unless a real requirement appears for persisted decision history beyond workflow stage.

### `SeismicEvidence`

Purpose:
capture and interpret seismic evidence that supports prospect maturation.

Planned module entity types:

- no additional local seismic entities beyond `PROSPECT_SEIS_SURVEY` when prospect-to-discovery is active

Planned process-owned transient classes:

- `SeismicSurveyRequest`
- `SeismicSurveyResponse`
- `SeismicLineRequest`
- `SeismicLineResponse`
- `SeismicInterpretationAnalysis`

Rule:

- shared PPDM seismic tables stay primary; request/response and interpretation models stay outside the module entity list.

### Outcome and workflow-support data

- `PROSPECT_WORKFLOW_STAGE`
- `PROSPECT_DISCOVERY`
- `PROSPECT_WELL`

These support prospect progression and outcome capture. They are closer to business-process state than to generic master data.

### Technical evidence and decision-support data

- `PROSPECT_ANALOG`
- `PROSPECT_BA`
- `PROSPECT_HISTORY`
- `PROSPECT_PLAY`
- `PROSPECT_RISK_ASSESSMENT`
- `PROSPECT_SEIS_SURVEY`
- `PROSPECT_VOLUME_ESTIMATE`

These are supporting entities. They should stay only when they back a real screening, maturation, or gate-review step.

### Exploration program and permitting

- `EXPLORATION_PROGRAM`
- `EXPLORATION_PERMIT`

These are not starting-point data models for the current live prospect slice. They are later-stage planning and approval shapes and should remain secondary unless the active implementation reaches well-program or permit workflows.

### Shared PPDM evidence tables used by exploration workflows

- `SEIS_ACQTN_SURVEY`
- `SEIS_LINE`
- `WELL`
- `WELL_STATUS`

## Explicit Non-Goals

- Do not keep `POOL`, `POOL_AREA`, `POOL_COMPONENT`, or `POOL_VERSION` in this module. Those belong to downstream development/reservoir ownership, not prospect identification.
- Do not duplicate shared reference-data seed behavior inside this project. Shared seeding remains in `Beep.OilandGas.PPDM39.DataManagement`.
- Do not treat every geology or detail class as a core module entity. If a class does not support a real workflow step, keep it out of the active module boundary.

## What The Module Really Needs First

### Must-have now

- `PROSPECT`
- `PROSPECT_WORKFLOW_STAGE`
- shared seismic evidence tables
- shared well evidence tables

Concrete planned minimum live module list:

- `PROSPECT`
- `PROSPECT_WORKFLOW_STAGE`
- `SEIS_ACQTN_SURVEY`
- `SEIS_LINE`
- `WELL`
- `WELL_STATUS`

### Keep only if active workflow is confirmed

- `LEAD`
- `PLAY`
- `PROSPECT_DISCOVERY`
- `PROSPECT_WELL`
- `PROSPECT_RISK_ASSESSMENT`
- `PROSPECT_VOLUME_ESTIMATE`

### Defer until a real business process uses them

- `EXPLORATION_PROGRAM`
- `EXPLORATION_PERMIT`
- `PROSPECT_HISTORY`
- `PROSPECT_BA`
- `PROSPECT_PLAY`
- speculative geology, portfolio, or detail-heavy prospect support tables

## Current Status

- [x] Module entity list revised to the exploration-domain core tables.
- [x] Shared reference-data seeding kept on `PPDMReferenceDataSeeder.SeedAnalysisReferenceDataAsync`.
- [x] Module ownership is now framed against the real exploration process rather than against a flat inventory of possible classes.
- [ ] Confirm whether any additional prospect PPDM tables should be promoted from `Data/ProspectIdentification` into `Beep.OilandGas.PPDM39` in a later consolidation.

## Verification

- `ExplorationModule.cs` compiles after the entity-list change.
- The module remains registered from `Beep.OilandGas.ApiService/Program.cs` as the project-owned exploration module.