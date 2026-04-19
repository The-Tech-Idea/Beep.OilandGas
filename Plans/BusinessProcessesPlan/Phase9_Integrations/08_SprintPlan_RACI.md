# Phase 9 — Sprint Plan and RACI
## Integrations Module — 4 Sprints (~90 Story Points), RACI, Risks, DoD

---

## Story Table

| ID | Story | Points | Owner | Deliverable |
|---|---|---|---|---|
| 9-01 | All 6 adapter interfaces + `IIntegrationHealthService` | 3 | BD | Interfaces in `Models/Core/Interfaces` |
| 9-02 | `IntegrationHealthService`: circuit breaker + state machine | 5 | BD | `IntegrationHealthService.cs` |
| 9-03 | `WitsmlAdapterService`: well + casing + log sync | 8 | BD | `WitsmlAdapterService.cs` with XML parsing |
| 9-04 | WITSML adapter deduplication via `WITSML_UID` | 3 | BD | Upsert logic tested |
| 9-05 | `ProdmlAdapterService`: volume + disposition sync | 8 | BD | `ProdmlAdapterService.cs` |
| 9-06 | PRODML unit conversion helpers (BBL/MCF/m³) | 2 | BD | `UnitConversionHelper.cs` |
| 9-07 | `OpcUaAdapterService`: tag subscription + buffered flush | 8 | BD | `OpcUaAdapterService.cs` + `OpcUaFlushService.cs` |
| 9-08 | OPC-UA bad quality handling | 2 | BD | Tag with `QUALITY_CODE='BAD'` does not update PROD_STRING |
| 9-09 | `DocumentManagementAdapter`: SharePoint OAuth + RM_INFORMATION_ITEM sync | 5 | BD | `SharePointDocAdapter.cs` |
| 9-10 | `SapErpAdapter`: WO + EQUIPMENT_MAINTAIN + FIN_COMPONENT sync | 8 | BD | `SapErpAdapterService.cs` |
| 9-11 | `OsduAdapter`: well + pool + seismic survey sync | 8 | BD | `OsduAdapterService.cs` |
| 9-12 | NOTIFICATION on circuit breaker open | 3 | BD | Auto-fires when state → Open |
| 9-13 | HealthChecks registration in Program.cs | 2 | BD | All 6 adapters registered; `/health/integrations` responds |
| 9-14 | `IntegrationController`: sync triggers + health endpoints | 5 | BD | Controller with all routes |
| 9-15 | Integration tests (mocked external servers) | 8 | QA | WireMock stubs; 18 test cases |
| 9-16 | `IntegrationHealthDashboard.razor` | 5 | FE | Admin dashboard page |
| 9-17 | Sync history log table (last 50 sync results per adapter) | 3 | FE | Displayed in dashboard |
| 9-18 | Credentials security review: env var injection verified | 2 | QA | No credentials in source; env vars documented |

**Total: ~88 SP across 4 sprints**

---

## Sprint Breakdown

| Sprint | Stories | Focus |
|---|---|---|
| 9.1 | 9-01 to 9-06 | Health service + WITSML + PRODML |
| 9.2 | 9-07 to 9-10 | OPC-UA + Documents + SAP |
| 9.3 | 9-11 to 9-14 | OSDU + notifications + controller |
| 9.4 | 9-15 to 9-18 | Tests + UI + security review |

---

## RACI Matrix

| Task | BD | FE | QA | IA | PM |
|---|---|---|---|---|---|
| Adapter interfaces | R | I | I | C | A |
| Circuit breaker service | R | I | C | A | I |
| WITSML adapter | R | I | C | A | I |
| PRODML adapter | R | I | C | A | I |
| OPC-UA adapter + flush | R | I | C | A | I |
| SharePoint document sync | R | I | I | A | I |
| SAP PM adapter | R | I | I | A | I |
| OSDU adapter | R | I | I | A | I |
| Controller | R | I | C | I | A |
| Integration tests | I | I | R | C | A |
| Health dashboard UI | C | R | C | I | A |
| Credentials security review | I | I | R | C | A |

**IA = Integration Architect**

---

## Risk Register

| ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R9-1 | WITSML server firewall/VPN access not available in dev | High | High | Use WITSML stub server (ETPServer or local SOAP mock) for Sprint 9.1 |
| R9-2 | OPC-UA requires direct network path to SCADA; not available in cloud dev | High | High | OPC-UA UA .NET SDK test server on localhost for Sprint 9.2 |
| R9-3 | SAP REST API not available; uses old RFC BAPI | Med | High | Wrap BAPI in a middleware proxy; expose as REST within company network |
| R9-4 | OSDU data partition entitlements not configured for app registration | Med | Med | Provision entitlements in Sprint 9.3 kickoff; document group names |
| R9-5 | Buffered OPC-UA writes miss data during 60s flush window | Med | Med | Reduce flush to 10s in high-frequency mode; configurable |

---

## Definition of Done

- [ ] All 6 adapters connect to real or mock external systems without exception
- [ ] Upsert logic verified: no duplicate rows after re-sync
- [ ] Circuit breaker opens on 3 consecutive failures; NOTIFICATION created
- [ ] OPC-UA bad quality readings tagged and excluded from PROD_STRING updates
- [ ] No credentials stored in source code or config files; all from env vars
- [ ] HealthChecks endpoint `/health/integrations` returns JSON per adapter
- [ ] `dotnet build Beep.OilandGas.sln` — zero errors
- [ ] ≥ 18 integration test cases passing (with WireMock stubs)
