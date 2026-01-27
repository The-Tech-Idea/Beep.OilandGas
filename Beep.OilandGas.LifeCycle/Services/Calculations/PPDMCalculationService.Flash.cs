using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;

using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        #region Flash Calculation

        /// <summary>
        /// Performs flash calculation (phase equilibrium) for a well or facility.
        /// Supports isothermal flash calculations with vapor-liquid equilibrium.
        /// </summary>
        /// <param name="request">Flash calculation request containing well/facility ID, pressure, temperature, and feed composition</param>
        /// <returns>Flash calculation result with vapor/liquid fractions, phase compositions, K-values, and phase properties</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when feed composition is unavailable or calculation fails</exception>
        public async Task<Beep.OilandGas.Models.Data.Calculations.FlashCalculationResult> PerformFlashCalculationAsync(Beep.OilandGas.Models.Data.Calculations.FlashCalculationRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.FacilityId))
                {
                    throw new ArgumentException("At least one of WellId or FacilityId must be provided");
                }

                if (request.FeedComposition == null || request.FeedComposition.Count == 0)
                {
                    throw new ArgumentException("Feed composition must be provided");
                }

                _logger?.LogInformation("Starting Flash Calculation for WellId: {WellId}, FacilityId: {FacilityId}",
                    request.WellId, request.FacilityId);

                // Step 1: Build flash conditions from request or PPDM data
                FlashConditions flashConditions;
                if (request.Pressure.HasValue && request.Temperature.HasValue && request.FeedComposition != null)
                {
                    // Use values from request (model types use decimal)
                    flashConditions = new FlashConditions
                    {
                        Pressure = (decimal)request.Pressure.Value,
                        Temperature = (decimal)request.Temperature.Value,
                        // Map incoming calculation request components to FlashCalculations.FlashComponent
                        FeedComposition = request.FeedComposition.Select(c => new Beep.OilandGas.Models.Data.FlashCalculations.FlashComponent
                        {
                            Name = c.Name,
                            MoleFraction = (decimal)c.MoleFraction,
                            CriticalTemperature = (decimal)c.CriticalTemperature,
                            CriticalPressure = (decimal)c.CriticalPressure,
                            AcentricFactor = (decimal)c.AcentricFactor,
                            MolecularWeight = (decimal)c.MolecularWeight
                        }).ToList()
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    flashConditions = await GetFlashConditionsFromPPDMAsync(request.WellId ?? string.Empty, request.FacilityId ?? string.Empty);
                }

                // Step 2: Perform flash calculation
                var flashResult = FlashCalculator.PerformIsothermalFlash(flashConditions);

                // Step 3: Calculate phase properties
                var vaporProperties = FlashCalculator.CalculateVaporProperties(flashResult, flashConditions);
                var liquidProperties = FlashCalculator.CalculateLiquidProperties(flashResult, flashConditions);

                // Step 4: Map to DTO
                var result = MapFlashResultToDTO(flashResult, request, vaporProperties, liquidProperties);

                // Step 5: Store result in PPDM database
                try
                {
                    var repository = await GetFlashResultRepositoryAsync();
                    result.Pressure = flashConditions.Pressure;
                    result.Temperature = flashConditions.Temperature;
                    result.FeedCompositionJson = JsonSerializer.Serialize(flashConditions.FeedComposition);
                    result.VaporCompositionJson = JsonSerializer.Serialize(result.VaporComposition);
                    result.LiquidCompositionJson = JsonSerializer.Serialize(result.LiquidComposition);
                    result.KValuesJson = JsonSerializer.Serialize(result.KValues);

                    // Avoid ambiguous overload resolution by casting to object
                    await InsertAnalysisResultAsync(repository, (object)result, request.UserId);
                    _logger?.LogInformation("Stored Flash Calculation result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Flash Calculation result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Flash Calculation for WellId: {WellId}, FacilityId: {FacilityId}",
                    request.WellId, request.FacilityId);

                // Try to store error result
                try
                {
                    var repository = await GetFlashResultRepositoryAsync();
                    var errorResult = new FlashCalculationResult
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        WellId = request.WellId,
                        FacilityId = request.FacilityId,
                        CalculationDate = DateTime.UtcNow,
                        Status = "FAILED",
                        ErrorMessage = ex.Message
                    };
                    await InsertAnalysisResultAsync(repository, errorResult, request.UserId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Flash Calculation error result");
                }

                throw;
            }
        }

        #endregion

        #region Flash Calculation Helper Methods

        /// <summary>
        /// Retrieves flash conditions from PPDM for a well or facility
        /// </summary>
        private Task<FlashConditions> GetFlashConditionsFromPPDMAsync(string wellId, string facilityId)
        {
            _logger?.LogWarning("Flash conditions retrieval from PPDM is not implemented. " +
                "Provide Pressure, Temperature, and FeedComposition in the request.");

            return Task.FromException<FlashConditions>(new InvalidOperationException(
                "Flash conditions retrieval from PPDM is not implemented. " +
                "Provide Pressure, Temperature, and FeedComposition in the request."));
        }

        /// <summary>
        /// Maps FlashResult from library to FlashCalculationResult DTO
        /// </summary>
        private FlashCalculationResult MapFlashResultToDTO(
            FlashResult flashResult,
            FlashCalculationRequest request,
            PhasePropertiesData vaporProperties,
            PhasePropertiesData liquidProperties)
        {
            var result = new FlashCalculationResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                FacilityId = request.FacilityId,
                CalculationType = request.CalculationType,
                CalculationDate = DateTime.UtcNow,
                Status = flashResult.Converged ? "SUCCESS" : "PARTIAL",
                UserId = request.UserId,
                VaporFraction = (decimal)flashResult.VaporFraction,
                LiquidFraction = (decimal)flashResult.LiquidFraction,
                // Assign list-based compositions/k-values directly
                VaporComposition = flashResult.VaporComposition,
                LiquidComposition = flashResult.LiquidComposition,
                KValues = flashResult.KValues,
                Iterations = flashResult.Iterations,
                Converged = flashResult.Converged,
                ConvergenceError = (decimal)flashResult.ConvergenceError,
                VaporProperties = new PhasePropertiesData
                {
                    Density = (decimal)vaporProperties.Density,
                    MolecularWeight = (decimal)vaporProperties.MolecularWeight,
                    SpecificGravity = (decimal)vaporProperties.SpecificGravity,
                    Volume = (decimal)vaporProperties.Volume
                },
                LiquidProperties = new PhasePropertiesData
                {
                    Density = (decimal)liquidProperties.Density,
                    MolecularWeight = (decimal)liquidProperties.MolecularWeight,
                    SpecificGravity = (decimal)liquidProperties.SpecificGravity,
                    Volume = (decimal)liquidProperties.Volume
                },
                AdditionalResults = new FlashCalculationAdditionalResults()
            };

            result.AdditionalResults.Pressure = request.Pressure ?? 0.0m;
            result.AdditionalResults.Temperature = request.Temperature ?? 0.0m;
            result.AdditionalResults.ComponentCount = request.FeedComposition?.Count ?? 0;

            return result;
        }

        /// <summary>
        /// Gets default critical temperature for a component name
        /// </summary>
        private decimal GetDefaultCriticalTemperature(string componentName)
        {
            // Common component critical temperatures (Rankine)
            return componentName.ToUpper() switch
            {
                "METHANE" or "CH4" => 343.0m,
                "ETHANE" or "C2H6" => 549.7m,
                "PROPANE" or "C3H8" => 665.7m,
                "BUTANE" or "C4H10" => 765.3m,
                "PENTANE" or "C5H12" => 845.4m,
                "HEXANE" or "C6H14" => 913.4m,
                _ => 500.0m // Default
            };
        }

        /// <summary>
        /// Gets default critical pressure for a component name
        /// </summary>
        private decimal GetDefaultCriticalPressure(string componentName)
        {
            // Common component critical pressures (psia)
            return componentName.ToUpper() switch
            {
                "METHANE" or "CH4" => 667.8m,
                "ETHANE" or "C2H6" => 707.8m,
                "PROPANE" or "C3H8" => 616.3m,
                "BUTANE" or "C4H10" => 550.7m,
                "PENTANE" or "C5H12" => 488.6m,
                "HEXANE" or "C6H14" => 436.9m,
                _ => 500.0m // Default
            };
        }

        /// <summary>
        /// Gets default acentric factor for a component name
        /// </summary>
        private decimal GetDefaultAcentricFactor(string componentName)
        {
            return componentName.ToUpper() switch
            {
                "METHANE" or "CH4" => 0.0115m,
                "ETHANE" or "C2H6" => 0.0995m,
                "PROPANE" or "C3H8" => 0.1521m,
                "BUTANE" or "C4H10" => 0.2002m,
                "PENTANE" or "C5H12" => 0.2515m,
                "HEXANE" or "C6H14" => 0.3007m,
                _ => 0.2m // Default
            };
        }

        /// <summary>
        /// Gets default molecular weight for a component name
        /// </summary>
        private decimal GetDefaultMolecularWeight(string componentName)
        {
            return componentName.ToUpper() switch
            {
                "METHANE" or "CH4" => 16.04m,
                "ETHANE" or "C2H6" => 30.07m,
                "PROPANE" or "C3H8" => 44.10m,
                "BUTANE" or "C4H10" => 58.12m,
                "PENTANE" or "C5H12" => 72.15m,
                "HEXANE" or "C6H14" => 86.18m,
                _ => 50.0m // Default
            };
        }

        #endregion
    }
}
