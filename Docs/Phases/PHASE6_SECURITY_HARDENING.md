# Phase 6: Role-Based Security Hardening

**Status:** In Progress
**Priority:** HIGHEST — must complete before Phase 1-5 dashboards

## Task 6.1: Populate ALLOWED_WORKFLOWS_JSON for All Personas

**File:** `Beep.OilandGas.UserManagement/Services/DefaultSecuritySeedService.cs`

### Current State
All 16 personas have `ALLOWED_WORKFLOWS_JSON = null` — meaning navigation policy is default-allow.

### Target
Each persona gets explicit workflow access codes that the NavigationPolicyService checks against routes.

### Implementation

```csharp
// Workflow keys from NavigationPolicyService.CoreRouteToWorkflow:
//   "exploration", "development", "production", "reservoir",
//   "economics" (covers /economics and /accounting), "hse",
//   "data" (covers /ppdm39/data-management and /ppdm39/setup),
//   "processes" (covers /ppdm39/process)

// Per-persona workflow access:
FIELD_ENGINEER             → ["production", "data"]
PRODUCTION_MANAGER         → ["production", "reservoir", "economics", "data"]
RESERVOIR_ENGINEER         → ["reservoir", "production", "data"]
DRILLING_ENGINEER          → ["development", "production", "data"]
HSE_OFFICER                → ["hse", "data"]
FACILITIES_ENGINEER        → ["development", "production", "data"]
DATA_ANALYST               → ["data", "processes"]
ADMINISTRATOR              → ["exploration", "development", "production", "reservoir", "economics", "hse", "data", "processes"]
EXPLORATION_GEOLOGIST      → ["exploration", "data"]
DEVELOPMENT_PLANNER        → ["development", "economics", "data"]
PRODUCTION_ENGINEER        → ["production", "data"]
DECOMMISSIONING_COORDINATOR → ["development", "hse", "data"]
HSE_COORDINATOR            → ["hse", "data"]
FACILITY_OPERATOR          → ["development", "production", "data"]
ASSET_MANAGER              → ["exploration", "development", "production", "reservoir", "economics", "data"]
WORKFLOW_ADMINISTRATOR     → ["processes", "data"]
```

## Task 6.2: Add [Authorize] to Unprotected API Controllers

Five controllers currently lack authorization:

| Controller | Route | Risk | Fix |
|------------|-------|------|-----|
| DataManagementController | /api/datamanagement | HIGH (CRUD for all PPDM tables) | Add `[Authorize]` |
| WellController | /api/well | Medium | Add `[Authorize]` |
| CalculationsController | /api/calculations | Low | Add `[Authorize]` |
| ConnectionController | /api/connections | HIGH (DB connection CRUD) | Add `[Authorize(Roles = "Admin,Administrator")]` |
| DemoDatabaseController | /api/demo | Medium | Add `[Authorize(Roles = "Admin,Administrator")]` |

## Task 6.3: Create Persona-to-Role Mapping

Files:
- `Beep.OilandGas.Models/Data/Identity/PERSONA_ROLE.cs` (NEW — PPDM table entity)
- `Beep.OilandGas.UserManagement/Models/Identity/PersonaRole.cs` (NEW)
- `Beep.OilandGas.UserManagement/Services/DefaultSecuritySeedService.cs` (MODIFY — seed persona-role mappings)

## Verification
1. Login as a user with FIELD_ENGINEER persona — verify only production + data pages accessible
2. Attempt unauthenticated GET /api/datamanagement/WELL — verify 401 response
3. Login as Admin — verify all pages accessible
