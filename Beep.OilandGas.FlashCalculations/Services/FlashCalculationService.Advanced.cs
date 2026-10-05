using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.FlashCalculations.Constants;
using Beep.OilandGas.FlashCalculations.Validation;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.FlashCalculations.Services
{
    public partial class FlashCalculationService
    {
        /// <inheritdoc />
        public async Task<FlashCalculationResult> RunRigorousFlashAsync(
            FlashCalculationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Pressure == null || request.Temperature == null)
                throw RefusalException.Invalid("Pressure and Temperature are required for PT Flash.");
            if (request.FeedComposition == null || !request.FeedComposition.Any())
                throw RefusalException.Invalid("Feed composition is required.");

            cancellationToken.ThrowIfCancellationRequested();

            _logger?.LogInformation("Starting Rigorous PT Flash at P={P}, T={T}", request.Pressure, request.Temperature);

            var eosRef = FlashEquationOfStateMapping.ToReferenceCode(request.AdditionalParameters?.EquationOfState);
            var result = new FlashCalculationResult
            {
                CalculationId = _defaults.FormatIdForTable("FLASH_CALCULATION", Guid.NewGuid().ToString()),
                CalculationDate = DateTime.UtcNow,
                CalculationType = "PT_FLASH_RIGOROUS",
                Pressure = request.Pressure,
                Temperature = request.Temperature,
                FeedComposition = request.FeedComposition,
                IsSuccessful = true,
                AdditionalResults = new FlashCalculationAdditionalResults
                {
                    Pressure = request.Pressure,
                    Temperature = request.Temperature,
                    ComponentCount = request.FeedComposition.Count,
                    EosModelReferenceCode = eosRef
                }
            };

            // Conditions the validator refuses, and a calculation that fails, reach the caller (OILGAS-CATCH-01): the catch
            // here had answered both as an unsuccessful result carrying the exception's text.
            var components = request.FeedComposition.Cast<FLASH_COMPONENT>().ToList();
            FlashValidator.ValidateFlashConditions(new FLASH_CONDITIONS
            {
                PRESSURE = request.Pressure!.Value,
                TEMPERATURE = request.Temperature!.Value,
                FEED_COMPOSITION = components
            });
            cancellationToken.ThrowIfCancellationRequested();

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

            result.VaporComposition = new List<FlashComponentFraction>();
            result.LiquidComposition = new List<FlashComponentFraction>();
            result.KValues = new List<FlashComponentKValue>();

            foreach (var phaseRes in phaseResults)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            result.Status = converged ? "SUCCESS" : "CONVERGENCE_FAILED";

            return await Task.FromResult(result);
        }
    }
}
