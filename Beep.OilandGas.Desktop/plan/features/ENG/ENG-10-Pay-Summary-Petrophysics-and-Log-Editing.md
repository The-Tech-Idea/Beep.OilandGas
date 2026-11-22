# ENG-10 — Pay summary, petrophysics and log editing

| Field | Value |
|---|---|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - reference interval reproduced, zone refusals, missing-curve exclusion, audited shift/splice with undo, CSV and XLSX exports. See [evidence](../../evidence/STG-16-ENG-10/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Data`, `TheTechIdea.OilGas.WellLogs` |
| Desktop project | `Modules/Engineering/PaySummary/` |
| External packages | `Wellbore.Data`, `WellLogs`, `Wellbore.Loaders` (`PaySummaryExport`) |
| Depends on | VSB-04 |
| Blocks | RES-05 |
| Web reference | `OilandGasTools/docs/customer/pay-summary-and-log-editing.html` |

## 1. Purpose

Compute pay from logs — Vsh, porosity, water saturation, net, net-to-gross — and edit the logs
(depth shift, splice) with an audit trail before the calculation.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Data/Model/PaySummary.cs` | `Compute(well, logSet, warnings, settings)`, `ToCsv` |
| `Wellbore.Data/Model/Petrophysics.cs`, `PetrophysicsCalculator` | Vsh, porosity, Archie Sw |
| `Wellbore.Data/Model/LogEditing.cs` | depth shift, splice, audit, undo |
| `Wellbore.Data/Model/LogSet.cs`, `CurveEdits.cs` | curves and edits |
| `Wellbore.Loaders/Tabular/PaySummaryExport.cs` | `ToXlsx` |
| `WellLogs/Analysis/CurveEditing.cs` | UI-side editing helpers |
| `WellLogs/Tracks/PaySummaryTrack.cs` | pay track overlay |

## 3. Screens

`Engineering → Pay summary`

- Zone editor: zone name, top, base, cutoffs (Vsh, porosity, Sw).
- Parameter set: Archie a, m, n, Rw; Vsh method; porosity method.
- Results: per zone — gross, net, N/G, average porosity, average Sw, hydrocarbon pore volume.
- Log overlay: the pay track drawn on the log view.
- Edit tab: depth shift and splice with preview, undo/redo, history.
- Export: XLSX and CSV.

## 4. Data and contracts

`OgPayZone { Name, Top, Base, Cutoffs, Gross, Net, Ntg, PhiAvg, SwAvg, Hcpv }`
Editing produces `TrackEditHistory` entries.

## 5. States

| State | Behaviour |
|---|---|
| Missing curve for a method | zone flagged and excluded, with the curve named |
| Cutoff not set | calculation refused with the reason |
| Overlapping zones | refused |
| Shift beyond the log range | refused with the range |
| Splice without a source interval | refused |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Compute | `engineering.paysummary.read` | no |
| Edit log | `wells.logs.edit` | yes |
| Save summary | `engineering.paysummary.write` | yes |

## 7. Acceptance Criteria

- AC-1 A reference interval reproduces the expected net and N/G within tolerance.
- AC-2 A missing curve excludes the zone and names it.
- AC-3 Overlapping zones are refused.
- AC-4 A depth shift is undoable and audited.
- AC-5 A splice records both intervals and the actor.
- AC-6 XLSX export matches the result grid.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-10"
```

Port assertions from `OilandGasTools/tests/TheTechIdea.OilGas.Tests/PaySummaryAndLogEditingTests.cs`
and `PetrophysicsTests.cs`.

## 9. Open questions and risks

- Editing before computing changes the answer: require an explicit "recompute after edit" step and
  show the edit count in the result header.

### Delivered 2026-11-06

- **Route `paysummary`** (en + ar), under the Wells module with the engineering roles (SystemAdmin,
  TenantAdmin, ReservoirEngineer, DrillingEngineer, ProductionEngineer): cutoffs and Vsh method, an editable
  zone grid, a results grid, an edits tab with shift/splice/undo and the audit history, and CSV/XLSX export.
- `OgPaySummaryWorkspace` wraps the tools: `Compute`, `ToCsv`, `ExportCsv`, `ExportXlsx`, the reference well and
  zones, and the refusals the screen must make. The arithmetic is `PaySummary.Compute`; the class adds the
  checks a desktop needs.
- **AC-1** the library's reference interval reproduces exactly: zone A gross 5, net reservoir 3, net pay 2,
  N/G 0.6, porosity 0.1875, Sw 0.1475/0.375, Vsh 0.25, hydrocarbon column 0.2275; zone B gross 4, net 0.5.
  The suite and the smoke run both assert it (overall net 3.5, net pay 2.5, N/G 0.389).
- **AC-2** a missing input excludes the zone, zeroes its net figures and **names the curve**:
  `no curve for GR (or VSH)`. The run still succeeds and says every zone was excluded.
- **AC-3** zones are validated before the arithmetic: overlap is refused naming both zones, a gap is refused as
  "not contiguous", the last zone must end at the log's bottom, and a cut-off that is not set is refused
  naming the cut-off. Each refusal carries a code (`pay-zones-overlap`, `pay-zones-not-contiguous`,
  `pay-cutoffs-missing`).
- **AC-4** a depth shift goes through `LogEditSession`: the model's audit records the author, the field
  (`depthShift`) and the reason, the desktop audit gets a `logs.edit` row, and undo restores the set.
- **AC-5** a splice records both run names and the splice depths in the audit entry with the author, and undo
  removes the composite.
- **AC-6** the XLSX export is the tools' writer, so numbers stay numbers; the test reads it back through the
  tools' own `ExcelTabularSource` and compares the rows and the net reservoir value. An existing file is
  refused unless overwrite is asked for.
- **An edit marks the result stale.** The page says "recompute to use the edited log" and the summary shows the
  edit count, which is §9's requirement rather than a silent recomputation.

### Still open

- **Zones are contiguous.** `PaySummary` derives a zone from its top to the next top, so the editor requires each
  zone to end where the next begins and the last to end at the log's bottom; a non-contiguous set is refused
  rather than silently reporting a different gross.
- **The page computes the reference well**, not a well chosen from the database: there is no well picker until
  DAT-02, and results are not persisted.
- **The pay track overlay is not drawn** on a log view; the numbers and the grid are the deliverable, and the
  tools' `PaySummaryTrack` is available for a later VSB integration.
- **The splice uses a synthetic second run** on the page; two real runs need loaded log sets (DAT-02/INT-01
  persistence).
- Editing is available to anyone who can open the page; the per-command permissions in §6
  (`engineering.paysummary.write`, `wells.logs.edit`) are not yet separate role checks.
