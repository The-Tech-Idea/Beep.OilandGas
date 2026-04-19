# Phase 0 — Database Setup, Seeding & Dummy Data

> **Status**: 🔲 Not Started  
> **Depends on**: Nothing (must complete before Phase 1)  
> **Duration**: ~1 week  
> **Goal**: Every new user is guided through database connection → reference data seeding → optional dummy data before they reach any business process page.

---

## Overview

Phase 0 is the **app bootstrap sequence**. It runs automatically on first launch and can be re-entered from the Data Management menu at any time. It has three steps in sequence:

```
[0.1] First-Run Wizard        → connect to an existing DB or create a new SQLite/LocalDB one
         ↓
[0.2] Reference Data Seeding  → seed R_*/RA_* well-status facets + enum reference tables
         ↓
[0.3] Dummy Data Generator    → optionally fill the DB with realistic PPDM39 demo data
         ↓
      → App Dashboard (Phase 1+)
```

First-run detection: checked via `Blazored.LocalStorage` key `ppdm39:setup:completed`.  
If the key is present and `true`, the wizard is skipped. It can be re-entered via `/ppdm39/setup`.

---

## Phase 0.1 — First-Run Database Setup Wizard

### Files

| File | Purpose |
|------|---------|
| `Pages/PPDM39/Setup/DatabaseSetupWizard.razor` | Full-page wizard (route `/ppdm39/setup`) |
| `Services/FirstRunService.cs` | Reads/writes `ppdm39:setup:*` LocalStorage keys; checks if setup is needed |
| `Components/Layout/DefaultLayout.razor` | *(modify)* Redirect to `/ppdm39/setup` when `FirstRunService.IsSetupRequired()` |

### Wizard Steps

**Step 1 — Connect to Your Database** *(primary path)*

App logo, "Welcome to Beep Oil & Gas" heading, then immediately the connection form.  
No choice cards — the user is expected to connect first. SQLite is offered as a fallback link below the form.

Connection form fields:
- Database Type (`MudSelect`): SQL Server · PostgreSQL · MySQL · Oracle · SQLite
- Connection Name (`MudTextField`, default `PPDM39`)
- Host (`MudTextField`, hidden when SQLite selected)
- Database Name (`MudTextField`, hidden when SQLite selected)
- File Path (`MudTextField`, shown only when SQLite selected)
- Username (`MudTextField`, hidden when SQLite selected)
- Password (`MudTextField` `InputType.Password`, hidden when SQLite selected)
- Port (`MudTextField`, pre-filled per type, hidden when SQLite selected)

Actions:
- `Test Connection` button → `POST /api/ppdm39/setup/test-connection`  
  Success: inline green badge "Connected ✓", Next step enabled.  
  Failure: inline `MudAlert` Severity.Error with server message.
- Below the form, a secondary text link: **"I don't have a database yet — create a local SQLite one →"**  
  Clicking it expands Step 1B inline (does not navigate away).

**Step 1B — Create a Local SQLite Database** *(secondary / fallback — shown inline when link clicked)*

Collapses the connection form and shows:
- Database file name (`MudTextField`, default `BeepOilandGas.db`)
- Save location (`MudSelect`): App Data Folder · Custom Path
- Custom path (`MudTextField`, visible only when Custom Path selected)
- `Create SQLite Database` button → `POST /api/ppdm39/setup/create-sqlite`  
  Show `MudProgressLinear` during creation.  
  Success: green alert with file path; connection auto-registered as `PPDM39`; auto-fills Step 1 form and runs Test Connection.  
  Failure: red alert with error details.
- `← Back to connection form` link to return to Step 1.

**Step 3 — Verify Schema**

- List of required PPDM39 core tables (WELL, FIELD, FACILITY, R_WELL_STATUS, etc.)
- Table exists: green chip. Missing: amber chip with "Create" individual button.
- `Run Schema Check` button → calls `GET /api/ppdm39/setup/schema-check`.
- `Create All Missing Tables` button → calls `POST /api/ppdm39/setup/create-schema`.
- Show `MudProgressLinear` during schema creation.
- Show per-table result in `MudExpansionPanel`.
- Next enabled when all core tables exist.

**Step 4 — Summary**

