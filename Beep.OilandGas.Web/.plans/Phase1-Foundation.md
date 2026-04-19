# Phase 1 — Foundation & Navigation

> **Status**: 🔲 Not Started  
> **Depends on**: Nothing (first phase)  
> **Duration**: ~1 week  
> **Goal**: Establish the process-centric navigation structure, shared UI components, SVG icon library, and update copilot guidelines before any business process pages are built.

---

## Objective

Before building any business process pages, establish the foundation:
1. A petroleum engineer-centric navigation menu
2. Reusable shared components (KpiCard, StatusBadge, ProcessTimeline)
3. SVG icon library for O&G domain concepts
4. Updated copilot instructions with UI/UX rules

---

## 1.1 NavMenu Redesign

**File**: `Beep.OilandGas.Web/Shared/NavMenu.razor`

**Current state**: Horizontal menu with generic section names (Field Management, Facility Management, Reservoir Management, Process & Analysis).

**Target state**: Process-centric menu reflecting the petroleum engineer's workflow sequence:

```
[🏠 Dashboard] [🔍 Exploration] [🏗 Development] [⚙️ Production] [📊 Reservoir] [💰 Economics] [🛡 HSE & Compliance] [🗄 Data Management]
```

Each menu item:
- Links to a **business process dashboard** for that domain
- Has a submenu with the key **workflow pages** (not CRUD lists)
- Uses SVG icons (domain concepts) or `Icons.Material.*` (standard UI)

### NavMenu Structure Spec

```
Dashboard            → /dashboard
Exploration          → /exploration
  ├─ Exploration Dashboard    → /exploration
  ├─ Prospect Maturation Board→ /exploration/prospect-board
  ├─ Well Program Approval    → /exploration/well-program
  └─ Seismic Surveys          → /exploration/seismic
Development          → /development
  ├─ Development Dashboard    → /development
  ├─ Field Development Plan   → /development/fdp
  ├─ Well Design              → /development/well-design
  └─ Construction Progress    → /development/construction
Production           → /production
  ├─ Daily Operations Center  → /production
  ├─ Well Performance         → /production/well-performance
  ├─ Intervention Decisions   → /production/intervention
  ├─ Allocation Workbench     → /production/allocation
  └─ Production Forecasting   → /production/forecasting
Reservoir            → /reservoir
  ├─ Reservoir Overview       → /reservoir
  ├─ Characterization         → /reservoir/characterization
  ├─ EOR Screening            → /reservoir/eor
  └─ Reserves Classification  → /reservoir/reserves
Economics            → /economics
  ├─ AFE Management           → /economics/afe
  └─ Economic Evaluation      → /economics/evaluation
HSE & Compliance     → /hse
  ├─ Incident Management      → /hse/incidents
  └─ Compliance Calendar      → /hse/compliance
Data Management      → /ppdm39/data-management  (PPDMTreeView — unchanged)
```

### Todo

| ID | Task | Status | Notes |
|----|------|--------|-------|
| 1.1.a | Remove current menu items from NavMenu.razor | 🔲 | Keep file structure |
| 1.1.b | Add Dashboard menu item | 🔲 | Links to /dashboard |
| 1.1.c | Add Exploration menu with 4 sub-items | 🔲 | Routes per spec above |
| 1.1.d | Add Development menu with 4 sub-items | 🔲 | |
| 1.1.e | Add Production menu with 5 sub-items | 🔲 | |
| 1.1.f | Add Reservoir menu with 4 sub-items | 🔲 | |
| 1.1.g | Add Economics menu with 2 sub-items | 🔲 | |
| 1.1.h | Add HSE & Compliance menu with 2 sub-items | 🔲 | |
| 1.1.i | Add Data Management menu (PPDM TreeView route) | 🔲 | Must be clearly separated |

---

## 1.2 SVG Icon Library

**Directory**: `wwwroot/imgs/icons/process/`

Create 6 core SVG icons for petroleum engineer domain concepts not covered by Material Icons:

| File | Concept | Description |
|------|---------|-------------|
| `exploration.svg` | Exploration phase | Magnifying glass over geological layers |
| `drilling-rig.svg` | Drilling rig | Simple onshore derrick silhouette |
| `wellhead.svg` | Wellhead / Christmas tree | Valve assembly top view |
| `production.svg` | Production / flowing well | Oil drop with upward arrow |
| `reservoir.svg` | Underground reservoir | Porous rock layer with oil |
| `decommission.svg` | Decommissioning | Rig with X or strikethrough |

**Rules**:
- ViewBox: `0 0 24 24` (Material Icon compatible size)
- Stroke-based (not fill-based) — allows CSS color theming
- No embedded colors (use `currentColor`)
- Accessible: include `<title>` element inside SVG

