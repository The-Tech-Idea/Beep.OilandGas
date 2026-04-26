# Sheet 2: Data Folder Boundary

## Goal

Keep only the data classes that are actually needed at the project boundary, organize them into a small core set plus workflow/process-owned slices, and stop treating duplicated PPDM table classes as a permanent design.

## Current Inventory Pressure

- The project currently contains 95 C# files.
- The full `Data` folder currently contains 85 C# files.
- `Data/ProspectIdentification` currently contains 45 C# files, including the `Domain` subfolder.
- `Data/Exploration` currently contains 10 C# files.
- `Data/Evaluation` currently contains 30 C# files.
- The canonical `PROSPECT` table model is local to this project:
	- canonical owner: `Data/ProspectIdentification/PROSPECT.cs`
	- duplicate shared package shape still exists in `Beep.OilandGas.PPDM39.Models.PROSPECT` and should be retired after impact review
- The local `PROSPECT` is the project-owned table shape and already carries the workflow and estimation fields used by the live prospect service boundary.

## Exhaustive Audit

- The complete per-file audit now lives in `05_Data_Class_Audit.md`.
- That audit classifies every file under `Data/ProspectIdentification`, `Data/Exploration`, and `Data/Evaluation` as one of:
	- local storage shape,
	- workflow/progress storage shape,
	- reference/lookup storage shape,
	- request/response contract,
	- domain aggregate/projection, or
	- evaluation/reporting projection.
- The audit also records whether each file is actively used, only used by legacy/parallel services, or has no obvious current usage.

## Proposed Organization Model

### Core data classes

- Core holds the canonical PPDM-backed entities and the small set of aggregate models reused across multiple services or workflows.
- Current core candidates are:
	- `PROSPECT`
	- `PROSPECT_WORKFLOW_STAGE`
	- `Prospect`
	- `ProspectEvaluation`
	- `ProspectRanking`
	- `ProspectRequest`
	- `ProspectResponse`
- `LEAD` and `PLAY` should stay in core only if they are confirmed as reusable exploration inventory shared by more than one workflow.

### Workflow and process slices

- `LeadToProspect`
	- owns classes tied to the `LEAD_TO_PROSPECT` workflow and the creation/update transition into a real prospect.
- `ProspectToDiscovery`
	- owns discovery handoff, analog, risk, volume, and prospect-to-well linkage shapes used by `PROSPECT_TO_DISCOVERY`.
- `GateReviewAndRanking`
	- owns ranking, comparison, report, validation, and economics shapes used by exploration gate-review decisions.
- `SeismicEvidence`
	- owns seismic request/response and interpretation models that support evidence gathering rather than canonical prospect storage.

### Placement rule

- If a class stores canonical state reused across workflows, it belongs in core.
- If a class mainly supports one workflow, one decision gate, or one bounded API flow, it belongs in that workflow/process slice.
- If there is no real workflow or endpoint yet, the class should remain deferred instead of occupying the active data surface.

## Current Local Storage-Backed Table Shapes

These classes represent the exploration inventory and supporting PPDM tables used directly by the module or services today. They are not all guaranteed to remain project-owned long term:

- `Data/ProspectIdentification/LEAD.cs`
- `Data/ProspectIdentification/PLAY.cs`
- `Data/ProspectIdentification/PROSPECT.cs`
- `Data/ProspectIdentification/PROSPECT_ANALOG.cs`
- `Data/ProspectIdentification/PROSPECT_BA.cs`
- `Data/ProspectIdentification/PROSPECT_DISCOVERY.cs`
- `Data/ProspectIdentification/PROSPECT_HISTORY.cs`
- `Data/ProspectIdentification/PROSPECT_PLAY.cs`
- `Data/ProspectIdentification/PROSPECT_RISK_ASSESSMENT.cs`
- `Data/ProspectIdentification/PROSPECT_SEIS_SURVEY.cs`
- `Data/ProspectIdentification/PROSPECT_VOLUME_ESTIMATE.cs`
- `Data/ProspectIdentification/PROSPECT_WELL.cs`
- `Data/ProspectIdentification/EXPLORATION_PROGRAM.cs`
- `Data/ProspectIdentification/EXPLORATION_PERMIT.cs`

