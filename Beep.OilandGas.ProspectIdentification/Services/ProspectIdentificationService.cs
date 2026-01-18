using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    /// <summary>
    /// Service for prospect identification operations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class ProspectIdentificationService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<ProspectIdentificationService>? _logger;

        public ProspectIdentificationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<ProspectIdentificationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public async Task<ProspectEvaluationDto> EvaluateProspectAsync(string prospectId)
        {
            if (string.IsNullOrWhiteSpace(prospectId))
                throw new ArgumentException("Prospect ID cannot be null or empty", nameof(prospectId));

             _logger?.LogInformation("Evaluating prospect {ProspectId}", prospectId);

             // TODO: Implement prospect evaluation logic
             var evaluation = new ProspectEvaluationDto
             {
                 EvaluationId = _defaults.FormatIdForTable("EVAL", Guid.NewGuid().ToString()),
                 ProspectId = prospectId,
                 EvaluationDate = DateTime.UtcNow,
                 Recommendation = "Further evaluation recommended"
             };

             _logger?.LogWarning("EvaluateProspectAsync not fully implemented - requires evaluation logic");

             await Task.CompletedTask;
             return evaluation;
        }

        public async Task<List<ProspectDto>> GetProspectsAsync(Dictionary<string, string>? filters = null)
        {
            _logger?.LogInformation("Getting prospects with filters: {FilterCount}", filters?.Count ?? 0);

            // Create repository for PROSPECT
            var prospectRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PROSPECT), _connectionName, "PROSPECT", null);

            var appFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (filters != null && filters.Count > 0)
            {
                foreach (var filter in filters)
                {
                    appFilters.Add(new AppFilter { FieldName = filter.Key, Operator = "=", FilterValue = filter.Value });
                }
            }

             var entities = await prospectRepo.GetAsync(appFilters);
             var prospects = entities.Cast<PROSPECT>().Select(entity => new ProspectDto
             {
                 ProspectId = entity.PROSPECT_ID ?? string.Empty,
                 ProspectName = entity.PROSPECT_NAME ?? string.Empty,
                 FieldId = entity.FIELD_ID ?? string.Empty,
                 Status = entity.STATUS
             }).ToList();

            _logger?.LogInformation("Retrieved {Count} prospects", prospects.Count);
            return prospects;
        }

        public async Task<string> CreateProspectAsync(ProspectDto prospect, string userId)
        {
            if (prospect == null)
                throw new ArgumentNullException(nameof(prospect));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Creating prospect {ProspectName}", prospect.ProspectName);

            if (string.IsNullOrWhiteSpace(prospect.ProspectId))
            {
                prospect.ProspectId = _defaults.FormatIdForTable("PROSPECT", Guid.NewGuid().ToString());
            }

            // Create repository for PROSPECT
            var prospectRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PROSPECT), _connectionName, "PROSPECT", null);

            var newEntity = new PROSPECT
            {
                PROSPECT_ID = prospect.ProspectId,
                PROSPECT_NAME = prospect.ProspectName ?? string.Empty,
                FIELD_ID = prospect.FieldId,
                EVALUATION_DATE = prospect.EvaluationDate,
                ESTIMATED_RESERVES = prospect.EstimatedResources,
                RISK_FACTOR = prospect.RiskScore,
                STATUS = prospect.Status ?? "New",
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await prospectRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully created prospect {ProspectId}", prospect.ProspectId);
            return prospect.ProspectId;
        }

        public async Task<List<ProspectRankingDto>> RankProspectsAsync(List<string> prospectIds, Dictionary<string, decimal> rankingCriteria)
        {
            if (prospectIds == null || prospectIds.Count == 0)
                throw new ArgumentException("Prospect IDs cannot be null or empty", nameof(prospectIds));
            if (rankingCriteria == null || rankingCriteria.Count == 0)
                throw new ArgumentException("Ranking criteria cannot be null or empty", nameof(rankingCriteria));

            _logger?.LogInformation("Ranking {Count} prospects using {CriteriaCount} criteria",
                prospectIds.Count, rankingCriteria.Count);

            // TODO: Implement prospect ranking logic
            var rankings = new List<ProspectRankingDto>();
            for (int i = 0; i < prospectIds.Count; i++)
            {
                rankings.Add(new ProspectRankingDto
                {
                    ProspectId = prospectIds[i],
                    ProspectName = $"Prospect {prospectIds[i]}",
                    Rank = i + 1,
                    Score = 100 - (i * 10) // Simplified scoring
                });
            }

             _logger?.LogWarning("RankProspectsAsync not fully implemented - requires ranking logic");

             await Task.CompletedTask;
             return rankings.OrderByDescending(r => r.Score).ToList();
         }

         /// <summary>
         /// Analyzes seismic interpretation data for prospect definition
         /// </summary>
         public async Task<SeismicInterpretationAnalysisDto> AnalyzeSeismicInterpretationAsync(
             string prospectId,
             string surveyId,
             List<HorizonDto> horizons,
             List<FaultDto> faults)
         {
             if (string.IsNullOrWhiteSpace(prospectId))
                 throw new ArgumentException("Prospect ID cannot be null or empty", nameof(prospectId));
             if (string.IsNullOrWhiteSpace(surveyId))
                 throw new ArgumentException("Survey ID cannot be null or empty", nameof(surveyId));

             _logger?.LogInformation("Analyzing seismic interpretation for prospect {ProspectId}: Horizons={Horizons}, Faults={Faults}",
                 prospectId, horizons?.Count ?? 0, faults?.Count ?? 0);

             var analysis = new SeismicInterpretationAnalysisDto
             {
                 AnalysisId = _defaults.FormatIdForTable("SEISMIC_INT", Guid.NewGuid().ToString()),
                 ProspectId = prospectId,
                 AnalysisDate = DateTime.UtcNow,
                 SurveyId = surveyId,
                 HorizonCount = horizons?.Count ?? 0,
                 FaultCount = faults?.Count ?? 0,
                 Horizons = horizons ?? new List<HorizonDto>(),
                 Faults = faults ?? new List<FaultDto>(),
                 InterpretationConfidence = 0.75m, // Default 75% confidence
                 InterpretationStatus = "Completed"
             };

             _logger?.LogInformation("Seismic interpretation analysis complete for prospect {ProspectId}",
                 prospectId);

             return await Task.FromResult(analysis);
         }

         /// <summary>
         /// Estimates hydrocarbon resources using volumetric method
         /// </summary>
         public async Task<ResourceEstimationResultDto> EstimateResourcesAsync(
             string prospectId,
             decimal grossRockVolume,
             decimal netToGrossRatio,
             decimal porosity,
             decimal waterSaturation,
             string estimatedBy)
         {
             if (string.IsNullOrWhiteSpace(prospectId))
                 throw new ArgumentException("Prospect ID cannot be null or empty", nameof(prospectId));
             if (string.IsNullOrWhiteSpace(estimatedBy))
                 throw new ArgumentException("Estimated by cannot be null or empty", nameof(estimatedBy));

             _logger?.LogInformation("Estimating resources for prospect {ProspectId}: GRV={GRV}, NGR={NGR}",
                 prospectId, grossRockVolume, netToGrossRatio);

             decimal netRockVolume = grossRockVolume * netToGrossRatio;
             decimal oilRecoveryFactor = 0.15m; // 15% typical
             decimal gasRecoveryFactor = 0.80m; // 80% typical
             
             decimal estimatedOilVolume = netRockVolume * porosity * (1 - waterSaturation) * oilRecoveryFactor;
             decimal estimatedGasVolume = netRockVolume * porosity * (1 - waterSaturation) * gasRecoveryFactor;

             var result = new ResourceEstimationResultDto
             {
                 EstimationId = _defaults.FormatIdForTable("RESOURCE_EST", Guid.NewGuid().ToString()),
                 ProspectId = prospectId,
                 EstimationDate = DateTime.UtcNow,
                 EstimatedBy = estimatedBy,
                 GrossRockVolume = grossRockVolume,
                 NetRockVolume = netRockVolume,
                 Porosity = porosity,
                 WaterSaturation = waterSaturation,
                 OilRecoveryFactor = oilRecoveryFactor,
                 GasRecoveryFactor = gasRecoveryFactor,
                 EstimatedOilVolume = estimatedOilVolume,
                 EstimatedGasVolume = estimatedGasVolume,
                 VolumeUnit = "MMbbl",
                 EstimationMethod = "Volumetric"
             };

             _logger?.LogInformation("Resource estimation complete: Oil={Oil}, Gas={Gas}",
                 estimatedOilVolume, estimatedGasVolume);

             return await Task.FromResult(result);
         }

         /// <summary>
         /// Analyzes trap geometry and closure geometry
         /// </summary>
         public async Task<TrapGeometryAnalysisDto> AnalyzeTrapGeometryAsync(
             string prospectId,
             string trapType,
             decimal crestDepth,
             decimal spillPointDepth,
             decimal area,
             decimal volume)
         {
             if (string.IsNullOrWhiteSpace(prospectId))
                 throw new ArgumentException("Prospect ID cannot be null or empty", nameof(prospectId));

             _logger?.LogInformation("Analyzing trap geometry for prospect {ProspectId}: Type={Type}, Closure={Closure}ft",
                 prospectId, trapType, spillPointDepth - crestDepth);

             decimal closure = spillPointDepth - crestDepth;

             var analysis = new TrapGeometryAnalysisDto
             {
                 AnalysisId = _defaults.FormatIdForTable("TRAP_GEOM", Guid.NewGuid().ToString()),
                 ProspectId = prospectId,
                 AnalysisDate = DateTime.UtcNow,
                 TrapType = trapType,
                 Closure = closure,
                 CrestDepth = crestDepth,
                 SpillPointDepth = spillPointDepth,
                 Area = area,
                 AreaUnit = "km²",
                 Volume = volume,
                 VolumeUnit = "km³",
                 TrapGeometry = closure > 100 ? "Good" : "Fair",
                 SourceRockProximity = closure < 1000 ? "Favorable" : "Marginal"
             };

             _logger?.LogInformation("Trap geometry analysis complete: Closure={Closure}ft, Geometry={Geometry}",
                 closure, analysis.TrapGeometry);

             return await Task.FromResult(analysis);
         }

         /// <summary>
         /// Analyzes hydrocarbon migration pathways
         /// </summary>
         public async Task<MigrationPathAnalysisDto> AnalyzeMigrationPathAsync(
             string prospectId,
             string sourceRockId,
             decimal maturityLevel,
             decimal distance)
         {
             if (string.IsNullOrWhiteSpace(prospectId))
                 throw new ArgumentException("Prospect ID cannot be null or empty", nameof(prospectId));

             _logger?.LogInformation("Analyzing migration path for prospect {ProspectId} from source {SourceRockId}",
                 prospectId, sourceRockId);

             string migrationEfficiency = maturityLevel > 0.6m && distance < 50 ? "Excellent" :
                                         maturityLevel > 0.5m && distance < 100 ? "Good" : "Fair";

             var analysis = new MigrationPathAnalysisDto
             {
                 AnalysisId = _defaults.FormatIdForTable("MIGRATION", Guid.NewGuid().ToString()),
                 ProspectId = prospectId,
                 AnalysisDate = DateTime.UtcNow,
                 SourceRockId = sourceRockId,
                 SourceRockMaturityLevel = maturityLevel,
                 MigrationPathway = "Primary vertical migration with lateral component",
                 MigrationDistance = distance,
                 DistanceUnit = "km",
                 MigrationEfficiency = migrationEfficiency switch
                 {
                     "Excellent" => 0.9m,
                     "Good" => 0.7m,
                     _ => 0.5m
                 },
                 SealIntegrity = "Good",
                 LateralMigrationRisk = distance > 50 ? "Moderate" : "Low"
             };

             _logger?.LogInformation("Migration analysis complete: Efficiency={Efficiency}, Risk={Risk}",
                 analysis.MigrationEfficiency, analysis.LateralMigrationRisk);

             return await Task.FromResult(analysis);
         }

         /// <summary>
         /// Assesses seal and source rock characteristics
         /// </summary>
         public async Task<SealSourceAssessmentDto> AssessSealAndSourceAsync(
             string prospectId,
             string sealRockType,
             decimal sealThickness,
             string sourceRockType,
             decimal sourceMaturity)
         {
             if (string.IsNullOrWhiteSpace(prospectId))
                 throw new ArgumentException("Prospect ID cannot be null or empty", nameof(prospectId));

             _logger?.LogInformation("Assessing seal and source for prospect {ProspectId}: SealType={SealType}, SourceType={SourceType}",
                 prospectId, sealRockType, sourceRockType);

             string sealQuality = sealThickness > 50 ? "Excellent" : sealThickness > 20 ? "Good" : "Fair";
             string sourceStatus = sourceMaturity > 0.6m ? "Active" : sourceMaturity > 0.5m ? "Marginal" : "Inactive";

             var assessment = new SealSourceAssessmentDto
             {
                 AssessmentId = _defaults.FormatIdForTable("SEAL_SOURCE", Guid.NewGuid().ToString()),
                 ProspectId = prospectId,
                 AssessmentDate = DateTime.UtcNow,
                 SealRockType = sealRockType,
                 SealRockThickness = sealThickness,
                 SealQuality = sealQuality,
                 SealIntegrityScore = sealQuality switch
                 {
                     "Excellent" => 0.9m,
                     "Good" => 0.7m,
                     _ => 0.5m
                 },
                 SourceRockType = sourceRockType,
                 SourceRockMaturity = sourceMaturity,
                 GenerationStatus = sourceStatus,
                 SourceRockProductivity = sourceMaturity > 0.5m ? 0.7m : 0.3m,
                 SystemStatus = sourceStatus
             };

             _logger?.LogInformation("Seal and source assessment complete: SealQuality={Quality}, SourceStatus={Status}",
                 sealQuality, sourceStatus);

             return await Task.FromResult(assessment);
         }

         /// <summary>
         /// Performs comprehensive risk assessment for prospect
         /// </summary>
         public async Task<ProspectRiskAnalysisResultDto> PerformRiskAssessmentAsync(
             string prospectId,
             string assessedBy,
             Dictionary<string, decimal> riskScores)
         {
             if (string.IsNullOrWhiteSpace(prospectId))
                 throw new ArgumentException("Prospect ID cannot be null or empty", nameof(prospectId));
             if (string.IsNullOrWhiteSpace(assessedBy))
                 throw new ArgumentException("Assessed by cannot be null or empty", nameof(assessedBy));

             _logger?.LogInformation("Performing risk assessment for prospect {ProspectId}",
                 prospectId);

             decimal trapRisk = riskScores.ContainsKey("Trap") ? riskScores["Trap"] : 0.3m;
             decimal sealRisk = riskScores.ContainsKey("Seal") ? riskScores["Seal"] : 0.2m;
             decimal sourceRisk = riskScores.ContainsKey("Source") ? riskScores["Source"] : 0.2m;
             decimal migrationRisk = riskScores.ContainsKey("Migration") ? riskScores["Migration"] : 0.15m;
             decimal characterizationRisk = riskScores.ContainsKey("Characterization") ? riskScores["Characterization"] : 0.15m;

             // Calculate overall risk as product of individual risks (multiplicative model)
             decimal overallRisk = trapRisk * sealRisk * sourceRisk * migrationRisk * characterizationRisk;
             decimal probabilityOfSuccess = 1m - overallRisk;

             string riskLevel = probabilityOfSuccess > 0.5m ? "Low" :
                               probabilityOfSuccess > 0.3m ? "Medium" :
                               probabilityOfSuccess > 0.1m ? "High" : "Critical";

             var assessment = new ProspectRiskAnalysisResultDto
             {
                 AnalysisId = _defaults.FormatIdForTable("RISK_ASSESS", Guid.NewGuid().ToString()),
                 ProspectId = prospectId,
                 AnalysisDate = DateTime.UtcNow,
                 AssessedBy = assessedBy,
                 TrapRisk = trapRisk,
                 SealRisk = sealRisk,
                 SourceRisk = sourceRisk,
                 MigrationRisk = migrationRisk,
                 CharacterizationRisk = characterizationRisk,
                 OverallRisk = overallRisk,
                 ProbabilityOfSuccess = probabilityOfSuccess,
                 OverallRiskLevel = riskLevel,
                 RiskCategories = CreateRiskCategories(riskScores)
             };

             _logger?.LogInformation("Risk assessment complete: POS={POS}, RiskLevel={Risk}",
                 probabilityOfSuccess, riskLevel);

             return await Task.FromResult(assessment);
         }

         /// <summary>
         /// Analyzes economic viability of prospect development
         /// </summary>
         public async Task<EconomicViabilityAnalysisDto> AnalyzeEconomicViabilityAsync(
             string prospectId,
             decimal estimatedOil,
             decimal estimatedGas,
             decimal capitalCost,
             decimal operatingCost,
             decimal oilPrice,
             decimal gasPrice)
         {
             if (string.IsNullOrWhiteSpace(prospectId))
                 throw new ArgumentException("Prospect ID cannot be null or empty", nameof(prospectId));

             _logger?.LogInformation("Analyzing economic viability for prospect {ProspectId}: Oil={Oil}MMbbl, Gas={Gas}Bcf",
                 prospectId, estimatedOil, estimatedGas);

             decimal discountRate = 0.1m; // 10% default
             int projectLife = 20; // 20 years
             
             // Simple NPV calculation (annual cashflow)
             decimal annualCashFlow = (estimatedOil / projectLife * oilPrice) + (estimatedGas / projectLife * gasPrice) - (operatingCost / projectLife);
             decimal npv = 0;
             for (int year = 1; year <= projectLife; year++)
             {
                 npv += annualCashFlow / (decimal)Math.Pow((double)(1 + discountRate), year);
             }
             npv -= capitalCost;

             decimal irr = CalculateIRR(annualCashFlow, capitalCost, projectLife);
             decimal paybackPeriod = capitalCost > 0 ? capitalCost / annualCashFlow : 0;
             decimal profitabilityIndex = npv / capitalCost;

             var analysis = new EconomicViabilityAnalysisDto
             {
                 AnalysisId = _defaults.FormatIdForTable("ECON_VIA", Guid.NewGuid().ToString()),
                 ProspectId = prospectId,
                 AnalysisDate = DateTime.UtcNow,
                 EstimatedCapitalCost = capitalCost,
                 EstimatedOperatingCost = operatingCost,
                 OilPrice = oilPrice,
                 GasPrice = gasPrice,
                 DiscountRate = discountRate,
                 ProjectLifeYears = projectLife,
                 NetPresentValue = npv,
                 InternalRateOfReturn = irr,
                 PaybackPeriodYears = paybackPeriod,
                 ProfitabilityIndex = profitabilityIndex,
                 ViabilityStatus = npv > 0 ? "Viable" : npv > capitalCost * -0.1m ? "Marginal" : "Non-Viable"
             };

             _logger?.LogInformation("Economic analysis complete: NPV={NPV}, IRR={IRR}%, Status={Status}",
                 npv, irr * 100, analysis.ViabilityStatus);

             return await Task.FromResult(analysis);
         }

         /// <summary>
         /// Optimizes prospect portfolio based on risk and opportunity
         /// </summary>
         public async Task<PortfolioOptimizationResultDto> OptimizePortfolioAsync(
             List<ProspectRankingDto> rankedProspects,
             decimal riskTolerance,
             decimal capitalBudget)
         {
             if (rankedProspects == null || rankedProspects.Count == 0)
                 throw new ArgumentException("Ranked prospects cannot be null or empty", nameof(rankedProspects));

             _logger?.LogInformation("Optimizing portfolio with {Count} prospects, Risk Tolerance={Risk}",
                 rankedProspects.Count, riskTolerance);

             var recommended = new List<string>();
             var marginal = new List<string>();
             var rejected = new List<string>();

             decimal budgetRemaining = capitalBudget;
             decimal portfolioRisk = 0;
             decimal expectedValue = 0;

             foreach (var prospect in rankedProspects.OrderByDescending(p => p.Score))
             {
                 decimal prospectValue = prospect.Score * 1000; // Simplified
                 decimal prospectRisk = 1m - (prospect.Score / 100m);

                 if (budgetRemaining > prospectValue && portfolioRisk + prospectRisk <= riskTolerance)
                 {
                     recommended.Add(prospect.ProspectId);
                     budgetRemaining -= prospectValue;
                     portfolioRisk += prospectRisk;
                     expectedValue += prospectValue;
                 }
                 else if (prospect.Score > 50)
                 {
                     marginal.Add(prospect.ProspectId);
                 }
                 else
                 {
                     rejected.Add(prospect.ProspectId);
                 }
             }

             var result = new PortfolioOptimizationResultDto
             {
                 OptimizationId = _defaults.FormatIdForTable("PORTFOLIO_OPT", Guid.NewGuid().ToString()),
                 OptimizationDate = DateTime.UtcNow,
                 RecommendedProspects = recommended,
                 MarginallProspects = marginal,
                 RejectedProspects = rejected,
                 TotalPortfolioRisk = portfolioRisk,
                 TotalExpectedValue = expectedValue,
                 RiskAdjustedReturn = expectedValue / (portfolioRisk > 0 ? portfolioRisk : 1),
                 OptimizationStrategy = "Risk-adjusted return maximization"
             };

             _logger?.LogInformation("Portfolio optimization complete: Recommended={Rec}, Risk={Risk}",
                 recommended.Count, portfolioRisk);

             return await Task.FromResult(result);
         }

         #region Helper Methods

         private List<RiskCategoryDto> CreateRiskCategories(Dictionary<string, decimal> riskScores)
         {
             var categories = new List<RiskCategoryDto>();

             var riskMapping = new Dictionary<string, string>
             {
                 { "Trap", "Structural/Stratigraphic" },
                 { "Seal", "Seal Integrity" },
                 { "Source", "Hydrocarbon Source" },
                 { "Migration", "Migration Pathway" },
                 { "Characterization", "Subsurface Characterization" }
             };

             foreach (var score in riskScores)
             {
                 string riskLevel = score.Value > 0.7m ? "High" :
                                   score.Value > 0.4m ? "Medium" : "Low";

                 categories.Add(new RiskCategoryDto
                 {
                     CategoryName = riskMapping.ContainsKey(score.Key) ? riskMapping[score.Key] : score.Key,
                     RiskScore = score.Value,
                     RiskLevel = riskLevel,
                     MitigationStrategies = GenerateMitigationStrategies(score.Key, score.Value)
                 });
             }

             return categories;
         }

         private List<string> GenerateMitigationStrategies(string riskCategory, decimal riskScore)
         {
             var strategies = new List<string>();

             switch (riskCategory)
             {
                 case "Trap":
                     strategies.Add("Conduct additional seismic interpretation");
                     strategies.Add("Perform well correlation analysis");
                     break;
                 case "Seal":
                     strategies.Add("Analyze core samples for seal integrity");
                     strategies.Add("Model pressure regimes");
                     break;
                 case "Source":
                     strategies.Add("Conduct source rock analysis");
                     strategies.Add("Perform thermal maturity analysis");
                     break;
                 case "Migration":
                     strategies.Add("Construct migration pathway models");
                     strategies.Add("Analyze pressure conditions");
                     break;
             }

             return strategies;
         }

         private decimal CalculateIRR(decimal annualCashFlow, decimal capitalCost, int projectLife)
         {
             // Simplified IRR calculation
             decimal guess = 0.15m;
             for (int i = 0; i < 10; i++)
             {
                 decimal npv = 0;
                 for (int year = 1; year <= projectLife; year++)
                 {
                     npv += annualCashFlow / (decimal)Math.Pow((double)(1 + guess), year);
                 }
                 npv -= capitalCost;

                 if (Math.Abs(npv) < 0.01m)
                     break;

                 decimal derivative = 0;
                 for (int year = 1; year <= projectLife; year++)
                 {
                     derivative -= (decimal)year * annualCashFlow / (decimal)Math.Pow((double)(1 + guess), year + 1);
                 }

                 if (derivative != 0)
                     guess = guess - npv / derivative;
             }

             return guess;
         }

         #endregion
     }
}