- Green card: connection name, DB type, table count.
- Two action buttons:
  - `Proceed to Seed Reference Data →` → navigates to `/ppdm39/setup/seed`
  - `Skip Seeding (Advanced)` → marks setup complete, navigates to `/dashboard`
- Writes `ppdm39:setup:completed = true` to LocalStorage.
- Writes `ppdm39:setup:connectionName = <name>` to LocalStorage.

### First-Run Detection in DefaultLayout

```
OnInitializedAsync → await FirstRunService.IsSetupRequired()
  true  → NavigationManager.NavigateTo("/ppdm39/setup")
  false → render normal layout
```

`FirstRunService.IsSetupRequired()`:
1. Check LocalStorage key `ppdm39:setup:completed` — if `true`, return false (not required).
2. Call `GET /api/ppdm39/setup/status` — if no connection registered, return true.
3. Otherwise return false.

### API Endpoints Required (ApiService)

These go in `PPDM39SetupController` or a new `PPDM39SetupSchemaController`:

| Method | Route | Purpose |
|--------|-------|---------|
| POST | `/api/ppdm39/setup/test-connection` | Test connection string; returns `{ success, message, tableCount }` |
| POST | `/api/ppdm39/setup/create-sqlite` | Create SQLite file + register connection |
| GET | `/api/ppdm39/setup/schema-check` | Returns list of core tables + exists flag per table |
| POST | `/api/ppdm39/setup/create-schema` | Runs CREATE TABLE scripts for missing tables |
| GET | `/api/ppdm39/setup/status` | Returns `{ hasConnection, connectionName, dbType, isSchemaReady }` |

### Todo

| ID | Task | File | Status |
|----|------|------|--------|
| 0.1.a | Create `FirstRunService.cs` | `Services/FirstRunService.cs` | 🔲 |
| 0.1.b | Inject `FirstRunService` in DI (Program.cs Web) | `Program.cs` | 🔲 |
| 0.1.c | Add first-run redirect to `DefaultLayout.razor` | `Components/Layout/DefaultLayout.razor` | 🔲 |
| 0.1.d | Create `DatabaseSetupWizard.razor` — Step 1 DB connection form (fields per DB type) | `Pages/PPDM39/Setup/DatabaseSetupWizard.razor` | 🔲 |
| 0.1.e | Add Step 1B inline SQLite creation (collapsible, triggered by fallback link) | same file | 🔲 |
| 0.1.f | Add Step 2 — Schema check + create missing tables | same file | 🔲 |
| 0.1.g | Add Step 3 — Summary + navigate to seeding | same file | 🔲 |
| 0.1.h | Add `POST /api/ppdm39/setup/test-connection` endpoint | `Controllers/PPDM39/PPDM39SetupController.cs` | 🔲 |
| 0.1.i | Add `POST /api/ppdm39/setup/create-sqlite` endpoint | same | 🔲 |
| 0.1.j | Add `GET /api/ppdm39/setup/schema-check` endpoint | same | 🔲 |
| 0.1.k | Add `POST /api/ppdm39/setup/create-schema` endpoint | same | 🔲 |
| 0.1.l | Add `GET /api/ppdm39/setup/status` endpoint | same | 🔲 |
| 0.1.m | Add `/ppdm39/setup` nav link in Data Management nav section | `Shared/NavMenu.razor` | 🔲 |

---

## Phase 0.2 — Reference Data Seeding Pages

> **Existing base**: `Pages/PPDM39/SeedReferenceData.razor` already exists.  
> This phase integrates it into the first-run flow and adds a dedicated setup route.

### Files

| File | Purpose |
|------|---------|
| `Pages/PPDM39/Setup/SeedSetupPage.razor` | New route `/ppdm39/setup/seed` — full-page seeding flow for the setup sequence |
| `Pages/PPDM39/SeedReferenceData.razor` | *(existing)* Standalone seeding page at `/ppdm39/seed-reference-data` — keep as-is for re-seeding |

### SeedSetupPage Layout

Three seeding categories shown as expandable cards, each with its own "Seed" button and status:

**Card 1 — Well Status Facets (WSC v3)**
- Seeds: `R_WELL_STATUS_TYPE`, `R_WELL_STATUS`, `R_WELL_STATUS_QUAL`, `R_WELL_STATUS_QUAL_VALUE`, `RA_WELL_STATUS_TYPE`, `RA_WELL_STATUS`
- Row counts from `GET /api/ppdm39/setup/seed/well-status-facets/status`
- Button: `Seed WSC v3 Facets` → `POST /api/ppdm39/setup/seed/well-status-facets`
- Badge: ✓ Seeded (green) / ⚠ Empty (amber)

