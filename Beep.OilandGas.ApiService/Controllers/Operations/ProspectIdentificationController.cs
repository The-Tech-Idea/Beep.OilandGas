using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.Models.Data.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Beep.OilandGas.ApiService.Controllers.Operations
{
    /// <summary>
    /// API controller for prospect identification operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProspectIdentificationController : ControllerBase
    {
        private readonly IProspectIdentificationService _service;
        private readonly ILogger<ProspectIdentificationController> _logger;

        public ProspectIdentificationController(IProspectIdentificationService service, ILogger<ProspectIdentificationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("/api/prospect/identify")]
        public async Task<ActionResult<PROSPECT>> IdentifyProspectCompatibility([FromBody] PROSPECT request)
        {
            if (request == null) return BadRequest(new { error = "Request body is required." });

            try
            {
                var prospect = MapLegacyProspect(request);
                var prospectId = await _service.CreateProspectAsync(prospect, GetUserId());
                request.PROSPECT_ID = prospectId;
                request.PROSPECT_STATUS = string.IsNullOrWhiteSpace(request.PROSPECT_STATUS) ? "New" : request.PROSPECT_STATUS;
                return Ok(request);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid legacy prospect identify request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error identifying prospect through legacy compatibility route");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("/api/prospect/risk")]
        public async Task<ActionResult<PROSPECT_RISK_ASSESSMENT>> EvaluateRiskCompatibility([FromBody] PROSPECT request)
        {
            if (request == null) return BadRequest(new { error = "Request body is required." });

            try
            {
                ProspectEvaluation evaluation;
                if (!string.IsNullOrWhiteSpace(request.PROSPECT_ID))
                {
                    evaluation = await _service.EvaluateProspectAsync(request.PROSPECT_ID);
                }
                else
                {
                    evaluation = BuildEvaluationFromLegacyProspect(request);
                }

                return Ok(MapRiskAssessment(evaluation, request));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid legacy prospect risk request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating prospect risk through legacy compatibility route");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("/api/prospect/{prospectId}")]
        public async Task<ActionResult<PROSPECT>> GetProspectCompatibility(string prospectId)
        {
            if (string.IsNullOrWhiteSpace(prospectId)) return BadRequest(new { error = "Prospect ID is required." });

            try
            {
                var prospect = await GetProspectByIdAsync(prospectId);
                if (prospect == null)
                    return NotFound(new { error = $"Prospect {prospectId} was not found." });

                return Ok(MapLegacyProspect(prospect));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospect {ProspectId} through legacy compatibility route", prospectId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("/api/prospect/{prospectId}/volumetrics")]
        public async Task<ActionResult<PROSPECT_VOLUME_ESTIMATE>> GetVolumetricsCompatibility(string prospectId)
        {
            if (string.IsNullOrWhiteSpace(prospectId)) return BadRequest(new { error = "Prospect ID is required." });

            try
            {
                var prospect = await GetProspectByIdAsync(prospectId);
                if (prospect == null)
                    return NotFound(new { error = $"Prospect {prospectId} was not found." });

                return Ok(MapVolumeEstimate(prospect));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting volumetrics for prospect {ProspectId}", prospectId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("/api/prospect/rank")]
        public async Task<ActionResult<PROSPECT_RANKING>> RankProspectsCompatibility([FromBody] PROSPECT_PORTFOLIO request)
        {
            if (request == null) return BadRequest(new { error = "Request body is required." });

            try
            {
                var prospects = await GetPortfolioProspectsAsync(request.PORTFOLIO_ID);
                if (prospects.Count == 0)
                    return NotFound(new { error = "No prospects were available to rank." });

                var rankingCriteria = new Dictionary<string, decimal>
                {
                    ["EstimatedReserves"] = 0.7m,
                    ["RiskFactor"] = 0.3m
                };

                var rankings = await _service.RankProspectsAsync(
                    prospects.Select(p => p.ProspectId).Where(id => !string.IsNullOrWhiteSpace(id)).ToList(),
                    rankingCriteria);

                var topRanked = rankings.OrderBy(r => r.Rank).FirstOrDefault();
                if (topRanked == null)
                    return NotFound(new { error = "No ranked prospects were returned." });

                return Ok(MapLegacyRanking(topRanked, request));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid legacy prospect rank request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ranking prospects through legacy compatibility route");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("/api/prospect/portfolio/{basinId}")]
        public async Task<ActionResult<List<PROSPECT>>> GetProspectPortfolioCompatibility(string basinId)
        {
            if (string.IsNullOrWhiteSpace(basinId)) return BadRequest(new { error = "Basin ID is required." });

            try
            {
                var prospects = await GetPortfolioProspectsAsync(basinId);
                return Ok(prospects.Select(MapLegacyProspect).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospect portfolio for basin {BasinId}", basinId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("evaluate/{prospectId}")]
        public async Task<ActionResult<ProspectEvaluation>> EvaluateProspect(string prospectId)
        {
            if (string.IsNullOrWhiteSpace(prospectId)) return BadRequest(new { error = "Prospect ID is required." });
            try
            {
                var result = await _service.EvaluateProspectAsync(prospectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating prospect {ProspectId}", prospectId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Prospect>>> GetProspects([FromQuery] Dictionary<string, string>? filters = null)
        {
            try
            {
                var result = await _service.GetProspectsAsync(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospects");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> CreateProspect([FromBody] Prospect prospect, [FromQuery] string? userId = null)
        {
            try
            {
                var prospectId = await _service.CreateProspectAsync(prospect, userId ?? GetUserId());
                return Ok(new { message = "Prospect created successfully", prospectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prospect");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("rank")]
        public async Task<ActionResult<List<ProspectRanking>>> RankProspects([FromBody] RankProspectsRequest request)
        {
            try
            {
                var result = await _service.RankProspectsAsync(request.ProspectIds, request.RankingCriteria);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ranking prospects");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private async Task<Prospect?> GetProspectByIdAsync(string prospectId)
        {
            var filtered = await _service.GetProspectsAsync(new Dictionary<string, string>
            {
                ["PROSPECT_ID"] = prospectId
            });

            return filtered.FirstOrDefault();
        }

        private async Task<List<Prospect>> GetPortfolioProspectsAsync(string basinId)
        {
            var prospects = await _service.GetProspectsAsync(new Dictionary<string, string>
            {
                ["FIELD_ID"] = basinId
            });

            if (prospects.Count > 0)
                return prospects;

            prospects = await _service.GetProspectsAsync(new Dictionary<string, string>
            {
                ["PLAY_ID"] = basinId
            });

            if (prospects.Count > 0)
                return prospects;

            return await _service.GetProspectsAsync();
        }

        private static Prospect MapLegacyProspect(PROSPECT request)
        {
            var estimatedResources = request.ESTIMATED_OIL_VOLUME;
            if ((!estimatedResources.HasValue || estimatedResources.Value == 0m) && request.ESTIMATED_GAS_VOLUME.HasValue)
                estimatedResources = request.ESTIMATED_GAS_VOLUME;

            return new Prospect
            {
                ProspectId = request.PROSPECT_ID ?? string.Empty,
                FieldId = request.PRIMARY_FIELD_ID ?? string.Empty,
                ProspectName = string.IsNullOrWhiteSpace(request.PROSPECT_NAME) ? request.PROSPECT_SHORT_NAME ?? string.Empty : request.PROSPECT_NAME,
                Description = request.DESCRIPTION,
                Location = request.LOCATION_DESCRIPTION,
                Latitude = request.LATITUDE,
                Longitude = request.LONGITUDE,
                Status = request.PROSPECT_STATUS,
                EvaluationDate = request.DISCOVERY_DATE,
                EstimatedResources = estimatedResources,
                ResourceUnit = request.ESTIMATED_VOLUME_OUOM,
                RiskLevel = request.RISK_LEVEL,
                RiskScore = NormalizeRiskScore(request.RISK_LEVEL, null)
            };
        }

        private static PROSPECT MapLegacyProspect(Prospect prospect)
        {
            return new PROSPECT
            {
                PROSPECT_ID = prospect.ProspectId ?? string.Empty,
                PROSPECT_NAME = prospect.ProspectName ?? string.Empty,
                PRIMARY_FIELD_ID = prospect.FieldId ?? string.Empty,
                PROSPECT_STATUS = prospect.Status ?? string.Empty,
                DESCRIPTION = prospect.Description ?? string.Empty,
                LOCATION_DESCRIPTION = prospect.Location ?? string.Empty,
                LATITUDE = prospect.Latitude,
                LONGITUDE = prospect.Longitude,
                DISCOVERY_DATE = prospect.EvaluationDate,
                ESTIMATED_OIL_VOLUME = prospect.EstimatedResources,
                ESTIMATED_VOLUME_OUOM = prospect.ResourceUnit ?? string.Empty,
                RISK_LEVEL = prospect.RiskLevel ?? string.Empty
            };
        }

        private static ProspectEvaluation BuildEvaluationFromLegacyProspect(PROSPECT request)
        {
            var riskScore = NormalizeRiskScore(request.RISK_LEVEL, null);
            var probabilityOfSuccess = Clamp01(1m - riskScore);

            return new ProspectEvaluation
            {
                EvaluationId = Guid.NewGuid().ToString("N"),
                ProspectId = request.PROSPECT_ID ?? string.Empty,
                EvaluationDate = DateTime.UtcNow,
                EstimatedOilResources = request.ESTIMATED_OIL_VOLUME,
                EstimatedGasResources = request.ESTIMATED_GAS_VOLUME,
                ResourceUnit = request.ESTIMATED_VOLUME_OUOM,
                ProbabilityOfSuccess = probabilityOfSuccess,
                RiskScore = riskScore,
                RiskLevel = string.IsNullOrWhiteSpace(request.RISK_LEVEL) ? CategorizeRisk(riskScore) : request.RISK_LEVEL,
                Recommendation = probabilityOfSuccess >= 0.5m ? "Further evaluation recommended" : "High-risk prospect; de-risk before drilling",
                Remarks = request.DESCRIPTION,
                FieldId = request.PRIMARY_FIELD_ID ?? string.Empty,
                Potential = probabilityOfSuccess >= 0.5m ? "Commercial" : "Speculative"
            };
        }

        private static PROSPECT_RISK_ASSESSMENT MapRiskAssessment(ProspectEvaluation evaluation, PROSPECT request)
        {
            var riskScore = NormalizeRiskScore(evaluation.RiskLevel, evaluation.RiskScore);
            var probabilityOfSuccess = Clamp01(evaluation.ProbabilityOfSuccess ?? (1m - riskScore));
            var unriskedOil = evaluation.EstimatedOilResources ?? request.ESTIMATED_OIL_VOLUME ?? 0m;
            var unriskedGas = evaluation.EstimatedGasResources ?? request.ESTIMATED_GAS_VOLUME ?? 0m;

            return new PROSPECT_RISK_ASSESSMENT
            {
                PROSPECT_ID = string.IsNullOrWhiteSpace(evaluation.ProspectId) ? request.PROSPECT_ID ?? string.Empty : evaluation.ProspectId,
                ASSESSMENT_ID = string.IsNullOrWhiteSpace(evaluation.EvaluationId) ? Guid.NewGuid().ToString("N") : evaluation.EvaluationId,
                RISK_MODEL_TYPE = "LEGACY_COMPAT",
                TRAP_RISK = riskScore,
                RESERVOIR_RISK = riskScore,
                SEAL_RISK = riskScore,
                SOURCE_RISK = riskScore,
                TIMING_RISK = riskScore,
                OVERALL_GEOLOGICAL_RISK = riskScore,
                LOW_ESTIMATE_OIL = decimal.Round(unriskedOil * 0.8m, 2, MidpointRounding.AwayFromZero),
                BEST_ESTIMATE_OIL = unriskedOil,
                HIGH_ESTIMATE_OIL = decimal.Round(unriskedOil * 1.2m, 2, MidpointRounding.AwayFromZero),
                LOW_ESTIMATE_GAS = decimal.Round(unriskedGas * 0.8m, 2, MidpointRounding.AwayFromZero),
                BEST_ESTIMATE_GAS = unriskedGas,
                HIGH_ESTIMATE_GAS = decimal.Round(unriskedGas * 1.2m, 2, MidpointRounding.AwayFromZero),
                RISKED_OIL_VOLUME = decimal.Round(unriskedOil * probabilityOfSuccess, 2, MidpointRounding.AwayFromZero),
                RISKED_GAS_VOLUME = decimal.Round(unriskedGas * probabilityOfSuccess, 2, MidpointRounding.AwayFromZero),
                UNRISKED_OIL_VOLUME = unriskedOil,
                UNRISKED_GAS_VOLUME = unriskedGas,
                EMV = request.ESTIMATED_VALUE,
                NPV = request.ESTIMATED_VALUE,
                RISK_CATEGORY = string.IsNullOrWhiteSpace(evaluation.RiskLevel) ? CategorizeRisk(riskScore) : evaluation.RiskLevel
            };
        }

        private static PROSPECT_VOLUME_ESTIMATE MapVolumeEstimate(Prospect prospect)
        {
            var baseVolume = prospect.EstimatedResources ?? 0m;
            var riskScore = NormalizeRiskScore(prospect.RiskLevel, prospect.RiskScore);
            var probabilityOfSuccess = Clamp01(1m - riskScore);

            return new PROSPECT_VOLUME_ESTIMATE
            {
                PROSPECT_ID = prospect.ProspectId,
                ESTIMATE_ID = Guid.NewGuid().ToString("N"),
                OIL_VOLUME_P10 = decimal.Round(baseVolume * 1.2m, 2, MidpointRounding.AwayFromZero),
                OIL_VOLUME_P50 = baseVolume,
                OIL_VOLUME_P90 = decimal.Round(baseVolume * 0.8m, 2, MidpointRounding.AwayFromZero),
                GAS_VOLUME_P10 = 0m,
                GAS_VOLUME_P50 = 0m,
                GAS_VOLUME_P90 = 0m,
                VOLUME_OUOM = string.IsNullOrWhiteSpace(prospect.ResourceUnit) ? "MMBOE" : prospect.ResourceUnit,
                UNRISKED_OIL_VOLUME = baseVolume,
                UNRISKED_GAS_VOLUME = 0m,
                RISKED_OIL_VOLUME = decimal.Round(baseVolume * probabilityOfSuccess, 2, MidpointRounding.AwayFromZero),
                RISKED_GAS_VOLUME = 0m,
                RECOVERY_FACTOR = 0.35m,
                FORMATION_VOLUME_FACTOR = 1.0m,
                ESTIMATE_METHOD = "LEGACY_COMPAT",
                ESTIMATE_DATE = DateTime.UtcNow,
                ESTIMATOR = "SYSTEM",
                DESCRIPTION = $"Compatibility volumetric estimate for prospect {prospect.ProspectName}"
            };
        }

        private static PROSPECT_RANKING MapLegacyRanking(ProspectRanking ranking, PROSPECT_PORTFOLIO request)
        {
            return new PROSPECT_RANKING
            {
                PROSPECT_ID = ranking.ProspectId,
                RANKING_ID = Guid.NewGuid().ToString("N"),
                RANKING_SCORE = ranking.Score,
                PRIORITY_ORDER = ranking.Rank,
                RANKING_CRITERIA = "EstimatedReserves=0.7;RiskFactor=0.3",
                RANKING_DATE = DateTime.UtcNow,
                RANKER = "SYSTEM",
                METHODOLOGY = "LEGACY_COMPAT_DEFAULT_WEIGHTS",
                DESCRIPTION = string.IsNullOrWhiteSpace(request.PORTFOLIO_NAME)
                    ? $"Top-ranked prospect selected from compatibility portfolio request {request.PORTFOLIO_ID}."
                    : $"Top-ranked prospect selected from compatibility portfolio request {request.PORTFOLIO_NAME}."
            };
        }

        private static decimal NormalizeRiskScore(string? riskLevel, decimal? explicitRiskScore)
        {
            if (explicitRiskScore.HasValue)
                return Clamp01(explicitRiskScore.Value);

            if (string.IsNullOrWhiteSpace(riskLevel))
                return 0.5m;

            return riskLevel.Trim().ToUpperInvariant() switch
            {
                "LOW" => 0.25m,
                "MEDIUM" => 0.5m,
                "HIGH" => 0.75m,
                _ => 0.5m
            };
        }

        private static decimal Clamp01(decimal value)
        {
            if (value < 0m) return 0m;
            if (value > 1m) return 1m;
            return value;
        }

        private static string CategorizeRisk(decimal riskScore)
        {
            if (riskScore >= 0.7m) return "HIGH";
            if (riskScore >= 0.4m) return "MEDIUM";
            return "LOW";
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

