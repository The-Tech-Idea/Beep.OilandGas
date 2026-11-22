# ENG-09 — Drilling performance

| Field | Value |
|---|---|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - see [evidence](../../evidence/STG-16-ENG-09/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Drilling` |
| Desktop project | `Modules/Engineering/DrillingPerformance/` |
| External packages | `TheTechIdea.OilGas.Drilling` |
| Depends on | DEV-06 |
| Blocks | — |
| Web reference | `OilandGasTools/docs/customer/drilling.html` (performance) |

## 1. Purpose

Analyse drilling performance from daily report data: time-depth curve, non-productive time by
cause, and rate of penetration trends.

## 2. Source components

| Source | Reuse |
|---|---|
| `Drilling/DrillingPerformance.cs` | `FromCsv`, `Analyze(activities) → PerformanceResult` |
| `Drilling/TimeDepthView.cs` | time-depth chart |
| `Drilling/DrillingToolbox.cs` | IADC activity codes |
| `Beep.OilandGas.DrillingAndConstruction` daily reports | source data (DEV-06) |

## 3. Screens

`Engineering → Drilling performance`

- Source selector: a well's daily reports, or a CSV import.
- Time-depth chart via `TimeDepthView` inside the Skia host.
- NPT breakdown: cause, hours, percentage, cost impact.
- ROP analysis: by interval, by bit, by formation.
- Comparison: this well vs offset wells.

## 4. Data and contracts

`OgPerformanceResult { Days, Metres, NptHours, NptByCause, RopByInterval[] }`.

## 5. States

| State | Behaviour |
|---|---|
| No daily reports | empty state with a link to DEV-06 |
| CSV without recognised codes | rejected rows listed with line numbers |
| Partial day data | marked incomplete; totals say so |
| NPT without a cause | aggregated as "unclassified" |
| Failed read | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Analyse | `engineering.performance.read` | no |
| Import CSV | `engineering.performance.write` | yes |
| Export | `data.export` | no |

## 7. Acceptance Criteria

- AC-1 Time-depth curve matches the sum of the daily reports.
- AC-2 NPT totals equal the sum of causes plus unclassified.
- AC-3 Unclassified NPT is visible, not hidden.
- AC-4 CSV import rejects unrecognised codes with line numbers.
- AC-5 Offset comparison uses the same metric definitions.
- AC-6 Export matches the screen.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-09"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-16-ENG-09/observed-evidence.json).

**The open question is answered before the importer was built.** *"`FromCsv` expects a specific column set; document it in §4
before building the importer UI."* The layout is a constant the importer and the page both read — `start, end, code, depth_from,
depth_to, description, npt`, ISO dates, invariant numbers, one header line — and a unit test reads a CSV through the **owner's
parser** and asserts every column lands where the documentation says.

**The data was missing, not the maths.** DEV-06's daily report records a shift's **period, depths, cost and author** — and **no
operation code and no non-productive cause**, which are the two fields `DrillingPerformance.Analyze` is driven by. So
`DEVELOPMENT_REPORT_ACTIVITY` was added in the owning module, with the report as its parent; the existing table is untouched and
every DEV-06 flow keeps working.

| What the page shows | Recorded in |
|---|---|
| Report periods, depth at each end, cost | `DEVELOPMENT_DAILY_REPORT` (DEV-06) |
| Operation code, footage, non-productive flag and cause | the added `DEVELOPMENT_REPORT_ACTIVITY` |
| Time-depth curve | the owner's `DrillingPerformance.Analyze`, drawn by the owner's `TimeDepthView` |
| Non-productive time by cause | derived from the same lines, with an `unclassified` bucket |
| Rate of penetration per run | derived from the same lines |

**AC-1 is a comparison, not a claim.** The owner's curve is checked against the **deepest `DEPTH_END` the report headers record**;
when they differ, the page names **both figures and the difference** rather than showing one and hoping.

**AC-3 is a bucket with a name.** A non-productive line with no cause appears as **"unclassified"** with its own hours, share, line
count and cost — and a warning says it is not being spread across the cause that does have a name.

**AC-5 is structural.** Every offset row calls the **same `LoadAsync`** as the well itself, so the definitions of days, depth,
non-productive share and rate cannot drift; the gateway asserts the well's row **equals its own analysis to six decimals** while the
offset's numbers genuinely differ.

**AC-4 needed provenance the parser does not give.** `FromCsv` returns activities without the lines they came from, so the importer
applies **the owner's own acceptance test** to the raw lines and matches the accepted rows by code in order — and says **"line
unknown"** rather than a wrong number when it cannot.

**Recorded rather than papered over:** the **cost impact is an apportionment** of the recorded daily cost by hours, because **no
non-productive cost exists anywhere in the data**; and a **rate by bit or by formation is not offered**, because a report line
records neither.

**A failing suite was fixed, not explained away.** Two tests pinned their as-of date to 2026-11-08 while the code reads the clock,
so they passed the day they were written and failed the next morning; both now take their date from the clock and the gateway's SQL
date filter is built from it.

**Not built, and stated as gaps:** a rate by bit or by formation (no such field exists); a fixed depth band (a number this build
would have chosen); a recorded non-productive cost; a dry-run preview of an import; a saved offset set; the chart's own zoom, cursor
readout or export (it is the owner's view); and paging.

## 9. Open questions and risks

- `DrillingPerformance.FromCsv` expects a specific column set; document it in §4 before building the
  importer UI.
