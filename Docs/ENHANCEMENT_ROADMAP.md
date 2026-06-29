# Beep.OilandGas — Enhancement Roadmap

**Generated:** 2026-06-27
**Scope:** All 50+ projects
**Methodology:** Every .cs file read in full — findings based on actual code, not assumptions

---

## Executive Summary

This document identifies enhancement opportunities across the entire Beep.OilandGas solution, organized by priority and category. Each finding is backed by specific code references discovered during comprehensive source code reading.

### Priority Distribution

| Priority | Count | Description |
|----------|-------|-------------|
| 🔴 CRITICAL | 12 | Bugs, security issues, data integrity risks |
| 🟠 HIGH | 28 | Architecture debt, missing implementations, duplicated code |
| 🟡 MEDIUM | 35 | Code quality, consistency, maintainability |
| 🟢 LOW | 22 | Cleanup, optimization, polish |

---

## 🔴 CRITICAL — Bugs and Security Issues

### C-1: Refresh Tokens Are Actually JWTs — No Server-Side Revocation
**Project:** Beep.OilandGas.UserManagement
**File:** `Services/AuthService.cs` lines 478-506
**Issue:** `ValidateRefreshToken` parses refresh tokens as JWTs using the same symmetric key. `RevokeTokenAsync` only writes an audit event — it does not invalidate the token. There is no token store.
**Impact:** Security vulnerability — a leaked access token can be used as a refresh token indefinitely.
**Fix:** Implement server-side refresh token storage in the database. Use opaque random tokens. Add proper revocation that marks tokens as invalid.

### C-2: SQL Injection in RowLevelSecurityService
**Project:** Beep.OilandGas.UserManagement
**File:** `Services/RowLevelSecurityService.cs` lines 106-118
**Issue:** Filter expressions build SQL strings via interpolation: `$"FIELD_ID IN ({string.Join(",", fieldScopes.Select(f => $"'{f}'"))})"`. No parameterization.
**Impact:** SQL injection vulnerability when scope values contain malicious input.
**Fix:** Use parameterized queries or AppFilter pattern consistently.

### C-3: Wilson K-Value Formula Mismatch in FlashCalculator
**Project:** Beep.OilandGas.FlashCalculations
**File:** `Calculations/FlashCalculator.cs` lines 27-33
**Issue:** The `lnK` formula computes `(1 - Tr)` but the executed `k` uses `(1 - 1/Tr)` — fundamentally different functional forms.
**Impact:** Incorrect K-values for flash calculations, potentially wrong phase behavior predictions.
**Fix:** Resolve which formula is correct (standard Wilson: `exp(5.37 * (1 + omega) * (1 - Tc/T))`), fix implementation, add test with known vapor-liquid equilibrium data.

### C-4: UserManagementService Creates Duplicates Instead of Updating
**Project:** Beep.OilandGas.UserManagement
**File:** `Services/UserManagementService.cs` line 98
**Issue:** `UpdateAsync` calls `repo.InsertAsync(user, "system")` instead of `repo.UpdateAsync`.
**Impact:** Every user update creates a duplicate record. Data corruption.
**Fix:** Change to `UpdateAsync` with proper userId parameter.

### C-5: Phase Envelope Dew Point Dead Code
**Project:** Beep.OilandGas.FlashCalculations
**File:** `Calculations/PhaseEnvelope.cs` lines 270-273
**Issue:** Variable `f` is assigned incorrectly then immediately overwritten to `0.0`. The intermediate calculation is dead code.
**Impact:** Dew point calculation uses only the second (Wilson K-based) formula, not EOS-consistent values when `useEos=true`.
**Fix:** Remove dead code or correct the logic to use EOS K-values when requested.

### C-6: CORS Policy — AllowAnyOrigin in Production
**Project:** Beep.OilandGas.ApiService
**File:** `Program.cs` lines 2517-2525
**Issue:** `AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()` is permissive.
**Impact:** Security risk in production — any website can call the API.
**Fix:** Restrict to known origins from configuration. Apply only in development.

### C-7: X-User-Id Header Impersonation
**Project:** Beep.OilandGas.ApiService
**File:** Controllers (multiple) — `DataManagementController.GetUserIdFromRequest()`
**Issue:** The `X-User-Id` header is accepted as a user identifier without validation.
**Impact:** Potential impersonation if JWT auth is bypassed or misconfigured.
**Fix:** Only extract user ID from validated JWT claims, never from headers.

### C-8: Refresh Token Duplicate — Same Symmetric Key as Access Tokens
**Project:** Beep.OilandGas.UserManagement
**File:** `Services/AuthService.cs`
**Issue:** Both access and refresh tokens use the same HMAC-SHA256 signing key. This means an access token can be used in place of a refresh token.
**Impact:** Expired access tokens could be reused for token refresh.
**Fix:** Use different keys. Store refresh tokens server-side.

