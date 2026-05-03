# Phase 0 — EnhancedRecovery overview and baseline

## Purpose

Anchor **scope**, **current behavior**, and **known gaps** before changing contracts, calculations, or schema. Enhanced recovery spans **waterflooding**, **gas injection (miscible / immiscible / WAG)**, **chemical EOR**, **thermal recovery**, **CO₂ / CCUS-oriented miscible schemes**, and **economics / screening** — implementation must stay aligned with PPDM entities and Beep layering rules.

## Repository inventory (summary)

| Area | Location | Notes |
|------|-----------|--------|
| Shared contract | **`Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService`** | Analyze EOR, recovery factor, economics, injection list, manage injection |
| Feature-local interface | **`Beep.OilandGas.EnhancedRecovery.Services.IEnhancedRecoveryService`** | **Additional** CRUD-style methods (**`GetEnhancedRecoveryOperationsAsync`**, water/gas flood lists, etc.) — **name collision** with Models interface on the same implementing class; resolve in phase 1 |
| Implementation | **`EnhancedRecoveryService.cs`** (partial) | **`PDEN`**, **`PDEN_FLOW_MEASUREMENT`**, **`WELL`**, **`FIELD`** via **`UnitOfWork`** |
| Analytics | **`EnhancedRecoveryService.Advanced.cs`** | Waterflood, gas injection, chemical, thermal, comparison, optimization, pressure, **economics** — **not** all exposed on **`IEnhancedRecoveryService`** or HTTP |
| Models bridge | **`EnhancedRecoveryService.ModelsCoreImpl.cs`** | Explicit **`Models.Core.Interfaces.IEnhancedRecoveryService`** implementations |
| API | **`ApiService/Controllers/Operations/EnhancedRecoveryController.cs`** | **`analyze-eor`**, **`recovery-factor`**, **`injection`**, **`economics`**, **`injection` POST** |
| Web | **`Beep.OilandGas.Web`** — **`EnhancedRecoveryClient`**, **`EnhancedRecoveryServiceClient`** | Subset of operations |

## PPDM mapping (baseline)

- **Primary entity**: **`PDEN`** — **`ENHANCED_RECOVERY_TYPE`**, **`PDEN_SUBTYPE`**, **`AREA_ID`/`FIELD_ID`**, **`CURRENT_WELL_STR_NUMBER`**, status dates.
- **Rates**: **`PDEN_FLOW_MEASUREMENT`** — **`INJECTION_RATE`**, **`FLOW_RATE_OUOM`**, **`PRODUCT_TYPE`**, **`MEASUREMENT_TYPE`**.
- **Classification heuristics** (current code): **`INJECTION`** vs other **`ENHANCED_RECOVERY_TYPE`** values; **`WATER_FLOOD`**, **`GAS_INJECTION`** filtered by **`PDEN_SUBTYPE`**.

## Known gaps (baseline)

| Gap | Impact | Phase |
|-----|--------|--------|
| **Advanced** analysis methods exist only on concrete service | No stable API/Web contract for waterflood/gas/chemical/thermal analytics | 2 / 4 |
| Duplicate interface name **`IEnhancedRecoveryService`** (Models vs feature namespace) | Confusion for DI, tests, and new contributors | 1 |
| **`CalculateRecoveryFactorAsync`** returns **`EnhancedRecoveryOperation`** without computed RF fields from production history | Misleading name vs petroleum engineering expectation | 1 / 2 |
| Economics / analytics use **simplified** correlations and placeholders | Acceptable for POC; must document assumptions and tighten for production decisions | 2 / 6 |
| Optional **reference LOV** for EOR methods may be inconsistent across installs | Screening comparisons and reporting drift | 3 |
| Limited automated tests dedicated to **EnhancedRecovery** | Regression risk when refactoring PDEN mapping | 5 |

## Governance checklist (every change set)

- [ ] Shared cross-library API changes live on **`Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService`** (or new types in **`Models.Data`** — no **`DTO`** namespaces).
- [ ] No **`ExecuteSql`** for application SELECT paths — use **`UnitOfWork`** / **`PPDMGenericRepository`** patterns per **CLAUDE.md**.
- [ ] **`OperationCanceledException`** — preserve cancellation through API boundaries (align with other calculators/controllers).

## Exit criteria (phase 0)

- [x] Inventory matches repository layout (refresh when adding files).
- [ ] Stakeholders agree priority: **contracts + PDEN fidelity (1)** vs **calculation hardening (2)** vs **API exposure (4)** vs **tests (5)**.
