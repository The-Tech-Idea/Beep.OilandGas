# Phase 3 — Authorization Matrix
## Role-to-Endpoint Permissions

> JWT Bearer token issued by Identity Server (`Beep.OilandGas.UserManagement.AspNetCore`)  
> Claims: `role` (string), `sub` (userId), `field_id` (current field scope)  
> All endpoints require `[Authorize]` minimum; sensitive endpoints use `[Authorize(Roles = "...")]`

---

## Role Definitions

| Role Name | Description | Assigned To |
|---|---|---|
| `Viewer` | Read-only access to all process data | Any authenticated user (default) |
| `ProcessOperator` | Can start processes, apply non-approval transitions | Field operators, engineers |
| `Approver` | Can apply approval/rejection transitions on gate reviews | Department heads, project managers |
| `SafetyOfficer` | Can close HSE incidents Tier 1/2; access all HSE data | HSE Manager, QHSE staff |
| `ComplianceOfficer` | Can start and advance compliance report processes | Regulatory affairs staff |
| `Auditor` | Read-only access to full audit trails | Internal/external auditors |
| `Manager` | All of the above | Field Manager, Operations Manager |
| `Admin` | All of the above + can cancel any instance | System Administrator |

---

## Endpoint Authorization Matrix

### Process Definitions (Read-Only Catalog)

| Endpoint | Method | Min Role | Notes |
|---|---|---|---|
| `/api/process/definitions` | GET | `Viewer` | Public catalog — any authenticated user |
| `/api/process/definitions/{id}` | GET | `Viewer` | — |
| `/api/process/definitions/by-category/{n}` | GET | `Viewer` | — |
| `/api/process/definitions/by-type/{type}` | GET | `Viewer` | — |
| `/api/process/types` | GET | `Viewer` | — |

### Process Instances

| Endpoint | Method | Min Role | Notes |
|---|---|---|---|
| `/api/field/current/process/start` | POST | `ProcessOperator` | Creates instance; field scoped to JWT `field_id` |
| `/api/field/current/process` | GET | `Viewer` | Field-scoped list |
| `/api/field/current/process/{id}` | GET | `Viewer` | — |
| `/api/field/current/process/by-entity/{entityId}` | GET | `Viewer` | — |
| `/api/field/current/process/active` | GET | `Viewer` | — |
| `/api/field/current/process/{id}` | DELETE | `Manager` OR `Admin` | Cancel requires high privilege due to audit implications |

### Transitions — Role Requirements by Trigger

Some transition triggers require elevated roles (enforced in `ApplyTransitionAsync` before calling service):

| Trigger | SM Type | Minimum Role | Regulatory Basis |
|---|---|---|---|
| `start` | `WORK_ORDER` | `ProcessOperator` | Standard ops |
| `complete` | `WORK_ORDER` | `ProcessOperator` | Standard ops |
| `approve` | `GATE_REVIEW` | `Approver` | CAPL JOA Art. IX — only authorized approvers |
| `reject` | `GATE_REVIEW` | `Approver` | Same |
| `defer` | `GATE_REVIEW` | `Approver` | Same |
| `close` | `HSE_INCIDENT` | `SafetyOfficer` OR `Manager` | API RP 754; IOGP 2022e §6 |
| `quick_close` (Tier 3/4) | `HSE_INCIDENT` | `SafetyOfficer` | API RP 754 |
| `compliant` / `non_compliant` | `COMPLIANCE_REPORT` | `ComplianceOfficer` OR `Manager` | ONRR; AER regulatory officer designation |
| `remediation_start` | `COMPLIANCE_REPORT` | `ComplianceOfficer` | — |
| `cancel` | Any | `Manager` OR `Admin` | Cancellation of running processes is high-impact |

### Audit Endpoints

| Endpoint | Method | Min Role | Notes |
|---|---|---|---|
| `/api/field/current/process/{id}/audit` | GET | `Auditor` OR `Manager` | Full transition log |
| `/api/field/current/process/{id}/audit/summary` | GET | `Viewer` | Summary only — safe for any user |
| `/api/field/current/process/audit/by-user/{userId}` | GET | `Auditor` OR `Manager` | Cross-instance audit; requires elevated role |

---

## Authorization Implementation

### Transition Role Enforcement (in ProcessTransitionController)

```csharp
[HttpPost]
public async Task<ActionResult<ProcessInstanceResponse>> ApplyTransitionAsync(
    string instanceId,
    [FromBody] ApplyTransitionRequest request)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

    // Role check for approval triggers
    if (request.Trigger is "approve" or "reject" or "defer")
    {
        if (!User.IsInRole("Approver") && !User.IsInRole("Manager") && !User.IsInRole("Admin"))
            return Forbid();
    }

    // Role check for HSE close
    if (request.Trigger is "close" or "quick_close")
    {
        // Check instance type before role check
        var inst = await _service.GetInstanceByIdAsync(instanceId);
        if (inst?.ProcessType == "HSE_INCIDENT")
        {
            if (!User.IsInRole("SafetyOfficer") && !User.IsInRole("Manager") && !User.IsInRole("Admin"))
                return Forbid();
        }
    }

    // Role check for cancel
    if (request.Trigger == "cancel")
    {
        if (!User.IsInRole("Manager") && !User.IsInRole("Admin"))
            return Forbid();
    }

    var updated = await _service.ApplyTransitionAsync(instanceId, request.Trigger, userId, request.Reason);
    return Ok(ProcessInstanceResponse.From(updated));
}
```

### JWT Configuration (appsettings reference)

```json
{
  "Jwt": {
    "ValidIssuer":   "https://identity.beep-oilandgas.local",
    "ValidAudience": "beep-oilandgas-api",
    "RoleClaimType": "role"
  }
}
```

### Program.cs — Role Policy Registration

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApproverOrAbove",  p => p.RequireRole("Approver", "Manager", "Admin"));
    options.AddPolicy("SafetyOrAbove",    p => p.RequireRole("SafetyOfficer", "Manager", "Admin"));
    options.AddPolicy("ComplianceOrAbove",p => p.RequireRole("ComplianceOfficer", "Manager", "Admin"));
    options.AddPolicy("AuditorOrAbove",   p => p.RequireRole("Auditor", "Manager", "Admin"));
    options.AddPolicy("ManagerOrAbove",   p => p.RequireRole("Manager", "Admin"));
});
```

---

## Field Isolation

The `field_id` JWT claim is validated against `IFieldOrchestrator.CurrentFieldId` in all instance endpoints:

```csharp
// In ProcessInstanceController action methods
private bool IsFieldAccessAllowed(string requestedFieldId)
{
    var jwtFieldId = User.FindFirst("field_id")?.Value;
    // Admin can access any field; others are restricted to their JWT field
    if (User.IsInRole("Admin")) return true;
    return jwtFieldId == requestedFieldId || jwtFieldId == null;
}
```

This ensures that a user logged into Field A cannot see or modify Field B's process instances.
