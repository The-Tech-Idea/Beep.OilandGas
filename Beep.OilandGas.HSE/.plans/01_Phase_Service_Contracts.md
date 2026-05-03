# Phase 1: Service Contracts

## Goal
Harden canonical interface boundaries: storage/domain in PPDM services, field-aware composition in lifecycle services.

## Target Files
- `Beep.OilandGas.Models/Core/Interfaces/IHSEService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IFieldHSEService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/*.cs`

## TODO Checklist
- [x] Verify `IHSEService` remains canonical storage/domain interface.
- [x] Verify `IFieldHSEService` remains field-scoped orchestrator facade.
- [x] Ensure transition/status normalization stays centralized.
- [x] Confirm no guessed columns are used in PPDM writes.

## Verification
- `dotnet build Beep.OilandGas.PPDM39.DataManagement/Beep.OilandGas.PPDM39.DataManagement.csproj`
- `dotnet build Beep.OilandGas.Models/Beep.OilandGas.Models.csproj`
