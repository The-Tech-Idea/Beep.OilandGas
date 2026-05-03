# Phase 2: Data And Calculation Surface

## Goal

Document how production forecasting touches PPDM tables and calculation helpers, without expanding stub implementations in this refresh pass.

## Target Files

- `Beep.OilandGas.ProductionForecasting/Services/ProductionForecastingService.cs` (generate/save, repository wiring)
- `Beep.OilandGas.ProductionForecasting/Services/ProductionForecastingService.DCA.cs` (decline curve analysis and DCA-related paths)
- `Beep.OilandGas.ProductionForecasting/Calculations/*` (as referenced by DCA partial)

## TODO Checklist

- [x] Confirm `PRODUCTION_FORECAST` and `PRODUCTION_FORECAST_POINT` are the primary persistence targets for save/load paths.
- [x] Note placeholder or simplified logic where full reservoir/ML workflows are not yet implemented.
- [x] Avoid new raw SQL or non-metadata repository patterns outside existing `PPDMGenericRepository` usage.

## Verification

- `dotnet build Beep.OilandGas.ProductionForecasting/Beep.OilandGas.ProductionForecasting.csproj`
