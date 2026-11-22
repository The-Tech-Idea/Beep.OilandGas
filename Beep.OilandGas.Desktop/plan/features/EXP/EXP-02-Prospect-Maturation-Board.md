# EXP-02 — Prospect maturation board

| Field | Value |
|---|---|
| Stage | STG-03 — Exploration and seismic |
| Area | Exploration |
| Status | Gate met - AC-1..AC-6: columns follow the recorded stage sequence, an unclassified prospect keeps its own last column, an incomplete risk reads "incomplete" with no chance, advancing is refused with a stated reason and writes nothing, the drop path runs the same command, and a failed read keeps the board with the failure stated. See [evidence](../../evidence/STG-03-EXP-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProspectIdentification` |
| Desktop project | `Modules/Exploration/ProspectBoard/` |
| External packages | none |
| Depends on | EXP-01 |
| Blocks | EXP-03 |
| Web reference | `Pages/PPDM39/Exploration/ProspectBoard.razor`, `Components/Exploration/ProspectCard.razor` |

## 1. Purpose

See the whole prospect portfolio as a stage board, and move work forward only through a recorded
process.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProspectIdentification/Services/ProspectIdentificationService.cs`, `.Maturation` partial | stage read |
| `ProspectIdentification/Services/IProspectTechnicalMaturationService` | maturation checks |
| `ProspectIdentification/Core/ReferenceCodes` (`PROSPECT_WORKFLOW_STAGE`) | stage codes |
| `LifeCycle` approval engine | stage advance (WFL-04) |

## 3. Screens

`Exploration → Prospect board`

- Columns: one per stage code, in configured order, with a count and an aggregate risk.
- Cards: prospect name, play, lead, area, chance range (from the risk assessment), owner, age.
- Card menu: open (→ EXP-03), advance stage (→ gate), view history.
- Filter bar: play, owner, basin, minimum chance, stage age.
- Board/list toggle.

## 4. Data and contracts

`OgProspectCard { Id, Name, Play, Stage, ChancePercent?, Owner, StageEnteredAt, RiskComplete }`.

## 5. States

| State | Behaviour |
|---|---|
| Stage not in the reference set | card is placed in an "unclassified" column and flagged |
| Risk incomplete | chance shows "incomplete", never a number |
| Advance unavailable | the menu item is disabled with the reason |
| Failed read | failed state preserving the last good board with its timestamp |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View board | `exploration.read` | no |
| Open prospect | `exploration.read` | no |
| Advance stage | `exploration.mature` + gate approval | yes (through the process engine) |

## 7. Acceptance criteria

- AC-1 Columns follow the reference-set order, not an alphabetical guess.
- AC-2 An unclassified stage is visible, not dropped.
- AC-3 Incomplete risk shows "incomplete".
- AC-4 Advancing is refused until the process engine is wired (STG-14), with a stated reason.
- AC-5 Drag-and-drop (if implemented) performs the same check as the menu command.
- AC-6 A failed read does not clear the board silently.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~EXP-02"
```

## 9. Open questions and risks

- The Web removed a board that advanced stages via a generic update. The desktop must not repeat
  that; if STG-14 is late, the board is read-only for stage changes.
