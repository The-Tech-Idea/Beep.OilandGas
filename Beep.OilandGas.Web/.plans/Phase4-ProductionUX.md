# Phase 4 — Production Business Process UX

> **Status**: 🔲 Not Started  
> **Depends on**: Phase 1 (Foundation & Navigation)  
> **Duration**: ~3 weeks  
> **Reference Process**: `Plans/BusinessProcessesPlan/PetroleumEngineerBusinessProcesses.md` — Part 4  
> **Goal**: Build the production section as a production engineer's daily work environment. This is the most heavily used section of the application.

---

## Objective

A production engineer opens the Production section and immediately answers:
- **Is the field producing on target today?**
- **Are any wells down or underperforming?**
- **What actions are needed right now?**

Every sub-page flows from that same operational mindset.

---

## Pages to Build

### 4.1 Production Daily Operations Center

**Route**: `/production`  
**File**: `Pages/PPDM39/Production/DailyOpsCenter.razor`  
**Process**: Daily production monitoring and field operations management  
**This is the P0 priority page — most visited by production engineers**

**Layout**:
```
┌─────────────────────────────────────────────────────────────────┐
│  Daily Operations  ●  [Field Name]  ●  [Today's Date]          │
├───────┬───────┬───────┬───────┬───────┐                         │
│Total  │Oil    │Gas    │Water  │Uptime │  ← KPI row (KpiCard x5) │
│Wells  │bbl/d  │mmscf/d│bbl/d  │%      │                         │
│ 24    │ 1,240 │  8.3  │  980  │ 96%   │                         │
├───────┴───────┴───────┴───────┴───────┤                         │
│ 🔴 Alarm (2)  ⚠️ Alerts (5)            │  Today vs Target: ▬▬▬▬  │
│ Well W-14: Shut-in (ESP failure)      │  [📊 vs Target Chart]   │
│ Well W-08: Low rate (-40%)            │                         │
├───────────────────────────────────────┴─────────────────────────┤
│ Well Status Grid                                                 │
│ [W-01 ●Green][W-02 ●Green][W-03 ⚠️ Amber][W-04 ●Green]...       │
│ Click any well to open Well Performance page                     │
├─────────────────────────────────────────────────────────────────┤
│ [+ Log Deferment]  [📋 Daily Report]  [🔧 Raise Work Order]     │
└─────────────────────────────────────────────────────────────────┘
```

**Well Status Grid**: Shows all wells as color-coded cards.
- Green = online, producing on target
- Amber = online but underperforming (>10% below test rate)
- Red = shut-in or alarm
- Grey = not yet on-stream

**Deferment Logger** (dialog, opens from `[+ Log Deferment]`):
- Well selector (multi-select)
- Deferment type: Planned / Unplanned
- Reason code (LOV: ESP failure, choke change, export constraint, etc.)
- Estimated volume deferred (auto-calculated from rate × hours)
- Expected return-to-production date

### Todo

| ID | Task | Status |
|----|------|--------|
| 4.1.a | Create `DailyOpsCenter.razor` page | 🔲 |
| 4.1.b | Add 5 KpiCard components (total wells, oil, gas, water, uptime) | 🔲 |
| 4.1.c | Add Alarms & Alerts panel | 🔲 |
| 4.1.d | Add Today vs Target bar chart | 🔲 |
| 4.1.e | Create `WellStatusCard.razor` component | 🔲 |
| 4.1.f | Build well status grid (all wells, color-coded) | 🔲 |
| 4.1.g | Create `DefermentLogger.razor` dialog component | 🔲 |
| 4.1.h | Add Daily Report button (generates summary) | 🔲 |
| 4.1.i | Wire to API: `GET /api/production/daily-summary?fieldId=&date=` | 🔲 |
| 4.1.j | Wire to API: `GET /api/production/well-status?fieldId=` | 🔲 |
| 4.1.k | Wire deferment to API: `POST /api/production/deferments` | 🔲 |

---

### 4.2 Well Performance Optimizer

**Route**: `/production/well-performance` (list) and `/production/well-performance/{uwi}` (detail)  
**File**: `Pages/PPDM39/Production/WellPerformance.razor`  
**Process**: Monitor individual well performance against expectations and optimize

**List view** — Multi-well comparison table:

| Well | Current Rate | Test Rate | Variance | Decline Rate | Last Test | Action |
|------|-------------|-----------|----------|--------------|-----------|--------|
| W-01 | 1,250 bbl/d | 1,300 | -3.8% | 8%/yr | 15 Mar | Monitor |
| W-03 | 620 bbl/d | 900 | **-31%** ⚠️ | 22%/yr | 02 Apr | **Investigate** |
| W-07 | 180 bbl/d | 500 | **-64%** 🔴 | 45%/yr | 01 Jan | **Work Over** |

