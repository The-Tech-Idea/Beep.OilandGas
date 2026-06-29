# Beep.OilandGas — Architecture Documentation

**Generated:** 2026-06-27
**Scope:** All 50+ projects in the Beep.OilandGas solution
**Methodology:** Every .cs file read in full — no skimming, no guessing

---

## 1. Executive Summary

Beep.OilandGas is a comprehensive **Oil & Gas Engineering and Data Management Platform** built on the **PPDM 3.9** industry-standard data model. The solution provides end-to-end lifecycle management for petroleum assets — from exploration through development, production, reservoir management, economics, HSE compliance, and decommissioning.

### Solution Scale

| Dimension | Count |
|-----------|-------|
| Total Projects | ~52 |
| Source Files (.cs) | 3,500+ |
| API Controllers | 70+ |
| API Endpoints | 350+ |
| Service Interfaces | 200+ |
| Domain Modules | 44+ |
| PPDM Entity Types | 1,400+ |
| Reference Code Tables (R_*) | 30+ |

---

## 2. Architectural Layers

```
┌──────────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER                                          │
│  ┌──────────────────────┐  ┌─────────────────────────────┐   │
│  │ Beep.OilandGas.Web   │  │ Beep.OilandGas.Client        │   │
│  │ Blazor Server (MudBlazor)││ Dual-mode HTTP/DI Facade     │   │
│  │ 60+ Razor pages      │  │ NuGet-packaged SDK          │   │
│  └──────────┬───────────┘  └──────────────┬──────────────┘   │
└─────────────┼──────────────────────────────┼──────────────────┘
              │ HTTP/REST + SignalR          │
┌─────────────▼──────────────────────────────────────────────────┐
│  API LAYER                                                     │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Beep.OilandGas.ApiService (ASP.NET Core)                 │  │
│  │ • 70+ Controllers, 350+ endpoints                        │  │
│  │ • JWT Bearer Auth (IdentityServer)                       │  │
│  │ • FieldOrchestrator for field-scoped operations          │  │
│  │ • Asset Access Middleware (RBAC with inheritance)        │  │
│  │ • Progress tracking via SignalR hub                      │  │
│  └──────────────────────────┬───────────────────────────────┘  │
└──────────────────────────────┼──────────────────────────────────┘
                               │ DI / Service calls
┌──────────────────────────────▼──────────────────────────────────┐
│  BUSINESS LOGIC LAYER                                           │
│  ┌─────────────────────────┐ ┌──────────────────────────────┐  │
│  │ Domain Lifecycle        │ │ Engineering Calculations      │  │
│  │ Beep.OilandGas.*        │ │ Beep.OilandGas.*              │  │
│  │ ┌─────────────────┐    │ │ ┌──────────────────────────┐  │  │
│  │ │ LifeCycle       │    │ │ │ WellTestAnalysis          │  │  │
│  │ │ ProspectIdent.  │    │ │ │ CompressorAnalysis        │  │  │
│  │ │ DevelopmentPlan. │    │ │ │ ChokeAnalysis             │  │  │
│  │ │ DrillingAndCon. │    │ │ │ NodalAnalysis             │  │  │
│  │ │ ProductionOper. │    │ │ │ GasLift                   │  │  │
│  │ │ ProductionAcct. │    │ │ │ SuckerRodPumping          │  │  │
│  │ │ EconomicAnalysis│    │ │ │ PlungerLift               │  │  │
│  │ │ HSE             │    │ │ │ PumpPerformance           │  │  │
│  │ │ Decommissioning │    │ │ │ HydraulicPumps            │  │  │
│  │ │ PermitsAndApps  │    │ │ │ PipelineAnalysis          │  │  │
│  │ │ LeaseAcquisition│    │ │ │ FlashCalculations         │  │  │
│  │ │ Branchs         │    │ │ │ OilProperties             │  │  │
│  │ └─────────────────┘    │ │ │ GasProperties             │  │  │
│  └─────────────────────────┘ │ │ Properties                │  │  │
│                               │ │ EnhancedRecovery          │  │  │
│  ┌─────────────────────────┐ │ │ ProductionForecasting     │  │  │
│  │ Visualization           │ │ │ Drawing                   │  │  │
│  │ • Drawing (schematic)  │ │ │ HeatMap                   │  │  │
│  │ • HeatMap (spatial)    │ │ └──────────────────────────┘  │  │
│  └─────────────────────────┘ └──────────────────────────────┘  │
└──────────────────────────────┬──────────────────────────────────┘
                               │ PPDMGenericRepository
┌──────────────────────────────▼──────────────────────────────────┐
│  DATA ACCESS LAYER                                              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Beep.OilandGas.PPDM39.DataManagement                     │  │
│  │ • PPDMGenericRepository (metadata-driven CRUD)           │  │
│  │ • PPDMMetadataService (1,400+ table schema registry)     │  │
│  │ • WellServices (well CRUD with WSC v3 facet management)  │  │
│  │ • ModuleSetupBase (auto-discovered schema seeding)       │  │
│  │ • QueryBuilder (cross-DB parameterized SQL)              │  │
│  │ • Seed data infrastructure (JSON, CSV, Enum, Dummy)      │  │
│  │ • 50+ domain service implementations                     │  │
│  └──────────────────────────┬───────────────────────────────┘  │
│  ┌──────────────────────────▼───────────────────────────────┐  │
│  │ Beep.OilandGas.PPDM39 (Central Hub)                      │  │
│  │ • PPDM 3.9 Schema Registry (51 categories)               │  │
│  │ • IPPDMEntity / ModelEntityBase (audit columns)          │  │
│  │ • PPDMTableMetadata / PPDMForeignKey                     │  │
│  │ • Common Column Handler (ROW_CREATED_BY, etc.)           │  │
│  │ • IPPDM39DefaultsRepository (default value system)       │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Beep.OilandGas.Models (Shared Type System)               │  │
│  │ • 1,156+ .cs files, 34 domain subdirectories             │  │
│  │ • 65+ service interfaces                                 │  │
│  │ • 44 enum types                                          │  │
│  │ • WellKnown/Constant classes (domain tokens)             │  │
│  │ • ModelEntityBase (PPDM audit columns + INotifyProperty) │  │
│  └──────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
                               │ IDMEEditor / IDataSource
┌──────────────────────────────▼──────────────────────────────────┐
│  INFRASTRUCTURE LAYER                                            │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ TheTechIdea.Beep (External NuGet Platform)               │  │
│  │ • IDMEEditor — Database-agnostic data access             │  │
│  │ • IDataSource — Multi-provider SQL abstraction            │  │
│  │ • AppFilter — Cross-DB WHERE clause builder              │  │
│  │ • Supports: SQL Server, PostgreSQL, SQLite,              │  │
│  │           Oracle, MySQL, MariaDB                         │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Identity / Support                                       │  │
│  │ • Beep.OilandGas.UserManagement (AuthN/AuthZ/RBAC)      │  │
│  │ • Beep.OilandGas.DataManager (DB script execution)       │  │
│  │ • Beep.OilandGas.Client (SDK facade)                     │  │
│  └──────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 3. Core Infrastructure — Deep Architecture

### 3.1 Beep.OilandGas.PPDM39 (Central Hub)

**Role:** Schema registry + repository + metadata + defaults

**Key Components:**
- **PPDM39SchemaRegistry** — Hardcoded registry of ~1,400 PPDM tables across 51 subject-area categories with parent-child FK hierarchy
- **PPDMGenericRepository** — Non-generic, dynamically-typed CRUD repository working with any PPDM entity type at runtime. Covers: Get, GetById, Insert, Update, SoftDelete, batch operations, aggregation, pagination, CSV import/export
- **PPDMModuleRepository** — Metadata-driven, module-scoped repository for domain entity groups with automatic FK handling
- **PPDMTableMetadata** — Table schemas, PKs, FKs, modules, subject areas
- **ICommonColumnHandler** — Automatic PPDM audit columns: ROW_CREATED_BY, ROW_CHANGED_BY, ROW_CREATED_DATE, ROW_CHANGED_DATE, ACTIVE_IND, ROW_EFFECTIVE_DATE, ROW_EXPIRY_DATE, PPDM_GUID, ROW_QUALITY
- **IPPDM39DefaultsRepository** — Default value system with user-specific overrides
- **ValueRetrievers** — Static utility class with configurable Func delegates for computed values

**Dependency:** Pure library, no UI or DB provider dependencies

### 3.2 Beep.OilandGas.Models (Shared Type System)

**Role:** All shared interfaces, DTOs, enums, and PPDM entity base classes

**Scale:** 1,156+ .cs files, organized in 34 domain subdirectories under `Data/`

**Critical abstractions:**
- `ModelEntityBase` — Root class for all PPDM-persistable entities. Inherits `Entity` (INotifyPropertyChanged), implements `IPPDMEntity`. Provides 14 standard PPDM audit columns via backing fields + `SetProperty` pattern
- **65+ service interfaces** in `Core/Interfaces/` — One per domain feature
- **WellKnown classes:** `PermissionConstants` (30+ domains), `WellTestAnalysisWellKnown`, `CompressorAnalysisWellKnown`, `JIBStatus`, `ObligationStatus`, `IncidentType`, `WorkOrderSubType`, `CalculationRunStatus`
- **44 enum types** — Wells, units, risk, HSE, process states, etc.
- **Domain DTOs:** Every domain has contracts, projections, and table entities

**Table vs Projection Class Rule (Critical):**
- Table classes: Extend `ModelEntityBase`, scalar properties only (no collections), SCREAMING_SNAKE_CASE names
- Projection classes: May have `List<T>`, `Dictionary<K,V>`, nested objects. PascalCase with suffix (Request/Response/Result/Summary/Report)

### 3.3 Beep.OilandGas.PPDM39.DataManagement (Infrastructure)

**Role:** Central data management — metadata, schema migration, seeding, domain service implementations

**Key components:**
- **PPDM39Metadata** — 5.8MB auto-generated file with complete PPDM 3.9 table metadata
- **PPDMMetadataService** — Cached metadata from JSON/embedded resource/fallback
- **QueryBuilder** — Cross-DB parameterized SQL builder using IDataSource delimiters
- **ModuleSetupBase** — Abstract base for schema modules (ModuleId, EntityTypes, SeedAsync)
- **ModuleSetupOrchestrator** — Discovers and runs all module seeds in order
- **ModuleSetupDiscovery** — Auto-discovers IModuleSetup implementations via assembly scanning
- **WellServices** — 6-file partial class (~1,400 lines), 13 WSC v3 well status facets
- **50+ domain service implementations** — HSE, Compliance, Work Order, Integrations, Analytics
- **5 seed data mechanisms:** JSON template, Enum-to-reference-table, CSV file, Static catalog, Dummy data generation

**Module Execution Order:**
```
Order 0:   CorePpdmModule
Order 10:  SharedReferenceModule
Order 20:  WellStatusFacetModule
Order 30:  WellReferenceModule (delegated)
Order 40:  SecurityModule (UserManagement)
Order 45:  LeaseAcquisitionModule
Order 50:  ExplorationModule
Order 60:  DrillingAndConstructionSetupModule
Order 61:  DevelopmentPlanningModule
Order 70:  ProductionAccountingModuleSetup
Order 80:  HseModule / FacilityManagementModuleSetup
Order 90:  EconomicsModule
Order 100: DecommissioningModule / DemoDataModule
```

### 3.4 Beep.OilandGas.ApiService (API Layer)

**Role:** ASP.NET Core REST API with JWT auth, field-scoped operations

**Scale:** ~135 .cs files, 70+ controllers, 350+ endpoints, 100+ DI service registrations

**Authentication Pipeline:**
1. JWT Bearer tokens from IdentityServer (`https://localhost:7062/`)
2. Claims: NameIdentifier, Name, Email, tenant_id, ba_id, roles, permissions
3. Custom `[RequireCurrentFieldAccess]` attribute for field-scoped endpoints
4. `[RequireRole]` for role-based access
5. `AssetAccessMiddleware` — Injects accessible assets into HttpContext.Items
6. `AuthorizationObservabilityService` — Audits every auth decision

