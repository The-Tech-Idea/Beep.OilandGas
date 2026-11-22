# STG-00 — Foundation and host shell

| Field | Value |
|---|---|
| Stage | STG-00 |
| Features | 7 (FND-01 … FND-07) |
| Depends on | — |
| Status | Gate met for FND-01, FND-02, FND-03, FND-04, FND-05, FND-06 and FND-07. See [HEADLESS](../evidence/STG-00-headless-verbs/observed-evidence.json), [FND-02](../evidence/STG-00-FND-02/observed-evidence.json), [FND-06](../evidence/STG-00-FND-06/observed-evidence.json), [FND-07](../evidence/STG-00-FND-07/observed-evidence.json) |
| Exit gate | The app signs in, shows a permission-filtered shell, and `--smoke` writes `result: ok` |

---

## 1. Objective

Create the WPF host that every later stage plugs into: the application object, the service
composition, the shell, the page catalog, the theme, the localization layer, the failure path,
and the setup/sign-in journey. Nothing in this stage shows business data; everything in this
stage makes business data presentable.

This is a direct transcription of the `Beep.DoitAll.Erp.Wpf` startup contract, adapted to the
Oil & Gas service façade.

---

## 2. Scope

**In**

- The six `src` projects and the three `test` projects from PLAN-03, buildable.
- `--smoke`, `--schema-apply`, `--drivers-report`, `--entitlement-report`, `--licence-check` verbs.
- `OgModuleCatalog` with the seven Stage-0 pages plus the full catalog declared (pages can point
  at a placeholder view until their stage lands).
- `OgNavigationService`, route guard, entitlement interface (permissive default), role gate.
- Theme via `BeepChrome` design systems, `Tokens.xaml`, `OgTypography`.
- `Loc` with English resources and the 40-culture resource files scaffolded.
- `OgNotificationService`, `OgAuditLog`, diagnostics wiring, `FailurePanel`.
- `FirstRunWindow` (setup), `SignInWindow`, `OgSession`.
- `PageFrame`, `StatePresenter`, `BusyOverlay`, `OgDataGrid`, `KpiCard`, `StatusBadge`.
- One real page: `Settings`.

**Out**

- Any connection to a database (STG-01).
- Any business page.
- Any `OilandGasTools` reference (blocked on ADR-001/002/008).
- Licensing enforcement (REL-02); the interface exists and is permissive.

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| Reference `Beep.DoitAll.Erp.Wpf` | Structure to mirror: `App.xaml.cs`, `ErpBootstrapper`, `ErpApplication`, `ErpSession`, `ErpPaths`, `ErpModuleCatalog`, `ErpNavigationService`, `ErpShellWindow`, `ErpSkins`, `ErpNotificationService`, `ErpAuditLog`, `FirstRunWindow`, `LoginWindow` |
| `Beep.OilandGas.Client` | `IBeepOilandGasApp`, `ServiceAccessMode`, `AppOptions`, `IAuthenticationProvider` |
| `Beep.OilandGas.Repository` + `UserManagement` | Personas, roles, permissions for FND-07 |
| `Beep.OilandGas.Diagnostics` | Failure store for FND-06 |
| `TheTechIdea.Beep.WPF.Controls` | `BeepForm`, `BeepChrome`, `WpfThemesManager`, `BeepControlStyle` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| FND-01 | Host application, bootstrap and headless commands | [FND-01](../features/FND/FND-01-Host-Application-Bootstrap-and-Headless-Commands.md) |
| FND-02 | Service composition and access modes | [FND-02](../features/FND/FND-02-Service-Composition-and-Access-Modes.md) |
| FND-03 | Shell window and navigation catalog | [FND-03](../features/FND/FND-03-Shell-Window-and-Navigation-Catalog.md) |
| FND-04 | Theme, skin and branding | [FND-04](../features/FND/FND-04-Theme-Skin-and-Branding.md) |
| FND-05 | Localization, RTL and number/date formatting | [FND-05](../features/FND/FND-05-Localization-RTL-and-Formatting.md) |
| FND-06 | Notifications, failure reporting, audit and toast pipeline | [FND-06](../features/FND/FND-06-Notifications-Failures-and-Audit.md) |
| FND-07 | First-run setup, sign-in and session | [FND-07](../features/FND/FND-07-First-Run-Setup-Sign-In-and-Session.md) |

---

## 5. New code introduced

| Project | Key types |
|---|---|
| `.Core` | `OgResult<T>`, `OgModuleKeys`, `OgPageDefinition`, `OgModuleCatalog`, `OgRoleNames`, `OgSession`, `OgDeploymentMode`, `OgPermissions`, `IOgNotificationService`, `IOgAuditLog`, `IOgNavigationService`, `IOgEntitlementService`, `ILocProvider` |
| `.Data` | Nothing yet beyond an empty `OgConnectionCatalog` stub |
| `.Controls` | `PageFrame`, `StatePresenter`, `BusyOverlay`, `OgDataGrid`, `KpiCard`, `StatusBadge`, `FailurePanel` |
| `.Modules` | `Foundation/Views/*`, `Foundation/ViewModels/*` (Setup steps, SignIn, Settings, About, Diagnostics) |
| `.Host` | `App`, `OgStartupSequence`, `OgBootstrapper`, `OgApplication`, `OgPaths`, `OgSession`, `OgModuleCatalog` (concrete pages), `OgNavigationService`, `OgNotificationService`, `OgAuditLog`, `OgSkins`, `OgTypography`, `Loc`, `OgShellWindow`, `OgShellViewModel`, `FirstRunWindow`, `SignInWindow` |
| `.WellTools` | Created, empty, excluded from host build |

