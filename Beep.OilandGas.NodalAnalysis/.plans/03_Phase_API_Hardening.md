# Phase 3: API Hardening

## Goal

Harden Nodal Analysis API endpoints for null guards, 400/500 mapping, and cancellation behavior, matching hardened patterns used in other domains.

## Target Files

- `Beep.OilandGas.ApiService/Controllers/Calculations/*Nodal*.cs` (or active nodal controller paths)

## TODO Checklist

- [x] Add explicit null-body checks for POST operations.
- [x] Map `ArgumentException`/`ArgumentNullException` to 400 with actionable error messages.
- [x] Rethrow `OperationCanceledException` instead of converting to 500.
- [x] Ensure unexpected exceptions are logged and mapped to 500 consistently.

## Verification

- [x] `dotnet build Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj` (succeeded)
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~Nodal"` (Passed: 9, Failed: 0)
