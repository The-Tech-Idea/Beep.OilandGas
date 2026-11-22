# HSE-04 — HAZOP studies

| Field | Value |
|---|---|
| Stage | STG-11 — HSE, compliance and permits |
| Area | HSE |
| Status | Gate met - see [evidence](../../evidence/STG-11-HSE-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `Modules/Hse/Hazop/` |
| External packages | none |
| Depends on | HSE-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/HSE/HAZOPStudy.razor`, `Components/BusinessProcess/StateTransitionPanel.razor` |

## 1. Purpose

Run a HAZOP study: nodes, deviations, causes, consequences, safeguards and actions — with the study
identity verified and every action traceable.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/HSE/HAZOPService.cs` | study, node, deviation, action |
| `Components/BusinessProcess/StateTransitionPanel.razor` | transition presentation |
| HAZOP tables | persistence |

## 3. Screens

`HSE → HAZOP`

- Study list opened by explicit recorded id (not "latest").
- Study header: facility, node count, status, team, dates.
- Node list: node, design intent, deviations.
- Deviation editor: guide word + parameter, cause, consequence, safeguard, risk rating, action.
- Action list with owner and due date.
- Transition panel with legal transitions only.

## 4. Data and contracts

`OgHazopStudy { Id, FacilityId, State, Nodes[] }`
`OgHazopDeviation { Node, GuideWord, Parameter, Cause, Consequence, Safeguards[], Risk, ActionId? }`.

## 5. States

| State | Behaviour |
|---|---|
| Study id not found | refused; never falls back to another study |
| Duplicate node keys | refused |
| Field changed | study cleared, late response discarded |
| Action without a node/deviation | refused |
| Transition not legal | refused with the legal set |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `hse.hazop.read` | no |
| Edit study | `hse.hazop.write` | yes |
| Raise action | `hse.hazop.write` | yes |

## 7. Acceptance criteria

- AC-1 A study is opened by explicit identity only.
- AC-2 Missing or mismatched acknowledgements are reported as unconfirmed.
- AC-3 Duplicate node keys are refused.
- AC-4 An action requires a node and a deviation.
- AC-5 A field change clears the study and discards late responses.
- AC-6 Only legal transitions are offered.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~HSE-04"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-11-HSE-04/observed-evidence.json). Nothing new was created: the owner
holds the study in `PROJECT` (type HAZOP) with its field in `PROJECT_COMPONENT`, its nodes in `PROJECT_STEP` (HAZOP_NODE), its
deviations in `PROJECT_CONDITION`, its states in `PROJECT_STATUS`, and its actions as `PROJECT_STEP` (HAZOP_ACTION) linked
by `PROJECT_STEP_XREF` with a responsible person in `PROJECT_STEP_BA`. `PROJECT_COMPONENT` and `PROJECT_STEP_XREF` were added
to `HseModule` (smoke 427 -> 429).

**Opened by identity, never by "latest".** The Web had a defect where the newest study was opened instead of the recorded
one; the desktop requires the id and refuses a missing study with a sentence that no other is opened in its place.

**Confirmation before legality.** Every transition carries the state the reviewer saw. A missing or different
acknowledgement is reported as *unconfirmed* - with both states named - before the transition is even judged, and nothing is
written. That is the difference between "your screen was stale" and "that move is not allowed", and the two are kept apart.

**The payload can hold only the owner's two fields.** The owner's HAZOP reader refuses a condition `REMARK` whose object does
not have exactly `Deviation` and `Consequence`. The desktop's extra fields - parameter, cause, safeguard and risk - therefore
live in a `PROJECT_COMPONENT` row linked by `CONDITION_ID`, the owner's own condition link, as `key=value` lines. The gateway
parses the stored payload to prove it still has two properties.

**An action always has a node and a deviation.** Both are checked against the recorded rows before anything is written
(AC-4, SG-11-5); the action is a step linked to its node by an xref, with the deviation number on the step and the
responsible person in a `PROJECT_STEP_BA` row carrying the owner's RESPONSIBLE role.

**A finding that also corrected HSE-03.** An update through the unit of work needs the table's key in the provider structure.
A hand-built test schema without the composite key made `Update` answer *"no primary key"* while `Commit` still returned Ok -
so HSE-03's earlier step-date write was silently skipped. The PPDM metadata (`PPDM39Metadata.Generated.cs`) declares the
canonical keys for the schema the app provisions; HSE-04's gateway now declares the same keys, every HSE-04 status write is
checked and surfaced, and HSE-03's redundant write was removed (the COMPLETED status row is the authoritative completion).
HSE-03's evidence and notes were corrected.

**Not built, and stated as gaps:** the study team (the owner records none on a HAZOP study); a risk rating the owner can read
(HIGH/MEDIUM/LOW and the guide-word list are the desktop's vocabulary, stored in its linked component); completing and
verifying an action from this screen (HSE-03 owns that); a revision token beyond the acknowledgement (two people applying the
same transition at the same instant are not serialised); and a single transaction across the seven rows a study command can
touch.

## 9. Open questions and risks

- The Web opens studies by recorded id after a defect where "latest" was used. Preserve that rule.
