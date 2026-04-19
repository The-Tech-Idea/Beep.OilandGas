# Phase 4 — Test Plan
## bUnit Component Tests and Browser Smoke Tests

> Framework: bUnit (Blazor component unit testing)  
> Location: `Beep.OilandGas.Web.Tests/`

---

## ProcessStateChip Tests

| # | Test | Setup | Assert |
|---|---|---|---|
| 1 | Renders "DRAFT" with Default color | `State = "DRAFT"` | Chip label = "DRAFT"; `Color = Default` |
| 2 | Renders "APPROVED" with Success color | `State = "APPROVED"` | Chip Color = Success |
| 3 | Renders "CANCELLED" with Error color | `State = "CANCELLED"` | Chip Color = Error |
| 4 | Unknown state renders Default color | `State = "UNKNOWN_XYZ"` | Color = Default; no exception |

---

## TransitionPanel Tests

| # | Test | Setup | Assert |
|---|---|---|---|
| 1 | Renders one button per transition | 3 transitions wired up | 3 MudButton elements rendered |
| 2 | Click button calls API and fires OnTransitionSuccess | API mock returns 200 | `OnTransitionSuccess` invoked with deserialized response |
| 3 | Click button, API returns 422 → fires OnGuardFailure | API mock returns 422 ProcessGuardProblem | `OnGuardFailure` invoked with correct `RequiredField` |
| 4 | Buttons disabled while request in flight | Slow API mock | `Disabled = true` on all buttons during await |
| 5 | Empty transitions list renders no buttons | `Transitions = []` | No MudButton in markup |

---

## ProcessTimeline Tests

| # | Test | Setup | Assert |
|---|---|---|---|
| 1 | ShowAll=false shows at most 5 items | 10 records | 5 MudTimelineItem elements |
| 2 | ShowAll=true shows all records | 10 records | 10 MudTimelineItem elements |
| 3 | Reason text displayed when non-empty | Record with Reason | Reason text present in markup |
| 4 | Reason text not rendered when null | Record with null Reason | No empty `<em>` tag |

---

## ProcessStartStepper Tests

| # | Test | Setup | Assert |
|---|---|---|---|
| 1 | Confirm disabled until step 3 reached | Mount stepper at step 1 | Confirm button disabled |
| 2 | Step 1 validation: no definition → cannot advance | Leave MudSelect empty | `HasError = true` on step 1 |
| 3 | Complete stepper calls API with correct request | Fill all 3 steps | `POST /api/field/current/process/start` called with matching JSON |
| 4 | On 201 response, `OnStarted` fired | API mock 201 | Callback invoked with `instanceId` |
| 5 | On 400 response, error displayed | API mock 400 | MudAlert or error text visible |

---

## GuardFailureDialog Tests

| # | Test | Setup | Assert |
|---|---|---|---|
| 1 | Renders `RequiredField` in alert | Non-null Problem | Alert text contains `Problem.RequiredField` |
| 2 | Renders `RegulatoryReference` when non-null | RegulatoryReference = "IOGP S-501 §3.2" | Text containing `§3.2` present |
| 3 | Regulatory reference section hidden when null | RegulatoryReference = null | No `<i>` element rendered |
| 4 | Dismiss button closes dialog | Click Dismiss | `MudDialog.Close()` invoked |

---

## ProcessFilterBar Tests

| # | Test | Setup | Assert |
|---|---|---|---|
| 1 | Initial render shows all states as chips | Default state | Non-empty chip set |
| 2 | Selecting "DRAFT" chip fires OnFilterChanged with `{"DRAFT"}` | Click DRAFT chip | Callback invoked; `SelectedStates` = `["DRAFT"]` |
| 3 | Reset clears all selections | Select 2 chips, click Reset | Both chips deselected; callback fired with empty set |

---

## Browser Smoke Tests (Manual Checklist)

> Run at viewport 1440×900 before each sprint release.

- [ ] `/ppdm39/process` loads within 2 seconds; grid shows ≥1 row from test data
- [ ] Filter by state "DRAFT" filters grid correctly
- [ ] Navigate to any instance → `TransitionPanel` shows buttons
- [ ] Click a valid transition → state chip updates without full page reload
- [ ] Trigger a guarded transition (remove contractor from test WO) → `GuardFailureDialog` appears
- [ ] `/ppdm39/process/catalog` loads all category cards
- [ ] Definition Detail page shows step timeline
- [ ] Start Process wizard: complete all 3 steps → new instance created → redirected to ProcessDetail
- [ ] Audit trail page shows timeline entries
- [ ] Audit trail "Download CSV" produces downloadable file
- [ ] Responsive: collapse sidebar to 1024px → grid still usable
