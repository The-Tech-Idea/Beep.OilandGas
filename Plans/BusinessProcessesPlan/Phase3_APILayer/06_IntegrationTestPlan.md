# Phase 3 — Integration Test Plan
## WebApplicationFactory-Based API Tests

> Framework: MSTest + `Microsoft.AspNetCore.Mvc.Testing`  
> Location: `Beep.OilandGas.ApiService.Tests/`  
> Test DB: SQLite in-memory (via `WebApplicationFactory` test host)

---

## Test Class Structure

```
Beep.OilandGas.ApiService.Tests/
├── ProcessDefinitionControllerTests.cs
├── ProcessInstanceControllerTests.cs
├── ProcessTransitionControllerTests.cs
├── ProcessAuditControllerTests.cs
└── TestHelpers/
    ├── ProcessApiFactory.cs      (WebApplicationFactory subclass)
    └── ProcessTestDataSeeder.cs  (seeds definitions + test entities)
```

---

## ProcessDefinitionController Tests

| # | Test Method | Scenario | Expected |
|---|---|---|---|
| 1 | `GetAll_Returns200WithAllDefinitions` | GET `/api/process/definitions` | 200; body is `List<ProcessDefinitionResponse>` with 96 items |
| 2 | `GetById_KnownId_Returns200` | GET `/api/process/definitions/WO-PREVENTIVE` | 200; `ProcessId = "WO-PREVENTIVE"` |
| 3 | `GetById_UnknownId_Returns404` | GET `/api/process/definitions/DOES-NOT-EXIST` | 404 |
| 4 | `GetByCategory_Returns_OnlyMatchingDefinitions` | GET `/api/process/definitions/by-category/7` (HSE) | 200; all items have `ProcessCategory = "7"` |
| 5 | `GetTypes_Returns_DistinctList` | GET `/api/process/types` | 200; 8 distinct strings (WORK_ORDER, GATE_REVIEW, etc.) |
| 6 | `GetAll_Unauthenticated_Returns401` | GET `/api/process/definitions` without JWT | 401 |

---

## ProcessInstanceController Tests

| # | Test Method | Scenario | Expected |
|---|---|---|---|
| 1 | `Start_ValidRequest_Returns201WithLocation` | POST `/start` with valid definition + existing entity | 201; `Location` header set; `currentState = "DRAFT"` |
| 2 | `Start_UnknownDefinition_Returns404` | POST `/start` with `processId = "FAKE"` | 404 |
| 3 | `Start_UnknownEntity_Returns404` | POST `/start` with non-existent `entityId` | 404 |
| 4 | `Start_MissingJurisdiction_Returns400` | POST `/start` without `jurisdiction` | 400 + `ValidationProblemDetails` |
| 5 | `GetForField_Returns_OnlyCurrentFieldInstances` | 3 instances in field A seeded; request as field-A user | 200; count = 3 |
| 6 | `GetForField_FilterByState_Returns_Matching` | 3 instances: 1 DRAFT, 1 PLANNED, 1 COMPLETED; `?state=PLANNED` | 200; count = 1 |
| 7 | `GetById_ExistingInstance_Returns200` | GET `/{instanceId}` for known instance | 200; `instanceId` matches |
| 8 | `GetById_UnknownInstance_Returns404` | GET `/{instanceId}` for unknown ID | 404 |
| 9 | `Cancel_ByNonManager_Returns403` | DELETE `/{instanceId}` by `ProcessOperator` role | 403 |
| 10 | `Cancel_ByManager_WithReason_Returns200` | DELETE `/{instanceId}` by `Manager` with reason in body | 200; `currentState = "CANCELLED"` |
| 11 | `Cancel_TerminalInstance_Returns409` | DELETE `/{instanceId}` for COMPLETED instance | 409 |

---

## ProcessTransitionController Tests

| # | Test Method | Scenario | Expected |
|---|---|---|---|
| 1 | `ApplyTransition_ValidNoGuard_Returns200` | Instance in DRAFT; `trigger = "plan"` | 200; `currentState = "PLANNED"` |
| 2 | `ApplyTransition_GuardBlocked_Returns422WithStructuredBody` | Instance in PLANNED, no contractor assigned; `trigger = "start"` | 422; body has `transitionName`, `requiredField`, `regulatoryReference` |
| 3 | `ApplyTransition_InvalidTriggerForState_Returns409` | Instance in COMPLETED; trigger = "plan" | 409 |
| 4 | `ApplyTransition_ApprovalByNonApprover_Returns403` | Instance in UNDER_REVIEW (GATE_REVIEW); trigger = "approve"; user role = `ProcessOperator` | 403 |
| 5 | `ApplyTransition_ApprovalByApprover_WithSufficientApprovers_Returns200` | GATE_REVIEW, 1 approver in PROJECT_STEP_BA, MinApproverCount=1; approver applies "approve" | 200; `currentState = "APPROVED"` |
| 6 | `ApplyTransition_HSEClose_BySafetyOfficer_Returns200` | HSE instance in PENDING_CLOSE_OUT; `SafetyOfficer` role; lessons-learned in reason | 200; `currentState = "CLOSED"` |
| 7 | `ApplyTransition_HSEClose_ByProcessOperator_Returns403` | Same state; role = `ProcessOperator` | 403 |
| 8 | `GetAvailableTransitions_Returns_CorrectTriggers` | Instance in PLANNED (WORK_ORDER) | 200; contains `"start"` and `"cancel"` |
| 9 | `ApplyTransition_WritesAuditRecord` | Any valid transition | After transition, `GET /{id}/audit` returns a record with correct `trigger` and `toState` |
| 10 | `ApplyTransition_WithReason_ReasonPersistedInAudit` | Transition with `reason = "Equipment arrived"` | Audit record `Reason` = `"Equipment arrived"` |

---

## ProcessAuditController Tests

| # | Test Method | Scenario | Expected |
|---|---|---|---|
| 1 | `GetAuditTrail_Returns_AllTransitions` | Instance with 3 recorded transitions | 200; array count = 3 |
| 2 | `GetAuditTrail_FilteredByDateRange_Returns_Correct` | 3 transitions; `?from` excludes first | 200; count = 2 |
| 3 | `GetAuditTrail_ByNonAuditor_Returns403` | GET `/{id}/audit` by `Viewer` role | 403 |
| 4 | `GetAuditSummary_Returns_StateVisitCounts` | Instance went DRAFT→PLANNED→IN_PROGRESS→ON_HOLD→IN_PROGRESS→COMPLETED | `stateVisitCounts["IN_PROGRESS"] = 2`, `totalTransitions = 5` |
| 5 | `GetByUser_Returns_OnlyThatUsersTransitions` | 2 users each applied 2 transitions | GET by-user for user A returns 2 records |

---

## ProcessApiFactory Setup

```csharp
public class ProcessApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real DB connection with SQLite in-memory for tests
            services.Configure<BeepConnectionOptions>(o =>
            {
                o.ConnectionName = "TEST_SQLITE";
                o.ConnectionString = "Data Source=:memory:";
                o.DatabaseType = "SQLite";
            });
        });

        builder.UseEnvironment("Testing");
    }
}
```

---

## Test Data Seeder

All tests that need a running instance call `ProcessTestDataSeeder.SeedMinimalAsync(client, userId)` which:
1. Ensures `WO-PREVENTIVE` definition exists (idempotent seed)
2. Creates a test `EQUIPMENT` entity
3. Returns a started process instance in `DRAFT` state