**FieldOrchestrator Integration:**
- `IFieldOrchestrator` registered as Scoped
- All `api/field/current/*` endpoints are field-scoped
- Phase services (exploration, development, production, HSE, decommissioning) are lazy-initialized per field
- Field switching resets all cached phase services

**Key Controllers:**
- **PPDM39 controllers** (10) — Data, Schema, Setup, Validation, Quality, Metadata, Workflow, Audit, Versioning, Import/Export
- **Accounting controllers** (25) — Complete revenue cycle: production → allocation → royalty → revenue → GL posting
- **Facility controllers** (7) — Equipment, Maintenance, Production, License, Work Order, Monitoring
- **Field-scoped controllers** (5) — FieldOrchestrator, Drilling, Reservoir, Production, Accounting, ProcessAnalytics, Compliance, Permits, WorkOrder
- **Engineering controllers** — PipelineAnalysis, GasProperties, PlungerLift, SuckerRodPumping, HydraulicPump, EnhancedRecovery, ProspectIdentification, HeatMap, Stratigraphy

### 3.5 Beep.OilandGas.Web (Blazor Server UI)

**Technology:** MudBlazor 7.x, .NET 10, OIDC + Cookie auth

**Pages:** 60+ Razor pages covering the full lifecycle:
- Exploration Dashboard, Prospect Detail, Seismic Tracker
- Development Dashboard, FDP Wizard, Well Design
- Reservoir Characterization
- Economic Analysis, Pipeline Analysis
- Plunger Lift, Sucker Rod Pumping
- Accounting Dashboard, Production Accounting
- Facility Decommissioning, Well P&A
- Compliance Obligation Detail
- Data Quality Dashboard, Validation, Versioning
- Admin: Access Control, User Roles, Hierarchy Config

