# Solution Layout — Beep.OilandGas.Desktop

**Document ID:** PLAN-03
**Version:** 1.0
**Created:** 2026-11-05

This is the exact project manifest to create in `STG-00`. Nothing here exists yet.

---

## 1. Root

```
Beep.OilandGas.Desktop/
├─ plan/
├─ src/
│  ├─ Beep.OilandGas.Desktop.slnx
│  ├─ Directory.Build.props
│  ├─ Directory.Packages.props
│  ├─ Beep.OilandGas.Desktop/                 WinExe      net10.0-windows
│  ├─ Beep.OilandGas.Desktop.Core/            library     net10.0
│  ├─ Beep.OilandGas.Desktop.Data/            library     net10.0
│  ├─ Beep.OilandGas.Desktop.Controls/        library     net10.0-windows
│  ├─ Beep.OilandGas.Desktop.Modules/         library     net10.0-windows
│  └─ Beep.OilandGas.Desktop.WellTools/       library     net10.0-windows
└─ test/
   ├─ Beep.OilandGas.Desktop.Tests/           xunit       net10.0-windows
   ├─ Beep.OilandGas.Desktop.IntegrationTests/ xunit      net10.0-windows
   └─ Beep.OilandGas.Desktop.SmokeTests/      xunit       net10.0-windows
```

Six product projects, three test projects. Rationale in ADR-005.

---

## 2. Project details

### 2.1 `Beep.OilandGas.Desktop` (host)

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net10.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <LangVersion>latest</LangVersion>
  <RootNamespace>TheTechIdea.Beep.OilandGas.Desktop</RootNamespace>
  <AssemblyName>TheTechIdea.Beep.OilandGas.Desktop</AssemblyName>
  <ApplicationManifest>app.manifest</ApplicationManifest>
  <NeutralLanguage>en</NeutralLanguage>
</PropertyGroup>
```

Folders:

```
App.xaml / App.xaml.cs
GlobalUsings.cs
app.manifest
Bootstrapping/     OgApplication.cs, OgBootstrapper.cs, OgPaths.cs, OgSession.cs,
                   OgStartupSequence.cs, OgDeploymentSettings.cs
Hosting/           OgServiceAccess.cs, LocalServiceComposer.cs, OgEnvironment.cs
Modules/           OgModuleCatalog.cs, OgModuleKeys.cs, OgPageDefinition.cs, OgRoleNames.cs
Security/          OgEntitlementService.cs, OgLicenceVerifier.cs, OgLocalIdentityService.cs,
                   OgExternalIdentityService.cs, OgLocalCredentialStore.cs
Services/          OgNavigationService.cs, OgBrandingService.cs, OgHealthService.cs,
                   OgBackupService.cs, OgWorkstationSettingsStore.cs, OgWriteAdmissionService.cs,
                   OgRouteHost.cs
Services/Diagnostics/
                   OgDiagnosticsPipeline.cs   implements IFailureReporter + IOgAuditService
                   OgNotificationHub.cs      severity → toast kind, dedupe, reference
                   OgToastService.cs         dispatcher-marshalled queue
                   OgFailureMessages.cs      Loc keys per consequence
                   OgFailureDetailsWindow.xaml(.cs)
                   OgApprovedStartupMessageBox.cs   the single allowed MessageBox
Setup/             FirstRunWindow.xaml(.cs), SetupSteps/, OgSetupStore.cs, RegionalCatalog.cs
Shell/             OgShellWindow.xaml(.cs), OgShellViewModel.cs, OgNavigationItem.cs,
                   NotificationCenterWindow.xaml(.cs), CommandPaletteWindow.xaml(.cs)
Theme/             OgSkins.cs, OgTypography.cs, Tokens.xaml, DiagnosticsOverlay.xaml
Localization/      Loc.cs, LocExtension.cs, Strings.resx, Strings.<culture>.resx
Views/             SignInWindow.xaml(.cs), SettingsView.xaml(.cs), AboutWindow.xaml(.cs),
                   LogViewerWindow.xaml(.cs)
