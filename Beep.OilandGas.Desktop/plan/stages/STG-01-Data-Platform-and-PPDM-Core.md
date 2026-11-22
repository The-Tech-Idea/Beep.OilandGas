# STG-01 — Data platform and PPDM core

| Field | Value |
|---|---|
| Stage | STG-01 |
| Features | 8 (DAT-01 … DAT-08) |
| Depends on | STG-00 |
| Status | Gate met - DAT-01..08 all met: the PPDM core platform, the unscoped 28-module apply (23.7 min), the table workbench, the quality review and the durable governance trail |
| Exit gate | A local PPDM database is created, all 29 modules are set up, and a table can be browsed and edited |

---

## 1. Objective

Make the desktop able to *hold* data. This stage creates the connection manager, the module
binding model, the module setup runner, the PPDM tree, a generic table workbench, the
metadata-driven entity form, the CSV import path, data quality and data governance screens.

Without this stage every other stage has nowhere to read from or write to.

---

## 2. Scope

**In**

- Driver discovery from the packaged `ConnectionDrivers` folder and the NuGet feed.
- Connection create / test / edit / delete for SQL Server, PostgreSQL, Oracle, SQLite, DuckDB.
- Module-database binding (which module writes to which connection) — mirrors
  `Beep.OilandGas.Web/Pages/Admin/ModuleDatabases.razor`.
- Module setup: run `IModuleSetup` in `Order`, show progress, resume and rollback.
- PPDM tree from `Beep.OilandGas.Branchs` + `PPDM39.Core.Tree`.
- Generic table workbench: paging, filter, sort, column chooser, inline edit, export.
- Entity form generated from `PPDMTableMetadata`.
- CSV import with mapping and preview.
- Data quality review, versioning list, access audit, sync dashboard.

**Out**

