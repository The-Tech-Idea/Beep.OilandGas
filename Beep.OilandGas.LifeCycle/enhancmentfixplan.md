# LifeCycle Enhancement & Fix Plan (Folder-by-Folder)

Purpose: Bring `Beep.OilandGas.LifeCycle` to a consistent, industry-aligned lifecycle standard (exploration → abandonment), while fully leveraging all OilAndGas projects in the solution and PPDM39 conventions.

## Baseline Standards (apply across all folders)

- **PPDM compliance**: use existing tables first; only add new tables when PPDM does not cover the data.
- **Lifecycle state governance**: enforce state transitions (stage-gate) for Field/Reservoir/Well/Facility with audit history.
- **HSE + regulatory**: track permits, inspections, integrity, and incidents with explicit status and expiry.
- **Data quality**: common ID formatting, required fields, validation rules, row quality, and row effectivity dates.
- **Observability**: structured logging, error categorization, correlation IDs, and metrics for workflows.
- **Integration discipline**: all analysis/calculation projects go through `PPDMCalculationService` and mapping helpers.

## Folder-by-Folder Review and Enhancements

### `Models/Processes`
Current: Core workflow entities for process definitions, steps, instances, transitions.
Gaps:
- No explicit stage-gate definitions for each lifecycle phase and entity type.
- Missing schema versioning for process definitions.
Enhancements:
- Add versioned process definitions (Phase + Entity + Version).
- Add process outcome metadata (approval, risk rating, gating criteria).

### `plans/`
Current: Integration guides and process documentation.
Gaps:
- Several integrations are marked as "planned" but not tracked against implementation progress.
Enhancements:
- Convert each integration plan into a checklist with target classes, PPDM tables, and mapping files.
- Add a consolidated "Implementation Matrix" table and status per project.

### `Services/AccessControl`
Current: Access control scaffolding.
Gaps:
- No explicit roles for exploration, drilling, production, decommissioning.
Enhancements:
- Add role-based policies aligned to lifecycle phases and asset scope (field/well/facility).

### `Services/Accounting`
Current: Accounting orchestration (partially integrated).
Gaps:
- Partial integration with `Beep.OilandGas.Accounting` and `ProductionAccounting`.
Enhancements:
- Complete integration for cost allocation, AFE, and capitalization; map to PPDM finance tables.

### `Services/Calculations`
Current: `PPDMCalculationService` centralizes calculations.
Gaps:
- Some analysis modules are not fully wired (Economic, Nodal, WellTest, Flash, GasLift, etc).
Enhancements:
- Finalize all analysis adapters and PPDM result storage.
- Standardize result identifiers and shared request/response DTOs.

### `Services/DataMapping`
Current: Mapping between PPDM and domain models.
Gaps:
- Inconsistent namespaces and partial coverage for new analysis models.
Enhancements:
- Enforce one mapper per analysis domain with consistent naming.
- Add validation for required PPDM columns before insert.

### `Services/Decommissioning`
Current: Decommissioning workflow.
Gaps:
- Weak integration to `Beep.OilandGas.Decommissioning`.
Enhancements:
- Add well plug/abandonment, facility removal, environmental restoration workflows.
- Tie decommissioning to final permit status and abandonment cost tracking.

### `Services/Development`
Current: Development phase operations.
Gaps:
- Partial integration with `DevelopmentPlanning`, `DrillingAndConstruction`, `PipelineAnalysis`, `CompressorAnalysis`.
Enhancements:
- Connect drilling programs, facilities, pipelines, and equipment commissioning.
- Map to PPDM tables for facilities, equipment, and construction milestones.

### `Services/Exploration`
Current: Exploration CRUD and prospect integration (partially).
Gaps:
- Prospect identification, seismic analysis, and lease acquisition are not fully integrated.
Enhancements:
- Integrate `ProspectIdentification`, `LeaseAcquisition`, and `SeismicAnalysis` workflows.
- Add exploration program, play, lead, and risk/volume estimation steps.

### `Services/FacilityManagement`
Current: Facility lifecycle management.
Gaps:
- Facility equipment linkage previously incorrect; needs robust install history.
Enhancements:
- Enforce equipment link history via `FACILITY_EQUIPMENT` (install obs no, effective dates).
- Add integrity inspection and maintenance schedules tied to equipment status.

### `Services/FieldLifecycle`
Current: Field lifecycle state handling.
Gaps:
- No standardized stage-gate progression criteria.
Enhancements:
- Add explicit phase transition rules with required deliverables (e.g., permits, reserves).

