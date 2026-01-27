using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        /// <summary>
        /// Performs nodal analysis (IPR/VLP) for a well to determine operating point and production optimization.
        /// Supports building reservoir and wellbore properties from request parameters or from PPDM data.
        /// </summary>
        /// <param name="request">Nodal analysis request containing well ID, reservoir/wellbore properties, and analysis parameters</param>
        /// <returns>Nodal analysis result with IPR/VLP curves, operating point, performance metrics, and recommendations</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when reservoir/wellbore properties are unavailable or calculation fails</exception>
        public async Task<Beep.OilandGas.Models.Data.Calculations.NodalAnalysisResult> PerformNodalAnalysisAsync(Beep.OilandGas.Models.Data.Calculations.NodalAnalysisRequest request)
        {
            string? resolvedWellId = null;

            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellUWI))
                {
                    throw new ArgumentException("WellUWI must be provided");
                }

                resolvedWellId = await GetWellIdByUwiAsync(request.WellUWI);
                if (string.IsNullOrEmpty(resolvedWellId))
                {
                    throw new InvalidOperationException($"Well not found for UWI: {request.WellUWI}");
                }

                _logger?.LogInformation("Starting Nodal Analysis for WellUWI: {WellUWI}, WellId: {WellId}",
                    request.WellUWI, resolvedWellId);

                // Step 1: Build reservoir properties from request or PPDM data
                ReservoirProperties reservoirProperties;
                if (request.ReservoirPressure.HasValue && request.ProductivityIndex.HasValue)
                {
                    // Use values from request
                    reservoirProperties = new ReservoirProperties
                    {
                        ReservoirPressure = (double)request.ReservoirPressure.Value,
                        BubblePointPressure = request.AdditionalParameters?.BubblePointPressure != null
                            ? (double)request.AdditionalParameters.BubblePointPressure.Value
                            : (double)(request.ReservoirPressure.Value * 0.8m),
                        ProductivityIndex = (double)request.ProductivityIndex.Value,
                        WaterCut = request.WaterCut.HasValue ? (double)request.WaterCut.Value / 100.0 : 0.0,
                        GasOilRatio = request.GasOilRatio.HasValue ? (double)request.GasOilRatio.Value : 0.0,
                        OilGravity = request.OilGravity.HasValue ? (double)request.OilGravity.Value : 35.0
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    reservoirProperties = await GetReservoirPropertiesForWellAsync(resolvedWellId);
                    
                    if (reservoirProperties == null || reservoirProperties.ReservoirPressure <= 0)
                    {
                        throw new InvalidOperationException("Reservoir properties not found or invalid. Provide ReservoirPressure and ProductivityIndex in request or ensure PPDM data is available.");
                    }
                }

                // Step 2: Build wellbore properties from request or PPDM data
                WellboreProperties wellboreProperties;
                if (request.TubingDiameter.HasValue && request.WellheadPressure.HasValue)
                {
                    // Use values from request
                    wellboreProperties = new WellboreProperties
                    {
                        TubingDiameter = (double)request.TubingDiameter.Value,
                        TubingLength = request.WellDepth.HasValue ? (double)request.WellDepth.Value : 8000.0,
                        WellheadPressure = (double)request.WellheadPressure.Value,
                        WaterCut = request.WaterCut.HasValue ? (double)request.WaterCut.Value / 100.0 : 0.0,
                        GasOilRatio = request.GasOilRatio.HasValue ? (double)request.GasOilRatio.Value : 0.0,
                        OilGravity = request.OilGravity.HasValue ? (double)request.OilGravity.Value : 35.0,
                        GasSpecificGravity = request.GasGravity.HasValue ? (double)request.GasGravity.Value : 0.65,
                        WellheadTemperature = request.Temperature.HasValue ? (double)request.Temperature.Value : 100.0,
                        BottomholeTemperature = request.Temperature.HasValue ? (double)request.Temperature.Value + 100.0 : 200.0
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    wellboreProperties = await GetWellborePropertiesForWellAsync(resolvedWellId);
                    
                    if (wellboreProperties == null || wellboreProperties.TubingDiameter <= 0)
                    {
                        throw new InvalidOperationException("Wellbore properties not found or invalid. Provide TubingDiameter and WellheadPressure in request or ensure PPDM data is available.");
                    }
                }

                // Step 3: Determine IPR method
                string iprMethod = request.IPRModel ?? "VOGEL";
                int numberOfPoints = request.NumberOfPoints ?? 50;
                double maxFlowRate = request.FlowRateRangeMax.HasValue 
                    ? (double)request.FlowRateRangeMax.Value 
                    : 5000.0; // Default 5000 BPD

                // Step 4: Generate IPR curve
                List<IPRPoint> iprCurve;
                try
                {
                    switch (iprMethod.ToUpperInvariant())
                    {
                        case "VOGEL":
                            iprCurve = IPRCalculator.GenerateVogelIPR(reservoirProperties, maxFlowRate, numberOfPoints);
                            break;
                        case "FETKOVICH":
                            // Fetkovich requires test points - use simplified approach
                            var testPoints = new List<(double flowRate, double pressure)>
                            {
                                (0, reservoirProperties.ReservoirPressure),
                                (maxFlowRate * 0.5, reservoirProperties.ReservoirPressure * 0.7),
                                (maxFlowRate, reservoirProperties.ReservoirPressure * 0.3)
                            };
                            iprCurve = IPRCalculator.GenerateFetkovichIPR(reservoirProperties, testPoints, maxFlowRate, numberOfPoints);
                            break;
                        default:
                            iprCurve = IPRCalculator.GenerateVogelIPR(reservoirProperties, maxFlowRate, numberOfPoints);
                            break;
                    }
                }
                catch (Exception iprEx)
                {
                    _logger?.LogError(iprEx, "Error generating IPR curve");
                    throw new InvalidOperationException($"IPR curve generation failed: {iprEx.Message}", iprEx);
                }

                // Step 5: Generate VLP curve
                double[] flowRates = iprCurve.Select(p => p.FlowRate).ToArray();
                List<VLPPoint> vlpCurve;
                try
                {
                    vlpCurve = VLPCalculator.GenerateVLP(wellboreProperties, flowRates);
                }
                catch (Exception vlpEx)
                {
                    _logger?.LogError(vlpEx, "Error generating VLP curve");
                    throw new InvalidOperationException($"VLP curve generation failed: {vlpEx.Message}", vlpEx);
                }

                // Step 6: Find operating point
                OperatingPoint operatingPoint;
                try
                {
                    operatingPoint = Beep.OilandGas.NodalAnalysis.Calculations.NodalAnalyzer.FindOperatingPoint(iprCurve, vlpCurve);
                }
                catch (Exception opEx)
                {
                    _logger?.LogError(opEx, "Error finding operating point");
                    throw new InvalidOperationException($"Operating point calculation failed: {opEx.Message}", opEx);
                }

                // Step 7: Map results to NodalAnalysisResult DTO
                var result = MapNodalAnalysisResultToDTO(
                    resolvedWellId, request, iprCurve, vlpCurve, operatingPoint, reservoirProperties, wellboreProperties);

                // Step 8: Store result in database
                var repository = await GetNodalResultRepositoryAsync();

                await repository.InsertAsync((object)result, request.UserId ?? "system");

                _logger?.LogInformation("Nodal Analysis calculation completed: {CalculationId}, Operating Flow Rate: {FlowRate} BPD, Operating Pressure: {Pressure} psi",
                    result.CalculationId, result.OperatingFlowRate, result.OperatingPressure);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Nodal Analysis");

                // Return error result
                var errorResult = new Beep.OilandGas.Models.Data.Calculations.NodalAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = resolvedWellId ?? string.Empty,
                    WellboreId = resolvedWellId ?? string.Empty,
                    FieldId = request.FieldId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    Status = "FAILED",
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    IPRCurve = new List<NodalCurvePoint>(),
                    VLPCurve = new List<NodalCurvePoint>(),
                    Recommendations = new List<string>(),
                    AdditionalResults = new NodalAnalysisAdditionalResults
                    {
                        WellUwi = request.WellUWI
                    }
                };

                // Try to store error result
                try
                {
                    var repository = await GetNodalResultRepositoryAsync();

                    await repository.InsertAsync((object)errorResult, request.UserId ?? "system");
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Nodal Analysis error result");
                }

                throw;
            }
        }

        #region Nodal Analysis Helper Methods

        /// <summary>
        /// Retrieves reservoir properties from PPDM for a well
        /// </summary>
        private async Task<ReservoirProperties?> GetReservoirPropertiesForWellAsync(string wellId)
        {
            try
            {
                if (string.IsNullOrEmpty(wellId))
                    return null;

                // Get productivity index from well test
                var pi = await GetWellTestProductivityIndexAsync(wellId);
                if (!pi.HasValue || pi.Value <= 0)
                {
                    _logger?.LogWarning("Productivity index not found for well {WellId}", wellId);
                    return null;
                }

                // Get static/reservoir pressure from well test
                var reservoirPressure = await GetWellTestStaticPressureAsync(wellId);
                if (!reservoirPressure.HasValue || reservoirPressure.Value <= 0)
                {
                    _logger?.LogWarning("Reservoir pressure not found for well {WellId}", wellId);
                    return null;
                }

                // Get bubble point pressure (try from well test or use default)
                var bubblePointPressure = await GetBubblePointPressureForWellAsync(wellId) 
                    ?? (double)(reservoirPressure.Value * 0.8m); // Default 80% of reservoir pressure

                // Get water cut from latest production data
                var waterCut = await GetWaterCutForWellAsync(wellId) ?? 0.0;

                // Get GOR from latest production data
                var gor = await GetGasOilRatioForWellAsync(wellId) ?? 0.0;

                // Get oil gravity (try from well or use default)
                var oilGravity = await GetOilGravityForWellAsync(wellId) ?? 35.0;

                return new ReservoirProperties
                {
                    ReservoirPressure = (double)reservoirPressure.Value,
                    BubblePointPressure = bubblePointPressure,
                    ProductivityIndex = (double)pi.Value,
                    WaterCut = waterCut,
                    GasOilRatio = gor,
                    OilGravity = oilGravity
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving reservoir properties for well {WellId}", wellId);
                return null;
            }
        }

        /// <summary>
        /// Gets bubble point pressure for a well
        /// </summary>
        private async Task<double?> GetBubblePointPressureForWellAsync(string wellId)
        {
            // Try to get from well test or reservoir data
            var mapping = await _fieldMappingService.GetFieldMappingAsync("Reservoir.BubblePointPressure");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetEntityAsync(mapping.TableName, wellId, "WELL_ID");
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Gets water cut for a well from production data
        /// </summary>
        private async Task<double?> GetWaterCutForWellAsync(string wellId)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", wellId) }
                };

                var entities = await GetEntitiesAsync("PDEN_VOL_SUMMARY", filters, "PRODUCTION_DATE", DataRetrievalMode.Latest);
                var latest = entities.FirstOrDefault();

                if (latest != null)
                {
                    var oilVol = GetPropertyValueMultiple(latest, "OIL_VOLUME", "OIL_PROD", "OIL_VOL") ?? 0;
                    var waterVol = GetPropertyValueMultiple(latest, "WATER_VOLUME", "WATER_PROD", "WATER_VOL") ?? 0;
                    var totalVol = oilVol + waterVol;

                    if (totalVol > 0)
                    {
                        return (double)(waterVol / totalVol);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting water cut for well {WellId}", wellId);
            }

            return null;
        }

        /// <summary>
        /// Gets gas-oil ratio for a well from production data
        /// </summary>
        private async Task<double?> GetGasOilRatioForWellAsync(string wellId)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", wellId) }
                };

                var entities = await GetEntitiesAsync("PDEN_VOL_SUMMARY", filters, "PRODUCTION_DATE", DataRetrievalMode.Latest);
                var latest = entities.FirstOrDefault();

                if (latest != null)
                {
                    var oilVol = GetPropertyValueMultiple(latest, "OIL_VOLUME", "OIL_PROD", "OIL_VOL") ?? 0;
                    var gasVol = GetPropertyValueMultiple(latest, "GAS_VOLUME", "GAS_PROD", "GAS_VOL") ?? 0;

                    if (oilVol > 0)
                    {
                        return (double)(gasVol / oilVol * 1000m); // Convert to SCF/STB
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting GOR for well {WellId}", wellId);
            }

            return null;
        }

        /// <summary>
        /// Gets oil gravity (API) for a well
        /// </summary>
        private async Task<double?> GetOilGravityForWellAsync(string wellId)
        {
            // Try to get from well test or fluid properties
            var mapping = await _fieldMappingService.GetFieldMappingAsync("Fluid.OilGravity");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetEntityAsync(mapping.TableName, wellId, "WELL_ID");
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves wellbore properties from PPDM for a well
        /// </summary>
        private async Task<WellboreProperties?> GetWellborePropertiesForWellAsync(string wellId)
        {
            try
            {
                if (string.IsNullOrEmpty(wellId))
                    return null;

                // Get tubing diameter from WELL_TUBULAR
                var tubingDiameter = await GetTubularOuterDiameterAsync(wellId, "TUBING");
                if (!tubingDiameter.HasValue || tubingDiameter.Value <= 0)
                {
                    _logger?.LogWarning("Tubing diameter not found for well {WellId}", wellId);
                    return null;
                }

                // Get tubing length/depth
                var tubingDepth = await GetTubularDepthAsync(wellId, "TUBING");
                var wellDepth = await GetWellTotalDepthAsync(wellId);
                var tubingLength = tubingDepth ?? wellDepth ?? 8000m; // Default 8000 ft

                // Get wellhead pressure (try from well test or use default)
                var wellheadPressure = await GetWellheadPressureForWellAsync(wellId) ?? 500m; // Default 500 psi

                // Get water cut and GOR (same as reservoir properties)
                var waterCut = await GetWaterCutForWellAsync(wellId) ?? 0.0;
                var gor = await GetGasOilRatioForWellAsync(wellId) ?? 0.0;

                // Get oil gravity
                var oilGravity = await GetOilGravityForWellAsync(wellId) ?? 35.0;

                // Get gas specific gravity (try from well test or use default)
                var gasGravity = await GetGasSpecificGravityForWellAsync(wellId) ?? 0.65;

                // Get temperatures (try from well test or use defaults)
                var wellheadTemp = await GetWellheadTemperatureForWellAsync(wellId) ?? 100.0;
                var bottomholeTemp = await GetBottomholeTemperatureForWellAsync(wellId) ?? (wellheadTemp + 100.0);

                return new WellboreProperties
                {
                    TubingDiameter = (double)tubingDiameter.Value,
                    TubingLength = (double)tubingLength,
                    WellheadPressure = (double)wellheadPressure,
                    WaterCut = waterCut,
                    GasOilRatio = gor,
                    OilGravity = oilGravity,
                    GasSpecificGravity = gasGravity,
                    WellheadTemperature = wellheadTemp,
                    BottomholeTemperature = bottomholeTemp
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving wellbore properties for well {WellId}", wellId);
                return null;
            }
        }

        /// <summary>
        /// Gets wellhead pressure for a well
        /// </summary>
        private async Task<decimal?> GetWellheadPressureForWellAsync(string wellId)
        {
            // Try to get from well test
            var mapping = await _fieldMappingService.GetFieldMappingAsync("WellTest.WellheadPressure");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetLatestEntityForWellAsync(mapping.TableName, wellId, "EFFECTIVE_DATE", null);
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return value.Value * (mapping.ConversionFactor ?? 1m);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets gas specific gravity for a well
        /// </summary>
        private async Task<double?> GetGasSpecificGravityForWellAsync(string wellId)
        {
            var mapping = await _fieldMappingService.GetFieldMappingAsync("Fluid.GasSpecificGravity");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetEntityAsync(mapping.TableName, wellId, "WELL_ID");
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Gets wellhead temperature for a well
        /// </summary>
        private async Task<double?> GetWellheadTemperatureForWellAsync(string wellId)
        {
            var mapping = await _fieldMappingService.GetFieldMappingAsync("WellTest.WellheadTemperature");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetLatestEntityForWellAsync(mapping.TableName, wellId, "EFFECTIVE_DATE", null);
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Gets bottomhole temperature for a well
        /// </summary>
        private async Task<double?> GetBottomholeTemperatureForWellAsync(string wellId)
        {
            var mapping = await _fieldMappingService.GetFieldMappingAsync("WellTest.BottomholeTemperature");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetLatestEntityForWellAsync(mapping.TableName, wellId, "EFFECTIVE_DATE", null);
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Maps NodalAnalysis results to NodalAnalysisResult DTO
        /// </summary>
        private NodalAnalysisResult MapNodalAnalysisResultToDTO(
            string wellId,
            NodalAnalysisRequest request,
            List<IPRPoint> iprCurve,
            List<VLPPoint> vlpCurve,
            OperatingPoint operatingPoint,
            ReservoirProperties reservoirProperties,
            WellboreProperties wellboreProperties)
        {
            var result = new NodalAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = wellId,
                WellboreId = wellId,
                FieldId = request.FieldId,
                AnalysisType = request.AnalysisType,
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId,
                OperatingFlowRate = (decimal)operatingPoint.FlowRate,
                OperatingPressure = (decimal)operatingPoint.BottomholePressure,
                OperatingTemperature = request.Temperature.HasValue ? request.Temperature : null,
                IPRCurve = new List<NodalCurvePoint>(),
                VLPCurve = new List<NodalCurvePoint>(),
                Recommendations = new List<string>(),
                AdditionalResults = new NodalAnalysisAdditionalResults
                {
                    WellUwi = request.WellUWI
                }
            };

            // Map IPR curve points
            foreach (var point in iprCurve)
            {
                result.IPRCurve.Add(new NodalCurvePoint
                {
                    FlowRate = (decimal)point.FlowRate,
                    Pressure = (decimal)point.FlowingBottomholePressure
                });
            }

            // Map VLP curve points
            foreach (var point in vlpCurve)
            {
                result.VLPCurve.Add(new NodalCurvePoint
                {
                    FlowRate = (decimal)point.FlowRate,
                    Pressure = (decimal)point.RequiredBottomholePressure
                });
            }

            // Calculate performance metrics
            if (iprCurve.Count > 0)
            {
                result.MaximumFlowRate = (decimal)iprCurve.Max(p => p.FlowRate);
                result.MinimumFlowRate = (decimal)iprCurve.Min(p => p.FlowRate);
                result.OptimalFlowRate = (decimal)operatingPoint.FlowRate;
            }

            if (vlpCurve.Count > 0 && iprCurve.Count > 0)
            {
                var maxVLP = vlpCurve.Max(p => p.RequiredBottomholePressure);
                var minIPR = iprCurve.Min(p => p.FlowingBottomholePressure);
                result.PressureDrop = (decimal)(maxVLP - minIPR);
            }

            // Calculate system efficiency (simplified)
            if (reservoirProperties.ReservoirPressure > 0)
            {
                double theoreticalMaxFlow = reservoirProperties.ProductivityIndex * reservoirProperties.ReservoirPressure;
                if (theoreticalMaxFlow > 0)
                {
                    result.SystemEfficiency = (decimal)((operatingPoint.FlowRate / theoreticalMaxFlow) * 100.0);
                }
            }

            // Generate recommendations
            if (result.MaximumFlowRate.HasValue && operatingPoint.FlowRate < (double)(result.MaximumFlowRate.Value * 0.8m))
            {
                result.Recommendations.Add("Consider optimizing well completion to increase flow rate");
            }

            if (result.PressureDrop > 1000)
            {
                result.Recommendations.Add("High pressure drop detected - consider larger tubing diameter");
            }

            if (result.SystemEfficiency < 50)
            {
                result.Recommendations.Add("Low system efficiency - review well configuration");
            }

            // Add additional results
            result.AdditionalResults.ReservoirPressure = (decimal)reservoirProperties.ReservoirPressure;
            result.AdditionalResults.ProductivityIndex = (decimal)reservoirProperties.ProductivityIndex;
            result.AdditionalResults.TubingDiameter = (decimal)wellboreProperties.TubingDiameter;
            result.AdditionalResults.TubingLength = (decimal)wellboreProperties.TubingLength;
            result.AdditionalResults.WellheadPressure = (decimal)wellboreProperties.WellheadPressure;
            result.AdditionalResults.IprMethod = request.IPRModel ?? "VOGEL";
            result.AdditionalResults.VlpModel = request.VLPModel ?? "HAGEDORN_BROWN";

            return result;
        }

        #endregion
    }
}
