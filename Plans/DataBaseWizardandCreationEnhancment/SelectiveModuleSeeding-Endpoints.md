# Selective Module Seeding - API Endpoints Implementation

## Overview
Implemented API endpoints to enable selective module seeding workflows, allowing operators to seed only chosen modules in their declared order. This builds on the IModuleSetup plugin architecture without requiring code changes to modules themselves.

## Files Created/Modified

### 1. **DTOs: ModuleSeedingRequest.cs** 
`Beep.OilandGas.Models/Core/DTOs/ModuleSeedingRequest.cs` (NEW)

**Classes:**
- `ModuleSeedingRequest` - Request to seed selected modules
  - `ModuleIds: List<string>` - Module IDs to execute (empty = all modules)
  - `ConnectionName: string` - Database connection (default: "PPDM39")
  - `UserId: string` - Audit user (default: "SYSTEM")
  - `OperationId: string?` - For progress tracking
  - `RunAsync: bool` - Fire-and-forget mode flag

- `ModuleSeedingResponse` - Full response from selective seeding
  - `Success: bool` - All modules succeeded
  - `Message: string` - Summary message
  - `TotalRecordsInserted: int` - Rows inserted across all modules
  - `ModulesRun: int` - Number executed
  - `ModulesSucceeded: int` - Number succeeded
  - `ModuleDetails: List<ModuleExecutionDetail>` - Per-module results
  - `Errors: List<string>` - All errors across modules
  - `OperationId: string?` - For async polling

- `ModuleExecutionDetail` - One module's result within response
  - `ModuleId: string` - Module ID
  - `ModuleName: string` - Display name
  - `Order: int` - Execution order
  - `Success: bool` - Completed successfully
  - `RecordsInserted: int` - Rows inserted
  - `TablesSeeded: int` - Tables touched
  - `SkipReason: string?` - Why module was skipped, if applicable
  - `Errors: List<string>` - Module-specific errors

- `AvailableModulesResponse` - List of all modules
  - `TotalModules: int` - Count
  - `Modules: List<ModuleInfo>` - Each module's metadata

- `ModuleInfo` - One module's metadata
  - `ModuleId: string`
  - `ModuleName: string`
  - `Order: int`
  - `EntityTypes: List<string>` - Table names owned

### 2. **ModuleSetupOrchestrator.cs** - Selective Run Support
`Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupOrchestrator.cs` (MODIFIED)

**New Methods:**

```csharp
/// Runs seed for only the specified module IDs, in their declared Order.
public async Task<OrchestratorSetupResult> RunSeedForModulesAsync(
    IReadOnlyList<string> selectedModuleIds,
    string connectionName,
    string userId,
    CancellationToken cancellationToken = default)
```
- Filters registered modules by ID list
- Executes only selected modules, **maintaining their declared Order**
- Continues on errors (except ModuleSetupAbortException or OperationCanceledException)
- Returns OrchestratorSetupResult with per-module details

```csharp
/// Returns metadata about all registered modules.
public IReadOnlyList<(string ModuleId, string ModuleName, int Order, 
    IReadOnlyList<Type> EntityTypes)> GetModuleMetadata()
```
- Lists all available modules with their metadata
- Used by `/available-modules` endpoint

### 3. **PPDM39SetupService.cs** - Selective Seeding Methods
`Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39SetupService.cs` (MODIFIED)

**New Methods:**

```csharp
/// Seeds only the specified modules, in their declared Order.
public async Task<SeedingOperationResult> SeedSelectedModulesAsync(
    IReadOnlyList<string> moduleIds,
    string connectionName = "PPDM39",
    string? userId = null,
    CancellationToken cancellationToken = default)
```
- Delegates to orchestrator's `RunSeedForModulesAsync` if available
- Falls back to `SeedAllReferenceDataAsync` if orchestrator unavailable
- Returns `SeedingOperationResult` with aggregated Details and Errors

```csharp
/// Returns metadata about all available IModuleSetup implementations.
public IReadOnlyList<(string ModuleId, string ModuleName, int Order, 
    IReadOnlyList<Type> EntityTypes)> GetAvailableModules()
```
- Returns orchestrator's module metadata
- Empty list if orchestrator unavailable

### 4. **PPDM39SetupController.cs** - API Endpoints
`Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.cs` (MODIFIED)

**New Endpoints:**

#### GET `/api/ppdm39/setup/available-modules`
**Purpose:** List all registered IModuleSetup implementations
**Authentication:** AllowAnonymous
**Response:** `AvailableModulesResponse`
```json
{
  "totalModules": 11,
  "modules": [
    {
      "moduleId": "CorePpdmModule",
      "moduleName": "Core PPDM Reference Data",
      "order": 0,
      "entityTypes": ["R_WELL_CLASS", "R_WELL_TYPE", ...]
    },
    ...
  ]
}
```

