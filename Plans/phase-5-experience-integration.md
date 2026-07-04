# Phase 5 — Experience & Integration

> **Status:** Not Started | **Depends on:** Phase 2 (enhanced engine) + Phase 3 (cross-role orchestration)
> **Est. Effort:** 2–3 weeks | **Module:** Extend `Beep.OilandGas.Web` + new SignalR hub

---

## Objectives

1. Unified task inbox — every persona sees ALL their pending approvals, reviews, and data-entry tasks in one place
2. Multi-channel notifications — in-app (SignalR), email, and webhook for external system integration
3. Workflow DAG visualization — render a running process instance as an interactive directed acyclic graph
4. Persona-aware dashboard widgets — pending counts, SLA health, bottleneck alerts
5. Mobile-responsive approval — approve/reject from any device
6. External system webhook triggers — integrate with ERP (SAP, Oracle), SCADA, regulatory portals

---

## UI Architecture

All new pages follow the existing Blazor pattern under `Beep.OilandGas.Web\Pages\`:

```
Beep.OilandGas.Web\
├── Pages\
│   └── PPDM39\
│       └── Workflow\
│           ├── TaskInbox.razor              (NEW - unified task inbox)
│           ├── TaskDetail.razor             (NEW - single task with approve/reject)
│           ├── WorkflowProgressPage.razor   (NEW - enhanced progress view)
│           └── WorkflowDagViewer.razor      (NEW - DAG visualization)
├── Components\
│   ├── Workflow\
│   │   ├── TaskInboxCard.razor             (NEW - reusable task card)
│   │   ├── ApprovalActionBar.razor         (NEW - approve/reject/delegate buttons)
│   │   ├── WorkflowDagVisualizer.razor     (NEW - DAG render component)
│   │   ├── SlaHealthIndicator.razor        (NEW - SLA status badge)
│   │   └── NotificationBadge.razor         (NEW - real-time notification count)
│   └── Notifications\
│       ├── NotificationCenter.razor        (NEW - notification drawer)
│       └── NotificationItem.razor          (NEW - single notification)
├── Hubs\
│   └── WorkflowNotificationHub.cs          (NEW - SignalR hub)
└── Services\
    ├── UnifiedTaskInboxService.cs          (NEW)
    ├── NotificationService.cs              (NEW)
    ├── WorkflowNotificationClient.cs       (NEW - SignalR client)
    └── ExternalWebhookTriggerService.cs    (NEW)
```

---

## Task Details

### P5-01: UnifiedTaskInboxService

**File:** `Beep.OilandGas.Web\Services\UnifiedTaskInboxService.cs` (NEW)

```csharp
public interface IUnifiedTaskInboxService
{
    /// <summary>
    /// Get ALL pending tasks for the current user across all personas and workflows.
    /// Unified view of approvals, reviews, data entry, and notifications.
    /// </summary>
    Task<UnifiedInbox> GetInboxAsync(string userId, string personaCode);

    /// <summary>
    /// Get task counts broken down by type (for badge numbers).
    /// </summary>
    Task<InboxCounts> GetInboxCountsAsync(string userId, string personaCode);

    /// <summary>
    /// Get tasks filtered by type, priority, due date, workflow.
    /// </summary>
    Task<List<UnifiedTask>> GetFilteredTasksAsync(InboxFilter filter);
}

public class UnifiedTask
{
    public string TaskId { get; set; }               // PROCESS_STEP_INSTANCE_ID or similar
    public string TaskType { get; set; }              // "APPROVAL", "REVIEW", "DATA_ENTRY", "NOTIFICATION"
    public string WorkflowName { get; set; }          // "AFE Approval with DoA"
    public string StepName { get; set; }              // "Manager Approval"
    public string EntityType { get; set; }            // "AFE"
    public string EntityId { get; set; }              // "AFE-001"
    public string EntityDescription { get; set; }     // "AFE-001: Well Workover #42"
    public string FromPersona { get; set; }           // "PetroleumEngineer"
    public string FromUserName { get; set; }          // "John Smith"
    public int Priority { get; set; }                 // 1=critical, 2=high, 3=normal, 4=low
    public DateTime? DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string SlaStatus { get; set; }             // "ON_TRACK", "AT_RISK", "BREACHED"
    public string Status { get; set; }                // "PENDING", "IN_PROGRESS"
    public string Route { get; set; }                 // Deep-link to the task detail page
    public Dictionary<string, string> Metadata { get; set; }
}

