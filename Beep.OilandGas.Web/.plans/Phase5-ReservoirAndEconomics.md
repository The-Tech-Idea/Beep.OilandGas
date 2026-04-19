# Phase 5 — Reservoir & Economics UX

> **Status**: 🔲 Not Started  
> **Depends on**: Phase 1 (Foundation), Phase 4 (Production — for shared components)  
> **Duration**: ~2 weeks  
> **Reference Process**: `Plans/BusinessProcessesPlan/PetroleumEngineerBusinessProcesses.md` — Parts 5 & 8  
> **Goal**: Build the reservoir engineering and economic analysis sections as decision-support tools, not data viewers.

---

## Objective

A reservoir engineer uses this section to:
- Summarize reservoir characterization knowledge
- Screen EOR options
- Classify and report reserves

An asset manager uses the Economics section to:
- Manage AFE approvals
- Run economic evaluations

---

## Pages to Build

### 5.1 Reservoir Characterization Summary

**Route**: `/reservoir`  
**File**: `Pages/PPDM39/Reservoir/CharacterizationSummary.razor`  
**Process**: Consolidated view of reservoir knowledge — single source of truth for the reservoir team

**KPIs** (use `KpiCard`):
1. STOIIP (MMbbl) — P50
2. GIIP (Bscf) — P50
3. Recovery Factor (%)
4. Reservoir Pressure (psia) — current
5. Fluid Contacts (OWC, GOC depth)

**Main sections** (MudTabs):

1. **Rock Properties** — Porosity, permeability distribution, net-to-gross. Summary table + histogram charts
2. **Fluid Properties** — API gravity, GOR, FVF, viscosity, saturation pressures. Summary table
3. **Drive Mechanism** — Identified drive (solution gas / water drive / gas cap / compaction). Text + supporting data
4. **Aquifer** — Aquifer size indicator, influx history chart
5. **Well Correlation** — Cross-section schematic (static SVG or data table showing formation tops by well)
6. **Data Quality** — Completeness indicator for each data category (% wells with logs, % wells with core, etc.)

**Actions**:
- `[Update STOIIP Estimate]` → opens volume estimation form
- `[Screen for EOR]` → navigates to EOR Screening page (pre-filled with current properties)
- `[Estimate Reserves]` → navigates to Reserves Classification page

### Todo

| ID | Task | Status |
|----|------|--------|
| 5.1.a | Create `CharacterizationSummary.razor` page | 🔲 |
| 5.1.b | Add 5 KpiCard components | 🔲 |
| 5.1.c | Add 6-tab content sections | 🔲 |
| 5.1.d | Tab 1: Rock properties summary + chart | 🔲 |
| 5.1.e | Tab 2: Fluid properties table | 🔲 |
| 5.1.f | Tab 3-6: Content sections | 🔲 |
| 5.1.g | Add action buttons | 🔲 |
| 5.1.h | Wire to API: `GET /api/reservoir/characterization?fieldId=` | 🔲 |

---

### 5.2 EOR Screening Tool

**Route**: `/reservoir/eor`  
**File**: `Pages/PPDM39/Reservoir/EORScreening.razor`  
**Process**: Evaluate which enhanced oil recovery methods are technically applicable

**Layout** (two panels):

**Left — Reservoir Properties Input**:
```
Depth (ft):           ___________
Temperature (°F):     ___________
Pressure (psia):      ___________
Porosity (%):         ___________
Permeability (mD):    ___________
API Gravity:          ___________
Viscosity (cp):       ___________
Water Saturation (%): ___________
```

Auto-populated from Characterization Summary; editable for "what-if" scenarios.

**Right — Screening Results**:

| EOR Method | Technical Fit | Key Constraint | Rating |
|-----------|---------------|----------------|--------|
| Waterflood | ✅ Excellent | None | ★★★★★ |
| CO₂ Flood | ✅ Good | CO₂ availability | ★★★★☆ |
| Polymer Flood | ⚠️ Possible | High permeability required | ★★★☆☆ |
| Steam Injection | ❌ Not applicable | API too high | ★☆☆☆☆ |
| SAGD | ❌ Not applicable | API too high | ★☆☆☆☆ |

