using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Operations
{
    /// <summary>
    /// Workflow-scoped prospect analysis endpoints (narrow DI: maturation, risk/economics, portfolio).
    /// </summary>
    public partial class ProspectIdentificationController
    {
        [HttpPost("workflow/maturation/seismic-interpretation")]
        public async Task<ActionResult<SeismicInterpretationAnalysis>> AnalyzeSeismicInterpretation(
            [FromBody] SeismicInterpretationAnalysisRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _technicalMaturation.AnalyzeSeismicInterpretationAsync(
                request.ProspectId,
                request.SurveyId,
                request.Horizons,
                request.Faults);
            return Ok(result);
        }

        [HttpPost("workflow/maturation/resource-estimate")]
        public async Task<ActionResult<ResourceEstimationResult>> EstimateResources(
            [FromBody] ResourceEstimationRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _technicalMaturation.EstimateResourcesAsync(
                request.ProspectId,
                request.GrossRockVolume,
                request.NetToGrossRatio,
                request.Porosity,
                request.WaterSaturation,
                request.EstimatedBy);
            return Ok(result);
        }

        [HttpPost("workflow/maturation/trap-geometry")]
        public async Task<ActionResult<TrapGeometryAnalysis>> AnalyzeTrapGeometry(
            [FromBody] TrapGeometryAnalysisRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _technicalMaturation.AnalyzeTrapGeometryAsync(
                request.ProspectId,
                request.TrapType,
                request.CrestDepth,
                request.SpillPointDepth,
                request.Area,
                request.Volume);
            return Ok(result);
        }

        [HttpPost("workflow/maturation/migration-path")]
        public async Task<ActionResult<MigrationPathAnalysis>> AnalyzeMigrationPath(
            [FromBody] MigrationPathAnalysisRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _technicalMaturation.AnalyzeMigrationPathAsync(
                request.ProspectId,
                request.SourceRockId,
                request.MaturityLevel,
                request.DistanceKm);
            return Ok(result);
        }

        [HttpPost("workflow/maturation/seal-source")]
        public async Task<ActionResult<SealSourceAssessment>> AssessSealAndSource(
            [FromBody] SealSourceAssessmentRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _technicalMaturation.AssessSealAndSourceAsync(
                request.ProspectId,
                request.SealRockType,
                request.SealThickness,
                request.SourceRockType,
                request.SourceMaturity);
            return Ok(result);
        }

        [HttpPost("workflow/risk-economics/risk-assessment")]
        public async Task<ActionResult<ProspectRiskAnalysisResult>> PerformRiskAssessment(
            [FromBody] WorkflowRiskAssessmentRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _riskEconomic.PerformRiskAssessmentAsync(
                request.ProspectId,
                request.AssessedBy,
                request.RiskScores);
            return Ok(result);
        }

        [HttpPost("workflow/risk-economics/economic-viability")]
        public async Task<ActionResult<EconomicViabilityAnalysis>> AnalyzeEconomicViability(
            [FromBody] EconomicViabilityRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _riskEconomic.AnalyzeEconomicViabilityAsync(
                request.ProspectId,
                request.EstimatedOil,
                request.EstimatedGas,
                request.CapitalCost,
                request.OperatingCost,
                request.OilPrice,
                request.GasPrice);
            return Ok(result);
        }

        [HttpPost("workflow/portfolio/optimize")]
        public async Task<ActionResult<PortfolioOptimizationResult>> OptimizePortfolioWorkflow(
            [FromBody] PortfolioOptimizationWorkflowRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });
            if (request.RankedProspects == null || request.RankedProspects.Count == 0)
                return BadRequest(new { error = "Ranked prospects are required." });

            var result = await _portfolioOptimization.OptimizePortfolioAsync(
                request.RankedProspects,
                request.RiskTolerance,
                request.CapitalBudget);
            return Ok(result);
        }
    }
}
