# Phase 1: Service Contracts

## Goal

Align and document canonical service boundaries for Nodal Analysis so API, DI, and implementation surfaces are explicit and stable.

## Target Files

- `Beep.OilandGas.Models/Core/Interfaces/INodalAnalysisService.cs`
- `Beep.OilandGas.NodalAnalysis/Services/NodalAnalysisService.cs`
- `Beep.OilandGas.NodalAnalysis/Services/NodalAnalysisService.Advanced.cs`
- `Beep.OilandGas.ApiService/Program.cs` (Nodal Analysis DI block)

## TODO Checklist

- [x] Add `<remarks>` clarifying `INodalAnalysisService` is canonical API-facing interface.
- [x] Ensure service partials do not expose conflicting contract semantics.
- [x] Verify DI uses factory pattern and registers only canonical interface for API.
- [x] Document any additional internal-only methods that should remain outside canonical contract.

## Verification

- `dotnet build Beep.OilandGas.Models/Beep.OilandGas.Models.csproj`
- `dotnet build Beep.OilandGas.NodalAnalysis/Beep.OilandGas.NodalAnalysis.csproj`
