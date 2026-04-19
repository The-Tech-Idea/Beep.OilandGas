# Phase 3 — Development Business Process UX

> **Status**: 🔲 Not Started  
> **Depends on**: Phase 1 (Foundation & Navigation)  
> **Duration**: ~2 weeks  
> **Reference Process**: `Plans/BusinessProcessesPlan/PetroleumEngineerBusinessProcesses.md` — Part 3  
> **Goal**: Build the development section as a development/project engineer's work environment for taking a discovery to first oil.

---

## Objective

A development or project engineer opens the Development section and answers:
- **Where is my Field Development Plan in the approval process?**
- **Which wells are being designed or drilled right now?**
- **What milestones are upcoming? Am I on schedule and budget?**

---

## Pages to Build

### 3.1 Development Dashboard

**Route**: `/development`  
**File**: `Pages/PPDM39/Development/Dashboard.razor`  
**Process**: Project status overview for the development phase

**KPIs** (use `KpiCard`):
1. Project Phase (text badge: Pre-FID / Execution / Commissioning)
2. Wells Drilled / Total Planned
3. % Schedule Progress
4. Spend to Budget ratio (%)
5. Days to First Oil (estimated)

**Main sections**:
- Milestone Timeline (horizontal, MudTimeline or custom)
- Budget vs Actual chart (MudChart bar)
- Active Well Programs grid (mini status cards)
- Open Punch Items / Action list

**Quick Actions**:
- `[View FDP]` → `/development/fdp`
- `[Add Well to Program]` → `/development/well-design/new`

### Todo

| ID | Task | Status |
|----|------|--------|
| 3.1.a | Create `Dashboard.razor` page | 🔲 |
| 3.1.b | Add 5 KpiCard components | 🔲 |
| 3.1.c | Add project milestone horizontal timeline | 🔲 |
| 3.1.d | Add budget vs actual bar chart | 🔲 |
| 3.1.e | Add active wells mini-grid | 🔲 |
| 3.1.f | Wire to API: `GET /api/development/dashboard-summary?fieldId=` | 🔲 |

---

### 3.2 FDP (Field Development Plan) Wizard

**Route**: `/development/fdp` and `/development/fdp/new`  
**File**: `Pages/PPDM39/Development/FDPWizard.razor`  
**Process**: Create and manage the Field Development Plan document lifecycle  
**Pattern**: MudStepper (5 steps) + document status tracker

**When a FDP already exists**: Show the existing FDP with its current approval status and `ProcessTimeline`. Actions: `[Revise FDP]`, `[View Document]`, `[Track Regulator Response]`.

**When creating new FDP** (wizard mode):

| Step | Name | Content |
|------|------|---------|
| 1 | Reservoir Summary | Reservoir description, STOIIP/GIIP, recovery factor, reserves classification |
| 2 | Development Concept | Concept options considered, selected concept rationale, facility type |
| 3 | Well Program | Number of wells, types (producer/injector), phasing schedule |
| 4 | Economics | Capital cost, opex, NPV, IRR, breakeven price, risked economics |
| 5 | Regulatory Plan | Jurisdiction, required permits, submission timeline, approval track record |

**Post-wizard**: FDP document record created. Status workflow:
```
Draft → Technical Review → Management Review → Regulatory Submission → Approved → Active
```

Each status change triggers an email notification (placeholder for now).

### Todo

| ID | Task | Status |
|----|------|--------|
| 3.2.a | Create `FDPWizard.razor` page | 🔲 |
| 3.2.b | Add view mode for existing FDP with status timeline | 🔲 |
| 3.2.c | Step 1: Reservoir summary form | 🔲 |
| 3.2.d | Step 2: Concept selection with options comparison table | 🔲 |
| 3.2.e | Step 3: Well program table (add/remove rows) | 🔲 |
| 3.2.f | Step 4: Economics summary form + key metrics display | 🔲 |
| 3.2.g | Step 5: Regulatory plan form | 🔲 |
| 3.2.h | Status change buttons per current state | 🔲 |
| 3.2.i | Wire to API: `POST /api/development/fdp` | 🔲 |

