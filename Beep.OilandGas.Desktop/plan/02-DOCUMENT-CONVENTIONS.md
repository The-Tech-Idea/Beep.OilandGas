# Document Conventions — Beep.OilandGas.Desktop

**Document ID:** PLAN-02
**Version:** 1.0
**Created:** 2026-11-05

This file defines how every other plan document is written, so the set stays consistent and
reviewable. It follows the conventions already used in `Beep.OilandGas/Plans` (no YAML
front matter, inline IDs, gate tables, evidence paired with commands).

---

## 1. Identifiers

| Kind | Format | Example |
|---|---|---|
| Stage | `STG-<NN>` | `STG-07` |
| Stage document file | `stages/STG-<NN>-<Title-In-Kebab-Case>.md` | `stages/STG-07-Well-Performance-and-Artificial-Lift.md` |
| Feature | `<AREA>-<NN>` | `LFT-03` |
| Feature document file | `features/<AREA>/<ID>-<Title-In-Kebab-Case>.md` | `features/LFT/LFT-03-ESP-and-Pump-Performance.md` |
| Decision | `ADR-<NNN>` | `ADR-001` |
| Gap | `G-<NN>` | `G-01` |
| Test case | `<FEATURE-ID>/T<NN>` | `LFT-03/T04` |
| Evidence folder | `evidence/<STG-or-FEATURE>-<slug>/` | `evidence/STG-00-smoke/` |

Areas: `FND`, `DAT`, `WEL`, `EXP`, `LSE`, `DEV`, `PRD`, `RES`, `LFT`, `WTA`, `ECO`, `ACC`,
`HSE`, `DEC`, `WRK`, `WFL`, `VSB`, `ENG`, `INT`, `TRN`, `RPT`, `REL`.

Numbers are never reused. A withdrawn item keeps its row and is marked `Deferred`.

---

## 2. Feature document template

Every file in `plan/features/**` has exactly these sections, in this order. Sections may be
short, but none may be removed.

```markdown
# <ID> — <Feature name>

| Field | Value |
|---|---|
| Stage | STG-<NN> — <stage title> |
| Area | <area name> |
| Status | Not started |
| Owner projects | <Beep.OilandGas projects that own the logic> |
| Desktop project | `src/Beep.OilandGas.Desktop.Modules/<Area>/` |
| External packages | <TheTechIdea.OilGas packages, or "none"> |
| Depends on | <feature IDs, or "—"> |
| Blocks | <feature IDs, or "—"> |
| Web reference | <razor page paths, or "none"> |

## 1. Purpose
One paragraph. What the user can do that they could not do before.

## 2. Source components
Table of exact paths and types that are consumed. No re-implementation is planned.

## 3. Screens
List of WPF screens with route, layout sketch, and the primary controls.

## 4. Data and contracts
The service interfaces called, the DTOs used, and the PPDM/extension tables touched.

## 5. States
What empty / loading / failed / denied / unsaved look like for this feature.

## 6. Commands and permissions
Each command, the role/permission it requires, and whether it writes.

## 7. Acceptance criteria
Numbered, testable statements. "AC-1 …". These become test cases.

## 8. Verification
Exact commands and the expected observable result.

## 9. Open questions and risks
Anything unresolved, with the owning decision or gap ID.
```

**Every feature inherits the pipeline rules in [`07-LOGGING-AUDIT-AND-NOTIFICATIONS.md`](07-LOGGING-AUDIT-AND-NOTIFICATIONS.md)
§7.** Do not restate them; do not contradict them. In particular: a failed state shows
`FailurePanel` with a reference, a failure reaches the user as a toast from the one pipeline, every
writing command and refusal writes one audit row, and no `catch` is allowed to swallow or only log.

---

## 3. Stage document template

Every file in `plan/stages/` has exactly these sections.

