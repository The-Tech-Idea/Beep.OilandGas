# MASTER-TODO-TRACKER — Oil & Gas facility + production operations

Single tracker for facility management and ProductionOperations service alignment. Each phase has a dedicated doc under `Plans/Phases/`.

## Phase index

| Phase | Doc | Goal |
|-------|-----|------|
| 1 | [Plans/Phases/ProductionOperations-Facility-Phase1-Services.md](Plans/Phases/ProductionOperations-Facility-Phase1-Services.md) | PPDM-native facility service, delegation from production operations, production management queries + cancellation |
| 2 | [Plans/Phases/ProductionOperations-Facility-Phase2-API-Verification.md](Plans/Phases/ProductionOperations-Facility-Phase2-API-Verification.md) | Facility API controllers, full solution build, integration tests |

## Current status (summary)

- Phase 1: **Mostly complete** — services, DI order, docs, and `Beep.OilandGas.ProductionOperations` build are done.
- Phase 2: **Controllers done** — facility HTTP surface is in `ApiService/Controllers/Facility/`. **Full ApiService build** still blocked on `Beep.OilandGas.PermitsAndApplications` compile errors; integration tests pending after Permits is green.

## Related feature trackers

| Feature | Tracker |
|---------|---------|
| Compressor analysis (extension tables, LOV seed, API/orchestration) | [Beep.OilandGas.CompressorAnalysis/MASTER-TODO-TRACKER.md](Beep.OilandGas.CompressorAnalysis/MASTER-TODO-TRACKER.md) |
| Enhanced oil recovery (PDEN, screening analytics, API/Web) | [Beep.OilandGas.EnhancedRecovery/MASTER-TODO-TRACKER.md](Beep.OilandGas.EnhancedRecovery/MASTER-TODO-TRACKER.md) |
| Flash / PVT (EOS LOVs, `R_FLASH_CALCULATION_REFERENCE_CODE`, module) | [Beep.OilandGas.FlashCalculations/MASTER-TODO-TRACKER.md](Beep.OilandGas.FlashCalculations/MASTER-TODO-TRACKER.md) |
| Gas lift (reference LOVs, `R_GAS_LIFT_REFERENCE_CODE`, module) | [Beep.OilandGas.GasLift/MASTER-TODO-TRACKER.md](Beep.OilandGas.GasLift/MASTER-TODO-TRACKER.md) |
| Gas properties (Z-factor, viscosity, pseudo-pressure, `IGasPropertiesService`) | [Beep.OilandGas.GasProperties/MASTER-TODO-TRACKER.md](Beep.OilandGas.GasProperties/MASTER-TODO-TRACKER.md) |
| Hydraulic pumps (jet/piston calculators, `IHydraulicPumpService`) | [Beep.OilandGas.HydraulicPumps/MASTER-TODO-TRACKER.md](Beep.OilandGas.HydraulicPumps/MASTER-TODO-TRACKER.md) |
| Oil properties (black-oil correlations, `IOilPropertiesService`) | [Beep.OilandGas.OilProperties/MASTER-TODO-TRACKER.md](Beep.OilandGas.OilProperties/MASTER-TODO-TRACKER.md) |
| Pump performance (H–Q, NPSH, ESP, `IPumpPerformanceService`) | [Beep.OilandGas.PumpPerformance/MASTER-TODO-TRACKER.md](Beep.OilandGas.PumpPerformance/MASTER-TODO-TRACKER.md) |
| Well test / PTA (Horner, MDH, derivative, `IWellTestAnalysisService`) | [Beep.OilandGas.WellTestAnalysis/MASTER-TODO-TRACKER.md](Beep.OilandGas.WellTestAnalysis/MASTER-TODO-TRACKER.md) |
