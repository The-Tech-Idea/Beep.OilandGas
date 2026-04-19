# Phase 9 — Integrations Overview
## WITSML, PRODML, SCADA/OPC-UA, Documents, SAP ERP, OSDU

> **Status**: Not started  
> **Depends on**: Phases 2–8 (all services operational)  
> **Owner**: Backend Dev + Integration Architect  
> **External standards**: WITSML 1.4.1, PRODML 2.2, OPC-UA (IEC 62541), OSDU R3, SAP PM

---

## Document Index

| # | Document | Purpose |
|---|---|---|
| This file | `00_Overview.md` | Goals, key files, milestones, adapter matrix |
| [01](01_WITSML_Adapter.md) | `01_WITSML_Adapter.md` | WITSML 1.4.1 → PPDM sync for wells and logs |
| [02](02_PRODML_Adapter.md) | `02_PRODML_Adapter.md` | PRODML 2.2 → production volume sync |
| [03](03_SCADA_OPC-UA.md) | `03_SCADA_OPC-UA.md` | OPC-UA subscription → INSTRUMENT readings; PROD_STRING telemetry |
| [04](04_DocumentManagement.md) | `04_DocumentManagement.md` | SharePoint/OpenText → RM_INFORMATION_ITEM sync |
| [05](05_SAP_ERP.md) | `05_SAP_ERP.md` | SAP PM WO → PROJECT/EQUIPMENT_MAINTAIN sync |
| [06](06_OSDU.md) | `06_OSDU.md` | OSDU R3 Well Delivery → PPDM WELL/POOL/SEIS_SURVEY |
| [07](07_HealthMonitor.md) | `07_HealthMonitor.md` | Health checks, circuit breaker, failure notifications |
| [08](08_SprintPlan_RACI.md) | `08_SprintPlan_RACI.md` | Sprint stories, RACI, risks, Definition of Done |

---

## Goals

1. Pull well drilling data from WITSML into `WELL`, `CASING_PROGRAM`, `LOG` tables
2. Sync production volumes from PRODML into `PDEN_VOL_SUMMARY`
3. Real-time sensor readings via OPC-UA subscriptions into `INSTRUMENT`
4. Document metadata from document management systems into `RM_INFORMATION_ITEM`
5. SAP work orders and costs pulled into `PROJECT` and `FIN_COMPONENT`
6. OSDU well/seismic delivery imported into PPDM master data
7. Circuit-breaker health monitoring with `NOTIFICATION` alerts on failure

---

## Adapter Matrix

| Adapter | Direction | Protocol | PPDM Tables Written |
|---|---|---|---|
| WITSML | Inbound (pull) | HTTP/WSDL | `WELL`, `CASING_PROGRAM`, `LOG`, `LOG_PARAMETER` |
| PRODML | Inbound (pull) | HTTP/REST | `PDEN_VOL_SUMMARY`, `PDEN_VOL_DISPOSITION` |
| OPC-UA | Inbound (push/subscribe) | TCP | `INSTRUMENT`, `PROD_STRING` |
| SharePoint | Inbound (pull) | HTTP/REST | `RM_INFORMATION_ITEM` |
| SAP PM | Inbound (pull) | RFC/REST | `PROJECT`, `PROJECT_STEP`, `FIN_COMPONENT`, `EQUIPMENT_MAINTAIN` |
| OSDU | Inbound (pull) | HTTP/REST | `WELL`, `POOL`, `SEIS_SURVEY`, `RESERVOIR` |

---

## Key Files

| File Path | Action |
|---|---|
| `Beep.OilandGas.Models/Core/Interfaces/IWitsmlAdapter.cs` | CREATE |
| `Beep.OilandGas.Models/Core/Interfaces/IProdmlAdapter.cs` | CREATE |
| `Beep.OilandGas.Models/Core/Interfaces/IScadaOpcUaAdapter.cs` | CREATE |
| `Beep.OilandGas.Models/Core/Interfaces/IIntegrationHealthService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Integrations/WitsmlAdapterService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Integrations/ProdmlAdapterService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Integrations/OpcUaAdapterService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Integrations/IntegrationHealthService.cs` | CREATE |
| `Beep.OilandGas.ApiService/Controllers/Integrations/IntegrationController.cs` | CREATE |

---

## Milestones

| ID | Target | Acceptance Criteria |
|---|---|---|
| M9-1 | Sprint 9.1 | WITSML test pull imports at least one well record |
| M9-2 | Sprint 9.2 | PRODML volume sync updates `PDEN_VOL_SUMMARY` |
| M9-3 | Sprint 9.3 | OPC-UA subscription receives tag values into `INSTRUMENT` |
| M9-4 | Sprint 9.4 | Health monitor fires `NOTIFICATION` on circuit-breaker open |
