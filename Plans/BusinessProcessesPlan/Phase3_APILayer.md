# Phase 3 — API Layer: Business Process Endpoints
## Business Process Branch — Detailed Implementation Plan

> **Status**: 🔲 Not Started  
> **Depends on**: Phase 2 (Service Layer)  
> **Owner**: API Layer Team  
> **Standards References**: OpenAPI 3.1, REST API best practices, OAuth 2.0, OWASP API Security Top 10

---

## 3.1 — Objective

Expose all 12 business process categories through a versioned REST API under
`/api/field/current/process/*`, following the FieldOrchestrator convention.

---

## 3.2 — Endpoint Design

### Controller: `BusinessProcessController`
**Route Prefix**: `api/field/current/process`

| Method | Endpoint | Action | Auth |
|---|---|---|---|
| GET | `/definitions` | List all process definitions for field | `[Authorize]` |
| GET | `/definitions/{processId}` | Get single definition | `[Authorize]` |
| GET | `/definitions/category/{categoryId}` | Filter by category | `[Authorize]` |
| GET | `/definitions/jurisdiction/{tag}` | Filter by jurisdiction (`USA`, `CANADA`, `INTERNATIONAL`) | `[Authorize]` |
| POST | `/instances` | Start new process instance | `[Authorize]` |
| GET | `/instances` | List all active instances for field | `[Authorize]` |
| GET | `/instances/{instanceId}` | Get instance status + current step | `[Authorize]` |
| POST | `/instances/{instanceId}/transitions` | Execute state transition | `[Authorize]` |
| GET | `/instances/{instanceId}/history` | Full state transition audit log | `[Authorize]` |
| PATCH | `/instances/{instanceId}/steps/{stepId}` | Update step data / attach documents | `[Authorize]` |
| POST | `/instances/{instanceId}/close` | Close / cancel instance with reason | `[Authorize]` |
| GET | `/templates` | List seed process templates | `[Authorize]` |

### Controller: `GateReviewController`
**Route Prefix**: `api/field/current/gates`

| Method | Endpoint | Action | Auth |
|---|---|---|---|
| POST | `/submit/{gateId}` | Submit for gate review | `[Authorize]` |
| POST | `/{gateId}/approve` | Approve gate (role-checked) | `[Authorize(Roles = "GateApprover")]` |
| POST | `/{gateId}/reject` | Reject gate | `[Authorize(Roles = "GateApprover")]` |
| POST | `/{gateId}/defer` | Defer gate with reason + target date | `[Authorize(Roles = "GateApprover")]` |
| GET | `/{gateId}/checklist` | Retrieve required document checklist | `[Authorize]` |

### Controller: `HSEProcessController`
**Route Prefix**: `api/field/current/hse`

| Method | Endpoint | Action | Auth |
|---|---|---|---|
| POST | `/incidents` | Report new incident | `[Authorize]` |
| GET | `/incidents` | List incidents by severity / status | `[Authorize]` |
| POST | `/incidents/{incidentId}/rca` | Submit root cause analysis | `[Authorize(Roles = "SafetyOfficer,Manager")]` |
| POST | `/incidents/{incidentId}/actions` | Raise corrective actions | `[Authorize]` |
| PATCH | `/incidents/{incidentId}/actions/{actionId}` | Close corrective action | `[Authorize]` |
| POST | `/incidents/{incidentId}/close` | Close incident | `[Authorize(Roles = "SafetyOfficer,Manager")]` |

### Controller: `ComplianceController`
**Route Prefix**: `api/field/current/compliance`

| Method | Endpoint | Action | Auth |
|---|---|---|---|
| GET | `/obligations` | List all obligations for field | `[Authorize]` |
| GET | `/obligations/due` | Obligations due within N days | `[Authorize]` |
| POST | `/reports` | Submit compliance report | `[Authorize]` |
| GET | `/reports/{reportId}/status` | Check submission status | `[Authorize]` |
| POST | `/reports/{reportId}/remediate` | Start remediation workflow | `[Authorize(Roles = "Compliance")]` |

---

## 3.3 — Request / Response Types

All types belong in `Beep.OilandGas.Models/Data/Process/`:

```
Beep.OilandGas.Models/Data/Process/
├── ProcessDefinition.cs              (already exists)
├── ProcessInstance.cs                (already exists)
├── ProcessStepDefinition.cs          (already exists)
├── ProcessTransitionRequest.cs       ← NEW
├── ProcessTransitionResult.cs        ← NEW
├── GateReviewSubmitRequest.cs        ← NEW
├── GateReviewDecisionRequest.cs      ← NEW
├── HSEIncidentReportRequest.cs       ← NEW
├── HSEIncidentUpdateRequest.cs       ← NEW
├── ComplianceReportRequest.cs        ← NEW
├── ComplianceObligationSummary.cs    ← NEW
└── ProcessInstanceSummary.cs         ← NEW
```

---

## 3.4 — PPDM Table Usage Per Endpoint

