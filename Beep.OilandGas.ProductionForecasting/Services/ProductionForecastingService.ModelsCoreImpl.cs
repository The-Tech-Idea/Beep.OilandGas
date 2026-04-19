using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.ProductionForecasting.Services
{
    public partial class ProductionForecastingService
    {
        // Explicit implementation of Models.Core.Interfaces.IProductionForecastingService
        async Task<ProductionForecastResult> Beep.OilandGas.Models.Core.Interfaces.IProductionForecastingService.GenerateForecastAsync(
            string? wellUWI, string? fieldId, string forecastMethod, int forecastPeriod)
        {
            var ft = System.Enum.TryParse<ForecastType>(forecastMethod, true, out var t)
                ? t
                : ForecastType.Decline;
            return await GenerateForecastAsync(wellUWI, fieldId, ft, forecastPeriod);
        }
    }
}
