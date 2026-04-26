using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using ProspectRecord = Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    /// <summary>
    /// Service for evaluating geological prospects using PPDM repositories.
    /// </summary>
    public class ProspectEvaluationService : IProspectEvaluationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler? _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository? _defaults;
        private readonly IPPDMMetadataRepository? _metadata;
        private readonly string _connectionName;
        private readonly ILogger<ProspectEvaluationService>? _logger;

        public ProspectEvaluationService(IDMEEditor editor, string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _connectionName = connectionName;
        }

        public ProspectEvaluationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<ProspectEvaluationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
            _logger = logger;
        }

        public async Task<ProspectEvaluation> EvaluateProspectAsync(string prospectId, ProspectEvaluationRequest request)
        {
            if (string.IsNullOrWhiteSpace(prospectId))
                throw new ArgumentException("Prospect ID cannot be null or empty.", nameof(prospectId));

            var prospectRepo = CreateProspectRepository();
            var prospect = await prospectRepo.GetByIdAsync(prospectId) as ProspectRecord;
            if (prospect == null)
                throw new KeyNotFoundException($"Prospect with ID {prospectId} not found.");

            var seismicCount = await GetSeismicSurveyCountAsync(prospect.PROSPECT_ID);
            var riskScore = CalculateRiskScore(prospect, seismicCount);
            var probability = CalculateProbabilityOfSuccess(prospect, seismicCount);

            return new ProspectEvaluation
            {
                EvaluationId = Guid.NewGuid().ToString("N"),
                ProspectId = prospect.PROSPECT_ID ?? prospectId,
                EvaluationDate = DateTime.UtcNow,
                EvaluatedBy = "System",
                EstimatedOilResources = prospect.ESTIMATED_OIL_VOLUME,
                EstimatedGasResources = prospect.ESTIMATED_GAS_VOLUME,
                ResourceUnit = prospect.ESTIMATED_VOLUME_OUOM,
                ProbabilityOfSuccess = probability,
                RiskScore = riskScore,
                RiskLevel = DetermineRiskLevel(riskScore),
                Recommendation = GenerateRecommendation(prospect, seismicCount),
                Remarks = prospect.DESCRIPTION ?? prospect.REMARK,
                FieldId = prospect.PRIMARY_FIELD_ID ?? prospect.FIELD_ID ?? string.Empty,
                Potential = probability >= 0.5m ? "Commercial" : "Speculative"
            };
        }

        public async Task<List<Prospect>> GetProspectsAsync(string? fieldId = null, string? basinId = null, ProspectStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var prospectRepo = CreateProspectRepository();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "PRIMARY_FIELD_ID", FilterValue = fieldId, Operator = "=" });

            if (!string.IsNullOrWhiteSpace(basinId))
                filters.Add(new AppFilter { FieldName = "PLAY_ID", FilterValue = basinId, Operator = "=" });

            if (status.HasValue)
            {
                var statusFilter = MapProspectStatus(status.Value);
                if (!string.IsNullOrWhiteSpace(statusFilter))
                    filters.Add(new AppFilter { FieldName = "PROSPECT_STATUS", FilterValue = statusFilter, Operator = "=" });
            }

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "EVALUATION_DATE", FilterValue = startDate.Value.ToString("o"), Operator = ">=" });

            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "EVALUATION_DATE", FilterValue = endDate.Value.ToString("o"), Operator = "<=" });

            var prospects = (await prospectRepo.GetAsync(filters)).OfType<ProspectRecord>().ToList();
            var result = new List<Prospect>(prospects.Count);

            foreach (var prospect in prospects)
            {
                var seismicCount = await GetSeismicSurveyCountAsync(prospect.PROSPECT_ID);
                result.Add(MapToProspectDto(prospect, seismicCount));
            }

            return result;
        }

        public async Task<Prospect?> GetProspectAsync(string prospectId)
        {
            if (string.IsNullOrWhiteSpace(prospectId))
                return null;

            var prospectRepo = CreateProspectRepository();
            var prospect = await prospectRepo.GetByIdAsync(prospectId) as ProspectRecord;
            if (prospect == null)
                return null;

            var seismicCount = await GetSeismicSurveyCountAsync(prospect.PROSPECT_ID);
            return MapToProspectDto(prospect, seismicCount);
        }

        public async Task<Prospect> CreateProspectAsync(CreateProspect createDto, string userId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var prospectId = _defaults!.FormatIdForTable("PROSPECT", Guid.NewGuid().ToString("N"));
            var prospect = new ProspectRecord
            {
                PROSPECT_ID = prospectId,
                PROSPECT_NAME = createDto.ProspectName,
                PROSPECT_SHORT_NAME = createDto.ProspectName,
                PRIMARY_FIELD_ID = createDto.FieldId,
                FIELD_ID = createDto.FieldId,
                PROSPECT_STATUS = "NEW",
                DESCRIPTION = createDto.Description,
                LOCATION_DESCRIPTION = createDto.Location,
                LATITUDE = createDto.Latitude,
                LONGITUDE = createDto.Longitude,
                ACTIVE_IND = "Y"
            };

            var prospectRepo = CreateProspectRepository();
            await prospectRepo.InsertAsync(prospect, userId);
            return MapToProspectDto(prospect, 0);
        }

        public async Task<Prospect> UpdateProspectAsync(string prospectId, UpdateProspect updateDto, string userId)
        {
            if (string.IsNullOrWhiteSpace(prospectId))
                throw new ArgumentException("Prospect ID cannot be null or empty.", nameof(prospectId));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var prospectRepo = CreateProspectRepository();
            var prospect = await prospectRepo.GetByIdAsync(prospectId) as ProspectRecord;
            if (prospect == null)
                throw new KeyNotFoundException($"Prospect with ID {prospectId} not found.");

            if (!string.IsNullOrWhiteSpace(updateDto.ProspectName))
            {
                prospect.PROSPECT_NAME = updateDto.ProspectName;
                prospect.PROSPECT_SHORT_NAME = updateDto.ProspectName;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Description))
                prospect.DESCRIPTION = updateDto.Description;

            if (!string.IsNullOrWhiteSpace(updateDto.Status))
                prospect.PROSPECT_STATUS = updateDto.Status;

            if (updateDto.EstimatedResources.HasValue)
                prospect.ESTIMATED_OIL_VOLUME = updateDto.EstimatedResources;

            if (!string.IsNullOrWhiteSpace(updateDto.ResourceUnit))
                prospect.ESTIMATED_VOLUME_OUOM = updateDto.ResourceUnit;

            await prospectRepo.UpdateAsync(prospect, userId);

            var seismicCount = await GetSeismicSurveyCountAsync(prospect.PROSPECT_ID);
            return MapToProspectDto(prospect, seismicCount);
        }

        public async Task<Prospect> ChangeProspectStatusAsync(string prospectId, ProspectStatus newStatus, string userId)
        {
            if (string.IsNullOrWhiteSpace(prospectId))
                throw new ArgumentException("Prospect ID cannot be null or empty.", nameof(prospectId));

            var prospectRepo = CreateProspectRepository();
            var prospect = await prospectRepo.GetByIdAsync(prospectId) as ProspectRecord;
            if (prospect == null)
                throw new KeyNotFoundException($"Prospect with ID {prospectId} not found.");

            prospect.PROSPECT_STATUS = MapProspectStatus(newStatus) ?? "EVALUATED";
            prospect.ACTIVE_IND = newStatus == ProspectStatus.Rejected ? "N" : "Y";

            await prospectRepo.UpdateAsync(prospect, userId);

            var seismicCount = await GetSeismicSurveyCountAsync(prospect.PROSPECT_ID);
            return MapToProspectDto(prospect, seismicCount);
        }

        public async Task DeleteProspectAsync(string prospectId, string userId)
        {
            if (string.IsNullOrWhiteSpace(prospectId))
                throw new ArgumentException("Prospect ID cannot be null or empty.", nameof(prospectId));

            var prospectRepo = CreateProspectRepository();
            var prospect = await prospectRepo.GetByIdAsync(prospectId) as ProspectRecord;
            if (prospect == null)
                throw new KeyNotFoundException($"Prospect with ID {prospectId} not found.");

            prospect.ACTIVE_IND = "N";
            await prospectRepo.UpdateAsync(prospect, userId);
        }

        public async Task<VolumetricAnalysis> PerformVolumetricAnalysisAsync(string prospectId, VolumetricAnalysisRequest request)
        {
            var evaluation = await EvaluateProspectAsync(prospectId, new ProspectEvaluationRequest());
            var gross = (evaluation.EstimatedOilResources ?? 0m) + (evaluation.EstimatedGasResources ?? 0m);

            return new VolumetricAnalysis
            {
                ProspectId = prospectId,
                AnalysisDate = DateTime.UtcNow,
                EstimatedOilResources = evaluation.EstimatedOilResources ?? 0m,
                EstimatedGasResources = evaluation.EstimatedGasResources ?? 0m,
                ResourceUnit = evaluation.ResourceUnit ?? "STB"
            };
        }

        public async Task<RiskAssessment> PerformRiskAssessmentAsync(string prospectId, ProspectRiskAssessmentRequest request)
        {
            var evaluation = await EvaluateProspectAsync(prospectId, new ProspectEvaluationRequest());

            return new RiskAssessment
            {
                ProspectId = prospectId,
                AssessmentDate = DateTime.UtcNow,
                AssessedBy = "System",
                RiskScore = evaluation.RiskScore ?? 0m,
                RiskLevel = evaluation.RiskLevel,
            };
        }

        public async Task<EconomicEvaluation> PerformEconomicEvaluationAsync(string prospectId, EconomicEvaluationRequest request)
        {
            var evaluation = await EvaluateProspectAsync(prospectId, new ProspectEvaluationRequest());
            var reserves = evaluation.EstimatedOilResources ?? evaluation.EstimatedGasResources ?? 0m;
            var probability = evaluation.ProbabilityOfSuccess ?? 0m;

            return new EconomicEvaluation
            {
                ProspectId = prospectId,
                EvaluationDate = DateTime.UtcNow,
                NPV = reserves * 3.5m * probability,
                IRR = 0.12m + (probability * 0.15m),
                PaybackYears = Math.Max(1m, 6m - (probability * 4m)),
            };
        }

        public async Task<List<ProspectRanking>> RankProspectsAsync(ProspectRankingRequest request)
        {
            var targetIds = request?.ProspectIds ?? new List<string>();
            var prospects = await GetProspectsAsync();
            var filtered = targetIds.Count == 0
                ? prospects
                : prospects.Where(p => targetIds.Contains(p.ProspectId, StringComparer.OrdinalIgnoreCase)).ToList();

            return filtered
                .Select(p => new ProspectRanking
                {
                    ProspectId = p.ProspectId,
                    ProspectName = p.ProspectName,
                    Score = (1m - (p.RiskScore ?? 0m)) * 0.6m + ((p.EstimatedResources ?? 0m) > 0m ? 0.4m : 0m),
                    WeightedScore = (1m - (p.RiskScore ?? 0m)) * 0.7m + ((p.EstimatedResources ?? 0m) > 0m ? 0.3m : 0m)
                })
                .OrderByDescending(r => r.WeightedScore)
                .Select((r, idx) =>
                {
                    r.Rank = idx + 1;
                    return r;
                })
                .ToList();
        }

        public async Task<ProspectComparison> CompareProspectsAsync(List<string> prospectIds, ProspectComparisonRequest request)
        {
            if (prospectIds == null || prospectIds.Count == 0)
                return new ProspectComparison { ProspectId = string.Empty, Score = 0m, CriteriaScores = new Dictionary<string, decimal>() };

            var prospects = await GetProspectsAsync();
            var selected = prospects.Where(p => prospectIds.Contains(p.ProspectId, StringComparer.OrdinalIgnoreCase)).ToList();
            if (selected.Count == 0)
                return new ProspectComparison { ProspectId = prospectIds[0], Score = 0m, CriteriaScores = new Dictionary<string, decimal>() };

            var best = selected
                .OrderByDescending(p => (1m - (p.RiskScore ?? 0m)) + ((p.EstimatedResources ?? 0m) / 1000m))
                .First();

            var criteriaScores = new Dictionary<string, decimal>
            {
                ["Risk"] = 1m - (best.RiskScore ?? 0m),
                ["Resource"] = Math.Min(1m, (best.EstimatedResources ?? 0m) / 1000m),
                ["Status"] = string.Equals(best.Status, "APPROVED", StringComparison.OrdinalIgnoreCase) ? 1m : 0.5m
            };

            return new ProspectComparison
            {
                ProspectId = best.ProspectId,
                Score = criteriaScores.Values.Average(),
                CriteriaScores = criteriaScores
            };
        }

        public Task<SensitivityAnalysis> PerformSensitivityAnalysisAsync(string prospectId, SensitivityAnalysisRequest request)
            => Task.FromResult(new SensitivityAnalysis { ProspectId = prospectId, AnalysisDate = DateTime.UtcNow });

        public async Task<ResourceEstimate> EstimateResourcesAsync(string prospectId, ResourceEstimateRequest request)
        {
            var prospect = await GetProspectAsync(prospectId);
            return new ResourceEstimate
            {
                ProspectId = prospectId,
                OilEstimate = prospect?.EstimatedResources ?? 0m,
                GasEstimate = (prospect?.EstimatedResources ?? 0m) * 0.6m
            };
        }

        public async Task<ProbabilisticAssessment> PerformProbabilisticAssessmentAsync(string prospectId, ProbabilisticAssessmentRequest request)
        {
            var estimate = await EstimateResourcesAsync(prospectId, new ResourceEstimateRequest());
            return new ProbabilisticAssessment
            {
                ProspectId = prospectId,
                P10 = estimate.OilEstimate * 1.3m,
                P50 = estimate.OilEstimate,
                P90 = estimate.OilEstimate * 0.7m
            };
        }

        public Task<ResourceEstimate> UpdateResourceEstimatesAsync(string prospectId, string userId)
            => EstimateResourcesAsync(prospectId, new ResourceEstimateRequest());

        public Task<PlayAnalysis> AnalyzePlayAsync(string playId, PlayAnalysisRequest request)
            => Task.FromResult(new PlayAnalysis { PlayId = playId });

        public async Task<PlayStatistics> GetPlayStatisticsAsync(string playId)
        {
            var prospects = await GetProspectsAsync(basinId: playId);
            return new PlayStatistics
            {
                Metric = "ProspectCount",
                Value = prospects.Count
            };
        }

        public async Task<List<AnalogProspect>> FindAnalogProspectsAsync(string prospectId, AnalogSearchRequest request)
        {
            var source = await GetProspectAsync(prospectId);
            if (source == null)
                return new List<AnalogProspect>();

            var candidates = await GetProspectsAsync(fieldId: source.FieldId);
            return candidates
                .Where(p => !string.Equals(p.ProspectId, source.ProspectId, StringComparison.OrdinalIgnoreCase))
                .Select(p =>
                {
                    var resourceDelta = Math.Abs((p.EstimatedResources ?? 0m) - (source.EstimatedResources ?? 0m));
                    var similarity = Math.Max(0m, 1m - (resourceDelta / 1000m));
                    return new AnalogProspect { ProspectId = p.ProspectId, SimilarityScore = similarity };
                })
                .Where(a => a.SimilarityScore >= (request?.SimilarityThreshold ?? 0.7m))
                .OrderByDescending(a => a.SimilarityScore)
                .ToList();
        }

        public Task<ProspectReport> GenerateProspectReportAsync(string prospectId, ProspectReportRequest request)
            => Task.FromResult(new ProspectReport { ProspectId = prospectId, Url = $"/api/prospects/{prospectId}/reports/{(request?.ReportType ?? "Comprehensive").ToLowerInvariant()}" });

        public async Task<byte[]> ExportProspectDataAsync(string prospectId, string format = "PDF")
        {
            var prospect = await GetProspectAsync(prospectId);
            if (prospect == null)
                return Array.Empty<byte>();

            var payload = $"ProspectId={prospect.ProspectId};Name={prospect.ProspectName};Status={prospect.Status};Risk={prospect.RiskScore:F2};Format={format}";
            return Encoding.UTF8.GetBytes(payload);
        }

        public Task<PortfolioReport> GeneratePortfolioReportAsync(PortfolioReportRequest request)
            => Task.FromResult(new PortfolioReport { PortfolioName = request?.PortfolioName ?? "Portfolio", Url = $"/api/prospects/portfolio/{(request?.PortfolioName ?? "portfolio").ToLowerInvariant()}" });

        public async Task<ProspectValidation> ValidateProspectDataAsync(string prospectId)
        {
            var prospect = await GetProspectAsync(prospectId);
            var errors = new List<string>();

            if (prospect == null)
                errors.Add("Prospect not found.");
            else
            {
                if (string.IsNullOrWhiteSpace(prospect.ProspectName)) errors.Add("Prospect name is required.");
                if (string.IsNullOrWhiteSpace(prospect.FieldId)) errors.Add("Field ID is required.");
                if ((prospect.EstimatedResources ?? 0m) < 0m) errors.Add("Estimated resources cannot be negative.");
            }

            return new ProspectValidation
            {
                ProspectId = prospectId,
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        public Task<PeerReview> PerformPeerReviewAsync(string prospectId, PeerReviewRequest request)
            => Task.FromResult(new PeerReview
            {
                ProspectId = prospectId,
                Reviewer = request?.Reviewer ?? "PeerReviewer",
                Summary = $"{(request?.ReviewType ?? "Technical")} peer review completed."
            });

        private void EnsureRepositoryDependencies()
        {
            if (_commonColumnHandler == null || _defaults == null || _metadata == null)
            {
                throw new InvalidOperationException("ProspectEvaluationService requires PPDM repository dependencies. Use the DI constructor with ICommonColumnHandler, IPPDM39DefaultsRepository, and IPPDMMetadataRepository.");
            }
        }

        private PPDMGenericRepository CreateProspectRepository()
        {
            EnsureRepositoryDependencies();
            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler!,
                _defaults!,
                _metadata!,
                typeof(ProspectRecord),
                _connectionName,
                "PROSPECT",
                null);
        }

        private PPDMGenericRepository CreateSeismicSurveyRepository()
        {
            EnsureRepositoryDependencies();
            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler!,
                _defaults!,
                _metadata!,
                typeof(SEIS_ACQTN_SURVEY),
                _connectionName,
                "SEIS_ACQTN_SURVEY",
                null);
        }

        private async Task<int> GetSeismicSurveyCountAsync(string? prospectId)
        {
            if (string.IsNullOrWhiteSpace(prospectId))
                return 0;

            var surveyRepo = CreateSeismicSurveyRepository();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AREA_ID", FilterValue = _defaults!.FormatIdForTable("SEIS_ACQTN_SURVEY", prospectId), Operator = "=" },
                new AppFilter { FieldName = "AREA_TYPE", FilterValue = "PROSPECT", Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            var surveys = await surveyRepo.GetAsync(filters);
            return surveys.OfType<SEIS_ACQTN_SURVEY>().Count();
        }

        private static Prospect MapToProspectDto(ProspectRecord prospect, int seismicSurveyCount)
        {
            return new Prospect
            {
                ProspectId = prospect.PROSPECT_ID ?? string.Empty,
                FieldId = prospect.PRIMARY_FIELD_ID ?? prospect.FIELD_ID ?? string.Empty,
                ProspectName = prospect.PROSPECT_NAME ?? prospect.PROSPECT_SHORT_NAME ?? string.Empty,
                Description = prospect.DESCRIPTION ?? prospect.REMARK,
                Location = prospect.LOCATION_DESCRIPTION,
                Latitude = prospect.LATITUDE,
                Longitude = prospect.LONGITUDE,
                Status = prospect.PROSPECT_STATUS ?? (string.Equals(prospect.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase) ? "Active" : "Inactive"),
                CreatedDate = prospect.ROW_CREATED_DATE,
                EvaluationDate = prospect.EVALUATION_DATE,
                EstimatedResources = prospect.ESTIMATED_OIL_VOLUME ?? prospect.ESTIMATED_RESERVES,
                ResourceUnit = prospect.ESTIMATED_VOLUME_OUOM,
                RiskLevel = prospect.RISK_LEVEL,
                RiskScore = CalculateRiskScore(prospect, seismicSurveyCount)
            };
        }

        private static decimal CalculateProbabilityOfSuccess(ProspectRecord prospect, int seismicSurveyCount)
        {
            decimal probability = 0.35m;
            if (seismicSurveyCount >= 1) probability += 0.1m;
            if (seismicSurveyCount >= 3) probability += 0.1m;
            if (string.Equals(prospect.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase)) probability += 0.05m;
            if (!string.IsNullOrWhiteSpace(prospect.RISK_LEVEL) && prospect.RISK_LEVEL.Equals("LOW", StringComparison.OrdinalIgnoreCase)) probability += 0.1m;
            return Math.Min(0.95m, Math.Max(0.05m, probability));
        }

        private static decimal CalculateRiskScore(ProspectRecord prospect, int seismicSurveyCount)
        {
            decimal riskScore = 0.55m;
            if (seismicSurveyCount > 0) riskScore -= 0.1m;
            if (!string.IsNullOrWhiteSpace(prospect.RISK_LEVEL) && prospect.RISK_LEVEL.Equals("HIGH", StringComparison.OrdinalIgnoreCase))
                riskScore += 0.15m;
            if (prospect.RISK_FACTOR.HasValue)
                riskScore = (riskScore + prospect.RISK_FACTOR.Value) / 2m;
            return Math.Max(0m, Math.Min(1m, riskScore));
        }

        private static string DetermineRiskLevel(decimal riskScore)
        {
            if (riskScore < 0.3m) return "Low";
            if (riskScore < 0.6m) return "Medium";
            return "High";
        }

        private static string? GenerateRecommendation(ProspectRecord prospect, int seismicSurveyCount)
        {
            var riskScore = CalculateRiskScore(prospect, seismicSurveyCount);
            var probability = CalculateProbabilityOfSuccess(prospect, seismicSurveyCount);

            if (probability > 0.7m && riskScore < 0.4m)
                return "Proceed with development";
            if (probability > 0.5m && riskScore < 0.6m)
                return "Proceed with caution - additional evaluation recommended";
            return "High risk - detailed evaluation required before proceeding";
        }

        private static string? MapProspectStatus(ProspectStatus status)
        {
            return status switch
            {
                ProspectStatus.Identified => "IDENTIFIED",
                ProspectStatus.Evaluated => "EVALUATED",
                ProspectStatus.Approved => "APPROVED",
                ProspectStatus.Rejected => "REJECTED",
                _ => null
            };
        }
    }
}