public class UnifiedInbox
{
    public List<UnifiedTask> CriticalTasks { get; set; }    // Priority 1, overdue, or breached
    public List<UnifiedTask> HighPriorityTasks { get; set; } // Priority 2
    public List<UnifiedTask> NormalTasks { get; set; }      // Priority 3-4
    public List<UnifiedTask> RecentlyCompleted { get; set; } // Last 10 completed
    public InboxCounts Counts { get; set; }
}

public class InboxCounts
{
    public int TotalPending { get; set; }
    public int Critical { get; set; }
    public int Overdue { get; set; }
    public int Approvals { get; set; }
    public int Reviews { get; set; }
    public int DataEntry { get; set; }
}

public class InboxFilter
{
    public string TaskType { get; set; }
    public int? MinPriority { get; set; }
    public DateTime? DueBefore { get; set; }
    public string WorkflowName { get; set; }
    public string EntityType { get; set; }
    public string FieldId { get; set; }
    public string SortBy { get; set; }  // "priority", "due_date", "created_date"
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 0;
}
```

**Data sources** (aggregates from multiple backends):
1. `PROCESS_STEP_INSTANCE` where `ASSIGNED_TO` matches user's roles AND status = PENDING
2. `PROCESS_APPROVAL` where `APPROVER_USER_ID` = userId AND status = PENDING
3. `CROSS_PERSONA_TASK` (Phase 3) where target persona matches
4. `ACCESS_REVIEW_ITEM` (Phase 4) where reviewer = userId

**Resolution:**
1. Call `CrossPersonaTaskRouter.GetTasksForPersonaAsync` (Phase 3)
2. Call `ApprovalWorkflowEngine.GetPendingApprovalsAsync`
3. Call `AccessReviewCampaignService.GetPendingReviewsAsync`
4. Merge, deduplicate, sort by priority + due date
5. Enrich with entity descriptions (look up WELL_NAME, AFE_NUMBER, etc.)

---

### P5-02: TaskInbox.razor Page

**File:** `Beep.OilandGas.Web\Pages\PPDM39\Workflow\TaskInbox.razor` (NEW)

```razor
@page "/ppdm39/tasks/inbox"
@page "/tasks/inbox"

<MudContainer MaxWidth="MaxWidth.Large">
    <MudText Typo="Typo.h3" GutterBottom>
        <MudIcon Icon="@Icons.Material.Filled.Inbox" Class="mr-2" />
        Task Inbox
        @if (_counts.TotalPending > 0)
        {
            <MudChip Color="Color.Primary" Size="Size.Small" Class="ml-2">@_counts.TotalPending pending</MudChip>
        }
    </MudText>

    @* ── Filter Bar ── *@
    <MudStack Row="true" Spacing="2" Class="mb-4">
        <MudChipSet @bind-SelectedValue="_filter.TaskType" Filter="true">
            <MudChip Value="">All</MudChip>
            <MudChip Value="APPROVAL">Approvals (@_counts.Approvals)</MudChip>
            <MudChip Value="REVIEW">Reviews (@_counts.Reviews)</MudChip>
            <MudChip Value="DATA_ENTRY">Data Entry (@_counts.DataEntry)</MudChip>
        </MudChipSet>
        <MudSpacer />
        <MudText Typo="Typo.caption" Color="@(_counts.Critical > 0 ? Color.Error : Color.Default)">
            @_counts.Critical critical · @_counts.Overdue overdue
        </MudText>
    </MudStack>

    @* ── Critical / Overdue Section ── *@
    @if (_inbox.CriticalTasks.Any())
    {
        <MudText Typo="Typo.h6" Color="Color.Error" Class="mb-2">
            <MudIcon Icon="@Icons.Material.Filled.PriorityHigh" /> Requires Immediate Attention
        </MudText>
        @foreach (var task in _inbox.CriticalTasks)
        {
            <TaskInboxCard Task="task" OnAction="HandleTaskAction" />
        }
        <MudDivider Class="my-4" />
    }

    @* ── Main Task List ── *@
    <MudVirtualize Items="_allTasks" OverscanCount="5">
        <TaskInboxCard Task="context" OnAction="HandleTaskAction" />
    </MudVirtualize>