### C-9: No Transactional Boundaries in ProcessProductionCycle
**Project:** Beep.OilandGas.ProductionAccounting
**File:** `Services/ProductionAccountingService.cs`
**Issue:** The 5-step production cycle (measurement → allocation → royalty → revenue → GL posting) has no distributed transaction wrapping. Failure in step 3 means steps 1-2 are already committed with no rollback.
**Impact:** Financial data inconsistency on partial failures.
**Fix:** Implement compensating transactions or use a saga/orchestration pattern with rollback handlers.

### C-10: Sync-Over-Async in Compatibility Layer
**Project:** Beep.OilandGas.ProductionAccounting
**File:** `Services/ProductionAccountingService.ControllerFacade.cs` (1,384 lines)
**Issue:** `RunSyncCompatibility` uses `.ConfigureAwait(false).GetAwaiter().GetResult()` extensively.
**Impact:** Deadlocks in ASP.NET Core with SynchronizationContext. Crashes under load.
**Fix:** Convert compatibility layer to async all the way, or remove it and update callers.

### C-11: FieldScoping Fragility — Phase Services Not Constructed With FieldId
**Project:** Beep.OilandGas.LifeCycle
**File:** `Services/FieldOrchestrator.cs`
**Issue:** Phase services (development, production, decommissioning) are not constructed with fieldId. FieldId is passed as a parameter to every method. If a caller forgets to pass it, cross-field data leakage occurs.
**Impact:** Potential data corruption — viewing or modifying wrong field's data.
**Fix:** Inject field-scoped proxies or use scoped DI container per field.

### C-12: No Database Transaction Wrapping Across Multi-Step Operations
**Project:** Multiple (Beep.OilandGas.LifeCycle, Beep.OilandGas.ProductionAccounting)
**Issue:** Combined operations (e.g., start process + transition state, create prospect + evaluate) are not wrapped in DB transactions.
**Impact:** Inconsistent system state on partial failure.
**Fix:** Use `System.Transactions.TransactionScope` or IDMEEditor unit-of-work transactions for combined operations.

---

## 🟠 HIGH — Architecture Debt and Missing Implementations

### H-1: PPDM39SetupController Is a God Controller (3,155 lines)
**Project:** Beep.OilandGas.ApiService
**Issue:** Single controller mixes database creation, LOV management, RA table extraction, schema migration, dummy data generation, module seeding, and script generation.
**Fix:** Split into: `DatabaseSetupController`, `LOVManagementController`, `SchemaMigrationController`, `DummyDataController`, `ReferenceDataSeedingController`.

### H-2: PPDM39Metadata.Generated.cs Is 5.8MB
**Project:** Beep.OilandGas.PPDM39.DataManagement
**Issue:** Massive auto-generated file increases compile time and assembly size.
**Fix:** Load metadata from an external JSON file or embedded resource at runtime.

### H-3: 1,384-Line Compatibility Layer with 17 Nested Classes
**Project:** Beep.OilandGas.ProductionAccounting
**Issue:** `ControllerFacade.cs` is a massive backward-compatibility bridge with in-memory ConcurrentDictionary caches that are not synced with the database.
**Fix:** Incrementally migrate callers to new async services. Remove in-memory caches in favor of database queries. Target: reduce to <200 lines within 3 sprints.

### H-4: 40+ HydraulicPumps Service Methods Are Stubs
**Project:** Beep.OilandGas.HydraulicPumps
**Issue:** `HydraulicPumpService.cs` has 40+ methods that create empty DTOs with validation checks but no actual calculation logic.
**Fix:** Prioritize and implement: (1) Design methods, (2) Performance analysis, (3) Diagnostics. Defer monitoring/reporting.

### H-5: 29 PlungerLift Service Methods Return Hardcoded Values
**Project:** Beep.OilandGas.PlungerLift
**Issue:** Most methods return hardcoded values. Only `AnalyzePlungerCycleAsync` does real calculation.
**Fix:** Implement cycle optimization using PlungerLiftCalculator, add pressure-dependent rise velocity, use critical velocity in gas requirement calculation.

### H-6: Development Planning Advanced Analysis Returns Mock Data
**Project:** Beep.OilandGas.DevelopmentPlanning
**Issue:** GHG emissions always returns 10,000 tCO2e. Equipment reliability always 98%. Risk analysis returns same hardcoded items. Sensitivity multiplies NPV by fixed factors.
**Fix:** Integrate with real engineering models. Use configurable emission factors. Load equipment reliability data from PPDM.

