# WEL-04 — Well status classification and history

| Field | Value |
|---|---|
| Stage | STG-02 — Wells, wellbores and stratigraphy |
| Area | Wells |
| Status | Gate met - AC-1..AC-6: append-only history, codes from R_WELL_STATUS, illegal transition refused with the legal set (LifeCycle state machine), unacknowledged change retained and retried unchanged, effective vs recorded dates both shown, audited with actor/from/to/reason. See [evidence](../../evidence/STG-02-WEL-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `Modules/Wells/WellStatus/` |
| External packages | none |
| Depends on | WEL-03 |
| Blocks | PRD-03, DEC-01, licensing and reporting screens |
| Web reference | `Pages/PPDM39/WellDetails/SetWellStatus.razor`, `Pages/PPDM39/WellDetails/Status.razor` |

## 1. Purpose

Change a well's status with a reason, and keep an append-only history that reports and audits can
trust.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Repositories/IWellStatusService.cs` and implementation | status read/write |
| `PPDM.Models/39/R_WELL_STATUS`, `WELL_STATUS` | reference codes and history |
| `PPDM39.DataManagement/Modules/WellStatusFacetModule.cs` (`WELL_STATUS_FACETS`) | seed data |
| `LifeCycle/Services/WellLifecycle/WellLifecycleService.cs` | lifecycle consequences |

## 3. Screens

`Wells → Status`

- Current status card: code, label, since date, last changed by.
- Change panel: new status (from `R_WELL_STATUS`, active only), effective date, reason code,
  free-text note, evidence attachment (optional).
- History grid: from → to, effective date, recorded at, actor, reason, note; read-only.
- Consequence preview: what the change will affect (production reporting, decommissioning queue,
  licensing), stated as information, not enforced.

## 4. Data and contracts

`OgWellStatusChange { WellId, FromCode, ToCode, EffectiveDate, ReasonCode, Note, Actor, RecordedAt }`.

## 5. States

| State | Behaviour |
|---|---|
| No reference codes | "status reference data not seeded" with a link to DAT-02 |
| Change pending | retained-original on lost acknowledgement (ADR-010) |
| Illegal transition | refused with the legal set listed |
| History unavailable | failed state, never an empty history |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Change status | `wells.status.write` | yes (append) |
| View history | `wells.read` | no |
| Edit history | — | never |

## 7. Acceptance criteria

- AC-1 A change appends history and cannot be edited afterwards.
- AC-2 Status codes come from the reference table, never literals.
- AC-3 An illegal transition is refused and lists the legal ones.
- AC-4 A lost acknowledgement retains the original and offers retry of the same original.
- AC-5 Effective date may differ from recorded date and both are shown.
- AC-6 The change is audited with actor, from, to and reason.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WEL-04"
```

## 9. Open questions and risks

- Legal transitions may not be encoded anywhere. If not, the service is authoritative and the UI
  offers everything, with the refusal surfaced.