**Component Library:**
- `PPDMDataGrid` — Wraps MudDataGrid<T> with column auto-discovery, toolbar, export, pagination
- `PPDMEntityForm` — Dynamic entity edit form via reflection
- `KpiCard`, `StatusBadge`, `ProcessTimeline`, `StageAdvanceDialog`
- `ProgressDisplay`, `CalculationFactGrid`, `LiftRecommendationCard`
- `WellVisualization`, `ConnectionCheck`, `FieldSelector`

**API Communication:**
- `ApiClient` — Generic typed HTTP client with System.Text.Json
- 25+ typed service clients (one per domain)
- `DataManagementService` — Central data management with retry, caching, events
- `ProgressTrackingClient` — SignalR real-time progress
- `NavigationPolicyService` — Persona-based route authorization

### 3.6 Beep.OilandGas.LifeCycle (Orchestration)

**Role:** Field orchestrator + lifecycle state machines + process workflows + 12 domain mapping services

**Scale:** ~95 source files

**State Machines:**
- **Field Lifecycle:** EXPLORATION → DEVELOPMENT → PRODUCTION → DECLINE → DECOMMISSIONING → DECOMMISSIONED
- **Well Lifecycle:** PLANNED → DRILLING → COMPLETED → PRODUCING → WORKOVER → SUSPENDED → ABANDONED
- **Reservoir Lifecycle:** DISCOVERED → APPRAISED → DEVELOPED → PRODUCING → DEPLETED → ABANDONED
- **Process Workflow Engine:** 82 process definitions across exploration, development, production, decommissioning, HSE, compliance, work orders, gate reviews

