# Nodal Analysis Master Tracker

## Phase Rollup

- [x] Phase 0 - Canonical refresh overview documented (`.plans/00_*.md`)
- [x] Phase 1 - Service contracts clarified (Models.Core vs feature implementation)
- [x] Phase 2 - Data and calculation surface hardened
- [x] Phase 3 - API hardening completed (`Nodal` controller paths)
- [x] Phase 4 - Tests and verification completed
- [x] Phase 5 - PPDM and reference alignment completed
- [x] Phase 6 - Advanced engine backlog documented

## Active TODOs

- [x] Add phased `.plans` docs and this tracker.
- [x] Document canonical contract intent in `INodalAnalysisService` remarks.
- [x] Remove/replace placeholder optimization recommendation text.
- [x] Add nodal controller regression tests with strict mocks.
- [x] Add nodal numeric edge-case vectors for intersection/no-intersection paths.
- [x] Verify PPDM persistence behavior and audit columns for nodal save flows.
- [x] Record build and test outcomes below.

## Verification Outcomes

- [x] `dotnet build Beep.OilandGas.sln` (succeeded)
- [x] `dotnet build Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj` (succeeded)
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~Nodal"` (Passed: 70, Failed: 0) — legacy **`CalculationsController`** nodal **`OperationCanceledException`** rethrow tests (`LegacyPerformNodal_Rethrows_WhenCanceled`, GET-by-id, GET list); **`Optimize_Rethrows_WhenCanceled`**; **`GetHistory_Rethrows_WhenCanceled`**; **`SaveResult`** paths; legacy POST null-body
- [x] `INodalAnalysisService` + six diagnostic HTTP routes; `NodalAnalysisApiRequests.cs`; history newest-first; save requires Well UWI; forecast months validation; `RunNodalAnalysisAsync` formatted IDs
- [x] **`Beep.OilandGas.Client`** — `ICalculationsService` XML docs reference **`NodalAnalysisHttpRoutes`**; history uses `IsNullOrWhiteSpace` + “Well UWI” message; API parity unchanged
- [x] **`Beep.OilandGas.Web`** — `CalculationServiceClient` nodal validation + **`ICalculationServiceClient`** XML summaries (`NodalAnalysisHttpRoutes`); **`NodalAnalysis.razor`** uses `IsNullOrWhiteSpace` for well UWI (aligned with API/client); well-load failures logged at Debug
- [x] **`NodalAnalysisHttpRoutes`** — canonical vs legacy **`/api/Calculations/nodal`**; **`CalculationsController`** nodal POST/GET (null body safe, **`ArgumentException`** → client message, **`OperationCanceledException`** rethrows like **`NodalAnalysisController`**) + remarks reference **`/api/nodalanalysis/*`**
- [x] **`NodalAnalysis.razor`** — history reload distinguishes **`ArgumentException`** vs generic failures; **`PerformAnalysis`** handles **`InvalidOperationException`** (e.g. empty API result); **`OperationCanceledException`** suppressed for misleading snackbars / debug logs on navigation cancel (history, analyze/save, **lifecycle header**, **well list** load); save-false / init / disabled-button behavior unchanged
- [x] **`CalculationServiceClient.GetNodalAnalysisHistoryAsync`** — **`ArgumentException`** when well UWI missing (desktop parity)
- [x] **`ApiClient.PostAsync` (bool)** — **`LogWarning`** when HTTP status is not success (helps diagnose nodal **`SaveNodalAnalysisResultAsync`** / other fire-and-forget POST saves)
- [x] **`ApiClient.GetAsync`** — **`LogWarning`** when HTTP status is not success before **`EnsureSuccessStatusCode`** (helps diagnose nodal history **`GET`** failures)
- [x] **`ApiClient`** typed **`PostAsync`**, **`PutAsync`**, **`PatchAsync`**, DELETE-with-body — **`LogWarning`** with status before **`EnsureSuccessStatusCode`** (parity with GET; nodal analyze and related calls)
- [x] **`ApiClient`** bool **`PutAsync`** / **`PatchAsync`** / **`DeleteAsync`**, multipart **`PostAsync`**, **`PostStreamAsync`** — **`LogWarning`** on non-success status (parity with bool POST / GET)
- [x] **`CalculationServiceClient` (nodal)** — rethrow **`OperationCanceledException`** (no error log / no empty history / no false save); null checks for artificial-lift and production-forecast requests
- [x] `dotnet build Beep.OilandGas.Models/Beep.OilandGas.Models.csproj` (succeeded, 0 warnings / 0 errors)
- [x] `dotnet build Beep.OilandGas.NodalAnalysis/Beep.OilandGas.NodalAnalysis.csproj` (succeeded, 0 warnings / 0 errors)
