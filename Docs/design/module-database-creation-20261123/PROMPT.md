# Module database creation visual target

Status: proposed visual target; implementation and interaction verification pending.

Generated visual evidence: [four wizard states](mockup.png), Image Gen output
`exec-eef24c8d-11de-4e79-a5cb-e3a4d6b760b0.png`. The generated image was shown in
the conversation before implementation. Use the real LocalDB server spelling
with one backslash; the mockup's duplicated slash is an image text artifact.

## Brief

An OilGas repository Administrator prepares a separate database connection before
binding tenant modules. Desktop-first MudBlazor dialog, responsive to narrow
screens, existing dark charcoal surfaces and purple primary accent. Keep the
app's theme owner, English language support, keyboard interaction and visible
labels. No people, faces, avatars, or human pictograms. Existing database use
must remain available for supported providers. New catalog creation is offered
only for the current BeepDM integrated-security SQL Server LocalDB capability.

## Observed gap

ConnectionSetupDialog.TestConnection must succeed before SaveConnection.
PPDM39SetupService.CreateMigrationManager opens the configured database before
planning entity tables. Neither path creates a missing SQL Server database.
LocalDbDriverTests provisions catalogs separately through
MigrationManager.BindDatabaseTarget and SQLServerDatabaseTargetMigrationProvider;
that existing framework owner is the implementation route, not app SQL or EF.

## Research read 2026-11-23

