using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    /// <summary>
    /// Request parameters for generating a production forecast.
    /// </summary>
    public class GenerateForecastRequest
    {
        /// <summary>Well UWI to forecast. Null to forecast at field level.</summary>
        public string? WellUWI { get; set; }

        /// <summary>Field identifier for field-level forecasting.</summary>
        public string? FieldId { get; set; }

        /// <summary>Decline curve or forecasting method to apply.</summary>
        public ForecastType ForecastMethod { get; set; }

        /// <summary>Number of months to forecast.</summary>
        public int ForecastPeriod { get; set; } = 12;
    }
}