- Any domain-specific screen.
- Remote mode against a real `ApiService` (proven in FND-02 probe only).
- Business EF migrations (ADR-009).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.PPDM39.DataManagement` | `PPDM39SetupService`, `PPDM39WorkflowService`, `PPDMMappingService`, `PPDMDataValidationService`, `PPDMDataQualityService`, `PPDMDataVersioningService`, `PPDMAccessAuditService`, `ConnectionService`, `DemoDatabaseService`, `LOVManagementService`, `ModuleSetupDiscovery` |
| `Beep.OilandGas.PPDM39` | `IPPDMGenericRepository`, `PPDM39TableRegistry`, `IPPDMMetadataRepository`, `IPPDMTreeBuilder`, `PPDMDataSourceTransaction`, `IPPDMRowAccessPolicy` |
| `Beep.OilandGas.PPDM.Models` | Entity definitions for the workbench and form |
| `Beep.OilandGas.Branchs` | PPDM tree nodes |
| `Beep.OilandGas.DataManager` | `DataManager`, `ScriptValidator`, 46 `*ModuleData` scripts |
| `Beep.OilandGas.Repository` | Row-level access and audit context |
| `BeepDM` (`IDMEEditor`, `UnitofWork<T>`) | **Only inside `.Data`** |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| DAT-01 | Connections and driver catalog | [DAT-01](../features/DAT/DAT-01-Connections-and-Driver-Catalog.md) |
| DAT-02 | Module bindings and module setup | [DAT-02](../features/DAT/DAT-02-Module-Bindings-and-Module-Setup.md) |
| DAT-03 | PPDM tree browser | [DAT-03](../features/DAT/DAT-03-PPDM-Tree-Browser.md) |
| DAT-04 | PPDM table workbench | [DAT-04](../features/DAT/DAT-04-PPDM-Table-Workbench.md) |
| DAT-05 | Entity form and validation | [DAT-05](../features/DAT/DAT-05-Entity-Form-and-Validation.md) |
| DAT-06 | CSV import wizard | [DAT-06](../features/DAT/DAT-06-CSV-Import-Wizard.md) |
| DAT-07 | Data quality review | [DAT-07](../features/DAT/DAT-07-Data-Quality-Review.md) |
| DAT-08 | Versioning, access audit and sync | [DAT-08](../features/DAT/DAT-08-Versioning-Audit-and-Sync.md) |

---

## 5. New code introduced

| Project | Key types |
|---|---|
| `.Data` | `OgConnectionCatalog`, `OgDriverCatalog`, `OgConnectionTester`, `OgModuleBindingService`, `OgModuleSetupRunner`, `OgPpdmTreeBuilder`, `OgPpdmMetadataReader`, `OgPpdmTableGateway`, `OgLocalDatabaseProvisioner`, `OgSchemaValidationService`, `LocalServiceComposer` |
| `.Modules/Data` | View models and views for the eight features |
| `.Controls` | `OgFormBuilder`, `FilterBar`, `PickerField` (connection picker), `Timeline` (setup progress) |

---

## 6. Architecture notes

- **Only `.Data` holds BeepDM types.** `OgPpdmTableGateway` returns page results
  (`OgResult<OgPage<T>>`) with dictionaries or owning-project models — never `IDataReader`,
  `DataTable` or `UnitofWork<T>`.
- **Module setup is resumable.** `IExecutionStateStore` (file or database) records the last
  completed module so a failed run can resume, mirroring `ErpSchemaValidationService.ResumeAsync`.
- **Demo data is optional and additive.** `DemoDatabaseModule` (`DEMO_DATA`, order 100) is only
  run when the operator asks for it — never automatically.
- **Setup must be safe on an existing database.** `PPDM39SetupService` must report "already
  present" rather than recreating.
- **Row-level policy applies to every read** through `IPPDMRowAccessPolicy`; the workbench must
  show the applied policy so an empty result is not mistaken for no data.

---

## 7. Dependencies and sequencing

1. DAT-01 (needs a working driver catalog before anything else).
2. DAT-02 (needs a connection).
3. DAT-03 + DAT-04 (need a set-up database).
4. DAT-05 (needs metadata).
5. DAT-06, DAT-07, DAT-08 in parallel.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-01-1 | A local SQLite PPDM database is created from scratch and survives an app restart | test `DAT-02/T01` |
| SG-01-2 | All 29 module ids appear in setup output in `Order` sequence | setup log |
| SG-01-3 | Re-running setup on a complete database produces no destructive operation | setup log + diff |
| SG-01-4 | PPDM tree shows at least `WELL`, `WELLBORE`, `PDEN_VOL_SUMMARY`, `FACILITY`, `R_*` groups | screenshot |
| SG-01-5 | Workbench pages 10 000 rows without loading all of them | test `DAT-04/T03` |
| SG-01-6 | A row edited in the workbench is read back with the new value | test `DAT-04/T05` |
| SG-01-7 | Entity form refuses an invalid mandatory field before any write | test `DAT-05/T02` |
| SG-01-8 | CSV import of 1 000 rows reports accepted/rejected counts and writes only accepted rows | test `DAT-06/T02` |
| SG-01-9 | A failed read is shown as failed, never as zero rows | test `DAT-04/T07` |
| SG-01-10 | Sync dashboard distinguishes never-run from last-failed | screenshot + test |

---

## 9. Test plan

| Test | What it proves |
|---|---|
| `DAT-01/T01` | Driver catalog ignores unloadable assemblies |
| `DAT-01/T02` | Connection test distinguishes bad host, bad credentials and missing database |
| `DAT-02/T01` | Fresh setup creates the expected tables |
| `DAT-02/T02` | Resume after an injected failure completes the remaining modules |
| `DAT-03/T01` | Tree node count matches the registry count |
| `DAT-04/T03` | Paging issues a bounded query |
| `DAT-04/T05` | Round-trip edit |
| `DAT-04/T07` | Failure is not rendered as empty |
| `DAT-05/T02` | Mandatory violation refuses before write |
| `DAT-06/T02` | Import counts and partial rejection |
| `DAT-07/T01` | Quality finding carries table, key and rule |
| `DAT-08/T01` | Access audit records reader, table, filter and row count |

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `PPDM39SetupService` behaviour against an existing schema is undocumented | Probe against a copied production-shaped database before SG-01-3 |
| 2 740 entity classes make metadata reflection slow on first use | Cache metadata per connection; measure in REL-04 |
| Multiple providers (SQL Server / PostgreSQL / Oracle / SQLite) behave differently on paging | `IPPDMGenericRepository` presumably abstracts this — confirm and record |
| Demo data could contaminate a real database | `DEMO_DATA` only on explicit request; gate in the wizard |
