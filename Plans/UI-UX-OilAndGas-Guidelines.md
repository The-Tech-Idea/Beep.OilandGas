# Beep Oil & Gas — UI/UX Design Guidelines

> **Version**: 1.0 | **Date**: 2026-04-18  
> **Audience**: UI Developers, UX Designers, Copilot Agents  
> **Scope**: `Beep.OilandGas.Web` — all pages, components, navigation

---

## 1. Core Design Philosophy

### 1.1 Business Process First — Not CRUD First

> **Principle**: Every page simulates a petroleum engineer's real work process. A user comes to a page to accomplish a business objective, not to manage database records.

| ❌ CRUD Mindset | ✅ Process Mindset |
|---|---|
| "Add Well" form | "Initiate Well Program" wizard |
| "Edit Prospect" grid row | "Advance Prospect to Lead" workflow |
| "List Production Data" table | "Review Well Performance & Optimize" dashboard |
| "Delete Facility" button | "Initiate Facility Decommissioning" workflow |

**Rule**: If the page title is a noun (e.g. "Wells"), it is probably CRUD. If it is a verb phrase (e.g. "Optimize Well Performance"), it is a business process page.

### 1.2 Context-Aware Navigation

The active field/asset context must always be visible. Every page knows **which field** the user is working on. Navigation reflects the engineer's current lifecycle stage.

### 1.3 Progressive Disclosure

Show the most important information first. Reveal detail on demand. Use cards → drawers → dialogs as disclosure layers.

---

## 2. Navigation Structure

### 2.1 Top-Level Menu (Petroleum Engineer Workflow)

The main navigation **must** follow the petroleum engineer's natural work sequence:

```
[Field Dashboard] [Exploration] [Development] [Production] [Reservoir] [Economics & Reserves] [HSE & Compliance] [Data Management]
```

- **Field Dashboard**: KPI overview for the active field. Entry point for all daily work.
- **Exploration**: Prospect identification → maturation → well program decisions.
- **Development**: FDP → well design → construction → commissioning → first oil.
- **Production**: Daily monitoring → optimization → intervention → allocation.
- **Reservoir**: Characterization → simulation → EOR → reserves estimation.
- **Economics & Reserves**: AFE → economic models → reserves booking → reporting.
- **HSE & Compliance**: Incident management → permits → regulatory reporting.
- **Data Management**: PPDM TreeView for data managers and administrators (kept separate — see §2.3).

### 2.2 Page Hierarchy Within a Section

Each section follows this three-level hierarchy:

```
Section Dashboard (KPIs, alerts, recent activity)
  └─ Active Workflow (step-wizard or kanban board)
       └─ Detail / Decision Page
```

Example — Exploration section:
```
Exploration Dashboard
  ├─ Prospect Maturation Board (kanban)
  │    ├─ Prospect Detail & Screening
  │    └─ Lead-to-Drill Decision
  ├─ Well Program Approval (gate wizard)
  └─ Seismic Coverage Map
```

### 2.3 Data Management Separation

The **PPDM TreeView** and raw CRUD operations are deliberately isolated under **Data Management**. They are tools for data managers and administrators, not daily petroleum engineer workflows. Business process pages must NEVER link back to raw CRUD pages as primary actions.

---

## 3. Page Design Patterns

### 3.1 Dashboard Page Pattern

Every section starts with a dashboard. Required elements:

```
┌─────────────────────────────────────────────────────┐
│  [Section Name] Dashboard  ●  [Active Field Name]   │
├─────────┬─────────┬─────────┬─────────┐             │
│  KPI 1  │  KPI 2  │  KPI 3  │  KPI 4  │  ← MudGrid  │
├─────────┴─────────┼─────────────────────┤            │
│  Active Workflows │   Alerts & Actions  │            │
│  (list/kanban)   │   (due, overdue)    │            │
├───────────────────┴─────────────────────┤            │
│  Trend Chart (production / activity)   │            │
└─────────────────────────────────────────┘            │
```

**Required elements:**
- Current field name badge (always visible)
- 3–5 KPI cards (color-coded: green=good, amber=attention, red=critical)
- Active workflows / open items list
- Quick-action buttons for the most common next step
- Trend chart (MudChart — line or bar)

### 3.2 Workflow (Wizard) Page Pattern

Multi-step business processes use `MudStepper`:

```
Step 1: Data Collection → Step 2: Analysis → Step 3: Decision → Step 4: Approval → Step 5: Action
```

Rules:
- Maximum **5 steps** per wizard. Break longer workflows into sub-workflows.
- Each step has a **clear gate question**: "What decision does the engineer make here?"
- Steps show **completion status** with icon (✓ done, ● in-progress, ○ not started).
- The final step always produces a **record** (document, approval, work order).

### 3.3 Kanban / Lifecycle Board Pattern

