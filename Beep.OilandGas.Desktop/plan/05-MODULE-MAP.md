# Module Map — source project → desktop stage and features

**Document ID:** PLAN-05
**Version:** 1.0
**Created:** 2026-11-05

One row per source project. This is the completeness proof that "all Tech and Projects in repo"
are used. Detail lives in the named feature documents.

---

## 1. `Beep.OilandGas` — 39 framework projects

| # | Project | TFM | Stage | Features | What the desktop uses from it |
|---|---|---|---|---|---|
| 1 | `Beep.OilandGas.Models` | net10.0 | 01, 06, 08, 18 | DAT, RES, WTA, RPT-03 | `Core/Interfaces/*` (`IRenderingService`, `IHSEService`, `IGasPropertiesService`, `IOilPropertiesService`, `IWellTestAnalysisService`, `IPipelineAnalysisService`, `ISuckerRodPumpingService`, `IPlungerLiftService`, `IPPDMProductionService`), `Security/*`, `Core/Refusals/RefusalException`, `Data/Rendering/*` |
| 2 | `Beep.OilandGas.PPDM.Models` | net10.0 | 01, 02, 04, 05 | DAT-04, WEL, DEV, PRD | 2 740 PPDM 3.9 entity files under `39/` |
| 3 | `Beep.OilandGas.PPDM39` | net10.0 | 01 | DAT-02…05 | `IPPDMGenericRepository`, `PPDM39TableRegistry`, metadata, `PPDMTreeNode`, `IModuleSetup` base |
| 4 | `Beep.OilandGas.PPDM39.DataManagement` | net10.0 | 01, 02, 11, 13, 18 | DAT, WEL-04, HSE-02…06, WRK, RPT-01 | 40+ services and 6 core modules (`SYSTEM_SETUP`, `PPDM_CORE`, `R_SHARED_REFERENCES`, `WELL_STATUS_FACETS`, `WELL_REFERENCES`, `DEMO_DATA`) |
| 5 | `Beep.OilandGas.Repository` | net10.0 | 00, 18 | FND-07, REL-02 | `AddOilGasRepository`, `RepositoryDbContext`, RBAC, personas, asset access, `RepositoryCompanyContext` |
| 6 | `Beep.OilandGas.DataManager` | net10.0 | 01 | DAT-02 | `DataManager`, `ScriptValidator`, 46 `*ModuleData` scripts, `IExecutionStateStore` |
| 7 | `Beep.OilandGas.Drawing` | net10.0 | 15, 18 | VSB-03, VSB-06, VSB-09, RPT-02 | `DrawingEngine`, `WellRenderer`, `LogRenderer`, `EnhancedWellSchematicRenderer`, `FieldMapLayer`, `Reservoir*Layer`, `ImageExporter`, `SvgExporter`, `PdfExporter`, `GeoJsonExporter` |
| 8 | `Beep.OilandGas.HeatMap` | net10.0 | deferred | — (ADR-006) | `HeatMapRenderer` available to INT-07 as a layer |
| 9 | `Beep.OilandGas.Branchs` | net8/9/10 | 01 | DAT-03 | `PPDM39RootNode`, `PPDM39CategoryNode`, `PPDM39TableNode`, `BusinessProcess*Node` |
| 10 | `Beep.OilandGas.Accounting` | net10.0 | 10 | ACC-01, 02, 04, 05, 06, 08 | `IAccountingServices`, `GLAccountService`, `JournalEntryService`, `ARService`, `APInvoiceService`, `APPaymentService`, `PurchaseOrderService`, `InventoryService`, `PeriodClosingService`, `TrialBalanceService`, `FinancialStatementService`, `TaxProvisionService`, `FixedAssetLifecycleService`, `CostCenterService` |
| 11 | `Beep.OilandGas.ProductionAccounting` | net10.0 | 05, 09, 10 | PRD-04, PRD-05, ACC-03, ACC-07, ECO-02 | `IAllocationService`, `IRevenueService`, `IRoyaltyService`, `IProductionAccountingService`, `JointInterestBillingService`, `CostAllocationService`, `AfeService`, `PricingService`, `ProductionTaxService`, `ReserveAccountingService`, `TakeOrPayService`, `CopasOverheadService` |
| 12 | `Beep.OilandGas.ProductionOperations` | net10.0 | 04, 05, 13 | PRD-01…03, PRD-06, DEV-04, WRK-03 | `IProductionOperationsService`, `IProductionManagementService`, `IFacilityManagementService`, 14 FacilityManagement partials |
| 13 | `Beep.OilandGas.ProductionForecasting` | net10.0 | 06 | RES-06, RES-07 | `IProductionForecastingService`, `DCAAnalysisService`, `ProductionHistoryLoader`, `ProductionForecastResultMapper`, `DCAInteractionHandler` |
| 14 | `Beep.OilandGas.EconomicAnalysis` | net10.0 | 09 | ECO-01 | `IEconomicAnalysisService`, `ICalculationService`, `EconomicRenderer`, `EconomicInteractionHandler` |
| 15 | `Beep.OilandGas.LeaseAcquisition` | net10.0 | 03a (ADR-007) | LSE-01…03 | `ILeaseAcquisitionService`, `ILeaseManagementService`, `ILeaseAcquisitionAdvancedService`, 9 partials |
| 16 | `Beep.OilandGas.DrillingAndConstruction` | net10.0 | 04 | DEV-05…07 | `IDrillingOperationService`, `DrillingEngineeringService`, 8 partials, `DRILLING_*` extension tables |
| 17 | `Beep.OilandGas.DevelopmentPlanning` | net10.0 | 04 | DEV-01…04, DEV-08 | `IFieldDevelopmentService`, `IDevelopmentPlanService`, `DevelopmentPlanService.Advanced/.Baseline`, `FIELD_DEVELOPMENT_PLAN` and friends |
| 18 | `Beep.OilandGas.Decommissioning` | net10.0 | 12 | DEC-01…03 | `IFieldDecommissioningService`, `FieldDecommissioningService.Analysis`, `WellAbandonment*`, `FacilityDecommissioning*`, `DecommissioningCost*` |
| 19 | `Beep.OilandGas.EnhancedRecovery` | net10.0 | 06 | RES-04 | `IEnhancedRecoveryOperationsService`, `IEnhancedRecoveryService`, `EnhancedRecoveryService.Advanced` |
| 20 | `Beep.OilandGas.ProspectIdentification` | net10.0 | 03 | EXP-01…04 | `IProspectIdentificationService`, `IFieldExplorationService`, `IProspectRiskEconomicAnalysisService`, `IProspectTechnicalMaturationService`, `IProspectPortfolioOptimizationService`, `SeismicAnalysisService`, `PPDMExplorationService` |
| 21 | `Beep.OilandGas.PermitsAndApplications` | net10.0 | 11 | HSE-07 | `IPermitApplicationLifecycleService`, `IPermitApplicationWorkflowService`, `IPermitComplianceCheckService`, `IPermitComplianceReportService`, `IPermitStatusHistoryService`, `PermitStatusTransitionRules` |
| 22 | `Beep.OilandGas.HSE` | net10.0 | 11 | HSE-03 | `ICompanyHseWorkflowCommands`, `CompanyHseAssignedActions`, `CompanyHseCorrection`, `CompanyHseRcaHandoffReview`, `GhgArithmeticCheck`, `GhgFactorCatalog`, `InvestigatorAssignmentReviewValidator` |
| 23 | `Beep.OilandGas.LifeCycle` | net10.0 | 02, 05, 09, 11, 14 | WEL, WFL, HSE-01, ECO-03, RES-05, WTA-02 | `IFieldOrchestrator`, `IApprovalWorkflowEngine`, `IAuditChainService`, `WellComparisonService`, ~40 process services, 13 PPDM mappers, `PPDMCalculationService` (18 partials), `PPDMAccountingService`, `PPDMHSEService`; modules `LIFECYCLE`, `ACCOUNTING` |
| 24 | `Beep.OilandGas.UserManagement` | net10.0 | 00, 11, 18 | FND-07, HSE-01, REL-02 | `IDefaultSecuritySeedService` (19 personas), `IPersonaProfileService`, `IRoleAssignmentService`, `IRowLevelSecurityService`, `RoleHierarchyService`, `SodConflictDetector`, `PermissionHandler` |
| 25 | `Beep.OilandGas.ChokeAnalysis` | net10.0 | 07 | LFT-07 | `IChokeAnalysisService`, `ChokeAnalysisService.Advanced/.Multiphase` |
| 26 | `Beep.OilandGas.CompressorAnalysis` | net10.0 | 07 | LFT-08 | `ICompressorAnalysisService`, `CompressorAnalysisService.Advanced/.Contract` |
| 27 | `Beep.OilandGas.FlashCalculations` | net10.0 | 06 | RES-03 | `FlashCalculationService`, `FlashFeedCatalogMerge`, `PhaseEnvelope`, `AdvancedEOS` |
| 28 | `Beep.OilandGas.GasLift` | net10.0 | 07 | LFT-02 | `GasLiftService`, `GasLiftService.Advanced/.WireAsync`, `GasLiftPotentialWireMapper` |
| 29 | `Beep.OilandGas.GasProperties` | net10.0 | 06 | RES-01 | `IGasPropertiesService`, `GasPropertiesService.Advanced` |
| 30 | `Beep.OilandGas.OilProperties` | net10.0 | 06 | RES-02 | `IOilPropertiesService`, `OilPropertiesService.Advanced/.CompositionScreening` |
| 31 | `Beep.OilandGas.NodalAnalysis` | net10.0 | 07 | LFT-01 | `INodalAnalysisService`, `.Advanced/.ArtificialLiftScoring/.SensitivityScenarios`, `NodalRenderer`, `NodalInteractionHandler` |
| 32 | `Beep.OilandGas.PipelineAnalysis` | net10.0 | 08 | WTA-03 | `IPipelineAnalysisService`, 10 partials (`Integrity`, `LeakDetection`, `ErosionCorrosion`, `SlugFlow`, `Simulation`, `Optimization`, `Reporting`) |
| 33 | `Beep.OilandGas.PlungerLift` | net10.0 | 07 | LFT-05 | `IPlungerLiftService`, `PlungerLiftService.Advanced` |
| 34 | `Beep.OilandGas.PumpPerformance` | net10.0 | 07 | LFT-03 | `IPumpPerformanceService`, `PumpPerformanceRenderer`, `PumpPerformanceInteractionHandler` |
| 35 | `Beep.OilandGas.SuckerRodPumping` | net10.0 | 07 | LFT-04 | `ISuckerRodPumpingService` |
| 36 | `Beep.OilandGas.HydraulicPumps` | net10.0 | 07 | LFT-06 | `IHydraulicPumpService`, `HydraulicPumpService.Advanced` |
| 37 | `Beep.OilandGas.WellTestAnalysis` | net10.0 | 08 | WTA-01 | `IWellTestAnalysisService`, `WellTestRenderer`, `WellTestInteractionHandler` |
| 38 | `Beep.OilandGas.Client` | net10.0 | 00, all | FND-02 + every data feature | `IBeepOilandGasApp` (15 subsystems), `ServiceAccessMode`, `AppOptions`, `ClientApiTransport`, `IAuthenticationProvider`, `AddBeepOilandGasApp*` |
| 39 | `Beep.OilandGas.Diagnostics` | net10.0 | 00, 18 | FND-06, REL-05 | `OilGasDiagnosticsDbContext`, `AddOilGasDiagnostics`, 3 provider migrations |