---

## 6. Architecture notes

- **Startup order is fixed:** static ctor sets env vars → theme/localization/typography →
  headless verb dispatch → bootstrapper → setup gate → sign-in → shell. Reordering breaks the
  `--smoke` root isolation.
- **`ShutdownMode="OnExplicitShutdown"`.** The sign-in dialog must be able to close without
  ending the process. Closing the shell is the explicit exit.
- **Catalog declares the whole product.** Every page in the master tracker is added to
  `OgModuleCatalog` in this stage, each pointing at a `PlaceholderView` whose title says which
  stage will replace it. This makes the navigation, permission and entitlement gates testable
  from day one and prevents "add a page" being a late-stage risk.
- **`OgApplication` is the composition root.** View models receive it or a service interface,
  never the `ServiceProvider`.

---

## 7. Dependencies and sequencing

1. ADR-003 (TFM) and ADR-004 (access mode) probes must be recorded first.
2. Create `Directory.Build.props`, `Directory.Packages.props`, `.Core`, host skeleton, solution.
3. Empty shell + one page (`Settings`) builds and opens.
4. `--smoke` writes a result file.
5. `.Controls` extracted from the first page.
6. `.Modules` created; catalog expanded to the full page list with placeholders.
7. FND-06 and FND-07.
8. FND-05 and FND-04 polish.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-00-1 | `dotnet build src/Beep.OilandGas.Desktop.slnx -c Debug` → 0 errors | build log |
| SG-00-2 | No `BEEP0001..0004` or `CS8509` diagnostic | build log |
| SG-00-3 | `--smoke` writes `smoke-result.txt` containing `result: ok` and `views-constructed: True` | result file |
| SG-00-4 | `--drivers-report` lists at least one driver from the packaged driver folder | stdout capture |
| SG-00-5 | The shell opens, shows the sign-in path, and lists only pages allowed for the signed-in role | screenshot |
| SG-00-6 | Removing a role from the test principal removes exactly its pages and the guard refuses a direct route | test `FND-03/T05` |
| SG-00-7 | A forced failure in a page shows the failure panel with a reference and does not show empty data | test `FND-06/T03` |
| SG-00-8 | All new XAML uses only `DynamicResource` tokens (grep for `#` and `Color="` in new views → zero hits) | grep output |
| SG-00-9 | Switching to `ar-SA` mirrors the shell and the page frame | screenshot |
| SG-00-10 | `ConstructViews` instantiates every catalog page without throwing | test `STG-00/T09` |
| SG-00-11 | A thrown exception in a page produces a toast with a 12-hex reference and no exception text | test `FND-06/T04` |
| SG-00-12 | The same failure is present in the durable store and in the rolling log | test `FND-06/T06` |
| SG-00-13 | An uncaught exception from a background task reaches the same reporter | test `FND-06/T07` |
| SG-00-14 | `IOgToastService` is referenced only by `OgNotificationHub` | guard `G-3` |
| SG-00-15 | A writing test command produces exactly one audit row; a refusal produces one too | test `FND-06/T09` |
| SG-00-16 | The failure store and log are created under `%LOCALAPPDATA%\TheTechIdea\BeepOilGas` and nothing is written beside the executable | test `FND-01/T03`, guard `G-11` |

---

## 9. Test plan

| Test | What it proves |
|---|---|
| `FND-01/T01` | Static ctor sets `BEEP_OILGAS_DATA_ROOT` before any path is read |
| `FND-01/T02` | Each headless verb returns the documented exit code |
| `FND-01/T03` | `OgPaths` resolves every writable path under the per-user root and none under `AppContext.BaseDirectory` |
| `FND-02/T01` | `Auto` resolves Remote when configuration is complete |
| `FND-02/T02` | `Auto` resolves Local when configuration is absent |
| `FND-02/T03` | A read that fails in Remote mode is reported as failed, not empty |
| `FND-03/T01` | Every catalog route maps to exactly one view factory case |
| `FND-03/T02` | An unknown route produces no view |
| `FND-03/T05` | Role removal removes pages and the guard refuses |
| `FND-04/T01` | Every skin in `BeepControlStyle` applies without exception |
| `FND-05/T01` | Every user-visible string has a `Strings.resx` entry |
| `FND-06/T03` | A failed read shows the failure panel and a toast, not an empty grid |
| `FND-06/T04` | A thrown exception yields a toast carrying only the reference |
| `FND-06/T06` | The failure is durable in the store and in the log |
| `FND-06/T07` | An unobserved task exception reaches the reporter |
| `FND-06/T09` | One audit row per write and per refusal |
| `FND-06/T11` | A logger-only catch fails the guard test |
| `FND-07/T01` | Setup incomplete → shell cannot open |
| `FND-07/T02` | Wrong password → failure, no session |
| `FND-07/T03` | Sign-in failure does not leak exception text |
| `STG-00/T09` | Every catalog page constructs |

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `AddBeepOilandGasAppLocal` does not compose every service | `LocalServiceComposer` fallback in `.Data` (ADR-004) |
| 40 culture resx files are a large mechanical task | Scaffold with English values; translation is REL-05 |
| Full catalog declared early creates 130 placeholder views | A single generic `PlaceholderView` bound to the page definition, not 130 files |
| `BeepChrome` skin APIs differ from the ERP's usage | Copy `ErpSkins` verbatim and adapt; it compiles against the same library |