**Color coding**: Green = excellent, Amber = possible with conditions, Red = not applicable

**Actions**:
- `[Run Sensitivity]` — change one parameter and see rating change
- `[Create EOR Pilot Proposal]` — for the top-rated applicable method; opens a form

### Todo

| ID | Task | Status |
|----|------|--------|
| 5.2.a | Create `EORScreening.razor` page | 🔲 |
| 5.2.b | Add reservoir properties input panel (left) | 🔲 |
| 5.2.c | Add screening results matrix (right) | 🔲 |
| 5.2.d | Implement client-side screening criteria logic (lookup table) | 🔲 |
| 5.2.e | Add color-coded rating column | 🔲 |
| 5.2.f | Add sensitivity toggle (change one parameter, auto-refresh results) | 🔲 |
| 5.2.g | Add EOR Pilot Proposal action | 🔲 |
| 5.2.h | Wire auto-populate from API: `GET /api/reservoir/properties?fieldId=` | 🔲 |

---

### 5.3 Reserves Classification Page

**Route**: `/reservoir/reserves`  
**File**: `Pages/PPDM39/Reservoir/ReservesClassification.razor`  
**Process**: Classify reserves per SPE-PRMS / SEC standards and track over time

**Layout**:

**Top — Reserves Summary (waterfall visual)**:
```
STOIIP: 450 MMbbl
  └─ Recovery Factor: 38%
      └─ EUR: 171 MMbbl
          ├─ 1P (Proved):           82 MMbbl  ██████████░░░░░░
          ├─ 2P (Proved+Probable): 134 MMbbl  ████████████████░░
          └─ 3P (+Possible):       171 MMbbl  ████████████████████
```

**Main content** (tabs):

1. **Current Estimate** — Input form for 1P/2P/3P by product type (oil/gas/condensate)
2. **Certification History** — Table of all reserve estimates by year and certifier
3. **Reconciliation** — Production removed, revisions, extensions (SPE reserve reconciliation format)
4. **Compliance** — Which standards this satisfies (SEC 17 CFR 210.4-10, SPE-PRMS, local regulator)

**Actions**:
- `[Submit Reserves Estimate]` — saves and logs as official estimate with timestamp and user
- `[Request Certification]` — creates a certification request (sends to certifier role)
- `[Export to SEC Report Format]` — generates structured export

### Todo

| ID | Task | Status |
|----|------|--------|
| 5.3.a | Create `ReservesClassification.razor` page | 🔲 |
| 5.3.b | Add reserves waterfall summary chart (MudChart stacked bar) | 🔲 |
| 5.3.c | Tab 1: 1P/2P/3P input form by product | 🔲 |
| 5.3.d | Tab 2: Certification history table | 🔲 |
| 5.3.e | Tab 3: SPE reconciliation format table | 🔲 |
| 5.3.f | Tab 4: Standards compliance checklist | 🔲 |
| 5.3.g | Add submit/certification/export actions | 🔲 |
| 5.3.h | Wire to API: `GET /api/reservoir/reserves?fieldId=` | 🔲 |
| 5.3.i | Wire to API: `POST /api/reservoir/reserves/estimates` | 🔲 |

---

### 5.4 AFE Management

**Route**: `/economics/afe`  
**File**: `Pages/PPDM39/Economics/AFEManagement.razor`  
**Process**: Manage capital expenditure authorizations through approval workflow

**List view** — AFE dashboard table:

| AFE# | Description | Amount ($K) | Status | Requested By | Due Date |
|------|-------------|-------------|--------|-------------|----------|
| AFE-2026-001 | Well W-15 Drilling | 4,500 | ● Approved | J.Smith | — |
| AFE-2026-002 | ESP Replacement W-03 | 380 | ⚠️ Pending L2 | M.Jones | Apr 30 |
| AFE-2026-003 | Platform Inspection | 125 | 🔴 Overdue | K.Lee | Apr 15 |

