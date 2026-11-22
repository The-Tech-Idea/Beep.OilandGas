# FND-03 — Shell window and navigation catalog

| Field | Value |
|---|---|
| Stage | STG-00 — Foundation and host shell |
| Area | Foundation |
| Status | Not started |
| Owner projects | none |
| Desktop project | `src/Beep.OilandGas.Desktop/Shell/`, `Modules/` |
| External packages | `TheTechIdea.Beep.WPF.Controls`, `CommunityToolkit.Mvvm` |
| Depends on | FND-02 |
| Blocks | every page |
| Web reference | `Beep.OilandGas.Web/Shared/NavMenu.razor`, `Services/PersonaContextService.cs` |

## 1. Purpose

Provide the single navigable surface of the product, with one declaration of every page, a
two-gate access check, and a view factory that cannot produce a page the catalog does not
contain.

## 2. Source components

| Source | Reuse |
|---|---|
| `Beep.DoitAll.Erp.Wpf/Modules/ErpModuleCatalog.cs` | `OgPageDefinition` record shape, `IsModuleEnabled` by company type |
| `Beep.DoitAll.Erp.Wpf/Services/ErpNavigationService.cs` | two-gate `CanAccess`, navigation build, icon path convention |
| `Beep.DoitAll.Erp.Wpf/Shell/ErpShellWindow.xaml.cs` | `CreateView` switch, selection restore, context switches, `RestoreSelection` |
| `Beep.DoitAll.Erp.Wpf/ViewModels/ErpShellViewModel.cs` | navigation collection, `ApplyPermissions`, `FindByRoute` |
| `Beep.OilandGas.Web/Shared/NavMenu.razor` | group names and the 20 functional groups |
| `Beep.OilandGas.Web/Services/PersonaContextService.cs` → `PersonaNavigation.HomeRoute` | "My workspace" entry |
| `Beep.OilandGas.UserManagement/Services/DefaultSecuritySeedService.cs` | 19 personas and their landing routes |

## 3. Screens

`OgShellWindow` layout:

```
┌────────────────────────────────────────────────────────────────────────┐
│ Title bar: product name · company ▾ · field ▾ · period ▾ · search · ★ · 👤 │
├───────────────┬────────────────────────────────────────────────────────┤
│ My workspace  │                                                        │
│ Dashboard     │  ┌ PageFrame ────────────────────────────────────────┐  │
│ ── Exploration│  │ title · context · status badge · command bar      │  │
│ ── Development│  ├───────────────────────────────────────────────────┤  │
│ ── Production │  │                                                   │  │
│ ── Reservoir  │  │               feature content                     │  │
│ ── Economics  │  │                                                   │  │
│ ── Accounting │  └───────────────────────────────────────────────────┘  │
│ ── HSE        │                                                        │
│ ── Decomm.    │                                                        │
│ ── Work       │                                                        │
│ ── Processes  │                                                        │
│ ── Data       │                                                        │
│ ── Admin      │                                                        │
├───────────────┴────────────────────────────────────────────────────────┤
│ Status: company · book · currency · period · mode · user · notifications │
└────────────────────────────────────────────────────────────────────────┘
```

Groups: *Overview, Exploration, Development, Production, Reservoir, Lift, Well test, Economics,
Accounting, HSE, Decommissioning, Work, Processes, Data, Administration, Training, Reporting.*

## 4. Data and contracts

```csharp
public static class OgModuleKeys { core, data, wells, exploration, lease, development,
    drilling, production, reservoir, properties, forecasting, lift, welltest, pipeline,
    economics, accounting, hse, permits, decommissioning, work, workflow, interop,
    training, reporting, administration }

public sealed record OgPageDefinition(string Route, string Title, string ModuleKey,
    IReadOnlyList<string> RequiredRoles, string? RequiredPermission = null,
    string Group = "", string Glyph = "", string IconPath = "");
```

Route conventions: `area` (`production`), `area/page` (`production/allocation`),
`analysis/x` for calculation screens, matching the Web routes so a user guide can be shared.

## 5. States

| State | Behaviour |
|---|---|
| No permitted pages | Shell shows an explanatory page, not an empty menu |
| Selected page removed by a role change | Fall back to the first permitted page, suppress re-entry |
| Route not in catalog | No view; audit `authz-denied` |
| Permission source unavailable | Role rule applies; a warning banner says grants could not be read |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Navigate | page gate |
| Switch company | membership in the company |
| Switch field/period | page gate plus context validity |
| Command palette (Ctrl+K) | page gate per result |

## 7. Acceptance criteria

- AC-1 Every catalog route maps to exactly one view factory case (guard test).
- AC-2 An unknown route produces no view.
- AC-3 Removing a role removes exactly that role's pages.
- AC-4 Direct navigation to a forbidden route is refused and audited.
- AC-5 Changing company/field cancels the in-flight load and clears the page.
- AC-6 The catalog declares every page in the master tracker, including not-yet-built ones.
- AC-7 Navigation icons resolve from `Assets/Icons/nav.<route>.svg`; a missing icon fails a guard test.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FND-03"
```

Manual: sign in as each of the 19 personas and confirm the landing route and menu differ.

## 9. Open questions and risks

- 130 catalog entries need 130 icons. Use a small generic set by group until a designer delivers.
- The Web has inconsistent route shapes (`/ppdm39/...`, `/analysis/...`, `/lifecycle/...`). The
  desktop should normalise them and record the mapping in each feature document.
