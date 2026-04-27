# MASTER-TODO-TRACKER — ProspectIdentification & Exploration Enhancement

Single tracker for revising **data classes → services → process orchestration** across `Beep.OilandGas.ProspectIdentification`, lifecycle **Exploration** services, and alignment with the **business process catalog** (`Plans/BusinessProcessesPlan`, `ProcessDefinitionInitializer`).

**Related repo docs (do not duplicate; extend them):**

- `Beep.OilandGas.ProspectIdentification/.plans/00_Context_Map.md`, `01_Exploration_Module_Sheet.md`, `03_Services_And_Workflows_Sheet.md`, `04_Execution_Plan.md`
- `Beep.OilandGas.LifeCycle/plans/process_Exploration.md`
- `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.cs` (seeded steps)
- `Plans/BusinessProcessesPlan/Phase2_ServiceLayer/03_ProcessDefinitionCatalog.md` (EXP-* IDs)

---

## Executive summary

| Area | Current state | Target state |
|------|----------------|--------------|
| **Data** | Rich table set in `ExplorationModule`; some projections vs PPDM overlap documented in `.plans` | One canonical **prospect/lead/play** model per aggregate; clear **read models** vs **write models**; `PROSPECT_WORKFLOW_STAGE` drives funnel state |
| **Services** | **`IProspectIdentificationService`** + **`IProspectTechnicalMaturationService`** / **`IProspectRiskEconomicAnalysisService`** / **`IProspectPortfolioOptimizationService`** (same scoped `ProspectIdentificationService`); roadmap **`IExplorationApplicationService`** (not DI) | Split **domain application services** (lead, program/budget, permit) + **thin** API facades; implement or drop remaining roadmap methods |
| **Process** | `ExplorationProcessService` delegates to `IProcessService` with step IDs matching initializer | **Step handlers** persist to correct tables; **gate** rules enforced; optional alignment of internal names with catalog `EXP-*` |

---

## Phase 0 — Baseline, ownership, and naming (execution-ready)

**Goal:** Freeze vocabulary so engineering, product, and process catalog use the same workflow IDs.

| # | Task | Target / verification | Status |
|---|------|------------------------|--------|
| 0.1 | Inventory all exploration-related **controllers**, **DI registrations**, and **interface** implementations | `EXPLORATION_SURFACE.md` | Done |
| 0.5 | Seed **`R_LEAD_STATUS`** (`ACTIVE`, `PROSPECT`, `CLOSED`) with exploration module setup | `ExplorationModule.SeedAsync` → `UpsertIfMissingAsync` on `R_LEAD_STATUS` | Done |
| 0.2 | Resolve **duplicate** `IProspectIdentificationService` (`Models.Core.Interfaces` vs `ProspectIdentification/Services`) | Live API = `Models.Core.Interfaces.IProspectIdentificationService`; roadmap = `IExplorationApplicationService` in `Services/IExplorationApplicationService.cs` | Done |
| 0.3 | Map seeded processes to catalog **EXP-*** IDs | Table in `EXPLORATION_SURFACE.md` | Done |
| 0.4 | Confirm canonical **`PROSPECT`** type for all writes (per `.plans/03`) | Grep: no stray `PPDM39.Models.PROSPECT` for new code | Ongoing (grep in CI) |

**Exit criteria:** Grep-based checklist passes; API builds with one prospect contract.

---

## Phase 1 — Data layer and domain aggregates

**Goal:** Every exploration workflow step maps to **persisted** entities or explicit **non-persisted** analysis DTOs.

### 1.1 Core aggregates (always in scope)

| Aggregate | Primary tables / classes | Notes |
|-----------|---------------------------|--------|
| **Prospect** | `PROSPECT`, `PROSPECT_HISTORY`, `PROSPECT_WORKFLOW_STAGE` | Stage transitions audited in history |
| **Lead → Prospect** | `LEAD`, `R_LEAD_STATUS`, promotion to `PROSPECT` | Set `LEAD_ID` on `PROSPECT`; after create, `LEAD.LEAD_STATUS` → `PROSPECT` (seed `R_LEAD_STATUS` accordingly) |
| **Technical maturation** | `PROSPECT_RISK_ASSESSMENT`, `PROSPECT_VOLUME_ESTIMATE`, `PROSPECT_TRAP`, `PROSPECT_SOURCE_ROCK`, `PROSPECT_RESERVOIR`, `PROSPECT_MIGRATION` | One-to-many versions where re-runs are expected |
| **Evidence** | `PROSPECT_SEIS_SURVEY`, `PROSPECT_WELL`, `PROSPECT_ANALOG` | Links to PPDM seismic/well where shared |
| **Economics** | `PROSPECT_ECONOMIC`, sensitivity tables if used | Tie to volume run ID |
| **Discovery** | `PROSPECT_DISCOVERY`, `PROSPECT_FIELD` | Post-drill truth |
| **Portfolio / ranking** | `PROSPECT_RANKING`, `PROSPECT_PORTFOLIO` | Rank within portfolio + as-of date |
| **Program & money** | `EXPLORATION_PROGRAM`, `EXPLORATION_BUDGET`, `EXPLORATION_COSTS` | AFE / program linkage |
| **Regulatory** | `EXPLORATION_PERMIT` | Jurisdiction-specific extensions later |