### H-7: Lease Acquisition Service Returns Hardcoded/Mock Data
**Project:** Beep.OilandGas.LeaseAcquisition
**Issue:** `SearchLeaseProspectsAsync` creates 5 hardcoded prospects. `EvaluateLeaseOpportunityAsync` returns 3 hardcoded factors. `SaveLeaseAcquisitionAsync` is labeled "Simulated persistence".
**Fix:** Connect to PPDM LAND_RIGHT/LAND_AGREEMENT tables. Implement real search and evaluation logic.

### H-8: Production Operations Advanced Methods Are Empty Stubs
**Project:** Beep.OilandGas.ProductionOperations
**Issue:** `RecordOperationalCostsAsync` does nothing. `ValidateOperationsDataAsync` reports zero records. `ExportOperationsDataAsync` returns empty byte array.
**Fix:** Implement cost recording to PRODUCTION_COSTS table. Add validation against operational rules. Add CSV/JSON export.

### H-9: Evaluate Prospect Logic Duplicated Across 3 Services
**Project:** Beep.OilandGas.ProspectIdentification
**Issue:** `PPDMExplorationService.EvaluateProspectAsync`, `ProspectEvaluationService.EvaluateProspectAsync`, and `ProspectIdentificationService.EvaluateProspectAsync` have overlapping but different algorithms.
**Fix:** Consolidate into one canonical implementation. Deprecate duplicates.

### H-10: Two Competing Lease Services (Dual Code Paths)
**Project:** Beep.OilandGas.LeaseAcquisition
**Issue:** `LeaseManagementService` (UnitOfWork-based) and `LeaseAcquisitionService` (PPDMGenericRepository-based) provide overlapping CRUD. Dual code paths with different ID generation strategies.
**Fix:** Consolidate into one service. Standardize on PPDMGenericRepository pattern.

### H-11: UserManagement Dual Schema (APP_* vs PPDM Tables)
**Project:** Beep.OilandGas.UserManagement
**Issue:** Custom models (AppUser, AppRole, AppPermission) target APP_* tables, while seed service uses PPDM types (USER, ROLE, PERMISSION). Two parallel schemas for the same logical data.
**Fix:** Choose one authoritative schema. Migrate data. Remove the other.

### H-12: No Global Exception Handling in API
**Project:** Beep.OilandGas.ApiService
**Issue:** Every controller has try/catch blocks with nearly identical error responses. ~350+ duplicated error handling blocks.
**Fix:** Add global `ExceptionFilter` or `UseExceptionHandler` middleware. Reduce controller boilerplate by ~80%.

### H-13: No API Versioning Strategy
**Project:** Beep.OilandGas.ApiService
**Issue:** 350+ endpoints with no versioning. Breaking changes require coordinated client updates.
**Fix:** Add `Microsoft.AspNetCore.Mvc.Versioning`. Start with `/api/v1/` namespace.

### H-14: No Health Check Endpoint
**Project:** Beep.OilandGas.ApiService
**Issue:** No `/health` or `/healthz` endpoint for load balancers and monitoring.
**Fix:** Add `Microsoft.Extensions.Diagnostics.HealthChecks` with database, identity server, and disk checks.

### H-15: No Rate Limiting
**Project:** Beep.OilandGas.ApiService
**Issue:** No protection against API abuse or accidental overuse.
**Fix:** Add `Microsoft.AspNetCore.RateLimiting` with token bucket or fixed window policy.

### H-16: Fire-and-Forget Task.Run Without Error Handling
**Project:** Beep.OilandGas.ApiService
**Files:** PPDM39SetupController, PPDM39WorkflowController
**Issue:** `Task.Run` used for long-running operations without exception handling, progress reporting, or structured concurrency.
**Fix:** Replace with proper background job framework (Hangfire, Quartz.NET, or BackgroundService).

### H-17: Visualization Components Are Placeholder Stubs
**Project:** Beep.OilandGas.Web
**Files:** WellboreDiagram.razor, CompletionDiagram.razor, WellDetailsView.razor, WellVisualization.razor, FieldMap.razor
**Issue:** Contain placeholder text ("Implementation needed") or simplified reflection-based displays.
**Fix:** Integrate with Drawing project for actual schematic rendering.

### H-18: UserRoles Page API Calls Commented Out
**Project:** Beep.OilandGas.Web
**File:** `Pages/Admin/AccessControl/UserRoles.razor` lines 107, 123
**Issue:** Actual API calls are commented out. Role management is non-functional, relying on Snackbar messages.
**Fix:** Uncomment and wire up to UserManagement API.

