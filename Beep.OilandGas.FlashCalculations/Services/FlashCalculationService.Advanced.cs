using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.FlashCalculations.Services
{
    public partial class FlashCalculationService
    {
        public async Task<FlashCalculationResult> RunRigorousFlashAsync(FlashCalculationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Pressure == null || request.Temperature == null)
                throw new ArgumentException("Pressure and Temperature are required for PT Flash.");
            if (request.FeedComposition == null || !request.FeedComposition.Any())
                throw new ArgumentException("Feed composition is required.");

            _logger?.LogInformation("Starting Rigorous PT Flash at P={P}, T={T}", request.Pressure, request.Temperature);

            var result = new FlashCalculationResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                CalculationDate = DateTime.UtcNow,
                CalculationType = "PT_FLASH_RIGOROUS",
                Pressure = request.Pressure,
                Temperature = request.Temperature,
                FeedComposition = request.FeedComposition,
                IsSuccessful = true
            };

            try
            {
                // Convert list to base type if needed, but they are FlashCalculations.Components which inherit
                var components = request.FeedComposition.Cast<FLASH_COMPONENT>().ToList();

                // 1. Solve Rachford-Rice
                int iterations;
                bool converged;
                decimal vaporFraction = FlashCalculator.SolveRachfordRice(
                    components, 
                    request.Pressure.Value, 
                    request.Temperature.Value, 
                    out iterations, 
                    out converged
                );

                result.VaporFraction = vaporFraction;
                result.LiquidFraction = 1.0m - vaporFraction;
                result.Iterations = iterations;
                result.Converged = converged;

                // 2. Calculate Phase Compositions
                FlashCalculator.CalculatePhaseCompositions(
                    vaporFraction,
                    components,
                    request.Pressure.Value,
                    request.Temperature.Value,
                    out var phaseResults
                );

                // 3. Populate Result Lists
                // Needed DTOs: FlashComponentFraction, FlashComponentKValue.
                // Assuming they are simple DTOs in result namespace or Data.
                
                // Let's create DTOs if they don't map directly, checking definition implies they exist in FlashCalculations model namespace?
                // The Result model has `List<FlashComponentFraction>`.
                // I need to find where `FlashComponentFraction` is defined or just instantiate it.
                // It's likely a class in `Beep.OilandGas.Models.Data.FlashCalculations`.
                
                result.VaporComposition = new List<FlashComponentFraction>();
                result.LiquidComposition = new List<FlashComponentFraction>();
                result.KValues = new List<FlashComponentKValue>();

                foreach (var phaseRes in phaseResults)
                {
                    result.VaporComposition.Add(new FlashComponentFraction 
                    { 
                        ComponentName = phaseRes.Name, 
                        MoleFraction = phaseRes.yi 
                    });
                    
                    result.LiquidComposition.Add(new FlashComponentFraction 
                    { 
                        ComponentName = phaseRes.Name, 
                        MoleFraction = phaseRes.xi 
                    });
                    
                    result.KValues.Add(new FlashComponentKValue 
                    { 
                        ComponentName = phaseRes.Name, 
                        KValue = phaseRes.K 
                    });
                }

                // serialize JSONs for storage if needed
                // result.VaporCompositionJson = ...
                
                result.Status = converged ? "SUCCESS" : "CONVERGENCE_FAILED";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during rigorous flash calculation");
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                result.Status = "FAILED";
            }

            return await Task.FromResult(result);
        }
    }
}
