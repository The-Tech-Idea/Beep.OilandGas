# Phase 4 — API, Web, and LifeCycle orchestration

## Goal

Expose **analysis and operations** consistently: **`Models.Core.Interfaces.IEnhancedRecoveryService`** for the **core** contract, **field-scoped** routes where other features use **`/api/field/current/*`**, and **optional** advanced analytics endpoints with **versioned** DTOs.

## Current API surface (baseline)

| HTTP | Route (controller base **`/api/EnhancedRecovery`**) | Service method |
|------|-----------------------------------|----------------|
| POST | **`analyze-eor`** | **`AnalyzeEORPotentialAsync`** |
| POST | **`recovery-factor`** | **`CalculateRecoveryFactorAsync`** |
| GET | **`injection`** | **`GetInjectionOperationsAsync`** |
| POST | **`economics`** | **`AnalyzeEOReconomicsAsync`** |
| POST | **`injection`** | **`ManageInjectionAsync`** |

**Gap**: **`EnhancedRecoveryService.Advanced.cs`** (**waterflood**, **gas**, **chemical**, **thermal**, **compare**, **optimize**, **pressure**) — **no** corresponding controller actions yet.

## Target files

- **`Beep.OilandGas.ApiService/Controllers/Operations/EnhancedRecoveryController.cs`**
- **`Beep.OilandGas.ApiService/Controllers/Field/`** — if adding **`/api/field/current/enhanced-recovery/**`** consistency (**check existing Web client URLs**).
- **`Beep.OilandGas.Web/Services/EnhancedRecovery*.cs`**
- **`Beep.OilandGas.LifeCycle/Services/Production/PPDMProductionService.cs`** — **`PlanEnhancedRecoveryAsync`**, **`ExecuteEnhancedRecoveryAsync`**

## TODO checklist

- [ ] Add **advanced analysis** endpoints **or** a single **`POST analyze-screening`** with **`AnalysisKind`** discriminator — prefer **explicit routes** per method family for clearer Swagger (**`/waterflood-performance`**, **`/gas-injection`**, …).
- [ ] **`OperationCanceledException`** — rethrow from controller actions (match **Compressor** / facilities patterns).
- [ ] **`userId`** — pass **`GetUserId()`** into persistence paths when replacing **`SYSTEM`** audit columns for compliance-sensitive tenants.
- [ ] Align **`EnhancedRecoveryClient`** (`**/api/field/current/enhanced-recovery/**`) with ApiService routes — **single source of truth** for path strings (shared constants class).
- [ ] Document **breaking vs additive** API changes in **`IMPLEMENTATION_SUMMARY`** or module **`README`**.

## Verification criteria

- `dotnet build Beep.OilandGas.ApiService`
- `dotnet build Beep.OilandGas.Web`
- Manual Swagger pass for new routes

## Risks

| Risk | Mitigation |
|------|------------|
| Controller bloat | Split **`EnhancedRecoveryController`** into **Operations** vs **Analytics** partial classes |
