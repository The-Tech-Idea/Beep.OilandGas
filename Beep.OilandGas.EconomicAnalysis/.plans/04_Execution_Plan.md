# Phase 3 Execution Plan

## Objective
Provide execution-ready implementation backlog with concrete file targets, TODO checklists, and verification per phase.

## Phase 3.1 Contract and Service Boundary Cleanup
### Target Files
- `Beep.OilandGas.EconomicAnalysis/Core/Interfaces/IEconomicAnalysisService.cs`
- `Beep.OilandGas.EconomicAnalysis/Services/EconomicAnalysisService.cs`
- `Beep.OilandGas.EconomicAnalysis/Services/EconomicAnalysisService.Advanced.cs`

### Tasks
- [x] Confirm canonical methods required for API-facing operations remain in `IEconomicAnalysisService`.
- [ ] If needed, split advanced methods to `IEconomicAnalysisAdvancedService`.
- [x] Keep `EconomicAnalysisService` as partial class with clear core vs advanced boundary.
- [x] Add `OperationCanceledException` precedence where cancellation is introduced in async paths.

### Acceptance Criteria
- Canonical interface contains only active API contract members.
- Advanced methods are clearly segregated and documented.

## Phase 3.2 PPDM Persistence Alignment
### Target Files
- `Beep.OilandGas.EconomicAnalysis/Services/EconomicAnalysisService.cs`
- `Beep.OilandGas.EconomicAnalysis/Modules/EconomicsModule.cs`
- `Beep.OilandGas.EconomicAnalysis/Data/Tables/*` (if new tables are introduced)

### Tasks
- [x] Preserve canonical writes to `ECONOMIC_ANALYSIS_RESULT`.
- [x] Decide and implement persistence strategy for cash flow/profile details (persist or explicitly defer). (defer documented; no new write paths added)
- [x] If adding module-owned tables, create table classes and register in `EntityTypes`. (registered PPDM economic reference entities in module setup)
- [x] Ensure table classes follow scalar-only rule and naming conventions.

### Acceptance Criteria
- Persistence boundary is explicit and code-aligned.
- No projection class is written directly via repository.

## Phase 3.3 Seeding and Reference Catalog
### Target Files
- `Beep.OilandGas.EconomicAnalysis/Constants/EconomicAnalysisReferenceCodes.cs` (new if needed)
- `Beep.OilandGas.EconomicAnalysis/Constants/EconomicAnalysisReferenceCodeSeed.cs` (new if needed)
- `Beep.OilandGas.EconomicAnalysis/Modules/EconomicsModule.cs`

### Tasks
- [x] Define module reference families and default values.
- [x] Implement idempotent seed application in `SeedAsync`.
- [x] Ensure reruns do not duplicate rows and produce deterministic results.

### Acceptance Criteria
- Seed run is idempotent.
- Required reference families are covered and auditable.

## Phase 3.4 API and DI Alignment
### Target Files
- `Beep.OilandGas.ApiService/Controllers/Calculations/EconomicAnalysisController.cs`
- `Beep.OilandGas.ApiService/Program.cs`

### Tasks
- [x] Validate controller routes map to canonical service methods only.
- [ ] Keep advanced routes out of canonical exposure until promotion criteria are met.
- [x] Confirm DI uses factory registration pattern and required dependencies.

### Acceptance Criteria
- API route ownership matrix is fully satisfied.
- DI remains stable and build-safe.

## Phase 3.5 Testing and Hardening
### Target Files
- `Beep.OilandGas.ApiService.Tests/EconomicAnalysisControllerTests.cs` (new)
- `Beep.OilandGas.ApiService.Tests/EconomicAnalysisReferenceSeedCatalogTests.cs` (new if seed catalog introduced)

### Tasks
- [x] Add controller tests for core routes and error handling paths.
- [x] Add seed catalog integrity/idempotency tests if module seed catalog is introduced.
- [x] Add interface boundary tests if canonical/advanced split is introduced.

### Acceptance Criteria
- Focused tests pass for controller and seed behavior.
- Canonical vs advanced boundary is protected by tests.

## Execution Ordering
1. Contracts and service split
2. Persistence boundary alignment
3. Seed catalog and module setup
4. API/DI alignment
5. Tests and verification

## Exit Criteria for Phase 3
- All file-targeted tasks completed.
- Acceptance criteria met per sub-phase.
- Ready for full verification and migration documentation.
