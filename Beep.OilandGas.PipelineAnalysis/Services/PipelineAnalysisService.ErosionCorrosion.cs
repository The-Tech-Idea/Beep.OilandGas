using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PipelineAnalysis.Services
{
    /// <summary>
    /// Partial class: Erosion & Corrosion Analysis Methods (5 methods)
    /// </summary>
    public partial class PipelineAnalysisService
    {
        public async Task<ErosionAnalysisDto> AnalyzeErosionAsync(string pipelineId, ErosionRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Analyzing erosion for {PipelineId}", pipelineId);

            try
            {
                var erosionVelocity = CalculateErosionCriticalVelocity(request.PipeMaterial, request.FlowRate);
                var pipelineArea = (decimal)Math.PI * (request.PipelineDiameter / 2) * (request.PipelineDiameter / 2) / 144m;
                var actualVelocity = (request.FlowRate * 0.00584m) / pipelineArea;
                var safetyMargin = erosionVelocity > 0 ? (erosionVelocity - actualVelocity) / erosionVelocity * 100m : 0;
                var erosionRisk = DetermineErosionRisk(actualVelocity, erosionVelocity);

                var result = new ErosionAnalysisDto
                {
                    PipelineId = pipelineId,
                    ErosionVelocity = erosionVelocity,
                    ActualVelocity = actualVelocity,
                    ErosionRisk = erosionRisk,
                    SafetyMargin = safetyMargin,
                    HighRiskSpots = new List<ErosionSpotDto>
                    {
                        actualVelocity > erosionVelocity * 0.9m ? new ErosionSpotDto 
                        { 
                            LocationDescription = "High flow velocity zones",
                            ErosionRating = (actualVelocity / erosionVelocity) * 100m,
                            MitigationStrategy = "Reduce flow rate or install erosion-resistant coating"
                        } : null
                    },
                    Recommendations = GenerateErosionRecommendations(erosionRisk, safetyMargin)
                };

                result.HighRiskSpots.RemoveAll(x => x == null);
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing erosion for {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<ErosionVelocityResultDto> CalculateErosionVelocityAsync(string pipelineId, ErosionVelocityRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Calculating erosion velocity for {PipelineId}", pipelineId);

            try
            {
                var erosionVelocity = CalculateErosionCriticalVelocity(request.PipeMaterial, 100m);
                var pipelineArea = (decimal)Math.PI * (request.PipeDiameter / 2) * (request.PipeDiameter / 2) / 144m;
                var actualVelocity = request.FluidType == "Gas" ? 15m : 8m; // Assumed flow rate

                var result = new ErosionVelocityResultDto
                {
                    ErosionVelocity = erosionVelocity,
                    ActualVelocity = actualVelocity,
                    IsSafe = actualVelocity < erosionVelocity,
                    VelocitySafetyFactor = erosionVelocity / Math.Max(actualVelocity, 0.1m)
                };

                _logger?.LogInformation("Erosion velocity calculated: {ErosionVelocity:F2} ft/s, Safety Factor: {SafetyFactor:F2}",
                    erosionVelocity, result.VelocitySafetyFactor);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating erosion velocity for {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<CorrosionAnalysisDto> AnalyzeCorrosionRiskAsync(string pipelineId, CorrosionRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Analyzing corrosion risk for {PipelineId}", pipelineId);

            try
            {
                var co2Factor = (request.CO2Concentration / 100m) * 30m;
                var h2sFactor = (request.H2SConcentration / 100m) * 40m;
                var temperatureFactor = request.Temperature > 200 ? (request.Temperature - 200m) * 0.2m : 0;
                var waterFactor = (request.WaterContent / 100m) * 25m;
                var pHFactor = request.pH < 6 ? (6 - request.pH) * 5m : 0;

                var corrosionRisk = Math.Min(co2Factor + h2sFactor + temperatureFactor + waterFactor + pHFactor, 100m);
                var riskLevel = DetermineCorrosionRiskLevel(corrosionRisk);

                var result = new CorrosionAnalysisDto
                {
                    PipelineId = pipelineId,
                    CorrosionRisk = corrosionRisk,
                    RiskLevel = riskLevel,
                    CorrosionFactors = new List<CorrosionFactorDto>
                    {
                        new() { FactorName = "CO2", ContributionPercent = co2Factor, Severity = co2Factor > 20 ? "High" : "Low" },
                        new() { FactorName = "H2S", ContributionPercent = h2sFactor, Severity = h2sFactor > 25 ? "High" : "Low" },
                        new() { FactorName = "Temperature", ContributionPercent = temperatureFactor, Severity = temperatureFactor > 10 ? "Medium" : "Low" },
                        new() { FactorName = "Water Content", ContributionPercent = waterFactor, Severity = waterFactor > 15 ? "High" : "Low" }
                    },
                    MitigationStrategies = GenerateCorrosionMitigationStrategies(riskLevel),
                    RecommendedMaterial = RecommendCorrosionResistantMaterial(request.PipeMaterial, riskLevel)
                };

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing corrosion risk for {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<CorrosionRateResultDto> EstimateCorrosionRateAsync(string pipelineId, CorrosionRateRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Estimating corrosion rate for {PipelineId}", pipelineId);

            try
            {
                decimal corrosionRate = 0.5m; // mm/year default
                if (request.HistoricalData.Count >= 2)
                {
                    var thicknessDiff = request.HistoricalData[0].WallThickness - request.HistoricalData[^1].WallThickness;
                    var dateDiff = (request.HistoricalData[0].MeasurementDate - request.HistoricalData[^1].MeasurementDate).TotalDays / 365.25;
                    corrosionRate = (decimal)dateDiff > 0 ? thicknessDiff / (decimal)dateDiff : 0.5m;
                }

                var remainingLife = request.CurrentWallThickness / Math.Max(corrosionRate, 0.001m);
                var inspectionDue = DateTime.UtcNow.AddYears((int)(remainingLife / 2));

                var result = new CorrosionRateResultDto
                {
                    CorrosionRate = Math.Abs(corrosionRate),
                    Unit = "mm/year",
                    CorrosionType = DetermineCorrosionType(request.PipeMaterial),
                    RemainingLifeYears = remainingLife,
                    InspectionDueDate = inspectionDue
                };

                _logger?.LogInformation("Corrosion rate: {Rate:F3} mm/year, Remaining life: {Life:F1} years",
                    corrosionRate, remainingLife);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error estimating corrosion rate for {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<MaterialCompatibilityDto> AssessMaterialCompatibilityAsync(string pipelineId, MaterialRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Assessing material compatibility for {PipelineId} with {Material}",
                pipelineId, request.ProposedMaterial);

            try
            {
                var isCompatible = EvaluateMaterialCompatibility(request.ProposedMaterial, request.FluidType, 
                    request.DesignPressure, request.DesignTemperature);
                var compatibilityScore = CalculateMaterialCompatibilityScore(request.ProposedMaterial, 
                    request.DesignPressure, request.DesignTemperature);
                var concerns = IdentifyMaterialConcerns(request.ProposedMaterial, request.FluidType);
                var alternatives = RecommendAlternativeMaterials(request.ProposedMaterial, request.DesignPressure);

                var result = new MaterialCompatibilityDto
                {
                    PipelineId = pipelineId,
                    CurrentMaterial = request.ProposedMaterial,
                    IsCompatible = isCompatible,
                    CompatibilityScore = compatibilityScore,
                    Concerns = concerns,
                    AlternativeMaterials = alternatives
                };

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assessing material compatibility for {PipelineId}", pipelineId);
                throw;
            }
        }

        #region Helper Methods

        private decimal CalculateErosionCriticalVelocity(string pipeMaterial, decimal flowRate)
        {
            return pipeMaterial?.ToLower() switch
            {
                "carbon steel" => 12m,
                "stainless steel" => 15m,
                "chromium steel" => 18m,
                "duplex" => 16m,
                _ => 10m
            };
        }

        private string DetermineErosionRisk(decimal actualVelocity, decimal erosionVelocity)
        {
            if (actualVelocity > erosionVelocity) return "Critical";
            if (actualVelocity > erosionVelocity * 0.9m) return "High";
            if (actualVelocity > erosionVelocity * 0.75m) return "Medium";
            return "Low";
        }

        private List<string> GenerateErosionRecommendations(string erosionRisk, decimal safetyMargin)
        {
            var recommendations = new List<string>();
            if (erosionRisk == "Critical") recommendations.Add("Immediately reduce flow rate or increase pipe diameter");
            if (erosionRisk == "High") recommendations.Add("Monitor erosion and consider system modifications");
            if (safetyMargin < 20) recommendations.Add("Install erosion-resistant coating");
            if (erosionRisk != "Low") recommendations.Add("Implement quarterly erosion velocity monitoring");
            return recommendations;
        }

        private string DetermineCorrosionRiskLevel(decimal corrosionRisk)
        {
            if (corrosionRisk >= 75) return "Critical";
            if (corrosionRisk >= 50) return "High";
            if (corrosionRisk >= 25) return "Medium";
            return "Low";
        }

        private List<string> GenerateCorrosionMitigationStrategies(string riskLevel)
        {
            return riskLevel switch
            {
                "Critical" => new() { "Install impressed current cathodic protection", "Use high-alloy piping", "Implement pH control", "Install corrosion inhibitor treatment" },
                "High" => new() { "Apply epoxy coating", "Implement corrosion monitoring", "Use inhibitor treatment", "Schedule regular inspections" },
                "Medium" => new() { "Paint exterior", "Implement biennial inspection", "Monitor water content" },
                _ => new() { "Maintain regular inspection schedule" }
            };
        }

        private string RecommendCorrosionResistantMaterial(string currentMaterial, string riskLevel)
        {
            return riskLevel switch
            {
                "Critical" => "Super Duplex Stainless Steel",
                "High" => "Duplex Stainless Steel",
                "Medium" => "Chromium Steel",
                _ => currentMaterial ?? "Carbon Steel"
            };
        }

        private string DetermineCorrosionType(string pipeMaterial)
        {
            return pipeMaterial?.ToLower() switch
            {
                "carbon steel" => "Uniform Corrosion",
                "stainless steel" => "Pitting Corrosion",
                "duplex" => "Stress Corrosion Cracking",
                _ => "General Corrosion"
            };
        }

        private bool EvaluateMaterialCompatibility(string material, string fluidType, decimal pressure, decimal temperature)
        {
            if (material?.ToLower() == "carbon steel" && fluidType == "H2S") return false;
            if (pressure > 2000 && material?.ToLower() == "standard steel") return false;
            if (temperature > 300 && material?.ToLower() == "carbon steel") return false;
            return true;
        }

        private string CalculateMaterialCompatibilityScore(string material, decimal pressure, decimal temperature)
        {
            var score = 85m;
            if (pressure > 1500) score -= 15;
            if (temperature > 250) score -= 10;
            return score >= 80 ? "Excellent" : score >= 60 ? "Good" : score >= 40 ? "Fair" : "Poor";
        }

        private List<string> IdentifyMaterialConcerns(string material, string fluidType)
        {
            var concerns = new List<string>();
            if (material?.ToLower() == "carbon steel") concerns.Add("Susceptible to general corrosion");
            if (fluidType == "CO2" && material?.ToLower() != "duplex") concerns.Add("CO2 corrosion risk");
            if (fluidType == "H2S") concerns.Add("Sulfide stress corrosion cracking risk");
            return concerns;
        }

        private List<string> RecommendAlternativeMaterials(string currentMaterial, decimal designPressure)
        {
            var alternatives = new List<string>();
            if (currentMaterial?.ToLower() != "duplex") alternatives.Add("Duplex Stainless Steel");
            if (designPressure > 1500) alternatives.Add("Chromium Molybdenum Steel");
            if (currentMaterial?.ToLower() != "super duplex") alternatives.Add("Super Duplex");
            return alternatives;
        }

        #endregion
    }
}