**Card 2 — Enum Reference Data**
- Seeds: all enum-backed R_* tables (well type, fluid type, unit type, etc.)
- Button: `Seed Enum Data` → `POST /api/ppdm39/setup/seed/enum-reference-data`

**Card 3 — Field & Facility Reference Data**
- Seeds: `RA_FIELD_STATUS`, `RA_FACILITY_STATUS`, `RA_WELL_STATUS` (already handled above)
- Button: `Seed Field/Facility Data` → seeding endpoint (TBD based on seeder availability)

**Footer Actions**
- `Seed All` button → `POST /api/ppdm39/setup/seed/all-reference-data`
- `Continue to Dummy Data →` — only enabled when all three cards are ✓ Seeded
- `Skip →` — marks seeding complete in LocalStorage, goes to dashboard

### Progress Tracking

- Each card shows `MudProgressLinear` during its own seeding operation.
- On completion: `MudExpansionPanel` opens showing per-table row counts.
- `MudSnackbar` toast for success / error.

### Todo

| ID | Task | File | Status |
|----|------|------|--------|
| 0.2.a | Create `SeedSetupPage.razor` at route `/ppdm39/setup/seed` | `Pages/PPDM39/Setup/SeedSetupPage.razor` | 🔲 |
| 0.2.b | Add 3 seeding cards with individual seed buttons | same | 🔲 |
| 0.2.c | Add "Seed All" + progress tracking | same | 🔲 |
| 0.2.d | Add "Continue to Dummy Data" navigation | same | 🔲 |
| 0.2.e | Wire `SeedSetupPage` into wizard Step 4 navigation | `Pages/PPDM39/Setup/DatabaseSetupWizard.razor` | 🔲 |
| 0.2.f | Add `/ppdm39/setup/seed` nav link alongside existing seed page | `Shared/NavMenu.razor` | 🔲 |

---

## Phase 0.3 — Dummy Data Generator

> **Goal**: One C# class that creates a complete, consistent PPDM39 dataset for demo/testing purposes.  
> The generated data uses **only values that exist in the seeded reference tables** — no hardcoded codes.

### Architecture

```
PPDM39DummyDataGenerator
│
├── GenerateAllAsync(connectionName, userId, options)  ← main entry point
│       ↓ in dependency order:
├── GenerateFieldsAsync           → FIELD (2 fields)
├── GeneratePoolsAsync            → POOL (1–2 per field)
├── GenerateWellsAsync            → WELL + WELL_STATUS facets (5 wells per field)
├── GenerateWellTestsAsync        → WELL_TEST (2 per well)
├── GenerateFacilitiesAsync       → FACILITY (1 per field)
├── GenerateProductionAsync       → PDEN_VOL_SUMMARY (12 months per well)
└── GenerateActivitiesAsync       → WELL_ACTIVITY (1 per well)
```

**Reference Data Used** (read from DB at generation time, never hardcoded):

| Reference Table | Used For |
|----------------|---------|
| `R_WELL_STATUS` (STATUS_TYPE = 'Well Status') | Well `WELL_STATUS.STATUS` values |
| `R_WELL_STATUS` (STATUS_TYPE = 'Life Cycle') | Well life-cycle facet |
| `R_WELL_STATUS` (STATUS_TYPE = 'Product Type') | Well fluid type facet |
| `R_WELL_STATUS` (STATUS_TYPE = 'Trajectory Type') | Wellbore trajectory facet |
| `RA_FIELD_STATUS` | Field status alias values |
| `RA_FACILITY_STATUS` | Facility status alias values |
| Any enum R_* table | Typed reference lookups |

**`DummyDataGeneratorOptions`** (controls what gets generated):

```csharp
public class DummyDataGeneratorOptions
{
    public int FieldCount            { get; set; } = 2;
    public int WellsPerField         { get; set; } = 5;
    public int MonthsOfProduction    { get; set; } = 12;
    public bool IncludeWellTests     { get; set; } = true;
    public bool IncludeFacilities    { get; set; } = true;
    public bool IncludeActivities    { get; set; } = true;
    public bool IncludeProduction    { get; set; } = true;
    public bool ClearExistingData    { get; set; } = false;   // destructive — default OFF
}
```

