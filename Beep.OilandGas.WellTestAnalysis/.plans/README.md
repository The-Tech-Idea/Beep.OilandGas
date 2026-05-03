# Beep.OilandGas.WellTestAnalysis — plan index

Phased work for **pressure transient analysis (PTA)**, **build-up / draw-down**, **derivative diagnostics**, **gas pseudo-pressure**, **type curves**, **Skia rendering**, **validation**, **PPDM-aligned models** (`WELL_TEST_*` in **`Beep.OilandGas.Models`**), and **`IWellTestAnalysisService`** / **`WellTestAnalysisService`**. Rollup: **[`../MASTER-TODO-TRACKER.md`](../MASTER-TODO-TRACKER.md)**.

| Phase | Document | Focus |
|-------|----------|--------|
| 0 | [00_WellTestAnalysis_Overview_And_Baseline.md](00_WellTestAnalysis_Overview_And_Baseline.md) | Scope, inventory, baseline gaps |
| 1 | [01_Calculations_Validation_Services_And_PPDM_Alignment.md](01_Calculations_Validation_Services_And_PPDM_Alignment.md) | Math layer, validator, service vs analyzer, units |
| 2 | [02_Tests_Verification_And_Golden_Vectors.md](02_Tests_Verification_And_Golden_Vectors.md) | Unit tests, synthetic Horner/derivative checks |
| 3 | [03_Documentation_Packaging_And_Consumers.md](03_Documentation_Packaging_And_Consumers.md) | README, package readme, API/Web, namespace drift |
| 4 | [04_Industry_Scenarios_Oil_Gas_WellTest_Best_Practices.md](04_Industry_Scenarios_Oil_Gas_WellTest_Best_Practices.md) | O&G PTA scenarios, applicability, boundaries |
| Run | [08_Consolidated_Execution_Checklist.md](08_Consolidated_Execution_Checklist.md) | Build / test commands |
