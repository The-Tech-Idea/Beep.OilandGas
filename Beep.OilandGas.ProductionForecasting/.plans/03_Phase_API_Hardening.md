# Phase 3: API Hardening

## Goal

Apply deterministic validation and exception mapping on `ProductionForecastingController`, consistent with HSE and Lease controllers.

## Target Files

- `Beep.OilandGas.ApiService/Controllers/Calculations/ProductionForecastingController.cs`

## TODO Checklist

- [x] Add null-body guards on POST endpoints (`generate`, `decline-curve`, `forecast`).
- [x] Map `ArgumentException` / `ArgumentNullException` to 400; generic unexpected errors to 500.
- [x] Rethrow `OperationCanceledException` (do not convert to 500).
- [x] Avoid dereferencing request in catch blocks before null checks.

## Verification

- `dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj`
