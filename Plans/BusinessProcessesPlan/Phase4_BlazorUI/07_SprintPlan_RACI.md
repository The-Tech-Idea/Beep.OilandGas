# Phase 4 — Sprint Plan & RACI
## Blazor UI Delivery Schedule

> Total story points: ~68 SP  
> Sprint velocity: 22–25 SP/sprint  
> Target: 3 sprints (~6 weeks) after Phase 3 API is confirmed

---

## Sprint Breakdown

### Sprint 4.1 — Component Library (Weeks 1–2)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| Create `ProcessStateChip` component | 2 | FE | All 9 state colors correct |
| Create `JurisdictionBadge` component | 1 | FE | 3 colors: USA/Canada/International |
| Create `ProcessCategoryCard` component | 2 | FE | Renders definition list with click event |
| Create `ProcessFilterBar` component | 4 | FE | Chips emit filter-changed callback |
| Register `ProcessStateContainer` service + DI | 2 | FE | Service injectable; tests mock it |
| bUnit tests: all 4 component tests + filter bar | 4 | FE | 13 tests pass |
| Scaffold `ProcessCatalog.razor` (loads all 96 definitions) | 4 | FE | Page renders grouped cards |
| **Sprint total** | **19 SP** | | |

---

### Sprint 4.2 — Dashboard & Detail (Weeks 3–4)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| Build `ProcessDashboard.razor` with `MudDataGrid` | 5 | FE | Grid shows field instances; filter bar wired |
| Build `ProcessDetail.razor` — layout and data load | 4 | FE | Detail page loads instance + transitions |
| Build `ProcessTimeline` component + tests | 3 | FE | Timeline renders recent/full; 4 tests pass |
| Build `TransitionPanel` component + tests | 4 | FE | Buttons render; success/guard callbacks work; 5 tests pass |
| Navigate from dashboard → detail → back | 2 | FE | NavigationManager calls work; back button preserves filter |
| Definition Detail page | 3 | FE | Step list, required documents, "Start" button |
| **Sprint total** | **21 SP** | | |

---

### Sprint 4.3 — Wizard, Audit & Polish (Weeks 5–6)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| Build `ProcessStartStepper` component + tests | 5 | FE | Wizard creates real instance; 5 tests pass |
| Build `StartProcess.razor` wrapping stepper | 2 | FE | Route wired; redirects on success |
| Build `GuardFailureDialog` + tests | 3 | FE | Dialog renders correctly; 4 tests pass |
| Build `ProcessAudit.razor` + CSV export | 4 | FE | Full audit list; CSV downloads |
| Category-filtered dashboards (5 pages) | 3 | FE | Each route pre-filters correctly |
| Nav sidebar items added | 1 | FE | All 7 nav links work |
| Responsive QA at 1024px | 2 | FE+QA | Browser smoke checklist complete |
| **Sprint total** | **20 SP** | | |

---

## RACI Matrix

| Task | FE | QA | BD | DA | DS | PM |
|---|---|---|---|---|---|---|
| Component design (props/events) | **R/A** | C | C | — | — | I |
| Page routing | **R/A** | — | — | — | — | I |
| `ProcessStateContainer` | **R/A** | C | — | — | — | I |
| bUnit test authoring | **R** | **R/A** | — | — | — | I |
| MudBlazor version verification | **R/A** | — | — | — | — | I |
| API contract verification | C | — | **R/A** | — | — | I |
| Responsive QA | R | **R/A** | — | — | — | I |
| Acceptance sign-off | C | C | — | — | **A** | **R** |

---

## Risk Register

| Risk ID | Description | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R4-01 | MudBlazor version incompatibility (MudStepper API mismatch) | Medium | Medium | Verify component API at start of Sprint 4.1; pin MudBlazor package version |
| R4-02 | `IFieldOrchestrator.CurrentFieldId` null during SSR pre-render | Low | Medium | Guard all API calls with `if (CurrentFieldId is null) return;`; skip pre-render |
| R4-03 | `ProcessStateContainer` event causes memory leak (no Dispose) | Low | Low | Each page implementing `IDisposable` must unsubscribe `OnChange` |
| R4-04 | Phase 3 API not stable → UI team blocked | Medium | High | Work from mock API responses in Sprint 4.1–4.2; integrate real API in 4.3 |

---

## Definition of Done (Phase 4)

- [ ] All 5 dashboard/detail pages load without JavaScript console errors
- [ ] All 8 components have bUnit tests (35 total tests pass)
- [ ] `ProcessStartStepper` creates a real process instance end-to-end
- [ ] Guard failure dialog appears correctly for blocked transitions
- [ ] Audit trail page loads and CSV export works
- [ ] Nav sidebar entries for all 7 process routes present
- [ ] Responsive smoke test checklist complete at 1024px and 1440px
- [ ] `dotnet build Beep.OilandGas.sln` exits 0 errors
- [ ] MudBlazor package version pinned and documented in `README.md`
