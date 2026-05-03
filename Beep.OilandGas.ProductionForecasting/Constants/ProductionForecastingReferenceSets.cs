namespace Beep.OilandGas.ProductionForecasting.Constants;

/// <summary>
/// Reference set identifiers stored in <c>REFERENCE_SET</c> for production forecasting picklists.
/// </summary>
public static class ProductionForecastingReferenceSets
{
    /// <summary>Forecast method / decline family codes aligned with <see cref="Beep.OilandGas.Models.Data.ProductionForecasting.ForecastType"/> names.</summary>
    public const string ForecastMethod = "PRODUCTION_FORECAST_METHOD";

    /// <summary>Run and result status strings used by forecasting services and projections.</summary>
    public const string ForecastRunStatus = "PRODUCTION_FORECAST_RUN_STATUS";

    /// <summary>Risk rating codes for forecast risk analysis.</summary>
    public const string ForecastRiskRating = "PRODUCTION_FORECAST_RISK_RATING";
}
