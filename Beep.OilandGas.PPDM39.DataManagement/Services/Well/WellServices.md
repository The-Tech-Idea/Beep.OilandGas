# WellServices — Developer Reference

> **Location**: `Beep.OilandGas.PPDM39.DataManagement/Services/Well/`  
> **Namespace**: `Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL`  
> **Standard**: PPDM 3.9 schema + PPDM Well Status & Classification v3 (R-3, June 2020)

---

## 1. Overview

`WellServices` is the **single entry-point** for all well-related operations in the Beep.OilandGas
system. It is implemented as a C# `partial class` spread across five files:

| File | Responsibility |
|---|---|
| `WellServices.cs` | Core CRUD (WELL entity) · constructor · field-scoped queries · `CreateAsync` alias |
| `WellServices.WellStatus.cs` | WELL_STATUS CRUD · facet accessors · reference data · default initialization |
| `WellServices.WellStructures.cs` | WELL_XREF operations · wellbore/completion tree · child entity discovery |
| `WellServices.Models.cs` | DTOs: `WellStatusInfo` · `WellSummaryDto` · `WellStatusTransition` |
| `WellServices.Helpers.cs` | Internal utilities: `FormatStatusId` · `ParseStatusId` · `GetStringValue` |

### Mandatory Rule

> **Never** create raw `PPDMGenericRepository` instances for `WELL`, `WELL_STATUS`, or `WELL_XREF`
> outside this service. Inject `WellServices` and call its typed methods.

---

## 2. PPDM 3.9 Well Entity Hierarchy

The entity diagram below maps to `WELL_XREF.XREF_TYPE` values used by `GetWellStructuresByUwiAsync`:

```
WELL_SET ──WELL_SET_WELL──► WELL
                              │
                 ┌────────────┼────────────────────────┐
                 │            │                        │
           WELL ORIGIN    WELL_XREF                FACILITY_WELL
                          (breakout)              (WELLHEAD STREAM)
                              │
              ┌───────────────┼────────────────────┐
              │               │                    │
           WELLBORE     WELL COMPLETION     WELLBORE SEGMENT
              │
        WELLBORE SEGMENT
              │
      WELLBORE COMPLETION
              │
    WELLBORE CONTACT INTERVAL
```

**Key column facts** (compile-time known — never use reflection on these):

| Table | Primary Key | Field Link | Notes |
|---|---|---|---|
| `WELL` | `UWI` | `ASSIGNED_FIELD` | Depth: `FINAL_TD`, `DRILL_TD` |
| `WELL_STATUS` | `UWI` + `STATUS_TYPE` + `STATUS_ID` + `EFFECTIVE_DATE` | — | `STATUS_TYPE` and `STATUS` stored directly |
| `WELL_XREF` | `UWI` + `XREF_TYPE` + `XREF_ID` | — | `XREF_TYPE` from `_defaults.GetWellboreXrefType()` |
| `R_WELL_STATUS` | `STATUS_TYPE` + `STATUS` | — | Reference/LOV — never insert directly |
| `FACILITY` | `FACILITY_ID` | `PRIMARY_FIELD_ID` | `FACILITY_TYPE = 'WELLHEAD STREAM'` for wellhead |

---

## 3. PPDM Well Status & Classification v3

The **PPDM Well Status & Classification v3** standard (R-3, June 2020) is a **faceted taxonomy**.
Each facet describes one orthogonal property of a well or its components.

### 3.1 What Changed from v2 → v3

| v2 Facet | v3 Replacement | Notes |
|---|---|---|
| Business Life Cycle Phase | **Life Cycle** | Simplified to 5 phases |
| Trajectory Type | **Profile Type** | Vertical/Inclined/Horizontal |
| Fluid Type | **Product Type** | Oil/Gas/Water/Geothermal/Mineral/Non-hydrocarbon Gas/Steam |
| Well Status + Wellbore Status | **Condition** | Active / Inactive (Shut In, Idle, Abandoned) |
| Operatorship | Absorbed into **Business Interest** qualifiers | Financial-Operated etc. |
| ~~Lahee Class~~ | **REMOVED in v3** | Was risk classification; now regulator-specific |
| ~~Well Reporting Class~~ | **REMOVED in v3** | Eliminated in v3 |
| *(new)* | **Product Significance** | Primary / Secondary / Tertiary / Show — always with Product Type |
| *(new)* | **Regulatory Approval** | Reg Submission → Review → Outcome → Monitoring → Reg Closed |

