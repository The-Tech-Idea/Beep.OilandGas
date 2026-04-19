# Phase 7 — HSE Module Overview
## Incident Management, RCA, HAZOP, Barriers, and Emergency Response

> **Status**: Not started  
> **Depends on**: Phase 2 (HSE_INCIDENT state machine), Phase 3 (API), Phase 4 (UI)  
> **Owner**: Backend Dev + HSE Subject-Matter Expert  
> **Regulatory frameworks**: API RP 754, IOGP 2022e, IEC 61882, OSHA 29 CFR 1910, OPA 90, CEPA

---

## Document Index

| # | Document | Purpose |
|---|---|---|
| This file | `00_Overview.md` | Goals, key files, milestones |
| [01](01_IncidentManagement.md) | `01_IncidentManagement.md` | HSE_INCIDENT + related tables; full workflow |
| [02](02_RootCauseAnalysis.md) | `02_RootCauseAnalysis.md` | RCA methodology; Bow-Tie and Fault Tree; PPDM mapping |
| [03](03_HAZOP_HAZID.md) | `03_HAZOP_HAZID.md` | HAZOP/HAZID process definition detail; IEC 61882 node table |
| [04](04_CorrectiveActions.md) | `04_CorrectiveActions.md` | PROJECT_STEP corrective action lifecycle |
| [05](05_BarrierManagement.md) | `05_BarrierManagement.md` | Bow-Tie barrier model; API RP 754 Tier influence |
| [06](06_EmergencyResponse.md) | `06_EmergencyResponse.md` | HSE-EMERGENCY-RESP process; OPA 90/CEPA notification |
| [07](07_KPIReporting.md) | `07_KPIReporting.md` | IOGP 2022e Tier 1–4 frequency rates; leading/lagging KPIs |
| [08](08_SprintPlan_RACI.md) | `08_SprintPlan_RACI.md` | Sprint stories, RACI, risks, Definition of Done |

---

## Goals

1. Full incident lifecycle from report → investigate → RCA → corrective actions → close
2. HAZOP/HAZID study management with node/deviation table
3. Bow-Tie barrier model linked to incidents
4. Emergency response notification workflow (OPA 90 / CEPA)
5. KPI reporting: Tier 1–4 PSE rates, TRIR, leading indicators
6. All data in `HSE_INCIDENT` and related PPDM tables

---

## Key Files

| File Path | Action |
|---|---|
| `Beep.OilandGas.Models/Core/Interfaces/IHSEService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/HSEIncidentService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/RCAService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/BarrierManagementService.cs` | CREATE |
| `Beep.OilandGas.ApiService/Controllers/HSE/HSEController.cs` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/HSE/HSEDashboard.razor` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/HSE/IncidentDetail.razor` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/HSE/HAZOPStudy.razor` | CREATE |

---

## Milestones

| ID | Target | Acceptance Criteria |
|---|---|---|
| M7-1 | Sprint 7.1 | Incident report created; Tier assigned; state machine starts |
| M7-2 | Sprint 7.2 | RCA work items linked; corrective actions assigned |
| M7-3 | Sprint 7.3 | HAZOP study nodes/deviations saved |
| M7-4 | Sprint 7.4 | Emergency response notification workflow fires OPA 90 timeline |

---

## PPDM Tables Used

| Table | Purpose |
|---|---|
| `HSE_INCIDENT` | Incident header |
| `HSE_INCIDENT_BA` | Injured/involved parties |
| `HSE_INCIDENT_CAUSE` | Contributing and root causes |
| `HSE_INCIDENT_COMPONENT` | Equipment/component involved |
| `PROJECT_STEP` | Corrective action work items |
| `PROJECT_STEP_BA` | CA responsible person assignment |
| `PROJECT` | HAZOP study project |
| `PROJECT_STEP_CONDITION` | HAZOP node/deviation table |
| `OBLIGATION` | OPA 90 / CEPA notification obligations |