### Referenced but not consumed as a library

| Project | Role |
|---|---|
| `Beep.OilandGas.ApiService` | Remote-mode server. The desktop talks to it over HTTP; it is never referenced. |
| `Beep.OilandGas.Web` | **Screen inventory source.** Every `Pages/PPDM39/**` and `Components/**` file informs a feature document. |
| `Beep.OilandGas.*.Tests` / `.Benchmarks` | Not referenced. Their assertions inform acceptance criteria. |

---

## 2. `OilandGasTools` — 12 libraries + 1 tool

| # | Package | TFM | Stage | Features | Capability consumed |
|---|---|---|---|---|---|
| 1 | `TheTechIdea.OilGas.Wellbore.Data` | net8/9 | 15, 16, 17 | VSB-02, ENG-01, ENG-02, ENG-10, INT-05, INT-06 | `WellModel` and friends, `WellboreEnvironment`, `WellboreServices`, `ToolboxRegistry`, `CountryPresets`, `SurveyUncertainty`, `AntiCollision`, `CasingDesign`, `PaySummary`, `Petrophysics`, `LogEditing`, `LeakPathAnalysis`, `BarrierValidator`, `MaaspScreening`, `Units` |
| 2 | `TheTechIdea.OilGas.Wellbore.Loaders` | net8/9 | 17 | INT-01…04, INT-06 | `LasReader/Writer`, `Las3Checker`, `DlisReader`, `Witsml*`, `OsduExport`, `DataSources`, `MappingProfile`, `MappedWellDataProvider`, `CsvTabularSource`, `ExcelTabularSource`, `EdmXmlTabularSource`, `TabularWriter`, `XlsxWriter`, `DataCoverage`, `FieldQc`, `ConfigurationResources` |
| 3 | `TheTechIdea.OilGas.Wellbore` | net8/9 | 15, 16 | VSB-01, 03, 06, 07, 09, ENG-09 | `ViewSurface`, `WellboreView`, `CorrelationView`, `CrossSectionView`, `MapView`, `LadderPlotView`, `TravellingCylinderView`, `WellboreDocument`, `WellboreExport`, `PanelExport`, `PrintPlanner`, `WellTrackRegistry` |
| 4 | `TheTechIdea.OilGas.WellLogs` | net8/9 | 15, 16 | VSB-04, ENG-10 | `WellLogView`, `WellLogDocuments`, `WellLogTemplates`, `WellLogTracks`, `Crossplots`, `CrossplotView`, `CementEvaluation`, `CurveEditing`, `LiveLogFeed` |
| 5 | `TheTechIdea.OilGas.WellSchematic` | net8/9 | 15 | VSB-05, VSB-08 | `WellSchematicView`, `WellSchematicSymbolView`, `WellModelConverter`, `WellSchematicLayoutEngine`, `WellSchematicValidator`, `SchematicTrack`, `BarrierTemplates`, `SymbolCatalog`, `WellComparison` |
| 6 | `TheTechIdea.OilGas.Wellbore.Setup` | net8/9 | 17 | INT-05, INT-06 | `SetupSession` (Create/Open/Get/Set/Validate/Preview/Verify/Handover), `DataSourcePlugins`, `MappingSuggestions` |
| 7 | `TheTechIdea.OilGas.Integrity` | net8/9 | 16 | ENG-03, 04, 05 | `MfcAnalysis`, `Rerating`, `CasingPressureDiagnostic`, `Mawop`, `IntegrityStatus`, `FieldIntegrity`, `IntegrityBoardView`, `CorrosionTrack` |
| 8 | `TheTechIdea.OilGas.Drilling` | net8/9 | 16 | ENG-06, 07, 08, 09 | `WellPlan`, `TorqueDrag`, `Hydraulics`, `DrillingPerformance`, `TimeDepthView` |
| 9 | `TheTechIdea.OilGas.Production` | net8/9 | 06, 07 | RES-06, LFT-01 cross-check | `DeclineFitter`, `Vogel`, `NodalAnalysis`, `VerticalLift`, `HornerAnalysis` — graphics-free, usable in view models |
| 10 | `TheTechIdea.OilGas.DataAI` | net8/9 | 17 | INT-03, INT-06 | `LegacyExtraction`, `WellReconciliation`, `OsduIngest` |
| 11 | `TheTechIdea.OilGas.Mapping` | net8/9 | 17 | INT-07, INT-08 | `Gridding`, `Kriging`, `Contouring`, `WellMaps`, `CoordinateSystems`/`Geodesy`, `ContourMapView`, `Scene3D`, `Scene3DView`, `MapExport`, `SceneExport` |
| 12 | `TheTechIdea.OilGas.Training` | net8/9 | 18 | TRN-01…03 | `ScenarioLibrary`, `Scenario`, `ScenarioScoring`, `ScenarioRender`, `CourseLibrary`, `CoursePack`, `ResultsLedger`, `SamplePacks`, `MapExhibit`, `TrainingServer` |
| 13 | `TheTechIdea.OilGas.ConfigTool` | net8 | 17 | INT-05 (parity) | CLI verbs `validate`, `show`, `diff`, `template`, `countries`, `verify`, `setup`, `qc`, `export-witsml`, `export-osdu` |