Color coding: >-10% variance = amber, >-25% = red.
Sorted by worst performers first by default.

**Detail view** (well-level):

**Header**: Well name, current status badge, UWI, location, completion type

**Tabs**:
1. **Performance** — Production trend chart (oil/gas/water/pressure), actual vs. expected overlay, DCA curve fit
2. **Well Tests** — Test history table, latest test data, test vs. allocated rates
3. **Diagnostic** — Pressure buildup plot, PI (productivity index) trend, skin calculation
4. **Interventions** — History of workovers, stimulations, AL changes with results
5. **Recommendations** — Rule-based recommendations: "Well is a candidate for acid stimulation based on declining PI"

**Action buttons** (right side panel):
- `[Schedule Well Test]` → opens test scheduling form
- `[Raise Intervention AFE]` → pre-fills intervention decision tool
- `[Change Artificial Lift]` → opens AL optimization dialog
- `[Shut-in Well]` → requires reason + approval workflow

### Todo

| ID | Task | Status |
|----|------|--------|
| 4.2.a | Create `WellPerformance.razor` list page with comparison table | 🔲 |
| 4.2.b | Add variance calculation and color coding to list | 🔲 |
| 4.2.c | Create well detail page with 5 tabs | 🔲 |
| 4.2.d | Tab 1: Production trend chart (MudChart — line, multi-series) | 🔲 |
| 4.2.e | Tab 2: Well test history grid | 🔲 |
| 4.2.f | Tab 3: Diagnostic indicators (PI, skin, pressure) | 🔲 |
| 4.2.g | Tab 4: Intervention history timeline | 🔲 |
| 4.2.h | Tab 5: Recommendations panel (rule-based text for now) | 🔲 |
| 4.2.i | Add context-sensitive action button panel (right side) | 🔲 |
| 4.2.j | Wire to API: `GET /api/production/well-performance?fieldId=` | 🔲 |
| 4.2.k | Wire to API: `GET /api/production/wells/{uwi}/performance` | 🔲 |

---

### 4.3 Well Intervention Decision Tool

**Route**: `/production/intervention`  
**File**: `Pages/PPDM39/Production/InterventionDecision.razor`  
**Process**: Evaluate and prioritize well intervention candidates; raise AFE  
**Pattern**: Decision support tool with ranked candidate list

**Layout**:
```
┌─────────────────────────────────────────────────────────────────┐
│  Intervention Candidates  ●  [Field Name]                      │
├───────────────────────────────────────────────────────────────── │
│  Filter: [All Types ▼]  [Sort: NPV Uplift ▼]  [Add Candidate]  │
├───────────────────────────────────────────────────────────────── │
│  Rank │ Well  │ Issue          │ Intervention    │ Est. Uplift │ NPV    │ Status      │
│   1   │ W-07  │ Scale buildup  │ Acid stimulation│ +320 bbl/d  │ $450K  │ Recommended │
│   2   │ W-03  │ ESP failure    │ ESP replacement │ +280 bbl/d  │ $380K  │ Pending AFE │
│   3   │ W-11  │ Paraffin plug  │ Hot oil treatment│ +90 bbl/d  │ $85K   │ Monitor     │
├───────────────────────────────────────────────────────────────── │
│  [Select Wells for Work Order Package]  [Generate AFE Package]  │
└─────────────────────────────────────────────────────────────────┘
```

**Detail panel** (click a row → expand or side drawer):
- Well issue description + evidence (chart showing decline)
- Proposed intervention type and rationale
- NPV calculation inputs (uplift rate × oil price × duration − cost)
- Risk level (low/medium/high)
- Historical success rate for this intervention type on this well/field

**Actions**:
- `[Generate AFE]` — creates AFE record pre-filled with intervention details
- `[Add to Work Order Queue]` — batches with other interventions for rig scheduling
- `[Defer — Next Review Date]` — removes from active list until that date
- `[Drop Candidate]` — mark as uneconomic with reason

### Todo

| ID | Task | Status |
|----|------|--------|
| 4.3.a | Create `InterventionDecision.razor` page | 🔲 |
| 4.3.b | Add ranked candidates table with sort/filter | 🔲 |
| 4.3.c | Add expandable detail panel per candidate | 🔲 |
| 4.3.d | Add NPV calculation display | 🔲 |
| 4.3.e | Add action buttons (Generate AFE, Add to Queue, Defer, Drop) | 🔲 |
| 4.3.f | Wire to API: `GET /api/production/intervention-candidates?fieldId=` | 🔲 |
| 4.3.g | Wire to API: `POST /api/production/interventions/{id}/afe` | 🔲 |

