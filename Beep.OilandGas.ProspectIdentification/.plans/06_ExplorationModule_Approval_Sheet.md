# Sheet 6: ExplorationModule Registry Approval

## Purpose

Capture the exact entity list in `Modules/ExplorationModule.cs` and confirm that `ExplorationModule` is the full table registry for ProspectIdentification project tables used by module setup and schema migration.

## Registry Rule

- `ExplorationModule.EntityTypes` is not the minimum live runtime slice.
- `ExplorationModule.EntityTypes` is the project table registry for module setup and schema migration.
- Runtime stage-gating belongs in service, workflow, and data-slice planning, not in trimming the module registry.

## Approved Module Registry List

This is the approved project table registry as it should exist in `ExplorationModule.cs`.

```csharp
typeof(LEAD),
typeof(PLAY),
typeof(PROSPECT),
typeof(PROSPECT_WORKFLOW_STAGE),
typeof(PROSPECT_ANALOG),
typeof(PROSPECT_BA),
typeof(PROSPECT_DISCOVERY),
typeof(PROSPECT_HISTORY),
typeof(PROSPECT_PLAY),
typeof(PROSPECT_RISK_ASSESSMENT),
typeof(PROSPECT_SEIS_SURVEY),
typeof(PROSPECT_VOLUME_ESTIMATE),
typeof(PROSPECT_WELL),
typeof(EXPLORATION_PROGRAM),
typeof(EXPLORATION_PERMIT),
typeof(SEIS_ACQTN_SURVEY),
typeof(SEIS_LINE),
typeof(WELL),
typeof(WELL_STATUS)
```

## Runtime Stage-Gating Still Applies

The workflow slices in the plan remain valid, but they do not remove table classes from the module registry.

### Active runtime slice now

- `PROSPECT`
- `PROSPECT_WORKFLOW_STAGE`
- `SEIS_ACQTN_SURVEY`
- `SEIS_LINE`
- `WELL`
- `WELL_STATUS`

### Stage-later runtime slices

- `LeadToProspect`
- `ProspectToDiscovery`
- `GateReviewAndRanking`
- `SeismicEvidence`

These slices control service activation, UI activation, and data-folder ownership. They do not redefine `ExplorationModule.EntityTypes`.

## Final Approval Decision

Approved rule:

- `ExplorationModule.cs` keeps the full project table registry.
- Runtime stage-gating stays in the service/workflow/data-slice plan.
- No project table class should be removed from `EntityTypes` just because its workflow is not active yet.

## Result If Approved

- `ExplorationModule.cs` remains the authoritative ProspectIdentification project table registry.
- The active runtime slice is controlled outside the module registry.
- Future planning should not collapse the schema-registry boundary and the runtime-slice boundary into one list.