**`DummyDataGenerationResult`**:

```csharp
public class DummyDataGenerationResult
{
    public bool              Success           { get; set; }
    public string            Message           { get; set; } = string.Empty;
    public int               FieldsCreated     { get; set; }
    public int               WellsCreated      { get; set; }
    public int               FacilitiesCreated { get; set; }
    public int               ProductionRows    { get; set; }
    public int               WellTestRows      { get; set; }
    public int               ActivityRows      { get; set; }
    public int               TotalRows         => FieldsCreated + WellsCreated + FacilitiesCreated
                                               + ProductionRows + WellTestRows + ActivityRows;
    public List<string>      Details           { get; set; } = new();
    public List<string>      Errors            { get; set; } = new();
}
```

### File Layout

```
Beep.OilandGas.PPDM39.DataManagement/SeedData/
├── DummyData/
│   ├── PPDM39DummyDataGenerator.cs         ← main generator class
│   ├── PPDM39DummyDataGenerator.Fields.cs  ← partial: FIELD + POOL
│   ├── PPDM39DummyDataGenerator.Wells.cs   ← partial: WELL + WELL_STATUS facets
│   ├── PPDM39DummyDataGenerator.Tests.cs   ← partial: WELL_TEST
│   ├── PPDM39DummyDataGenerator.Facilities.cs ← partial: FACILITY
│   ├── PPDM39DummyDataGenerator.Production.cs ← partial: PDEN_VOL_SUMMARY
│   ├── PPDM39DummyDataGenerator.Activities.cs ← partial: WELL_ACTIVITY
│   └── DummyDataGeneratorOptions.cs        ← options + result DTOs
```

Namespace: `Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData`

### Constructor

```csharp
public PPDM39DummyDataGenerator(
    IDMEEditor editor,
    ICommonColumnHandler commonColumnHandler,
    IPPDM39DefaultsRepository defaults,
    IPPDMMetadataRepository metadata,
    WellServices wellServices,
    string connectionName = "PPDM39",
    ILogger<PPDM39DummyDataGenerator>? logger = null)
```

### Generation Logic Rules

1. **IDs are generated deterministically** using a `DummyDataPrefix` constant (`"DEMO_"`) so they can be idempotently re-created or cleaned up.
2. **All STATUS values are read from seeded reference tables** using `PPDMGenericRepository.GetAsync` — never hardcoded. If a reference table is empty, the generator logs a warning and uses a safe default.
3. **All wells use `WellServices.CreateAsync(..., initializeDefaultStatuses: true)`** — PPDM 3.9 facet seeding is automatic.
4. **PPDM column conventions**: `ACTIVE_IND = "Y"`, `ROW_CREATED_BY = userId`, common columns set by `ICommonColumnHandler`.
5. **`ClearExistingData = false` by default** — never destructive unless explicitly set.

### API Endpoint

Add to `PPDM39SetupController`:

| Method | Route | Purpose |
|--------|-------|---------|
| POST | `/api/ppdm39/setup/generate-dummy-data` | Runs `PPDM39DummyDataGenerator.GenerateAllAsync`; body = `DummyDataGeneratorOptions` |
| GET | `/api/ppdm39/setup/dummy-data/status` | Returns row counts for FIELD, WELL, FACILITY, PDEN_VOL_SUMMARY — shows if dummy data exists |
| DELETE | `/api/ppdm39/setup/dummy-data` | Removes all rows with prefix `DEMO_` (soft-deletes) |

### UI — Dummy Data Page

**Route**: `/ppdm39/setup/dummy-data`  
**File**: `Pages/PPDM39/Setup/DummyDataPage.razor`

Layout:
- Status section: current counts of FIELD / WELL / FACILITY / PRODUCTION rows (from status endpoint)
- Options form: `DummyDataGeneratorOptions` fields (field count, wells per field, toggles)
- `Generate Dummy Data` button → POST, show `MudProgressLinear`
- `MudExpansionPanel` for per-entity result counts after generation
- Warning card if `ClearExistingData` is enabled (destructive action)
- `Continue to Dashboard →` button → navigates to `/dashboard`, writes `ppdm39:setup:completed = true`

### Todo

