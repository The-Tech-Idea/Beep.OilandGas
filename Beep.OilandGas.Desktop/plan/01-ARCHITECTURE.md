# Architecture — Beep.OilandGas.Desktop

**Document ID:** PLAN-01
**Version:** 1.0
**Created:** 2026-11-05

---

## 1. What this application is

A WPF desktop Oil & Gas workstation. It is the desktop counterpart of
`Beep.OilandGas.Web`, consuming the same module projects and the same service contracts, with
the additional wellbore/log/schematic rendering and well-engineering calculations of
`OilandGasTools`.

It is deliberately shaped like `Beep.DoitAll.Erp.Wpf`:

| ERP concept | Desktop equivalent |
|---|---|
| `App.xaml.cs` with headless verbs and startup sequence | `OgApp.xaml.cs` + `OgStartupSequence` |
| `ErpBootstrapper`, `ErpApplication`, `ErpSession`, `ErpPaths` | `OgBootstrapper`, `OgApplication`, `OgSession`, `OgPaths` |
| `ErpModuleCatalog` + `ErpPageDefinition` | `OgModuleCatalog` + `OgPageDefinition` |
| `ErpNavigationService` (entitlement + role gate) | `OgNavigationService` |
| `ErpShellWindow` + `ErpShellViewModel` | `OgShellWindow` + `OgShellViewModel` |
| `ErpSkins` / `ErpTypography` / `Loc` | `OgSkins` / `OgTypography` / `Loc` |
| `ErpNotificationService` / `ErpAuditLog` | `OgNotificationService` / `OgAuditLog` |
| `FirstRunWindow` / `LoginWindow` / `SettingsView` | `FirstRunWindow` / `SignInWindow` / `SettingsView` |
| `XxxViewModel` + `XxxView` per page | same, one pair per feature |

---

## 2. Layering

```
┌──────────────────────────────────────────────────────────────────────┐
│ Beep.OilandGas.Desktop  (WinExe, net10.0-windows)                    │
│ App · Startup · Shell · Setup wizard · Sign-in · Theme · Localization │
│ Route guard · Navigation host · Settings                              │
└──────────────┬───────────────────────────────────────────────────────┘
               │
┌──────────────▼───────────────┐  ┌────────────────────────────────────┐
│ .Modules (net10.0-windows)   │  │ .WellTools (net10.0-windows)       │
│ One folder per area:         │  │ OilandGasTools integration:         │
│   Foundation, Data, Wells,   │  │   Environment bootstrap            │
│   Exploration, Development,  │  │   ViewSurface → SKElement host     │
│   Production, Reservoir,     │  │   Track/theme bridging             │
│   Lift, WellTest, Economics, │  │   Toolbox registration             │
│   Accounting, HSE,           │  │   Export adapters                  │
│   Decommissioning, Work,     │  │                                    │
│   Workflow, Interop,         │  │                                    │
│   Training, Reporting        │  │                                    │
│ Views + ViewModels           │  │                                    │
└──────────────┬───────────────┘  └───────────────┬────────────────────┘
               │                                  │
┌──────────────▼──────────────────────────────────▼────────────────────┐
│ .Controls (net10.0-windows)                                          │
│ Shared WPF: page frame, data grid, filter bar, form builder,         │
│ KPI card, status badge, timeline, picker, dialog host, failure panel,│
│ Skia view host, busy overlay, empty/denied/failed state presenters   │
└──────────────┬───────────────────────────────────────────────────────┘
               │
┌──────────────▼───────────────────────────────────────────────────────┐
│ .Data (net10.0)                                                      │
│ Connection + module-database administration (BeepDM), module setup   │
│ orchestration, local service composition, repositories/unit of work  │
│ factories, PPDM tree/metadata adapters                               │
└──────────────┬───────────────────────────────────────────────────────┘
               │
┌──────────────▼───────────────────────────────────────────────────────┐
│ .Core (net10.0)                                                      │
│ Contracts, results, session, module keys, page catalog, permissions, │
│ notification contract, localization contract, path resolution,       │
│ deployment modes, licence/entitlement contract                       │
└──────────────┬───────────────────────────────────────────────────────┘
               │
┌──────────────▼───────────────────────────────────────────────────────┐
│ Beep.OilandGas.Client  (IBeepOilandGasApp)                           │
│ Remote (ApiService) │ Local (DI over module projects) │ Auto         │
└──────────────┬───────────────────────────────────────────────────────┘
               │
┌──────────────▼───────────────────────────────────────────────────────┐
│ Beep.OilandGas.* module projects  +  TheTechIdea.OilGas.* packages   │
└──────────────────────────────────────────────────────────────────────┘
```

