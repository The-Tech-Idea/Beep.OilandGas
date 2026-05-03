# Beep.OilandGas.ProductionForecasting

Production forecasting engine for Beep.OilandGas, including decline-curve analysis (DCA), deterministic forecasts, history-based parameter fitting from PPDM, and PPDM persistence for forecast runs and points.

## What This Library Covers

- Decline-based forecasting (exponential, harmonic, hyperbolic, modified-hyperbolic).
- Additional forecasting calculations (pseudo-steady-state, transient, gas well).
- API/service orchestration (`ProductionForecastingService`) with:
  - request-driven forecast generation,
  - optional history fit from PPDM tables,
  - algorithm guardrails (`b` bounds, `q_econ`, `Dlim`).
- Forecast persistence into PPDM forecast tables.

---

## Architecture At A Glance

### Core calculation classes

- `Calculations/DeclineForecast.cs`
- `Calculations/PseudoSteadyStateForecast.cs`
- `Calculations/TransientForecast.cs`
- `Calculations/GasWellForecast.cs`

### Forecast orchestration service

- `Services/ProductionForecastingService.cs`
- `Services/ProductionForecastingService.ForecastGeneration.cs`
- `Services/ProductionForecastingService.DCA.cs`

### DCA engine (merged into this project)

- `DCA/*`
- includes `DCAManager`, `DCAAnalysisService`, Arps methods, nonlinear regression, advanced methods.

### PPDM integration helpers

- `Services/ProductionHistoryLoader.cs` (history read path)
- `Services/ProductionForecastResultMapper.cs` (calculation model -> API result mapping)

---

## PPDM Tables Used (Current Implementation)

## 1) History-fit input tables

- `PDEN_WELL`
  - Join anchor by `PRIMARY_UWI` to locate PDEN entities for a well.
- `PDEN_VOL_SUMMARY`
  - Uses `OIL_VOLUME` over time to build production-rate series for fitting.
  - Time alignment uses `EFFECTIVE_DATE` (fallback `EXPIRY_DATE`).

The service path for this is:

- `ProductionHistoryLoader.TryLoadOilHistoryAsync(...)`
- called from `ProductionForecastingService.ForecastGeneration`.

## 2) Forecast persistence tables

- `PRODUCTION_FORECAST`
- `PRODUCTION_FORECAST_POINT`

Used by `ProductionForecastingService.SaveForecastAsync(...)`.

## 3) Are new PPDM entity classes or custom mapping layers being created?

No new PPDM table classes are created in this pass.

- We use existing PPDM model classes and `PPDMGenericRepository`.
- We added orchestration/mapping helpers only:
  - `ProductionHistoryLoader`
  - `ProductionForecastResultMapper`

---

## Install

```bash
dotnet add package Beep.OilandGas.ProductionForecasting
```

---

## Quick Start: Decline Calculations (Direct)

## Exponential decline

```csharp
using Beep.OilandGas.ProductionForecasting.Calculations;

var exp = DeclineForecast.GenerateExponentialDeclineForecast(
    qi: 1000m,
    di: 0.015m,
    forecastDuration: 365m, // days
    timeSteps: 24);
```

## Harmonic decline with economic limit

```csharp
var harmonic = DeclineForecast.GenerateHarmonicDeclineForecast(
    qi: 1000m,
    di: 0.02m,
    forecastDuration: 3650m,
    economicLimit: 25m,
    timeSteps: 120);
```

## Hyperbolic decline

```csharp
var hyp = DeclineForecast.GenerateHyperbolicDeclineForecast(
    qi: 1200m,
    di: 0.01m,
    b: 0.6m,
    forecastDuration: 3650m,
    economicLimit: 20m,
    timeSteps: 120);
```

## Modified hyperbolic (Dlim crossover)

```csharp
var modified = DeclineForecast.GenerateModifiedHyperbolicDeclineForecast(
    qi: 1000m,
    di: 0.01m,
    b: 0.5m,
    forecastDuration: 400m,
    terminalDi: 0.005m, // Dlim
    economicLimit: null,
    timeSteps: 4);      // t = 0,100,200,300,400
```

Deterministic crossover vector for this example:

- `t_switch = (Di / Dlim - 1) / (b * Di) = 200`
- `q_switch = qi / (1 + b*Di*t_switch)^(1/b) = 250`
- post-switch tail point:
  - `q(300) = q_switch * exp(-Dlim*(300-200)) = 151.6327`

---

## Service Usage (Recommended API Path)

`ProductionForecastingService` is the high-level route used by API.

