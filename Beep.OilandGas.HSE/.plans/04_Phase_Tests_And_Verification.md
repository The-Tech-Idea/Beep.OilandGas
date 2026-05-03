# Phase 4: Tests And Verification

## Goal
Add regression protection for canonical HSE boundaries and API behavior.

## Target Files
- `Beep.OilandGas.ApiService.Tests/*HSE*.cs`
- `Beep.OilandGas.HSE/MASTER-TODO-TRACKER.md`

## TODO Checklist
- [x] Add/update `HSEController` tests for validation and error mapping.
- [x] Keep process-controller tests covering workflow enrollment + PPDM incident linkage.
- [x] Add interface-boundary comments if HSE interfaces change.
- [x] Record build/test outcomes in tracker docs.

## Verification Commands
- [x] `dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj`
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~HSE"`
- [x] `dotnet build Beep.OilandGas.sln`
