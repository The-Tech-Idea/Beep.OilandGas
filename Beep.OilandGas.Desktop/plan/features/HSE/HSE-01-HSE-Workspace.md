# HSE-01 — HSE workspace

| Field | Value |
|---|---|
| Stage | STG-11 — HSE, compliance and permits |
| Area | HSE |
| Status | Gate met - see [evidence](../../evidence/STG-11-HSE-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle`, `Beep.OilandGas.UserManagement` |
| Desktop project | `Modules/Hse/Workspace/` |
| External packages | none |
| Depends on | WEL-01, WFL-05 |
| Blocks | HSE-02 … HSE-07 |
| Web reference | `Pages/PPDM39/HSE/HSEDashboard.razor` (`/hse/dashboard`, `/ppdm39/hse`, `/lifecycle/hse/dashboard`) |

## 1. Purpose

The safety officer's landing: incidents by state, obligations due, studies open, permits pending —
all from recorded facts, plus the persona's assigned work.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/HSE/HSEKPIService.cs` | KPI facts |
| `PPDM39.DataManagement/Services/HSE/HSEIncidentService.cs` | incident counts |
| `LifeCycle/Services/HSE/PPDMHSEService.cs` | HSE orchestration |
| `DefaultSecuritySeedService` | `HSE_OFFICER`, `HSE_COORDINATOR` landings |
| Persona inbox | assigned work |

## 3. Screens

- KPI cards: open incidents, overdue actions, obligations due, open HAZOP actions, permits pending.
- Incident state strip: the five recognised active states plus cancelled and other.
- Persona inbox panel.
- Source links: incidents, compliance calendar, obligations, HAZOP, permits, GHG.
- Removed: any hardcoded incident/overdue/permit alert (the Web removed these).

## 4. Data and contracts

`OgHseCounts { OpenIncidents, OverdueActions, ObligationsDue, OpenHazopActions, PermitsPending }`
`OgIncidentStateCount { State, Count }` over `IncidentState` values.

## 5. States

| State | Behaviour |
|---|---|
| Failed read | failure panel; no zero counts |
| Empty jurisdiction | shown as unspecified; never defaulted to a country |
| Cancelled incidents | counted separately, not in open |
| Restricted source | link hidden with a reason |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | `hse.read` |
| Open a source page | that page's gate |

## 7. Acceptance criteria

- AC-1 Open counts include only the five recognised active states.
- AC-2 Cancelled and other states are shown separately.
- AC-3 A failed read never becomes zero.
- AC-4 No hardcoded alert exists in this feature (guard test).
- AC-5 Each source link is independently gated.
- AC-6 The persona landing route matches the RBAC configuration.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~HSE-01"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-11-HSE-01/observed-evidence.json). The workspace reads seven owner
tables through `UnitofWork<T>` and composes WFL-05's task inbox for the assigned-work panel; it writes nothing.

**The state is a history, not a column.** `HSE_INCIDENT` carries no status. The state is a series of `HSE_INCIDENT_RESPONSE`
rows with `RESPONSE_TYPE = 'STATUS'` and the state in `RESPONSE_RESULT`, so "the state" only exists as the **latest active
row**. The workspace takes the latest per incident (`RESPONSE_OBS_NO`, then `EFFECTIVE_DATE`), ignores inactive rows and
non-STATUS responses, and the gateway test seeds a state that changes and an inactive later row to prove both rules.

**There are no HAZOP or corrective-action tables.** Both are `PROJECT` rows distinguished by `PROJECT_TYPE`
(`HAZOP`, `CORRECTIVE_ACTION`), with `PROJECT_STEP` for nodes/steps and `PROJECT_STATUS` for their history - the owner's own
reference codes say so. So an overdue action is a past-due step whose latest `CA_STEP` status is not `COMPLETED` (and a step
with no status row is open, the owner's default), and an open HAZOP action is a deviation whose latest `HAZOP_DEVIATION`
status is `OPEN` or `ACTION_REQUIRED`.

**Permits are read from history, not from the header.** `PERMIT_APPLICATION.STATUS` is an enum, and one of its storage keys
(`ADDITIONAL_INFO_REQUIRED`) does not match the enum member name - so materialising the header would fail on exactly the
rows the dashboard cares about. Pending permits are therefore counted from `PERMIT_STATUS_HISTORY`, whose `STATUS` is a
plain canonical string.

**Nothing was provisioned for these tables before.** `HseModule` declared no entity types (the incident command binds to
PPDM_CORE, which declares none of its own either), so the six tables this screen reads are now declared in `HseModule` -
the module that owns HSE - and smoke moved from 418 to 423 entity types.

**AC-3 is structural.** A failed read returns a failed result with **no snapshot at all**, so the page has no figure it could
show as zero; the guard test asserts the failed branch clears the counts, the strip and the links, and the unit test proves
the no-connection path returns no value.

**AC-4 keeps the Web's removal.** The view may not contain a `Count = <number>` assignment or a `<number> incidents`-style
sentence; the guard test enforces both and requires the figures to come from the workspace.

**AC-5 is three answers from one predicate.** A route the shell opens is offered; a route that exists but the account cannot
open is refused with that reason; a route this build does not carry names the feature that adds it. The gateway asserts all
three at once.

**AC-6 needed a real change.** The HSE officer's persona route was `charts`, a page that happened to exist, because no HSE
page did. It is now the RBAC seed's `/hse/dashboard` mapped to `hse`; a guard test reads `DEFAULT_LANDING_ROUTE` from the
seed and the smoke report records `HseOfficer->hse`.

**The gap that matters most:** the stage's architecture note asks for FIELD-scoped reads revalidated before and after a load.
These PPDM tables carry no FIELD column the desktop can filter on, so the workspace reads the company database and shows the
session's field name only. That is recorded as the feature's largest limit rather than presented as done; the first
field-scoped HSE record (HSE-02's stored context) is where it must be closed.

**Other limits:** the jurisdiction is always reported "not specified" because no jurisdiction is available to the session;
obligations due use only a recorded date that has arrived (HSE-05 owns lead times); an open HAZOP action is a deviation
status rather than an action item; and a permit with no history row is not counted.

## 9. Open questions and risks

- The Web explicitly removed hardcoded alert values from this page. Preserve that removal as a
  guard test.