## Confirmed Duplicate Ownership To Resolve

- `PROSPECT` is currently defined both locally and in the shared PPDM package.
- The local `PROSPECT` should be treated as a temporary compatibility shape until the refactor chooses one canonical persistence model.
- Future work must either:
	- adopt the shared PPDM `PROSPECT` model and move the extra fields into projections or mappers, or
	- keep a local wrapper/projection with a name and boundary that no longer pretends to be the canonical PPDM table model.

## Workflow And Progress Storage

- `PROSPECT_WORKFLOW_STAGE.cs` is the one clearly justified workflow-progress persistence shape in the current local table set.
- `PROSPECT_HISTORY.cs` looks like a second attempt at audit/history storage, but it is not currently used and overlaps with PPDM/common audit columns.
- `PROSPECT_RANKING.cs` is currently used as an in-memory ranking result shape, not as persisted workflow state.
- Most of the remaining `PROSPECT_*` table-shaped files do not currently store live workflow progress.

## Request / Response Contracts By Process

These should follow the workflow or API process that owns them rather than staying in one flat exploration-contract bucket:

- Core prospect CRUD slice:
	- `ProspectRequest`
	- `ProspectResponse`
- Seismic-evidence slice:
	- `SeismicSurveyRequest`
	- `SeismicSurveyResponse`
	- `SeismicLineRequest`
	- `SeismicLineResponse`
- Discovery-handoff slice:
	- `ExploratoryWellRequest`
	- `ExploratoryWellResponse`
- Discovery or gate-review slice:
	- `RiskAssessmentRequest`
	- `RiskAssessmentResponse`

## Aggregate Models By Process

These should be grouped by the workflow or decision stage they support rather than by a single flat `Evaluation` folder:

- Core reusable prospect views:
	- `Prospect`
	- `ProspectEvaluation`
	- `ProspectRanking`
- Gate-review and ranking slice:
	- `ProspectComparison`
	- `ProspectReport`
	- `ProspectValidation`
	- `EconomicEvaluation`
	- `RiskAssessment`
- Discovery slice:
	- `ResourceEstimationResult`
	- `VolumetricAnalysis`
- Seismic-evidence slice:
	- `SeismicInterpretationAnalysis`
- Future-only portfolio or scenario analysis stays deferred until a real process uses it.

## Reduction Rules

- Use PPDM table models for persistence whenever a verified PPDM table exists.
- Favor a `Core` plus workflow/process slice organization over a flat technical-type split.
- Treat `Data/ProspectIdentification/PROSPECT.cs` as a temporary compatibility shape, not a final ownership decision.
- Audit the other local PPDM-style table classes against the shared PPDM package before keeping them project-local.
- Do not create new duplicate project-local classes for `WELL`, `WELL_STATUS`, `SEIS_ACQTN_SURVEY`, or `SEIS_LINE`; consume the PPDM/shared models directly.
- Do not create new duplicate project-local classes for tables that already exist in `Beep.OilandGas.PPDM39.Models`.
- Do not keep a class in the active surface unless its owner is explicit: core persistence, a named workflow, or a named API process.
- Treat `Data/Evaluation` and `Domain` types as computed projections. They should not become ad hoc persistence shapes.
- Avoid adding new `FIELD`-as-prospect substitutions. Prospect identification should center on `PROSPECT`, with `WELL` and seismic tables attached as supporting evidence.

## Current Status

- [x] The planning boundary is documented.
- [x] The live prospect service now maps the project prospect record more consistently.
- [x] The full `Data` folder has now been audited file by file in `05_Data_Class_Audit.md`.
- [x] The target organization is now documented as core data classes plus workflow/process-owned slices.
- [ ] The confirmed duplicate `PROSPECT` ownership still needs to be collapsed into one canonical model boundary.
- [ ] The speculative local storage shapes still need to be reduced to the subset backed by real processes, endpoints, or persisted workflow state.
- [ ] Seismic and evaluation services still need to drop legacy `FIELD`/`SEIS_SET` stand-ins where those are acting as prospect surrogates.