Assets/Icons/      nav.<route>.svg  (one per page in the catalog)
```

Key behaviours to mirror from the ERP reference:

- `static App()` sets `BEEP_OILGAS_DATA_ROOT` for `--smoke` and `BEEP_LOG_DIR` before any service reads paths.
- `OnStartup` wires three unhandled-exception handlers, initialises skins/localization/typography, then dispatches headless verbs.
- Headless verbs: `--smoke`, `--schema-apply`, `--schema-resume`, `--drivers-report`, `--entitlement-report`, `--licence-check <file>`, `--restore <file>`, `--seed-benchmark <n>`, `--report-benchmark`.
- `ShutdownMode="OnExplicitShutdown"`; closing the shell is the explicit exit.
- Setup gate: `IsSetupComplete` false → `FirstRunWindow`; initialise failure → repair through the same window.

### 2.2 `Beep.OilandGas.Desktop.Core` (contracts)

`net10.0`, no UI, no BeepDM.

```
Contracts/     IOgNotificationService.cs, IOgAuditLog.cs, IOgNavigationService.cs,
               IOgEntitlementService.cs, IOgBranding.cs, IOgClock.cs
Results/       OgResult.cs, OgResult<T>.cs  (Success, Message, Detail, Issues)
Modules/       OgModuleKeys.cs, OgPageDefinition.cs, OgModuleCatalog.cs (empty shell),
               OgRoleNames.cs, NavigationPermissionCatalog.cs
Session/       OgSession.cs, OgDeploymentMode.cs, OgCompanyContext.cs
Permissions/   OgPermissions.cs
Localization/  ILocProvider.cs
Failures/      OgFailure.cs, OgFailureSeverity.cs
```

`Core` is testable on any OS. It is the only project the test projects can reference without a
Windows desktop.

### 2.3 `Beep.OilandGas.Desktop.Data` (platform adapters)

`net10.0`. References the BeepDM/Beep.OilandGas data-side projects.

```
Connections/   OgConnectionCatalog.cs, OgDriverCatalog.cs, OgConnectionTester.cs
Modules/       OgModuleBindingService.cs, OgModuleSetupRunner.cs, OgModuleSetupResult.cs
Ppdm/          OgPpdmTreeBuilder.cs, OgPpdmMetadataReader.cs, OgPpdmTableGateway.cs
Local/         LocalServiceComposer.cs, LocalBeepHost.cs
Setup/         OgLocalDatabaseProvisioner.cs, OgSchemaValidationService.cs
```

Only this project may reference `IDMEEditor`, `DatasourceManagementService`,
`UnitofWork<T>`, `IPPDMGenericRepository` and driver types.

### 2.4 `Beep.OilandGas.Desktop.Controls` (shared WPF)

`net10.0-windows`. References `TheTechIdea.Beep.WPF.Controls` and `SkiaSharp.Views.WPF`.

```
PageFrame.xaml            Title + context + status + command bar + content slot
StatePresenter.xaml       Empty / Loading / Failed / Denied / Unsaved
BusyOverlay.xaml          Cancellable progress with stage text
FilterBar.xaml            Declarative filter host
OgDataGrid.xaml           Virtualised grid, column chooser, group, export
OgFormBuilder.xaml        Metadata-driven form from PPDM entity metadata
KpiCard.xaml, StatusBadge.xaml, TrendSpark.xaml
Timeline.xaml             Step timeline used by WFL-03
PickerField.xaml          Company / field / well / period / persona pickers
FailurePanel.xaml         Failure reference + copy + report + retry (inline failed state)
ContextHelp.xaml          Reused copy of the Web component's behaviour
SkiaViewSurfaceHost.cs    The ViewSurface adapter (VSB-01)
ChartHost.cs              SKElement wrapper for IRenderingService charts (RPT-03)
Notifications/
  ToastHost.xaml(.cs)     Stacked live toasts, worst on top, max 5, ARIA live setting
  ToastCard.xaml(.cs)     One toast: icon, localized text, reference, copy, action, dismiss
  NotificationCenter.xaml(.cs)  History, filter, mark read
  Dialogs/                Confirm, Reason, PickOne, UnsavedChanges, FailureDetails