#### POST `/api/ppdm39/setup/seed/selected-modules`
**Purpose:** Seed only selected modules
**Authentication:** Requires Authorization
**Request:** `ModuleSeedingRequest`
```json
{
  "moduleIds": ["CorePpdmModule", "SharedReferenceModule"],
  "connectionName": "PPDM39",
  "userId": "SYSTEM",
  "operationId": "op-12345",
  "runAsync": false
}
```
**Response:** `ModuleSeedingResponse` (200 OK or 207 Multi-Status if partial failure)
```json
{
  "success": true,
  "message": "Selected modules seeded successfully: 450 total rows.",
  "totalRecordsInserted": 450,
  "modulesRun": 2,
  "modulesSucceeded": 2,
  "moduleDetails": [
    {
      "moduleId": "CorePpdmModule",
      "moduleName": "Core PPDM Reference Data",
      "order": 0,
      "success": true,
      "recordsInserted": 200,
      "tablesSeeded": 10,
      "skipReason": null,
      "errors": []
    },
    ...
  ],
  "errors": [],
  "operationId": "op-12345"
}
```

**Helper Methods:**
- `ExtractModuleIdFromDetail(string detail)` - Parse module ID from detail string
- `ExtractModuleNameFromDetail(string detail)` - Parse module name from detail string
- `ExtractSkipReason(string detail)` - Parse skip reason from detail string

## Usage Scenarios

### Scenario 1: Development Environment - Core Only
```json
POST /api/ppdm39/setup/seed/selected-modules
{
  "moduleIds": ["CorePpdmModule", "SharedReferenceModule", "WellStatusFacetModule"],
  "connectionName": "PPDM39-DEV"
}
```
**Result:** Fast bootstrap of core PPDM tables without demo data.

### Scenario 2: Production - Skip Demo Data
```json
POST /api/ppdm39/setup/seed/selected-modules
{
  "moduleIds": [
    "CorePpdmModule",
    "SharedReferenceModule",
    "WellStatusFacetModule",
    "SecurityModule"
  ],
  "connectionName": "PPDM39-PROD",
  "userId": "setup-operator"
}
```
**Result:** All reference and security data seeded; DemoDataModule intentionally excluded.

### Scenario 3: List Available Modules
```json
GET /api/ppdm39/setup/available-modules
```
**Result:** UI dropdown or admin panel shows all seedable modules with their details.

### Scenario 4: Retry Single Failed Module
If SharedReferenceModule fails during a full seed, operator can:
```json
POST /api/ppdm39/setup/seed/selected-modules
{
  "moduleIds": ["SharedReferenceModule"],
  "connectionName": "PPDM39"
}
```
**Result:** Only that module re-runs, preserving all other data.

## Execution Guarantees

1. **Module Order Respected**
   - Modules always run in their declared `Order` value, regardless of list order
   - Example: If `["SecurityModule", "CorePpdmModule"]` requested, CorePpdmModule (Order 0) runs first

2. **All-or-Nothing per Module**
   - Each module succeeds or fails independently
   - One module's failure does not stop others (unless ModuleSetupAbortException thrown)
   - Response includes per-module results

3. **Idempotency**
   - Modules use `SkipIfExistsAsync` helpers to avoid duplicate inserts
   - Safe to re-run selective seed multiple times

4. **Error Isolation**
   - Errors collected per module and per request
   - Details show which modules succeeded/failed
   - 207 Multi-Status returned for partial success (not 500)

## Backward Compatibility

- Existing `SeedAllReferenceDataAsync` endpoint unchanged
- Falls back to legacy behavior if `ModuleSetupOrchestrator` not available (graceful degradation)
- No breaking changes to service contracts

## Integration with Existing Infrastructure

- **ModuleSetupOrchestrator:** Sorted modules by Order, executes with cancellation support
- **SeedingOperationResult:** Aggregates Details and Errors as before
- **DI Container:** Orchestrator resolved as optional dependency (nullable parameter)
- **Logging:** Full audit trail via _logger.LogInformation for each module

## Testing Recommendations

1. **Happy Path:** Seed core modules only in development environment
2. **Partial Failure:** Intentionally break one module, verify others complete
3. **Module Ordering:** Request modules in reverse order, verify they execute in declared order
4. **Idempotency:** Run same selective seed twice, verify no duplicates
5. **Metadata:** Call available-modules endpoint, verify all 11 modules listed with correct Order

## Future Enhancements

1. **Async Progress Polling:** Implement `/api/ppdm39/setup/operation/{operationId}` endpoint for long-running seeds
2. **Dependency Graph:** Add module prerequisite validation (e.g., Security before Data)
3. **Rollback on Abort:** Optional transaction rollback if any module throws ModuleSetupAbortException
4. **Custom Filters:** Date-range filters, record-count limits per module
5. **Seed Templates:** Saved configurations for common workflows (dev, test, prod)
