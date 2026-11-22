# WEL-01 — Well search and index

| Field | Value |
|---|---|
| Stage | STG-02 — Wells, wellbores and stratigraphy |
| Area | Wells |
| Status | Gate met - AC-1..AC-5: name/UWI/API identity search (30 ms over 5 000 wells), server-side paging and filtering (3 ms), empty vs failed states, shared well context that cancels stale loads, saved searches read back across store instances. See [evidence](../../evidence/STG-02-WEL-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Wells/WellSearch/` |
| External packages | none |
| Depends on | DAT-04 |
| Blocks | WEL-02 … WEL-06 |
| Web reference | `Pages/PPDM39/WellsIndex.razor`, `Pages/PPDM39/Wells/Search.razor`, `Pages/PPDM39/WellDetails/Search.razor` |

## 1. Purpose

The fastest way to reach any well, and the source of the well context every other page consumes.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle/Services/WellManagement/WellManagementService.cs` | well queries |
| `LifeCycle/Services/WellLifecycle/WellLifecycleService.cs` | current lifecycle state |
| `PPDM39.DataManagement/Services/Well/WellServices*.cs` | well read services |
| `PPDM.Models/39/WELL`, `WELL_XREF` | identity |
| `PPDM39/Core/PPDMGenericRepository.cs` | paged query |

## 3. Screens

- Search bar: free text, with explicit hints (name, UWI, API number, field).
- Filter chips: field, status, well type, purpose, operator, spud year.
- Result grid: name, UWI, API, field, status, type, spud date, last activity.
- Saved searches (per user, stored locally).
- Row action: open (sets the well context and lands on the well overview).
- Multi-select: compare (→ WEL-06), add to a working set.

## 4. Data and contracts

`IBeepOilandGasApp.Well` query methods. `OgWellSummary { Id, Name, Uwi, ApiNumber, FieldName,
Status, WellType, SpudDate, LastActivity }`.

## 5. States

| State | Behaviour |
|---|---|
| No filter, no rows | "no wells in this company/field" |
| Filter, no rows | "no wells match" with clear-filters |
| Ambiguous identity | the row shows a warning chip; opening shows the conflict (WEL-02) |
| Read failed | failed state, not empty |
| > 5 000 results | paging with a total count and a narrowing hint |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Search | `wells.read` |
| Open | `wells.read` |
| Add to working set | `wells.read` |

## 7. Acceptance criteria

- AC-1 Name, UWI and API number all return the expected well.
- AC-2 5 000 wells page and filter within the REL-04 budget.
- AC-3 Empty and failed are distinguishable.
- AC-4 Selecting a well sets the shared well context and cancels prior loads.
- AC-5 Saved searches survive a restart.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WEL-01"
```

## 9. Open questions and risks

- `WELL_XREF` may hold multiple identifiers per well; the index must not silently pick one.
