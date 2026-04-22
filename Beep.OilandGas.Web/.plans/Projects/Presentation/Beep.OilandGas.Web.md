# Beep.OilandGas.Web

## Snapshot

- Category: Presentation
- Scan depth: Heavy
- Current role: primary web surface for phases 6-10
- Maturity signal: active and broad, but structurally overlapped in routes and shared components

## Observed Structure

- Root files: `App.razor`, `Program.cs`, `_Imports.razor`
- Top-level folders: `Pages`, `Components`, `Services`, `Data`, `Shared`, `Theme`, `wwwroot`, `.plans`
- Pages root includes `Admin`, `Connection`, `Data`, and `PPDM39`
- Components root includes `Layout`, `Navigation`, `Shared`, `BusinessProcess`, `Visualization`, `Data`, `Production`, and routed `Account/Pages`
- Services folder contains typed clients and orchestration services such as `ApiClient`, `CalculationServiceClient`, `OperationsServiceClient`, `PumpServiceClient`, `PropertiesServiceClient`, `AccountingServiceClient`, and `LifeCycleService`

## Representative Evidence

- Startup: `Program.cs`
- Shell and routing: `App.razor`, `Components/Routes.razor`, `Components/App.razor`
- Data/admin overlap: `Pages/Data/DatabaseSetup.razor`, `Pages/PPDM39/Data/DatabaseSetup.razor`
- Setup overlap: `Pages/PPDM39/CreateDatabaseWizard.razor`, `Pages/PPDM39/Setup/DatabaseSetupWizard.razor`
- Forecast overlap: `Pages/PPDM39/Production/Forecasts.razor`, `Pages/PPDM39/Production/ProductionForecasting.razor`
- Shared component overlap: `Components/Shared/KpiCard.razor`, `Components/Dashboard/KpiCard.razor`

## Planning Notes

- This project is the primary owner for phase 6 page/component consolidation.
- Routed files exist under both `Pages` and `Components`, with the `Account/Pages` area as the clearest intentional exception.
- The typed-client layer is present, but the phase plans assume some pages still need migration away from direct or legacy integration paths.
- This project is the main source for route retirement, duplicate UI removal, and final boundary validation in phase 10.
