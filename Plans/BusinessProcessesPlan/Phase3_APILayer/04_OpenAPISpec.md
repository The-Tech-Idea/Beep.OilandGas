# Phase 3 — OpenAPI Specification
## Swagger Annotations, Version Strategy, and Example Payloads

> OpenAPI version: 3.0 (via Swashbuckle.AspNetCore)  
> API version: v1 (process engine endpoints)  
> Base URL: `https://{host}/api`

---

## Swashbuckle Configuration (Program.cs additions)

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Beep Oil & Gas — Process Engine API",
        Version     = "v1",
        Description = "Business process lifecycle management for PPDM39-based oil & gas operations.",
        Contact     = new OpenApiContact { Name = "Beep OilandGas Team" }
    });

    // JWT Bearer scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        Description  = "Enter JWT token issued by the Identity Server"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // XML doc comments (ensure <GenerateDocumentationFile>true</GenerateDocumentationFile> in .csproj)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});
```

---

## Controller XML Doc Annotations

Add these XML doc comments to action methods so Swagger shows descriptions:

```csharp
/// <summary>
/// Start a new process instance for a PPDM entity.
/// </summary>
/// <param name="request">Process ID, entity ID, entity table, jurisdiction, and optional name.</param>
/// <returns>The newly created ProcessInstance.</returns>
/// <response code="201">Process instance created successfully.</response>
/// <response code="400">Validation error in request body.</response>
/// <response code="404">ProcessId or EntityId not found.</response>
[HttpPost("start")]
```

```csharp
/// <summary>
/// Apply a named state machine transition to a process instance.
/// Returns 422 if a guard condition blocks the transition, with structured
/// details including the regulatory reference.
/// </summary>
/// <param name="instanceId">Process instance ID.</param>
/// <param name="request">Trigger name and optional reason.</param>
/// <response code="200">Transition applied; updated instance returned.</response>
/// <response code="400">Invalid request body.</response>
/// <response code="403">Caller does not have the required role for this trigger.</response>
/// <response code="404">Instance not found.</response>
/// <response code="409">Transition not defined for current state (terminal or invalid path).</response>
/// <response code="422">Guard condition blocked the transition. See ProcessGuardProblem in response body.</response>
[HttpPost]
```

---

## Versioning Strategy

Phase 3 ships a single version (`v1`). Version escalation rules:

| Change Type | Version Action |
|---|---|
| New endpoints added | remain `v1` |
| Breaking change to request shape | introduce `v2`, deprecate `v1` with 6-month notice |
| New optional fields on response | remain `v1` (additive, non-breaking) |
| Rename of `ProcessId` or trigger names | `v2` required |

---

## Example Payloads for Swagger "Try It Out"

### POST /api/field/current/process/start

**Request**:
```json
{
  "processId": "WO-PREVENTIVE",
  "entityId":  "PUMPING-UNIT-007",
  "entityTableName": "EQUIPMENT",
  "jurisdiction": "USA",
  "instanceName": "Annual PM — Jan 2025"
}
```

**201 Response**:
```json
{
  "instanceId": "WO-20250115-PRV-007",
  "processId": "WO-PREVENTIVE",
  "instanceName": "Annual PM — Jan 2025",
  "processType": "WORK_ORDER",
  "currentState": "DRAFT",
  "entityId": "PUMPING-UNIT-007",
  "entityTableName": "EQUIPMENT",
  "fieldId": "FIELD-PERMIAN-001",
  "jurisdiction": "USA",
  "startDate": "2025-01-15T14:32:00Z",
  "initiatedBy": "user-jsmith",
  "availableTransitions": [
    { "trigger": "plan",   "label": "Plan Work Order",  "targetState": "PLANNED",    "hasGuard": false },
    { "trigger": "cancel", "label": "Cancel",           "targetState": "CANCELLED",  "hasGuard": true, "regulatoryRef": "Internal policy" }
  ]
}
```

### POST /api/field/current/process/{instanceId}/transition — Success

**Request**:
```json
{ "trigger": "plan", "reason": null }
```

**200 Response**:
```json
{
  "instanceId": "WO-20250115-PRV-007",
  "currentState": "PLANNED",
  "availableTransitions": [
    { "trigger": "start",  "label": "Start Work",  "targetState": "IN_PROGRESS", "hasGuard": true,  "regulatoryRef": "IOGP S-501 §3.2" },
    { "trigger": "cancel", "label": "Cancel",      "targetState": "CANCELLED",   "hasGuard": true,  "regulatoryRef": "Internal policy" }
  ]
}
```

### POST /api/field/current/process/{instanceId}/transition — 422 Guard Block

**Request**:
```json
{ "trigger": "start" }
```

**422 Response**:
```json
{
  "title": "Process transition blocked by guard condition",
  "status": 422,
  "detail": "Transition 'start' blocked: Contractor assignment (PROJECT_STEP_BA) is required [IOGP S-501 §3.2]",
  "instance": "/api/field/current/process/WO-20250115-PRV-007/transition",
  "transitionName": "start",
  "requiredField": "Contractor assignment (PROJECT_STEP_BA)",
  "regulatoryReference": "IOGP S-501 §3.2",
  "instanceId": "WO-20250115-PRV-007"
}
```

### GET /api/process/definitions — Catalog Snippet

**200 Response** (truncated):
```json
[
  {
    "processId": "WO-PREVENTIVE",
    "processName": "Preventive Maintenance Work Order",
    "processType": "WORK_ORDER",
    "processCategory": "5",
    "jurisdictionTags": "USA,CANADA,INTERNATIONAL",
    "minApproverCount": 0,
    "requiresDocuments": false,
    "requiredDocumentTypes": [],
    "steps": [
      { "stepId": "WO-PM-01", "stepName": "Draft WO", "sequence": 1, "inboundTrigger": "draft", "primaryPPDMTable": "PROJECT" },
      { "stepId": "WO-PM-02", "stepName": "Plan WO",  "sequence": 2, "inboundTrigger": "plan",  "primaryPPDMTable": "PROJECT_PLAN" }
    ]
  }
]
```
