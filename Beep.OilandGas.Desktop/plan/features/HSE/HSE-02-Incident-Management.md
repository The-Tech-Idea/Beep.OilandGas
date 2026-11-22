# HSE-02 — Incident management

| Field | Value |
|---|---|
| Stage | STG-11 — HSE, compliance and permits |
| Area | HSE |
| Status | Gate met - see [evidence](../../evidence/STG-11-HSE-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `Modules/Hse/Incidents/` |
| External packages | none |
| Depends on | HSE-01 |
| Blocks | HSE-03 |
| Web reference | `Pages/PPDM39/HSE/IncidentManagement.razor`, `Pages/PPDM39/BusinessProcess/HSEIncidents.razor`, `HSEIncidentDetail.razor`, `Components/Workflow/HseWorkflowStart.razor` |

## 1. Purpose

Report an incident completely at the moment it happens, and manage the list of open incidents with
their real state.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/HSE/HSEIncidentService.cs` | incident CRUD and workflow link |
| `LifeCycle/Services/HSE/PPDMHSEService.cs` | process linkage |
| `Beep.OilandGas.HSE/Models/CompanyHseWorkflowStart.cs` | workflow start contract |
| Tables incident parent/components plus `PROCESS_*` | persistence |

## 3. Screens

`HSE → Incidents`

- Grid: number, date/time (UTC + company zone), type, tier, severity, field, facility, state,
  investigator, actions open.
- Report dialog: type, tier, explicit jurisdiction, ISO timestamp with offset, field, facility,
  description, immediate actions, people involved, attachments.
- Detail: everything recorded, plus workflow state and linked actions.
- Filters: state (recorded values only), tier, type, date range, field.

## 4. Data and contracts

`OgIncident { Id, Number, OccurredAtUtc, CompanyZoneDisplay, Type, Tier, Severity, FieldId,
FacilityId, Jurisdiction, State, Investigator, OpenActions }`
`OgIncidentReport { Type, Tier, Jurisdiction, OccurredAtIso, FieldId, FacilityId, Description }`.

## 5. States

| State | Behaviour |
|---|---|
| Missing jurisdiction | shown as unspecified; reporting refused until entered |
| Local timestamp without offset | refused |
| Field changed mid-report | refused; the reviewed field must match |
| Failed read | failed state; rows cleared, not shown stale |
| Null population from API | refused, not treated as empty |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Report | `hse.incident.report` | yes |
| Edit | `hse.incident.write` | yes |
| Change state | `hse.incident.workflow` | yes |

## 7. Acceptance criteria

- AC-1 Reporting requires type, tier, explicit jurisdiction and an ISO timestamp with an offset.
- AC-2 No jurisdiction defaults to any country.
- AC-3 A company/field change during a read clears rows and discards the late response.
- AC-4 A null population is refused, not shown as empty.
- AC-5 State changes go through the workflow, not a direct update.
- AC-6 Status filter offers only recorded `IncidentState` values.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~HSE-02"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-11-HSE-02/observed-evidence.json). The report writes four owner tables
through `UnitofWork<T>` and the register reads them back; nothing new was created.

**The owner already had the shape.** The API's own `IncidentReportSchema` validator shows a report as four rows:

| Table | Carries |
|---|---|
| `HSE_INCIDENT` | the header: date and description |
| `HSE_INCIDENT_DETAIL` | `INCIDENT_TYPE_ID` (type) and `INCIDENT_SEVERITY_ID` (tier) |
| `HSE_INCIDENT_COMPONENT` | `FIELD_ID`, `FACILITY_ID`, `JURISDICTION`, location in `REMARK`, and `PROJECT_ID` for linked corrective actions |
| `HSE_INCIDENT_RESPONSE` | the state history, starting with `REPORTED` |

So the desktop writes those and invents no flat incident table. `HSE_INCIDENT_COMPONENT` also carries `FACILITY_ID` and
`PROJECT_ID`, which is why neither a facility column nor an action link had to be added. The two tables the desktop did not
yet provision - detail and component - are now declared by `HseModule` (smoke 423 -> 425 entity types).

**AC-1 is a strict ISO 8601 check.** The timestamp must match `yyyy-MM-ddTHH:mm(:ss(.fff)?)?(Z|±hh:mm)` and parse as a
`DateTimeOffset`; a local time without an offset is refused. The instant is stored in UTC and the register prints it in **UTC
and the company zone**, so neither is mistaken for the other.

**AC-2 is two-sided.** The service refuses a blank jurisdiction with *"None is assumed from the field, the company or the
browser"*, and a guard test forbids a country literal in the view. The stored value is exactly what was reported.

**AC-3 closes HSE-01's largest recorded gap for this read.** The scope (company and field) is captured at the start,
re-read after the rows are read, and a difference discards them with a stated refusal; a newer `BeginLoad` discards an
older response the same way. The page clears its grid. The company id is part of the scope identity but not a row filter,
because these PPDM tables carry no company column - that half of the gap remains.

**AC-4 and AC-5.** A null population is a refusal, not an empty register, and a failed read carries no rows. A state change
is refused with *"The incident keeps its recorded state"* because WFL-01/WFL-02 are not wired; the test asserts the status
history is unchanged, and the page shows the reason instead of offering the command.

**The retry is the interesting design.** The incident id is `INC-<intent key>`, derived from a key the caller holds before
anything is written. A retry adopts the header it already wrote; a different report under the same key is refused as a
conflict; and a retry after a **partial write completes the missing detail, component or status row** instead of duplicating
the header. All three are asserted.

**A real defect the gateway test caught.** The first read model took the incident's *latest* detail row for its type. A report
with immediate actions or people writes later detail rows, so the PEOPLE row became "latest" and the type came back empty -
`Expected "SPILL", Actual ""`. The read now takes the latest detail row that actually carries a type, falling back to the
latest row.

**Not built, and stated as gaps:** **Edit** (the feature lists it, but no acceptance criterion covers it and editing awaits
HSE-03's correction rules); **attachments** (no document store exists, so the dialog does not offer a file it cannot keep);
a live **API population** (the register reads the local owner tables, so AC-4's null-population rule is proven on the read
helper and a failed table read); a **single transaction across the four rows** (each commits separately, and the intent key
completes a partial write on retry); and the incident **number is its id**, because the owner's header has no number column.

## 9. Open questions and risks

- The repository has repeatedly fixed default-to-USA and null-population defects here. Treat the
  existing focused tests as the specification.
