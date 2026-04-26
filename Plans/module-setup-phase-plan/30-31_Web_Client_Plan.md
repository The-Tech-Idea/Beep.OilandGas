# Projects 30–31 — Web & Client
## Module Setup & Best-Practice Audit Plan

---

## Project 30 — Beep.OilandGas.Web (Blazor Server)

### Purpose
Blazor Server UI for the petroleum-engineer workflow application.  
All pages follow the engineer-workflow pattern from `Plans/UI-UX-OilAndGas-Guidelines.md`.

### SP-A: Structure Review
- **Status**: Complete
- No `ModelEntityBase` subclasses (Web = UI layer only).
- Pages organised under `Pages/PPDM39/{Phase}/` — Exploration, Development, Production, etc.
- Shared components in `Components/Shared/` (`KpiCard`, `StatusBadge`, `ProcessTimeline`).
- Services under `Services/` wrap `ApiClient` calls.

### SP-B: Data Class Audit
- **Status**: Complete — **Zero violations**.
- No local persisted table classes; all data models referenced from `Beep.OilandGas.Models`.

### SP-C: Build Fixes Applied
Two pre-existing compile errors found and fixed:

| # | File | Error | Fix |
|---|---|---|---|
| 1 | `Services/ExplorationServiceClient.cs`, `IExplorationServiceClient.cs` | `using PROSPECT = Beep.OilandGas.PPDM39.Models.PROSPECT` — namespace does not contain `PROSPECT` | Changed alias target to `Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT` |
| 2 | `Pages/PPDM39/Exploration/ProspectBoard.razor`, `Prospects.razor`, `WellProgramApproval.razor` | Fully-qualified `Beep.OilandGas.PPDM39.Models.PROSPECT` references | Updated all 3 to use `Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT` |

Additionally, `PROSPECT.Table.cs` in `Beep.OilandGas.ProspectIdentification` was missing the
PPDM 3.9 standard column `RESPONSIBLE_BA_ID` (VARCHAR 40 — business associate responsible for
the prospect). Added using correct scalar `SetProperty` pattern.

### SP-D: Build Validation
- **Status**: Complete ✓
- **Result**: `0 Error(s)  0 Warning(s)`

---

## Project 31 — Beep.OilandGas.Client

### Purpose
Shared HTTP client / service-proxy layer used by both the Blazor Web project and any external
consumers.

### SP-A: Structure Review
- **Status**: Complete
- No `ModelEntityBase` subclasses.
- Contains typed HTTP clients, API-contract interfaces, and request/response DTO helpers.

### SP-B: Data Class Audit
- **Status**: Complete — **Zero violations**.

### SP-C: Build Fix Applied

| # | File | Error | Fix |
|---|---|---|---|
| 1 | `Beep.OilandGas.Client.csproj` | `NU1605` — package downgrade: `TheTechIdea.Beep.DataManagementModels` `2.0.140` vs transitive requirement `>= 2.0.1444` (via `Beep.OilandGas.PPDM.Models`) | Updated direct reference to `2.0.1444` |

### SP-D: Build Validation
- **Status**: Complete ✓
- **Result**: `0 Error(s)  0 Warning(s)`
