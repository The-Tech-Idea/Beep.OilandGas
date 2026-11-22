# DAT-07 — Data quality review

| Field | Value |
|---|---|
| Stage | STG-01 — Data platform and PPDM core |
| Area | Data |
| Status | Gate met - AC-1..AC-5: findings carry table/key/column/rule, a table with no rules is distinguished from one with no findings, the last successful run is kept in the durable SQLite journal, resolving records the actor, and severity order is the enum numbers and stable. See [evidence](../../evidence/STG-01-DAT-07/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `Modules/Data/DataQuality/` |
| External packages | none |
| Depends on | DAT-06 |
| Blocks | — |
| Web reference | `Pages/PPDM39/DataQuality.razor`, `Pages/Data/Validation.razor` |

## 1. Purpose

Find and fix the data defects that break every downstream calculation: missing mandatory values,
orphan keys, duplicate identities, out-of-range numbers, unit mismatches.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/PPDMDataQualityService.cs` | rule execution |
| `PPDM39.DataManagement/Services/PPDMDataValidationService.cs` | per-record validation |
| `PPDM39/Core/PPDM39TableRegistry.cs` | table scope |
| `TheTechIdea.OilGas.Wellbore.Loaders/FieldQc.cs` | field-wide well QC (INT-06) |

## 3. Screens

`Data → Quality`

- Rule catalogue grouped by table, with severity.
- Run panel: scope (all / table / field / well), rule selection, run (cancellable), last run time.
- Findings grid: severity, table, key, column, rule, message, first seen, occurrences.
- Detail pane: the offending record inline, with a "fix" action that opens DAT-05.
- Trend strip: finding count over the last ten runs.

## 4. Data and contracts

`OgQualityFinding { Severity, Table, Key, Column, RuleId, Message, Occurrences, FirstSeen,
LastSeen, IsResolved }`.

## 5. States

| State | Behaviour |
|---|---|
| No rules for a table | stated explicitly, not "no problems" |
| Run failed | failed state; previous findings remain visible with their timestamp |
| Findings stale | banner shows the last-run time |
| Fix applied | finding moves to resolved with the resolving actor |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run rules | `data.quality.run` | no |
| Fix a record | `data.write` | yes |
| Mark resolved | `data.quality.manage` | yes |

## 7. Acceptance criteria

- AC-1 A finding carries table, key, column and rule.
- AC-2 A table with no rules is distinguished from a table with no findings.
- AC-3 Findings retain their last-run timestamp when a run fails.
- AC-4 Resolving a finding records the actor.
- AC-5 Severity ordering is stable and documented.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.IntegrationTests --filter "FullyQualifiedName~DAT-07"
```

## 9. Open questions and risks

- Rule definitions may live in the service rather than a table; if so the catalogue is read-only
  and configuration is a code change. Record which.
