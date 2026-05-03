# Phase 2 Data Access and Seed Strategy

## Objective
Define PPDM-first persistence boundaries, class-first schema strategy, minimal table contracts, and idempotent reference seeding for EconomicAnalysis.

## Target Files
- `Beep.OilandGas.EconomicAnalysis/Services/EconomicAnalysisService.cs`
- `Beep.OilandGas.EconomicAnalysis/Services/EconomicAnalysisService.Advanced.cs`
- `Beep.OilandGas.EconomicAnalysis/Modules/EconomicsModule.cs`
- `Beep.OilandGas.EconomicAnalysis/Constants/EconomicAnalysisReferenceCodes.cs` (if introduced)
- `Beep.OilandGas.EconomicAnalysis/Constants/EconomicAnalysisReferenceCodeSeed.cs` (if introduced)
- `Beep.OilandGas.EconomicAnalysis/Data/Tables/*` (if table expansion is approved)

## PPDM-First Data Access Strategy

### Canonical Persisted Table (Current)
- `ECONOMIC_ANALYSIS_RESULT`
  - Use `PPDMGenericRepository` for insert/get.
  - Keep this as canonical economic analysis result header persistence.

### Candidate Persisted Tables (Future Expansion)
- `ECONOMIC_CASH_FLOW` for persisted scenario cash flow rows.
- `NPV_PROFILE_POINT` persistence only if profile history retention is required.

### Projection-Only Shapes (Remain Non-Table)
- All classes in `Beep.OilandGas.EconomicAnalysis/Data/Projections/*` remain projections/results unless explicitly promoted.
- Do not pass projection classes to `InsertAsync`/`UpdateAsync`.

## Table vs Projection Contract
- Table classes:
  - Extend `ModelEntityBase`.
  - Scalar columns only.
  - Live under `Data/Tables/`.
- Projection classes:
  - Can include nested/collection structures.
  - Live under `Data/Projections/`.
  - Never passed directly to repository write operations.

## Class-First Schema Rule
- No manual SQL scripts.
- Add/modify C# table classes first; setup pipeline materializes schema.
- Any new table addition must be registered by module setup `EntityTypes`.

## Minimal Required Column Contracts

### `ECONOMIC_ANALYSIS_RESULT` (required)
- `ANALYSIS_ID` (PK/business key)
- `ANALYSIS_DATE`
- `NPV`
- `IRR`
- `PAYBACK_PERIOD`
- `DISCOUNT_RATE`
- `ACTIVE_IND`

### `ECONOMIC_CASH_FLOW` (if introduced)
- `ANALYSIS_ID` (FK/logical join to result header)
- `PERIOD_NO`
- `CASH_FLOW_AMOUNT`
- `SCENARIO_TYPE`
- `DISCOUNT_RATE` (optional but recommended for auditability)
- `ACTIVE_IND`

### `NPV_PROFILE_POINT` (if introduced)
- `ANALYSIS_ID`
- `DISCOUNT_RATE`
- `NPV_VALUE`
- `POINT_ORDER`
- `ACTIVE_IND`

## Seed Strategy

### Current State
- `EconomicsModule.SeedAsync` delegates to shared accounting seed routines.
- No module-specific reference constants/seed catalog yet.

### Planned Canonical Pattern
- Add module-local constants and seed catalog when EconomicAnalysis-owned reference families are confirmed.
- Use idempotent upsert pattern by natural keys (`REFERENCE_SET`, `REFERENCE_CODE` style).
- Keep seed operations cancellation-safe and deterministic.

## Seed Family Coverage Checklist
- Analysis status codes
- Scenario types (`BASE`, `BEST`, `WORST`, `STRESS`)
- Valuation model types (`DCF`, `MONTE_CARLO`, `REAL_OPTIONS`, `DECISION_TREE`)
- Risk bands (`LOW`, `MEDIUM`, `HIGH`)
- Sensitivity parameter types (price, discount, capex, opex, volume)
- Recommendation classes (`PROCEED`, `HOLD`, `REVISE`, `STOP`)

## Verification Checklist (Phase 2)
- [ ] Every persisted table in scope has a minimal required column contract.
- [ ] Every projection-only class is explicitly documented as non-persistent.
- [ ] Any added table class is included in `EconomicsModule.EntityTypes`.
- [ ] Seed family catalog covers all approved reference families.
- [ ] Seed rerun path is idempotent by natural keys.

## Exit Criteria for Phase 2
- Persisted vs projection boundaries explicitly documented.
- Minimal required contracts defined for current and candidate PPDM tables.
- Seed strategy and coverage families defined for idempotent implementation.
