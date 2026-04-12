# Petroleum Engineer XLS - File to Project Mapping

## Complete File Inventory and Mapping

### Nodal Analysis (8 files) → Enhance `Beep.OilandGas.NodalAnalysis`

| File | Description | Implementation |
|------|-------------|----------------|
| `BottomHoleModalOil-GG.xls` | Bottom hole nodal (Gray-Gray) | Add Gray-Gray correlation |
| `BottomHoleModalOil-HB.xls` | Bottom hole nodal (Hagedorn-Brown) | Enhance existing HB |
| `BottomHoleModalOil-PC.xls` | Bottom hole nodal (Poettmann-Carpenter) | Add PC correlation |
| `WellheadModalOil-GG.xls` | Wellhead nodal (Gray-Gray) | Add wellhead analysis |
| `WellheadModalOil-HB.xls` | Wellhead nodal (Hagedorn-Brown) | Add wellhead analysis |
| `WellheadModalOil-PC.xls` | Wellhead nodal (Poettmann-Carpenter) | Add wellhead analysis |
| `BottomHoleNodalGas.xls` | Bottom hole nodal for gas | Enhance gas support |
| `WellheadNodalGas-SonicFlow.xls` | Wellhead nodal gas (sonic) | Add sonic flow |

---

### Gas Properties (5 files) → New `Beep.OilandGas.Properties`

| File | Description | Implementation |
|------|-------------|----------------|
| `Brill-Beggs-Z.xls` | Z-factor (Brill-Beggs) | Implement Brill-Beggs |
| `Hall-Yarborough-Z.xls` | Z-factor (Hall-Yarborough) | Implement Hall-Yarborough |
| `Carr-Kobayashi-Burrows-GasViscosity.xls` | Gas viscosity | Implement CKB viscosity |
| `PseudoPressure.xls` | Pseudo-pressure | Implement pseudo-pressure |
| `AverageTZ.xls` | Average T & Z | Implement averaging |

---

### Production Forecasting (6 files) → New `Beep.OilandGas.ProductionForecasting`

| File | Description | Implementation |
|------|-------------|----------------|
| `GasWellProductionForecast.xls` | Gas well forecast | Gas forecasting |
| `Pseudo-steady-1phase Production Forecast.xls` | Single-phase forecast | Pseudo-steady 1-phase |
| `Pseudo-steady-2phase Production Forecast.xls` | Two-phase forecast | Pseudo-steady 2-phase |
| `Transient Production Forecast.xls` | Transient forecast | Transient analysis |
| `Pseudosteady1PhaseForecastPlot.xls` | 1-phase plotting | Visualization |
| `Pseudosteady2PhaseForecastPlot.xls` | 2-phase plotting | Visualization |

---

### Artificial Lift - ESP (2 files) → Enhance `Beep.OilandGas.PumpPerformance`

| File | Description | Implementation |
|------|-------------|----------------|
| `ESPdesign-SI Units.xls` | ESP design (SI) | ESP design calculations |
| `ESPdesign-US Field Units.xls` | ESP design (US) | ESP design calculations |

---

### Artificial Lift - Gas Lift (4 files) → New `Beep.OilandGas.GasLift`

| File | Description | Implementation |
|------|-------------|----------------|
| `GasLiftPotential.xls` | Gas lift potential | Potential analysis |
| `GasLiftValveDesign-SI Units.xls` | Valve design (SI) | Valve design |
| `GasLiftValveDesign-US Field Units.xls` | Valve design (US) | Valve design |
| `GasLiftValveSpacing.xls` | Valve spacing | Spacing calculations |

---

### Artificial Lift - Sucker Rod (2 files) → New `Beep.OilandGas.SuckerRodPumping`

| File | Description | Implementation |
|------|-------------|----------------|
| `SuckerRodPumpingLoad.xls` | Rod load calculations | Load analysis |
| `SuckerRodPumpingFlowRate&Power.xls` | Flow rate & power | Performance analysis |

---

### Artificial Lift - Hydraulic Pumps (4 files) → New `Beep.OilandGas.HydraulicPumps`