### 3.2 Facet Reference Table (v3)

| STATUS_TYPE | Scope | Values | When it changes |
|---|---|---|---|
| **Life Cycle** | Well / Wellbore | Planning · Constructing · Operating · Closing · Closed | Changes through life cycle. Well LC derived from component LCs. |
| **Business Interest** | Well / Wellbore | Yes-Financial-Operated · Yes-Financial-Non-operated · Yes-Obligatory · Yes-Technical · No | Can change at any time. Store most important qualifier. |
| **Business Intention** | Well | Explore · Appraise · Extend · Develop | Set at drilling start; does not change unless LC reverts to Planning. |
| **Outcome** | Well | Achieved · Unachieved | Evaluated once Business Intention is known. |
| **Play Type** | Well | Conventional · Shale · Oil Sands · Coalbed Methane · Gas Hydrate · Tight Sand · Sub-salt · Nonhydrocarbon · Carbon Capture and Storage | May change if Role or target formation changes. |
| **Role** | Well / Wellbore | Produce · Inject · Produce/Inject · Service · Research · No Role | Subject to review; may change over LC. Multiple roles → use highest significance. |
| **Condition** | Well / Wellbore | Active · Inactive (Shut In / Idle / Abandoned) | Multiple conditions → use highest significance. |
| **Profile Type** | Wellbore | Vertical · Inclined (Slant Hole, S-type, Deep Inclined) · Horizontal (Toe-up, Toe-down, Level, Undulating) | Confirmed after construction; may change if drilling deviates from plan. |
| **Well Structure** | Well | Simple · Simplex · Compound · Complex · Network | May change as new wellbores added. |
| **Product Type** | Well / Wellbore | Oil · Gas · Geothermal · Mineral · Non-hydrocarbon Gas · Steam · Water | Determined when well becomes operational; may change. |
| **Product Significance** | Well / Wellbore | Primary · Secondary · Tertiary · Show | Paired with Product Type; based on technical/economic factors. |
| **Fluid Direction** | Wellhead Stream | Inflow · Outflow · Static · Dual Flow | Can change over well life. |
| **Regulatory Approval** | Well / Component | Reg Submission · Reg Review · Reg Outcome · Reg Monitoring · Reg Closed | Per regulated activity; multiple approvals during well life. |

### 3.3 Life Cycle Hierarchy Rule

> At any point in time the Life Cycle phase of a **component** (wellbore, completion) may differ
> from the **well**, but a component cannot be in a more advanced phase than its parent.
>
> Example: a well cannot be **Closed** if one of its completions is still **Operating**.

### 3.4 Condition vs Life Cycle

| | Condition | Life Cycle |
|---|---|---|
| Granularity | Operational state relative to Role | Major milestone phases |
| Active | Fulfilling its Role | Operating |
| Inactive (Shut In) | Intentional pause — no regulatory notification | Still Operating |
| Idle | Intentional pause — requires regulatory notification | Still Operating |
| Abandoned | Permanently incapable | Closing → Closed |

---

## 4. STATUS_ID Format

All `WELL_STATUS.STATUS_ID` values use the composite key format:

```
STATUS_ID = "STATUS_TYPE,STATUS"
```

**Examples:**

| STATUS_TYPE | STATUS | STATUS_ID |
|---|---|---|
| Life Cycle | Operating | `Life Cycle,Operating` |
| Role | Produce | `Role,Produce` |
| Condition | Inactive | `Condition,Inactive` |
| Product Type | Oil | `Product Type,Oil` |

Use the helpers to avoid string manipulation:

```csharp
string id = WellServices.FormatStatusId("Life Cycle", "Operating");
// → "Life Cycle,Operating"

var (type, status) = WellServices.ParseStatusId(id);
// → ("Life Cycle", "Operating")
```

