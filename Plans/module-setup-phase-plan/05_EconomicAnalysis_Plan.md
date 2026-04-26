# EconomicAnalysis ModuleSetup Plan

## Current state
- Module file: Beep.OilandGas.EconomicAnalysis/Modules/EconomicsModule.cs
- ModuleSetupBase exists; EntityTypes intentionally empty.

## Target state
- EconomicsModule should register only EconomicAnalysis-owned persisted tables.
- Contracts and projections remain excluded from EntityTypes.
- SeedAsync restricted to economics/project-local defaults.

## Phase tasks
- [x] Phase 1: define economics schema ownership list.
- [x] Phase 2: validate Data/Projections remain non-table classes.
- [x] Phase 3: confirm no local table classes exist and keep EntityTypes empty.
- [x] Phase 4: keep seed scope local (current skip reason is explicit and idempotent).
- [x] Phase 5: build validation completed.
- [ ] Phase 6: add ownership documentation and CI checks.

## Audit snapshot
- No local persisted table classes found in project.
- `Data/Projections` classes are intentionally excluded from EntityTypes.

---

## SP-A — Domain Audit (Standards Applied)

**Standards referenced**: SPE PRMS 2018 §6 (economic evaluation), SPE PRMS §2.1 (reserves categories), SPEE (Society of Petroleum Evaluation Engineers) guidelines §4 (NPV10 convention), SEC Reg S-X Rule 4-10 (discount rate disclosure).

**Project shape**: No persisted table classes exist. All `Data/Projections` classes are projection/result types — correctly excluded from `EntityTypes`. SP-B focuses on projection class completeness against O&G standards.

### Audit Findings Matrix

| Class | Gap identified | Standard violated |
|---|---|---|
| `EconomicAnalysisResult` | Missing `DISCOUNT_RATE` (rate used for NPV — must be disclosed), `NPV10` (NPV at 10% — SPEE standard benchmark), `CURRENCY` (ISO 4217 for all monetary values), `RESERVES_CATEGORY` (1P/2P/3P linkage per SPE PRMS §2.1) | SPE PRMS §6.2; SPEE guideline §4; SEC Reg S-X Rule 4-10 |
| `ProjectMetrics` | Uses `double` for NPV, IRR, PaybackPeriod — financial precision risk (noted, type not changed due to service impact). Missing `DISCOUNT_RATE`, `NPV10`, `EVALUATION_DATE`, `CURRENCY` | SPE PRMS §6.2; SPEE guideline §4 (NPV10 as standard ranking metric) |
| `PriceScenario` | Missing `ScenarioName` (LOW/BASE/HIGH — PRMS §6.2 price deck convention), `CommodityType` (OIL/GAS/NGL/CONDENSATE), `PriceUnit` (USD/BBL, USD/MCF etc.) | SPE PRMS §6.2 (sensitivity analysis must identify commodity and unit) |

### Technical debt noted (not changed)
- `EconomicAnalysisResult`: `CalculationId` / `CalculationDate` are intentional alias shims sharing backing fields with `AnalysisId` / `AnalysisDate`. Functional but potentially confusing. Documented — no action taken.
- `ProjectMetrics`: `double` for financial metrics (NPV, IRR) — preferred is `decimal` for precision. Breaking change deferred; noted as technical debt.

---

## SP-B — Code Revisions

| File | Changes made |
|---|---|
| `Data/Projections/EconomicAnalysisResult.cs` | Added: `DISCOUNT_RATE` (decimal), `NPV10` (decimal?), `CURRENCY` (string, default "USD"), `RESERVES_CATEGORY` (string?) per SPE PRMS §6.2. |
| `Data/Projections/ProjectMetrics.cs` | Added: `DISCOUNT_RATE` (double), `NPV10` (double), `EVALUATION_DATE` (DateTime), `CURRENCY` (string, default "USD") per SPE PRMS §6.2 / SPEE §4. |
| `Data/Projections/PriceScenario.cs` | Added: `ScenarioName` (string, default "BASE"), `CommodityType` (string, default "OIL"), `PriceUnit` (string, default "USD/BBL") per SPE PRMS §6.2 price-deck convention. |

---

## SP-C — EntityTypes Module

- Module file: `Modules/EconomicsModule.cs`
- EntityTypes remains intentionally empty — no persisted table classes in this project.
- No changes to module registration.

---

## Build Result

```
dotnet build Beep.OilandGas.EconomicAnalysis.csproj -v q
Build succeeded. 0 Warning(s). 0 Error(s).
```