| File | Description | Implementation |
|------|-------------|----------------|
| `HydraulicJetPump.xls` | Jet pump design | Jet pump calculations |
| `HydraulicPistonPump.xls` | Piston pump design | Piston pump calculations |
| `HydraulicPistonPump-SI Units.xls` | Piston pump (SI) | SI unit support |
| `HydraulicPistonPump-US Field Units.xls` | Piston pump (US) | US unit support |

---

### Artificial Lift - Plunger Lift (1 file) → New `Beep.OilandGas.PlungerLift`

| File | Description | Implementation |
|------|-------------|----------------|
| `PlungerLift.xls` | Plunger lift | Plunger lift calculations |

---

### Compressor Analysis (5 files) → New `Beep.OilandGas.CompressorAnalysis`

| File | Description | Implementation |
|------|-------------|----------------|
| `CentrifugalCompressorPower-SI Units.xls` | Centrifugal (SI) | Centrifugal power |
| `CentrifugalCompressorPower-US Field Units.xls` | Centrifugal (US) | Centrifugal power |
| `ReciprocatingCompressorPower-SI Units.xls` | Reciprocating (SI) | Reciprocating power |
| `ReciprocatingCompressorPower-US Field Units.xls` | Reciprocating (US) | Reciprocating power |
| `CompressorPressure.xls` | Compressor pressure | Pressure calculations |

---

### Well Deliverability (3 files) → Enhance `Beep.OilandGas.NodalAnalysis`

| File | Description | Implementation |
|------|-------------|----------------|
| `Multilateral Oil Well Deliverability.xls` | Multilateral oil | Multilateral support |
| `Multilateral Gas Well Deliverability (C-n IPR).xls` | Multilateral gas (C-n) | C-n IPR method |
| `Multilateral Gas Well Deliverability (Radial-Flow IPR).xls` | Multilateral gas (Radial) | Radial-flow IPR |

---

### BHP Correlations (4 files) → Enhance `Beep.OilandGas.NodalAnalysis`

| File | Description | Implementation |
|------|-------------|----------------|
| `Poettmann-Carpenter BHP.xls` | PC BHP correlation | PC method |
| `HagedornBrownCorrelation.xls` | HB correlation | Enhance existing |
| `Cullender-SmithBHP.xls` | CS BHP for gas | CS method |
| `Guo-GhalamborBHP.xls` | GG BHP correlation | GG method |

---

### Choke Analysis (2 files) → New `Beep.OilandGas.ChokeAnalysis`

| File | Description | Implementation |
|------|-------------|----------------|
| `GasDownChokePressure.xls` | Downhole choke | Downhole calculations |
| `GasUpChokePressure.xls` | Uphole choke | Uphole calculations |

---

### Operating Point (2 files) → Enhance `Beep.OilandGas.NodalAnalysis` or `Beep.OilandGas.PumpPerformance`

| File | Description | Implementation |
|------|-------------|----------------|
| `Operating Point - SI Units.xls` | Operating point (SI) | Operating point analysis |
| `Operating Point - US Field Units.xls` | Operating point (US) | Operating point analysis |

---

### Oil Properties (1 file) → Enhance `Beep.OilandGas.ProductionAccounting`

| File | Description | Implementation |
|------|-------------|----------------|
| `OilProperties.xls` | Oil properties | Enhance crude oil models |

---

### Other Calculations (4 files) → Various Projects

| File | Description | Implementation |
|------|-------------|----------------|
| `OptimumGLR.xls` | Optimum GLR | Add to Nodal Analysis |
| `PipelineCapacity.xls` | Pipeline capacity | New: Beep.OilandGas.PipelineAnalysis |
| `LP - Flash.xls` | Low-pressure flash | New: Beep.OilandGas.FlashCalculations |
| `MixingRule.xls` | Mixing rules | Add to Properties |

---

## Summary

**Total Files:** 53
**New Projects Needed:** 8-10 projects
**Projects to Enhance:** 4 projects
**Total Calculations:** 50+ calculation methods

---

**Status: Mapping Complete** ✅

