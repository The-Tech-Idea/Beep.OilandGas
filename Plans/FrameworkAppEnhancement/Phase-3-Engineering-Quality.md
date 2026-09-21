# Phase 3: trustworthy engineering calculations

Dependencies: FG-01/20 for persistence contracts and FG-10/11 for application delivery. Responsible areas: LifeCycle calculation orchestration, existing engineering modules, Models/contracts and API clients. Finding: R07; legacy roadmap drift: R10.

| Task | Work | Acceptance |
|---|---|---|
| FG-30 | Trace calculation input units, sources, dates and assumptions through existing request/result persistence. Replace silent required-input defaults with validation or explicit user-selected assumptions. | A saved forecast can be reproduced from its stored input snapshot and algorithm version. The app exposes assumptions and input provenance. Unit conversion round trips and missing/stale input cases are tested. |
| FG-31 | Extend module benchmark and regression coverage before selecting advanced algorithms. | Each released method has a cited benchmark fixture or independently derived oracle, documented applicability, tolerance, convergence/failure behavior and boundary tests. No inferred accuracy percentage. |
| FG-32 | Add scenario comparison and uncertainty reporting to existing calculation/result workflows. | Scenarios share a traceable baseline; deterministic seeds reproduce stochastic runs; chart/export show units, method, assumptions and run status consistently. |

## Prioritized module backlog

| Existing owner | First verification/enhancement slice | Later candidate after evidence |
|---|---|---|
| ProductionForecasting including DCA | Validate existing Arps implementation, time/rate units, zero decline and economic-limit handling; connect observed production to persisted forecast. | Fitting diagnostics and scenario envelopes using existing DCA infrastructure. |
| NodalAnalysis, PipelineAnalysis, ChokeAnalysis | Check valid ranges, bracket/convergence failures and consistent pressure/rate references across modules. | Compare interventions and operating constraints through a common persisted scenario. |
| FlashCalculations, GasProperties, OilProperties | Verify composition normalization, phase limits, pressure/temperature units and correlation applicability. | Multi-condition PVT comparison with method provenance. |
| GasLift, HydraulicPumps, PumpPerformance, SuckerRodPumping, PlungerLift, CompressorAnalysis | Verify operating-envelope and invalid-input reporting against module fixtures. | Multi-option lift/equipment selection using constrained comparisons, not unexplained scores. |
| WellTestAnalysis | Verify time ordering, shut-in assumptions, noisy/degenerate data and existing interpretation output. | Additional interpretations only after benchmark sources and applicability are documented. |
| EnhancedRecovery, EconomicAnalysis | Trace physical scenarios into costs and economic assumptions; verify cash-flow period/currency conventions. | Sensitivity and uncertainty views tied to the same scenario versions. |

Do not create a new DCA project or a parallel result repository. Reuse `PPDMCalculationService`, existing mappers and each module's calculation service; evaluate whether existing result contracts can carry provenance before adding fields under the approved owner.

## Verification

Run the relevant existing module test project and API mapping tests per slice. Include dimensionally equivalent unit inputs, nonphysical inputs, empty and out-of-order series, solver nonconvergence, repeatability, and save/load preservation. Benchmark fixtures must document source and expected tolerance when introduced; this plan does not invent petroleum reference results. Independent engineering review is required before labeling new algorithms validated for operational decisions.

Exit requires a complete pilot forecast journey: production data -> validated inputs -> calculation -> persistence -> reload -> comparison/export. Source implementation and benchmark verification remain separate tracker evidence.
