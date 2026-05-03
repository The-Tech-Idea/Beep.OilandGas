# Phase 4: Tests And Verification

## Goal

Add focused controller/service regression tests for nodal analysis and record reproducible verification commands in tracker docs.

## Target Files

- `Beep.OilandGas.ApiService.Tests/*Nodal*Tests.cs`
- `Beep.OilandGas.NodalAnalysis/MASTER-TODO-TRACKER.md`

## TODO Checklist

- [x] Add API controller tests (null body, happy path, argument error mapping, cancellation propagation).
- [x] Add service tests for operating-point calculation and save/history repository behavior.
- [x] Add edge-case tests (no intersection, low PI, high WHP, depth extremes).
- [x] Record command outputs and pass counts in tracker.

## Verification Commands

- [x] `dotnet build Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj`
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~Nodal"`