**Create New AFE** (drawer or dialog):
- AFE number (auto-generated)
- Description + scope
- Cost breakdown table (line items: material, labor, services, contingency)
- Total amount
- Approval authority level (auto-determined from amount vs. authority matrix)
- Supporting documents upload

**Detail view** (click AFE row):
- Status timeline (ProcessTimeline component)
- Cost breakdown (pie chart by category)
- Approval history (who approved/rejected and when)
- Actual vs. AFE cost when committed/closed

### Todo

| ID | Task | Status |
|----|------|--------|
| 5.4.a | Create `AFEManagement.razor` page with list view | 🔲 |
| 5.4.b | Add New AFE drawer/dialog with cost breakdown table | 🔲 |
| 5.4.c | Add detail view with ProcessTimeline and cost pie chart | 🔲 |
| 5.4.d | Add approval action buttons (Approve/Reject by level) | 🔲 |
| 5.4.e | Wire to API: `GET /api/economics/afes?fieldId=` | 🔲 |
| 5.4.f | Wire to API: `POST /api/economics/afes` | 🔲 |
| 5.4.g | Wire to API: `POST /api/economics/afes/{id}/approve` | 🔲 |

---

### 5.5 Economic Evaluation Workbench

**Route**: `/economics/evaluation`  
**File**: `Pages/PPDM39/Economics/EconomicEvaluation.razor`  
**Process**: Run economic models for development decisions, AFE justification, and portfolio ranking

**Layout** (three-column on desktop):

```
LEFT — Inputs          CENTRE — Results          RIGHT — Scenarios
┌─────────────────┐    ┌─────────────────────┐    ┌─────────────────┐
│ Production      │    │ NPV: $42.5M         │    │ Scenario A (Base│
│ forecast import │    │ IRR: 18.4%          │    │ Scenario B (+10%│
│                 │    │ Payback: 4.2 yrs    │    │   oil price)    │
│ CAPEX ($):      │    │ PI: 1.42            │    │ Scenario C (DCA │
│                 │    ├─────────────────────┤    │   optimistic)   │
│ OPEX ($/bbl):   │    │ Cash Flow Chart     │    │                 │
│                 │    │ (waterfall or area  │    │ [Compare All]   │
│ Oil Price Deck: │    │ chart)              │    └─────────────────┘
│ [Flat / Strip/  │    ├─────────────────────┤
│  Custom]        │    │ Sensitivity Tornado │
│                 │    │ (price / costs /    │
│ Discount Rate:  │    │ production)         │
│ 10 %            │    └─────────────────────┘
│                 │
│ [Run Model]     │
└─────────────────┘
```

**Key charts**:
- Annual cash flow waterfall (pre- and post-tax)
- NPV sensitivity tornado chart (5 variables)
- Cumulative production and cumulative cash flow on same axis

**Scenario management**: Save up to 5 named scenarios; compare side-by-side in a summary table.

### Todo

| ID | Task | Status |
|----|------|--------|
| 5.5.a | Create `EconomicEvaluation.razor` page | 🔲 |
| 5.5.b | Add 3-column layout (inputs / results / scenarios) | 🔲 |
| 5.5.c | Add price deck selector (Flat/Strip/Custom) | 🔲 |
| 5.5.d | Add NPV/IRR/Payback/PI results display | 🔲 |
| 5.5.e | Add cash flow chart (MudChart) | 🔲 |
| 5.5.f | Add sensitivity tornado chart | 🔲 |
| 5.5.g | Add scenario save/compare functionality | 🔲 |
| 5.5.h | Wire to API: `POST /api/economics/evaluate` | 🔲 |

---

## Definition of Done — Phase 5

- [ ] Reservoir Characterization shows KPIs + 6 tabs with real structure
- [ ] EOR Screening shows screening matrix with color-coded ratings
- [ ] Reserves Classification shows 1P/2P/3P with waterfall chart
- [ ] AFE Management shows approval workflow with status timeline
- [ ] Economic Evaluation has 3-column layout with NPV metrics + charts
- [ ] No inline `style=""` in any new file
- [ ] All routes work from the NavMenu
- [ ] `dotnet build` passes with 0 errors