**Dependency rule:** arrows point one way only. `Core` references nothing from the app.
`Modules` never references `Host`. `WellTools` never references `Modules`. Analyzer
`BEEP0001..0004` applies to every new project.

---

## 3. The data access spine

`Beep.OilandGas.Client` already solves the hard problem: it exposes fifteen subsystem
interfaces behind one façade, with three access modes.

```csharp
public interface IBeepOilandGasApp
{
    ServiceAccessMode AccessMode { get; }
    IConnectionService        Connection     { get; }
    IWellService              Well           { get; }
    IDataManagementService    DataManagement { get; }
    IPumpService              Pumps          { get; }
    IPropertiesService        Properties     { get; }
    ICalculationService       Calculations   { get; }
    IAnalysisService          Analysis       { get; }
    IFieldService             Field          { get; }
    ILifeCycleService         LifeCycle      { get; }
    IOperationsService        Operations     { get; }
    IDrillingService          Drilling       { get; }
    IProductionService        Production     { get; }
    IAccountingService        Accounting     { get; }
    IAccessControlService     AccessControl  { get; }
    IPermitService            Permits        { get; }
    ILeaseService             Lease          { get; }
}
```

### 3.1 Modes

| Mode | When | Composition |
|---|---|---|
| `Remote` | A company API is configured (base URL + token) | `AddBeepOilandGasAppRemote` — HTTP to `Beep.OilandGas.ApiService` |
| `Local` | Offline / personal installation | `AddBeepOilandGasAppLocal` — module services composed in-process over BeepDM |
| `Auto` | Default | Remote if configuration is complete, otherwise Local |

**Hard rule:** a view model receives an `I<Area>Service` or `IBeepOilandGasApp`. It never
receives `IDMEEditor`, `UnitofWork<T>`, `DbContext`, `SqlConnection` or any driver type.

**The only exception** is the Data Platform area (`DAT-01`, `DAT-02`), which administers
connections and runs module setup. Those screens call `IConnectionService`,
`IModuleBindingService` and the module-setup orchestrator — still not `IDMEEditor` directly
from the view model; the adapter lives in `.Data`.

### 3.2 Why not call module services directly from view models

Because the same view model must work in Remote mode. Direct calls would compile offline and
fail on a company deployment. The `Client` façade is the only path that exists in both modes.

### 3.3 Where the local composition lives

Two options, decided in ADR-004:

- **Preferred:** call `AddBeepOilandGasAppLocal` from `Beep.OilandGas.Client`, which already
  knows how to compose the local services.
- **Fallback:** `.Data` contains a `LocalServiceComposer` that registers the same interfaces by
  hand (mirroring `Beep.OilandGas.ApiService/Program.cs` lines ~1490–1820, which is the only
  complete list of calculation-service registrations in the repo).

---

## 4. Shell and navigation

### 4.1 One page catalog

```csharp
public sealed record OgPageDefinition(
    string Route,                       // "wells", "analysis/nodal"
    string Title,                       // fallback title; localized through "Page.<route>"
    string ModuleKey,                   // OgModuleKeys.*
    IReadOnlyList<string> RequiredRoles,
    string? RequiredPermission = null,
    string Group = "",
    string Glyph = "",
    string IconPath = "");

public static class OgModuleCatalog
{
    public static IReadOnlyList<OgPageDefinition> Pages { get; }
    public static OgPageDefinition? Find(string route);
}
```

The shell builds navigation from `OgModuleCatalog.Pages`, filtered by
`OgNavigationService.Build(permissions, companyType)`. `CreateView(route)` is a single
`switch` — the same shape as `ErpShellWindow.CreateView`, with the same property: a route the
catalog does not contain can never produce a view.

### 4.2 Two gates

```csharp
bool CanAccess(page, permissions, companyType) =>
       OgModuleCatalog.IsModuleEnabled(companyType, page.ModuleKey)
    && (!page.RequiresEntitlement || _entitlements.Allows(page.ModuleKey))
    && permissions is not null
    && (permissions.IsSystemAdmin
        || hasNavigationGrants  ? permissions.HasPermission(NavigationCodes.Code(page.Route))
                                : page.RequiredRoles.Any(permissions.HasRole))
    && (page.RequiredPermission is null
        || !permissions.PermissionSourceAvailable
        || permissions.HasPermission(page.RequiredPermission));
```

