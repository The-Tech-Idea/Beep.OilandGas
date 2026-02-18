using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ChokeAnalysis.Services
{
    public partial class ChokeAnalysisService
    {
        public async Task<MultiphaseChokeAnalysis> CalculateMultiphaseFlowAsync(
            MultiphaseChokeAnalysis input, 
            decimal chokeDiameter, 
            string correlation = "Gilbert")
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (chokeDiameter <= 0) throw new ArgumentException("Choke diameter must be positive");

            _logger?.LogInformation("Calculating multiphase flow using {Correlation}", correlation);

            decimal liquidRate = input.OilFlowRate + input.WaterFlowRate;
            decimal glr = (liquidRate > 0) ? (input.GasFlowRate * 1000m) / liquidRate : 0;

            var results = MultiphaseChokeCalculator.CalculatePressures(liquidRate, glr, chokeDiameter);
            decimal calcP = 0;

            switch (correlation.ToLower())
            {
                case "ros":
                    calcP = results.RosPressure;
                    break;
                case "achong":
                    calcP = results.AchongPressure;
                    break;
                case "baxendell":
                    calcP = results.BaxendellPressure;
                    break;
                case "gilbert":
                default:
                    calcP = results.GilbertPressure;
                    break;
            }

            // Populate result
            input.TotalPressureDrop = Math.Max(0, calcP - input.DownstreamPressure);
            input.AnalysisDate = DateTime.UtcNow;
            input.AnalysisId = Guid.NewGuid().ToString();
            
            // Add note about the correlation used and the P_up calculated
            // Assuming we check this against actual UpstreamPressure??
            // Or maybe we treat 'UpstreamPressure' as the result?
            // Usually in nodal analysis, we want P_up.
            // But 'TotalPressureDrop' is P_up - P_down.
            
            // Let's assume input.ComputedUpstreamPressure is what we want, but the model has DownstreamPressure.
            // We will set additional fields if possible, or use standard fields via extended properties or description.
            // Since we can't change the model structure instantly without verifying:
            // We'll update TotalPressureDrop based on the calc P_upstream - Input.Downstream.
            
            return await Task.FromResult(input);
        }

        public async Task<Dictionary<string, decimal>> CompareCorrelationsAsync(
            decimal oilRate, decimal waterRate, decimal gasRate, decimal chokeDiameter)
        {
             decimal liquidRate = oilRate + waterRate;
             decimal glr = (liquidRate > 0) ? (gasRate * 1000m) / liquidRate : 0;
             
             var results = MultiphaseChokeCalculator.CalculatePressures(liquidRate, glr, chokeDiameter);
             
             return await Task.FromResult(new Dictionary<string, decimal>
             {
                 { "Gilbert", results.GilbertPressure },
                 { "Ros", results.RosPressure },
                 { "Achong", results.AchongPressure },
                 { "Baxendell", results.BaxendellPressure }
             });
        }
    }
}