### `Services/FieldManagement`
Current: Field management actions and analyses.
Gaps:
- Analysis return types and namespaces inconsistent with calculation models.
Enhancements:
- Use `Models.Data.Calculations` consistently for DCA/Nodal/Economic results.

### `Services/Inspection`
Current: Inspection operations (light).
Gaps:
- Needs integrated inspection templates and compliance rules.
Enhancements:
- Add inspection plan templates and link to `Permits` + `FacilityManagement`.

### `Services/Integration`
Current: Data flow orchestrations.
Gaps:
- Some integration routines use inconsistent analysis types/namespaces.
Enhancements:
- Enforce single integration gateway per analysis domain.

### `Services/Maintenance`
Current: Maintenance operations.
Gaps:
- Not tied to equipment lifecycle status or work order triggers.
Enhancements:
- Add preventive/condition-based maintenance with equipment performance triggers.

### `Services/Operations`
Current: Operations workflow layer.
Gaps:
- Not clearly tied to `ProductionOperations` project.
Enhancements:
- Integrate production surveillance, downtime, and optimization routines.

### `Services/Permits`
Current: Permit management service (partial).
Gaps:
- Integration with `PermitsAndApplications` incomplete.
Enhancements:
- Align permit workflows with lifecycle gating and compliance reporting.

### `Services/PipelineManagement`
Current: Pipeline management.
Gaps:
- Partial integration with `PipelineAnalysis`.
Enhancements:
- Add integrity, capacity, and flow assurance workflows.

### `Services/Processes`
Current: Process orchestration framework.
Gaps:
- Phase-specific process definitions missing or incomplete.
Enhancements:
- Define workflows per phase (Exploration, Development, Production, Decommissioning).
- Add approval steps and SLA tracking.

### `Services/Production`
Current: Production phase operations.
Gaps:
- Partial integration of `ProductionOperations`, `ProductionForecasting`, `ProductionAccounting`.
Enhancements:
- Integrate production allocation, forecasting updates, and performance KPIs.

### `Services/ReservoirLifecycle`
Current: Reservoir lifecycle state handling.
Gaps:
- Limited integration with reservoir engineering workflows.
Enhancements:
- Tie reservoir pressure, reserves, and recovery method updates to lifecycle transitions.

### `Services/WellLifecycle`
Current: Well lifecycle state handling.
Gaps:
- No explicit link between completion/workover events and lifecycle state transitions.
Enhancements:
- Add completion/workover triggers and well integrity state updates.

### `Services/WellManagement`
Current: Well management actions and analysis.
Gaps:
- Analysis result namespaces inconsistent with domain models.
Enhancements:
- Use `Models.Data.Calculations` results consistently for all analysis APIs.

### `Services/WorkOrder`
Current: Work order management.
Gaps:
- Not integrated with maintenance/inspection triggers.
Enhancements:
- Auto-create work orders based on inspection findings and maintenance rules.

## Full Project Integration Matrix (High Level)

- **Exploration**: ProspectIdentification, LeaseAcquisition, DrillingAndConstruction (planning), EconomicAnalysis.
- **Development**: DevelopmentPlanning, DrillingAndConstruction, PipelineAnalysis, CompressorAnalysis, PumpPerformance.
- **Production**: ProductionOperations, ProductionForecasting, ProductionAccounting, GasLift, ChokeAnalysis, NodalAnalysis, WellTestAnalysis, SuckerRodPumping, PlungerLift, HydraulicPumps.
- **Decommissioning**: Decommissioning, Accounting (abandonment costs).
- **Cross-cutting**: PermitsAndApplications, UserManagement, Drawing/HeatMap (visualization).

## Phased Execution Plan

### Phase 1: Stabilize Core Calculations and Data Mapping
- Finish all analysis adapters in `PPDMCalculationService`.
- Fix namespace mismatches in `WellManagement` and `FieldManagement`.
- Validate PPDM mapping for all analysis results.

### Phase 2: Phase Services Integration
- Exploration: integrate prospect, risk, lease workflows.
- Development: connect drilling, facilities, pipeline, compressor.
- Production: integrate production operations, forecasting, and lift optimization.

### Phase 3: Asset Integrity and Compliance
- Tie permits to lifecycle gating.
- Implement inspection and maintenance workflows.
- Add compliance reporting and audit trails.

### Phase 4: Decommissioning and Closure
- Implement abandonment workflows with cost tracking and final permits.
- Ensure final lifecycle state transitions and reporting.

## Deliverables (per folder)

- Updated service interfaces and DTOs.
- PPDM table mapping coverage and missing-table list.
- Integration test checklist per phase.
- Error handling and logging standardization.