**Key Services:**
- `FieldOrchestrator` — Single active field context, lazy phase service initialization
- `PPDMDevelopmentService` — Field-scoped pool/well/facility/pipeline CRUD
- `PPDMProductionService` — Production data with choke analysis and SRP integration
- `PPDMDecommissioningService` — Well abandonment, facility decommissioning, cost estimation
- `PPDMCalculationService` — 16-file partial class wrapping all engineering calculations
- `DataFlowService` — Simplified API over PPDMCalculationService
- 12 domain mappers (DCA, Flash, GasLift, HydraulicPumps, NodalAnalysis, PipelineAnalysis, etc.)
- `ApprovalWorkflowEngine` — Sequential, parallel, any-of-N approval with delegation/escalation

### 3.7 Beep.OilandGas.UserManagement

**Role:** Authentication, authorization, RBAC, MFA, session management

**Key Features:**
- JWT generation (HMAC-SHA256) with refresh tokens
- Password hashing: PBKDF2 with SHA-256, 120k iterations
- Account lockout: 5 attempts, 30-minute lockout
- TOTP MFA per RFC 6238 (30s period, 6 digits, 10 backup codes)
- 13 roles seeded: Viewer, Manager, PetroleumEngineer, ReservoirEngineer, Admin, etc.
- 16 personas seeded covering field engineer through workflow admin
- Row-level security with scope-based access (FIELD/ASSET/ORGANIZATION)
- `PermissionHandler` — ASP.NET Core IAuthorizationHandler checking permission claims