```

### 2.5 `Beep.OilandGas.Desktop.Modules` (pages)

`net10.0-windows`. One folder per area; each folder has `Views/` and `ViewModels/`.

```
Foundation/    Setup wizard steps, SignIn, Settings, About, LogViewer, Diagnostics,
               Failures (durable list), AuditLog, FailureDetails
Data/          Connections, ModuleBindings, PpdmTree, TableWorkbench, EntityForm,
               CsvImport, DataQuality, Versioning, AccessAudit, Sync
Wells/         WellSearch, WellHeader, Wellbores, WellStatus, Stratigraphy, WellCompare
Exploration/   ExplorationWorkspace, ProspectBoard, ProspectEditor, Seismic, WellProgram
Development/   DevelopmentWorkspace, FdpWizard, FdpReview, WellDesign, PoolsFacilities,
               DrillingProgram, DrillingExecution, CompletionTests, Construction
Production/    OperationsCentre, FieldPoolRegister, WellStatusBoard, DefermentLog,
               Allocation, VolumeReconciliation, ProductionReporting, Interventions
Reservoir/     GasProperties, OilProperties, Flash, EorScreening, Reserves,
               Characterization, DeclineCurve, Forecasting
Lift/          Nodal, GasLift, PumpPerformance, SuckerRod, PlungerLift,
               HydraulicPumps, Choke, Compressor
WellTest/      WellTestAnalysis, WellTestRegister, Pipeline
Economics/     EconomicEvaluation, Afe, InvestmentReview
Accounting/    Dashboard, PeriodClose, Ledger, Revenue, Receivables, Payables,
               Warehouse, Royalties, Jib, CostAllocation, Tax
Hse/           HseWorkspace, Incidents, IncidentDetail, Investigation, Hazop,
               ComplianceCalendar, Obligations, Ghg, Permits
Decommissioning/ Workspace, WellPa, FacilityDecom, CostEstimate
Work/          WorkOrderDashboard, WorkOrderDetail, WorkOrderCalendar,
               Maintenance, Inspections, Contractors, PermitToWork
Workflow/      ProcessDashboard, Definitions, Instances, Gates, TaskInbox,
               Analytics, Admin
Interop/       LasDlis, Witsml, Osdu, MappingProfiles, SetupWizard, FieldQc,
               Mapping, Scene3D
Training/      ScenarioRunner, Courses, Results, TrainingServer
Reporting/     ReportCatalogue, Templates, PrintPacks, Charts
```

Estimated 190–230 files at completion.

### 2.6 `Beep.OilandGas.Desktop.WellTools` (OilandGasTools adapter)

`net10.0-windows`. References the `TheTechIdea.OilGas.*` packages.

```
Environment/   OgWellboreEnvironment.cs, OgToolboxBootstrap.cs, OgEnvironmentStore.cs
Hosting/       SkiaViewSurfaceHost.cs (thin re-export), ViewInputMapper.cs,
               ViewKeyMap.cs, AccessibilityPeer.cs
Bridging/      OgWellModelMapper.cs       WellModel ↔ PPDM WELL/WELLBORE
               OgDocumentMapper.cs        WellboreDocument ↔ PPDM logs/tops
               OgSchematicMapper.cs       WellSchematicDocument ↔ PPDM tubulars
               OgMapCoordinateMapper.cs   MapWell ↔ FIELD/WELL coordinates
Export/        OgExportService.cs         PDF/SVG/PNG/GLB/XLSX via tools exporters
Toolboxes/     SurveyToolboxVm, CasingToolboxVm, IntegrityToolboxVm,
               DrillingToolboxVm, ProductionToolboxVm, MappingToolboxVm,
               TrainingToolboxVm
