# Phase 3 — Sprint Plan & RACI
## API Layer Delivery Schedule

> Total story points: ~72 SP  
> Sprint velocity: 25–28 SP/sprint  
> Target: 3 sprints (~6 weeks) after Phase 2 is complete

---

## Sprint Breakdown

### Sprint 3.1 — Controllers & Models (Weeks 1–2)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| Create `StartProcessRequest`, `ApplyTransitionRequest`, `CancelProcessRequest` request models | 2 | BD | Validated request models |
| Create `ProcessDefinitionResponse`, `ProcessInstanceResponse`, `TransitionOption` response models | 3 | BD | Response model classes |
| Create `ProcessGuardProblem`, `ProcessAuditSummary` | 2 | BD | Exception → ProblemDetails mapping |
| Scaffold `ProcessDefinitionController` — all 5 endpoints | 5 | BD | Routes return stubs, Swagger shows them |
| Scaffold `ProcessInstanceController` — all 6 endpoints | 5 | BD | Routes return stubs |
| `ProcessGuardExceptionMiddleware` + registration | 3 | BD | 422 responses tested manually |
| Register middleware in `Program.cs` | 1 | BD | Middleware in pipeline |
| **Sprint total** | **21 SP** | | |

---

### Sprint 3.2 — Business Logic Implementation (Weeks 3–4)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| Implement `ProcessDefinitionController` action bodies (call `IProcessService`) | 5 | BD | GET definitions returns real data |
| Implement `ProcessInstanceController.StartAsync` with entity validation | 5 | BD | POST `/start` creates real instance |
| Implement `ProcessInstanceController` GET endpoints (field filter, by-entity, active) | 4 | BD | GET `/` returns field-scoped list |
| Scaffold + implement `ProcessTransitionController` both endpoints | 6 | BD | POST `/transition` applies SM transition; 422 on guard |
| Scaffold + implement `ProcessAuditController` all 3 endpoints | 4 | BD | GET `/audit` returns records |
| Authorization role checks in `ProcessTransitionController` | 3 | BD | 403 returned for wrong role |
| **Sprint total** | **27 SP** | | |

---

### Sprint 3.3 — Testing & Documentation (Weeks 5–6)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| Integration tests: `ProcessDefinitionControllerTests` (6 tests) | 3 | QA | 6/6 pass |
| Integration tests: `ProcessInstanceControllerTests` (11 tests) | 5 | QA | 11/11 pass |
| Integration tests: `ProcessTransitionControllerTests` (10 tests) | 5 | QA | 10/10 pass |
| Integration tests: `ProcessAuditControllerTests` (5 tests) | 3 | QA | 5/5 pass |
| Swagger XML doc comments on all action methods | 3 | BD | Swagger renders descriptions and examples |
| Code review & hardening | 3 | BD+QA | Zero open P1 comments |
| **Sprint total** | **22 SP** | | |

---

## RACI Matrix

| Task | BD | QA | DA | DS | CS | HS | PM |
|---|---|---|---|---|---|---|---|
| Request/response model design | **R/A** | C | — | C | — | — | I |
| Controller scaffolding | **R/A** | — | — | — | — | — | I |
| Guard exception middleware | **R/A** | C | — | — | — | — | I |
| Role authorization checks | **R** | C | — | — | C | C | A |
| Swagger annotation | **R/A** | — | — | — | — | — | I |
| Integration test authoring | R | **R/A** | — | C | C | C | I |
| `Program.cs` middleware order | **R/A** | — | — | — | — | — | I |
| Acceptance review of Definition of Done | C | C | — | **A** | A | A | **R** |

---

## Risk Register

| Risk ID | Description | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R3-01 | `IProcessService` not fully implemented when API work starts | Medium | High | Phase 2 must be completed and service injectable before Sprint 3.1 |
| R3-02 | Role names mismatch between JWT issuer and `[Authorize(Roles=...)]` attribute | Low | High | Align role strings with `UserManagement.AspNetCore` role seeds in Sprint 3.1 |
| R3-03 | `ProcessGuardExceptionMiddleware` intercepts unrelated exceptions | Low | Medium | Middleware only catches `ProcessGuardException` + specific `InvalidOperationException` messages; unit-tested separately |
| R3-04 | Swagger XML file not generated (missing `.csproj` flag) | Low | Low | Add `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to `Beep.OilandGas.ApiService.csproj` in Sprint 3.1 |

---

## Definition of Done (Phase 3)

- [ ] All 16 API endpoints return correct HTTP status codes for happy path
- [ ] `ProcessGuardException` produces `422` with `ProcessGuardProblem` response body
- [ ] All 32 integration test cases pass
- [ ] Swagger UI shows all endpoints with descriptions, request schemas, and response examples
- [ ] Authorization matrix verified: wrong-role callers receive `403`; unauthenticated callers receive `401`
- [ ] `dotnet build Beep.OilandGas.sln` exits 0 errors
- [ ] No `TODO` in controller or middleware files
- [ ] Phase 4 team has reviewed Swagger and confirmed UI can consume the API