### 4.3 Module keys

Derived from the 29 registered `ModuleId` values so that commercial gating and module setup
agree:

```
core, data, wells, exploration, development, drilling, production, reservoir,
properties, forecasting, lift, welltest, pipeline, economics, accounting,
hse, permits, decommissioning, work, workflow, interop, training, reporting,
administration
```

`properties`, `forecasting`, `lift`, `welltest` and `pipeline` map onto the calculation
`ModuleId`s (`OIL_PROPERTIES`, `GAS_PROPERTIES`, `FLASH_CALCULATIONS`, `PRODUCTION_FORECASTING`,
`NODAL_ANALYSIS`, `GAS_LIFT`, `CHOKE_ANALYSIS`, `COMPRESSOR_ANALYSIS`, `PIPELINE_ANALYSIS`,
`WELL_TEST_ANALYSIS`).

### 4.4 Personas

`Beep.OilandGas.UserManagement.DefaultSecuritySeedService` already seeds 19 personas with
`DEFAULT_LANDING_ROUTE`. The desktop reads those routes and shows them as the "My workspace"
entry — identical to `PersonaNavigation.HomeRoute()` in Web.

---

## 5. Screens: the mandatory nine-section pattern

Every feature document uses the same structure, and every implemented page follows it:

1. **Header** — title, context line (company · field · well · period), status badge.
2. **Filter/context bar** — the pickers that scope the page.
3. **Work area** — grid, chart, form or diagram.
4. **Detail pane** — selected-row detail, collapsible.
5. **Command bar** — actions allowed by the current role.
6. **State presenter** — empty / loading / failed / denied / unsaved.
7. **Progress surface** — cancellable, with a progress value and stage text.
8. **Recovery surface** — retained original, retry, discard (for write operations).
9. **Help/context** — `ContextHelp` text and a deep link to the relevant plan section.

Reference: `Beep.OilandGas.Web/Components/Shared/ContextHelp.razor`,
`Components/Shared/CalculationResultTabs.razor`, `Components/Progress/MultiOperationProgress.razor`.

---

## 6. SkiaSharp hosting

### 6.1 The contract

`OilandGasTools` exposes:

```csharp
public abstract class ViewSurface
{
    public float Width, Height;
    public event EventHandler? Invalidated;
    public event EventHandler<ViewSetting>? SettingChanged;
    public void Draw(SKCanvas canvas);            // seals branding, calls DrawContent
    protected abstract void DrawContent(SKCanvas canvas);
    public bool HandleMouseDown(SKPoint); HandleMouseMove; HandleMouseUp;
    public bool HandleMouseWheel(SKPoint, float delta);
    public bool HandleKey(ViewKey, ViewModifiers);
    public bool HandlePinch(SKPoint, float scale);
    public bool HandleTwoFingerPan(SKPoint delta);
}
```

### 6.2 The WPF adapter

`WellTools/Hosting/SkiaViewSurfaceHost.cs` — a `ContentControl` hosting an `SKElement`:

```csharp
public sealed class SkiaViewSurfaceHost : ContentControl
{
    public ViewSurface? Surface { get; set; }
    // PaintSurface → set Surface.Width/Height from e.Info → Surface.Draw(e.Surface.Canvas)
    // Surface.Invalidated → Dispatcher.BeginInvoke(InvalidateVisual)
    // MouseDown/Move/Up/Wheel  → Handle* with Point→SKPoint
    // PreviewKeyDown            → key map → ViewKey + ViewModifiers
    // ManipulationDelta         → HandlePinch / HandleTwoFingerPan
    // AutomationPeer            → expose AccessibleName + AccessibleChildren
}
```

This one control is what makes all nine `VSB` features and several `ENG` features possible.
It is built once, in `VSB-01`, and reused everywhere.

### 6.3 Environment ordering constraint

`OilandGasTools` views read `EnvironmentBridge.Env` at **construction**. Therefore:

1. Register every toolbox (`IntegrityToolbox.Register()`, `DrillingToolbox.Register()`, …).
2. Set `WellboreServices.Global` from the configured environment file.
3. Only then construct views.

`VSB-02` owns this sequence and must run before any `VSB-03…09` screen is created.

---

## 7. Failure, audit and notifications

**One pipeline. No swallowed exception. One toast surface.** Full contract:
[`07-LOGGING-AUDIT-AND-NOTIFICATIONS.md`](07-LOGGING-AUDIT-AND-NOTIFICATIONS.md).