### 3.8 Beep.OilandGas.Client (SDK)

**Role:** NuGet-packaged client library providing a dual-mode facade for all domain services

**Modes:**
- **Remote:** HTTP API calls with bearer token auth, retry logic
- **Local:** Direct IServiceProvider resolution from Beep engine
- **Auto:** Falls back from remote to local

**Domain Coverage:** 15 service facades covering Connection, Well, DataManagement, Pumps, Properties, Calculations, Analysis, Field, LifeCycle, Operations, Drilling, Production, Accounting, AccessControl, Permits, Lease

### 3.9 Beep.OilandGas.DataManager

**Role:** Database script execution engine — schema creation with checkpoint/resume

**Key Features:**
- 44 IModuleData implementations, each managing SQL scripts for one domain
- Topological sort with circular dependency detection
- Checkpoint/resume for long-running operations
- Cross-DB validation (table existence, syntax checks)
- File and database state store backends

---

## 4. Domain Lifecycle Projects — Architecture

### 4.1 ProspectIdentification (Exploration)

**Scale:** 94 source files
**Core:** Exploration lifecycle — leads, prospects, seismic surveys, risk assessment, economic viability analysis, portfolio optimization
**Key patterns:** PROSPECT table entity with 50+ SPE PRMS fields, PPDMGenericRepository with AppFilter, manual DTO mapping

### 4.2 DevelopmentPlanning

**Scale:** 22 source files
**Core:** FDP creation, well schedules, facility investment plans, maintenance/service job scheduling
**Key patterns:** PlanLinkageContext for cross-entity resolution, 12 advanced analysis methods

### 4.3 DrillingAndConstruction

**Scale:** 14 source files
**Core:** Drilling programs, daily reports, bit/fluid/casing/cement tracking
**Engineering calculations:** HydraulicsCalculator (Bingham Plastic + Power Law), TorqueDragCalculator (soft-string Johancsik)

### 4.4 ProductionOperations

**Scale:** 16 source files
**Core:** Well production monitoring, equipment reliability, facility operations, production recording, cost tracking, optimization
**Note:** Most advanced methods are stubs returning empty data

### 4.5 ProductionAccounting

**Scale:** 120+ source files
**Core:** Complete production revenue cycle — run tickets → allocation → royalty → revenue recognition (ASC 606) → JIB → GL posting → period closing → tax
**32 service implementations**, 44 service interfaces, 80+ entity types registered
**Massive 1,384-line compatibility layer** with 17 nested classes and ConcurrentDictionary caches

### 4.6 EconomicAnalysis

**Scale:** 43 source files
**Core:** NPV, IRR, MIRR, payback, PI, ROI + Monte Carlo simulation, real options, decision trees, after-tax analysis, DCF valuation
**Rendering:** SkiaSharp-based cash flow charts

### 4.7 HSE

