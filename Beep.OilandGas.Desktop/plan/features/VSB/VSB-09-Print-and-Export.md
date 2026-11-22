# VSB-09 — Print and export

| Field | Value |
|---|---|
| Stage | STG-15 — Wellbore visual suite |
| Area | Visuals |
| Status | Gate met - PDF/SVG/PNG export, planning, DPI fidelity, no-partial-file, and a georeferenced map export (world file + metadata). See [evidence](../../evidence/STG-15-VSB-09/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore` (`Output`), `TheTechIdea.OilGas.Mapping` (`MapExport`), `Beep.OilandGas.Drawing` (`Export`) |
| Desktop project | `WellTools/Export/OgExportService.cs` |
| External packages | `TheTechIdea.OilGas.Wellbore`, `Mapping` |
| Depends on | VSB-03 |
| Blocks | RPT-02 |
| Web reference | none |

## 1. Purpose

Get the picture out of the application: PDF with correct pagination and header/legend, vector SVG,
high-resolution PNG, and vector PDF for maps and sections.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore/Output/WellboreExport.cs` | `ToPdf`, `ToPng`, `ToSvg`, `Paginate`, `RenderPage`, `FooterTokens` |
| `Wellbore/Output/PrintPlanner.cs` | `Plan`, `PointsPerDepthUnit` |
| `Wellbore/Output/PanelExport.cs` | correlation and section PDFs |
| `Wellbore/Output/Legend.cs`, `Branding.cs`, `PrintTokens.cs` | chrome |
| `Wellbore/WellborePrintOptions.cs`, `WellboreTheme.cs` | options |
| `Mapping/MapExport.cs` | map PDF/PNG/SVG |
| `Mapping/Scene3D/SceneExport.cs` | glTF/GLB (INT-08) |
| `Beep.OilandGas.Drawing/Export/*` | `ImageExporter`, `SvgExporter`, `PdfExporter`, `GeoJsonExporter` |

## 3. Screens

No page; an export dialog from any visual screen:

- Format: PDF / SVG / PNG / XLSX (data) / GLB (3D).
- Paper: A4, A3, Letter, custom; orientation; scale ratio; margins.
- Content: depth range (whole well / current window / selection), tracks included, header block,
  legend, footer tokens (company, well, date, page x of y).
- Preview of page 1 and page count.
- Output path with "open after export".

## 4. Data and contracts

`OgExportRequest { Kind, Format, Paper, Scale, DepthRange, Tracks[], Header, Legend, Footer,
Destination }`
`OgExportResult { Path, Pages, Bytes, Warnings[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Huge depth range | pagination planned first and the page count shown before export |
| Destination not writable | refused with the path |
| Export cancelled | partial file removed |
| Export failure | retained request; retry offered |
| Very large PNG | warns about memory before starting |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Export | `data.export` | file |

## 7. Acceptance Criteria

- AC-1 A PDF export is paginated with header, legend and "page x of y".
- AC-2 The exported SVG opens in a browser and in `Svg.Skia`.
- AC-3 The PNG resolution matches the requested DPI.
- AC-4 A cancelled export leaves no partial file.
- AC-5 The depth range and scale in the dialog match the output.
- AC-6 Exporting a map produces a georeferenced option when coordinates exist.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~VSB-09"
```

Manual: produce one PDF and open it; verify page count and footer tokens.

## 9. Open questions and risks

- Two PDF paths exist (`Wellbore.Output` and `Beep.OilandGas.Drawing.Export`). Standardise on the
  tools path for well visuals and record the decision.

### Decision — one PDF path

**The tools' `Wellbore.Output` is the standard for well visuals.** `Beep.OilandGas.Drawing.Export` stays for
the Drawing module's own assets. One exporter means one page planner, one set of print options and one
footer-token vocabulary; two would drift.

### Delivered 2026-11-05

- **Route `export`** (en + ar): source (wellbore / well log / well schematic), format (PDF / SVG / PNG),
  scale (fit / 1:100 / 1:500), DPI (96 / 192 / 300), destination under `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Exports`,
  **Plan** (page count before writing) and **Export**.
- `OgExportService` wraps the tools: `Plan` paginates through the same options `Export` will use.
  **Every export writes `<destination>.partial` and moves it into place only on success**, so a cancelled or
  failed export leaves nothing to open.
- **AC-1** a PDF starts with `%PDF-`, the exporter reports its page count, and at 1:20 the demo needs more
  than one page — the plan's count equals the written PDF's. Header, legend and footer tokens are the tools'
  defaults.
- **AC-2** the SVG contains `<svg>` and **Svg.Skia parses it** into a picture (the ADR-001 renderer), not
  merely "it is text".
- **AC-3** a 96 dpi and a 192 dpi export decode to images whose width and height double.
- **AC-4** a pre-cancelled export returns `export-cancelled` with neither the destination nor a `.partial`.
- **AC-5** the plan reports the requested range and scale; a narrower range never needs more pages.
- **AC-6 delivered** (2026-11-06): the "Well map" source exports a PNG from `MapView` with a **world file**
  (`.pgw`) and coordinate metadata (`.geo.json`) when every well carries a CRS. The transform is sampled from
  the view that drew the image, so the file describes the picture rather than a second calculation. A well
  without a CRS is **refused** (`map-no-crs`) rather than georeferenced wrongly; unticking Georeferenced
  writes the image alone. The tests read the world file back, map each well's coordinates to a pixel inside
  the image and back again, and check the file's extent brackets the map's bounds.

### Still open

- Track selection, a depth-window picker, a file dialog and "open after export".
- XLSX (data) and GLB (3D) formats from §3.
- The PDF's header/legend/footer text is not read back by a test; only its structure and page counts are.
