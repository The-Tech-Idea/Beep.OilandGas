# ProspectIdentification HTTP surface vs lifecycle workflows

**Purpose:** Map **`ProspectIdentificationController`** routes to seeded process IDs (`ExplorationReferenceCodes.ProcessEngine`, `ProcessDefinitionInitializer`) so engineers know **where workflow state advances** versus **where only prospect data or analysis DTOs are read/written**.

---

## Authoritative workflow HTTP entry

| Process ID (seed) | HTTP owner | Notes |
|---------------------|------------|-------|
| **`LEAD_TO_PROSPECT`** | **`ExplorationController`** (field-scoped) | Lead creation, evaluation, approval, prospect creation after **`PROSPECT_CREATION`** step; uses **`ILeadExplorationService`**, **`IFieldExplorationService`**, **`EnsureLeadInFieldForWorkflowStartAsync`**, etc. |
| **`PROSPECT_TO_DISCOVERY`** | **`ExplorationController`** | Step POSTs (risk, volume, economics, drilling decision, discovery recording); field guards on prospect / discovery. |
| **`GATE_EXPLORATION_REVIEW`** | **`ExplorationController`** | Gate / approval pattern aligned with exploration review. |

Catalog cross-reference stays in **`EXPLORATION_SURFACE.md`** (`EXP-*` rows).

---

## `ProspectIdentificationController` — alignment by route group

| Route group | Relation to workflows |
|-------------|----------------------|
| **`/api/prospect/*`** (legacy compatibility: identify, risk, get, volumetrics, rank, portfolio) | **Data / projection helpers**, not process-instance drivers. Results feed **PROSPECT_TO_DISCOVERY**-style decisions in the UI or integrations, but these endpoints **do not** validate **`IProcessService`** instances or step IDs. Prefer **`ExplorationController`** when the caller must respect active field + running workflow. |
| **`api/ProspectIdentification`** (`GET`, `POST`, `POST rank`, **`evaluate/{id}`**) | Same as above: **live prospect service** (`**IProspectIdentificationService**`) without tying to a specific **`ProcessInstanceId`**. |
| **`api/ProspectIdentification/workflow/*`** (partial in **`*.WorkflowAnalysis.cs`**) | **Deterministic analysis** (`**IProspectTechnicalMaturationService**`, **`IProspectRiskEconomicAnalysisService**`, **`IProspectPortfolioOptimizationService**`). Conceptually supports steps such as **risk assessment** and **volume estimation** inside **PROSPECT_TO_DISCOVERY** when the client holds a valid process context; the controller **does not** advance the workflow engine. |

---

## Implementation rules

1. **Never** assume a **`ProspectIdentification`** legacy route has enforced field or process scope unless you add explicit checks (today: **none** on these routes).
2. When adding new prospect writes that must align with **LEAD_TO_PROSPECT** or gate rules, implement them under **`ExplorationController`** / **`LeadExplorationService`** or add the same **`EnsureWorkflowProcessMatchesCurrentFieldAsync`** pattern before widening **`ProspectIdentificationController`**.
3. Constants for process IDs belong in **`ExplorationReferenceCodes`** (partial **`ProcessEngine`**); reference them from docs and tests to avoid string drift.

---

## See also

- **`12_Phase4_Data_Ownership_And_Handoffs.md`** — data-class slices per workflow id, **`ExplorationController`** step POST ↔ **`ExplorationProcessService`** methods, and **`ProcessInstance`** **`EntityId`** rules.

---

## Revision

| Date | Change |
|------|--------|
| 2026-04-27 | Initial alignment sheet; closes Phase 4 “map operations to workflow IDs” for HTTP surface documentation. |