---

### 3.3 Development Well Design Workflow

**Route**: `/development/well-design` (list) and `/development/well-design/{id}` (detail)  
**File**: `Pages/PPDM39/Development/WellDesign.razor`  
**Process**: Engineer designs and approves a development well before drilling

**List view**: `MudDataGrid` of all development wells showing:
- Well name, type (producer/injector/observation), target zone, status, AFE cost, rig assigned

**Detail view** (tabs):
1. **Well Design** — Location (coordinates), target depth, trajectory type (vertical/deviated/horizontal), casing program table
2. **Drilling Program** — Mud program, formation tops, BHA design summary, time estimate
3. **Completion Design** — Perforations, gravel pack, artificial lift type, tubing design
4. **AFE** — Cost breakdown (drilling/completion/testing), total AFE
5. **Progress** — Real-time drilling status when active: current depth, days vs plan, NPT log

**Actions from detail**:
- `[Submit for Approval]` — sends to approver
- `[Assign Rig]` — opens rig selection dialog
- `[Record Spud]` — marks well as active/drilling, captures spud date
- `[Record TD]` — captures total depth reached
- `[Complete Well]` — moves to completion tracking

### Todo

| ID | Task | Status |
|----|------|--------|
| 3.3.a | Create `WellDesign.razor` list page | 🔲 |
| 3.3.b | Create well detail tabs (5 tabs) | 🔲 |
| 3.3.c | Add casing program editable table | 🔲 |
| 3.3.d | Add AFE cost breakdown table | 🔲 |
| 3.3.e | Add drilling progress section | 🔲 |
| 3.3.f | Add action buttons with state-based visibility | 🔲 |
| 3.3.g | Wire to API: `GET /api/development/wells?fieldId=` | 🔲 |
| 3.3.h | Wire to API: `GET /api/development/wells/{id}` | 🔲 |

---

### 3.4 Construction Progress Tracker

**Route**: `/development/construction`  
**File**: `Pages/PPDM39/Development/ConstructionProgress.razor`  
**Process**: Track facility construction, installation, and commissioning milestones

**Layout** (two sections):

**Top — Project Health Banner**:
```
Schedule: ● On Track    Budget: ⚠ 3% Over    Safety: ● Zero incidents    Quality: ● 98% punch items closed
```

**Main — Milestone Tracker**:

Custom `MilestoneTracker` component displaying work packages:

```
PLATFORM / FACILITY
  ✅ Engineering Complete          Jan 2025
  ✅ Procurement Complete          Jun 2025
  🔄 Fabrication (68% complete)  Target: Mar 2026
  ○  Installation                Target: Aug 2026
  ○  Hook-up & Commissioning     Target: Nov 2026

WELLS
  ✅ Well 1 Drilled & Completed   Feb 2026
  🔄 Well 2 Drilling (at 3,200m) Target: Jun 2026
  ○  Well 3 Planned               Target: Sep 2026

FIRST OIL ● Target: Dec 2026
```

**Side panel — Open Actions**:
- Punch items list with owners and due dates
- Cost variance alerts
- Safety observation count

### Todo

| ID | Task | Status |
|----|------|--------|
| 3.4.a | Create `ConstructionProgress.razor` page | 🔲 |
| 3.4.b | Create `MilestoneTracker.razor` component | 🔲 |
| 3.4.c | Add project health banner with color-coded indicators | 🔲 |
| 3.4.d | Add open punch items side panel | 🔲 |
| 3.4.e | Add cost tracker (budget vs actual bar chart) | 🔲 |
| 3.4.f | Wire to API: `GET /api/development/construction-progress?fieldId=` | 🔲 |

---

## Definition of Done — Phase 3

- [ ] All 4 Development pages exist and build cleanly
- [ ] FDP Wizard has all 5 steps with MudStepper
- [ ] Well Design shows list + detail with 5 tabs
- [ ] Construction Progress shows milestone tree with status icons
- [ ] No inline `style=""` in any new file
- [ ] All routes work from the NavMenu
- [ ] `dotnet build` passes with 0 errors
