# Beep.OilandGas.OilProperties

Black-oil **screening** correlations (Standing bubble point / solution GOR / oil FVF; Beggs–Robinson dead and saturated oil viscosity) and **`OilPropertiesService`** persistence helpers for **`OIL_COMPOSITION`** / **`OIL_PROPERTY_RESULT`**.

## Units

- **`IOilPropertiesService`** temperature arguments are **Rankine** (see XML on the interface).
- **`OilPropertyCalculator`** uses **Fahrenheit** for Standing and Beggs–Robinson; convert with **`OilPropertyUnits.RankineToFahrenheit`**.
- **`OIL_PROPERTY_CONDITIONS.TEMPERATURE`** is treated as **°R** in **`CalculateBlackOilPropertiesAsync`** (validated ≥ ~0 °F equivalent).

## Planning

Phased work: **[`.plans/README.md`](.plans/README.md)**. Rollup: **[`MASTER-TODO-TRACKER.md`](MASTER-TODO-TRACKER.md)**.

## Quick references

| Area | Location |
|------|-----------|
| Correlations | `Calculations/OilPropertyCalculator.cs` |
| Pb / Rs at P (Standing screening) | `Calculations/BlackOilScreening.cs` |
| Unit conversion | `Constants/OilPropertyUnits.cs` |
| Validation | `Validation/OilPropertyValidator.cs` (`ValidateSimpleScreeningInputs` for API helpers) |
| Black-oil orchestration | `Services/OilPropertiesService.Advanced.cs` |

`CalculateOilPropertiesAsync` uses **`OilPropertyConstants.DefaultGasSpecificGravity`** (0.65) because **`OilComposition`** does not carry γg; use **`OIL_PROPERTY_CONDITIONS`** / **`CalculateBlackOilPropertiesAsync`** when gas gravity must come from data.

**Property matrix / trend** (`GeneratePropertyCorrelationMatrixAsync`, `AnalyzePropertyTrendAsync`) use the same **Pb / Rs at P** screening as composition-based Bo and **saturated** oil viscosity, and validate the P–T window up front (`OilPropertyValidator.ValidateCompositionForPvtSweeps` / `ValidateCompositionForPressureTrend`).

**Interfacial tension screening** uses ideal-gas density with **T in Rankine** (not °F) in the denominator; the temperature tweak term uses **°F** from **`OilPropertyUnits.RankineToFahrenheit`**.

`CalculateFormationVolumeFactor` uses the **Standing** oil FVF correlation. Unknown `correlation` names are accepted for API compatibility but **logged as a warning** and still evaluated with Standing.

## API requests

`CalculateFVFRequest` includes optional **`GasSpecificGravity`** (default **0.65**). The API clamps non-positive or extreme values (above **2.5**) back to **0.65** before calling the service.

## Tests

```bash
dotnet test Beep.OilandGas.OilProperties.Tests/Beep.OilandGas.OilProperties.Tests.csproj
```

## License

MIT
