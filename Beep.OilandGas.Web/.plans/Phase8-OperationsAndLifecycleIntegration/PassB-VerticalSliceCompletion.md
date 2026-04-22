# Phase 8 Pass B — Vertical Slice Completion

## Objective

Complete the operational vertical slices so the main business flows work end to end across the web, API, and domain projects.

---

## Vertical Slices

| Slice | Primary Projects | Supporting Projects | Expected Result |
|-------|------------------|---------------------|-----------------|
| Exploration | Web, ApiService, ProspectIdentification, LifeCycle | Models, PPDM39.DataManagement, HeatMap, EconomicAnalysis | prospect pages align to prospect-identification services and can hand off to development and economics |
| Development planning | Web, ApiService, DevelopmentPlanning, DrillingAndConstruction | Models, PPDM39.DataManagement, EconomicAnalysis, PermitsAndApplications | field development plans, well programs, and construction flows use one domain path |
| Production operations | Web, ApiService, ProductionOperations, LifeCycle | DCA, ProductionForecasting, NodalAnalysis, WellTestAnalysis, GasLift, Pump modules, PipelineAnalysis | well-performance, deferment, and intervention flows use production-operations ownership |
| Lease and land | Web, ApiService, LeaseAcquisition | Models, PermitsAndApplications, Accounting | lease due diligence and readiness flows are connected to development planning and compliance |
| Enhanced recovery | Web, ApiService, EnhancedRecovery | EconomicAnalysis, ProductionOperations, Reservoir-support calculations | EOR screening and pilot flows align to one EOR ownership path |
| Decommissioning | Web, ApiService, Decommissioning, LifeCycle | Accounting, ProductionAccounting, PermitsAndApplications | late-life and abandonment flows use one decommissioning path |

---

## Required Outputs

1. One owning client and controller family for each operational slice.
2. One owning domain-service path for each operational slice.
3. Consistent active-field propagation across operational pages.
4. Supporting calculations invoked through typed clients and API seams only.

---

## Exit Gate

Each operational area must work as a single vertical slice rather than a mixture of generic data calls, direct library calls, and overlapping services.