---

### 4.4 Production Allocation Workbench

**Route**: `/production/allocation`  
**File**: `Pages/PPDM39/Production/AllocationWorkbench.razor`  
**Process**: Monthly production allocation — distribute measured field volumes to individual wells

**Workflow** (steps via top stepper bar):
```
[1. Enter Meter Data] → [2. Apply Corrections] → [3. Choose Method] → [4. Validate Balance] → [5. Finalize & Report]
```

**Step 1 — Meter Data Entry**: 
- Table with metering stations, daily readings for the month
- Import from CSV option

**Step 2 — Corrections**:
- Shrinkage factors, BS&W correction, flare/vent deduction
- Show corrected net volumes vs. gross

**Step 3 — Allocation Method**:
- Method selector: Proportional / Well Test Based / Simulation Based
- Show allocation split preview by well

**Step 4 — Balance Check**:
- Volume balance table: Metered vs. Allocated (should reconcile to <0.1%)
- Color highlight any out-of-tolerance wells
- Must be green before Step 5 unlocks

**Step 5 — Finalize**:
- Generate monthly allocation report (PDF/Excel)
- Submit to regulatory system button
- Lock period (prevents edits after submission)

### Todo

| ID | Task | Status |
|----|------|--------|
| 4.4.a | Create `AllocationWorkbench.razor` page | 🔲 |
| 4.4.b | Add 5-step stepper bar | 🔲 |
| 4.4.c | Step 1: Meter data entry table | 🔲 |
| 4.4.d | Step 2: Correction factors form | 🔲 |
| 4.4.e | Step 3: Method selector with preview | 🔲 |
| 4.4.f | Step 4: Volume balance validation table | 🔲 |
| 4.4.g | Step 5: Report generation + lock period | 🔲 |
| 4.4.h | Wire to API: `GET /api/production/allocation?fieldId=&period=` | 🔲 |
| 4.4.i | Wire to API: `POST /api/production/allocation/finalize` | 🔲 |

---

### 4.5 Production Forecasting Workbench

**Route**: `/production/forecasting`  
**File**: `Pages/PPDM39/Production/ForecastingWorkbench.razor`  
**Process**: Build production forecasts using DCA and scenario analysis

**Layout**:
```
LEFT (2/3): Forecast Chart
  - Historical production (solid line, last 24 months)
  - P50 forecast (solid, primary color)
  - P10/P90 envelope (shaded area)
  - Individual well contributions (stacked area option)

RIGHT (1/3): Scenario Controls
  - Decline type: Exponential / Hyperbolic / Harmonic
  - b-factor (for hyperbolic)
  - Initial decline rate
  - Scenario: Base / Optimistic / Conservative
  - Workover schedule (add planned workovers → bump production)
  - New well additions (add planned wells to forecast)
```

**Bottom panel** — Forecast Summary Table:
| Year | Oil (Mbbl) | Gas (MMscf) | Water (Mbbl) | P10 | P50 | P90 |
|------|-----------|-------------|--------------|-----|-----|-----|

**Actions**:
- `[Save Forecast]` — saves as a named forecast version
- `[Export to Budget]` — sends P50 volumes to budget module
- `[Compare Scenarios]` — overlay multiple saved forecasts

### Todo

| ID | Task | Status |
|----|------|--------|
| 4.5.a | Create `ForecastingWorkbench.razor` page | 🔲 |
| 4.5.b | Add forecast chart (MudChart with multi-series) | 🔲 |
| 4.5.c | Add DCA parameter controls (right panel) | 🔲 |
| 4.5.d | Add scenario selector (P10/P50/P90) | 🔲 |
| 4.5.e | Add workover / new well schedule table | 🔲 |
| 4.5.f | Add forecast summary annual table | 🔲 |
| 4.5.g | Add save / export / compare actions | 🔲 |
| 4.5.h | Wire to API: `GET /api/production/history?fieldId=&months=` | 🔲 |
| 4.5.i | Wire to API: `POST /api/production/forecasts` | 🔲 |

---

## Definition of Done — Phase 4

- [ ] Daily Ops Center shows live well status grid with color-coded cards
- [ ] Deferment Logger dialog works with all required fields
- [ ] Well Performance list shows ranked by worst variance
- [ ] Well Performance detail has all 5 tabs
- [ ] Intervention Decision shows NPV-ranked candidate list
- [ ] Allocation Workbench has 5-step stepper, Step 4 blocks Step 5 if unbalanced
- [ ] Forecasting Workbench shows P10/P50/P90 chart
- [ ] No inline `style=""` in any new file
- [ ] All routes work from the NavMenu
- [ ] `dotnet build` passes with 0 errors