### H-19: DataManager Resume Stores Filename Not Full Path
**Project:** Beep.OilandGas.DataManager
**File:** `Services/DataManager.cs` line 289
**Issue:** `ResumeModuleExecutionAsync` sets `FullPath = fileName` instead of the original full path. `File.Exists` checks will fail.
**Impact:** Resume functionality is broken. Long-running schema installations cannot be resumed.
**Fix:** Store and restore the original full script path.

### H-20: DataManager Duplicate Module Registrations
**Project:** Beep.OilandGas.DataManager
**File:** `Core/Registry/ModuleDataRegistry.cs`
**Issue:** `PipelineAnalysis` and `ProductionForecasting` modules registered twice in `CreateDefaultModules()`. Second registration silently overwrites first.
**Fix:** Remove duplicates. Add detection/logging for duplicate registrations.

### H-21: No Password History or Session Management Implementations
**Project:** Beep.OilandGas.UserManagement
**Issue:** `IPasswordHistoryService` and `ISessionManagementService` interfaces exist but neither is registered in DI nor implemented. Password rotation is not enforced.
**Fix:** Implement and register both services. Add password history check to `ChangePasswordAsync`.

### H-22: API 11L Implementation Is Incomplete
**Project:** Beep.OilandGas.SuckerRodPumping
**File:** `Calculations/Api11LCalculator.cs`
**Issue:** Most methods return hardcoded values or linear approximations with comments like "stubs", "rough", "placeholder".
**Fix:** Implement full API RP 11L chart data as lookup tables with interpolation. Integrate with published API reference data.

### H-23: HI 9.6.7 Viscosity Correction Has Conflicting Formulas
**Project:** Beep.OilandGas.PumpPerformance
**File:** `Calculations/ViscosityCorrectionCalculator.cs`
**Issue:** Multiple conflicting formulas commented out. C_H set equal to C_Q with note that this is approximate. Author admits uncertainty.
**Fix:** Validate against ANSI/HI 9.6.7-2015 standard. Implement correct formulas. Add unit tests with known test cases from the standard.

### H-24: GetProcessDefinitionRepositoryAsync Marked Async But Does No Async Work
**Project:** Beep.OilandGas.LifeCycle
**File:** Multiple services
**Issue:** Methods marked `async` with no actual `await` — triggers CS1998 warnings.
**Fix:** Remove `async` keyword or add actual async work.

### H-25: WellComparisonService Uses GetAwaiter().GetResult()
**Project:** Beep.OilandGas.LifeCycle
**File:** `Services/WellComparisonService.cs` line 449
**Issue:** `_metadata.GetTableMetadataAsync("WELL").GetAwaiter().GetResult()` can cause deadlocks in ASP.NET Core.
**Fix:** Convert to fully async pattern with proper await throughout call chain.

### H-26: EOR Type Detection Uses Fragile Substring Matching
**Project:** Beep.OilandGas.EnhancedRecovery
**File:** `Data/Constants/EorMethodConstants.cs`
**Issue:** `IsWaterFlood` checks `Contains("WATER")` which matches "WATER_FLOOD" but would also match any type containing "WATER".
**Fix:** Use exact string comparison with a well-known constant set.

### H-27: PPDM39.DataManagement Has No DI Registration Extension Method
**Issue:** Consumers must manually register 100+ services. No `AddPPDM39DataManagement(this IServiceCollection)` method exists.
**Fix:** Create a comprehensive extension method with sensible defaults.

### H-28: PPDMCalculationService Partial Class Sprawl (16 files)
**Project:** Beep.OilandGas.LifeCycle
**Issue:** Extremely large service split across 16 partial files. Total surface area is unmanageable.
**Fix:** Split into separate services per domain (NodalCalculationService, DCACalculationService, FlashCalculationService, etc.).

---

## 🟡 MEDIUM — Code Quality and Maintainability

### M-1: ZFactorCalculator Duplicated Across Projects
**Projects:** Beep.OilandGas.GasProperties + Beep.OilandGas.Properties
**Fix:** Extract to shared library. Reference instead of duplicating.

### M-2: Material Database Duplicated in SuckerRodPumping
**File:** `Calculations/FatigueAnalysis.cs` + `Calculations/RodStringOptimization.cs`
**Fix:** Create single `RodMaterialDatabase` class with all properties (tensile, endurance, cost, BasquinExponent).

### M-3: Global Exception Handling Missing — Try/Catch in Every Controller
**Project:** Beep.OilandGas.ApiService
**Fix:** Add global exception filter or middleware.

### M-4: Inconsistent DTO Location — Some in Interface Files, Some in Service Files
**Impact:** 30+ DTOs defined inline in `IProductionOperationsService.cs`. Makes shared consumption difficult.
**Fix:** Move all DTOs to Models project in dedicated files.

