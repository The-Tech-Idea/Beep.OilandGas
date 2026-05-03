# Phase 5 — Tests and verification

## Goal

Add **`Beep.OilandGas.CompressorAnalysis.Tests`** (xUnit) with **golden-vector** regression tests for calculators and optional **API contract** tests mirroring **`ApiService.Tests`** patterns.

## Target files (as implemented)

- `Beep.OilandGas.CompressorAnalysis.Tests/Beep.OilandGas.CompressorAnalysis.Tests.csproj`
- `CompressorAnalysisModuleContractTests.cs`
- `CompressorAnalysisServiceContractTests.cs`
- `CompressorCalculatorGoldenVectorTests.cs` (centrifugal + reciprocating regression vectors)
- *Backlog:* per-calculator test classes, pressure monotonicity, dedicated validator suite

## TODO checklist

- [x] Create test project targeting **net10.0**, reference **`CompressorAnalysis`** + **`Models`** (+ **`PPDM39`** / **`GasProperties`** as needed).
- [x] **Centrifugal**: golden vector in **`CompressorCalculatorGoldenVectorTests`** — compression ratio, **`BRAKE_HORSEPOWER`**, discharge **T** ≥ suction.
- [x] **Reciprocating**: golden vector — positive brake HP at fixed ratio / speed.
- [x] **Pressure**: **`CompressorCalculatorGoldenVectorTests.CalculateRequiredPressure_VectorA_RespectsMaxPowerAndRaisesDischarge`** — discharge above suction, power at most **`maxHp`**.
- [x] **Validator**: **`CompressorValidatorTests`** — suction/discharge, efficiency range, compression ratio cap, centrifugal/recip bounds.
- [ ] Add test filter guidance to **`.plans/README.md`** (`dotnet test ... --filter FullyQualifiedName~Compressor`).

## Verification criteria

```bash
dotnet test Beep.OilandGas.CompressorAnalysis.Tests
dotnet build Beep.OilandGas.sln
```

CI: optional **`dotnet test`** job scoped to compressor tests for fast feedback.

## Coverage backlog

- Multistage / map / surge margin helpers — tests once API stable (**CentrifugalCompressorCalculator** advanced methods)
