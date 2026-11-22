# WEL-03 — Wellbore management

| Field | Value |
|---|---|
| Stage | STG-02 — Wells, wellbores and stratigraphy |
| Area | Wells |
| Status | Gate met - AC-1..AC-5: per-well list, parent well required, delete refused with the survey count, total depth carries its unit, sidetrack records its parent. New LifeCycle-owned WELLBORE/WELLBORE_SURVEY tables. See [evidence](../../evidence/STG-02-WEL-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Wells/Wellbores/` |
| External packages | none |
| Depends on | WEL-02 |
| Blocks | VSB-03, DEV-03, ENG-01 |
| Web reference | `Pages/PPDM39/Wells/Wellbores.razor`, `Pages/PPDM39/WellDetails/Wellbores.razor` (both are stubs) |

## 1. Purpose

Manage the physical holes under a well: sidetracks, re-drills, and their directional identity.
The Web has only "data has moved" stubs here, so this is genuinely new surface.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle/Services/WellLifecycle/WellLifecycleService.cs` | wellbore lifecycle |
| `PPDM.Models/39/WELLBORE`, `WELL_DIR_SRVY`, `WELL_DIR_SRVY_STATION` | entities |
| `PPDM39/Core/PPDMGenericRepository.cs` | CRUD |

## 3. Screens

`Wells → Wellbores`

- Grid: name, parent, type (original / sidetrack / re-drill), status, spud, TD, datum.
- Detail: identity, parent link, surface location (may differ from the well), datum elevations,
  survey reference, status.
- Relationship view: simple parent/child tree when sidetracks exist.
- Link to survey stations (read-only here; VSB-07 and ENG-01 consume them).

## 4. Data and contracts

`OgWellboreSummary { Id, Name, ParentId, Kind, Status, SpudDate, TotalDepth, DepthUnit }`.

## 5. States

| State | Behaviour |
|---|---|
| No wellbores | explicit "a well must have at least one wellbore" warning with a create action |
| Parent missing | refused; an orphan wellbore is not allowed |
| Depth unit missing | shown as unspecified; survey and geometry screens are disabled |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Create wellbore | `wells.write` | yes |
| Edit | `wells.write` | yes |
| Retire | `wells.write` | yes |

## 7. Acceptance criteria

- AC-1 The list only contains wellbores of the selected well.
- AC-2 A wellbore cannot exist without a parent well.
- AC-3 Deleting a wellbore with surveys is refused with the count.
- AC-4 Total depth carries a unit.
- AC-5 A sidetrack records its parent explicitly.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WEL-03"
```

## 9. Open questions and risks

- The Web stubs mean there is no behavioural precedent; design from the PPDM model and record the
  decisions here before coding.