```

This is the only project where SkiaSharp types appear.

---

## 3. Central package management

`src/Directory.Packages.props` with `ManagePackageVersionsCentrally=true`.

Starting versions (to be confirmed by ADR-001/ADR-002):

| Package | Version | Notes |
|---|---|---|
| `CommunityToolkit.Mvvm` | 8.4.2 | Same as ERP reference |
| `Microsoft.Extensions.DependencyInjection` | 10.0.10 | |
| `Microsoft.Extensions.Logging` | 10.0.10 | |
| `Microsoft.Extensions.Logging.Debug` | 10.0.10 | |
| `Microsoft.Extensions.Http` | 10.0.10 | For remote mode |
| `Microsoft.Extensions.Options` | 10.0.10 | |
| `System.Security.Cryptography.ProtectedData` | 10.0.10 | Credential store |
| `SkiaSharp` | 4.154.0 | ADR-001 |
| `SkiaSharp.Views.WPF` | *match* | ADR-002 |
| `Svg.Skia` | 5.1.1 | Replaces `SkiaSharp.Svg` |
| `xunit` | 2.9.x | Tests |
| `xunit.runner.visualstudio` | 2.8.x | Tests |
| `Microsoft.NET.Test.Sdk` | 17.11.x | Tests |
| `coverlet.collector` | 6.x | Tests |

`src/Directory.Build.props` imports the repository `Directory.Build.props` so the
`BEEP0001..0004` analyzers and `CS8509` rule apply automatically, then adds product metadata:

```xml
<OgProductVersion>0.1.0</OgProductVersion>
<Version>$(OgProductVersion)</Version>
<Product>Beep Oil &amp; Gas Desktop</Product>
<Company>TheTechIdea</Company>
```

---

## 4. Project references

```
Host        → Core, Data, Controls, Modules, WellTools
Modules     → Core, Controls
WellTools   → Core, Controls
Data        → Core
Controls    → Core
Core        → (nothing from the desktop)
```

Forbidden: `Modules → Data`, `WellTools → Modules`, `Controls → Modules`, `Core → anything WPF`.

---

## 5. References into the two source repositories

```xml
<!-- Existing Oil & Gas modules -->
<ProjectReference Include="..\..\..\Beep.OilandGas.Client\...\csproj" />
<ProjectReference Include="..\..\..\Beep.OilandGas.LifeCycle\...\csproj" />
... (Host and Data reference the module set they compose)

<!-- OilandGasTools -->
<ProjectReference Include="..\..\..\..\OilandGasTools\src\TheTechIdea.OilGas.Wellbore\...\csproj" />
```

**Relative-path caveat.** `OilandGasTools` is a sibling checkout
(`..\The-Tech-Idea\OilandGasTools`). Two options, decided in ADR-008:

- **A (recommended):** consume `TheTechIdea.OilGas.*` as **NuGet packages** pinned in
  `Directory.Packages.props`. Keeps the desktop build independent of a second working copy and
  matches how the packages are published.
- **B:** `ProjectReference` into the sibling checkout, with a property for the path
  (`OilGasToolsRoot`) so a machine without the checkout fails loudly at restore.

Until ADR-008 is settled, Stage 0 must not reference `WellTools` from the host.

---

## 6. Test projects

| Project | Covers |
|---|---|
| `.Tests` | Core contracts, catalog, guard logic, localization keys, view-model unit tests with substituted services |
| `.IntegrationTests` | `LocalServiceComposer` against a real SQLite PPDM database; module setup; PPDM repository reads/writes |
| `.SmokeTests` | Drives `--smoke` in-process and asserts `smoke-result.txt`; constructs every view once |

`SmokeTests` mirrors the ERP's `ConstructViews(_application)` pattern: it proves every page can
be instantiated without throwing, which catches XAML/resource mistakes that unit tests miss.

---

## 7. Files to create in STG-00, in order

1. `src/Directory.Build.props`, `src/Directory.Packages.props`
2. `src/Beep.OilandGas.Desktop.Core/` (+ `OgModuleCatalog` shell, `OgResult`)
3. `src/Beep.OilandGas.Desktop/` host with `App.xaml` and an empty shell
4. `src/Beep.OilandGas.Desktop.slnx`
5. `test/*` projects
6. `--smoke` verb that writes `smoke-result.txt`
7. `src/Beep.OilandGas.Desktop.Controls/` with `PageFrame` and `StatePresenter`
8. `src/Beep.OilandGas.Desktop.Modules/` with `Foundation/`
9. `src/Beep.OilandGas.Desktop.Data/` with the connection adapter
10. `src/Beep.OilandGas.Desktop.WellTools/` — created but excluded from the host build until
    ADR-001/002/008 are settled
