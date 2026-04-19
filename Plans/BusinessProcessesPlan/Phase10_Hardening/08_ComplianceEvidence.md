# Phase 10 — Compliance Evidence Package
## Test Results Mapped to ISO 27001, SOC 2, OWASP, and Industry Frameworks

---

## ISO 27001:2022 Control Cross-Reference

| ISO Control | Annex A Ref | Test / Evidence |
|---|---|---|
| Access control policy | A.5.15 | JWT role-based claims enforced; test `AUTH-01`: request with `ProcessOperator` token to `/api/admin/*` → 403 |
| Authentication | A.8.5 | JWT 15-min access + 7-day SHA-256 hashed refresh; see `01_SecurityHardening.md §JWT` |
| Cryptography | A.8.24 | AES-256 at rest (SQL TDE); TLS 1.2+ in transit; no MD5 or SHA-1 |
| Logging and monitoring | A.8.15 | Serilog structured logs to file + Azure Monitor; access log captured by `PPDMDataAccessAuditService` |
| Secure development | A.8.25 | GitHub Actions CI with `dotnet test` gate; OWASP dependency check on merge |
| Software testing | A.8.29 | MSTest unit tests (≥80% coverage per Phase); bUnit for UI; `WebApplicationFactory` integration tests |
| Supplier relationships | A.5.19 | Adapter adapter credentials in Azure Key Vault; no secrets in git |
| Incident management | A.5.24 | HSE incident → obligation → notification workflow tested in Phase 7 |
| Business continuity | A.5.29 | DR test runbook in `04_DisasterRecovery.md`; monthly drill required |

---

## SOC 2 Type II Trust Service Criteria (TSC) Cross-Reference

| TSC | Criteria | Evidence |
|---|---|---|
| CC6.1 | Logical access controls | Role claims checked in `[Authorize(Roles="...")]` on every endpoint |
| CC6.2 | Credential management | No hardcoded credentials; all in Key Vault; JWT auto-expiry |
| CC6.3 | Remove access on termination | JWT token revocation via refresh token invalidation (SHA-256 hash deleted from PPDM) |
| CC7.1 | Detection of unauthorized activity | `PPDMDataAccessAuditService` records every CRUD operation; alert on anomaly via Azure Monitor |
| CC8.1 | Change management | GitHub PRs required for all main-branch merges; CI must pass |
| A1.1 | Performance capacity | p95 benchmarks in `02_PerformanceTuning.md`; auto-scaling Helm configuration |
| A1.2 | Recovery | DR objectives: RTO ≤ 4h / RPO ≤ 24h; documented test in `04_DisasterRecovery.md` |

---

## OWASP API Security Top 10 (2023) — Pen Test Results

| PT-ID | OWASP Category | Test Description | Result | Evidence |
|---|---|---|---|---|
| PT-01 | API1 Broken Object Level Auth | Request field B data with field A token | PASS — 403 returned | FieldOrchestrator guard `CurrentFieldId` check |
| PT-02 | API2 Broken Authentication | Replayed expired access token | PASS — 401 returned | JWT `exp` claim validated by ASP.NET JWT middleware |
| PT-03 | API3 Broken Object Property Auth | `PATCH /api/field/current/incidents/:id` with extra fields (e.g., `APPROVAL_USER`) | PASS — extra fields silently ignored via DTO binding | Only mapped properties persisted |
| PT-04 | API4 Unrestricted Resource Consumption | 2000 req/min brute force | PASS — 429 after 1000 req/min threshold | `AspNetCoreRateLimiter` sliding window |
| PT-05 | API5 Broken Function Level Auth | `ProcessOperator` token calls `DELETE /api/admin/users` | PASS — 403 returned | `[Authorize(Roles="Manager")]` on admin controller |
| PT-06 | API6 Unrestricted Access to Sensitive Flows | Register 1000 users in parallel | PASS — account creation rate-limited | Identity server registration endpoint rate-limited |
| PT-07 | API7 Server-Side Request Forgery | Adapter URL set to `http://169.254.169.254` (cloud metadata) | PASS — 400 returned | `IntegrationAdapterFactory.ValidateAdapterUrls` private IP check |
| PT-08 | API8 Security Misconfiguration | Missing `X-Frame-Options` | PASS — present | `SecurityHeadersMiddleware` adds all OWASP-recommended headers |
| PT-09 | API9 Improper Inventory Management | Call undocumented `/api/debug/config` | PASS — 404 in non-Development env | Debug endpoints stripped in Release build |
| PT-10 | API10 Unsafe Consumption of APIs | Malformed WITSML XML injection | PASS — XDocument.Load throws; 400 returned | Adapter input is deserialized via typed XDocument only |

---

## IOGP 2022e Compliance Evidence

| IOGP Requirement | System Evidence |
|---|---|
| §5.2 — Tier 1/2 PSE classification | `HSE_INCIDENT.BARRIER_FAILED_COUNT` drives auto-classification |
| §6.1 — Exposure hours reporting | Pulled from `PDEN_SOURCE.MAN_HOURS` linked to field; shown in KPI dashboard |
| §6.3 — Fatality reporting | Injury type `FATALITY` triggers immediate escalation + IOGP export flag |
| Annual report dataset | Export from **Analytics → HSE KPIs → IOGP Export** produces 2022e CSV |

---

## BSEE SEMS 30 CFR §250 Evidence

| Element | CFR Section | Evidence |
|---|---|---|
| Safety and environmental information | §250.1922 | PPDM equipment data (`EQUIPMENT` table) current and maintained |
| Hazard analysis | §250.1923 | HAZOP studies stored in `PROJECT_STEP_CONDITION` with guide word + severity |
| Management of change | §250.1927 | MOC process workflow in Phase 4 (DevelopmentPlanning) triggers safety review |
| Emergency response | §250.1930 | OPA 90 ER workflow tested; NRC/USCG notification obligations auto-created |
| Investigation of incidents | §250.1931 | RCA workflow to ROOT cause required before incident closure |
| Audits | §250.1934 | Annual BSEE_SEMS_ANNUAL_AUDIT obligation auto-created each January 1 |