</MudContainer>
```

**Features:**
- Virtual scrolling for large task lists
- Real-time updates via SignalR (new tasks appear without refresh)
- Click task → slide-out detail panel or navigate to TaskDetail.razor
- Bulk actions: "Approve All Low-Risk", "Delegate Selected"
- Keyboard shortcuts: A=approve, R=reject, D=delegate, N=next task

---

### P5-03: Add Task Inbox to All Persona Nav Menus

Add to `PetroleumEngineerNavMenu.razor` and all other nav menus:

```razor
<MudMenuItem Href="/tasks/inbox">
    <MudIcon Icon="@Icons.Material.Filled.Inbox" Class="mr-2" />
    Task Inbox
    @if (_pendingTaskCount > 0)
    {
        <MudChip Size="Size.Small" Color="Color.Error" Class="ml-auto">@_pendingTaskCount</MudChip>
    }
</MudMenuItem>
```

The `_pendingTaskCount` is populated by `NotificationBadge` component which subscribes to SignalR.

---

### P5-04, P5-05, P5-06: Multi-Channel Notification Service

**File:** `Beep.OilandGas.Web\Services\NotificationService.cs` (NEW)

```csharp
public interface INotificationService
{
    /// <summary>
    /// Send a notification through all configured channels for the recipient.
    /// </summary>
    Task SendAsync(Notification notification);

    /// <summary>
    /// Get notification history for current user.
    /// </summary>
    Task<List<Notification>> GetHistoryAsync(int pageSize = 50);

    /// <summary>
    /// Mark notification(s) as read.
    /// </summary>
    Task MarkReadAsync(List<string> notificationIds);
}

public class Notification
{
    public string NotificationId { get; set; }
    public string RecipientUserId { get; set; }
    public string Title { get; set; }              // "AFE-001 Requires Your Approval"
    public string Body { get; set; }               // "Estimated cost: $750,000. Due by: 2026-07-15"
    public string Severity { get; set; }           // "INFO", "WARNING", "CRITICAL"
    public string Category { get; set; }           // "APPROVAL", "ESCALATION", "REMINDER", "SYSTEM"
    public string ActionRoute { get; set; }        // "/tasks/inbox/AFE-001"
    public string ActionLabel { get; set; }        // "Review AFE"
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Channel { get; set; }            // "IN_APP", "EMAIL", "BOTH"
}
```

**Channel providers (strategy pattern):**
```csharp
public interface INotificationChannel
{
    string ChannelName { get; }                    // "IN_APP", "EMAIL", "WEBHOOK"
    Task<bool> SendAsync(Notification notification);
    Task<bool> IsAvailableAsync(string userId);    // Does user have email configured?
}

// Implementations:
public class InAppNotificationChannel : INotificationChannel { ... }   // SignalR
public class EmailNotificationChannel : INotificationChannel { ... }   // SMTP
public class WebhookNotificationChannel : INotificationChannel { ... } // External systems
```

**SignalR Hub (P5-06):**
```csharp
public class WorkflowNotificationHub : Hub
{
    public async Task SubscribeToUser(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
    }

