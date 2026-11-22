# FND-02 — Service composition and access modes

| Field | Value |
|---|---|
| Stage | STG-00 — Foundation and host shell |
| Area | Foundation |
| Status | Gate met - resolution, audited switch, health states, the IDMEEditor guard and a business read that fails as a failure. See [evidence](../../evidence/STG-00-FND-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Client`, plus every module the local composer wires |
| Desktop project | `src/Beep.OilandGas.Desktop.Data/` + host `Hosting/` |
| External packages | `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Http` |
| Depends on | FND-01 |
| Blocks | every data feature |
| Web reference | n/a — Web composes typed clients; desktop composes the façade |

## 1. Purpose

Give the desktop one way to reach business services that works offline and online, so no view
model ever knows which mode it is in and no view model ever opens a connection.

## 2. Source components

| Source | Reuse |
|---|---|
| `Beep.OilandGas.Client/DependencyInjection/ClientServiceCollectionExtensions.cs` | `AddBeepOilandGasApp`, `AddBeepOilandGasAppLocal`, `AddBeepOilandGasAppAuto`, `AddBeepOilandGasAppRemote` |
| `Beep.OilandGas.Client/App/IBeepOilandGasApp.cs` | the façade and its 15 subsystem properties |
| `Beep.OilandGas.Client/App/ServiceAccessMode.cs` | `Remote` / `Local` / `Auto` |
| `Beep.OilandGas.Client/App/ClientApiTransport.cs` | remote transport |
| `Beep.OilandGas.Client/Authentication/IAuthenticationProvider.cs` | implemented by `OgExternalIdentityService` |
| `Beep.OilandGas.ApiService/Program.cs` (lines ~1470–1830) | the canonical list of calculation-service registrations, used to build `LocalServiceComposer` if the packaged local extension is incomplete |
| `Beep.OilandGas.LifeCycle/DependencyInjection/LifeCycleServiceCollectionExtensions.cs` | `AddLifeCycleServices` |
| `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupDiscovery.cs` | `AddDiscoveredModuleSetups` |
| `Beep.OilandGas.UserManagement/DependencyInjection/UserManagementServiceCollectionExtensions.cs` | `AddUserManagement` |
| `Beep.OilandGas.Repository/RepositoryServiceRegistration.cs` | `AddOilGasRepository` |

## 3. Screens

No page. The shell shows an access-mode indicator (chip in the status bar) and a
`ConnectionHealthWindow` from the indicator's menu.

## 4. Data and contracts

```csharp
public interface IOgServiceAccess
{
    ServiceAccessMode Mode { get; }
    Task<OgResult> ConfigureAsync(OgServiceAccessSettings settings, CancellationToken ct);
    Task<OgResult<OgHealthSnapshot>> CheckHealthAsync(CancellationToken ct);
}
```

Resolution order in `Auto`:

1. `CompanyApiBaseUrl` and a usable token → `Remote`.
2. Registered local connection and a present database file → `Local`.
3. Otherwise → not configured; the Data screens are reachable but business pages refuse.

## 5. States

| State | Behaviour |
|---|---|
| Not configured | Business pages show "not configured" with a link to DAT-01 |
| Remote unreachable | Failed state; never empty; retry offered |
| Token expired | Sign-in prompt; no silent retry loop |
| Local database missing | Repair through `FirstRunWindow` |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Switch mode | Administration only; recorded in the audit log |
| Re-check health | any signed-in user |

## 7. Acceptance criteria

- AC-1 `Auto` chooses `Remote` only when both base URL and token are usable.
- AC-2 `Auto` chooses `Local` when no API is configured and a local connection exists.
- AC-3 A failed remote read produces a failed state, not an empty result.
- AC-4 No type outside `.Data` references `IDMEEditor` (guard test).
- AC-5 Mode changes are audited with actor and previous/new mode.
- AC-6 Health distinguishes unreachable, unauthorised and database-missing.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FND-02"
```

Plus a manual probe: point `CompanyApiBaseUrl` at a stub returning one well; confirm the same
DTO appears in both modes.

## 9. Open questions and risks

- **Highest-risk feature in the programme.** If `AddBeepOilandGasAppLocal` does not register every
  service the desktop needs, `LocalServiceComposer` must mirror `ApiService/Program.cs`, which is
  ~360 lines of registrations.
- `IAuthenticationProvider` must be supplied or Client throws. Covered by FND-07.
- Some services may only exist as concrete classes (e.g. `FlashCalculationService`,
  `GasLiftService`, `PlungerLiftService`). The composer wraps them behind desktop interfaces.

### Delivered 2026-11-05

- `OgAccessResolver` is the one rule: `Remote` only when the base URL **and** a usable token are present,
  `Local` only when a connection and its database file are present, and `Auto` tries API then local then
  refuses. An explicit `Local`/`Remote` is honoured and does not silently fall back.
- `OgServiceAccess` re-resolves on every read of `Mode`, audits each change as one `access.switch` row
  carrying the previous mode and the preference, and never holds the token itself — only whether one
  exists.
- `CheckHealthAsync` answers the three states a person can act on: `Unreachable` (no answer),
  `Unauthorized` (401/403) and `DatabaseMissing`. An unreachable host is a *successful check with a bad
  state*, not a failed check, and it is still reported so the evidence exists.
- The shell status chip now reads `Mode: … · access: … · <user>`.
- `OgPaths` owns the local database path, and `ConnectionsView` uses it instead of composing its own.
- Guard `No_type_outside_the_data_project_references_IDMEEditor` keeps AC-4 true.

### Still open

- **AC-3 delivered** (2026-11-06): `OgRemoteReader` performs the first business read against the company API
  (`api/wells/comparison-fields`) and returns an **outcome**, never an empty success: `NotConfigured` (nothing
  sent), `Unauthorized` (401/403, sign in — do not retry), `ServerError` (5xx), `Unreachable` (no answer,
  reported by reference), `InvalidPayload` (2xx with a missing or unreadable body), and `Ok` (a list,
  including a legitimately empty one). The health window has a **Test business read** button that shows the
  outcome and writes an `api.read` audit row, so "why is the grid empty?" has an answer.
- The `ConnectionHealthWindow` behind the status chip is built: it shows the mode, the reason and the
  health state, with a **Check again** action, and is reachable from the shell status bar.
- The API base URL and token are not yet read from configuration or from FND-07; the registration passes
  `null`/`false`, so a fresh installation truthfully resolves to `Local` or `NotConfigured`.
- The plan's §8 manual probe (same DTO from a stub API and from local) cannot run until an ApiService
  business endpoint exists.