**Scale:** 1 source file (skeleton module only)
**Note:** Domain logic lives in PPDM39.DataManagement/Services/HSE/ — this project is a placeholder

### 4.8 Decommissioning

**Scale:** 33 source files
**Core:** Well abandonment, facility decommissioning, environmental restoration, cost estimation
**Note:** Cost models use hardcoded rates ($150/ft plugging, $250k offshore wellhead removal)

### 4.9 PermitsAndApplications

**Scale:** 65+ source files
**Core:** Full permit lifecycle across 30+ regulatory bodies (USA, Canada, international)
**Features:** State machine validation (DRAFT→SUBMITTED→UNDER_REVIEW→APPROVED/REJECTED), compliance scoring, form template rendering, jurisdiction mapping
**12 jurisdiction-specific validation rules**

### 4.10 LeaseAcquisition

**Scale:** 35+ source files
**Core:** Mineral lease lifecycle — prospect evaluation, negotiation, documentation, rights management, stakeholder management, financial management, due diligence
**8 partial class files** for service implementation

### 4.11 Branchs

**Scale:** 12 source files
**Core:** Navigation tree — PPDM 3.9 (700+ tables in 51 categories) + Business Processes (90 workflows in 12 categories)
**Note:** Tree is entirely hardcoded, not database-introspected

---

## 5. Engineering Calculation Projects — Architecture

### 5.1 WellTestAnalysis
- **Domain:** Pressure transient analysis (PTA)
- **Algorithms:** Horner build-up, MDH, drawdown semi-log, Bourdet derivative, gas well pseudo-pressure, type-curve matching
- **Rendering:** SkiaSharp-based diagnostic plots
- **Maturity:** Well-structured, constants centralized in WellTestConstants + WellTestAnalysisWellKnown

### 5.2 CompressorAnalysis
- **Domain:** Centrifugal and reciprocating compressor analysis
- **Algorithms:** Polytropic/adiabatic head, volumetric efficiency, multi-stage power, feasibility analysis
- **Module:** Full ModuleSetupBase with R_COMPRESSOR_ANALYSIS_REFERENCE_CODE

### 5.3 ChokeAnalysis
- **Domain:** Gas choke flow with 6 industry correlations (Gilbert, Ros, Achong, Pilehvari, Sachdeva, Baxendell)
- **Algorithms:** Sonic/subsonic flow regime determination, choke sizing, pressure calculations
- **Issue:** ChokeAnalysisService.Advanced.cs has significant magic numbers

### 5.4 NodalAnalysis
- **Domain:** IPR/VLP nodal analysis
- **IPR correlations:** Vogel, Fetkovich, Wiggins, composite, gas-well
- **VLP correlations:** Hagedorn-Brown, Beggs-Brill, Duns-Ros, Orkiszewski, Aziz-Govier-Fogarasi
- **Services:** Full partial class service with sensitivity, optimization, artificial lift ranking, diagnostics
- **Module:** Full ModuleSetupBase with R_NODAL_ANALYSIS_REFERENCE_CODE

### 5.5 GasLift
- **Domain:** Gas lift design and optimization (US/SI units)
- **Algorithms:** Valve design, equal pressure drop/depth spacing, performance curves, system optimization
- **Module:** Full ModuleSetupBase with R_GAS_LIFT_REFERENCE_CODE, GasLiftReferenceSets, GasLiftDesignLimitMessages

### 5.6 PipelineAnalysis
- **Domain:** Gas and liquid pipeline hydraulics
- **Algorithms:** Weymouth, Panhandle B, Darcy-Weisbach, Beggs-Brill two-phase, Dukler, API RP 14E erosion, pigging, looping vs. compression economics
- **Scale:** 24 files, 8 static calculator classes, 10 partial service files

### 5.7 PumpPerformance
- **Domain:** Pump screening — H-Q curves, efficiency, NPSH, affinity laws, ESP design
- **Algorithms:** HI 9.6.7 viscosity correction, multi-pump series/parallel configuration
- **Pump types:** Centrifugal, Positive Displacement, ESP, Jet
- **Rendering:** SkiaSharp-based pump performance charts

