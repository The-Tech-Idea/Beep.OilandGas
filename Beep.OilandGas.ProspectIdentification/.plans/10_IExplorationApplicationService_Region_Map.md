# IExplorationApplicationService — region map (roadmap vs live)

**Contract:** `Beep.OilandGas.ProspectIdentification/Services/IExplorationApplicationService.cs`  
**Status:** Not registered in DI. Live HTTP and field workflows use **`IProspectIdentificationService`** plus **`IProspectTechnicalMaturationService`**, **`IProspectRiskEconomicAnalysisService`**, **`IProspectPortfolioOptimizationService`** (`Beep.OilandGas.Models.Core.Interfaces`), **`ISeismicAnalysisService`**, **`IProspectEvaluationService`**, **`IFieldExplorationService`**, and **`ILeadExplorationService`** as appropriate.

This sheet satisfies **Phase 3** of `04_Execution_Plan.md`: each `#region` on the interface is classified as **deferred**, **partial overlap** (conceptual or subset), or **implemented elsewhere** (with pointer).

---

## Interface method regions (lines ~20–210)

| `#region` | Roadmap methods (summary) | Classification | Live / planned substitute |
|-----------|---------------------------|----------------|---------------------------|
| **Workflow Management** | Project CRUD, status, list | **Deferred** | No `ProspectIdentificationProject` persistence or API. Future: exploration program / workspace tables or thin facade over lifecycle process instances. |
| **Data Acquisition & Integration** | Import seismic/well, integrate, QC | **Partial overlap** | **`ISeismicAnalysisService`** — PPDM **`SEIS_ACQTN_SURVEY`** CRUD (not project-scoped). Well import / multi-source integration / project-scoped QC — **deferred**. |
| **Prospect Generation** | Generate from seismic/geology/play, merge | **Deferred** | **`ILeadExplorationService`** + field **`PROSPECT`** creation for lead path; **`IProspectIdentificationService.CreateProspectAsync`** for direct create. No bulk “generated prospect” pipeline. |
| **Prospect Evaluation Pipeline** | Full pipeline, rapid screening, detailed eval, status | **Partial overlap** | **`IProspectIdentificationService`** — evaluate + rank. **`IProspectEvaluationService`** — broader evaluation/risk/volumetric helpers on **`PROSPECT`**. Workflow POSTs — maturation / risk-economics. No orchestrated multi-stage “project pipeline” entity. |
| **Portfolio Management** | Create portfolio, optimize, risk, drilling schedule | **Partial overlap** | **`IProspectPortfolioOptimizationService.OptimizePortfolioAsync`** + **`IProspectIdentificationService.RankProspectsAsync`**. Portfolio create / portfolio-level risk / drilling schedule — **deferred**. |
| **Collaboration & Review** | Assign, progress, QA, escalate | **Deferred** | No handlers or tables wired. |
| **Reporting & Analytics** | Project report, inventory, analytics, export | **Partial overlap** | **`IProspectEvaluationService`** / **`ISeismicAnalysisService`** — prospect- and survey-level reports/exports. Project-scoped aggregate reports — **deferred**. |
| **Decision Support** | Recommendations, scenario, VOI, presentation | **Deferred** | No API or persistence. |

---

## DTO regions in the same file (lines ~213+)

| `#region` | Classification | Notes |
|-----------|----------------|-------|
| **Project Management DTOs** | **Roadmap types** | Used only if/when Workflow Management is implemented. |
| **Data Management DTOs** | **Mixed** | **`SeismicSurvey`** etc. align with **`Models.Data`** seismic shapes used by **`ISeismicAnalysisService`**; import/integration wrappers remain roadmap-only. |
| **Prospect Generation DTOs** | **Roadmap types** | |
| **Evaluation DTOs** | **Partial overlap** | Overlap with **`Models.Data`** evaluation types consumed by **`IProspectEvaluationService`** / workflow requests; not all roadmap DTOs have endpoints. |
| **Portfolio Management DTOs** | **Partial overlap** | **`PortfolioOptimizationResult`** / ranking types align with workflow portfolio optimization; full portfolio entity CRUD deferred. |
| **Request DTOs** | **Roadmap types** | Wire when matching regions go live. |

---

## Verification

- Grep **`IExplorationApplicationService`** in **`Beep.OilandGas.ApiService`** — should find **no** `AddScoped` / constructor injection (roadmap-only).
- Live workflow analysis: **`ProspectIdentificationController.WorkflowAnalysis.cs`** + **`ProspectIdentificationControllerWorkflowTests`**.

---

## Revision

| Date | Change |
|------|--------|
| 2026-04-27 | Initial map; Phase 3 checklist closure in `04_Execution_Plan.md`. |
| 2026-04-27 | Interface marked **`[Obsolete]`** (roadmap-only); removal deferred until external consumers are cleared. |
