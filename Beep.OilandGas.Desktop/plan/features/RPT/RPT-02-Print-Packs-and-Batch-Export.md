# RPT-02 — Print packs and batch export

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Reporting |
| Status | Gate met - see [evidence](../../evidence/STG-18-RPT-02/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore` (`Output`), `Beep.OilandGas.Drawing` (`Export`) |
| Desktop project | `Modules/Reporting/PrintPacks/` |
| External packages | `TheTechIdea.OilGas.Wellbore`, `Mapping` |
| Depends on | VSB-09, RPT-01 |
| Blocks | — |
| Web reference | none |

## 1. Purpose

Produce the documents people actually put in a binder: one PDF per well for a whole field, a pack of
schematics, a set of log prints — with a manifest and consistent headers.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore/Output/WellboreExport.cs`, `PrintPlanner.cs` | pagination and rendering |
| `Wellbore/Output/PanelExport.cs` | correlation and section packs |
| `Mapping/MapExport.cs` | map packs |
| `Beep.OilandGas.Drawing/Export/ImageExporter.cs`, `SvgExporter.cs`, `PdfExporter.cs`, `GeoJsonExporter.cs` | alternative exporters |

## 3. Screens

`Reporting → Print packs`

- Pack definition: content type (schematic, log, correlation, map, report), scope (selected wells /
  field / saved list), options (paper, scale, header, legend).
- Preview: file count estimate and a sample of the first document.
- Run: progress with current well, cancellable.
- Result: output folder, manifest (CSV/JSON) listing well, file, pages, bytes, warnings.
- Failures: per-well failures listed with the reason; the run continues unless "stop on error" is set.

## 4. Data and contracts

`OgPrintPack { ContentType, Scope, Options }`
`OgPackManifest { GeneratedAt, Items[] { WellId, File, Pages, Bytes, Warnings[] } }`.

## 5. States

| State | Behaviour |
|---|---|
| Some wells lack data | skipped and listed, not silently omitted |
| Disk full | run stops and the manifest records what completed |
| Cancelled | completed items kept, manifest written |
| Duplicate file name | disambiguated deterministically |
| No wells in scope | run refused with the reason |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Define pack | `reporting.pack.write` | yes |
| Run pack | `reporting.pack.run` + `data.export` | files |
| Open output | any user with file access | no |

## 7. Acceptance Criteria

- AC-1 A pack over N wells produces N documents and one manifest.
- AC-2 A well without data is listed as skipped with the reason.
- AC-3 Cancelling writes a manifest of completed items.
- AC-4 File names are unique and deterministic.
- AC-5 Each document carries the header and legend options.
- AC-6 The manifest page counts match the documents.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~RPT-02"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-18-RPT-02/observed-evidence.json).

**The document is the owners'.** `WellboreExport.ToPdf` with `WellborePrintOptions`, over the document `WellModelConverter` makes from the
engine's `WellModel` — nothing in this feature draws a page, which is what makes a printed pack consistent with the well view on screen.

| What the page shows | Where it comes from |
|---|---|
| The documents | `WellboreExport.ToPdf`, one per well, or an RPT-01 report printed through its own service |
| The skipped wells and their reasons | the pack's own check, before a document is attempted |
| Progress | a report per well boundary |
| The manifest | the run's items, as JSON and CSV beside the documents |
| The page counts | read back out of each PDF's page objects |

**`PaperSize` is a readonly struct, not an enum.** It has four static sizes — A4, A3, Letter, Tabloid — so `Enum.TryParse` throws on it. A
first version offered A2 and A1, which the owner cannot make; the page now offers **exactly the four the owner defines**, because a choice
the owner cannot honour is a lie.

**AC-2 is a status, not a silent drop.** A well that cannot produce a document is an item with status `skipped` and a reason — no
identifier, or fewer than two survey stations — named in the **preview before the run** and again in the manifest and its CSV.

**AC-3 is the only cancellation the owner allows.** `ToPdf` has no token, so the pack takes it **at a well boundary**: a cancellation
never leaves half a document, the documents already written are kept, and the manifest is **still written** for what completed.

**AC-4 re-runs onto its own files.** The name is the pack's order number, the content type and the well's UWI made safe, so the same scope
in the same order gives the same names, and two wells whose identifiers clean to the same text are still told apart by the index.

**AC-6 reads the file, not the plan.** Every manifest page count is counted out of the PDF's own page objects — the same correction
RPT-01 needed, because a PDF's page-tree node `/Type /Pages` **contains** the page's marker, so a naive count is one too many.

**A third copy became one.** INT-07 and INT-08 each carried a private `ReadWellsAsync` assembling a `WellModel` from five PPDM tables.
Rather than write a third for the pack, it was extracted to **`OgWellModelReader`** and both interop services now delegate to it — so a
well is placed on a map, drawn in a scene and printed in a pack from **one assembly of the same rows**.

**Not built, and stated as gaps:** schematics, correlation panels and map packs (the plan's source table names `PanelExport` and
`MapExport`, which produce single sheets rather than a paged print, so they are recorded as not built rather than offered as a type that
would behave differently); a cancellation inside one document (the owner has no token); file names that survive a change of scope (they
are numbered by order); a manifest written incrementally (it is written after the last document); a saved pack definition; paper larger
than A3; and per-well print options.

## 9. Open questions and risks

- A large pack can run for a long time; require the operation to be cancellable and resumable at
  well granularity.
