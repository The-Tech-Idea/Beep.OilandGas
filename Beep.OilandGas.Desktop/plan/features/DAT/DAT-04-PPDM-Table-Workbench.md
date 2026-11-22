# DAT-04 — PPDM table workbench

| Field | Value |
|---|---|
| Stage | STG-01 — Data platform and PPDM core |
| Area | Data |
| Status | Gate met - AC-1..AC-7: bounded server-side paging (clamped at 500), server-side filter/sort, edit round-trip preserving untouched columns, guarded update/delete reporting `workbench-conflict`, failed read is a failure, row-policy predicate applied and surfaced, filtered export. Limits: page Save/Delete not UI-tested, no Add row button. See [evidence](../../evidence/STG-01-DAT-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39` |
| Desktop project | `src/Beep.OilandGas.Desktop.Data/Ppdm/`, `Modules/Data/TableWorkbench/` |
| External packages | none beyond `BeepDM` |
| Depends on | DAT-03 |
| Blocks | DAT-05, DAT-06, DAT-07, DAT-08 |
| Web reference | `Pages/PPDM39/DataManagement/PPDMTablePage.razor`, `Components/Data/CategoryTableBrowser.razor`, `EnhancedDataGrid.razor` |

## 1. Purpose

One universal grid over any PPDM table: page, filter, sort, edit a cell, add a row, delete a
row, and export — without writing a screen per table.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39/Core/PPDMGenericRepository.cs` (`IPPDMGenericRepository`) | paged reads, CRUD, filters |
| `PPDM39/Core/PPDMDataSourceTransaction.cs` | transactional writes |
| `PPDM39/Core/Security/IPPDMRowAccessPolicy.cs` | row filtering |
| `PPDM39/Core/CommonColumnHandler.cs` | audit/system columns |
| `PPDM39/Core/Metadata/*` | column types, keys, nullability |
| `Web/Components/Data/EnhancedDataGrid.razor` | grid behaviour to reproduce |

## 3. Screens

`Data → Table workbench`

- Context bar: table name, row count, active filter chips, policy note.
- Toolbar: Add, Delete, Refresh, Columns, Group, Export (CSV/XLSX), Open related.
- Grid: virtualised, server-paged (default 200 rows), sortable, filterable, inline editable for
  allowed columns, key columns pinned and read-only.
- Detail pane: full row as a form (hands off to DAT-05 for structured editing).
- Footer: page navigation, total, "n filtered of m".

## 4. Data and contracts

```csharp
public interface IOgPpdmTableGateway
{
    Task<OgResult<OgPage<OgRow>>> QueryAsync(string table, OgQuery query, CancellationToken ct);
    Task<OgResult<OgRow>> UpdateAsync(string table, OgRow row, OgRow original, CancellationToken ct);
    Task<OgResult> InsertAsync(string table, OgRow row, CancellationToken ct);
    Task<OgResult> DeleteAsync(string table, OgRow row, CancellationToken ct);
    Task<OgResult<byte[]>> ExportAsync(string table, OgQuery query, OgExportFormat format, CancellationToken ct);
}
```

`OgQuery`: `Page`, `PageSize`, `Sort[]`, `Filters[]`, `Search`. `OgRow`: ordered column→value map
plus `OriginalValues` for the update.

## 5. States

| State | Behaviour |
|---|---|
| No rows | empty state with a clear-filters action |
| Read failed | failed state with the failure reference |
| Policy hides rows | shows `n of m` and the policy name |
| Cell edit rejected | the row is restored and the reason is shown inline |
| Concurrency conflict | the row is marked "changed by another user" and reload is offered |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Read | `data.read` + table policy | no |
| Insert/Update/Delete | `data.write` + table policy | yes |
| Export | `data.export` | no (file) |
| See system tables | `data.system.read` | — |

## 7. Acceptance criteria

- AC-1 Paging issues a bounded query; 10 000 rows never load at once.
- AC-2 Sorting and filtering are server-side.
- AC-3 An edit round-trips and preserves untouched columns.
- AC-4 A concurrency conflict is detected and shown.
- AC-5 A failed read is never rendered as an empty grid.
- AC-6 Row policy is applied and visible.
- AC-7 Export respects the active filter.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.IntegrationTests --filter "FullyQualifiedName~DAT-04"
```

## 9. Open questions and risks

- Paging across four providers may behave differently; capture each provider's SQL in the feature
  evidence once.
- `PPDMGenericRepository` may materialise all rows for paging (the repository tracker records this
  limitation for another reader). Measure before promising 10 000-row paging.