| Endpoint | Read Tables | Write Tables |
|---|---|---|
| GET `/definitions/*` | `PROJECT`, `PROJECT_PLAN`, `PROJECT_STEP` | — |
| POST `/instances` | `PROJECT`, `POOL`, `WELL`, `FIELD` | `PROJECT`, `PROJECT_STEP`, `PPDM_AUDIT_HISTORY` |
| POST `/instances/{id}/transitions` | Current state in `PROJECT_STEP_CONDITION` | `PROJECT_STEP`, `PROJECT_STEP_CONDITION`, `PPDM_AUDIT_HISTORY` |
| POST `/gates/{id}/approve` | `PROJECT_STEP_BA`, `BA_AUTHORITY` | `PROJECT_STATUS`, `NOTIFICATION`, `NOTIF_BA` |
| POST `/hse/incidents` | — | `HSE_INCIDENT`, `HSE_INCIDENT_BA`, `NOTIFICATION` |
| POST `/hse/incidents/{id}/rca` | `HSE_INCIDENT_CAUSE` | `HSE_INCIDENT_CAUSE`, `HSE_INCIDENT_DETAIL` |
| POST `/compliance/reports` | `OBLIGATION`, `PDEN_VOL_SUMMARY` | `NOTIFICATION`, `OBLIGATION` |

---

## 3.5 — Responsibility Matrix (RACI)

| Task | API Dev | Backend Dev | QA | Security | Domain SME | Compliance |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| 3.1 — BusinessProcessController | R/A | C | C | I | I | I |
| 3.2 — GateReviewController | R | C | C | C | C | A |
| 3.3 — HSEProcessController | R | C | C | C | A | C |
| 3.4 — ComplianceController | R | C | C | C | I | R/A |
| 3.5 — Request/Response model classes | A | R | C | I | C | I |
| 3.6 — Swagger documentation + examples | R/A | C | C | I | I | I |
| 3.7 — Role-based auth rules per endpoint | C | C | C | R/A | I | C |
| 3.8 — Integration tests | C | C | R/A | I | I | I |
| 3.9 — API contract review (OpenAPI spec) | R/A | I | C | C | I | I |

---

## 3.6 — Todo Tracker

| ID | Task | Priority | File | Status | Notes |
|---|---|---|---|---|---|
| 3.1 | Create `BusinessProcessController.cs` | HIGH | `ApiService/Controllers/BusinessProcess/` | 🔲 | GET definitions + POST instances + PATCH steps |
| 3.2 | Implement `GateReviewController.cs` | HIGH | `ApiService/Controllers/BusinessProcess/` | 🔲 | Require `[Authorize(Roles = "GateApprover")]` on approve/reject |
| 3.3 | Implement `HSEProcessController.cs` | HIGH | `ApiService/Controllers/BusinessProcess/` | 🔲 | IOGP 2022e incident classification enum |
| 3.4 | Implement `ComplianceController.cs` | HIGH | `ApiService/Controllers/BusinessProcess/` | 🔲 | Obligation due-date filter using AppFilter |
| 3.5 | Add `ProcessTransitionRequest/Result` model classes | HIGH | `Models/Data/Process/` | 🔲 | Include `TransitionName`, `Comment`, `EffectiveDate` |
| 3.6 | Add `GateReviewSubmitRequest` + `GateReviewDecisionRequest` models | HIGH | `Models/Data/Process/` | 🔲 | Decision enum: `Approve/Reject/Defer` |
| 3.7 | Add `HSEIncidentReportRequest` model (maps to `HSE_INCIDENT` columns) | HIGH | `Models/Data/Process/` | 🔲 | Fields: `IncidentType`, `Severity` (API-754 Tier), `Location`, `BA` |
| 3.8 | Add `ComplianceReportRequest` model | MEDIUM | `Models/Data/Process/` | 🔲 | Fields: `JurisdictionTag`, `ObligationType`, `PeriodStart`, `PeriodEnd` |
| 3.9 | Register all 4 controllers in `Program.cs` | HIGH | `ApiService/Program.cs` | 🔲 | verify DI order — services registered before controllers resolved |
| 3.10 | Add Swagger XML comments + OpenAPI examples for all endpoints | MEDIUM | all controller files | 🔲 | Use `[ProducesResponseType]` attributes |
| 3.11 | Integration test: full process lifecycle (start → complete) | MEDIUM | `Tests/Integration/ProcessLifecycleTests.cs` | 🔲 | Uses in-memory PPDM test dataset |
| 3.12 | Integration test: gate rejection + resubmit path | MEDIUM | `Tests/Integration/GateReviewFlowTests.cs` | 🔲 | |
| 3.13 | Integration test: HSE incident lifecycle | MEDIUM | `Tests/Integration/HSEIncidentFlowTests.cs` | 🔲 | |

---

## 3.7 — Definition of Done (Phase 3)

- [ ] All 4 controllers compile and routes appear in Swagger UI
- [ ] All endpoints return correct HTTP status codes (200/201/400/401/403/404/422)
- [ ] Role-based authorization enforced on approve/reject/close endpoints
- [ ] `ProcessTransitionRequest.TransitionName` validated against allowed state machine transitions
- [ ] All integration tests green
- [ ] `dotnet build Beep.OilandGas.sln` — 0 errors
