# Phase 4 — UX Flows
## User Journey Wireframes and Navigation Diagrams

---

## Flow 1: Browse Catalog → Start a Process

```
[Nav Sidebar]
  └─► Process Catalog (/ppdm39/process/catalog)
        • Grid of 96 definitions, grouped by category
        • Filter chips by ProcessType (WORK_ORDER / GATE_REVIEW / HSE_INCIDENT …)
        │
        ▼
      [Click definition row]
        │
        ▼
      Definition Detail (/ppdm39/process/definition/{processId})
        • Step list (read-only MudTimeline)
        • Required documents badge
        • "Start This Process" button (visible if role ≥ ProcessOperator)
        │
        ▼
      Start Process Wizard (/ppdm39/process/start?definitionId={processId})
        • Step 1: Confirm Process Definition (pre-filled if arrived via button)
        • Step 2: Select Entity (autocomplete for EQUIPMENT / WELL / FACILITY …)
                  Select Jurisdiction
                  Optional name
        • Step 3: Review & Confirm
        │
        ▼
      POST /api/field/current/process/start
        │
        ├─► 201 ──► Process Detail (/ppdm39/process/{instanceId})
        │              MudSnackbar "Process started"
        │
        └─► 400/404 ──► Stay on wizard; MudValidationMessage shows error
```

---

## Flow 2: Dashboard → Apply Transition

```
[Nav Sidebar]
  └─► Process Dashboard (/ppdm39/process)
        • MudDataGrid, columns: Name | State | Category | Entity | Jurisdiction | Started
        • ProcessStateChip in State column
        • ProcessFilterBar (state, category, jurisdiction)
        │
        ▼
      [Click row]
        │
        ▼
      Process Detail (/ppdm39/process/{instanceId})
        ┌─────────────────────────────────────────────────────────┐
        │  Header:  Instance Name   [State Chip]  [JurisdictionBadge] │
        │  Entity:  EQUIPMENT-007                                  │
        │  Field:   PERMIAN-001                                    │
        │                                                          │
        │  TransitionPanel:                                        │
        │   [Plan Work Order]  [Cancel]                            │
        │                                                          │
        │  ProcessTimeline (last 5 events)                         │
        │   ↳ "View full audit" link                               │
        └─────────────────────────────────────────────────────────┘
        │
        ▼
      [Click "Plan Work Order"]
        │
        ├─► 200 ──► Page reloads instance; state chip updates to PLANNED
        │              MudSnackbar "Transition applied"
        │
        ├─► 422 ──► GuardFailureDialog opens
        │              Shows: RequiredField, Detail, RegulatoryReference
        │              [OK] closes dialog; state unchanged
        │
        └─► 403 ──► MudAlert: "You do not have permission to apply this transition"
```

---

## Flow 3: Apply Approval (Gate Review)

```
Process Detail (GATE_REVIEW instance in UNDER_REVIEW)
  • Approver count badge: "1 of 2 approvals submitted"
  • TransitionPanel shows: [Approve] [Reject] (only visible for Approver role)
  │
  ▼
[Approver clicks "Approve"]
  │
  POST /api/field/current/process/{instanceId}/transition  { trigger: "approve" }
  │
  ├─► 200, newState = "UNDER_REVIEW"  (count 2/2 not yet met)
  │       MudSnackbar "Your approval recorded (1 of 2)"
  │
  └─► 200, newState = "APPROVED"      (count 2/2 met, SM advances)
          MudSnackbar "All approvals complete — process approved"
```

---

## Flow 4: View Audit Trail

```
Process Detail
  └─► "View full audit" link
        │
        ▼
      Process Audit (/ppdm39/process/{instanceId}/audit)
        • ProcessTimeline in full-history mode
        • MudTable below timeline with columns: Date | User | From | To | Trigger | Reason
        • "Download CSV" button
```

---

## Responsive Breakpoints

| Viewport | Grid Columns | Sidebar |
|---|---|---|
| ≥1440px | 12 columns, 4 per row | Expanded |
| 1024–1439px | 12 columns, 3 per row | Collapsed to icons |
| < 1024px | Single column | Hidden (hamburger) |

`ProcessDashboard` grid uses `xs=12 sm=6 md=4` for category card layout; the `MudDataGrid` stays full-width on all breakpoints.
