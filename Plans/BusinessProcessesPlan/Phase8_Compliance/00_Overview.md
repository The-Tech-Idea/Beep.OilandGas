# Phase 8 — Compliance Overview
## USA, Canada, and International Regulatory Obligations

> **Status**: Not started  
> **Depends on**: Phase 2 (COMPLIANCE_REPORT state machine), Phase 3 (API), Phase 5 (Analytics base)  
> **Owner**: Backend Dev + Compliance Subject-Matter Expert  
> **Regulatory frameworks**: ONRR MMS-2014, BSEE 30 CFR 250, EPA 40 CFR 98, AER ST-39/ST-60, CER/NEB, ECCC NIR, OSPAR, IMO MARPOL, EU ETS

---

## Document Index

| # | Document | Purpose |
|---|---|---|
| This file | `00_Overview.md` | Goals, key files, milestones |
| [01](01_USA_Regulatory.md) | `01_USA_Regulatory.md` | ONRR, BSEE SEMS, EPA; OBLIGATION row creation per trigger |
| [02](02_Canada_Regulatory.md) | `02_Canada_Regulatory.md` | AER, CER/NEB, ECCC NIR; Crown royalty forms |
| [03](03_International_Regulatory.md) | `03_International_Regulatory.md` | OSPAR, IOGP, MARPOL, EU ETS; jurisdiction routing |
| [04](04_ObligationTracking.md) | `04_ObligationTracking.md` | OBLIGATION + OBLIG_PAYMENT deep dive; escalation |
| [05](05_RoyaltyCalculation.md) | `05_RoyaltyCalculation.md` | USA ONRR GOR formula; Canada Crown formula; variance |
| [06](06_GHGReporting.md) | `06_GHGReporting.md` | EPA 40 CFR 98 subparts; ECCC NIR; emission factor lookup |
| [07](07_ReportFormats.md) | `07_ReportFormats.md` | EIA-914, OGOR, AER ST-39 column mappings |
| [08](08_SprintPlan_RACI.md) | `08_SprintPlan_RACI.md` | Sprint stories, RACI, risks, Definition of Done |

---

## Goals

1. Auto-create `OBLIGATION` rows for regulatory filings triggered by operational events
2. Track due dates, submission status, and variance to expected royalty
3. GHG emission reporting linked to production volumes and emission factors
4. Royalty calculation for USA (ONRR) and Canada (Alberta Crown) with variance alert
5. All obligation data in PPDM `OBLIGATION` and `OBLIG_PAYMENT` tables

---

## Key Files

| File Path | Action |
|---|---|
| `Beep.OilandGas.Models/Core/Interfaces/IComplianceService.cs` | CREATE |
| `Beep.OilandGas.Models/Core/Interfaces/IRoyaltyCalculationService.cs` | CREATE |
| `Beep.OilandGas.Models/Core/Interfaces/IGHGReportingService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Compliance/ComplianceService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Compliance/RoyaltyCalculationService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Compliance/GHGReportingService.cs` | CREATE |
| `Beep.OilandGas.ApiService/Controllers/Compliance/ComplianceController.cs` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/Compliance/ComplianceDashboard.razor` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/Compliance/ObligationDetail.razor` | CREATE |

---

## PPDM Tables Used

| Table | Purpose |
|---|---|
| `OBLIGATION` | Regulatory obligation header |
| `OBLIG_PAYMENT` | Payment/submission records per obligation |
| `PRODUCTION_WELLBORE` | Production volumes for royalty basis |
| `PDEN_VOL_SUMMARY` | Aggregate volumes by product |
| `GAS_SALES` / `OIL_SALES` | Revenue basis for royalty |
| `R_OBLIG_TYPE` | LOV for obligation types |
| `BUSINESS_ASSOCIATE` | Operator / regulator party |

---

## Milestones

| ID | Target | Acceptance Criteria |
|---|---|---|
| M8-1 | Sprint 8.1 | OBLIGATION rows auto-created on production event triggers |
| M8-2 | Sprint 8.2 | Royalty calculation produces variance report |
| M8-3 | Sprint 8.3 | GHG emission totals report generated from emission factors |
| M8-4 | Sprint 8.4 (optional) | EIA-914 / OGOR format CSV export |