    public async Task SubscribeToPersona(string personaCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"persona:{personaCode}");
    }

    // Server-side method to push:
    // await _hubContext.Clients.Group($"user:{userId}").SendAsync("Notification", notification);
    // await _hubContext.Clients.Group($"persona:{personaCode}").SendAsync("TaskUpdate", task);
}
```

**Trigger points** (integrated into workflow services from Phases 2-3):
- Step assigned to user → notification
- SLA at 50% → reminder
- SLA breached → escalation notification
- Approval received → notify requester
- Process completed → notify all participants
- SoD conflict detected → notify administrator

---

### P5-07: NotificationCenter.razor Component

**File:** `Beep.OilandGas.Web\Components\Notifications\NotificationCenter.razor` (NEW)

A bell-icon dropdown in the app bar (similar to GitHub/Facebook notifications):

```razor
<MudMenu AnchorOrigin="Origin.BottomRight" TransformOrigin="Origin.TopRight">
    <ActivatorContent>
        <MudIconButton Icon="@Icons.Material.Filled.Notifications" Color="Color.Inherit">
        </MudIconButton>
        @if (_unreadCount > 0)
        {
            <MudBadge Content="@_unreadCount" Color="Color.Error" Overlap="true" />
        }
    </ActivatorContent>
    <ChildContent>
        @foreach (var notification in _recentNotifications.Take(10))
        {
            <MudMenuItem OnClick="@(() => HandleNotificationClick(notification))">
                <MudStack Spacing="0">
                    <MudText Typo="Typo.body2" Style="font-weight:@(notification.IsRead ? "normal" : "bold")">
                        @notification.Title
                    </MudText>
                    <MudText Typo="Typo.caption" Color="Color.Secondary">
                        @notification.CreatedAt.ToString("g")
                    </MudText>
                </MudStack>
            </MudMenuItem>
        }
        <MudDivider />
        <MudMenuItem Href="/notifications">View All</MudMenuItem>
    </ChildContent>
</MudMenu>
```

---

### P5-08: WorkflowDagVisualizer Component

**File:** `Beep.OilandGas.Web\Components\Workflow\WorkflowDagVisualizer.razor` (NEW)

Renders a process instance as an interactive DAG (directed acyclic graph) showing:
- **Completed steps** → green with checkmark + completion timestamp
- **Current step** → blue pulsing with assigned user + elapsed time
- **Pending steps** → grey outline
- **Failed steps** → red with error message
- **Approval steps** → diamond shape with pending/approved/rejected count
- **Conditional branches** → dashed lines with condition expression
- **Sub-processes** → nested boxes with expand/collapse

**Implementation approach:**
- SVG-based rendering (no external JS library needed) — works with MudBlazor
- Each step is a positioned SVG `<rect>` or `<polygon>` element
- Transitions are SVG `<path>` elements with arrowheads
- Clicking a step opens detail panel
- SignalR updates re-render changed nodes in real-time

```razor
<svg width="100%" height="@(_graphHeight)" style="min-height:400px">
    @foreach (var node in _graphNodes)
    {
        <g transform="translate(@node.X, @node.Y)">
            @if (node.Shape == "diamond")
            {
                <polygon points="0,-30 60,0 0,30 -60,0" fill="@node.FillColor" 
                         stroke="@node.StrokeColor" stroke-width="2"
                         @onclick="@(() => OnNodeClick(node))" />
            }
            else
            {
                <rect x="-80" y="-24" width="160" height="48" rx="8"
                      fill="@node.FillColor" stroke="@node.StrokeColor" stroke-width="2"
                      @onclick="@(() => OnNodeClick(node))" />
            }
            <text text-anchor="middle" dy=".3em" font-size="12" fill="#333">
                @node.Label
            </text>
        </g>
    }
    @* Render edges *@
    @foreach (var edge in _graphEdges)
    {
        <path d="@edge.SvgPath" stroke="@(edge.IsActive ? "#1976D2" : "#CCC")" 
              stroke-width="@(edge.IsActive ? 3 : 1)" 
              marker-end="url(#arrowhead)" fill="none" />
    }
