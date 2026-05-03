using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.OilProperties;
using Beep.OilandGas.OilProperties.Calculations;
using Beep.OilandGas.OilProperties.Constants;
using Beep.OilandGas.OilProperties.Validation;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.OilProperties.Services
{
    public partial class OilPropertiesService
    {
        public async Task<OilPropertyResult> CalculateBlackOilPropertiesAsync(OIL_PROPERTY_CONDITIONS input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            OilPropertyValidator.ValidateOilPropertyConditions(input);

            _logger?.LogInformation("Calculating Black Oil Properties for Conditions {Id}", input.OIL_PROPERTY_CONDITIONS_ID);

            decimal tempF = OilPropertyUnits.RankineToFahrenheit(input.TEMPERATURE);

            var (_, rsAtP) = BlackOilScreening.GetPbAndRsAtPressure(
                input.PRESSURE,
                tempF,
                input.API_GRAVITY,
                input.GAS_SPECIFIC_GRAVITY,
                input.BUBBLE_POINT_PRESSURE,
                input.SOLUTION_GAS_OIL_RATIO);

            var result = new OilPropertyResult
            {
                 CalculationId = _defaults.FormatIdForTable("OIL_PROPERTY", Guid.NewGuid().ToString()),
                 CalculationDate = DateTime.UtcNow,
                 CorrelationMethod = "STANDING_BEGGS",
                 Pressure = input.PRESSURE,
                 Temperature = input.TEMPERATURE,
                 ApiGravity = input.API_GRAVITY,
                 SolutionGasOilRatio = rsAtP,
                 Density = OilPropertyConstants.WaterDensity * OilPropertyCalculator.OilSpecificGravityFromApi(input.API_GRAVITY)
            };

            result.FormationVolumeFactor = OilPropertyCalculator.CalculateOilFVF_Standing(rsAtP, input.GAS_SPECIFIC_GRAVITY, input.API_GRAVITY, tempF);

            decimal deadVisc = OilPropertyCalculator.CalculateDeadOilViscosity_BeggsRobinson(input.API_GRAVITY, tempF);
            result.Viscosity = OilPropertyCalculator.CalculateSaturatedViscosity_BeggsRobinson(deadVisc, rsAtP);
            
            // Undersaturated viscosity correction would be next step if P > Pb (Vasquez-Beggs), omitting for brevity in this step.

            return await Task.FromResult(result);
        }
    }
}