| # | Task | Verification |
|---|------|--------------|
| 1.1 | Add **FK and uniqueness** rules in code (validators) for prospect-child relationships | Unit tests for invalid orphan rows rejected |
| 1.2 | Define **versioning** for risk/volume/economic (append-only vs update-in-place) | Document per entity; migrations if schema change |
| 1.3 | Align `Prospect`, `ProspectRequest`, `ProspectEvaluation` DTOs in `Beep.OilandGas.Models` with table fields used in UI/API | No dead properties on API DTOs |

---

## Phase 2 — Service decomposition (from “god interface” to workflows)

**Goal:** Replace the monolithic `IExplorationApplicationService` roadmap surface (`Services/IExplorationApplicationService.cs`) with **workflow-scoped** services consumed by API and/or `ExplorationProcessService`.

### 2.1 Recommended service boundaries

| Service | Responsibility | Primary callers |
|---------|----------------|-----------------|
| `ILeadExplorationService` | CRUD `LEAD`, screen, status transitions, start `LEAD_TO_PROSPECT` | API, `ExplorationProcessService` |
| `IProspectMaturationService` | Risk, volumetrics, trap/source/seal/migration edits | API, process steps |
| `IProspectEconomicService` | Run economic cases, link to `PROSPECT_ECONOMIC` | API, process step `ECONOMIC_EVALUATION` |
| `IProspectPortfolioService` | Ranking, portfolio membership, compare | API, gate review |
| `IExplorationProgramService` | Program, budget, costs, permit list by program/field | API, integration with Permits module |
| `IProspectDiscoveryService` | Record discovery, dry hole, sidetrack outcomes | API, `DISCOVERY_RECORDING` step |
| `ISeismicProspectLinkService` | `PROSPECT_SEIS_SURVEY` + PPDM survey entities | Already partial via `SeismicAnalysisService` — align |

