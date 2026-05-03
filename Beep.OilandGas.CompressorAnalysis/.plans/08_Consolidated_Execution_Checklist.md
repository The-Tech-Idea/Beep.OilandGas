# Consolidated execution checklist — CompressorAnalysis

Single runbook aggregating checks from phases **0–6**. Use before releases or large merges.

## Architecture and governance

- [ ] **`CLAUDE.md`** rules: shared interfaces in **`Models.Core.Interfaces`** (feature **`Core.Interfaces`** when tied to feature-local extension tables); no **`DTO`** namespaces; **WellServices** if touching well entities from callers
- [ ] **Extension tables**: entities + **`IModuleSetup.EntityTypes`** + tooling — no new hand **`Models/Scripts`** DDL for compressor extensions
- [ ] **`PPDM39.DataManagement`** does **not** reference **`CompressorAnalysis`**

## Build

```bash
dotnet build Beep.OilandGas.CompressorAnalysis/Beep.OilandGas.CompressorAnalysis.csproj
dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
```

## Contracts and services

- [x] **`ICompressorAnalysisService`** in **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces`**; **`CompressorAnalysisService`** registered in ApiService
- [ ] **`Program.cs`** DI order respected for new dependencies

## Calculations

- [ ] **`CompressorValidator`** invoked on public API paths before **`CalculatePower`**
- [ ] Constants (**k**, efficiencies) consistent across centrifugal/recip paths

## Module / data

- [x] **`CompressorAnalysisModule`** registered; **`EntityTypes`** + **`SeedAsync`** for **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`** aligned with **`CompressorAnalysis.Data`**

## API / orchestration

- [ ] **`CompressorController`** routes match **`AnalysisService.Compressor`** client
- [ ] **`PerformCompressorAnalysisAsync`** behavior documented relative to full calculator path

## Tests (when phase 5 exists)

```bash
dotnet test Beep.OilandGas.CompressorAnalysis.Tests
```

## Documentation

- [ ] **`README.md`** examples build conceptually
- [ ] **`IMPLEMENTATION_SUMMARY.md`** reflects current integration
