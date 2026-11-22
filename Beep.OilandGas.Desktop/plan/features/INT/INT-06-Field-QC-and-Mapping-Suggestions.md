# INT-06 — Field QC and mapping suggestions

| Field | Value |
|---|---|
| Stage | STG-17 — Data interop, mapping and 3D |
| Area | Interop |
| Status | Gate met - see [evidence](../../evidence/STG-17-INT-06/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Loaders`, `TheTechIdea.OilGas.Wellbore.Setup`, `TheTechIdea.OilGas.DataAI` |
| Desktop project | `Modules/Interop/FieldQc/` |
| External packages | `Wellbore.Loaders` (`FieldQc`, `DataCoverage`), `Wellbore.Setup` (`MappingSuggestions`), `DataAI` |
| Depends on | INT-05 |
| Blocks | — |
| Web reference | `OilandGasTools/docs/customer/field-qc-and-suggestions.html` |

## 1. Purpose

Find the defects across a whole field's wells before they poison every calculation, and get
suggestions for the mapping gaps — never applied automatically.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Loaders/Tabular/FieldQc.cs` | `Run(source, options)`, `Run(provider, sourceName, …)` |
| `Wellbore.Loaders/Tabular/DataCoverage.cs` | `Analyse(source, options) → CoverageReport` |
| `Wellbore.Setup/SetupSession.cs` | `Suggestions`, `Accept`, `CandidatesFor` |
| `Wellbore.Setup/MappingSuggestions.cs` | `SimilarityMatcher` |
| `DataAI/WellReconciliation.cs` | duplicate identity proposals |
| `DataAI/LegacyExtraction.cs` | extraction proposals for review |

## 3. Screens

`Data → Field QC`

- Scope: source and field; run (cancellable).
- Findings grid: well, entity, field, rule, severity, message, first seen.
- Coverage panel: per entity, fraction of wells with the field populated.
- Suggestions panel: unmapped value → candidate canonical value with a score; accept or reject.
- Duplicates panel: candidate duplicate wells with score and evidence.
- Export findings to CSV.

## 4. Data and contracts

`OgQcFinding { WellId, Entity, Field, Rule, Severity, Message }`
`OgSuggestion { Source, Target, Score, Accepted }`.

## 5. States

| State | Behaviour |
|---|---|
| Source not configured | run disabled with the reason |
| Coverage cannot be computed | shown as unknown, not zero |
| Suggestion below threshold | hidden by default, shown with "show all" |
| Run cancelled | partial findings reported with the scope covered |
| No findings | explicitly "no findings for this scope" |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run QC | `interop.qc.read` | no |
| Accept a suggestion | `settings.wellbore.setup` | environment file |
| Export | `data.export` | no |

## 7. Acceptance Criteria

- AC-1 A suggestion is never applied without explicit acceptance.
- AC-2 Unknown coverage is distinct from zero coverage.
- AC-3 A cancelled run reports the scope actually covered.
- AC-4 Duplicate candidates are proposals only.
- AC-5 Findings carry entity, field and rule.
- AC-6 Export matches the grid filter.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~INT-06"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-17-INT-06/observed-evidence.json).

**The run is the engine's.** `FieldQc.Run` raises its own codes - QC001..QC006 for a missing name, survey, position, datum, tops or
tubulars, LOAD for a load warning, and the equipment rules' own - with its own severities and wording. Nothing about the rules is
re-implemented here.

| What the page shows | Where it comes from |
|---|---|
| Findings per well with entity, field and rule | the engine's per-well rows intersected with its issue groups |
| Coverage | the engine's eight completeness flags, against the wells read |
| Mapping proposals | the setup engine's `Suggestions`, applied only by its `Accept` |
| Duplicate proposals | `WellReconciliation.Propose`, decided through `WellCrosswalk` |
| Thresholds | the engine's codes, `dataai.minimumMatchScore`, the matcher's 0.5, and the location tolerance |

**AC-5 needed an intersection rather than a new rule.** The engine keeps **a row per well with eight completeness flags** and the
**issues grouped by code**; `QcIssue` carries **no well and no field**. A finding is attributed by intersecting the two - the row names
the well and the missing item, the group supplies the code, severity and message - with the code-to-field table taken from the
engine's own `Add`. A code it only aggregates (LOAD, an equipment rule, a host validator) is shown at **field scope with "(not
attributed)"** rather than pinned on a well it did not name.

**AC-3 found a real gap.** `FieldQc.Run` calls `ThrowIfCancellationRequested` **before the next well and lets the exception out**, so a
cancelled run returns **no report at all** and every well already read is lost. The service counts the engine's own progress and then
**runs the same engine over that slice**, so the partial findings stay the engine's - at the cost of a second pass only when a run is
cancelled.

**AC-4 is a decision, not a message.** The pair is proposed with its evidence and conflicts and marked **"proposed only"**; a
rejection is written through the tools' `WellCrosswalk` **beside the environment**, after which the engine no longer proposes it - and
confirming it afterwards is refused.

**AC-1 has one boundary.** The run creates **no file at all** (asserted), and an acceptance goes through **INT-05's setup command**, so
an acceptance the engine does not offer is refused with nothing written.

**The open question is answered on screen.** The page lists the thresholds the run used: QC001..QC006 are raised whenever an item is
missing and have nothing to tune, the duplicate floor is `dataai.minimumMatchScore`, the proposal floor is the matcher's 0.5, and
location is not weighed as evidence unless `dataai.locationTolerance` is configured.

**Not built, and stated as gaps:** per-well attribution for LOAD and equipment codes (the engine does not say which well); a coverage
fraction for an arbitrary model field (the eight engine items are counted); a lower proposal floor for "show all" (the page lists the
unresolved values instead); reversing a rejected duplicate; and exporting the coverage or duplicate panels.

## 9. Open questions and risks

- `FieldQc` thresholds come from options; surface them so a user can see why a finding exists.
