# Phase 5: Reference seeds (Permits / Lease depth)

## Goal

Provide stable LOV-style reference data for production forecasting (forecast methods aligned with `ForecastType`, run statuses, risk rating) using the same pattern as Development Planning and Lease Acquisition: **table classes + module `EntityTypes` + idempotent seed rows**.

## Schema (module-driven, no hand-written SQL)

- Register `PRODUCTION_FORECAST`, `PRODUCTION_FORECAST_POINT`, and `R_PRODUCTION_FORECASTING_REFERENCE_CODE` on [`ProductionForecastingModule`](../Modules/ProductionForecastingModule.cs) so the **PPDM39 setup / migration pipeline** can derive schema from the entity types.
- **Do not** add or maintain hand-written `Beep.OilandGas.Models/Scripts/**/R_PRODUCTION_FORECASTING_REFERENCE_CODE*.sql` for this table; physical DDL is generated from the data classes and your existing setup workflow (same approach as other domain modules).

## Target files

- `Beep.OilandGas.Models/Data/ProductionForecasting/R_PRODUCTION_FORECASTING_REFERENCE_CODE.cs`
- [`Constants/ProductionForecastingReferenceSets.cs`](../Constants/ProductionForecastingReferenceSets.cs)
- [`Constants/ProductionForecastingReferenceCodes.cs`](../Constants/ProductionForecastingReferenceCodes.cs)
- [`Constants/ProductionForecastingReferenceCodeSeed.cs`](../Constants/ProductionForecastingReferenceCodeSeed.cs)
- [`Modules/ProductionForecastingModule.cs`](../Modules/ProductionForecastingModule.cs)
- `Beep.OilandGas.ApiService.Tests/ProductionForecastingReferenceSeedCatalogTests.cs`

## TODO checklist

- [x] `R_PRODUCTION_FORECASTING_REFERENCE_CODE` model in Models.
- [x] Reference sets and seed catalog; module seeds with skip-if-exists.
- [x] Service uses `ProductionForecastingReferenceCodes` for status / risk literals.
- [x] Catalog tests for uniqueness and coverage.

## Verification

- `dotnet build Beep.OilandGas.sln`
- `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~ProductionForecastingReferenceSeedCatalogTests"`