```
catch → IFailureReporter.ReportHandled(ex, operation, consequence, severity)
write → IOgAuditService.Add(verb, subjectKind, subjectId, outcome, detail)
trace → ILogger (framework only)

            └────────────► OgDiagnosticsPipeline ◄────────────┘
                                   │
              ┌────────────────────┼─────────────────────┐
              ▼                    ▼                     ▼
      durable store          rolling log          OgNotificationHub
      (OilGasDiagnostics)    (BEEP_LOG_DIR)              │
                                                          ▼
                                            OgToastService → ToastHost
                                                          + NotificationCenter
```

| Concern | Mechanism |
|---|---|
| Every caught exception | `IFailureReporter.ReportHandled` — returns a 12-hex reference |
| Every uncaught exception | `DispatcherUnhandledException`, `AppDomain.UnhandledException`, `TaskScheduler.UnobservedTaskException` → `ProcessFailureSubscription` → same reporter |
| Durable record | `Beep.OilandGas.Diagnostics` `OilGasDiagnosticsDbContext` via `AddOilGasDiagnostics`, with retention |
| Audit trail | `IOgAuditService`, written alongside the owning store's audit (`Repository`, `PPDMAccessAuditService`) |
| Diagnosis | framework `ILogger` to `BEEP_LOG_DIR` — never a substitute for reporting |
| Visible message | `OgToastService`, driven by the hub, never called by a feature |
| Compile-time gate | `BEEP0001..0004` analyzers + `CS8509` as error, from `Directory.Build.props` |
| Test-time gate | the ten guard tests in PLAN-07 §6 |

**Rules:**

- If a read fails, the screen shows *failed* with `FailurePanel` and a reference — not *zero rows*.
- If a write outcome is unknown, the screen retains the original and offers retry; it never
  re-submits silently.
- A toast never contains exception text, a credential, a token or a connection string.
- A `catch` that only logs is a defect, not a mitigation.

---

## 8. Threading and long operations

- Every data call is `async` with a `CancellationToken`.
- Every page exposes `LoadAsync(CancellationToken)`; the shell cancels the previous page's token
  when navigation changes.
- A late response for a superseded context is discarded by comparing a context generation
  number — the same technique Web uses in its `*State.cs` classes and ERP uses with
  `_restoringSelection`.
- Skia rendering stays on the UI thread; data preparation for a view runs off-thread.

---

## 9. Configuration and files

```
%LOCALAPPDATA%\TheTechIdea\BeepOilGas\
├─ Beep\                     BeepDM configuration (drivers, connections)
├─ Data\                     local database files
├─ Diagnostics\              failure store (failures.db) — ADR-013
├─ ConnectionDrivers\        packaged driver assemblies
├─ NuGet\                    local package feed
├─ Logs\                     rolling logs
├─ Wellbore\                 OilandGasTools environment.json + parts
├─ skin.json                 selected Beep design system
├─ setup.json                setup wizard state
├─ licence.json              entitlement (optional)
└─ smoke-result.txt          headless smoke output
```

Nothing is ever written beside the executable, inside the install folder, under `%ProgramData%`, in
the roaming profile, or in the repository. Checked by guard `G-11` (`DataRootGuardTests`).

Override root with `BEEP_OILGAS_DATA_ROOT`; override log dir with `BEEP_LOG_DIR`
(mirrors `ErpPaths`).

---

## 10. Deployment modes

| Mode | Storage | Identity | API |
|---|---|---|---|
| `Personal` | Local file database | Local credential store | none |
| `CompanyDatabase` | Shared SQL Server / PostgreSQL / Oracle | IdentityServer token | none (direct DB through ApiService-owned connection) |
| `CompanyApi` | none locally | IdentityServer token | `Beep.OilandGas.ApiService` |

Mirrors `ErpDeploymentMode`. `OgApplication.GetSetupBlocker()` refuses to open the shell when
the chosen mode is not usable — the exact ERP behaviour.

---

## 11. What the desktop does not do

- It does not re-implement any calculation. `AfeService`, `CasingDesign`, `DeclineFitter`, … are
  called, never copied.
- It does not define a second PPDM entity model.
- It does not create EF migrations. Business schema stays entity-driven Beep Migration.
- It does not host the Web UI. The Web and desktop are siblings over the same services.
- It does not run the `OilandGasTools` `setup-app` WinForms tool; `INT-05` re-implements its
  screens in WPF over the same `SetupSession` engine.
