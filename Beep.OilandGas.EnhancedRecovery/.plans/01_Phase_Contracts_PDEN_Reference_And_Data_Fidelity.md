# Phase 1 — Contracts, PDEN conventions, and data fidelity

## Goal

Align **service contracts**, **PDEN + flow measurement** persistence rules, and **identifier** handling so EOR operations are auditable, queryable, and consistent across API, LifeCycle, and Web — without breaking **`Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService`**.

## Target files

- **`Beep.OilandGas.Models/Core/Interfaces/IEnhancedRecoveryService.cs`** — extend only when adding **cross-library** surface area.
- **`Beep.OilandGas.EnhancedRecovery/Services/IEnhancedRecoveryService.cs`** — **rename** (e.g. **`IEnhancedRecoveryOperationsService`** / **`IEnhancedRecoveryRepository`**) to eliminate duplicate short name; update **`EnhancedRecoveryService`** declaration.
- **`EnhancedRecoveryService.cs`** — PDEN insert/update paths; **`UpsertFlowMeasurementAsync`**; filters (**`AREA_ID`** vs **`FIELD_ID`**).
- **`EnhancedRecoveryService.ModelsCoreImpl.cs`** — **`AnalyzeEORPotentialAsync`**, **`CalculateRecoveryFactorAsync`**, **`ManageInjectionAsync`** behavior vs names.
- **`Beep.OilandGas.ApiService/Program.cs`** — DI registrations after **`AddBeepServices`**.
- **`PPDMProductionService`** (**LifeCycle**) — **`PlanEnhancedRecoveryAsync`** / **`ExecuteEnhancedRecoveryAsync`** alignment.

## Industry-aligned conventions (to implement / verify)

| Topic | Practice | Implementation note |
|-------|----------|---------------------|
| **Voidage / injection balance** | Track injection vs production at **field** or **pattern** level | Prefer **`PDEN`** per scheme + **`PDEN_FLOW_MEASUREMENT`** time series; document **`PRODUCT_TYPE`** (**`WATER`**, **`GAS`**, **`CO2`**, **`POLYMER`** literals aligned with future LOV). |
| **EOR method taxonomy** | Distinct **miscible gas**, **immiscible**, **WAG**, **ASP**, **polymer**, **steam**, **CO₂ storage + EOR** | Map **`ENHANCED_RECOVERY_TYPE`** / **`PDEN_SUBTYPE`** to a **controlled vocabulary** (phase 3 seed or metadata table). |
| **Pilot vs full-field** | Separate **pilot** PDEN or amendment seq | Use **`PDEN`** naming fields + remarks until extension entities exist. |

## TODO checklist

- [x] **Rename** feature-local interface → **`IEnhancedRecoveryOperationsService`** (was colliding with **`Models.Core.Interfaces.IEnhancedRecoveryService`**).
- [x] **`CalculateRecoveryFactorAsync`**: population of **screening** incremental RF (% OOIP) on **`Efficiency`** + **Remarks**; full **volumetric / decline** RF from **`POOL` / `PDEN_VOL_SUMMARY`** — backlog.
- [ ] **Optional**: add **`CalculateRecoveryMetricsAsync`** returning a dedicated projection with **`OOIP`**, **decline RF**, etc., when data integration exists.
- [ ] **`AnalyzeEORPotentialAsync`**: replace “create PDEN if missing” with explicit **screening result** type if requirements demand read-only analysis first (optional projection **`EORScreeningResult`** in **`Models.Data.Calculations`**).
- [ ] Inject **`ILogger<EnhancedRecoveryService>`** via constructor overload **without** breaking existing **`Program.cs`** two-arg ctor — use optional parameter or secondary ctor pattern used elsewhere in solution.
- [ ] **`IPPDM39DefaultsRepository.FormatIdForTable`** for new persisted IDs when table metadata requires formatted keys (if introducing extension tables in phase 3).
- [ ] Document **unit conventions** on **`PDEN_FLOW_MEASUREMENT.FLOW_RATE_OUOM`** (**`BBL/D`**, **`Mcf/d`**, **`tonnes/d`**) and enforce validation before insert.

## Verification criteria

- `dotnet build Beep.OilandGas.EnhancedRecovery`
- `dotnet build Beep.OilandGas.Models`
- `dotnet build Beep.OilandGas.ApiService`
- Smoke: **`POST /api/EnhancedRecovery/analyze-eor`** and **`GET /api/EnhancedRecovery/injection`** unchanged contract unless versioned.

## Risks

| Risk | Mitigation |
|------|------------|
| Renaming feature interface breaks internal callers | Solution-wide grep; keep **`Models.Core.Interfaces.IEnhancedRecoveryService`** stable for API |
| PDEN filter mismatch (**`AREA_ID`** vs **`FIELD_ID`**) | Add integration test with seeded PDEN rows (phase 5) |