- [SQL Server Management Studio database creation](https://learn.microsoft.com/en-us/sql/relational-databases/databases/create-a-database?view=sql-server-ver17):
  server connection precedes database naming and an explicit create action.
  Adopt distinct server/target review; avoid mixing module table creation into it.
- [pgAdmin database dialog](https://www.pgadmin.org/docs/pgadmin4/latest/database_dialog.html):
  database identity, ownership and settings are visible before saving. Adapt a
  concise target summary; avoid exposing unsupported provider configuration.
- MudStepper online page did not render in the web reader. Its installed/current
  MudBlazor source and the user-supplied local documentation remain authoritative
  for component APIs. Do not infer implementation APIs from the mockup.
- UI/UX Pro Max local query `wizard review confirmation recovery`, domain `ux`,
  returned feedback/error recovery and confirmation guidance. Adopt explicit
  progress, outcome and recovery wording; uncertain creation is never auto-retried.

## Image Gen prompt

Create a high fidelity four-panel UI storyboard, a 2 by 2 grid of four desktop
dialog screenshots for Beep Oil & Gas. Each panel is a different state of the
same "Add database connection" MudBlazor wizard, not four unrelated cards. Dark
charcoal background and dialog surface, purple selected step circles and purple
primary buttons, subtle borders, white and muted gray text, restrained orange
warning accent. Match the existing flat enterprise MudBlazor theme. A faint
purple app bar is behind each dialog. No people, faces, avatars, human icons,
decorative illustrations, gradients, invented analytics, or fake record lists.
Use database/storage icons only if needed. Readable, carefully spaced typography.
Four numbered horizontal step labels: "Target", "Connection", "Review", "Save".

Panel 1, Target active: heading "Choose a database target". Database type select
shows "SQL Server". Two radio options "Existing database" and selected "New
LocalDB database". Short helper "Create an empty LocalDB database, then choose
modules separately." Bottom: Cancel and Next. No data-changing action here.

Panel 2, Connection active: heading "Connection details". Two-column form with
visible labels Connection name (ModuleRepo_Dev), Host / server
((localdb)\\MSSQLLocalDB), Database name (BeepOilGasModules_Dev), authentication
"Windows integrated". Helper "LocalDB uses a named instance; no TCP port is
required." No password displayed or requested. Bottom: Back and Review target.

Panel 3, Review active: heading "Review database creation". Flat target summary
Server (localdb)\\MSSQLLocalDB, Database BeepOilGasModules_Dev, Connection
ModuleRepo_Dev, Status "Database does not exist". Info note "Creates an empty
module database. User accounts and roles stay in the default repository."
Unchecked confirmation "Create this LocalDB database". Back and a clearly
labelled "Create database" action. No SQL editor or migration code shown.

Panel 4, Save active: heading "Connection ready". Success line "Database created.
Connection test passed." Same compact server/database/connection summary. Note
"Saving this connection does not create module tables. Review a module migration
plan after choosing a tenant and binding." Back and Save connection primary;
secondary link "Module database setup". Keep all four panels visually consistent.

## Implementation sequence and acceptance

1. Extend the existing setup contract/controller/service with read-only target
   review and explicit native target creation, using the BeepDM target owner.
2. Bind creation to the exact reviewed provider/server/catalog, authenticated
   Administrator, durable framework ownership receipt and canonical target.
   Refuse protected/default repositories, unsupported capability, changed intent,
   existing unowned databases and uncertain ownership. No automatic drop/adoption.
3. Extend ConnectionSetupDialog in place. Keep existing-database testing; in new
   LocalDB mode create before testing and saving. Invalidate review/test results
   when inputs change. Keep uncertain outcomes visible and require reconciliation.
4. Keep module binding, table migration, approval hashes, execution and seeding in
   the existing ModuleDatabases / ModuleRepositoryController flow.
5. Focused tests: missing target creation, existing target refusal, changed review,
   missing authority, receipt/restart outcomes, named instance port handling,
   connection persistence and two separate module targets. LocalDB only; retain
   created test databases. Demonstrate new guards fail with the defect restored.
6. Compare implemented screenshots against all four mockup states; verify keyboard
   navigation, narrow layout and error recovery. Mockup is not acceptance evidence.

## Observed implementation - 2026-11-23

## Driver prerequisite extension - 2026-11-23

Requested: database selection must expose driver download inside the wizard
when its Beep connector or database provider is missing. Keep the existing
SetupDrivers installer as the sole UI/installation owner, filtered to the
selected canonical Beep datasource type. Insert a Driver step before Connection.
Loading/unavailable/missing/ready states are distinct; Continue, target creation,
connection testing and saving remain blocked until readiness is confirmed.
Installing uses only the existing authenticated API, server catalog, exact
versions and configured feeds. An uncertain response requires a catalog refresh,
not an automatic retry. Changing database type invalidates readiness and reviews.
No arbitrary packages, feeds, database writes or added role grants are involved.

Image Gen prompt (generate and show before UI implementation):
Create a polished, realistic MudBlazor desktop dialog storyboard for the existing
Beep Oil & Gas app, no human imagery, avatars or person icons. Preserve charcoal
surfaces, pale text, restrained purple accents, green ready and amber warning
status. Two side-by-side panels of the SAME five-step wizard: Target, Driver,
Connection, Review, Save. Title Module database connection. Driver is active.
LEFT missing PostgreSQL driver: heading Database driver; amber notice Driver
required for PostgreSQL; two plain rows Beep connector - Not loaded, Database
provider - Not loaded; package/version described as Server-configured package
and version, no invented package IDs. Explain Download and load on the API server
from configured feeds. Compact download icon with Download and load tooltip and
accessible-label callout; compact refresh icon; Continue disabled. RIGHT ready:
green Driver ready for PostgreSQL; both connector/provider Loaded; Continue
enabled. In a small inset show failed/unconfirmed download with Refresh driver
status before trying again. Preserve Back/Continue/Close structure, grouped
status text, no nested cards, no new palette, no generic dashboard decoration.
State explicitly this is a design target, not evidence of an actual PostgreSQL
connection or package installation. Save output as driver-prerequisite-mockup.png.

Research read 2026-11-23: DataGrip JDBC drivers
https://www.jetbrains.com/help/datagrip/jdbc-drivers.html and DBeaver Driver Manager
https://dbeaver.com/docs/dbeaver/Driver-Manager/ inform the contextual missing-driver
action; adapt to BeepDM's configured NuGet owner, do not introduce JDBC or arbitrary
downloads. Local MudBlazor 9.7 Stepper reference governs navigation. UI Pro Max
query `wizard prerequisite recovery progress --domain ux` supports visible progress
and explicit refresh/recovery. Existing theme and English/RTL infrastructure stay.

Acceptance: rendered dialog tests missing/ready/type-change/catalog failure and
uncertain installation; existing installer exact-response checks stay covered.
Build Web and run only focused driver/dialog tests; prove new guard red/restored.
Live browser check loaded SQL Server and a missing/unconfigured driver without
connecting to PostgreSQL/Oracle or modifying installed drivers to fake absence.
Record actual installation separately if a configured missing package is available.

Driver extension verification: implemented five-step ready/missing screens are
retained as `driver-step-ready.jpg` and `driver-step-missing-final.jpg`. The actual
missing provider was MySql.Data 9.5.0; the connector was loaded, download enabled,
and continuation blocked. SQL Server readiness allowed connection details.
`driver-step-mobile.jpg` records the final 390px layout, with package identifiers
wrapping inside the panel. The mockup's PostgreSQL wording was illustrative;
the real PostgreSQL driver was already loaded. No new provider was installed or
connected during browser verification. See INSTALLATION-ACCEPTANCE.md for test
commands, guard mutation evidence and limitations.

Before the driver prerequisite extension, the dialog followed the four target
states. Those earlier desktop captures are retained:
`implemented-target.jpg`, `implemented-connection.jpg`, `implemented-review.jpg`,
`implemented-saved.jpg`. Phone-width capture: `implemented-mobile-connection.jpg`.
The running app's charcoal/purple palette and Mud controls are preserved. This
is a workflow/layout match rather than a pixel copy of generated artwork. The
LocalDB server contains one backslash. No theme switch or custom CSS was added.

Actual Web/API created `OilGasModulesDev` through BeepDM, confirmed ownership,
tested and saved the named connection; reopening confirmed ownership. No module
tables, tenant membership or global-current selection were added. The empty
database and canonical Beep ownership receipt are retained. Module table plans
still belong to the existing tenant-scoped module binding/approval workflow.
Focused tests and limitations are owned by INSTALLATION-ACCEPTANCE.md.