| # | Task | Verification |
|---|------|--------------|
| 2.1 | Extract methods from large interface file into above interfaces + **partial** `ProspectIdentificationService` classes (one file per concern) | **Part done** — partials as above; **`IProspectTechnicalMaturationService`**, **`IProspectRiskEconomicAnalysisService`**, **`IProspectPortfolioOptimizationService`** in `Core/Interfaces` + DI (same instance as **`IProspectIdentificationService`**); roadmap **`IExplorationApplicationService`** + catalog-mapped handlers still open |
| 2.0 | **`ILeadExplorationService`** — `LeadExplorationService` (LifeCycle) persists `PROSPECT` + `LEAD_ID` via **`IFieldExplorationService`**; idempotent by `GetProspectForFieldByLeadIdAsync`; `ExplorationProcessService` invokes after `PROSPECT_CREATION` | Done — unit tests: `LeadExplorationServiceTests`, `ExplorationProcessServiceTests`; **`POST …/workflows/reject-lead`**, **`promote-lead-to-prospect`**; **`CancellationToken`** on lead workflow service + controller |
| 2.2 | Implement **only** APIs needed for Phase 3 process MVP; mark advanced methods (VOI, genetic portfolio) as **future** or stub with `NotSupportedException` + doc | Swagger shows accurate availability — **`EnsureWorkflowProcessMatchesCurrentFieldAsync`** on lead + prospect/discovery step POSTs + **`IsProcessInstanceInFieldAsync`** |
| 2.2c | **Start-workflow** field guards: **`GetProspectForFieldAsync`** before prospect→discovery; **`IsProspectDiscoveryInFieldAsync`** (`PROSPECT_DISCOVERY` → prospect in field) before discovery→development; **`EnsureLeadInFieldForWorkflowStartAsync`** before lead→prospect (validates **`LEAD.FIELD_ID`**, persists when unset, **one DB read**) | Done — **`ExplorationController`** returns **404** when IDs are out of scope; **`EXPLORATION_SURFACE.md`**; **`Beep.OilandGas.ApiService.Tests`** green |
| 2.2d | **`PPDMExplorationService`**: single **`LoadLeadWithRepositoryAsync`** (optional throw if **`LEAD`** metadata missing) for **`IsLeadInFieldAsync`**, **`EnsureLeadInFieldForWorkflowStartAsync`**, **`UpdateLeadStatusAsync`** | Done — fewer duplicated **`LEAD`** queries / metadata branches |
| 2.2e | **`ExplorationReferenceCodes`** (partial: domain + **`ProcessEngine`**) — process type / names / IDs, entity types, step IDs, outcomes — used by **`ExplorationProcessService`**, **`LeadExplorationService`**, **`ProcessDefinitionInitializer`**, **`ExplorationModule.ModuleId`**, **`PPDMExplorationService`** / **`ExploratoryWellRequest`** well type, **`FieldLifecycleService`** / **`FieldManagementService`** initial phase | Done — **`ExplorationCategoryToken`** / **`FieldLifecyclePhaseExploration`** unify **`EXPLORATION`** literal; **`ApiService.Tests`** use constants |
| 2.3 | Wire **`IFieldExplorationService`** → `PPDMExplorationService` in DI; orchestrators/controllers inject interface where practical | Done — Api **`Program.cs`**: **`IFieldExplorationService`** scoped + **`GetRequiredService`** into **`FieldOrchestrator`**; **`ExplorationController`** injects interface; **`FieldOrchestrator`** holds injected exploration only (**no** lazy **`PPDMExplorationService`**) |
| 2.3b | Configurable **`LEAD.LEAD_STATUS`** after promotion | `LeadExplorationWorkflowOptions` (`Exploration:LeadWorkflow:PromotedLeadStatusCode`); **`LeadExplorationService`** uses **`IOptions<>`** with fallback to **`ExplorationReferenceCodes.LeadStatusPromotedToProspect`**; template in **`ApiService/appsettings.json`**; **`EXPLORATION_SURFACE.md`** |
| 2.4 | Confirm **`SeismicAnalysisService`** / **`ProspectEvaluationService`** are **`PPDMGenericRepository`**-first (no **`UnitOfWork`**) | **Done** — both use **`SEIS_ACQTN_SURVEY`** + **`PROSPECT`**; optional later: **`SEIS_LINE`**, **`PROSPECT_SEIS_SURVEY`**, richer evidence |

---

## Phase 3 — Process layer: business workflows (detailed)

Each subsection lists **business intent**, **actors**, **data in/out**, **system steps**, and **alignment** to existing seed (`ProcessDefinitionInitializer`) or catalog (`EXP-*`).

---

### WF-A — Play & basin screening (inventory)

**Industry:** Play fairway definition, common risk segment, analog screening.  
**Catalog:** feeds `EXP-GG-DATA-ACQUIRE`, `EXP-LEAD-ASSESS`.

| Step | Business action | Data written | Service |
|------|-----------------|--------------|---------|
| A1 | Define / update **PLAY** | `PLAY`, `R_PLAY_TYPE` | New `IPlayExplorationService` or fold into `ILeadExplorationService` |
| A2 | Attach prospects/leads to play | `PROSPECT_PLAY` | Maturation service |
| A3 | Play-level statistics | `PlayStatistics` (if persisted) or reporting DB | Read-side |

**Gate:** Play “active” for maturation when corporate criteria met (config table or feature flags).

---

### WF-B — G&G data acquisition & QC

**Industry:** Seismic spec, acquisition, processing milestones, interpretation products, well log loads.  
**Catalog:** `EXP-GG-DATA-ACQUIRE`.

| Step | Business action | Data | Service |
|------|-----------------|------|---------|
| B1 | Register survey / project | PPDM `SEIS_*` + `PROSPECT_SEIS_SURVEY` | `SeismicAnalysisService` + exploration link |
| B2 | QC & version interpretation | Projection or `RM_*` linkage per architecture | Data QC service (shared) |
| B3 | Tie evidence to prospect | `PROSPECT_SEIS_SURVEY` | Link service |

**Process alignment:** Optional subprocess under prospect maturation; not all three seeded processes need to duplicate B1–B3.

---

### WF-C — License, lease, and exploration rights

**Industry:** License rounds, work commitments, relinquishment.  
**Catalog:** `EXP-LICENSE-ACQUIRE`.

| Step | Business action | Data | Service |
|------|-----------------|------|---------|
| C1 | Record exploration permit / license | `EXPLORATION_PERMIT` | `IExplorationProgramService` |
| C2 | Link permit to prospect/program | FK or bridge table | Same + Permits module if authoritative |

**Integration:** Prefer **single source** for permits if `Beep.OilandGas.PermitsAndApplications` owns master data.

