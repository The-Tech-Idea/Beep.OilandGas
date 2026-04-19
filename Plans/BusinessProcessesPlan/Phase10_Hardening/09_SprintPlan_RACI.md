# Phase 10 — Sprint Plan & RACI Matrix
## Hardening, Security, Documentation, and Go-Live

---

## Sprint Breakdown

### Sprint 10.1 — Security Hardening (2 weeks)

| Story ID | Story | Points | Owner | Deliverable |
|---|---|---|---|---|
| 10.1.01 | Implement `SecurityHeadersMiddleware` with all OWASP headers | 3 | BD | Middleware registered in pipeline |
| 10.1.02 | JWT refresh token rotation (15min/7day); SHA-256 hash stored in PPDM | 5 | BD | Token rotation tested |
| 10.1.03 | `AspNetCoreRateLimiter` sliding window (1000 req/min) per user | 3 | BD | 429 returned on breach |
| 10.1.04 | SSRF allow-list in `IntegrationAdapterFactory.ValidateAdapterUrls` | 3 | BD | Private IP addresses rejected |
| 10.1.05 | OWASP pen test PT-01 to PT-10 execution and evidence capture | 8 | QA | All 10 tests PASS; PT results in `08_ComplianceEvidence.md` |
| 10.1.06 | Secrets migration: move remaining config secrets to Azure Key Vault | 5 | DevOps | Zero secrets in `appsettings.json` on main |
| 10.1.07 | CI/CD `.github/workflows/ci.yml` with OWASP dependency scan | 3 | DevOps | Build gate fails on critical CVE |
| **Total** | | **30 SP** | | |

### Sprint 10.2 — Performance, Deployment, and Go-Live (2 weeks)

| Story ID | Story | Points | Owner | Deliverable |
|---|---|---|---|---|
| 10.2.01 | Query profiling: identify all endpoints > p95 500ms threshold | 5 | BD | Slow-query list documented |
| 10.2.02 | Add composite indexes for top 5 slow tables (AER ST-39 export, OBLIGATION, HSE_INCIDENT) | 5 | BD | p95 benchmarks met per `02_PerformanceTuning.md` |
| 10.2.03 | `IMemoryCache` LOV + KPI caching with configurable TTL | 3 | BD | Cache-aside pattern verified |
| 10.2.04 | Helm chart completed; tested on AKS dev namespace | 5 | DevOps | `helm install` succeeds on clean cluster |
| 10.2.05 | `docker-compose.yml` validated; all containers build and start | 3 | DevOps | All health checks green |
| 10.2.06 | DR test runbook executed; RTO ≤ 4h verified | 5 | DevOps | Restore drill log added to repo |
| 10.2.07 | Operator manuals reviewed by field ops team (USA, Canada, International) | 5 | PM | Sign-off documented for each manual |
| 10.2.08 | Go-live checklist 20 items verified (see below) | 3 | PM | All items checked off |
| **Total** | | **34 SP** | | |

**Combined: 64 SP across 2 sprints**

---

## Go-Live Checklist (20 Items)

| # | Item | Owner | Status |
|---|---|---|---|
| 1 | All OWASP PT-01 to PT-10 test cases PASS | QA | ☐ |
| 2 | JWT refresh rotation tested in staging | BD | ☐ |
| 3 | Rate limiting verified at 1000 req/min | QA | ☐ |
| 4 | SSRF validation tested with cloud metadata IP | QA | ☐ |
| 5 | No secrets in `appsettings.json` (Key Vault verified) | DevOps | ☐ |
| 6 | CI/CD pipeline green on main branch | DevOps | ☐ |
| 7 | p95 benchmarks met for all 6 KPI endpoints | BD | ☐ |
| 8 | DR restore drill completed within RTO 4h | DevOps | ☐ |
| 9 | All SQL Scripts run successfully against production schema | DevOps | ☐ |
| 10 | PPDM seeder data loaded for all R_* tables | BD | ☐ |
| 11 | Helm chart deployed to production namespace successfully | DevOps | ☐ |
| 12 | USA operator manual signed off by USA field ops | PM | ☐ |
| 13 | Canada operator manual signed off by Canada field ops | PM | ☐ |
| 14 | International operator manual signed off by intl operations | PM | ☐ |
| 15 | SOC 2 TSC evidence package assembled in `08_ComplianceEvidence.md` | CS | ☐ |
| 16 | ISO 27001 Annex A control evidence reviewed | CS | ☐ |
| 17 | BSEE SEMS 14-element annual obligation auto-created for current year | QA | ☐ |
| 18 | Training sessions completed for all roles (USA + Canada + International) | PM | ☐ |
| 19 | Monitoring alerts configured in Azure Monitor (latency, 5xx rate, integration health) | DevOps | ☐ |
| 20 | Rollback procedure documented and tested | DevOps | ☐ |

---

## RACI Matrix

| Activity | BD (Dev) | FE (Frontend) | QA | DevOps | CS (Compliance SME) | PM |
|---|---|---|---|---|---|---|
| Security middleware implementation | R | I | A | I | C | I |
| OWASP pen testing | C | I | R | C | I | A |
| Performance index optimization | R | I | A | C | I | I |
| Helm / Docker deployment | C | I | I | R | I | A |
| DR restore drill | C | I | C | R | I | A |
| Operator manual review | C | C | I | I | C | R |
| Go-live checklist sign-off | C | C | C | C | C | R |
| Compliance evidence assembly | C | I | C | I | R | A |

---

## Risk Register

| Risk ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R10-01 | AKS cluster provisioning delayed | Medium | High | Parallel docker-compose testing; fall back to VM deploy first go-live |
| R10-02 | Pen test finds critical OWASP finding late in sprint | Low | High | Early PT execution (Sprint 10.1 day 5); remediation buffer built into sprint |
| R10-03 | Operator manual review incomplete before go-live | Medium | Medium | Conditional go-live with USA only; Canada/International to follow within 2 weeks |
| R10-04 | Key Vault secret migration causes service break | Medium | High | Blue-green deployment; rollback to previous pod revision |
| R10-05 | DR drill exceeds 4h RTO | Low | High | Automate restore script `Scripts/DR/RestoreDatabase.ps1`; re-drill until target met |

---

## Definition of Done — Phase 10

- [ ] All 10 OWASP API Top 10 (2023) pen tests PASS
- [ ] DR restore drill completed with RTO ≤ 4 hours documented
- [ ] p95 < 500 ms for all KPI and compliance report endpoints
- [ ] All 3 operator manuals reviewed and signed off
- [ ] SOC 2 and ISO 27001 evidence package complete
- [ ] CI/CD pipeline green with OWASP dependency scan gate
- [ ] Helm chart deployed and stable in production for 48 hours
- [ ] 20-item go-live checklist 100% checked off
