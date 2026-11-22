# INT-01 — LAS and DLIS import/export

| Field | Value |
|---|---|
| Stage | STG-17 — Data interop, mapping and 3D |
| Area | Interop |
| Status | Gate met - LAS 2.0/3.0 load+export with the tools' checker, DLIS frames with array-channel warnings, decimation and encoding reported. See [evidence](../../evidence/STG-17-INT-01/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Loaders` |
| Desktop project | `Modules/Interop/LasDlis/` |
| External packages | `TheTechIdea.OilGas.Wellbore.Loaders` |
| Depends on | VSB-02 |
| Blocks | ENG-03, ENG-10, INT-04 |
| Web reference | `OilandGasTools/docs/customer/data.html`, `exports.html` |

## 1. Purpose

Load and save well log data in the formats the industry actually exchanges: LAS 2.0, LAS 3.0 and
DLIS, with warnings preserved instead of discarded.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Loaders/Las/LasReader.cs` | `Load(text, into, name)`, `LoadFile(path, into, options)`, `LoadFrom(reader, …)` |
| `Wellbore.Loaders/Las/LasWriter.cs` | `Write(well, logSet, options)` — LAS 2.0 / 3.0 |
| `Wellbore.Loaders/Las/Las3Writer.cs`, `Las3Checker.cs` | LAS 3.0 compliance |
| `Wellbore.Loaders/Dlis/DlisReader.cs`, `DlisFile.cs` | DLIS load |
| `Wellbore.Loaders/Las/LasTestSupport` equivalent tests | reference files |
| `Wellbore.Data/Model/LogSet.cs` | target model |

## 3. Screens

`Data → LAS / DLIS` wizard:

1. Choose files (multi-select) and format (detected).
2. Target well/wellbore, and whether to create a new well.
3. Preview: detected standard (LAS 2.0 / 3.0 / DLIS), index type, curve list, sample count,
   warnings.
4. Mapping: source curve → target curve profile, unit reconciliation.
5. Commit with progress; result summary.
6. Export tab: select well/wellbore/log set, format, options (LAS 2.0, LAS 3.0 with checker,
   DLIS not supported for write).

## 4. Data and contracts

`OgLasLoadResult { Model, Log, Warnings[], Standard, SourceRows, Encoding, Decimated }`
Every warning is preserved and shown; a decimated load states the ratio.

## 5. States

| State | Behaviour |
|---|---|
| Unreadable file | refused with the line and reason |
| Encoding ambiguous | detected value shown, override offered |
| LAS 3.0 non-compliant | `Las3Checker` findings listed; import allowed, export refused |
| Array channel in DLIS | reported as a warning, not silently dropped |
| Very large file | decimation applied and stated |
| Export overwrite | requires confirmation |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Import | `interop.las.write` | yes |
| Export | `interop.las.read` | file |
| Preview | `interop.las.read` | no |

## 7. Acceptance Criteria

- AC-1 A LAS 2.0 file loads, assigns curves, and produces a log set.
- AC-2 A LAS 3.0 file passes `Las3Checker` and round-trips byte-compatibly in structure.
- AC-3 A DLIS file loads frames as log sets and reports array channels as warnings.
- AC-4 A decimated load states the ratio.
- AC-5 Encoding is shown and overridable.
- AC-6 An export produces a file that reloads identically.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~INT-01"
```

Port fixtures from `OilandGasTools/tests/TheTechIdea.OilGas.Tests/Las*Tests.cs`,
`DlisTests.cs`, `DlisioConformanceTests.cs`.

## 9. Open questions and risks

- DLIS write is not supported by the tools; state that in the export tab.

### Delivered 2026-11-06

- **Route `las`** (en + ar): file, encoding, max samples, **Example / Preview / Import**, then
  **Export as 2.0 / 3.0** with an explicit overwrite tick, a facts grid, the warnings line, and the imported
  log drawn in `SkiaViewSurfaceHost`.
- `OgLasWorkspace` wraps the tools' loaders and writers: `Load`, `LoadText`, `LoadDlis`, `ExportText`,
  `ExportFile`, `CheckLas3`, plus a deterministic `ExampleLas` file. Every reader warning is kept.
- **AC-1** the example LAS 2.0 loads with 2 curves (GR, RHOB), 9 rows, index in metres, standard `LAS 2.0`.
- **AC-2 / AC-6** an export as LAS 3.0 passes `Las3Checker` with **zero findings** and reloads with the same
  curves, rows and values; the 2.0 example does **not** pass the 3.0 checker, so the check is not vacuous.
- **AC-3** a DLIS file (RP66 v1, ported fixture with one frame, a scalar GR and an **array** WAVE channel)
  loads as a log set and reports `frame MAIN: array channels not loaded as curves: WAVE` instead of dropping
  the channel silently.
- **AC-4** a bounded load states both counts and the ratio: `Decimated: 9 source row(s) shown as 4 (2.3:1).`
- **AC-5** the encoding the file was read with is reported (`utf-8` detected) and an override is honoured
  (`windows-1252`).
- **§5 states** an unreadable file is `HasErrors` with a warning (never an exception); an existing export is
  refused with `las-export-exists` unless overwrite is ticked.
- **DLIS write** is absent by design: the tools ship no writer, and the export tab offers only LAS 2.0/3.0.

### Still open

- **Persistence is DAT-02's.** The import is session-only and the page says so; nothing is written to a well.
- The mapping step (source curve → target curve profile) is not built; the LAS curve mnemonics are used
  as they arrive.
- DLIS multi-frame export, LAS parameter sections beyond what the reader keeps, and very large files beyond
  bounded decimation are not exercised.