---

## 5. Common Operation Recipes

### Create a well and seed all v3 facets

```csharp
// Canonical call — always pass initializeDefaultStatuses: true
var well = new WELL
{
    UWI = "100/06-11-037-26W4/00",
    WELL_NAME = "Eagle Ford 11-37",
    ASSIGNED_FIELD = "EAGLE_FORD_01",
    OPERATOR = "ACME Energy"
};

await wellServices.CreateAsync(well, userId, initializeDefaultStatuses: true);
// Seeds all 11 well-level STATUS_TYPE facets from R_WELL_STATUS
```

### Read current status facets (no N+1 queries)

```csharp
// Returns Dictionary<string, WELL_STATUS> keyed by STATUS_TYPE
// Groups by WELL_STATUS.STATUS_TYPE directly — no R_WELL_STATUS round-trips
var current = await wellServices.GetCurrentWellStatusByUwiAsync(uwi);

Console.WriteLine(current["Life Cycle"].STATUS);        // "Operating"
Console.WriteLine(current["Condition"].STATUS);         // "Active"
Console.WriteLine(current["Role"].STATUS);              // "Produce"
Console.WriteLine(current["Product Type"].STATUS);      // "Oil"
```

### Get a compact summary with all facets

```csharp
WellServices.WellSummaryDto summary = await wellServices.GetWellSummaryAsync(uwi);

Console.WriteLine(summary.WellName);           // "Eagle Ford 11-37"
Console.WriteLine(summary.LifecyclePhase);     // "Operating"
Console.WriteLine(summary.Condition);          // "Active"
Console.WriteLine(summary.Role);               // "Produce"
Console.WriteLine(summary.ProductType);        // "Oil"
Console.WriteLine(summary.ProfileTypeFacet);   // "Horizontal"
```

### Transition a status (auto-expires previous)

```csharp
// Moves well from Planning → Constructing, automatically expires the old record
await wellServices.CreateWellStatusAsync(
    uwi, "Life Cycle", "Constructing", userId,
    effectiveDate: DateTime.Now);
```

### Check a specific facet

```csharp
bool isProducer = await wellServices.HasStatusTypeAsync(uwi, "Role");

var condition = await wellServices.GetWellConditionAsync(uwi);
bool isActive = condition?.STATUS == "Active";

var role = await wellServices.GetWellRoleAsync(uwi);
```

### Query by field

```csharp
// All active wells in a field
var wells = await wellServices.GetWellsByFieldAsync("EAGLE_FORD_01");

// Count
int count = await wellServices.GetWellCountByFieldAsync("EAGLE_FORD_01");

// Search by UWI prefix or name fragment
var matches = await wellServices.SearchWellsAsync("Eagle Ford", fieldId: "EAGLE_FORD_01");
```

### Get wellbores

```csharp
// All wellbore WELL_XREF entries for a well
var wellbores = await wellServices.GetWellboresAsync(uwi);

// Full structure tree grouped by XREF_TYPE
var structures = await wellServices.GetWellStructuresByUwiAsync(uwi);
// keys: wellOrigin, wellbore, wellboreSegment, wellboreContactInterval, wellboreCompletion, wellheadStream
```

### Status batch on new wellbore

```csharp
// Seed all wellbore-level facets for a new wellbore record
await wellServices.CreateWellStatusesBatchAsync(uwi, new Dictionary<string, string>
{
    ["Life Cycle"]          = "Planning",
    ["Business Interest"]   = "Yes-Financial-Operated",
    ["Role"]                = "Produce",
    ["Condition"]           = "Inactive",
    ["Profile Type"]        = "Horizontal",
    ["Product Type"]        = "Gas",
    ["Product Significance"]= "Primary"
}, userId);
```

---

## 6. Anti-Patterns

### ❌ Do NOT use deprecated v2 STATUS_TYPEs