### 5.8 HydraulicPumps
- **Domain:** Hydraulic jet and piston pump analysis
- **Issue:** 40+ interface methods, most are stubs returning empty DTOs

### 5.9 SuckerRodPumping
- **Domain:** Sucker rod pumping — API RP 11L loads, fatigue analysis (modified Goodman), rod string optimization
- **Algorithms:** Dynamometer card analysis (10 pump condition diagnoses), Basquin S-N curve, Miner's rule
- **Issue:** API 11L implementation is simplified; material databases duplicated

### 5.10 PlungerLift
- **Domain:** Plunger lift for gas well deliquification
- **Algorithms:** Turner critical velocity, cycle timing, gas requirements
- **Issue:** Most methods return hardcoded values; rise velocity constant at 12.5 ft/s

### 5.11 EnhancedRecovery
- **Domain:** EOR — waterflooding, gas injection, chemical EOR, thermal recovery
- **Persistence:** PDEN-based (UnitOfWork pattern, different from other projects)
- **Module:** Full ModuleSetupBase with R_ENHANCED_RECOVERY_REFERENCE_CODE
- **Issue:** EOR type detection uses fragile substring matching

### 5.12 FlashCalculations (Most Rigorous)
- **Domain:** Vapor-liquid equilibrium (VLE) flash calculations
- **Algorithms:** Wilson K-values, Rachford-Rice Newton-Raphson, PR/SRK EOS, Michelsen stability test, GDEM acceleration, three-phase VLLE, phase envelope construction
- **Maturity:** Highest engineering rigor in the solution — academic references, advanced algorithms

### 5.13 OilProperties
- **Domain:** Black-oil PVT correlations
- **Algorithms:** Standing (Pb, Rs, Bo), Beggs-Robinson (dead/saturated viscosity)
- **Issue:** Some methods ignore GOR parameter

### 5.14 GasProperties
- **Domain:** Gas PVT properties
- **Algorithms:** Z-factor (Brill-Beggs, Hall-Yarborough, Dranchuk-Abu-Kassem/Standing-Katz), gas viscosity (Carr-Kobayashi-Burrows, Lee-Gonzalez-Eakin), pseudo-pressure integration
- **Issue:** Calculates viscosity with simple power-law instead of using dedicated calculator methods

### 5.15 Properties (Standalone)
- **Domain:** Combined oil + gas PVT service layer (no PPDM dependency)
- **Note:** Duplicates ZFactorCalculator, GasViscosityCalculator, OilPropertyCalculator from GasProperties and OilProperties projects
- **Issue:** Broad interface (~120 methods) but shallow implementations

---

## 6. Visualization Projects — Architecture

### 6.1 Beep.OilandGas.Drawing
- **Scale:** 145+ source files
- **Core:** Geological and petroleum engineering visualization library
- **Features:** Well schematics, reservoir maps, cross-sections, field maps, log visualization
- **Data loaders:** WITSML, LAS, DLIS, CSV, PPDM38, Prodml, RESQML
- **Export:** SVG, PDF, GeoJSON, georeferenced images
- **Styling:** USGS FGDC lithology patterns, custom themes

### 6.2 Beep.OilandGas.HeatMap
- **Scale:** 70+ source files
- **Core:** SkiaSharp-based spatial data heat maps
- **Features:** Configurable color schemes (Heat, Viridis), zoom/pan, UTM-to-canvas, IDW/Kriging interpolation, contour maps, clustering (Grid/KMeans/DBSCAN), kernel density estimation, time-series animation, real-time data

---

## 7. Cross-Cutting Architectural Patterns

### 7.1 Data Access Pattern (Canonical)
```csharp
var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME");
var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(
    _editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "TABLE_NAME");
var filters = new List<AppFilter> {
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId }
};
var entities = await repo.GetAsync(filters);
```

### 7.2 Service Registration Pattern
All services follow the factory pattern:
```csharp
builder.Services.AddScoped<IMyService>(sp => {
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new MyService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});
```