Use for processes where items move through stages (e.g., Prospect Maturation):

```
[Leads]  →  [Prospects]  →  [Drill Decision]  →  [Active Wells]
   3             7               2                    12
```

Rules:
- Maximum **6 columns** (stages).
- Each card shows: name, key metric, age-in-stage indicator.
- Drag between columns triggers a **state transition dialog** (not silent update).
- Color-code cards by urgency/status.

### 3.4 Decision Page Pattern

Gate reviews, intervention decisions, AFE approvals:

```
┌─────────────────────────────────────────────┐
│  Decision: [Name]                           │
│  Context: [Key facts that drive decision]   │
├─────────────────────────────────────────────┤
│  Supporting Data  │  Recommendation         │
│  (charts, tables) │  (AI/rules-based text)  │
├─────────────────────────────────────────────┤
│  [Approve]   [Reject]   [Defer + Reason]    │
└─────────────────────────────────────────────┘
```

### 3.5 Monitoring Page Pattern

For production monitoring, well surveillance, and real-time operations:

```
┌─────────────────────────────────────────────────────┐
│  Well / Facility: [Name]   Status: ● [Online/Shut]  │
├───────────────┬─────────────────────────────────────┤
│  Today's Data │  30-Day Trend Chart                 │
│  (big numbers)│  (oil / gas / water / pressure)     │
├───────────────┴─────────────────────────────────────┤
│  Anomalies & Alerts  │  Recommended Actions         │
└─────────────────────────────────────────────────────┘
```

---

## 4. Visual Design Rules

### 4.1 Color Semantics (Oil & Gas Standard)

| Color | MudBlazor Token | Meaning | Use Case |
|-------|-----------------|---------|----------|
| Petroleum Green | `Color.Success` | Good / On-target / Online | Production rate on target |
| Amber/Warning | `Color.Warning` | Attention needed / Declining | Production decline alert |
| Red | `Color.Error` | Critical / Shut-in / Failure | Well down, HSE incident |
| Blue | `Color.Primary` | Active / Information / Workflow | Active process step |
| Grey | `Color.Default` | Inactive / Historical / Closed | Completed or abandoned |

### 4.2 Status Indicators

Always use consistent status indicators:

| Status | Icon | Color | Context |
|--------|------|-------|---------|
| Online/Producing | `circle` filled | Green | Well/facility active |
| Shut-in | `circle` outlined | Amber | Temporary stop |
| Abandoned | `block` | Grey | Permanently closed |
| Testing | `science` | Blue | Well test in progress |
| Alarm | `warning` | Red | Immediate attention |
| Deferred | `pause_circle` | Grey/Blue | Delayed intentionally |

### 4.3 Typography Hierarchy

| Level | `MudText Typo` | Use |
|-------|----------------|-----|
| Page Title | `Typo.h5` | Section / page name |
| Section Header | `Typo.h6` | Card or panel heading |
| KPI Value | `Typo.h4` bold | Large metric numbers |
| KPI Label | `Typo.caption` uppercase | Label under KPI |
| Body | `Typo.body1` | Regular content |
| Sub-label | `Typo.body2` | Secondary info |

### 4.4 Layout Grid

- Use `MudGrid` with `xs=12, sm=6, md=4, lg=3` for KPI cards.
- Use `MudGrid` with `xs=12, md=8 / md=4` for main content + side panel split.
- Maximum content width: `MaxWidth.ExtraLarge` container.
- Page padding: `pa-4` minimum.

---

## 5. Image & Icon Rules

### 5.1 SVG Only — Mandatory

> **Rule**: All images in `wwwroot/imgs/` MUST be SVG format. No PNG, JPEG, or GIF files for icons, diagrams, or UI elements. Existing PNG files are legacy and should be replaced when touched.

Rationale:
- SVGs scale perfectly at all screen sizes and DPI.
- SVGs can be styled with CSS (color theming, dark mode).
- SVGs have smaller file sizes for simple illustrations.

### 5.2 SVG Icon Naming Convention

```
wwwroot/imgs/icons/
├── process/           # Business process icons
│   ├── exploration.svg
│   ├── drilling.svg
│   ├── production.svg
│   ├── reservoir.svg
│   └── decommissioning.svg
├── equipment/         # Physical equipment
│   ├── wellhead.svg
│   ├── separator.svg
│   ├── pipeline.svg
│   └── pump.svg
├── status/            # Status indicators
│   ├── online.svg
│   ├── shutin.svg
│   └── alarm.svg
└── workflow/          # Workflow & process
    ├── approval.svg
    ├── gate-review.svg
    └── work-order.svg
```

### 5.3 Using SVGs in Components

Preferred: inline SVG in Blazor for themeable icons:
```razor
<img src="/imgs/icons/process/drilling.svg" alt="Drilling" class="og-icon og-icon--md" />
```