</svg>
```

---

### P5-09: WorkflowProgressPage.razor (Enhanced)

**File:** `Beep.OilandGas.Web\Pages\PPDM39\Workflow\WorkflowProgressPage.razor` (NEW)

Replaces the basic `WorkflowProgress.razor` component with a full-page view:

- **Left panel:** DAG visualization (P5-08 component)
- **Right panel:** Step detail (selected step)
  - Step name, description, status
  - Assigned to (with avatar)
  - SLA timer (countdown or "BREACHED" in red)
  - Approval chain status (who approved, who hasn't)
  - Validation results (if validation step)
  - Action buttons: Approve, Reject, Delegate, Request Info, Skip (if allowed)
  - Comments/notes thread
  - History timeline for this step
- **Top bar:** Process breadcrumb, overall progress %, elapsed time, cancel button

---

### P5-10: Persona-Aware Dashboard Widgets

Add to each persona's dashboard landing page:

| Widget | Data Source | Refresh |
|--------|------------|---------|
| **My Pending Tasks** | UnifiedTaskInboxService | SignalR (real-time) |
| **SLA Health** | SlaTrackingService | 60s poll |
| **Bottleneck Alert** | Steps where SLA > 80% with no activity > 24h | 5min poll |
| **Approval Queue Depth** | Count of pending approvals in user's workflows | SignalR |
| **Recent Completions** | Last 5 steps completed in user's workflows | SignalR |
| **Cross-Persona Handoffs** | Pending handoffs FROM this persona | SignalR |

Implementation: reusable `MudCard` components that accept a data source and render accordingly. Each dashboard page composes these widgets based on persona relevance.

---

### P5-11: Mobile-Responsive Approval Actions

All approval components use MudBlazor's responsive grid:

```razor
<MudStack Row="true" Spacing="2" Class="approval-actions">
    <MudButton Variant="Variant.Filled" Color="Color.Success" 
               OnClick="Approve" StartIcon="@Icons.Material.Filled.Check">
        Approve
    </MudButton>
    <MudButton Variant="Variant.Filled" Color="Color.Error" 
               OnClick="Reject" StartIcon="@Icons.Material.Filled.Close">
        Reject
    </MudButton>
    <MudButton Variant="Variant.Outlined" Color="Color.Secondary" 
               OnClick="Delegate" StartIcon="@Icons.Material.Filled.Forward">
        Delegate
    </MudButton>
    <MudButton Variant="Variant.Text" OnClick="RequestInfo" 
               StartIcon="@Icons.Material.Filled.Help">
        Request Info
    </MudButton>
</MudStack>
```

On mobile (`xs` breakpoint), buttons stack vertically and expand to full width. On desktop, they're horizontal.

---

### P5-12: ExternalWebhookTriggerService

**File:** `Beep.OilandGas.Web\Services\ExternalWebhookTriggerService.cs` (NEW)

```csharp
public interface IExternalWebhookTriggerService
{
    /// <summary>
    /// Register a webhook endpoint that receives workflow events.
    /// </summary>
    Task<WebhookRegistration> RegisterWebhookAsync(WebhookConfig config, string userId);

    /// <summary>
    /// Fire a webhook event to all registered subscribers.
    /// </summary>
    Task FireWebhookAsync(string eventType, object payload);

    /// <summary>
    /// Receive an inbound webhook from an external system (e.g., SAP posts a cost).
    /// </summary>
    Task<WebhookReceiveResult> ReceiveWebhookAsync(string sourceSystem, object payload);
}

