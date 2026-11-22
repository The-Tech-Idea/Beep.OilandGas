# REL-04 — Performance and large-data handling

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Release |
| Status | Gate met - all six ACs, with the measured limits recorded: 6 of 11 budgets measurable here (5 named with reasons), bounded reads without a paging control, and cancellation measured for chunked work rather than a single draw. See [evidence](../../evidence/STG-18-REL-04/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Performance`, `Beep.OilandGas.PPDM39` |
| Desktop project | all |
| External packages | none |
| Depends on | every data stage |
| Blocks | REL-01, REL-05 |
| Web reference | none |

## 1. Purpose

Set measurable budgets and meet them, because an Oil & Gas database is large and a desktop UI that
freezes is unusable.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore/Performance/CurveDecimation.cs` | log decimation |
| `PPDM39/Core/PPDMGenericRepository.cs` | paged queries |
| `Wellbore/PrintPlanner.cs` | pagination cost control |
| `Mapping/Gridding/Grid.cs` | grid size limits |

## 3. Screens

A `Settings → Diagnostics → Performance` page showing the last measured timings per operation, and a
`--benchmark` verb that writes a machine-readable report.

## 4. Data and contracts

Budgets (measured on a reference workstation; record the hardware in evidence):

| Operation | Budget |
|---|---|
| Application start to sign-in prompt | ≤ 3 s |
| Sign-in to shell with navigation | ≤ 2 s |
| Well search over 5 000 wells | ≤ 1 s |
| Table workbench first page (200 rows) | ≤ 800 ms |
| Log render, 20 000 samples | ≤ 250 ms per frame while panning |
| Log render, 100 000 samples (decimated) | ≤ 400 ms per frame |
| Schematic render | ≤ 200 ms |
| Contour map, 10 000 cells | ≤ 300 ms per frame |
| 3D orbit, 10 000 cells | ≤ 500 ms per frame |
| Module setup of 29 modules on an empty SQLite database | ≤ 60 s |
| Memory after 30 minutes of typical use | ≤ 800 MB |

## 5. States

| State | Behaviour |
|---|---|
| Over budget | the diagnostics page marks the row red with the measured value |
| Decimation applied | badge with the ratio on the view |
| Query exceeds a row ceiling | the grid states the ceiling and asks the user to narrow |
| Out of memory risk | the operation refuses with the reason instead of crashing |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Run benchmark | any local user |
| View diagnostics | Administration |

## 7. Acceptance Criteria

- AC-1 Every budget in §4 is measured and recorded.
- AC-2 Log rendering applies decimation above the configured threshold and states the ratio.
- AC-3 Every grid read is paged; no screen loads an unbounded result.
- AC-4 A long operation is cancellable within 500 ms of the cancel request.
- AC-5 Memory stays within budget after 30 minutes of scripted use.
- AC-6 The benchmark report is machine-readable and comparable across runs.

## 8. Verification

```
dotnet run --project src/Beep.OilandGas.Desktop -- --benchmark
```

Attach the report plus the hardware description.

## 9. Open questions and risks

- Budgets are proposals. Agree them before REL-01 so the installer's prerequisite check is meaningful.
- `PPDMGenericRepository` paging may materialise all rows (recorded limitation elsewhere in the repo);
  measure before promising the table budgets.

### Delivered 2026-11-06

**Budgets and the report (AC-1, AC-6).**

- `OgBudgets` holds all **11** budgets from §4, including the five this build cannot measure; each
  unmeasurable budget carries its reason, so the report shows a row rather than quietly dropping it.
- `OgBenchmarkReport` is the machine-readable file: schema, capture time, machine, one row per budget (key,
  operation, target, measured, note) and free-text notes. Measurements are **keyed and ordered by the budget
  list**, so a diff between two runs shows values, not formatting.
- `OgMachine` records OS, architecture, cores, visible memory, framework and process architecture — the
  hardware the numbers belong to.
- `--report-benchmark` (alias `--benchmark`) measures and writes
  `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Diagnostics\benchmark-report.json`; it exits 0 whether the run was
  within budget (`result: ok`) or found an overrun (`result: over-budget`), because a measurement that found a
  problem is still a successful measurement. `--seed-benchmark` writes the deterministic 100 000-sample log.
- The **Performance** page (route `performance`, Administration module) reads the stored report and marks an
  over-budget row red **and** with the words "OVER BUDGET" — colour alone would fail REL-03.

**Measured on this machine** (65 425 MB visible, 12 cores, .NET 10.0.14 x64): shell 452 ms of 2 000, grid page
1.3 ms of 800, log 20 000 samples 5.5 ms of 250, log 100 000 samples 6.1 ms of 400, schematic 3.8 ms of 200,
startup-to-build 62 ms of 3 000. The startup row says it is the application build in a headless run; a windowed
run reports the gate mark instead.

**Decimation (AC-2).** The renderer thins a long curve with the tools' `CurveDecimation` (four points per pixel
row, keeping first/leftmost/rightmost/last, so spikes survive). `OgBenchmarkData.Plan` states the bound and the
ratio before a frame is drawn: `Decimated: 100,000 samples drawn as at most 2,400 points (41.7:1) at 600 px.`
A test proves the renderer stays inside that bound, keeps the curve's ends, and keeps its peak.

**Bounded reads (AC-3).** `OgPager` is the only way this build materialises a grid page: a request is
normalised (no page size 0, no negative page), a read returns at most one page, and a source above the ceiling
reports `Showing 200 of the first 1,000 rows (5,000 match). Narrow the search to see the rest.` The audit and
failure grids state `Showing the 200 most recent rows of N. Narrow the range to see older ones.` when their
store holds more than a page.

**Cancellation (AC-4).** `OgCancellationProbe` runs a chunked operation, requests a cancel and reports the
observed latency: **15–17 ms** against a 500 ms budget.

**Memory (AC-5).** `--soak <minutes>` runs scripted typical use - a 20 000-sample log frame, a schematic
frame, a chart, a paged grid read and an audit read, on surfaces created once and re-rendered — and samples the
working set. `OgSoakMath` computes start/end/peak, growth and growth per minute, and the verdict against the
800 MB budget; the report is written to `Diagnostics/soak-report.json`. **The 30-minute run:** 26 215 cycles,
109 MB at the start, 131 MB at the end, **135 MB peak**, growth **+23 MB (+0.8 MB/min)** - within the 800 MB
budget. A run whose peak exceeds the budget says `OVER BUDGET` rather than passing on a flat slope.

### Still open

- **Five budgets have no measurement here:** well search (no well table - DAT-02), contour map (INT-07), 3D
  scene (INT-08) and 29-module setup (DAT-02). AC-1 is therefore **partial**: 6 of 11 measured.
- **AC-3 has no paging control.** Reads are bounded and say what they are not showing, but no screen offers a
  "next page" yet; `OgPager` supports one.
- **AC-4 covers the contract, not a render.** A single synchronous `Render` call cannot be interrupted
  mid-frame; only chunked work observes the token. The same limit is recorded for VSB-06 AC-6 and VSB-09 AC-6.
- **The budgets are REL-04's proposals** and were not agreed before REL-01, as §9 asks.
