using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
﻿using System;
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
using Beep.OilandGas.FlashCalculations.Constants;
using Beep.OilandGas.FlashCalculations.Validation;
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
        /// <exception cref="RefusalException">What was sent cannot be calculated, or no pressure and temperature are known for it.</exception>
        public async Task<Beep.OilandGas.Models.Data.Calculations.FlashCalculationResult> PerformFlashCalculationAsync(Beep.OilandGas.Models.Data.Calculations.FlashCalculationRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.FacilityId))
                {
                    throw RefusalException.Invalid("Choose the well or facility for the flash calculation.");
                }

                if (request.FeedComposition == null || request.FeedComposition.Count == 0)
                {
                    throw RefusalException.Invalid("Give the feed composition for the flash calculation.");
                }

                _logger?.LogInformation("Starting Flash Calculation for WellId: {WellId}, FacilityId: {FacilityId}",
                    request.WellId, request.FacilityId);

                // Step 1: Build flash conditions: pressure and temperature from the request, or from the well's
                // recorded data where the request leaves them out; the feed composition is the request's.
                var pressure = request.Pressure;
                var temperature = request.Temperature;
                if (!pressure.HasValue || !temperature.HasValue)
                {
                    var recorded = await GetFlashConditionsFromPPDMAsync(request.WellId ?? string.Empty);
                    pressure ??= recorded.Pressure;
                    temperature ??= recorded.Temperature;
                }

                if (!pressure.HasValue || !temperature.HasValue)
                {
                    throw RefusalException.Invalid(
                        "Give the pressure and the temperature for the flash calculation: none is recorded for this well.");
                }

                var FLASH_CONDITIONS = new FLASH_CONDITIONS
                {
                    PRESSURE = pressure.Value,
                    TEMPERATURE = temperature.Value,
                    // Map incoming calculation request components to FlashCalculations.FLASH_COMPONENT
                    FEED_COMPOSITION = request.FeedComposition.Select(c => new Beep.OilandGas.Models.Data.FlashCalculations.FLASH_COMPONENT
                    {
                        NAME = c.NAME,
                        COMPONENT_NAME = string.IsNullOrWhiteSpace(c.COMPONENT_NAME) ? c.NAME : c.COMPONENT_NAME,
                        MOLE_FRACTION = (decimal)c.MOLE_FRACTION,
                        CRITICAL_TEMPERATURE = (decimal)c.CRITICAL_TEMPERATURE,
                        CRITICAL_PRESSURE = (decimal)c.CRITICAL_PRESSURE,
                        ACENTRIC_FACTOR = (decimal)c.ACENTRIC_FACTOR,
                        MOLECULAR_WEIGHT = (decimal)c.MOLECULAR_WEIGHT
                    }).ToList()
                };

                FlashValidator.ValidateFlashConditions(FLASH_CONDITIONS);

                // Step 2: Perform flash calculation
                var flashResult = FlashCalculator.PerformIsothermalFlash(FLASH_CONDITIONS);

                // Step 3: Calculate phase properties
                var vaporProperties = FlashCalculator.CalculateVaporProperties(flashResult, FLASH_CONDITIONS);
                var liquidProperties = FlashCalculator.CalculateLiquidProperties(flashResult, FLASH_CONDITIONS);

                // Step 4: Map to DTO
                var eosRef = FlashEquationOfStateMapping.ToReferenceCode(request.AdditionalParameters?.EquationOfState);
                var result = MapFlashResultToDTO(flashResult, request, vaporProperties, liquidProperties, eosRef);

                // Step 5: Store result in PPDM database (the result is the caller's answer even when saving fails)
                result.Pressure = FLASH_CONDITIONS.PRESSURE;
                result.Temperature = FLASH_CONDITIONS.TEMPERATURE;
                result.FeedCompositionJson = JsonSerializer.Serialize(FLASH_CONDITIONS.FEED_COMPOSITION);
                result.VaporCompositionJson = JsonSerializer.Serialize(result.VaporComposition);
                result.LiquidCompositionJson = JsonSerializer.Serialize(result.LiquidComposition);
                result.KValuesJson = JsonSerializer.Serialize(result.KValues);
                await SaveCompletedRunAsync("flash calculation", async () =>
                {
                    var repository = await GetFlashResultRepositoryAsync();
                    // Avoid ambiguous overload resolution by casting to object
                    await InsertAnalysisResultAsync(repository, (object)result, request.UserId);
                    _logger?.LogInformation("Stored Flash Calculation result with ID: {CalculationId}", result.CalculationId);
                });

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            // Every other way the run ends short of a result is recorded in the calculation history, then goes on to the
            // caller: the API's handler answers a refusal with its sentence and reports anything else.
            catch (Exception ex)
            {
                var errorResult = new FlashCalculationResult
                {
                    CalculationId = _defaults.FormatIdForTable("FLASH_CALCULATION", Guid.NewGuid().ToString()),
                    WellId = request.WellId,
                    FacilityId = request.FacilityId,
                    CalculationDate = DateTime.UtcNow,
                    Status = "FAILED",
                    ErrorMessage = FailedRunMessage(ex, "flash calculation"),
                    AdditionalResults = new FlashCalculationAdditionalResults
                    {
                        EosModelReferenceCode = FlashEquationOfStateMapping.ToReferenceCode(request.AdditionalParameters?.EquationOfState)
                    }
                };
                await RecordFailedRunAsync("flash calculation", async () =>
                {
                    var repository = await GetFlashResultRepositoryAsync();
                    await InsertAnalysisResultAsync(repository, errorResult, request.UserId);
                });

                throw;
            }
        }

        #endregion

        #region Flash Calculation Helper Methods

        /// <summary>
        /// The flash pressure (psia) and temperature (Rankine) recorded for a well: the latest bottom-hole survey, then the
        /// latest well test for what that leaves out. Null where nothing is recorded — no value is made up.
        /// </summary>
        private async Task<(decimal? Pressure, decimal? Temperature)> GetFlashConditionsFromPPDMAsync(string wellId)
        {
            decimal? pressure = null;
            decimal? temperature = null;

            if (!string.IsNullOrEmpty(wellId))
            {
                // Try WELL_PRESSURE_BH for wellhead pressure and bottom-hole temperature
                var bhMeta = await _metadata.GetTableMetadataAsync("WELL_PRESSURE_BH");
                if (bhMeta != null)
                {
                    var bhType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{bhMeta.EntityTypeName}") ?? typeof(WELL_PRESSURE_BH);
                    var bhRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, bhType, _connectionName, "WELL_PRESSURE_BH");
                    var bhFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };
                    var bhResults = await bhRepo.GetAsync(bhFilters);
                    var bhRecord = bhResults?.OfType<WELL_PRESSURE_BH>().OrderByDescending(r => r.SURVEY_DATE).FirstOrDefault();
                    if (bhRecord != null)
                    {
                        if (bhRecord.WELL_HEAD_PRESSURE > 0) pressure = bhRecord.WELL_HEAD_PRESSURE;
                        if (bhRecord.RUN_DEPTH_TEMPERATURE > 0)
                            temperature = ToRankineFromPpdm(bhRecord.RUN_DEPTH_TEMPERATURE, bhRecord.RUN_DEPTH_TEMPERATURE_OUOM);
                    }
                }

                // Fall back to WELL_TEST for temperature/pressure if BH data missing
                if (!pressure.HasValue || !temperature.HasValue)
                {
                    var wtMeta = await _metadata.GetTableMetadataAsync("WELL_TEST");
                    if (wtMeta != null)
                    {
                        var wtType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{wtMeta.EntityTypeName}") ?? typeof(WELL_TEST);
                        var wtRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, wtType, _connectionName, "WELL_TEST");
                        var wtFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId },
                            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                        };
                        var wtResults = await wtRepo.GetAsync(wtFilters);
                        var wtRecord = wtResults?.OfType<WELL_TEST>().OrderByDescending(t => t.TEST_DATE).FirstOrDefault();
                        if (wtRecord != null)
                        {
                            if (!pressure.HasValue && wtRecord.FLOW_PRESSURE > 0)
                                pressure = wtRecord.FLOW_PRESSURE;
                            if (!temperature.HasValue && wtRecord.FLOW_TEMPERATURE > 0)
                                temperature = ToRankineFromPpdm(wtRecord.FLOW_TEMPERATURE, wtRecord.FLOW_TEMPERATURE_OUOM);
                        }
                    }
                }
            }

            _logger?.LogInformation("Flash conditions recorded for WellId: {WellId}, P={Pressure}, T={Temperature}", wellId, pressure, temperature);

            return (pressure, temperature);
        }

        /// <summary>
        /// Maps FlashResult from library to FlashCalculationResult DTO
        /// </summary>
        private FlashCalculationResult MapFlashResultToDTO(
            FlashResult flashResult,
            FlashCalculationRequest request,
            PhasePropertiesData vaporProperties,
            PhasePropertiesData liquidProperties,
            string eosModelReferenceCode)
        {
            var result = new FlashCalculationResult
            {
                CalculationId = _defaults.FormatIdForTable("FLASH_CALCULATION", Guid.NewGuid().ToString()),
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
                    Volume = vaporProperties.Volume ?? 0.0m
                },
                LiquidProperties = new PhasePropertiesData
                {
                    Density = (decimal)liquidProperties.Density,
                    MolecularWeight = (decimal)liquidProperties.MolecularWeight,
                    SpecificGravity = (decimal)liquidProperties.SpecificGravity,
                    Volume = liquidProperties.Volume ?? 0.0m
                },
                AdditionalResults = new FlashCalculationAdditionalResults()
            };

            result.AdditionalResults.Pressure = request.Pressure ?? 0.0m;
            result.AdditionalResults.Temperature = request.Temperature ?? 0.0m;
            result.AdditionalResults.ComponentCount = request.FeedComposition?.Count ?? 0;
            result.AdditionalResults.EosModelReferenceCode = eosModelReferenceCode;

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