```markdown
# STG-<NN> — <Stage title>

| Field | Value |
|---|---|
| Stage | STG-<NN> |
| Features | <count> (<IDs>) |
| Depends on | <stages> |
| Status | Not started |
| Exit gate | <one-line gate> |

## 1. Objective
## 2. Scope (in / out)
## 3. Source projects consumed
## 4. Features
Table: ID | Feature | Doc link
## 5. New code introduced
Projects, folders, key types.
## 6. Architecture notes
Anything stage-specific that overrides or extends PLAN-01.
## 7. Dependencies and sequencing
## 8. Exit gate
Numbered, testable gate items `SG-<NN>-<n>`.
## 9. Test plan
Focused test list with IDs.
## 10. Risks and open questions
```

---

## 4. Writing rules

1. **No front matter.** Metadata lives in the table.
2. **No invented status.** A status is `Not started` until evidence exists.
3. **Every claim pairs with a check.** "Builds" is not evidence; the command and its result are.
4. **Paths are repo-relative** to `Beep.OilandGas` or `OilandGasTools` and are written with the
   repository prefix when ambiguous.
5. **No credentials, tokens or connection strings** in any document.
6. **No duplicated model definitions** in prose. Name the owning project and move on.
7. **One idea per acceptance criterion.** If it has an "and", it is two criteria.
8. **State the negative.** Say what the feature refuses to do (no display-name inference, no
   silent zero on read failure, no automatic resubmit).
9. **Dates inline** as `(YYYY-MM-DD)` or `**2026-11-05**`.
10. **Historical claims stay labelled.** If a number came from a Web test run, say so.

---

## 5. Definition of Done

A feature is `Gate met` only when all of the following are true.

| # | Condition |
|---|---|
| 1 | All acceptance criteria have a passing test each, or a recorded reason they are manual. |
| 2 | The five states are implemented and were seen at least once. |
| 3 | Route is in `OgModuleCatalog`; guard refuses without entitlement and without role. |
| 4 | Every string comes from `Loc`; RTL layout was checked. |
| 5 | Every interactive control has an accessible name and keyboard path. |
| 6 | No `BEEP0001..0004` diagnostic; no swallowed catch; no logger-only catch. |
| 7 | No hardcoded colour, font size or margin outside the theme resource files. |
| 8 | Every failure on this feature produced a toast with a reference and no exception text. |
| 9 | Every writing command and every refusal wrote exactly one audit row. |
| 10 | `IOgToastService` is not injected anywhere in this feature. |
| 11 | The feature document's `Status` and the master tracker row were both updated. |
| 12 | Evidence folder created with the command output. |

A stage is `Gate met` when every feature is `Gate met` **or** `Deferred` with a recorded
reason, and every `SG-<NN>-n` gate item passed.

---

## 6. Evidence format

```
plan/evidence/<STG-00|FEATURE-ID>-<slug>/
├─ REPRODUCE.md            exact commands, in order
├─ observed-evidence.json  machine-readable results
└─ captures/               screenshots, .trx, stdout.txt
```

`observed-evidence.json` minimum shape:

```json
{
  "id": "STG-00",
  "date": "2026-11-05",
  "commit": "<hash or 'uncommitted'>",
  "commands": [{ "cmd": "dotnet build ...", "exit": 0, "warnings": 0 }],
  "tests": { "run": 24, "passed": 24, "failed": 0, "skipped": 0 },
  "claims": ["smoke run writes result: ok"],
  "limits": ["no live provider", "no real browser"]
}
```

`limits` is mandatory. It records what the evidence does **not** prove.

---

## 7. Review checklist for a plan change

- Does the change touch a stage gate? Update the stage document and the tracker in the same edit.
- Does it add a feature? Add the row, the document, the stage doc row, and the module matrix row.
- Does it remove scope? Mark `Deferred` with a reason; never delete.
- Does it create a dependency not in `06-DEPENDENCY-GRAPH.md`? Update that graph.
- Does it contradict `01-ARCHITECTURE.md`? Raise an ADR instead of editing prose silently.