| ID | Task | File | Status |
|----|------|------|--------|
| 0.3.a | Create `DummyDataGeneratorOptions.cs` (options + result DTOs) | `SeedData/DummyData/DummyDataGeneratorOptions.cs` | 🔲 |
| 0.3.b | Create `PPDM39DummyDataGenerator.cs` — constructor + `GenerateAllAsync` + reference-data loader | `SeedData/DummyData/PPDM39DummyDataGenerator.cs` | 🔲 |
| 0.3.c | Create `PPDM39DummyDataGenerator.Fields.cs` — `GenerateFieldsAsync` + `GeneratePoolsAsync` | `SeedData/DummyData/PPDM39DummyDataGenerator.Fields.cs` | 🔲 |
| 0.3.d | Create `PPDM39DummyDataGenerator.Wells.cs` — `GenerateWellsAsync` using `WellServices.CreateAsync(initializeDefaultStatuses: true)` | `SeedData/DummyData/PPDM39DummyDataGenerator.Wells.cs` | 🔲 |
| 0.3.e | Create `PPDM39DummyDataGenerator.Tests.cs` — `GenerateWellTestsAsync` | `SeedData/DummyData/PPDM39DummyDataGenerator.Tests.cs` | 🔲 |
| 0.3.f | Create `PPDM39DummyDataGenerator.Facilities.cs` — `GenerateFacilitiesAsync` | `SeedData/DummyData/PPDM39DummyDataGenerator.Facilities.cs` | 🔲 |
| 0.3.g | Create `PPDM39DummyDataGenerator.Production.cs` — `GenerateProductionAsync` (PDEN_VOL_SUMMARY) | `SeedData/DummyData/PPDM39DummyDataGenerator.Production.cs` | 🔲 |
| 0.3.h | Create `PPDM39DummyDataGenerator.Activities.cs` — `GenerateActivitiesAsync` (WELL_ACTIVITY) | `SeedData/DummyData/PPDM39DummyDataGenerator.Activities.cs` | 🔲 |
| 0.3.i | Register `PPDM39DummyDataGenerator` in `Program.cs` (ApiService) | `Program.cs` | 🔲 |
| 0.3.j | Add `/api/ppdm39/setup/generate-dummy-data` endpoint | `Controllers/PPDM39/PPDM39SetupController.cs` | 🔲 |
| 0.3.k | Add `/api/ppdm39/setup/dummy-data/status` endpoint | same | 🔲 |
| 0.3.l | Add `/api/ppdm39/setup/dummy-data` DELETE endpoint | same | 🔲 |
| 0.3.m | Create `DummyDataPage.razor` at `/ppdm39/setup/dummy-data` | `Pages/PPDM39/Setup/DummyDataPage.razor` | 🔲 |
| 0.3.n | Wire `DummyDataPage` into `SeedSetupPage` "Continue" navigation | `Pages/PPDM39/Setup/SeedSetupPage.razor` | 🔲 |

---

## Phase 0 Completion Criteria

All of the following must be true before Phase 1 begins:

- [ ] New user opens app → redirected to `/ppdm39/setup` automatically
- [ ] Wizard step 2B creates a working SQLite database on the server
- [ ] Wizard step 2A connects to an existing database and verifies the connection
- [ ] Schema check identifies and creates missing core tables
- [ ] `/ppdm39/setup/seed` seeds all 6 well-status reference tables + enum tables
- [ ] `/ppdm39/setup/dummy-data` generates demo FIELD + WELL + FACILITY + PRODUCTION rows using only seeded reference values
- [ ] After completing all three steps, `ppdm39:setup:completed = true` is stored and wizard is skipped on subsequent launches
- [ ] Setup re-entry is available from `/ppdm39/data-management` → "Re-run Setup"

---

## Notes

- `DemoDatabaseService` + `DemoDatabaseWizard.razor` (existing) targeted demo database creation via a dialog. Phase 0 replaces/supersedes that flow with a full-page wizard — the existing dialog can remain as a shortcut but the full wizard is the canonical path.
- `FirstRunService` must handle the case where the API is unreachable (network down during setup check) — in that case, assume setup is needed and redirect to `/ppdm39/setup`.
- SQLite creation endpoint should use `TheTechIdea.Beep.SqliteDatasourceCore` (already referenced in the Web project's packages) through `IDMEEditor` to register the connection.