### M-5: 44 DataManager Module Classes Are Trivially Identical
**Project:** Beep.OilandGas.DataManager
**Issue:** Each module differs only in 5 property values but requires its own class file.
**Fix:** Data-driven approach using attributes or configuration JSON. Eliminates 99% of module boilerplate.

### M-6: Hardcoded 20% Contingency in Decommissioning Cost Models
**Project:** Beep.OilandGas.Decommissioning
**Fix:** Make configurable. Load from jurisdiction defaults or industry cost databases.

### M-7: Hardcoded Pr/Pc/Tc/Acentric Factors
**Projects:** Multiple (FlashCalculations, GasProperties, OilProperties)
**Issue:** Component critical properties are estimated from simple correlations rather than loaded from a standard component database.
**Fix:** Create a `ComponentDatabase` with standard Tc, Pc, omega, MW for common hydrocarbons and non-hydrocarbons (from GPSA, DIPPR, or NIST).

### M-8: Async Marked Methods Without Real Async Operations
**Projects:** ALL engineering projects
**Issue:** Methods marked `async Task` but do `await Task.CompletedTask` after synchronous calculations.
**Fix:** Either make truly async (e.g., for I/O operations) or convert to synchronous where only CPU-bound calculations occur.

### M-9: Thread Safety — new Random() Per Call
**Projects:** PipelineAnalysis, PumpPerformance, PlungerLift, SuckerRodPumping
**Issue:** `new Random()` per call creates non-random seeds in tight loops and is not thread-safe.
**Fix:** Use `Random.Shared` (C# 13) or a static thread-safe `Random` instance.

### M-10: Missing CancellationToken Support
**Projects:** ALL services
**Issue:** Long-running methods (Monte Carlo simulation, multi-stage flash, portfolio optimization) don't accept CancellationToken.
**Fix:** Add CancellationToken parameters. Forward to repository calls and loop iterations.

### M-11: Inconsistent Nullable Handling
**Projects:** ALL
**Issue:** Some use `string?` with nullable annotations, others use `string` with `string.Empty` default. Mixed `ArgumentNullException.ThrowIfNull()` (new) and `if/throw` (old).
**Fix:** Standardize on .NET 8+ nullable patterns throughout.

### M-12: Hardcoded Economic Defaults — 10% Discount Rate, 20% Contingency
**Project:** Beep.OilandGas.EconomicAnalysis
**Fix:** Make configurable via PPDM reference data or application settings.

### M-13: Real Options Analysis Returns Hardcoded Percentages
**Project:** Beep.OilandGas.EconomicAnalysis
**File:** `Services/EconomicAnalysisService.Advanced.cs`
**Issue:** Expansion option = 20% of base NPV, abandonment = 15%, switching = 10% — hardcoded.
**Fix:** Implement Black-Scholes or binomial lattice models.

### M-14: OilProperties Calculates Density/Viscosity Ignoring GOR
**Project:** Beep.OilandGas.OilProperties
**File:** `Services/OilPropertiesService.cs`
**Issue:** `CalculateOilDensity` discards `gasOilRatio` with `_ =`. Density is stock-tank only.
**Fix:** Implement saturated oil density with solution gas correction.

### M-15: GasProperties Uses Simple Power-Law Instead of Lee-Gonzalez-Eakin
**Project:** Beep.OilandGas.GasProperties
**File:** `Services/GasPropertiesService.cs`
**Issue:** `AnalyzeGasViscosityAsync` uses `viscosityAtSC * (1 + 10.8 * (P/1000)^0.4)` instead of the dedicated calculator class.
**Fix:** Use the dedicated `GasViscosityCalculator.CalculateLeeGonzalezEakin()` method.

### M-16: No Formal State Machine Library
**Project:** Beep.OilandGas.LifeCycle
**Issue:** Field, Well, Reservoir lifecycle state machines use manual `Dictionary<string, List<string>>` transitions.
**Fix:** Use Stateless, Automatonymous, or a simple custom state machine library with better visualization and testing.

### M-17: Reflection-Based Entity Property Access
**Project:** Beep.OilandGas.LifeCycle
**Files:** Multiple services
**Issue:** Methods like `GetStringValue`, `GetDateTimeValue`, `SetPropertyViaReflection` use runtime reflection.
**Fix:** Use strongly-typed PPDM models or generated accessors (source generators).

### M-18: In-Memory-Only Data in ProductionAccounting Compatibility Layer
**Project:** Beep.OilandGas.ProductionAccounting
**Issue:** Lease, Storage, Trading compatibilities use ConcurrentDictionary without DB persistence.
**Fix:** Migrate to PPDM table persistence.

### M-19: Hardcoded Connection Name String "PPDM39"
**Issue:** Hardcoded in 5+ controllers and 50+ services.
**Fix:** Centralize in `ConnectionConstants.DefaultConnectionName` or configuration.

### M-20: Dual-Target Framework Build Artifacts
**Projects:** Multiple
**Issue:** obj/ directories contain net8.0, net9.0, and net10.0 artifacts. csproj only targets net10.0.
**Fix:** Clean build artifacts. Consolidate on net10.0.

### M-21: MudBlazor CSS Loading — No Application-Specific CSS Files
**Project:** Beep.OilandGas.Web
**Issue:** All custom styles are inline `<style>` blocks or CSS isolation. No global stylesheet.
**Fix:** Create `app.css` with shared styles. Reduce inline style duplication.

### M-22: PPDMEntityForm's OnValidSubmit/OnInvalidSubmit Not Properly Routed
**Project:** Beep.OilandGas.Web
**File:** `Components/Shared/PPDMEntityForm.razor`
**Issue:** `@onsubmit` passes event args but `@code` declares parameterless `EventCallback`.
**Fix:** Add `EditContext` parameter to event callbacks.

### M-23: GenericCrudPage Uses JavaScript eval for File Download
**Project:** Beep.OilandGas.Web
**File:** `Components/Shared/GenericCrudPage.razor`
**Issue:** `await JS.InvokeVoidAsync("eval", ...)` creates blob download — fragile pattern.
**Fix:** Use `IJSObjectReference` with a proper `downloadFile` JavaScript function.

### M-24: Type Conversion Logic Duplicated Across 4 Components
**Project:** Beep.OilandGas.Web
**Files:** PPDMEntityForm, GenericCrudPage, ImportCsvWizard, ImportCsvDialog
**Issue:** Each has its own `ConvertValue` logic.
**Fix:** Extract to shared `ValueConverter` service.

### M-25: $150/ft Well Plugging Cost Is Hardcoded
**Project:** Beep.OilandGas.Decommissioning
**Fix:** Make configurable by jurisdiction and well type.

### M-26: 82 Process Definitions Not Registered in DI
**Project:** Beep.OilandGas.LifeCycle
**Issue:** The `ProcessDefinitionInitializer` creates 82 process definitions but is not registered via DI. Must be called manually.
**Fix:** Register as a hosted service or module seed step.

### M-27: PipelineAnalysis Data Management Methods All Commented Out
**Project:** Beep.OilandGas.PipelineAnalysis
**Issue:** Repository calls are commented out. Data management methods return sample data.
**Fix:** Uncomment and wire up to PPDM persistence.

### M-28: No PPDM Persistence for Development Planning Analysis Results
**Project:** Beep.OilandGas.DevelopmentPlanning
**Issue:** Advanced analysis methods return in-memory objects that vanish after the request.
**Fix:** Save analysis results to appropriate PPDM tables.

### M-29: PPDM39.Branchs Tree Is Hardcoded — Not Database-Introspected
**Project:** Beep.OilandGas.Branchs
**Issue:** 700+ table names hardcoded in PPDM39TableMapping. Tree does not reflect actual database.
**Fix:** Build tree from PPDMMetadataRepository at runtime. Use hardcoded as fallback.

### M-30: Deprecated Stubs Left in Source
**Project:** Beep.OilandGas.Decommissioning
**Files:** IWellPluggingService.cs, WellPluggingService.cs, WellPluggingService.Advanced.cs
**Fix:** Remove empty stubs or mark with [Obsolete] and provide migration path.

### M-31: Permit Application Duplicate Columns
**Project:** Beep.OilandGas.PermitsAndApplications
**File:** Data/PermitsAndApplications/Tables/INJECTION_PERMIT_APPLICATION.cs
**Issue:** `MAX_INJECTION_PRESSURE` and `MAXIMUM_INJECTION_PRESSURE` (same for RATE) backed by separate fields.
**Fix:** Settle on one naming convention. Migrate data.

### M-32: WellServices Does Not Implement IWellStatusService
**Project:** Beep.OilandGas.PPDM39.DataManagement
**Issue:** `IWellStatusService` interface exists in `Repositories/IWellStatusService.cs` but WellServices does not implement it.
**Fix:** Either implement the interface or remove it.

### M-33: Mixed Logging Frameworks — Serilog static vs ILogger<T>
**Project:** Beep.OilandGas.PPDM39.DataManagement
**Issue:** Several services use `Serilog.Log` static methods instead of injected `ILogger<T>`.
**Fix:** Standardize on `ILogger<T>` DI injection.

### M-34: Template Path Resolution Fragility
**Project:** Beep.OilandGas.PPDM39.DataManagement
**Issue:** `PPDMReferenceDataSeeder.GetTemplatePath()` uses hardcoded `../../../` relative paths.
**Fix:** Use `IHostEnvironment.ContentRootPath` or embedded resources.

### M-35: No CancellationToken Forwarding in Seeding
**Project:** Beep.OilandGas.PPDM39.DataManagement
**Issue:** Long-running methods (dummy data generation, demo database creation) accept but don't forward CancellationToken.
**Fix:** Forward to all inner repository calls.

---

## 🟢 LOW — Cleanup, Optimization, Polish

### L-1: Remove Empty/Duplicate Project — UserManagement.AspNetCore
**Project:** Beep.OilandGas.UserManagement.AspNetCore
**Issue:** Zero source code files. Only build artifacts remain.
**Fix:** Remove from solution or recreate with actual implementation.

### L-2: Remove Dead/Redirect Pages from Web
**Files:** Counter.razor, FetchData.razor, Data/Audit.razor, wells/Wellbores.razor, wells/Logs.razor, WellDetails/Wellbores.razor, WellDetails/Logs.razor
**Fix:** Remove or fully implement.

### L-3: GlobalUsings.cs Files That Import Only One Namespace
**Project:** Beep.OilandGas.LifeCycle
**Fix:** Remove and use regular using directive.

### L-4: Consolidate Micro-Directories in Models
**Project:** Beep.OilandGas.Models
**Issue:** Directories with 1-3 files: General, LeaseManagement, WellSourceMapping, CompressorAnalysis (empty).
**Fix:** Consolidate into parent directories where logical.

### L-5: Standardize on Record vs Class for DTOs
**Project:** ALL
**Issue:** Newer DTOs use `record`, older use `class` with backing fields. Inconsistent.
**Fix:** Adopt `record` for pure data transfer objects. Use `class : ModelEntityBase` for persistence entities.

### L-6: Remove Commented-Out HeatMap Color Schemes
**Project:** Beep.OilandGas.HeatMap
**File:** HeatMapGenerator.cs
**Issue:** `FromRedtoGreen()`, `FromRedtoBlue()` methods commented out. Default only interpolates Gray→Black.
**Fix:** Either enable the color schemes or remove dead code.

### L-7: Fix Zoom Level Inconsistency in HeatMap
**Project:** Beep.OilandGas.HeatMap
**File:** HeatMapGenerator.cs lines 274, 286
**Issue:** Zoom in = 99% per step. Zoom out = 9% per step. 100x asymmetry.
**Fix:** Use consistent symmetric zoom factor (e.g., `* / 1.2` and `/ 1.2`).

### L-8: Add XML Documentation to Undocumented Public APIs
**Priority:** Low — incremental over time
**Scope:** ALL projects — many public methods lack XML doc comments.

### L-9: Remove HEAT_MAP_CONFIGURATION Property Aliases
**Project:** Beep.OilandGas.HeatMap
**Issue:** Both camelCase and UPPERCASE property duplicates (e.g., `UseInterpolation` / `USE_INTERPOLATION`).
**Fix:** Settle on one convention. Remove aliases.

### L-10: Centralize BOE Conversion Factor
**Project:** Beep.OilandGas.ProductionAccounting
**Issue:** `ConvertProductionToBOE` uses hardcoded `6m` instead of `BoeConversionFactors.StandardMcfPerOilBarrelEquivalent`.
**Fix:** Use the constant from the same project.

### L-11: Remove Duplicate LangVersion/ImplicitUsings in csproj
**Projects:** Multiple (UserManagement, others)
**Fix:** Clean up csproj files.

### L-12: Standardize on ConnectionName Constant
**Fix:** Create `Beep.OilandGas.Models.Constants.ConnectionNames.PPDM39 = "PPDM39"`.

### L-13: Add EditorConfig for Consistent Code Style
**Fix:** Create `.editorconfig` with naming, formatting, and analysis rules.

### L-14: Enable Nullable in All Projects Without It
**Fix:** Add `<Nullable>enable</Nullable>` to any csproj missing it. Fix warnings.

### L-15: Remove AllocationPatchEndpoint That Does Nothing
**Project:** Beep.OilandGas.ApiService
**File:** Controllers/Field/ProductionController.cs
**Issue:** `PatchAllocationAsync` returns `NoContent()` without persisting.
**Fix:** Implement or remove.

### L-16: Replace String Interpolation for JSON in Audit
**Project:** Beep.OilandGas.UserManagement
**File:** AuthService.cs line 542
**Issue:** `$"{{\"description\":\"{description}\"}}"` fragile manual JSON.
**Fix:** Use `JsonSerializer.Serialize`.

### L-17: Add Unit Test Projects
**Priority:** Strategic
**Scope:** ALL projects — zero test projects found.
**Fix:** Start with critical calculation projects: FlashCalculations, WellTestAnalysis, NodalAnalysis.

### L-18: Add Performance Benchmarks
**Priority:** Strategic
**Scope:** Engineering calculation projects
**Fix:** Add BenchmarkDotNet projects for hot-path calculations (Z-factor, flash, nodal analysis).

### L-19: Remove ProspectEvaluationService Simple Constructor
**Project:** Beep.OilandGas.ProspectIdentification
**Issue:** Constructor with just `IDMEEditor` will fail at runtime when PPDM methods are called.
**Fix:** Remove or ensure all dependencies are available.

### L-20: Rename RankineToFahrenheit Constant
**Project:** Beep.OilandGas.GasProperties
**File:** Constants/GasPropertiesConstants.cs
**Issue:** Value is `-459.67` but name suggests it's the conversion offset. Misleading.
**Fix:** Rename to `AbsoluteZeroFahrenheit` or `RankineZeroInFahrenheit`.

### L-21: IProspectEvaluationService Is a Marker Interface
**Project:** Beep.OilandGas.ProspectIdentification
**Issue:** Interface with no methods. All contracts are effectively public methods on the implementation.
**Fix:** Add method contracts to the interface or remove it and use concrete type directly.

### L-22: Add OpenTelemetry/Metrics Instrumentation
**Priority:** Strategic
**Fix:** Add OpenTelemetry tracing spanning service boundaries. Add metrics for API latency, DB query counts, calculation performance.

---

## Enhancement Category Summary

| Category | Issues | Priority |
|----------|--------|----------|
| Security | 4 (JWT, SQL injection, CORS, impersonation) | 🔴 CRITICAL |
| Data Integrity | 5 (duplicates, no transactions, field scoping) | 🔴 CRITICAL |
| Calculation Bugs | 3 (Wilson K, dew point, viscosity correction) | 🔴 CRITICAL |
| Architecture Debt | 28 (god controller, large files, compat layers) | 🟠 HIGH |
| Missing Implementations | 20 (stubs, mock data, placeholder endpoints) | 🟠 HIGH |
| Code Duplication | 8 (calculators, material DBs, DTOs) | 🟡 MEDIUM |
| Code Quality | 15 (async/sync, nullable, reflection, thread safety) | 🟡 MEDIUM |
| Maintainability | 12 (DI extensions, file org, XML docs, tests) | 🟡 MEDIUM |
| Cleanup/Polish | 22 (dead code, redirs, conventions, tests) | 🟢 LOW |

---

## Recommended Implementation Sequence

### Phase 1 (Weeks 1-2): Critical Security and Data Integrity
1. Fix C-1: Implement server-side refresh token storage
2. Fix C-2: Parameterize SQL in RowLevelSecurityService
3. Fix C-3: Fix Wilson K-value formula in FlashCalculator
4. Fix C-4: Fix UserManagementService duplicate creation
5. Fix C-6: Restrict CORS for production
6. Fix C-7: Remove X-User-Id header acceptance
7. Fix C-9: Add transactional boundaries to production cycle
8. Fix C-10: Remove sync-over-async from compatibility layer

### Phase 2 (Weeks 3-4): Critical Bugs and Architecture Foundation
1. Fix C-5, C-8, C-11, C-12
2. Fix H-19 (DataManager resume)
3. Fix H-20 (duplicate module registrations)
4. Implement H-12 (global exception handling)
5. Fix H-24, H-25 (async and deadlock issues)

### Phase 3 (Weeks 5-8): Architecture Debt
1. Decompose H-1 (god controller)
2. Address H-2 (5.8MB metadata file)
3. Consolidate H-10 (dual lease services)
4. Resolve H-11 (dual user schema)
5. Fix H-26 (EOR substring matching)
6. Implement H-27 (DI extension method)

### Phase 4 (Weeks 9-16): Missing Implementations
1. Implement H-4 (HydraulicPumps)
2. Implement H-5 (PlungerLift)
3. Fix H-6 (DevelopmentPlanning mock data)
4. Fix H-7 (LeaseAcquisition mock data)
5. Implement H-8 (ProductionOperations stubs)
6. Implement H-22 (API 11L)
7. Fix H-23 (HI 9.6.7 viscosity correction)

### Phase 5 (Weeks 17-24): Code Quality and Optimization
1. Address M-1 through M-35 (code duplication, consistency)
2. Address L-1 through L-22 (cleanup, polish)
3. Add L-17 (unit tests)
4. Add L-18 (benchmarks)
5. Add L-22 (telemetry)

---

**Document Version:** 1.0
**Generated:** 2026-06-27
**Methodology:** Every .cs file read — findings traceable to specific files and line numbers
