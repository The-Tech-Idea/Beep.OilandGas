# Workflow-RBAC Configuration Guide — Phases 1–5

> All new features are configurable via `appsettings.json` in the ApiService project.

## Required appsettings.json Sections

### Notifications (Phase 5)

```json
{
  "Notifications": {
    "Email": {
      "Smtp": {
        "Host": "smtp.office365.com",
        "Port": 587,
        "EnableSsl": true,
        "From": "noreply@yourcompany.com",
        "Username": "",
        "Password": ""
      }
    },
    "SignalR": {
      "HubUrl": "/hubs/workflow-notifications",
      "AutoReconnect": true
    }
  }
}
```

### JWT Authentication (Phase 1)

```json
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "<32-char-minimum-secret>",
      "Issuer": "Beep.OilandGas",
      "Audience": "Beep.OilandGas.Client",
      "AccessTokenExpiryMinutes": 60,
      "RefreshTokenExpiryDays": 7
    },
    "MaxFailedLoginAttempts": 5,
    "LockoutDurationMinutes": 30,
    "MaxPasswordAgeDays": 90
  }
}
```

### Database Connection

```json
{
  "BeepOg": {
    "DatabaseConnectionName": "PPDM39"
  }
}
```

## WebHook Configuration (Phase 5)

Register webhook subscribers via `IExternalWebhookTriggerService.RegisterWebhookAsync()`:

```csharp
await webhookService.RegisterWebhookAsync(new WebhookConfig
{
    Name = "SAP ERP Integration",
    Url = "https://sap.yourcompany.com/api/oilgas/webhook",
    Secret = "<hmac-secret>",
    EventTypes = new List<string> { "step.completed", "process.completed", "approval.decision" }
});
```

## Outbound Webhook Events

| Event | Payload | When |
|-------|---------|------|
| `step.started` | `{ processId, stepId, entityType, entityId, assignedRole }` | Step becomes active |
| `step.completed` | `{ processId, stepId, outcome, completedBy }` | Step finishes |
| `process.completed` | `{ processId, entityType, entityId, finalStatus, chainHash }` | Process finishes |
| `approval.requested` | `{ processId, approvalId, approverUserId, dueDate }` | Approval requested |
| `approval.decision` | `{ approvalId, decision, comments }` | Approved or rejected |
| `sla.breached` | `{ stepId, slaHours, elapsedHours, escalationAction }` | SLA exceeded |

## Delegation of Authority (Phase 2)

DoA thresholds are seeded automatically by `LifeCycleSeedService`. Custom thresholds can be added via `IDoAEvaluationService` or directly in the `DELEGATION_OF_AUTHORITY` table.

**Default 5-level scale:**
| Level | Threshold | Role |
|-------|-----------|------|
| LEVEL_1 | > $0 | Supervisor |
| LEVEL_2 | > $50,000 | Manager |
| LEVEL_3 | > $500,000 | Senior Manager |
| LEVEL_4 | > $5,000,000 | Executive |
| LEVEL_5 | > $50,000,000 | Board |

## Segregation of Duties (Phase 4)

25 SoD rules are seeded automatically. SoD enforcement is **active at role assignment time** — critical conflicts block the assignment with an `InvalidOperationException`.

To grant a waiver, start the `SOD_WAIVER` workflow which requires:
1. Manager requests with business justification
2. Security Administrator reviews
3. Independent approval (Auditor/Compliance Officer)
4. Compensating control documented with 90-day auto-expiry

## SignalR Client Integration (Phase 5)

In any Blazor component, inject `INotificationService` and start:

```csharp
@inject INotificationService NotificationService

protected override async Task OnInitializedAsync()
{
    await NotificationService.StartAsync(userId, personaCode);
}

public void Dispose()
{
    _ = NotificationService.StopAsync();
}
```

## Domain Event Publishing (Phase 3)

Inject `IDomainEventPublisher` in any domain service and fire events after entity changes:

```csharp
// After posting production volumes:
await _eventPublisher.PublishStatusChangedAsync(
    "PDEN_VOL_SUMMARY", entityId, fieldId,
    previousStatus, "POSTED", entityFields, userId);
// This auto-triggers the Production→Revenue workflow (CRW-01)
```

## Verify the Installation

1. **Check seed data**: Query `PERSONA_ROLE`, `ROLE_HIERARCHY`, `SOD_RULE`, `DELEGATION_OF_AUTHORITY` tables
2. **Check workflows**: Query `PROCESS_DEFINITION` for `ProcessId LIKE 'CRW_%'` or `ProcessId LIKE 'RBAC_%'`
3. **Check JWT claims**: Decode a login token — verify `field_scope`, `permissions`, `elevated_permissions` claims exist
4. **Check SoD**: Try assigning both `WellManagement.Create` and `WellManagement.Approve` roles to the same user — should throw `InvalidOperationException`
5. **Check notifications**: Open `/tasks/inbox` — should show empty state "All caught up!" when no tasks pending
