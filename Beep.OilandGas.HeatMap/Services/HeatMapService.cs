using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.HeatMap.Configuration;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HeatMap;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using Beep.OilandGas.Models.Data.HeatMap;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.HeatMap.Services
{
    /// <summary>
    /// Service for heat map generation and management.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class HeatMapService : IHeatMapService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<HeatMapService>? _logger;

        public HeatMapService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<HeatMapService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public async Task<HeatMapResult> GenerateHeatMapAsync(List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> dataPoints, HeatMapConfigurationRecord configuration)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                throw new ArgumentException("Data points cannot be null or empty", nameof(dataPoints));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _logger?.LogInformation("Generating heat map with {PointCount} data points", dataPoints.Count);

            // Convert from Models.HeatMap.HeatMapDataPoint to HeatMap.HeatMapDataPoint for the generator
            var generatorDataPoints = dataPoints.Select(p => new HeatMapDataPoint(p.OriginalX, p.OriginalY, p.Value, p.Label)
            {
                X = p.X,
                Y = p.Y
            }).ToList();

            // Use the existing HeatMapGenerator
            var generator = new HeatMapGenerator(generatorDataPoints, 800, 600, SKColors.Blue);
            
            // Generate the heat map
            var result = new HeatMapResult
            {
                HeatMapId = _defaults.FormatIdForTable("HEAT_MAP", Guid.NewGuid().ToString()),
                HeatMapName = $"HeatMap_{DateTime.UtcNow:yyyyMMddHHmmss}",
                GeneratedDate = DateTime.UtcNow,
                DataPoints = dataPoints,
                Configuration = configuration
            };

            _logger?.LogInformation("Heat map generated: {HeatMapId}", result.HeatMapId);

            await Task.CompletedTask;
            return result;
        }

        public async Task<string> SaveHeatMapConfigurationAsync(HeatMapConfigurationRecord configuration, string userId)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving heat map configuration {ConfigurationId}", configuration.ConfigurationId);

            if (string.IsNullOrWhiteSpace(configuration.ConfigurationId))
            {
                configuration.ConfigurationId = _defaults.FormatIdForTable("HEAT_MAP", Guid.NewGuid().ToString());
            }

            // Create repository for HEAT_MAP_CONFIGURATION
            var configRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(HEAT_MAP_CONFIGURATION), _connectionName, "HEAT_MAP_CONFIGURATION", null);

            var newEntity = new HEAT_MAP_CONFIGURATION
            {
                HEAT_MAP_ID = configuration.ConfigurationId,
                CONFIGURATION_NAME = configuration.ConfigurationName ?? string.Empty,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await configRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully saved heat map configuration {ConfigurationId}", configuration.ConfigurationId);
            return configuration.ConfigurationId;
        }

        public async Task<HeatMapConfigurationRecord?> GetHeatMapConfigurationAsync(string heatMapId)
        {
            if (string.IsNullOrWhiteSpace(heatMapId))
            {
                _logger?.LogWarning("GetHeatMapConfigurationAsync called with null or empty heatMapId");
                return null;
            }

            _logger?.LogInformation("Getting heat map configuration {HeatMapId}", heatMapId);

            // Create repository for HEAT_MAP_CONFIGURATION
            var configRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(HEAT_MAP_CONFIGURATION), _connectionName, "HEAT_MAP_CONFIGURATION", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "HEAT_MAP_ID", Operator = "=", FilterValue = heatMapId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await configRepo.GetAsync(filters);
            var entity = entities.Cast<HEAT_MAP_CONFIGURATION>().FirstOrDefault();

            if (entity == null)
            {
                _logger?.LogWarning("Heat map configuration {HeatMapId} not found", heatMapId);
                return null;
            }

            var config = new HeatMapConfigurationRecord
            {
                ConfigurationId = entity.HEAT_MAP_ID ?? string.Empty,
                ConfigurationName = entity.CONFIGURATION_NAME ?? string.Empty,
                CreatedDate = entity.ROW_CREATED_DATE ?? DateTime.UtcNow
            };

            _logger?.LogInformation("Successfully retrieved heat map configuration {HeatMapId}", heatMapId);
            return config;
        }

         public async Task<HeatMapResult> GenerateProductionHeatMapAsync(string fieldId, DateTime startDate, DateTime endDate)
         {
             if (string.IsNullOrWhiteSpace(fieldId))
                 throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));

             _logger?.LogInformation("Generating production heat map for field {FieldId} from {StartDate} to {EndDate}",
                 fieldId, startDate, endDate);

             // TODO: Retrieve production data from database and generate heat map
             // For now, return empty result
             var result = new HeatMapResult
             {
                 HeatMapId = _defaults.FormatIdForTable("HEAT_MAP", Guid.NewGuid().ToString()),
                 HeatMapName = $"ProductionHeatMap_{fieldId}_{startDate:yyyyMMdd}",
                 GeneratedDate = DateTime.UtcNow,
                 DataPoints = new List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint>(),
                 Configuration = new HeatMapConfigurationRecord()
             };

             _logger?.LogWarning("GenerateProductionHeatMapAsync not fully implemented - requires production data integration");

             await Task.CompletedTask;
             return result;
         }

         /// <summary>
         /// Performs comprehensive thermal analysis on heat map data
         /// Calculates temperature statistics, patterns, and thermal characteristics
         /// </summary>
         public async Task<ThermalAnalysisResult> AnalyzeThermalPatternAsync(
             string locationId,
             List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> dataPoints)
         {
             if (string.IsNullOrWhiteSpace(locationId))
                 throw new ArgumentException("Location ID cannot be null or empty", nameof(locationId));
             if (dataPoints == null || dataPoints.Count == 0)
                 throw new ArgumentException("Data points cannot be null or empty", nameof(dataPoints));

             _logger?.LogInformation("Analyzing thermal pattern for location {LocationId} with {Count} data points",
                 locationId, dataPoints.Count);

             var temperatures = dataPoints.Select(p => (decimal)p.Value).OrderBy(v => v).ToList();
             decimal average = temperatures.Average();
             decimal max = temperatures.Max();
             decimal min = temperatures.Min();
             decimal range = max - min;
             
             // Calculate standard deviation
             decimal sumSquareDiff = temperatures.Sum(t => (t - average) * (t - average));
             decimal stdDev = (decimal)Math.Sqrt((double)(sumSquareDiff / temperatures.Count));

             // Determine thermal pattern
             string pattern = DetermineThermalPattern(average, stdDev, temperatures);
             
             // Calculate gradient
             decimal gradient = range > 0 ? range / dataPoints.Count : 0;

             var result = new ThermalAnalysisResult
             {
                 AnalysisId = _defaults.FormatIdForTable("THERMAL_ANALYSIS", Guid.NewGuid().ToString()),
                 LocationId = locationId,
                 AnalysisDate = DateTime.UtcNow,
                 AverageTemperature = average,
                 MaximumTemperature = max,
                 MinimumTemperature = min,
                 TemperatureGradient = gradient,
                 StandardDeviation = stdDev,
                 ThermalPattern = pattern,
                 DataPointCount = dataPoints.Count,
                 TemperatureRange = range
             };

             _logger?.LogInformation("Thermal analysis complete: Pattern={Pattern}, Avg={Avg}°C, Range={Range}°C",
                 pattern, average, range);

             return await Task.FromResult(result);
         }

         /// <summary>
         /// Detects thermal anomalies in heat map data
         /// Identifies outliers and unusual temperature zones
         /// </summary>
         public async Task<List<ThermalAnomaly>> DetectThermalAnomaliesAsync(
             string locationId,
             List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> dataPoints,
             decimal stdDevThreshold = 2.0m)
         {
             if (string.IsNullOrWhiteSpace(locationId))
                 throw new ArgumentException("Location ID cannot be null or empty", nameof(locationId));
             if (dataPoints == null || dataPoints.Count == 0)
                 throw new ArgumentException("Data points cannot be null or empty", nameof(dataPoints));

             _logger?.LogInformation("Detecting thermal anomalies for location {LocationId} with threshold {Threshold}",
                 locationId, stdDevThreshold);

             var temperatures = dataPoints.Select(p => (decimal)p.Value).ToList();
             decimal average = temperatures.Average();
             decimal sumSquareDiff = temperatures.Sum(t => (t - average) * (t - average));
             decimal stdDev = (decimal)Math.Sqrt((double)(sumSquareDiff / temperatures.Count));

             var anomalies = new List<ThermalAnomaly>();

             for (int i = 0; i < dataPoints.Count; i++)
             {
                 decimal dataPointValue = (decimal)dataPoints[i].Value;
                 decimal deviation = Math.Abs(dataPointValue - average);
                 decimal deviationInStdDev = deviation / (stdDev > 0 ? stdDev : 1);

                 if (deviationInStdDev >= stdDevThreshold)
                 {
                     string anomalyType = dataPointValue > average ? "Hot Anomaly" : "Cold Anomaly";
                     string severity = deviationInStdDev > 4 ? "Critical" : 
                                      deviationInStdDev > 3 ? "High" : 
                                      deviationInStdDev > 2.5m ? "Medium" : "Low";

                     var anomaly = new ThermalAnomaly
                     {
                         AnomalyId = _defaults.FormatIdForTable("ANOMALY", Guid.NewGuid().ToString()),
                         LocationId = locationId,
                         DetectionDate = DateTime.UtcNow,
                         AnomalyTemperature = dataPointValue,
                         ExpectedTemperature = average,
                         TemperatureDeviation = deviation,
                         DeviationPercent = average > 0m ? (deviation / average) * 100m : 0m,
                         AnomalyType = anomalyType,
                         Severity = severity,
                         X = (decimal)dataPoints[i].X,
                         Y = (decimal)dataPoints[i].Y,
                         Description = $"{anomalyType} detected at ({dataPoints[i].X}, {dataPoints[i].Y})",
                         RecommendedActions = GenerateAnomalyRecommendations(anomalyType, severity)
                     };

                     anomalies.Add(anomaly);
                 }
             }

             _logger?.LogInformation("Detected {Count} thermal anomalies in location {LocationId}",
                 anomalies.Count, locationId);

             return await Task.FromResult(anomalies);
         }

         /// <summary>
         /// Analyzes temperature trends over time using historical data
         /// Performs linear regression and forecasts future temperatures
         /// </summary>
         public async Task<ThermalTrendAnalysis> AnalyzeTemperatureTrendAsync(
             string locationId,
             List<decimal> historicalTemperatures,
             int forecastMonths = 6)
         {
             if (string.IsNullOrWhiteSpace(locationId))
                 throw new ArgumentException("Location ID cannot be null or empty", nameof(locationId));
             if (historicalTemperatures == null || historicalTemperatures.Count < 2)
                 throw new ArgumentException("At least 2 historical temperature values required", nameof(historicalTemperatures));

             _logger?.LogInformation("Analyzing temperature trend for location {LocationId} with {Count} historical points",
                 locationId, historicalTemperatures.Count);

             // Calculate linear regression
             int n = historicalTemperatures.Count;
             decimal sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

             for (int i = 0; i < n; i++)
             {
                 sumX += i;
                 sumY += historicalTemperatures[i];
                 sumXY += i * historicalTemperatures[i];
                 sumX2 += i * i;
             }

             decimal slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
             decimal intercept = (sumY - slope * sumX) / n;

             // Calculate R-squared (trend confidence)
             decimal meanY = sumY / n;
             decimal ssTotal = historicalTemperatures.Sum(t => (t - meanY) * (t - meanY));
             decimal ssPred = 0;
             for (int i = 0; i < n; i++)
             {
                 decimal predicted = intercept + slope * i;
                 ssPred += (predicted - meanY) * (predicted - meanY);
             }
             decimal rSquared = ssTotal > 0 ? ssPred / ssTotal : 0;

             // Forecast future temperature
             decimal predictedTemp = intercept + slope * (n + forecastMonths);

             string direction = slope > 0 ? "Increasing" : slope < 0 ? "Decreasing" : "Stable";

             var result = new ThermalTrendAnalysis
             {
                 TrendId = _defaults.FormatIdForTable("TREND", Guid.NewGuid().ToString()),
                 LocationId = locationId,
                 AnalysisDate = DateTime.UtcNow,
                 MonthsAnalyzed = historicalTemperatures.Count,
                 TemperatureTrend = slope,
                 TrendSlope = slope,
                 TrendDirection = direction,
                 PercentChange = historicalTemperatures[0] > 0 ? 
                     ((historicalTemperatures[n-1] - historicalTemperatures[0]) / historicalTemperatures[0]) * 100 : 0,
                 HistoricalTemperatures = historicalTemperatures,
                 PredictedTemperature = predictedTemp,
                 PredictionMonths = forecastMonths,
                 RSquared = rSquared
             };

             _logger?.LogInformation("Temperature trend analysis complete: Direction={Direction}, Slope={Slope}, R²={RSquared}",
                 direction, slope, rSquared);

             return await Task.FromResult(result);
         }

         /// <summary>
         /// Analyzes temperature gradients to identify spatial temperature changes
         /// </summary>
         public async Task<TemperatureGradientAnalysis> AnalyzeTemperatureGradientAsync(
             string locationId,
             List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> dataPoints)
         {
             if (string.IsNullOrWhiteSpace(locationId))
                 throw new ArgumentException("Location ID cannot be null or empty", nameof(locationId));
             if (dataPoints == null || dataPoints.Count < 2)
                 throw new ArgumentException("At least 2 data points required for gradient analysis", nameof(dataPoints));

             _logger?.LogInformation("Analyzing temperature gradient for location {LocationId}",
                 locationId);

             // Sort points by distance from origin
             var sortedPoints = dataPoints.OrderBy(p => Math.Sqrt(p.X * p.X + p.Y * p.Y)).ToList();

             var gradientPoints = new List<GradientPoint>();
             decimal totalHorizontalGradient = 0;
             decimal totalVerticalGradient = 0;

             for (int i = 1; i < sortedPoints.Count; i++)
             {
                 decimal distDiff = (decimal)Math.Sqrt(
                     Math.Pow(sortedPoints[i].X - sortedPoints[i-1].X, 2) +
                     Math.Pow(sortedPoints[i].Y - sortedPoints[i-1].Y, 2));
                 
                 decimal tempDiff = (decimal)sortedPoints[i].Value - (decimal)sortedPoints[i-1].Value;
                 decimal localGradient = distDiff > 0 ? tempDiff / distDiff : 0;

                 gradientPoints.Add(new GradientPoint
                 {
                     Distance = (decimal)Math.Sqrt(sortedPoints[i].X * sortedPoints[i].X + sortedPoints[i].Y * sortedPoints[i].Y),
                     Temperature = (decimal)sortedPoints[i].Value,
                     LocalGradient = localGradient
                 });

                 // Calculate directional gradients
                 if (sortedPoints[i].X != sortedPoints[i-1].X)
                     totalHorizontalGradient += tempDiff / (decimal)(sortedPoints[i].X - sortedPoints[i-1].X);
                 if (sortedPoints[i].Y != sortedPoints[i-1].Y)
                     totalVerticalGradient += tempDiff / (decimal)(sortedPoints[i].Y - sortedPoints[i-1].Y);
             }

             decimal avgGradient = gradientPoints.Count > 0 ? gradientPoints.Average(gp => gp.LocalGradient) : 0;
             decimal maxGradient = gradientPoints.Count > 0 ? gradientPoints.Max(gp => gp.LocalGradient) : 0;
             decimal minGradient = gradientPoints.Count > 0 ? gradientPoints.Min(gp => gp.LocalGradient) : 0;

             var result = new TemperatureGradientAnalysis
             {
                 GradientId = _defaults.FormatIdForTable("GRADIENT", Guid.NewGuid().ToString()),
                 LocationId = locationId,
                 AnalysisDate = DateTime.UtcNow,
                 AverageGradient = avgGradient,
                 MaxGradient = maxGradient,
                 MinGradient = minGradient,
                 HorizontalGradient = totalHorizontalGradient / Math.Max(1, gradientPoints.Count),
                 VerticalGradient = totalVerticalGradient / Math.Max(1, gradientPoints.Count),
                 GradientPattern = DetermineGradientPattern(gradientPoints),
                 GradientPoints = gradientPoints
             };

             _logger?.LogInformation("Temperature gradient analysis complete: AvgGradient={Avg}, MaxGradient={Max}",
                 avgGradient, maxGradient);

             return await Task.FromResult(result);
         }

         /// <summary>
         /// Identifies and classifies temperature zones (hot, normal, cold)
         /// </summary>
         public async Task<List<TemperatureZone>> IdentifyTemperatureZonesAsync(
             string locationId,
             List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> dataPoints)
         {
             if (string.IsNullOrWhiteSpace(locationId))
                 throw new ArgumentException("Location ID cannot be null or empty", nameof(locationId));
             if (dataPoints == null || dataPoints.Count == 0)
                 throw new ArgumentException("Data points cannot be null or empty", nameof(dataPoints));

             _logger?.LogInformation("Identifying temperature zones for location {LocationId}",
                 locationId);

             var temperatures = dataPoints.Select(p => (decimal)p.Value).OrderBy(v => v).ToList();
             decimal min = temperatures.First();
             decimal max = temperatures.Last();
             decimal range = max - min;
             decimal thirdRange = range / 3;

             var zones = new List<TemperatureZone>();

             // Create three zones: Cold, Normal, Hot
             var coldPoints = dataPoints.Where(p => (decimal)p.Value < min + thirdRange).ToList();
             var normalPoints = dataPoints.Where(p => (decimal)p.Value >= min + thirdRange && (decimal)p.Value < min + 2 * thirdRange).ToList();
             var hotPoints = dataPoints.Where(p => (decimal)p.Value >= min + 2 * thirdRange).ToList();

             zones.AddRange(CreateTemperatureZones(locationId, coldPoints, "Cold Zone", min, min + thirdRange));
             zones.AddRange(CreateTemperatureZones(locationId, normalPoints, "Normal Zone", min + thirdRange, min + 2 * thirdRange));
             zones.AddRange(CreateTemperatureZones(locationId, hotPoints, "Hot Zone", min + 2 * thirdRange, max));

             _logger?.LogInformation("Identified {Count} temperature zones for location {LocationId}",
                 zones.Count, locationId);

             return await Task.FromResult(zones);
         }

         /// <summary>
         /// Assesses the quality of thermal imaging data
         /// </summary>
         public async Task<ThermalImageQuality> AssessThermalImageQualityAsync(
             string imageId,
             List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> dataPoints)
         {
             if (string.IsNullOrWhiteSpace(imageId))
                 throw new ArgumentException("Image ID cannot be null or empty", nameof(imageId));
             if (dataPoints == null || dataPoints.Count == 0)
                 throw new ArgumentException("Data points cannot be null or empty", nameof(dataPoints));

             _logger?.LogInformation("Assessing thermal image quality for image {ImageId}",
                 imageId);

             var temperatures = dataPoints.Select(p => (decimal)p.Value).ToList();
             decimal average = temperatures.Average();
             decimal variance = temperatures.Sum(t => (t - average) * (t - average)) / temperatures.Count;
             decimal stdDev = (decimal)Math.Sqrt((double)variance);

             // Calculate quality metrics (0-100 scale)
             decimal clarity = Math.Min(100, 50 + (decimal)(dataPoints.Count * 0.5)); // More points = clearer
             decimal noise = Math.Max(0, 100m - (stdDev * 10m)); // Lower std dev = less noise
             decimal contrast = Math.Min(100, average > 0 ? (stdDev / average) * 100m : 0); // Std dev / average ratio
             decimal overallQuality = (clarity + noise + contrast) / 3;

             string rating = overallQuality >= 80 ? "Excellent" :
                            overallQuality >= 60 ? "Good" :
                            overallQuality >= 40 ? "Fair" : "Poor";

             var qualityIssues = new List<string>();
             var improvements = new List<string>();

             if (noise > 30)
                 qualityIssues.Add("High noise level detected");
             if (contrast < 20)
             {
                 qualityIssues.Add("Low contrast - temperature range too narrow");
                 improvements.Add("Expand temperature range in imaging");
             }
             if (dataPoints.Count < 50)
             {
                 improvements.Add("Increase data point resolution");
             }

             var result = new ThermalImageQuality
             {
                 QualityId = _defaults.FormatIdForTable("QUALITY", Guid.NewGuid().ToString()),
                 ImageId = imageId,
                 AssessmentDate = DateTime.UtcNow,
                 Clarity = clarity,
                 NoiseLevel = noise,
                 Contrast = contrast,
                 OverallQualityScore = overallQuality,
                 QualityRating = rating,
                 QualityIssues = qualityIssues,
                 RecommendedImprovements = improvements
             };

             _logger?.LogInformation("Thermal image quality assessment complete: Rating={Rating}, Score={Score}",
                 rating, overallQuality);

             return await Task.FromResult(result);
         }

         /// <summary>
         /// Compares thermal data between baseline and current measurements
         /// </summary>
         public async Task<ThermalComparisonResult> CompareThermalDataAsync(
             string locationId,
             List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> baselineData,
             List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> currentData)
         {
             if (string.IsNullOrWhiteSpace(locationId))
                 throw new ArgumentException("Location ID cannot be null or empty", nameof(locationId));
             if (baselineData == null || baselineData.Count == 0)
                 throw new ArgumentException("Baseline data cannot be null or empty", nameof(baselineData));
             if (currentData == null || currentData.Count == 0)
                 throw new ArgumentException("Current data cannot be null or empty", nameof(currentData));

             _logger?.LogInformation("Comparing thermal data for location {LocationId}",
                 locationId);

             // Calculate statistics for both datasets
             var baselineTemps = baselineData.Select(p => (decimal)p.Value).ToList();
             var currentTemps = currentData.Select(p => (decimal)p.Value).ToList();

             decimal baselineAvg = baselineTemps.Average();
             decimal currentAvg = currentTemps.Average();
             decimal tempChange = currentAvg - baselineAvg;
             decimal percentChange = baselineAvg > 0m ? (tempChange / baselineAvg) * 100m : 0m;

             // Calculate standard deviations
             decimal baselineStdDev = (decimal)Math.Sqrt((double)(baselineTemps.Sum(t => (t - baselineAvg) * (t - baselineAvg)) / baselineTemps.Count));
             decimal currentStdDev = (decimal)Math.Sqrt((double)(currentTemps.Sum(t => (t - currentAvg) * (t - currentAvg)) / currentTemps.Count));

             // Determine if change is significant (> 10% or > 2°C)
             bool isSignificant = Math.Abs(percentChange) > 10m || Math.Abs(tempChange) > 2m;

             var result = new ThermalComparisonResult
             {
                 ComparisonId = _defaults.FormatIdForTable("COMPARISON", Guid.NewGuid().ToString()),
                 LocationId = locationId,
                 ComparisonDate = DateTime.UtcNow,
                 BaselineDate = DateTime.UtcNow.AddMonths(-1),
                 CurrentDate = DateTime.UtcNow,
                 BaselineAverageTemperature = baselineAvg,
                 CurrentAverageTemperature = currentAvg,
                 TemperatureChange = tempChange,
                 PercentChange = percentChange,
                 BaselineStdDev = baselineStdDev,
                 CurrentStdDev = currentStdDev,
                 SignificantChange = isSignificant ? "Yes" : "No",
                 ChangePatterns = IdentifyChangePatterns(baselineAvg, currentAvg, baselineStdDev, currentStdDev)
             };

             _logger?.LogInformation("Thermal comparison complete: Change={Change}°C ({Percent}%), Significant={Significant}",
                 tempChange, percentChange, isSignificant);

             return await Task.FromResult(result);
         }

         #region Helper Methods

         private string DetermineThermalPattern(decimal average, decimal stdDev, List<decimal> temperatures)
         {
             decimal coefficient = stdDev / average;
             if (coefficient < 0.1m)
                 return "Uniform";
             if (temperatures.Max() > average * 1.3m)
                 return "Hot Spot";
             if (temperatures.Min() < average * 0.7m)
                 return "Cold Spot";
             return "Mixed";
         }

         private List<string> GenerateAnomalyRecommendations(string anomalyType, string severity)
         {
             var recommendations = new List<string>();

             if (anomalyType == "Hot Anomaly")
             {
                 recommendations.Add("Investigate heat source at anomaly location");
                 recommendations.Add("Check for thermal bridging or insulation failure");
                 if (severity == "Critical")
                     recommendations.Add("Immediate inspection required");
             }
             else if (anomalyType == "Cold Anomaly")
             {
                 recommendations.Add("Verify cooling system or ventilation");
                 recommendations.Add("Check for moisture accumulation");
             }

             return recommendations;
         }

         private string DetermineGradientPattern(List<GradientPoint> gradientPoints)
         {
             if (gradientPoints.Count < 2)
                 return "Insufficient Data";

             var gradients = gradientPoints.Select(gp => Math.Abs(gp.LocalGradient)).ToList();
             decimal avgGradient = gradients.Average();
             decimal maxDeviation = gradients.Max(g => Math.Abs(g - avgGradient));

             if (maxDeviation < avgGradient * 0.2m)
                 return "Linear";
             if (gradients.Last() > gradients.First())
                 return "Exponential";
             return "Nonlinear";
         }

         private List<TemperatureZone> CreateTemperatureZones(string locationId, List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> zonePoints, 
             string classification, decimal minTemp, decimal maxTemp)
         {
             var zones = new List<TemperatureZone>();

             if (zonePoints.Count == 0)
                 return zones;

             zones.Add(new TemperatureZone
             {
                 ZoneId = _defaults.FormatIdForTable("ZONE", Guid.NewGuid().ToString()),
                 LocationId = locationId,
                 IdentificationDate = DateTime.UtcNow,
                 MinTemperature = minTemp,
                 MaxTemperature = maxTemp,
                 AverageTemperature = (decimal)zonePoints.Average(p => p.Value),
                 ZoneClassification = classification,
                 Area = zonePoints.Count,
                 PointCount = zonePoints.Count,
                 BoundaryCoordinates = ExtractBoundaryCoordinates(zonePoints)
             });

             return zones;
         }

         private List<decimal> ExtractBoundaryCoordinates(List<Beep.OilandGas.Models.Data.HeatMap.HeatMapDataPoint> points)
         {
             var coords = new List<decimal>();
             if (points.Count > 0)
             {
                 coords.Add((decimal)points.Min(p => p.X));
                 coords.Add((decimal)points.Min(p => p.Y));
                 coords.Add((decimal)points.Max(p => p.X));
                 coords.Add((decimal)points.Max(p => p.Y));
             }
             return coords;
         }

         private List<string> IdentifyChangePatterns(decimal baselineAvg, decimal currentAvg, decimal baselineStdDev, decimal currentStdDev)
         {
             var patterns = new List<string>();

             if (currentAvg > baselineAvg)
                 patterns.Add("Overall temperature increase");
             if (currentAvg < baselineAvg)
                 patterns.Add("Overall temperature decrease");

             if (currentStdDev > baselineStdDev * 1.2m)
                 patterns.Add("Increased temperature variability");
             if (currentStdDev < baselineStdDev * 0.8m)
                 patterns.Add("Decreased temperature variability");

             return patterns;
         }

         #endregion
     }
}
