using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    public partial class ProspectIdentificationService
    {
        public async Task<ProspectRiskAnalysisResult> PerformRiskAssessmentAsync(
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

            decimal overallRisk = trapRisk * sealRisk * sourceRisk * migrationRisk * characterizationRisk;
            decimal probabilityOfSuccess = 1m - overallRisk;

            string riskLevel = probabilityOfSuccess > 0.5m ? "Low" :
                              probabilityOfSuccess > 0.3m ? "Medium" :
                              probabilityOfSuccess > 0.1m ? "High" : "Critical";

            var assessment = new ProspectRiskAnalysisResult
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

        public async Task<EconomicViabilityAnalysis> AnalyzeEconomicViabilityAsync(
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

            decimal discountRate = 0.1m;
            int projectLife = 20;

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

            var analysis = new EconomicViabilityAnalysis
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

        private List<RiskCategory> CreateRiskCategories(Dictionary<string, decimal> riskScores)
        {
            var categories = new List<RiskCategory>();

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

                categories.Add(new RiskCategory
                {
                    CategoryName = riskMapping.ContainsKey(score.Key) ? riskMapping[score.Key] : score.Key,
                    RiskScore = score.Value,
                    RiskLevel = riskLevel,
                    MitigationStrategies = GenerateMitigationStrategies(score.Key)
                });
            }

            return categories;
        }

        private List<string> GenerateMitigationStrategies(string riskCategory)
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
    }
}
