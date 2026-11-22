# DEC-02 — Well plug and abandonment

| Field | Value |
|---|---|
| Stage | STG-12 — Decommissioning |
| Area | Decommissioning |
| Status | Gate met - see [evidence](../../evidence/STG-12-DEC-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Decommissioning` |
| Desktop project | `Modules/Decommissioning/WellPa/` |
| External packages | `TheTechIdea.OilGas.WellSchematic` (`Abandonment`, `BarrierTemplates.PermanentAbandonment`), `TheTechIdea.OilGas.Wellbore.Data` (`BarrierValidator`, `LeakPathAnalysis`) |
| Depends on | DEC-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Decommissioning/WellPAWorkflow.razor`, `WellAbandonment.razor` |

## 1. Purpose

Plan and record a well's permanent abandonment: barriers, plugs, verification evidence, execution
and final status.

## 2. Source components

| Source | Reuse |
|---|---|
| `Decommissioning` contracts `WellAbandonment*` | data shape |
| `WellSchematic/Abandonment.cs` | abandonment track rendering |
| `WellSchematic/BarrierTemplates.cs` `PermanentAbandonment` | required barrier set |
| `Wellbore.Data/BarrierValidator.cs`, `BarrierEvidence.cs`, `LeakPathAnalysis.cs` | validation and leak paths |
| Tables `ABANDONMENT_STATUS`, barrier evidence records | persistence |

## 3. Screens

`Decommissioning → Well P&A`

- Programme: well, objective, planned barriers with type and depth, plugs, verification method.
- Barrier panel: required vs planned vs verified, with evidence reference per barrier.
- Schematic preview: abandonment track with plugs and barriers.
- Leak path panel: identified paths and their status.
- Execution record: actual barriers, dates, contractor, verification.
- Final status panel.

## 4. Data and Contracts

`OgAbandonmentBarrier { Kind, TopDepth, BaseDepth, DepthUnit, Verification, EvidenceRef, Verified }`.

## 5. States

| State | Behaviour |
|---|---|
| Barrier missing evidence | marked unverified; completion refused |
| Required barrier absent | completion refused with the barrier named |
| Leak path unaddressed | completion refused |
| Schematic unavailable | text panel still works; state the reason |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Plan programme | `decommissioning.wellpa.write` | yes |
| Record execution | `decommissioning.wellpa.write` | yes |
| Complete and close | `decommissioning.wellpa.close` + SoD | yes |

## 7. Acceptance Criteria

- AC-1 Completion is refused without evidence for every required barrier.
- AC-2 Required barriers come from `BarrierTemplates.PermanentAbandonment`, not the view.
- AC-3 An unaddressed leak path blocks completion.
- AC-4 Depths carry units.
- AC-5 The schematic is a visual aid, not the evidence; say so.
- AC-6 Closure records actor, date and verification.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DEC-02"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-12-DEC-02/observed-evidence.json). The owner held no barrier or leak-path
record, so both are the plan's own contracts in the module that owns decommissioning, and the required barrier set is the
owner's permanent-abandonment template.

**Why two tables were added.** `ABANDONMENT_STATUS` carried the state and the plug-back depth, and a barrier set existed only
in memory, derived from a schematic. A barrier that cannot be recorded cannot be verified - and AC-1 is precisely that
completion is refused without evidence. So `ABANDONMENT_BARRIER` (envelope, type, top/base depth, depth unit, verification
method, evidence reference, verified by/date/flag) and `ABANDONMENT_LEAK_PATH` (description, status, how/by/date addressed) are
declared by `DecommissioningModule`, taking its planned schema from three tables to five (smoke 432 -> 434 entity types).

**AC-2 is the owner's template, not a list in the view.** `BarrierTemplates.PermanentAbandonment` takes a
`WellSchematicDocument` and derives the set from its cement plugs, so it cannot be called without a schematic. The template it
builds from is embedded and readable: `JsonBarrierTemplates.BuiltIn.Templates["permanentAbandonment"]` names the deep envelopes
(`Primary`, `Secondary`) and the surface envelope (`Environmental`) and the per-plug element types (`CEMENT_PLUG`,
`CASING_CEMENT`, `IN_SITU_FORMATION`). The desktop reads that definition. An element the template guards with a `when` condition
depends on a schematic (a host casing, a formation), so it is marked **conditional** rather than required outright - and each
refusal names what the template requires in the empty envelope.

| Requirement | Recorded in |
|---|---|
| Barrier, envelope, type, interval, unit, verification method | `ABANDONMENT_BARRIER` |
| Evidence reference, verifier, date, verified flag | `ABANDONMENT_BARRIER` |
| Leak path, its state and how it was addressed | `ABANDONMENT_LEAK_PATH` |
| Planned date and planner | `ABANDONMENT_STATUS.PLANNED_DATE` / `PLANNED_BY` (added) |
| Programme state and history | `ABANDONMENT_STATUS` rows, latest current |

**The planned date closes DEC-01's recorded gap.** DEC-01 recorded that neither status table carried a planned date, so its
"planned year" and "overdue" had no source. `ABANDONMENT_STATUS` now has `PLANNED_DATE` and `PLANNED_BY` additively, the plan
records them, and DEC-01 reads a well's planned year from that date. The facility side still records none, which DEC-03 owns.

**Completion refuses with every reason at once.** An envelope without a verified-and-evidenced barrier; a barrier marked
executed with no evidence reference; no barriers at all; an open leak path; a programme already closed. A closed programme's
checklist states the outcome - the gateway caught a first version where a finished abandonment displayed a cross against its
own "already completed" block.

**Closure records the actor, the date and the verification,** and the planner and anyone who recorded barrier evidence are
refused as the closer; the completed row is what DEC-01 reads as the well's state, so the two screens cannot disagree.

**Not built, and stated as gaps:** jurisdiction-specific barrier rules (the stage says configuration, and this build has no
barrier-rule configuration, so the template is the only source); proving a conditional element without a schematic; a schematic
preview (VSB owns the renderer, so the page states what the schematic is instead of drawing it); more than one programme per
well; a single transaction across the status row and its barriers; an evidence file store (evidence is a reference string); and
a leak-path analysis engine.

## 9. Open questions and risks

- Jurisdiction-specific barrier rules: use configuration, not desktop code.
