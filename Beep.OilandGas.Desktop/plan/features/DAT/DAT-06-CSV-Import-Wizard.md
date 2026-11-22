# DAT-06 — CSV import wizard

| Field | Value |
|---|---|
| Stage | STG-01 — Data platform and PPDM core |
| Area | Data |
| Status | Gate met - AC-1..AC-6 plus the plan options: 1000-row counts (900/100), line+column+reason per rejection, SHA-256 hash + mapping profile, cancel between rows and batches, invalid rows never written, composite-key upsert, and insert-only mode, batch size and encoding override on the screen (one page, not six steps; not click-tested). See [evidence](../../evidence/STG-01-DAT-06/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `Modules/Data/CsvImport/` |
| External packages | none |
| Depends on | DAT-05 |
| Blocks | DAT-07 |
| Web reference | `Pages/PPDM39/DataImport.razor`, `Components/ImportCsvDialog.razor`, `ImportCsvWizard.razor` |

## 1. Purpose

Bring tabular data in reliably: choose a file, map its columns to a table, preview, validate,
then commit — with a per-row outcome so a partial import is unmistakable.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/PPDMDataValidationService.cs` | row validation |
| `PPDM39.DataManagement/Services/PPDMMappingService.cs` | column mapping |
| `PPDM39.DataManagement/Services/PPDMDefaultValueService.cs` | missing mandatory defaults |
| `PPDM39/Core/PPDMDataSourceTransaction.cs` | batched commit |
| `TheTechIdea.OilGas.Wellbore.Loaders/Tabular/CsvTabularSource.cs` | alternative reader for well-shaped files (INT-04) |

## 3. Screens

`Data → Import` wizard:

1. File — path, encoding (detected, overridable), delimiter, header row, first-row preview.
2. Target — table, insert mode (insert / upsert by key), batch size.
3. Mapping — source column → target field, automatic suggestions, unmapped warnings, unit column
   handling, constant values for missing fields.
4. Preview — first 100 mapped rows with validation state per cell; counts of valid/invalid.
5. Commit — progress per batch, cancellable; summary of inserted / updated / rejected with reasons.
6. Result — downloadable rejection report (CSV) and a link to the imported rows.

## 4. Data and contracts

```csharp
public interface IOgCsvImporter
{
    Task<OgResult<OgImportPreview>> PreviewAsync(OgImportRequest request, CancellationToken ct);
    Task<OgResult<OgImportOutcome>> CommitAsync(OgImportRequest request,
        IProgress<OgImportProgress> progress, CancellationToken ct);
}
```

`OgImportOutcome`: `Inserted`, `Updated`, `Rejected`, `Rejections[] { Line, Column, Reason }`,
`SourceHash`, `ProfileName`.

## 5. States

| State | Behaviour |
|---|---|
| Unreadable file | refused with the encoding and line number |
| No header | first row is data, columns named `Column1…n`; mapping still possible |
| All rows invalid | commit is disabled; preview explains why |
| Partial success | result states both counts and lists rejections; never "success" |
| Cancelled mid-commit | completed batches are reported; the remainder is not silently applied |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Preview | `data.import` | no |
| Commit | `data.import` + `data.write` on the table | yes |
| Download rejections | `data.export` | no |

## 7. Acceptance criteria

- AC-1 1 000 rows import with correct accepted/rejected counts.
- AC-2 Every rejected row reports line, column and reason.
- AC-3 The source file hash and mapping profile are stored with the import.
- AC-4 Cancelling stops further batches and reports what completed.
- AC-5 No row is written when preview validation fails for it.
- AC-6 Importing into a table with a composite key upserts, never duplicates.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.IntegrationTests --filter "FullyQualifiedName~DAT-06"
```

## 9. Open questions and risks

- Very large files need streaming; agree a size ceiling (proposed 500 000 rows) and record it.
- Encoding detection is heuristic; always allow an override and show the detected value.