```csharp
// WRONG — "Business Life Cycle Phase" is a v2 name; use "Life Cycle"
await wellServices.GetCurrentWellStatusByTypeAsync(uwi, "Business Life Cycle Phase");

// WRONG — "Lahee Class" eliminated in v3
await wellServices.HasStatusTypeAsync(uwi, "Lahee Class");

// WRONG — "Trajectory Type" renamed to "Profile Type"
await wellServices.GetCurrentWellStatusByTypeAsync(uwi, "Trajectory Type");

// WRONG — "Fluid Type" renamed to "Product Type"
await wellServices.GetCurrentWellStatusByTypeAsync(uwi, "Fluid Type");
```

### ❌ Do NOT create WELL repositories directly

```csharp
// WRONG — bypasses facet initialization and WellServices encapsulation
var repo = new PPDMGenericRepository(..., typeof(WELL), conn, "WELL");
await repo.InsertAsync(well, userId);
```

### ❌ Do NOT use DateTime.MinValue as "no expiry" sentinel

```csharp
// WRONG — was used in old code; causes incorrect expiry filtering
wellStatus.EXPIRY_DATE = DateTime.MinValue;

// CORRECT — null means "currently active, no defined end date"
wellStatus.EXPIRY_DATE = null;
```

### ❌ Do NOT use reflection on compile-time-known types

```csharp
// WRONG — fragile, wrong column names, silently returns null
var id = entity.GetType().GetProperty("WELL_ID")?.GetValue(entity);

// CORRECT — use typed property directly
var id = well.UWI;
```

### ❌ Do NOT query WELL_STATUS without STATUS_TYPE context

```csharp
// WRONG — returns all historical records for all facets; use for history only
var all = await wellServices.GetWellStatusByUwiAsync(uwi);

// CORRECT — current status per facet
var current = await wellServices.GetCurrentWellStatusByUwiAsync(uwi);
```

---

## 7. DI Registration

```csharp
// In Program.cs — register AFTER IDMEEditor, IPPDMMetadataRepository, IPPDM39DefaultsRepository
builder.Services.AddScoped<WellServices>(sp =>
    new WellServices(
        sp.GetRequiredService<IDMEEditor>(),
        sp.GetRequiredService<ICommonColumnHandler>(),
        sp.GetRequiredService<IPPDM39DefaultsRepository>(),
        sp.GetRequiredService<IPPDMMetadataRepository>(),
        connectionName));  // connectionName resolved earlier in Program.cs
```

---

## 8. Reference Data Seeding

The `R_WELL_STATUS` table must be pre-populated with WSC v3 STATUS_TYPE + STATUS combinations
before `InitializeDefaultWellStatusesAsync` can seed wells. Use the CSV seeder:

```csharp
var seeder = new PPDMSeederOrchestrator(editor, commonColumnHandler, defaults, metadata);
await seeder.SeedAllAsync(@"C:\SeedData\CSV", userId);
```

The reference data seed CSV should contain rows such as:

| STATUS_TYPE | STATUS | LONG_NAME | STATUS_GROUP |
|---|---|---|---|
| Life Cycle | Planning | Planning Phase | Business |
| Life Cycle | Constructing | Constructing Phase | Business |
| Life Cycle | Operating | Operating Phase | Business |
| Life Cycle | Closing | Closing Phase | Business |
| Life Cycle | Closed | Closed Phase | Business |
| Condition | Active | Active — Fulfilling Role | Operational |
| Condition | Inactive | Inactive — Not Fulfilling Role | Operational |
| Role | Produce | Producer | Operational |
| Role | Inject | Injector | Operational |
| Profile Type | Vertical | Vertical Wellbore | Technical |
| Profile Type | Inclined | Inclined Wellbore | Technical |
| Profile Type | Horizontal | Horizontal Wellbore | Technical |
| Product Type | Oil | Crude Oil | Product |
| Product Type | Gas | Natural Gas | Product |
| Product Significance | Primary | Primary Product | Product |
| Fluid Direction | Outflow | Outflow — Fluids to Surface | Operational |

---

## 9. Source Attribution

Well Status & Classification v3 is published by the **PPDM Association** (R-3, June 2020).  
Sponsored by Chevron Global.  
© Copyright 2020, PPDM Association. All Rights Reserved — www.PPDM.org