```csharp
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.ProductionForecasting.Services;

var request = new GenerateForecastRequest
{
    WellUWI = "UWI-12345",
    ForecastMethod = ForecastType.Hyperbolic,
    ForecastPeriod = 24,              // months

    // Optional manual parameters
    InitialOilRateQi = 1400m,
    InitialDeclineDi = 0.012m,
    DeclineExponentB = 0.65m,

    // Guardrails / tail handling
    UseModifiedHyperbolic = true,
    TerminalDeclineDi = 0.0002m,      // Dlim
    EconomicLimitOilRate = 25m,       // q_econ

    // If false and WellUWI exists, service tries PPDM history fit
    SkipHistoryFit = false
};

var result = await forecastingService.GenerateForecastAsync(request);
```

### Request behavior summary

- If `SkipHistoryFit == false` and `WellUWI` is provided, service tries PPDM history-fit.
- If sufficient history exists, service fits and uses fitted parameters.
- If history is insufficient and required manual parameters are missing, service throws an argument error.
- If `EconomicLimitOilRate` is invalid (`<= 0` or `>= qi`), service rejects request.

---

## PPDM History Read Example

This is the practical pattern used for fitting:

```csharp
// 1) Lookup PDEN entities connected to well UWI
var pdenWellRepo = new PPDMGenericRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(PDEN_WELL), connectionName, "PDEN_WELL", null);

var links = await pdenWellRepo.GetAsync(new List<AppFilter>
{
    new AppFilter { FieldName = "PRIMARY_UWI", Operator = "=", FilterValue = wellUwi },
    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = defaults.GetActiveIndicatorYes() }
});

var pdenIds = links.OfType<PDEN_WELL>()
    .Select(x => x.PDEN_ID)
    .Where(id => !string.IsNullOrWhiteSpace(id))
    .Distinct()
    .ToList();

// 2) Read production volumes by PDEN_ID
var volRepo = new PPDMGenericRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(PDEN_VOL_SUMMARY), connectionName, "PDEN_VOL_SUMMARY", null);

// 3) Convert OIL_VOLUME to daily-equivalent rate and fit decline
```

---

## Forecast Save Example (PPDM Persistence)

```csharp
using Beep.OilandGas.Models.Data.Calculations;

var forecast = new ProductionForecastResult
{
    ForecastId = "FC-001",
    WellUWI = "UWI-12345",
    ForecastMethod = ForecastType.Hyperbolic,
    ForecastDate = DateTime.UtcNow,
    ForecastPoints = new List<ProductionForecastPoint>
    {
        new() { Date = DateTime.UtcNow.Date.AddDays(30), OilRate = 980m, GasRate = 0m, WaterRate = 0m },
        new() { Date = DateTime.UtcNow.Date.AddDays(60), OilRate = 960m, GasRate = 0m, WaterRate = 0m }
    }
};

await forecastingService.SaveForecastAsync(forecast, userId: "SYSTEM");
```

This persists to:

- `PRODUCTION_FORECAST`
- `PRODUCTION_FORECAST_POINT`

---

## Deterministic Numerical Regression Vectors

Current regression coverage includes:

- very short forecast periods,
- ultra-low decline rates,
- extreme `b` edges (`b=0`, `b~1`),
- modified hyperbolic transition vector (`t_switch`, `q_switch`, tail continuity).

Primary test file:

- `Beep.OilandGas.ApiService.Tests/ProductionForecastingNumericalEdgeCaseRegressionTests.cs`

---

## Other Forecast Methods (Non-Decline)

You can also call:

- `PseudoSteadyStateForecast.GenerateSinglePhaseForecast(...)`
- `PseudoSteadyStateForecast.GenerateTwoPhaseForecast(...)`
- `TransientForecast.GenerateTransientForecast(...)`
- `GasWellForecast.GenerateGasWellForecast(...)`

Use these where reservoir-mechanics assumptions are preferred over decline-curve fitting.

---

## Guardrails and Defaults

Key policy constants live in:

- `Constants/ForecastAlgorithmConstants.cs`

Includes:

- minimum history points for fit,
- Arps `b` bounds,
- default `qi`, `Di`, `b`,
- default terminal decline (`Dlim`),
- month-to-day conversion constant.

---

## Validation and Error Handling

Typical validation failures:

- missing `WellUWI` and `FieldId`,
- `ForecastMethod == None`,
- `ForecastPeriod < 1`,
- invalid `EconomicLimitOilRate`,
- insufficient PPDM history with no manual fallback parameters.

Handle `ArgumentException` for request issues and domain-specific forecast exceptions for lower-level calculation errors.

---

## Integration Notes

- API controller path: `Beep.OilandGas.ApiService/Controllers/Calculations/ProductionForecastingController.cs`
- Canonical interface: `Beep.OilandGas.Models.Core.Interfaces.IProductionForecastingService`
- DCA engine is merged into this project under `DCA/` (no separate DCA project dependency required).

---

## License

MIT License - see repository license.