For MudBlazor icons, use `Icons.Material.*` — do NOT create custom icon components for standard UI icons.

Custom SVG icons are ONLY for domain-specific O&G concepts not covered by Material Icons.

---

## 6. Business Process Page Rules

### 6.1 One Process Per Page

Each page simulates exactly ONE business process. If a page title contains "and" or "or", it must be split.

### 6.2 Process Context Always Visible

Every business process page must show:
- Active field name (in breadcrumb or page header)
- Current user role (determines available actions)
- Process step/stage indicator (where are we in the workflow?)

### 6.3 No Dead-End Pages

Every page must have at least one **next-action button** visible (what does the engineer do next?). Pages must not end with a list and no clear next step.

### 6.4 Data Entry Minimization

Business process forms must:
- Pre-fill fields from available data (well, field, date defaults).
- Show only fields relevant to the current step.
- Validate in real-time; never show full-page error on submit.
- Confirm destructive or irreversible decisions with a modal dialog.

### 6.5 Engineer's Mental Model

Organize information the way a petroleum engineer thinks:

```
Well → What is it producing today?
       → Is it performing to expectation?
       → What is the recommended action?
       → Execute action → Record result
```

NOT the database developer's mental model:
```
WELL table → WELL_TEST table → WELL_ACTIVITY table → WELL_WORKOVER table
```

---

## 7. Component Standards

### 7.1 KPI Card Component

```razor
<!-- Standard KPI card pattern -->
<MudPaper Class="pa-4" Elevation="2">
    <MudStack>
        <MudText Typo="Typo.caption" Style="text-transform:uppercase;letter-spacing:.05em;">
            @Label
        </MudText>
        <MudText Typo="Typo.h4" Color="@ValueColor">
            @Value @Unit
        </MudText>
        <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="1">
            <MudIcon Icon="@TrendIcon" Size="Size.Small" Color="@TrendColor" />
            <MudText Typo="Typo.body2" Color="@TrendColor">@TrendText</MudText>
        </MudStack>
    </MudStack>
</MudPaper>
```

### 7.2 Status Badge Component

```razor
<MudChip T="string" Color="@StatusColor" Size="Size.Small" Icon="@StatusIcon">
    @StatusText
</MudChip>
```

### 7.3 Process Step Timeline

Use `MudTimeline` for showing process history and current state:
```razor
<MudTimeline TimelinePosition="TimelinePosition.Start">
    @foreach (var step in ProcessSteps)
    {
        <MudTimelineItem Color="@step.Color" Icon="@step.Icon">
            <MudText Typo="Typo.subtitle2">@step.Name</MudText>
            <MudText Typo="Typo.body2">@step.Date.ToString("dd MMM yyyy")</MudText>
        </MudTimelineItem>
    }
</MudTimeline>
```

---

## 8. Anti-Patterns (Forbidden)

| Anti-Pattern | Why Forbidden | Correct Approach |
|---|---|---|
| Page with only a CRUD grid and no workflow context | Engineer doesn't know why they're there | Add process context, KPIs, next-action buttons |
| Inline `style=""` attributes | Breaks theme consistency, hard to maintain | Use CSS classes or MudBlazor utility classes |
| PNG/JPG icons in new components | Not scalable, not themeable | SVG only |
| Form with 20+ visible fields | Overwhelming, error-prone | Use MudStepper or tabbed sections, progressive disclosure |
| Navigation items named after DB tables | CRUD mindset leaks into UX | Name by business action or domain concept |
| Multiple business domains on one page | Violates single-responsibility | Split into separate pages |
| Hard-coded colors (`style="color: red"`) | Breaks dark mode and theming | Use `Color.*` enum or CSS variables |
| Icons without accessible alt text | Accessibility failure | Always provide `Title` or `alt` attribute |

---

## 9. Accessibility Rules

- All icons must have a `Title` attribute or accompanying text label.
- Color alone must NOT convey status — always pair color with icon or text.
- Keyboard navigation must work for all workflows (MudStepper, MudDialog).
- Focus trapping in dialogs is handled by MudBlazor — do not break it.
- Minimum contrast ratio: 4.5:1 for body text, 3:1 for large text.

---

## 10. Responsive Design

- **Mobile (xs)**: Show critical KPIs only; collapse non-essential panels.
- **Tablet (sm/md)**: Two-column layout; show workflows in full.
- **Desktop (lg/xl)**: Full dashboard layout with side panels.
- Kanban boards collapse to list view on mobile.
- Process wizards are always full-width to maintain readability.

---

*These guidelines supplement `.github/copilot-instructions.md`. In case of conflict, the copilot-instructions.md architecture rules take precedence for data access and DI patterns; this document governs UI/UX decisions.*