public class WebhookConfig
{
    public string WebhookId { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }                // Target URL
    public string Secret { get; set; }              // HMAC signing secret
    public List<string> EventTypes { get; set; }    // ["STEP_COMPLETED", "PROCESS_COMPLETED", "APPROVAL_REQUIRED"]
    public string Format { get; set; }              // "JSON", "XML"
    public bool IsActive { get; set; }
}
```

**Outbound events** (fired by the workflow engine):
- `step.started` — payload: processId, stepId, entityType, entityId, assignedRole
- `step.completed` — payload: processId, stepId, outcome, completedBy, timestamp
- `process.completed` — payload: processId, entityType, entityId, finalStatus, chainHash
- `approval.requested` — payload: processId, approvalId, approverUserId, dueDate
- `approval.decision` — payload: approvalId, decision, comments, timestamp
- `sla.breached` — payload: stepId, slaHours, elapsedHours, escalationAction

**Inbound events** (received from external systems):
- `cost.posted` (from SAP/Oracle) → trigger AFE cost tracking workflow update
- `production.updated` (from SCADA) → trigger production validation workflow
- `permit.approved` (from regulatory portal) → trigger permit workflow completion
- `incident.reported` (from field mobile app) → trigger HSE incident workflow

---

## Phase 5 Completion Checklist

- [ ] UnifiedTaskInboxService aggregates tasks from all sources
- [ ] TaskInbox.razor page with filtering, sorting, virtual scrolling
- [ ] Task inbox link in all persona nav menus with live badge count
- [ ] NotificationService delivers through all channels
- [ ] SignalR hub pushes real-time task updates
- [ ] NotificationCenter dropdown in app bar with unread count
- [ ] Email notifications for SLA breaches and escalations
- [ ] WorkflowDagVisualizer renders process instances as interactive SVG
- [ ] Clicking a DAG node shows step detail with approve/reject/delegate
- [ ] WorkflowProgressPage replaces basic component with full-page view
- [ ] Dashboard widgets show real-time task counts and SLA health
- [ ] Approval buttons work on mobile (responsive layout)
- [ ] Webhook system sends outbound events and receives inbound triggers
- [ ] SAP/ERP webhook integration tested (at minimum, a mock)

## Phase 5 Deliverables

| # | File | Action |
|---|------|--------|
| 1 | `Web\Services\UnifiedTaskInboxService.cs` | CREATE |
| 2 | `Web\Services\NotificationService.cs` | CREATE |
| 3 | `Web\Services\ExternalWebhookTriggerService.cs` | CREATE |
| 4 | `Web\Hubs\WorkflowNotificationHub.cs` | CREATE |
| 5 | `Web\Pages\PPDM39\Workflow\TaskInbox.razor` | CREATE |
| 6 | `Web\Pages\PPDM39\Workflow\TaskDetail.razor` | CREATE |
| 7 | `Web\Pages\PPDM39\Workflow\WorkflowProgressPage.razor` | CREATE |
| 8 | `Web\Components\Workflow\TaskInboxCard.razor` | CREATE |
| 9 | `Web\Components\Workflow\ApprovalActionBar.razor` | CREATE |
| 10 | `Web\Components\Workflow\WorkflowDagVisualizer.razor` | CREATE |
| 11 | `Web\Components\Workflow\SlaHealthIndicator.razor` | CREATE |
| 12 | `Web\Components\Notifications\NotificationCenter.razor` | CREATE |
| 13 | `Web\Components\Notifications\NotificationBadge.razor` | CREATE |
| 14 | `Web\Components\Navigation\PetroleumEngineerNavMenu.razor` | MODIFY |
| 15 | `Web\Shared\MainLayout.razor` | MODIFY (add NotificationCenter) |
| 16 | `ApiService\Program.cs` | MODIFY (register SignalR hub) |
| 17 | `Web\wwwroot\js\workflowSignalR.js` | CREATE (minimal JS for SignalR) |

---

*Previous: [Phase 4 — Governance & Compliance](phase-4-governance-compliance.md)*
*Back to: [Master Plan](workflow-rbac-master-plan.md)*