---

### WF-D — Lead funnel (screen → promote)

**Industry:** Lead generation from geology/geophysics; kill early.  
**Catalog:** `EXP-LEAD-ASSESS`.  
**Seed:** `LEAD_TO_PROSPECT` (`LEAD_CREATION` → `LEAD_EVALUATION` → `LEAD_APPROVAL` → `PROSPECT_CREATION` → `PROSPECT_ASSESSMENT`).

| Step ID (seed) | Business action | Validation | Persistence | Service / handler |
|----------------|-----------------|------------|---------------|-------------------|
| `LEAD_CREATION` | Create lead | Name, basin/area, source | `LEAD` | `ILeadExplorationService` |
| `LEAD_EVALUATION` | Technical screen | At least one evidence ref | `LEAD` status + notes / attachment ref | Lead service + optional scoring DTO |
| `LEAD_APPROVAL` | Management gate | Eval complete | Approval audit on process instance | `IProcessService` + approver role |
| `PROSPECT_CREATION` | Promote to prospect | Lead approved | `PROSPECT` + `LEAD` → PROMOTED | `PPDMExplorationService.CreateProspectForFieldAsync` + `LEAD_ID` |
| `PROSPECT_ASSESSMENT` | Initial resource idea | Prospect exists | Optional `PROSPECT_VOLUME_ESTIMATE` “concept” | Maturation service |

**Enhancement TODO:** Prefer a **single transaction** spanning process step completion and `ILeadExplorationService` persistence (today: `ExecuteStepAsync` + `CompleteStepAsync`, then `AfterProspectCreationStepCompletedAsync` with idempotent `PROSPECT` insert and `LEAD` status update).

---

### WF-E — Prospect maturation (technical)

**Industry:** Petroleum system, trap, seal, charge timing, volumetrics (PRMS prospect-level), risking (COS/geologic).  
**Catalog:** `EXP-PROSPECT-DISC`, `EXP-PROSPECT-APPROVE`.  
**Seed:** `PROSPECT_TO_DISCOVERY` (steps include `RISK_ASSESSMENT`, `VOLUME_ESTIMATION`, `ECONOMIC_EVALUATION`, `DRILLING_DECISION`, `DISCOVERY_RECORDING` — see initializer for exact sequence).

| Step ID (seed) | Business action | Typical tables | Service |
|----------------|-----------------|----------------|---------|
| `PROSPECT_CREATION` | Ensure prospect shell | `PROSPECT` | Exploration CRUD |
| `RISK_ASSESSMENT` | Document COS elements | `PROSPECT_RISK_ASSESSMENT`, trap/source | `IProspectMaturationService` |
| `VOLUME_ESTIMATION` | STOIIP/GIIP, recovery, P10/50/90 | `PROSPECT_VOLUME_ESTIMATE` | Same |
| `ECONOMIC_EVALUATION` | Price deck, costs, fiscal | `PROSPECT_ECONOMIC` | `IProspectEconomicService` |
| `DRILLING_DECISION` | Drill / defer / drop | Process outcome + `PROSPECT_STATUS` | Process + `PPDMExplorationService.UpdateProspectStatusAsync` |
| `DISCOVERY_RECORDING` | HC found post-drill | `PROSPECT_DISCOVERY`, update `PROSPECT` | `IProspectDiscoveryService` |

**Enhancement TODO:** Each `ExplorationProcessService.Perform*` method should invoke a **step handler** that validates prerequisites (e.g. volume exists before economics) per `process_Exploration.md`.

---

### WF-F — Peer review, QA, and technical assurance

**Industry:** Independent review before gate; data quality defensibility.  
**Data:** `PeerReview` table if present; else process attachments on `PROCESS_STEP_DATA`.

| Step | Action | Verification |
|------|--------|--------------|
| F1 | Assign reviewers | Role-based | AuthZ |
| F2 | Capture sign-off | Process step or `PeerReview` | Integration test |

---

### WF-G — Portfolio, ranking, and capital gate

**Industry:** Compare risked EMV, portfolio diversification, capital limits.  
**Catalog:** `EXP-PROSPECT-APPROVE`.  
**Seed:** `GATE_EXPLORATION_REVIEW` (see initializer).

| Step | Action | Tables | Service |
|------|--------|--------|---------|
| G1 | Rank prospects | `PROSPECT_RANKING` | `IProspectPortfolioService` |
| G2 | Build drill slate | `PROSPECT_PORTFOLIO` | Same |
| G3 | Management decision | Process + status | `ExplorationProcessService` + notifications |

---

### WF-H — Exploration well authorization & execution

