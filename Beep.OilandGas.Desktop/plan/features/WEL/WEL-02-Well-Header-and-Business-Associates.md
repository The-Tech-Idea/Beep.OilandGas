# WEL-02 — Well header and business associates

| Field | Value |
|---|---|
| Stage | STG-02 — Wells, wellbores and stratigraphy |
| Area | Wells |
| Status | Gate met - AC-1..AC-6: guarded edit round-trip preserving untouched columns, conflicting identifiers refuse auto-selection and record the explicit choice, location edits require and record the coordinate system, participant changes record actor/date, the operator cannot be removed, status is read-only (WEL-04 owns it). See [evidence](../../evidence/STG-02-WEL-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Wells/WellHeader/` |
| External packages | none |
| Depends on | WEL-01 |
| Blocks | WEL-03, WEL-06, most domain pages |
| Web reference | `Components/Shared/PPDMEntityForm.razor` with `WELL` definitions; `Pages/PPDM39/DataManagement/WellDataManagement.razor` |

## 1. Purpose

The single authoritative view of a well's identity and participants, and the place where
conflicting identifiers are resolved deliberately.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle/Services/DataMapping/WellMapper.cs` | PPDM projection |
| `PPDM.Models/39/WELL`, `WELL_XREF`, `BUSINESS_ASSOCIATE`, `R_WELL_STATUS` | entities |
| `PPDM39.DataManagement/Services/PPDMDataValidationService.cs` | validation |
| `PPDM39.DataManagement/Services/Well/WellServices*.cs` | read/write |

## 3. Screens

`Wells → Well header` (opens from search or from the well context bar)

Tabs: Identity · Location · Classification · Dates · Participants · Identifiers · Notes.

- Identity: name, legal name, UWI, API number, government number, field, operator.
- Location: surface latitude/longitude, datum, coordinate system, ground elevation, KB, TD.
- Classification: well type, purpose, status (read-only here, changed in WEL-04), class, profile.
- Dates: spud, completion, first production, last activity, abandonment.
- Participants: business associates with a role (operator, partner, contractor) and interest.
- Identifiers: all `WELL_XREF` rows with type and source; conflicts highlighted.

## 4. Data and contracts

`OgWellHeader` mirrors the PPDM columns the service exposes; **no new entity is defined**.
Business associates are read through the owning service, not a desktop copy.

## 5. States

| State | Behaviour |
|---|---|
| Ambiguous identifier | banner naming the conflicting sources; the well name is not auto-chosen |
| Missing location | warning chip; map and survey pages are disabled with a reason |
| Concurrency conflict | side-by-side original vs current |
| Unsaved changes | navigation guard |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit header | `wells.write` | yes |
| Add/remove participant | `wells.participants.write` | yes |
| Add identifier | `wells.identifiers.write` | yes |
| Resolve identity conflict | `wells.identity.resolve` | yes + audit |

## 7. Acceptance criteria

- AC-1 An edit round-trips and preserves untouched columns.
- AC-2 A conflicting identifier is refused for automatic selection and must be resolved explicitly.
- AC-3 Location edits record the coordinate system used.
- AC-4 Participant changes record actor and date.
- AC-5 Removing the operator is refused.
- AC-6 Status cannot be changed here (only in WEL-04).

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WEL-02"
```

## 9. Open questions and risks

- Interest fractions are financial: `decimal`, sum to 1.0 within an explicit tolerance, refuse
  otherwise.