### 7.3 Module Setup Pattern
Every domain project with extension tables implements ModuleSetupBase:
```csharp
public sealed class MyModule : ModuleSetupBase {
    public override string ModuleId => "MY_DOMAIN";
    public override string ModuleName => "My Domain";
    public override int Order => 50;
    public override IReadOnlyList<Type> EntityTypes => new[] { typeof(MY_TABLE), typeof(R_MY_REFERENCE_CODE) };
    public override async Task<ModuleSetupResult> SeedAsync(string connectionName, string userId, CancellationToken ct) { ... }
}
```

### 7.4 Partial Class Service Pattern
Most engineering services split across multiple partial files:
- `Service.cs` — Core methods
- `Service.Advanced.cs` — Advanced/specialized calculations
- `Service.ModelsCoreImpl.cs` — Explicit interface implementation for cross-assembly contracts
- Optional: `.DataManagement.cs`, `.Reporting.cs`, `.Simulation.cs`, `.Optimization.cs`, etc.

### 7.5 Static Calculator Pattern
All engineering calculations use static classes:
- No DI, no state, pure functions
- Parameter validation via dedicated Validator classes
- Custom exception hierarchy per domain
- Constants centralized in per-project Constants classes

---

## 8. PPDM 3.9 Integration

### Schema
- Single unified PPDM 3.9 schema for all lifecycle data
- 1,400+ standard tables organized in 51 subject-area categories
- Extension tables (R_*, domain-specific) registered via domain ModuleSetup

### Well Services (WSC v3)
- 13 standard well status facets: Life Cycle, Role, Condition, Business Interest, Business Intention, Outcome, Play Type, Well Structure, Profile Type, Product Type, Product Significance, Fluid Direction, Regulatory Approval
- Well-level, wellbore-level, and wellhead-stream faceted status
- Database-first/catalog-fallback for offline resilience

### Reference Data
- 30+ R_* reference code tables seeded across all domain modules
- Enum-to-reference-table seeding via EnumReferenceDataSeeder
- JSON, CSV, and static catalog seeding mechanisms
- Industry standard code alignment (API, ISO, IHS, WSC)

---

## 9. Dependencies Map

### External Platform
- **TheTechIdea.Beep** (NuGet): IDMEEditor, IDataSource, AppFilter, Beep engine
- **IdentityServer** (Duende): JWT token issuance, OIDC endpoints
- **MudBlazor 7.x**: UI component framework
- **SkiaSharp**: Cross-platform 2D rendering (engineering charts, schematics, heat maps)
- **Serilog**: Structured logging

### Internal Dependency Flow
```
ApiService → LifeCycle → Domain Projects → PPDM39.DataManagement → PPDM39 → Models
     ↓            ↓              ↓                    ↓
    Web ←──── Client ←─────────────────────────────────┘
```

---

## 10. Known Architecture Debt

1. **Code Duplication:** ZFactorCalculator duplicated across GasProperties and Properties projects; Material databases duplicated in SuckerRodPumping
2. **PPDM39Metadata.Generated.cs** is 5.8MB — should be loaded from external data file
3. **PPDM39SetupController** is 3,155 lines — god controller needing decomposition
4. **ProductionAccounting Compatibility Layer** is 1,384 lines with 17 nested classes — technical debt hotspot
5. **1,400+ PPDM tables hardcoded** in PPDM39SchemaRegistry and Branchs tree — not database-introspected
6. **PPDM39.DataManagement** has no DI extension method — consumers must manually register 100+ services
7. **Mixed numeric types (decimal/double)** — extensive casting throughout all calculation projects
8. **UserManagement custom models vs PPDM models** — dual-schema risk with APP_* and PPDM table sets
9. **Fire-and-forget Task.Run patterns** in API without proper error handling
10. **In-memory caches (ConcurrentDictionary)** in ProductionAccounting not synced with database

---

**Document Version:** 1.0
**Generated:** 2026-06-27
**Methodology:** Full-source-code reading — every .cs file analyzed
