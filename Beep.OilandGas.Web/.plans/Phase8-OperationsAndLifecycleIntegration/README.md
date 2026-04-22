# Phase 8 Folder Plan — Operations and Lifecycle Integration

> Detailed execution plan for Phase 8  
> Companion overview: `../Phase8-OperationsAndLifecycleIntegration.md`
> Project knowledge base: `../Projects/INDEX.md`
> Evidence manifest: `ScanEvidence.md`

---

## Purpose

Phase 8 is no longer only a web-page integration phase. It now runs as a whole-solution phase across the Beep.OilandGas projects so operational and lifecycle flows are aligned from web routes down to API orchestration, domain services, and PPDM-backed persistence.

---

## Whole-Solution Pass Model

Every Beep.OilandGas solution project is reviewed in each pass as one of these roles:

- `Primary`: the pass makes direct structural or API/domain changes in that project.
- `Supporting`: the project provides contracts, UI, calculations, persistence, or security needed by the primary work.
- `Validation`: the project is checked for compatibility, route impact, or workflow handoff stability.

Use `ProjectCoverageMatrix.md` as the authoritative project-by-project checklist.

Use the per-project docs in `../Projects/Operations/`, `../Projects/Presentation/`, `../Projects/Core/`, and supporting categories as the canonical maturity baseline for deciding whether a slice is anchor, partial, thin, or validation-only.

---

## Project-Driven Priorities

| Project set | Planning rule |
|-------------|---------------|
| `Beep.OilandGas.LifeCycle`, `Beep.OilandGas.Web`, `Beep.OilandGas.ApiService` | Treat as the primary orchestration and boundary owners for this phase |
| `ProspectIdentification`, `LeaseAcquisition`, `Decommissioning` | Treat as anchor slices that can drive the first vertical completion pass |
| `DevelopmentPlanning` | Treat as present but partial; include orchestration growth, not only seam cleanup |
| `ProductionOperations`, `EnhancedRecovery`, `DrillingAndConstruction` | Treat as thin or under-surfaced domains that require build-out work |
| `Accounting`, `ProductionAccounting`, `PermitsAndApplications` | Keep in supporting roles for handoffs that cross into finance, compliance, or permit state |
| Engineering modules without strong vertical surfacing | Use as supporting calculators, not as primary workflow owners in this phase |

---

## Pass Set

| Pass | Focus | Document |
|------|-------|----------|
| A | Domain boundary mapping and ownership freeze across the solution | `PassA-DomainBoundaryAndProjectOwnership.md` |
| B | Vertical slice completion for exploration, development, production, lease, EOR, and decommissioning | `PassB-VerticalSliceCompletion.md` |
| C | Cross-module workflow linking and state handoffs | `PassC-CrossModuleWorkflowLinking.md` |

---

## Project Groups In Scope

### Presentation and platform

- `Beep.OilandGas.Web`
- `Beep.OilandGas.ApiService`
- `Beep.OilandGas.Client`
- `Beep.OilandGas.Models`
- `Beep.OilandGas.PPDM.Models`
- `Beep.OilandGas.PPDM39`
- `Beep.OilandGas.PPDM39.DataManagement`
- `Beep.OilandGas.DataManager`
- `Beep.OilandGas.UserManagement`
- `Beep.OilandGas.Branchs`
- `Beep.OilandGas.Drawing`

### Operational domain projects

- `Beep.OilandGas.ProspectIdentification`
- `Beep.OilandGas.DevelopmentPlanning`
- `Beep.OilandGas.ProductionOperations`
- `Beep.OilandGas.LifeCycle`
- `Beep.OilandGas.LeaseAcquisition`
- `Beep.OilandGas.EnhancedRecovery`
- `Beep.OilandGas.Decommissioning`
- `Beep.OilandGas.DrillingAndConstruction`

### Supporting finance, compliance, and workflow domains

- `Beep.OilandGas.ProductionAccounting`
- `Beep.OilandGas.Accounting`
- `Beep.OilandGas.PermitsAndApplications`
- `Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf`

### Engineering and calculation support

- `Beep.OilandGas.EconomicAnalysis`
- `Beep.OilandGas.ProductionForecasting`
- `Beep.OilandGas.DCA`
- `Beep.OilandGas.NodalAnalysis`
- `Beep.OilandGas.WellTestAnalysis`
- `Beep.OilandGas.PumpPerformance`
- `Beep.OilandGas.ChokeAnalysis`
- `Beep.OilandGas.GasLift`
- `Beep.OilandGas.SuckerRodPumping`
- `Beep.OilandGas.PlungerLift`
- `Beep.OilandGas.HydraulicPumps`
- `Beep.OilandGas.CompressorAnalysis`
- `Beep.OilandGas.PipelineAnalysis`
- `Beep.OilandGas.FlashCalculations`
- `Beep.OilandGas.OilProperties`
- `Beep.OilandGas.GasProperties`
- `Beep.OilandGas.HeatMap`

---

## Phase Deliverables

- domain-specific typed clients with clear ownership
- field-scoped API boundary map by operational domain
- complete vertical slices for operational workflows
- explicit handoffs between calculations, operations, work orders, AFEs, permits, and lifecycle state
- project-by-project change/validation checklist for all Beep.OilandGas solution projects

---

## Scan-Based Findings That Shape This Phase

| Finding | Planning Impact |
|---------|-----------------|
| `ProspectIdentification`, `LeaseAcquisition`, and `Decommissioning` have real service + API + page presence | These are anchor slices for the first operational pass |
| `DevelopmentPlanning` is present but partial | The phase must include orchestration and sequencing build-out, not only connection cleanup |
| `ProductionOperations` is API-visible but service-thin | Production orchestration must be treated as incomplete and expanded in Pass B |
| `EnhancedRecovery` is API-visible but minimal | EOR is a slice build-out item, not just an integration item |
| `DrillingAndConstruction` has service/calculation pieces and one page but thin workflow surfacing | Initial development-to-drilling handoff is now live; deeper drilling/construction ownership hardening remains thin |
| `LifeCycle` is the main orchestration and mapping hub | Cross-module handoffs should consolidate around LifeCycle rather than create parallel orchestration |
| `PermitsAndApplications` lacks first-class UI/API surfacing | Permit steps are a cross-phase gap touching phases 8 and 9 |
| Work-order ownership is duplicated across controller families | Work-order ownership must be frozen before later handoff work |
