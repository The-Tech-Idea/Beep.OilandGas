# Phase 10 Folder Plan — Hardening and Retirement

> Detailed execution plan for Phase 10  
> Companion overview: `../Phase10-HardeningAndRetirement.md`
> Project knowledge base: `../Projects/INDEX.md`
> Evidence manifest: `ScanEvidence.md`

---

## Purpose

Phase 10 closes the update stream across the full solution. It validates the new structure, removes dead paths, reduces dependencies, and leaves the web/API/domain boundary in a maintainable state.

---

## Whole-Solution Pass Model

All Beep.OilandGas solution projects participate in this phase. The question for each project is no longer only “what changes do we make?” but also “what can now be retired, simplified, or validated as stable?” Use `ProjectCoverageMatrix.md` as the authoritative project checklist.

Use the per-project docs as the baseline for deciding whether a page, controller family, client seam, or project reference is duplicate, canonical, thin-but-legitimate, or deferred.

---

## Project-Driven Retirement Rules

| Project set | Planning rule |
|-------------|---------------|
| Mature operational slices such as `ProspectIdentification`, `LeaseAcquisition`, and `Decommissioning` | Validate and harden before considering any route retirement around them |
| Partial or thin slices such as `ProductionOperations`, `EnhancedRecovery`, `DrillingAndConstruction`, and backend-only engineering modules | Track as deferred or build-out gaps; do not misclassify them as regressions |
| `Beep.OilandGas.Web` and `Beep.OilandGas.ApiService` | Own duplicate route, controller-family, and dependency retirement decisions |
| `Beep.OilandGas.LifeCycle`, `Beep.OilandGas.UserManagement`, `Beep.OilandGas.Models` | Validate workflow, access, and contract continuity before trimming seams |
| Support libraries such as `Drawing`, `Branchs`, and `DataManager` | Keep only if still backing retained routes/components; otherwise move to explicit follow-up backlog |

---

## Pass Set

| Pass | Focus | Document |
|------|-------|----------|
| A | Validation, coverage, and regression proof | `PassA-ValidationAndCoverage.md` |
| B | Retirement of duplicates and dependency reduction | `PassB-RetirementAndDependencyReduction.md` |
| C | Operational readiness, documentation, and closure | `PassC-OperationalReadinessAndClosure.md` |

---

## Phase Deliverables

- solution-wide validation checklist
- duplicate page/component retirement list
- reduced direct dependencies and clearer boundaries
- final route, client, and workflow matrices
- documented exceptions and follow-up backlog

---

## Scan-Based Findings That Shape This Phase

| Finding | Planning Impact |
|---------|-----------------|
| Duplicate page areas are real: data pages, setup pages, production forecasting pages, and routed files under `Components/` | Retirement work needs an explicit duplicate-path list |
| API duplication is real around work orders | Endpoint retirement must include controller-family consolidation |
| Several domains are mature while others remain partial or thin | Validation must distinguish missing implementation from regression |
| Finance and compliance domains are service-heavy with thin UI | Hardening must include coverage validation, not just deletion |
| Some engineering modules still lack first-class API/web surfacing | Phase 10 needs explicit deferred-gap tracking |
