# Phase 3: API Hardening

## Goal
Apply deterministic validation and exception mapping across HSE API surfaces.

## Target Files
- `Beep.OilandGas.ApiService/Controllers/HSE/HSEController.cs`
- `Beep.OilandGas.ApiService/Controllers/BusinessProcess/HSEProcessController.cs`
- `Beep.OilandGas.ApiService/Program.cs`

## TODO Checklist
- [x] Add null-body guards on mutable endpoints.
- [x] Map `ArgumentException` to 400 and not-found cases to 404.
- [x] Ensure `OperationCanceledException` is not swallowed.
- [x] Verify DI wiring for canonical and field-scoped HSE dependencies.

## Verification
- `dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj`