**Industry:** AFE, hazard ID, regulatory approval, spud.  
**Catalog:** `EXP-EXPL-WELL-AUTH`, `EXP-WELL-RESULTS`.

| Step | Action | Tables / systems | Notes |
|------|--------|------------------|-------|
| H1 | Link prospect to planned/appraisal well | `PROSPECT_WELL` + PPDM `WELL` | `PPDMExplorationService` exploratory wells |
| H2 | Record well result | Well status + `PROSPECT_DISCOVERY` or dry-hole flag | Hand off to WF-E terminal branches |

---

### WF-I — Farmout / farm-in (optional tranche)

**Catalog:** `EXP-FARMOUT`.  
**Implementation:** Contract module or CRM; exploration only stores **interest** pointers on `PROSPECT_BA` if aligned with JOA data model.

---

### WF-J — Discovery → development handoff

**Industry:** Appraisal program, reserves booking path, FID.  
**Seed:** `DISCOVERY_TO_DEVELOPMENT` (`APPRAISAL`, `RESERVE_ESTIMATION`, `ECONOMIC_ANALYSIS`, `DEVELOPMENT_APPROVAL`).

| Step ID | Business action | Downstream |
|---------|-----------------|------------|
| `APPRAISAL` | Appraisal wells / tests | Development planning module |
| `RESERVE_ESTIMATION` | Bookable volumes | Reserves / `RESERVE_ENTITY` family (when implemented) |
| `ECONOMIC_ANALYSIS` | Full-field or block economics | EconomicAnalysis project |
| `DEVELOPMENT_APPROVAL` | FID | Lifecycle phase transition EXPLORATION → DEVELOPMENT |

**Enhancement TODO:** `FieldLifecycleService` transition should be callable from approved process completion handler only.

---

## Phase 4 — API, integration, and UX contracts

| # | Task | Verification |
|---|------|--------------|
| 4.1 | One controller group per workflow service (or feature folder) | Route naming consistent |
| 4.2 | OpenAPI examples for **each** WF-D/E/G happy path | Contract tests |
| 4.3 | Events (optional): publish `ProspectStageChanged` for analytics | Consumer smoke test |

---

## Phase 5 — Quality, security, and operations

| # | Task | Verification |
|---|------|--------------|
| 5.1 | Row-level security or field-scoped access for prospects | Matches `FieldOrchestrator` access patterns |
| 5.2 | Idempotent process step completion | Retry tests |
| 5.3 | Observability: structured logging on step handlers | Log inspection playbook |

---

## Appendix A — Quick gap list (code vs plan)

| Item | Gap |
|------|-----|
| `ProspectIdentification/Services/IExplorationApplicationService.cs` | Large roadmap surface; **`[Obsolete]`**; not registered — **region disposition:** `.plans/10_IExplorationApplicationService_Region_Map.md` |
| `ExplorationProcessService` | Thin wrapper over **`IProcessService`** for exploration steps; domain persistence for step payloads still mostly in process engine — **handoff map:** `.plans/12_Phase4_Data_Ownership_And_Handoffs.md` |
| `process_Exploration.md` | References `LeadToProspectProcess.cs` etc.; confirm files exist or update plan |
| Business catalog `EXP-*` | Naming differs from `LEAD_TO_PROSPECT`; align or document mapping |
| `Beep.OilandGas.ProspectIdentification.Tests` | **25** xUnit tests — `ProspectIdentificationService` + **`ExplorationReferenceCodes`** gate / process-id drift guards (no live DB); **`Beep.OilandGas.ApiService.Tests`** **93** passed (exploration + cross-module) |

---

## Appendix B — Master checkbox (rollup)

- [x] Phase 0 complete (0.1–0.3 done; 0.4 ad hoc grep)  
- [ ] Phase 1 data rules + DTO alignment  
- [x] Phase 2 exploration **MVP** (2.0, 2.2–2.2e, 2.3–2.3b, 2.4; field guards, `ExplorationReferenceCodes` + initializer alignment, DI / `IFieldExplorationService`) — **still open:** 2.1 remainder (roadmap **`IExplorationApplicationService`** disposition, deeper catalog-aligned splits)  
- [ ] Phase 3 step handlers for WF-D, WF-E, WF-J minimum  
- [ ] Phase 4 API routes + OpenAPI  
- [ ] Phase 5 security + idempotency  

---

*Last updated: rollup + test counts; verification Apr 2026 — `dotnet test` Beep.OilandGas.ProspectIdentification.Tests (**25**), Beep.OilandGas.ApiService.Tests (**93**); `dotnet build` Beep.OilandGas.ApiService.*
