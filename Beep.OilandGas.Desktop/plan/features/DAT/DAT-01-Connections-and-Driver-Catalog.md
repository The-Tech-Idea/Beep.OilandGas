# DAT-01 — Connections and driver catalog

| Field | Value |
|---|---|
| Stage | STG-01 — Data platform and PPDM core |
| Area | Data |
| Status | Gate met - AC-1 to AC-6, including distinct test outcomes and an advertising-driver local database. See [evidence](../../evidence/STG-01-DAT-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `src/Beep.OilandGas.Desktop.Data/Connections/`, `Modules/Data/Connections/` |
| External packages | `BeepDM` (`IDMEEditor`, `DatasourceManagementService`) |
| Depends on | FND-02 |
| Blocks | DAT-02 … DAT-08 |
| Web reference | `Pages/Admin/ModuleDatabases.razor`, `Pages/Data/DatabaseSetup.razor`, `Components/Connection/ConnectionSelector.razor`, `ConnectionTestDialog.razor`, `FirstLoginConnectionWizard.razor` |

## 1. Purpose

Let an operator see which database drivers the installation has, create and test a connection,
and make one connection active — the prerequisite for every other data feature.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/ConnectionService.cs` | connection CRUD, driver listing |
| `BeepDM` `DatasourceManagementService`, `IDMEEditor.Config_editor` | `GetAllDatasources`, driver catalog, save |
| ERP `ErpBootstrapper.RegisterPackagedDrivers` | load drivers from `ConnectionDrivers` next to the executable |
| ERP `ErpBootstrapper` NuGet source setup | `nuget.org` + local feed |
| `Beep.DoitAll.Erp.Wpf/Services/ErpLocalDatabaseProvisioner.cs` | create a local file database from a driver that advertises `CreateLocal` |
| `Web/Services/ConnectionService.cs` | connection shape used by the Web |

## 3. Screens

`Data → Connections`

- Toolbar: New, Test, Edit, Delete, Set active, Refresh drivers, Open driver folder.
- Grid: name, provider, category, file/host, state (never tested / ok / failed / missing file).
- Detail pane: provider, server, database, authentication, schema, options, driver assembly.
- `ConnectionTestDialog`: runs connect → open → simple query → close, with per-step result.
- Driver panel: package name, driver class, data source class, assembly, version, local/remote/in-memory.

## 4. Data and contracts

`OgConnectionCatalog`:

```csharp
Task<OgResult<IReadOnlyList<OgConnectionInfo>>> ListAsync(CancellationToken ct);
Task<OgResult<OgConnectionInfo>> TestAsync(OgConnectionDefinition def, CancellationToken ct);
Task<OgResult> SaveAsync(OgConnectionDefinition def, CancellationToken ct);
Task<OgResult> DeleteAsync(string name, CancellationToken ct);
Task<OgResult> SetActiveAsync(string name, CancellationToken ct);
Task<OgResult<IReadOnlyList<OgDriverInfo>>> ListDriversAsync(CancellationToken ct);
```

Password handling: stored through `Config_editor` (already protected) or DPAPI; never logged.

## 5. States

| State | Behaviour |
|---|---|
Never tested | neutral chip, "Test" is the primary action |
| Test failed | failed chip with the failing step and the provider message (not the stack) |
| File missing | failed chip, "Locate file" action |
| No drivers | empty state linking to the driver folder and the NuGet feed |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Create/edit connection | `data.connections.manage` | config |
| Test | any signed-in user | no |
| Delete | `data.connections.manage` | config |
| Set active | `data.connections.manage` | config + session |

## 7. Acceptance criteria

- AC-1 The driver list comes from loaded assemblies plus the config catalog; unloadable
  assemblies are skipped and reported.
- AC-2 A test distinguishes bad host, bad credentials, missing database and missing driver.
- AC-3 A local file database can be created from any driver advertising `CreateLocal`.
- AC-4 Deleting a connection that is in use is refused with the reason.
- AC-5 The active connection survives a restart.
- AC-6 Connection passwords never appear in logs, audit detail or the UI.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.IntegrationTests --filter "FullyQualifiedName~DAT-01"
```

Blind spot: SQLite is the only provider exercised in CI; SQL Server/PostgreSQL/Oracle are manual.

## 9. Open questions and risks

- Provider coverage differs per installation; the UI must not assume a fixed list.
- `Config_editor.SaveConfigValues()` rewrites the whole Beep config — batch changes to avoid
  repeated writes.
- **Delivered 2026-11-05:** `OgBeepHost` (AddBeepRuntime), `OgDriverDiscovery` (folder + loaded
  assembly), `BeepDmConnectionCatalog` (list / test / ensure-local / remove), `ConnectionsView`, and a
  lazy `OgBeepLifetime` so startup never pays for BeepDM.
- **AC-4 delivered:** removing a connection that is open is refused with `connection-in-use` before
  any configuration change.
- **Build gate.** `Data` builds BeepDM from source, so a clean rebuild shows the sibling projects'
  warnings. Our gate is zero warnings in `Beep.OilandGas.Desktop.*` paths.

## 10. AC-2 and AC-3 — delivered 2026-11-05

**AC-2 — one code per outcome.** `OgConnectionFailureClassifier` is a pure mapping from a provider's
failure to one of `DriverMissing`, `MissingDatabase`, `HostUnreachable`, `CredentialsRejected` or
`Failed`, each with a stable code:

| Outcome | Code |
|---|---|
| missing database file | `connection-database-missing` |
| no installed driver for the type | `connection-driver-missing` |
| host/DNS/refused/timeout | `connection-host-unreachable` |
| login, password, 401/403, permission | `connection-credentials-rejected` |
| anything else | `connection-failed` |
| no such connection | `connection-not-found` |

The two definite cases (file absent, driver absent) are decided from facts, not from wording. The rest is
best effort from the provider's own message — providers do not agree on codes — so the sentence is offered
as a hint and the failure reference is still recorded. Credentials are checked before reachability,
because "login failed for user 'x' on host 'y'" is a credential problem and calling it "unreachable" sends
the person to the wrong team.

**AC-3 — the driver must advertise local creation.** The old `FirstLocalDriver` used
`FirstOrDefault(d => d.CreateLocal)`. BeepDM's list contains local **in-memory** drivers (a cache, a memory
provider) and the drivers report now shows them side by side with SQLite:

```
driver: SqlLite | package=System.Data.SQLite | category=RDBMS | local=True | in-memory=False
driver: SqlLite | package=System.Data.SQLite | category=INMEMORY | local=False | in-memory=True
driver: SqlLite | package=TheTechIdea.Beep.Maui.DataSource.Sqlite | category=INMEMORY | local=True | in-memory=False
```

`OgDriverDiscovery.SelectLocalDriver` now requires `CreateLocal && !InMemory`, and there is **no fallback
to the first non-in-memory driver** any more: when no installed driver advertises local creation the
catalog refuses with `connection-driver-missing` and names the requirement. A successful provision reports
which driver was used (`… · driver: SqlLite (advertises local creation)`), so the choice is visible rather
than inferred.

**Still open (outside the acceptance criteria):** provider coverage differs per installation, so a new
provider's wording may land in `connection-failed` until its message is added to the classifier.
