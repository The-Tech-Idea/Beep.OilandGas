# WTA-02 — Well test register

| Field | Value |
|---|---|
| Stage | STG-08 — Well test and pipeline |
| Area | Well test |
| Status | Gate met - see [evidence](../../evidence/STG-08-WTA-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/WellTest/Register/` |
| External packages | none |
| Depends on | WTA-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Production/WellTests.razor`, `Pages/PPDM39/Calculations/WellTestAnalysis.razor` (test selection) |

## 1. Purpose

The record of every well test: when, how, by whom, with what result, and which analysis was made
from it.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle/Services/WellManagement/WellManagementService.cs` | test reads |
| `PPDM.Models/39/WELL_TEST`, `WELL_TEST_FLOW_MEAS`, `R_WELL_TEST_TYPE`, `R_TEST_EQUIPMENT`, `R_TEST_RESULT` | entities |
| `PPDM39.DataManagement` module `WELL_REFERENCES` | seed |
| `WellTestAnalysis` analysis results | linkage |

## 3. Screens

`Well test → Register`

- Grid: well, date, test type, duration, method, result code, analysed (yes/no), analyst.
- Detail: conditions, rates, pressures over time, equipment, remarks, attachments.
- Analysis panel: linked analyses with their results, and an "analyse this test" action.
- Import: manual entry or CSV (DAT-06 with a well-test profile).

## 4. Data and contracts

`OgWellTestSummary { Id, WellId, TestDate, TestType, Duration, ResultCode, HasAnalysis,
AnalysisCount }`.

## 5. States

| State | Behaviour |
|---|---|
| No result code | "pending result", distinct from a failed test |
| Test without analysis | flag with a link to WTA-01 |
| Duplicate test (well + date + type) | refused |
| Failed read | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Create/edit test | `welltest.register.write` | yes |
| Analyse | `welltest.read` | no |
| Delete | `welltest.register.delete` | yes |

## 7. Acceptance criteria

- AC-1 A test links to exactly one well and may link to several analyses.
- AC-2 Test type and result codes come from reference tables.
- AC-3 A duplicate well+date+type is refused.
- AC-4 "Pending result" is distinct from "failed".
- AC-5 Rates and pressures carry units.
- AC-6 Deleting a test with analyses is refused.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WTA-02"
```

## 9. Open questions and risks

- `R_TEST_RESULT` seed content may vary; read it rather than assuming a code list.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-08-WTA-02/observed-evidence.json). Entries are read and written
through `UnitofWork<T>` over the new LifeCycle-owned `WELL_TEST_REGISTER`.

**The open question above, answered by reading the table.** The type list comes from `R_WELL_TEST_TYPE` and the result list
from `R_TEST_RESULT`, active rows only, on every load. An unknown code is refused naming the table, and an empty table
refuses registration with *"seed the reference table first"* rather than accepting any string - so the screen never assumes
a code list.

**Why a register table rather than writing `WELL_TEST`.** PPDM's `WELL_TEST` owns the test and already carries the date,
type, duration and result code, but it has **no surrogate key** for a unit of work to address (its identity is the
four-part natural key) and no place for the actor or the unit-bearing conditions this screen records. The register row
therefore holds what the app adds and points at the test by `UWI|TEST_TYPE|RUN_NUM|TEST_NUM`. It was added to
`LifeCycleModule`'s entity list, which already owns this domain, rather than creating another module.

**The link (AC-1, SG-08-4).** One test, one well - the key begins with the well. Many analyses - each analysis stores the
test key in its own `TEST_ID`, so the count on the register is read from the analyses rather than kept twice.

**A note on what the register does not see.** A test that exists only in PPDM's `WELL_TEST` (imported, never registered
here) does not appear on this screen, and an analysis whose `TEST_ID` predates this link is not counted. The screen says
"No well test is registered yet." rather than showing a partial list, and the limit is recorded rather than hidden.

**Not built:** the CSV import profile (DAT-06) and attachments. The register is entered on the page.
