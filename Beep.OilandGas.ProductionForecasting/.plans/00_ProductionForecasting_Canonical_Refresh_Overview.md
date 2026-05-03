# Production Forecasting Canonical Refresh Plan

## Scope

Refresh `Beep.OilandGas.ProductionForecasting` planning artifacts to match the canonicalization pattern used across other Beep.OilandGas domains (HSE, Permits, Lease).

## Current Baseline

- **Canonical API/DI contract**: `Beep.OilandGas.Models/Core/Interfaces/IProductionForecastingService.cs` (three methods used by `ProductionForecastingController`).
- **Rich feature interface**: `Beep.OilandGas.ProductionForecasting/Services/IProductionForecastingService.cs` (extended surface; same type name, different namespace).
- **Implementation**: `ProductionForecastingService` partial class; explicit Core bridge in `ProductionForecastingService.ModelsCoreImpl.cs`.
- **API**: `Beep.OilandGas.ApiService/Controllers/Calculations/ProductionForecastingController.cs` under `api/ProductionForecasting`.
- **Registration**: `Beep.OilandGas.ApiService/Program.cs` (`AddScoped` for `Models.Core.Interfaces.IProductionForecastingService`).

## Architecture Objective

- Keep PPDM persistence for forecast headers and points aligned with metadata-driven repositories.
- Keep **Models.Core** as the HTTP-facing service contract; avoid confusing it with the feature-local interface.
- Harden API behavior for deterministic error mapping (null bodies, 400 vs 500, cancellation).

## Phase Index

1. `01_Phase_Service_Contracts.md`
2. `02_Phase_Data_And_Calculation_Surface.md`
3. `03_Phase_API_Hardening.md`
4. `04_Phase_Tests_And_Verification.md`
5. `05_Phase_Reference_Seeds.md`