### Todo

| ID | Task | Status |
|----|------|--------|
| 1.2.a | Create `exploration.svg` | 🔲 |
| 1.2.b | Create `drilling-rig.svg` | 🔲 |
| 1.2.c | Create `wellhead.svg` | 🔲 |
| 1.2.d | Create `production.svg` | 🔲 |
| 1.2.e | Create `reservoir.svg` | 🔲 |
| 1.2.f | Create `decommission.svg` | 🔲 |
| 1.2.g | Add `.og-icon` CSS class to `wwwroot/css/app.css` | 🔲 |

---

## 1.3 Shared Components

**Directory**: `Beep.OilandGas.Web/Components/Shared/`

### KpiCard.razor

A reusable KPI card component used on all dashboards.

**Parameters**:
```csharp
[Parameter] string Label { get; set; }         // "Daily Production"
[Parameter] string Value { get; set; }         // "1,250"
[Parameter] string Unit { get; set; }          // "bbl/d"
[Parameter] Color ValueColor { get; set; }     // Color.Success / Warning / Error
[Parameter] string TrendIcon { get; set; }     // Icons.Material.Filled.TrendingUp
[Parameter] Color TrendColor { get; set; }     // Color.Success
[Parameter] string TrendText { get; set; }     // "+5% vs yesterday"
[Parameter] string Icon { get; set; }          // Optional domain icon
```

### StatusBadge.razor

Consistent status chip used everywhere a status is displayed.

**Parameters**:
```csharp
[Parameter] string Status { get; set; }   // "Online", "Shut-in", "Testing", "Alarm"
[Parameter] Size Size { get; set; }       // Size.Small (default)
```

Internal mapping: `Status → Color + Icon` (centralized in one place).

### ProcessTimeline.razor

Renders a `MudTimeline` from a list of process steps.

**Parameters**:
```csharp
[Parameter] List<ProcessStep> Steps { get; set; }
[Parameter] int CurrentStepIndex { get; set; }
```

`ProcessStep` model:
```csharp
public record ProcessStep(string Name, string Date, bool IsCompleted, bool IsActive, string Icon = "");
```

### Todo

| ID | Task | File | Status |
|----|------|------|--------|
| 1.3.a | Create `KpiCard.razor` | `Components/Shared/KpiCard.razor` | 🔲 |
| 1.3.b | Create `KpiCard.razor.css` | `Components/Shared/KpiCard.razor.css` | 🔲 |
| 1.3.c | Create `StatusBadge.razor` | `Components/Shared/StatusBadge.razor` | 🔲 |
| 1.3.d | Create `ProcessTimeline.razor` | `Components/Shared/ProcessTimeline.razor` | 🔲 |
| 1.3.e | Register component namespace in `_Imports.razor` | `_Imports.razor` | 🔲 |

---

## 1.4 Update copilot-instructions.md

Add the UI/UX section to `.github/copilot-instructions.md`:

```markdown
## UI/UX Design Rules (Oil & Gas)

See `Plans/UI-UX-OilAndGas-Guidelines.md` for full guidelines.

### Quick Rules
- Business process pages simulate engineer workflows, NOT CRUD operations
- SVG only for new images — no PNG/JPG icons
- All pages show active field name context
- Use MudBlazor `Color.*` enum — never hardcode colors
- One business domain per page — split if title has "and"
- PPDMTreeView is for data managers — keep separate from engineer UX
```

| ID | Task | Status |
|----|------|--------|
| 1.4.a | Add UI/UX section to `.github/copilot-instructions.md` | 🔲 |

---

## 1.5 Update Field Dashboard

Update `Pages/PPDM39/FieldDashboard.razor` to use the new KpiCard components and match the dashboard pattern from guidelines §3.1.

| ID | Task | Status |
|----|------|--------|
| 1.5.a | Replace existing dashboard with KpiCard-based layout | 🔲 |
| 1.5.b | Add active field context badge to dashboard header | 🔲 |
| 1.5.c | Add "Today's Activity" section with quick-action buttons | 🔲 |

---

## Definition of Done — Phase 1

- [ ] NavMenu shows process-centric structure (8 top-level items)
- [ ] All sub-menu routes are defined (even if pages return placeholder)
- [ ] 6 SVG icons created in `wwwroot/imgs/icons/process/`
- [ ] `KpiCard`, `StatusBadge`, `ProcessTimeline` components exist and build
- [ ] `.github/copilot-instructions.md` updated with UI/UX rules
- [ ] `dotnet build` passes with 0 errors
