# HSE-05 — Compliance calendar and obligations

| Field | Value |
|---|---|
| Stage | STG-11 — HSE, compliance and permits |
| Area | HSE |
| Status | Gate met - see [evidence](../../evidence/STG-11-HSE-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `Modules/Hse/Compliance/` |
| External packages | none |
| Depends on | HSE-01 |
| Blocks | HSE-06, DEC-01 |
| Web reference | `Pages/PPDM39/HSE/ComplianceCalendar.razor`, `Pages/PPDM39/BusinessProcess/ComplianceObligations.razor`, `ComplianceReport.razor`, `CreateFieldObligationPage.razor`, `Compliance/ObligationDetail.razor`, `Components/BusinessProcess/ObligationCalendar.razor` |

## 1. Purpose

Show what is due, for which field and jurisdiction, and record the discharge of an obligation —
without inventing deadlines or legal status.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/Compliance/ComplianceService.cs` | obligations |
| `Beep.OilandGas.LifeCycle` `PPDMHSEService` and compliance process | report process |
| Tables `OBLIGATION`, `OBLIGATION_COMPONENT`, recorded facts | persistence |
| `Components/BusinessProcess/ObligationCalendar.razor` | calendar behaviour |

## 3. Screens

`HSE → Compliance`

- Calendar: month view with obligation markers, overdue highlighted.
- Obligation list: id, type, description, jurisdiction, due date, state, components.
- Detail: recorded facts, components with fulfilled dates, payments, history.
- New obligation: type, jurisdiction (explicit), due date, description, components.
- Report process: start an internal process with explicit jurisdiction; state that no regulator
  delivery is established.

## 4. Data and contracts

`OgObligation { Id, Kind, Description, Jurisdiction, DueDate, State, Components[] }`
`OgObligationComponent { Name, Required, FulfilledDate, EvidenceRef }`.

## 5. States

| State | Behaviour |
|---|---|
| No jurisdiction | shown as unspecified; reporting refused |
| Due date missing | "unscheduled"; not overdue |
| Overdue | computed from the recorded due date and today, never inferred |
| Failed read | failed state |
| Response mismatch on submit | refused, blocked for that company/field |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `compliance.read` | yes (read) |
| Create obligation | `compliance.write` | yes |
| Record fulfilment | `compliance.write` | yes |
| Start report process | `compliance.report` | yes |

## 7. Acceptance criteria

- AC-1 Due dates come from records; no interpretation of legal deadlines.
- AC-2 A missing due date shows unscheduled, not overdue.
- AC-3 Jurisdiction is explicit and never defaulted.
- AC-4 An obligation without components cannot be marked fulfilled.
- AC-5 The report process states that no regulator delivery is established.
- AC-6 The calendar and list agree on counts.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~HSE-05"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-11-HSE-05/observed-evidence.json). The register is read from the owner's
`OBLIGATION` and `OBLIGATION_COMPONENT` through `UnitofWork<T>`; three additive columns carry what the feature's contract needs
and the PPDM tables lacked.

**The owner's compliance service targets columns that do not exist.** `ComplianceService` builds and maps an obligation by
`OBLIG_TYPE`, `OBLIG_STATUS`, `DUE_DATE`, `JURISDICTION_CODE`, `FULFILL_DATE` and `FIELD_ID`, none of which are on the PPDM
table, and it reads them through a reflection helper that returns empty for a missing property - so its reads would silently
produce blank obligations. The desktop maps the columns that exist (`OBLIGATION_TYPE`, `CRITICAL_DATE`, `DESCRIPTION`,
`FULFILLED_IND`/`FULFILLED_DATE`, `REMARK`) and adds `OBLIGATION.FIELD_ID`, `OBLIGATION.JURISDICTION` and
`OBLIGATION_COMPONENT.REQUIRED_IND` additively, so the owner service's own intent becomes expressible without overwriting it.

| Feature field | Recorded in |
|---|---|
| Id | `OBLIGATION_ID` with `OBLIGATION_SEQ_NO` |
| Kind | `OBLIGATION_TYPE` |
| Description | `DESCRIPTION` |
| Jurisdiction | `JURISDICTION` (added) |
| Due date | `CRITICAL_DATE`, falling back to `EXPIRY_DATE` when that is the recorded one |
| Fulfilment | `FULFILLED_IND` and `FULFILLED_DATE` |
| Waiver | `REMARK` prefixed `WAIVED: ` |
| Components | `OBLIGATION_COMPONENT` rows with `DESCRIPTION`, `REQUIRED_IND`, `FULFILLED_DATE` and `FULFILLED_REMARK` |

**The state is derived once.** Fulfilled and waived are recorded facts and win over the date. Otherwise a recorded due date in
the past is `OVERDUE`, today or later is `PENDING`, and no due date at all is `UNSCHEDULED` - a value the owner's vocabulary
does not have, added because "no date recorded" must never be read as "overdue". Nothing is inferred from a reporting period,
a frequency or a jurisdiction, and the page shows the recorded date or the word "unscheduled".

**Fulfilment is a component's work.** An obligation is created with at least one component, because one without could never
be fulfilled; fulfilling it is refused while a required component is outstanding, and the refusal names the components. The
requirement flag is the added `REQUIRED_IND`, read as required when null so existing rows stay meaningful.

**The report process is the honest one the Web settled on.** It is an internal request, not a regulator submission: the page
prints "No regulator delivery is established" and the command is refused with that sentence, because there is no workflow
engine (WFL-01/WFL-02) and no regulator channel to file through. Nothing pretends to be filed.

**Not built, and stated as gaps:** a graphical month grid (the calendar is the month's days with their due and overdue
counts); payments from `OBLIG_PAYMENT` (the feature's detail mentions payments and history); a history of state changes; a
startable report process; a waiver as its own record rather than a remark prefix; a kind outside the owner's enumerated set;
a single transaction across the obligation and its components; and per-command permission enforcement.

## 9. Open questions and risks

- The Web replaced a false regulator-submission form with an internal process request. Preserve
  that honesty in the desktop.
