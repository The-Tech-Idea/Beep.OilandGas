# Phase 3 — API Layer Overview
## ASP.NET Core Controllers for Business Process Engine

> **Status**: Not started  
> **Depends on**: Phase 2 (`IProcessService`, `ProcessDefinition`, `ProcessInstance` models)  
> **Blocks**: Phase 4 (Blazor UI)  
> **Owner**: Backend Dev  
> **Estimated effort**: 3–4 sprints (~6–8 weeks)

---

## Document Index

| # | Document | Purpose |
|---|---|---|
| This file | `00_Overview.md` | Goals, milestones, key files, dependency map |
| [01](01_ControllerDesign.md) | `01_ControllerDesign.md` | All 4 controller classes, route tables, action method signatures |
| [02](02_RequestResponseModels.md) | `02_RequestResponseModels.md` | Request/response model classes with validation attributes |
| [03](03_AuthorizationMatrix.md) | `03_AuthorizationMatrix.md` | Role-to-endpoint matrix, claim requirements, JWT configuration |
| [04](04_OpenAPISpec.md) | `04_OpenAPISpec.md` | Swagger/OpenAPI annotations, example payloads, version strategy |
| [05](05_ErrorHandlingCatalog.md) | `05_ErrorHandlingCatalog.md` | HTTP status codes per error type, ProblemDetails format, middleware |
| [06](06_IntegrationTestPlan.md) | `06_IntegrationTestPlan.md` | WebApplicationFactory-based test cases, test data setup |
| [07](07_SprintPlan_RACI.md) | `07_SprintPlan_RACI.md` | Sprint stories, RACI matrix, risk register, Definition of Done |

---

## Goals

1. Expose all `IProcessService` operations over REST at `/api/field/current/process/*`
2. Expose process definition catalog via `/api/process/definitions` for UI dropdown population
3. Provide a transition endpoint with typed guard-failure responses so the UI can show actionable messages
4. Support SSE/SignalR progress push for long-running operations (start, bulk transitions)
5. Enforce row-level field isolation: every process endpoint scopes to `IFieldOrchestrator.CurrentFieldId`

---

## Out of Scope (Phase 3)

- Analytics queries (Phase 5)
- Work-order-specific scheduling logic (Phase 6)
- HSE reporting dashboard (Phase 7)
- Regulatory filing integrations (Phase 9)

---

## Key Files

| File Path | Action | Notes |
|---|---|---|
| `Beep.OilandGas.ApiService/Controllers/Process/ProcessDefinitionController.cs` | CREATE | Catalog endpoints |
| `Beep.OilandGas.ApiService/Controllers/Process/ProcessInstanceController.cs` | CREATE | CRUD + transition endpoints |
| `Beep.OilandGas.ApiService/Controllers/Process/ProcessTransitionController.cs` | CREATE | Dedicated transition endpoint |
| `Beep.OilandGas.ApiService/Controllers/Process/ProcessAuditController.cs` | CREATE | Audit trail read endpoints |
| `Beep.OilandGas.ApiService/Program.cs` | MODIFY | Register 4 new controllers (automatic with `AddControllers()`) |
| `Beep.OilandGas.Models/Data/Process/ProcessRequests.cs` | CREATE | All request models |
| `Beep.OilandGas.Models/Data/Process/ProcessResponses.cs` | CREATE | All response models |
| `Beep.OilandGas.ApiService/Middleware/ProcessGuardExceptionMiddleware.cs` | CREATE | Maps `ProcessGuardException` → 422 |

---

## Milestones

| ID | Target | Acceptance Criteria |
|---|---|---|
| M3-1 | Sprint 3.1 | `ProcessDefinitionController` returns all 96 definitions; Swagger shows 200 response shape |
| M3-2 | Sprint 3.2 | `ProcessInstanceController` `POST /start` creates instance; `GET /field` returns field-scoped list |
| M3-3 | Sprint 3.3 | `ProcessTransitionController` `POST /apply` returns `422 Unprocessable` with `ProcessGuardProblem` when guard fires |
| M3-4 | Sprint 3.4 | All integration tests green; zero `TODO` in controller files |

---

## Dependency Diagram

```
                  Phase 2 (IProcessService)
                          │
            ┌─────────────┴────────────────────┐
            │                                  │
  ProcessDefinitionController      ProcessInstanceController
  GET /definitions                 POST /start
  GET /definitions/{id}            GET /field
  GET /types                       GET /{id}
            │                      PATCH /{id} (partial update)
            │                                  │
            └─────────────┬────────────────────┘
                          │
              ProcessTransitionController
              POST /{instanceId}/transition
              GET  /{instanceId}/available-transitions
                          │
              ProcessAuditController
              GET  /{instanceId}/audit
```

---

## Standards Traceability

| Requirement | Standard | Controller | Endpoint |
|---|---|---|---|
| JWT Bearer on all endpoints | OAuth 2.0 / RFC 7519 | All | `[Authorize]` |
| Field-scoped isolation | PPDM; CAPL JOA Art. IX | `ProcessInstanceController` | `CurrentFieldId` filter |
| Guard failure must return structured error | OGC API; REST best practices | `ProcessTransitionController` | 422 + `ProcessGuardProblem` |
| Audit trail accessible | SEMS §250.1932; ISO 55001 §9.1 | `ProcessAuditController` | `GET /{id}/audit` |
| OpenAPI documentation | OAS 3.0 | All | Swagger annotations |
