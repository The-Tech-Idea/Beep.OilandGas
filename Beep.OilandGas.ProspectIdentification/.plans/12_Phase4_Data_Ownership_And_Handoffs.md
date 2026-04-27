# Phase 4 — Data ownership by workflow ID and exploration handoffs

This document closes the remaining **Phase 4** items in `04_Execution_Plan.md`: **folder/slice ownership** tied to seeded workflow IDs, **explicit handoff** routes for risk / volume / economics / discovery, and **entity ID** rules so process instances stay aligned with real **`LEAD`** / **`PROSPECT`** (and **`DISCOVERY`**) keys.

---

## 1. Workflow ID → data slice (target folder ownership)

Target layout is defined in **`02_Data_Folder_Sheet.md`** and **`07_Target_Slice_Folder_Class_Mapping.md`** (referenced from the planning bundle). Use this **runtime** mapping when placing new table-backed or projection types:

| `ExplorationReferenceCodes` process constant | Process name (seed) | Primary anchor `EntityType` | Owning workflow / data slice |
|-----------------------------------------------|----------------------|------------------------------|------------------------------|
| `ProcessIdLeadToProspect` | `LeadToProspect` | `EntityTypeLead` | **`LeadToProspect`** — `LEAD`, promotion payloads, lead status LOVs; field **`PROSPECT`** created after **`PROSPECT_CREATION`** (via **`ILeadExplorationService`**) |
| `ProcessIdProspectToDiscovery` | `ProspectToDiscovery` | `EntityTypeProspect` | **`ProspectToDiscovery`** — `PROSPECT_RISK_ASSESSMENT`, `PROSPECT_VOLUME_ESTIMATE`, economics, drilling decision, `PROSPECT_DISCOVERY` shapes |
| `GATE_EXPLORATION_REVIEW` (`ExplorationReferenceCodes.ProcessIdGateExplorationReview`) | Gate / exploration review | Seeded anchor `EntityType` = **`POOL`** (`EntityTypeExplorationGateReview`); step ids **`StepGateExploration*`** — see `ProcessDefinitionInitializer` | **`GateReviewAndRanking`** — ranking, portfolio, approval snapshots |
| N/A (supporting evidence) | — | — | **`SeismicEvidence`** — `SEIS_ACQTN_SURVEY`, optional `PROSPECT_SEIS_SURVEY` linkage |
| **`Core`** | — | — | Shared **`PROSPECT`** row shape, shared requests used across slices (until duplicate ownership is collapsed per plan) |

New classes should be filed under the slice that **owns the workflow step** that first persists them, not under whichever controller first calls them.

---

## 2. Explicit handoffs — risk, volume, economics, discovery

### 2.1 Authoritative HTTP + service chain

All of the following require an **active field** (`FieldOrchestrator.CurrentFieldId`), a valid **`ExplorationWorkflowStepRequest`** (`InstanceId`, `UserId`, optional `StepData`), and **`ExplorationController`** guards (`EnsureWorkflowProcessMatchesCurrentFieldAsync`, etc.) as implemented today.

| Step ID (`ExplorationReferenceCodes`) | `ExplorationController` route (relative to field exploration API) | `ExplorationProcessService` entry |
|---------------------------------------|---------------------------------------------------------------------|-----------------------------------|
| `StepRiskAssessment` | `POST …/workflows/prospect-to-discovery/risk-assessment` | `PerformRiskAssessmentAsync` |
| `StepVolumeEstimation` | `POST …/workflows/prospect-to-discovery/volume-estimation` | `PerformVolumeEstimationAsync` |
| `StepEconomicEvaluation` | `POST …/workflows/prospect-to-discovery/economic-evaluation` | `PerformEconomicEvaluationAsync` |
| `StepDiscoveryRecording` | `POST …/workflows/prospect-to-discovery/discovery-recording` | `RecordDiscoveryAsync` (also completes step on success) |

Each handler delegates to **`IProcessService.ExecuteStepAsync`** with the fixed step id above. Persisted domain tables (e.g. **`PROSPECT_RISK_ASSESSMENT`**) are the responsibility of **process step execution** / future domain handlers inside **`IProcessService`** implementations—not of **`ProspectIdentificationController`**.

### 2.2 Optional pre-step analysis (ProspectIdentification)

| Purpose | Surface | Role |
|---------|---------|------|
| Deterministic maturation / risk math / portfolio math | `POST api/ProspectIdentification/workflow/...` | **Advisory** — UI or integration can call before composing **`StepData`** for **`ExplorationWorkflowStepRequest`**. Does **not** advance **`ProcessInstance`**. |

Rule: **advance state only** through **`ExplorationController`** workflow POSTs after the client holds a **`InstanceId`** from **`StartProspectToDiscoveryProcessAsync`**.

---

## 3. Keep workflow entity IDs tied to real lead / prospect / discovery IDs

### 3.1 `ProcessInstance` fields (authoritative)

From **`Beep.OilandGas.Models.Processes.ProcessInstance`**:

| Property | Source at start | Rule |
|----------|-----------------|------|
| **`InstanceId`** | Returned by **`IProcessService.StartProcessAsync`** | Clients pass this as **`ExplorationWorkflowStepRequest.InstanceId`** for every step POST. |
| **`ProcessId`** | Definition id (e.g. `LEAD_TO_PROSPECT`) | Must match **`ExplorationReferenceCodes`** constants used in **`ProcessDefinitionInitializer`**. |
| **`EntityId`** | **Business key** passed to **`StartProcessAsync`** | For **`ProspectToDiscovery`**, this is the **`PROSPECT`** primary key the field already owns. For **`LeadToProspect`**, it is the **`LEAD`** id. For **`DiscoveryToDevelopment`**, it is the **`PROSPECT_DISCOVERY`** (or product-standard discovery) id—see **`StartDiscoveryToDevelopmentRequest`**. |
| **`EntityType`** | Same call | **`LEAD`**, **`PROSPECT`**, etc. — must match **`ExplorationReferenceCodes.EntityType*`** used at start. |
| **`FieldId`** | Current field at start | Field orchestrator scope; guards on **`ExplorationController`** ensure the anchor row belongs to **`FieldId`**. |

### 3.2 Anti-patterns

- Starting or stepping a workflow with a **synthetic** or display-only id that does not exist on **`PROSPECT`** / **`LEAD`** in the current field.
- Calling **`ProspectIdentification`** legacy **`/api/prospect/*`** routes and assuming **`ProcessInstance.CurrentStepId`** moved—those routes **do not** touch **`IProcessService`**.

---

## 4. Verification checklist

- [ ] New PPDM row types for exploration are placed under the slice from §1.
- [ ] New UI flows that “complete” risk/volume/economics use **`ExplorationController`** routes in §2.1 with real **`InstanceId`**.
- [ ] **`StepData`** keys align with what **`PROCESS_STEP_DATA`** / **`IProcessService`** expects for that step (document per handler when adding persistence).

---

## Revision

| Date | Change |
|------|--------|
| 2026-04-27 | Initial Phase 4 ownership + handoff + entity-id rules. |
