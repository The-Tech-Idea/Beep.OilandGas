using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.DataBase;
using ProspectRecord = Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    public partial class ProspectIdentificationService
    {
        public async Task<ProspectEvaluation> EvaluateProspectAsync(string prospectId)
        {
            if (string.IsNullOrWhiteSpace(prospectId))
                throw new ArgumentException("Prospect ID cannot be null or empty", nameof(prospectId));

            _logger?.LogInformation("Evaluating prospect {ProspectId}", prospectId);

            var prospectRepo = CreateProspectRepository();

            var entity = await prospectRepo.GetByIdAsync(prospectId);
            var prospect = entity as ProspectRecord;

            var estimatedResources = prospect?.ESTIMATED_OIL_VOLUME ?? prospect?.ESTIMATED_RESERVES;
            var riskScore = prospect?.RISK_FACTOR ?? 0.5m;
            var probabilityOfSuccess = prospect?.RISK_FACTOR != null
                ? Math.Max(0.05m, 1m - prospect.RISK_FACTOR.Value)
                : 0.5m;

            var evaluation = new ProspectEvaluation
            {
                EvaluationId = _defaults.FormatIdForTable("EVAL", Guid.NewGuid().ToString()),
                ProspectId = prospectId,
                EvaluationDate = DateTime.UtcNow,
                EstimatedOilResources = estimatedResources,
                RiskScore = riskScore,
                ProbabilityOfSuccess = probabilityOfSuccess,
                Recommendation = ResolveRecommendation(prospect, riskScore)
            };

            _logger?.LogInformation("Prospect evaluation completed for {ProspectId}", prospectId);

            return evaluation;
        }

        public async Task<List<Prospect>> GetProspectsAsync(Dictionary<string, string>? filters = null)
        {
            _logger?.LogInformation("Getting prospects with filters: {FilterCount}", filters?.Count ?? 0);

            var prospectRepo = CreateProspectRepository();

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
            var prospects = entities
                .OfType<ProspectRecord>()
                .Select(entity => new Prospect
                {
                    ProspectId = entity.PROSPECT_ID ?? string.Empty,
                    ProspectName = entity.PROSPECT_NAME ?? string.Empty,
                    Description = string.IsNullOrWhiteSpace(entity.DESCRIPTION) ? entity.REMARK : entity.DESCRIPTION,
                    FieldId = ResolveFieldId(entity),
                    Status = ResolveStatus(entity),
                    RiskLevel = entity.RISK_LEVEL,
                    EstimatedResources = entity.ESTIMATED_OIL_VOLUME ?? entity.ESTIMATED_RESERVES,
                    CreatedDate = entity.ROW_CREATED_DATE,
                    EvaluationDate = entity.EVALUATION_DATE
                })
                .ToList();

            _logger?.LogInformation("Retrieved {Count} prospects", prospects.Count);
            return prospects;
        }

        public async Task<string> CreateProspectAsync(Prospect prospect, string userId)
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

            var prospectRepo = CreateProspectRepository();

            var newEntity = new ProspectRecord
            {
                PROSPECT_ID = prospect.ProspectId,
                PROSPECT_NAME = prospect.ProspectName ?? string.Empty,
                PRIMARY_FIELD_ID = prospect.FieldId,
                FIELD_ID = prospect.FieldId,
                DESCRIPTION = prospect.Description ?? string.Empty,
                REMARK = prospect.Description ?? string.Empty,
                EVALUATION_DATE = prospect.EvaluationDate,
                ESTIMATED_OIL_VOLUME = prospect.EstimatedResources,
                ESTIMATED_RESERVES = prospect.EstimatedResources,
                RISK_FACTOR = prospect.RiskScore,
                RISK_LEVEL = prospect.RiskLevel,
                PROSPECT_STATUS = prospect.Status ?? "NEW",
                STATUS = prospect.Status ?? "New",
                ACTIVE_IND = "Y"
            };

            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await prospectRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully created prospect {ProspectId}", prospect.ProspectId);
            return prospect.ProspectId;
        }

        public async Task<List<ProspectRanking>> RankProspectsAsync(List<string> prospectIds, Dictionary<string, decimal> rankingCriteria)
        {
            if (prospectIds == null || prospectIds.Count == 0)
                throw new ArgumentException("Prospect IDs cannot be null or empty", nameof(prospectIds));
            if (rankingCriteria == null || rankingCriteria.Count == 0)
                throw new ArgumentException("Ranking criteria cannot be null or empty", nameof(rankingCriteria));

            _logger?.LogInformation("Ranking {Count} prospects using {CriteriaCount} criteria",
                prospectIds.Count, rankingCriteria.Count);

            var prospectRepo = CreateProspectRepository();

            var rankings = new List<ProspectRanking>();
            foreach (var id in prospectIds)
            {
                var entity = await prospectRepo.GetByIdAsync(id);
                var p = entity as ProspectRecord;

                decimal score = 0m;
                var estimatedReserves = p?.ESTIMATED_OIL_VOLUME ?? p?.ESTIMATED_RESERVES;
                if (rankingCriteria.TryGetValue("EstimatedReserves", out var resWeight) && estimatedReserves != null)
                    score += resWeight * (estimatedReserves.Value / 1_000_000m);
                if (rankingCriteria.TryGetValue("RiskFactor", out var riskWeight) && p?.RISK_FACTOR != null)
                    score += riskWeight * (1m - p.RISK_FACTOR.Value);

                rankings.Add(new ProspectRanking
                {
                    ProspectId = id,
                    ProspectName = p?.PROSPECT_NAME ?? id,
                    Score = score
                });
            }

            var ordered = rankings.OrderByDescending(r => r.Score).ToList();
            for (int i = 0; i < ordered.Count; i++) ordered[i].Rank = i + 1;

            return ordered;
        }

        public async Task<PortfolioOptimizationResult> OptimizePortfolioAsync(
            List<ProspectRanking> rankedProspects,
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
                decimal prospectValue = prospect.Score * 1000;
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

            var result = new PortfolioOptimizationResult
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
    }
}
