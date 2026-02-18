using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.OilProperties;
using Beep.OilandGas.OilProperties.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.OilProperties.Services
{
    public partial class OilPropertiesService
    {
        public async Task<OilPropertyResult> CalculateBlackOilPropertiesAsync(OIL_PROPERTY_CONDITIONS input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            _logger?.LogInformation("Calculating Black Oil Properties for Conditions {Id}", input.OIL_PROPERTY_CONDITIONS_ID);

            var result = new OilPropertyResult
            {
                 CalculationId = Guid.NewGuid().ToString(),
                 CalculationDate = DateTime.UtcNow,
                 CorrelationMethod = "STANDING_BEGGS",
                 Pressure = input.PRESSURE,
                 Temperature = input.TEMPERATURE,
                 ApiGravity = input.API_GRAVITY
            };
            
            decimal rs = 0;
            decimal pb = 0;

            // 1. Determine State (Saturated vs Undersaturated) and Pb
            // If BubblePoint is known, we use it. If not, we might calculate it from Solution GOR if provided.
            
            if (input.BUBBLE_POINT_PRESSURE.HasValue)
            {
                pb = input.BUBBLE_POINT_PRESSURE.Value;
            }
            else if (input.SOLUTION_GAS_OIL_RATIO.HasValue)
            {
                // Calc Pb from Rs
                rs = input.SOLUTION_GAS_OIL_RATIO.Value;
                pb = OilPropertyCalculator.CalculateBubblePointPressure_Standing(rs, input.GAS_SPECIFIC_GRAVITY, input.API_GRAVITY, input.TEMPERATURE);
            }
            else
            {
                // Calc Rs from P (assume saturated at P)
                pb = input.PRESSURE; // Assume saturation pressure is current pressure or calculate Rs at current P
                rs = OilPropertyCalculator.CalculateSolutionGOR_Standing(input.PRESSURE, input.GAS_SPECIFIC_GRAVITY, input.API_GRAVITY, input.TEMPERATURE);
            }
            
            // Adjust Rs if P > Pb (Rs stays constant above Pb)
            decimal rs_at_p = rs;
            if (input.PRESSURE > pb)
            {
                 rs_at_p = OilPropertyCalculator.CalculateSolutionGOR_Standing(pb, input.GAS_SPECIFIC_GRAVITY, input.API_GRAVITY, input.TEMPERATURE);
            }
            else
            {
                 rs_at_p = OilPropertyCalculator.CalculateSolutionGOR_Standing(input.PRESSURE, input.GAS_SPECIFIC_GRAVITY, input.API_GRAVITY, input.TEMPERATURE);
            }

            result.SolutionGasOilRatio = rs_at_p;

            // 2. Calculate FVF (Bo) at Pressure
            // Use Rs at pressure
            result.FormationVolumeFactor = OilPropertyCalculator.CalculateOilFVF_Standing(rs_at_p, input.GAS_SPECIFIC_GRAVITY, input.API_GRAVITY, input.TEMPERATURE);

            // 3. Calculate Viscosity
            decimal deadVisc = OilPropertyCalculator.CalculateDeadOilViscosity_BeggsRobinson(input.API_GRAVITY, input.TEMPERATURE);
            result.Viscosity = OilPropertyCalculator.CalculateSaturatedViscosity_BeggsRobinson(deadVisc, rs_at_p);
            
            // Undersaturated viscosity correction would be next step if P > Pb (Vasquez-Beggs), omitting for brevity in this step.

            return await Task.FromResult(result);
        }
    }
}
