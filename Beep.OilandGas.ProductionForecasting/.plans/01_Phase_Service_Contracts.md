# Phase 1: Service Contracts

## Goal

Clarify and document the boundary between the **canonical** `Beep.OilandGas.Models.Core.Interfaces.IProductionForecastingService` and the **feature-local** `Beep.OilandGas.ProductionForecasting.Services.IProductionForecastingService`.

## Target Files

- `Beep.OilandGas.Models/Core/Interfaces/IProductionForecastingService.cs`
- `Beep.OilandGas.ProductionForecasting/Services/IProductionForecastingService.cs`
- `Beep.OilandGas.ProductionForecasting/Services/ProductionForecastingService.ModelsCoreImpl.cs`
- `Beep.OilandGas.ApiService/Program.cs` (Production Forecasting `AddScoped` block)

## TODO Checklist

- [x] Document Models.Core interface as API-facing surface (`<remarks>`).
- [x] Document feature-local interface as extended/internal capability surface.
- [x] Confirm explicit Core implementation maps string `forecastMethod` to `ForecastType`.
- [x] Confirm DI registers `Models.Core.Interfaces.IProductionForecastingService` only for API consumption.

## Verification

- `dotnet build Beep.OilandGas.Models/Beep.OilandGas.Models.csproj`
- `dotnet build Beep.OilandGas.ProductionForecasting/Beep.OilandGas.ProductionForecasting.csproj`
