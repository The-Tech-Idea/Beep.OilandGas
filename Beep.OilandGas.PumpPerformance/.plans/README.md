# Beep.OilandGas.PumpPerformance — plan index

Phased work for **pump curves (H–Q, power, efficiency)**, **affinity laws**, **NPSH**, **system curves**, **ESP design**, **pump-type abstractions**, **validation**, **tests**, **documentation**, and **industry scenario coverage** (surface vs downhole, ESP / centrifugal / PD, multiphase, VSD). Rollup: **[`../MASTER-TODO-TRACKER.md`](../MASTER-TODO-TRACKER.md)**.

| Phase | Document | Focus |
|-------|----------|--------|
| 0 | [00_PumpPerformance_Overview_And_Baseline.md](00_PumpPerformance_Overview_And_Baseline.md) | Scope, inventory, baseline gaps |
| 1 | [01_Calculators_Services_Validation_And_Contracts.md](01_Calculators_Services_Validation_And_Contracts.md) | Math layer, `PumpPerformanceService`, validators, units, DI |
| 2 | [02_Tests_And_Verification.md](02_Tests_And_Verification.md) | Unit/regression tests, golden vectors, curve-fit checks |
| 3 | [03_Documentation_Packaging_API_Alignment.md](03_Documentation_Packaging_API_Alignment.md) | README, package readme, namespace vs legacy docs, API/Web alignment |
| 4 | [04_Industry_Scenarios_Wells_ESP_Pipeline_Rotating.md](04_Industry_Scenarios_Wells_ESP_Pipeline_Rotating.md) | O&G operating contexts, applicability, boundaries vs other modules |
| Run | [08_Consolidated_Execution_Checklist.md](08_Consolidated_Execution_Checklist.md) | Build / test commands |