### Assets inside `OilandGasTools` the desktop should surface

| Asset | Used by |
|---|---|
| `Wellbore.Data/Configuration/Countries/*.json` (62 files) | INT-05 country picker |
| `Wellbore.Data/Configuration/environment.schema.json` + `environment.template.json` | INT-05 |
| `Wellbore.Data/Configuration/model-catalog.json`, `tool-models.json`, `leak-path-networks.json`, `WMM2025.COF` | VSB-02, ENG-02, ENG-04 |
| `Wellbore/Resources/lithology.json`, `lithology-standard.gost21302.json` | VSB-04 |
| `WellSchematic/Resources/wellschematic.symbols.json` (235 symbols), `component-symbols.json`, `barrier-templates.json` | VSB-08 |
| `Wellbore.Loaders/Profiles/*.profile.json` (`csv`, `ppdm39`, `edm`, `template`) | INT-04 |
| `Wellbore.Loaders/Samples/*.json` | INT-05 demo |
| `docs/customer/*.html` (22 pages) | Capability source for VSB/ENG/INT/TRN feature documents |

---

## 3. Coverage check

| Source set | Count | Mapped | Unmapped |
|---|---|---|---|
| `Beep.OilandGas` product projects | 39 | 39 | 0 |
| `OilandGasTools` libraries | 12 | 12 | 0 |
| `OilandGasTools` tools | 1 | 1 | 0 |
| `OilandGasTools` samples | 2 | 0 | **2 — intentional** |
| Total consumed | 52 | 52 | 0 |

**Samples excluded on purpose:** `samples/WinFormsViewer` and `samples/HeadlessReport` are
demonstrations of the same `ViewSurface` contract the desktop hosts. Their *patterns* are used
(`samples/Shared/Workbench.cs` for environment loading and document building), but the projects
are not referenced.
