# Phase 10 — Hardening and Go-Live Overview
## Security, Performance, Deployment, Disaster Recovery, and Operator Manuals

> **Status**: Not started  
> **Depends on**: All Phases 2–9  
> **Owner**: DevOps + Security + All developers  
> **Standards**: OWASP API Top 10, ISO 27001, SOC 2 Type II checklist

---

## Document Index

| # | Document | Purpose |
|---|---|---|
| This file | `00_Overview.md` | Goals, key files, milestones |
| [01](01_SecurityHardening.md) | `01_SecurityHardening.md` | OWASP API Top 10; JWT rotation; field-level isolation pen-test |
| [02](02_PerformanceTuning.md) | `02_PerformanceTuning.md` | PPDM query profiling; AppFilter index strategy; startup benchmarks |
| [03](03_DeploymentGuide.md) | `03_DeploymentGuide.md` | Docker Compose / Helm chart; env var matrix; DB migration |
| [04](04_DisasterRecovery.md) | `04_DisasterRecovery.md` | RTO/RPO targets; backup schedule; PPDM restore procedure |
| [05](05_USA_OperatorManual.md) | `05_USA_OperatorManual.md` | End-user guide: USA jurisdiction processes |
| [06](06_Canada_OperatorManual.md) | `06_Canada_OperatorManual.md` | End-user guide: Canada jurisdiction processes |
| [07](07_International_OperatorManual.md) | `07_International_OperatorManual.md` | End-user guide: International jurisdiction processes |
| [08](08_ComplianceEvidence.md) | `08_ComplianceEvidence.md` | Audit evidence package; test results → standard cross-reference |
| [09](09_SprintPlan_RACI.md) | `09_SprintPlan_RACI.md` | Sprint stories, go-live checklist, RACI, DoD |

---

## Goals

1. Close all OWASP API Top 10 security gaps identified in pen-test
2. All PPDM queries instrumented with profiling; slow queries optimized
3. Deployable via Docker Compose for dev/test and Helm chart for production Kubernetes
4. RTO ≤ 4 hours, RPO ≤ 24 hours for the PPDM database
5. Operator manuals for all 3 jurisdictions covering all 96 process workflows
6. Audit evidence package ready for ISO 27001 / SOC 2 certification review

---

## Key Files

| File Path | Action |
|---|---|
| `docker-compose.yml` | CREATE at solution root |
| `helm/beep-oilgas/Chart.yaml` | CREATE |
| `helm/beep-oilgas/values.yaml` | CREATE |
| `docs/OperatorManuals/USA.md` | CREATE (alias of `05_USA_OperatorManual.md`) |
| `.github/workflows/ci.yml` | CREATE or EXTEND |
| `Beep.OilandGas.ApiService/Middleware/SecurityHeadersMiddleware.cs` | CREATE |
| `Beep.OilandGas.ApiService/Middleware/RateLimitingMiddleware.cs` | CREATE |

---

## Milestones

| ID | Target | Acceptance Criteria |
|---|---|---|
| M10-1 | Sprint 10.1 | All OWASP Top 10 items addressed; pen-test clean |
| M10-2 | Sprint 10.1 | Slow query list resolved; p95 API response < 500 ms |
| M10-3 | Sprint 10.2 | Docker Compose starts complete stack; Helm chart deploys |
| M10-4 | Sprint 10.2 | DR restore verified; all 3 operator manuals complete |
| M10-5 | Sprint 10.2 | Audit evidence package signed off by compliance officer |
