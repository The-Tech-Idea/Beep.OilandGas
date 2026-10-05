using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.WellTestAnalysis.Exceptions;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers
{

    /// <summary>
    /// API controller for calculation operations (DCA, Economic Analysis, Nodal Analysis)
    /// Integrates calculation services with PPDM39 data via PPDMGenericRepository
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalculationsController : ControllerBase
    {
        private readonly ICalculationService _calculationService;
        private readonly IFieldOrchestrator? _fieldOrchestrator;
        private readonly IProgressTrackingService? _progressTracking;
        private readonly ILogger<CalculationsController> _logger;

        public CalculationsController(
            ICalculationService calculationService,
            IFieldOrchestrator? fieldOrchestrator,
            IProgressTrackingService? progressTracking,
            ILogger<CalculationsController> logger)
        {
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _fieldOrchestrator = fieldOrchestrator;
            _progressTracking = progressTracking;
            _logger = logger;
        }

        #region Decline Curve Analysis (DCA)

        /// <summary>
        /// Perform Decline Curve Analysis
        /// </summary>
        [HttpPost("dca")]
        public async Task<ActionResult<object>> PerformDCAAnalysis([FromBody] DCARequest request)
        {
            var userId = User.ActingUserId();
            // Set field context if available
            if (_fieldOrchestrator != null && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId) && string.IsNullOrEmpty(request.FieldId))
            {
                request.FieldId = _fieldOrchestrator.CurrentFieldId;
            }

            request.UserId = userId;

            // Start progress tracking
            var operationId = _progressTracking?.StartOperation("DCA", $"DCA Analysis for Well {request.WellId ?? "N/A"}");
            _progressTracking?.UpdateProgress(operationId!, 10, "Initializing DCA calculation...");

            // Execute calculation asynchronously with progress updates
            var result = await Task.Run(async () =>
            {
                try
                {
                    _progressTracking?.UpdateProgress(operationId!, 20, "Fetching production data...");
                    var dcaResult = await _calculationService.PerformDCAAnalysisAsync(request);
                    _progressTracking?.UpdateProgress(operationId!, 90, "Saving calculation results...");
                    return dcaResult;
                }
                // Whatever ends the calculation, the tracked operation is closed as not completed before the exception goes
                // on to the API's handler, which answers a refusal with its sentence and reports anything else.
                catch (Exception)
                {
                    _progressTracking?.CompleteOperation(operationId!, false, errorMessage: "The DCA analysis did not complete.");
                    throw;
                }
            });

            _progressTracking?.CompleteOperation(operationId!, true, "DCA analysis completed successfully");
            return Ok(new { OperationId = operationId, Result = result });
        }

        /// <summary>
        /// Get DCA calculation result by ID
        /// </summary>
        [HttpGet("dca/{calculationId}")]
        public async Task<ActionResult<DCAResult>> GetDCAResult(string calculationId)
        {
            if (string.IsNullOrWhiteSpace(calculationId)) return BadRequest(new { error = "Calculation ID is required." });
            var result = await _calculationService.GetCalculationResultAsync(calculationId, "DCA");
            if (result == null)
            {
                    return NotFound(new { error = $"DCA calculation {calculationId} not found." });
            }

            if (result is DCAResult dcaResult)
            {
                return Ok(dcaResult);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get DCA calculation results for a well, pool, or field
        /// </summary>
        [HttpGet("dca")]
        public async Task<ActionResult<List<DCAResult>>> GetDCAResults(
            [FromQuery] string? wellId = null,
            [FromQuery] string? poolId = null,
            [FromQuery] string? fieldId = null)
        {
            // Use current field if no field ID specified
            if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
            {
                fieldId = _fieldOrchestrator.CurrentFieldId;
            }

            var results = await _calculationService.GetCalculationResultsAsync(wellId, poolId, fieldId, "DCA");
            return Ok(results.DcaResults);
        }

        #endregion

        #region Choke Analysis

        /// <summary>
        /// Perform Choke Analysis.
        /// </summary>
        [HttpPost("choke")]
        public async Task<ActionResult<ChokeAnalysisResult>> PerformChokeAnalysis([FromBody] ChokeAnalysisRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null)
                return BadRequest(new { error = "Request body is required." });
            request.UserId = userId;

            var result = await _calculationService.PerformChokeAnalysisAsync(request);
            return Ok(result);
        }

        #endregion

        #region Compressor Analysis

        /// <summary>
        /// Perform packaged compressor analysis for a facility (orchestrated centrifugal/recip pressure vs power paths).
        /// Calculation failures are returned as HTTP 200 with <see cref="CompressorAnalysisResult.Status"/> set to
        /// <see cref="CalculationRunStatus.Failed"/> and <see cref="CompressorAnalysisResult.ErrorMessage"/> populated (same pattern as other packaged calculators).
        /// </summary>
        [HttpPost("compressor")]
        public async Task<ActionResult<CompressorAnalysisResult>> PerformCompressorAnalysis([FromBody] CompressorAnalysisRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            request.UserId = userId;

            var result = await _calculationService.PerformCompressorAnalysisAsync(request);
            return Ok(result);
        }

        #endregion

        #region Economic Analysis

        /// <summary>
        /// Perform Economic Analysis
        /// </summary>
        [HttpPost("economic")]
        public async Task<ActionResult<EconomicAnalysisResult>> PerformEconomicAnalysis([FromBody] EconomicAnalysisRequest request)
        {
            var userId = User.ActingUserId();
            // Set field context if available
            if (_fieldOrchestrator != null && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId) && string.IsNullOrEmpty(request.FieldId))
            {
                request.FieldId = _fieldOrchestrator.CurrentFieldId;
            }

            request.UserId = userId;

            var result = await _calculationService.PerformEconomicAnalysisAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get Economic Analysis calculation result by ID
        /// </summary>
        [HttpGet("economic/{calculationId}")]
        public async Task<ActionResult<EconomicAnalysisResult>> GetEconomicAnalysisResult(string calculationId)
        {
            if (string.IsNullOrWhiteSpace(calculationId)) return BadRequest(new { error = "Calculation ID is required." });
            var result = await _calculationService.GetCalculationResultAsync(calculationId, "ECONOMIC");
            if (result == null)
            {
                    return NotFound(new { error = $"Economic Analysis calculation {calculationId} not found." });
            }

            if (result is EconomicAnalysisResult economicResult)
            {
                return Ok(economicResult);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get Economic Analysis calculation results for a well, pool, or field
        /// </summary>
        [HttpGet("economic")]
        public async Task<ActionResult<List<EconomicAnalysisResult>>> GetEconomicAnalysisResults(
            [FromQuery] string? wellId = null,
            [FromQuery] string? poolId = null,
            [FromQuery] string? fieldId = null)
        {
            // Use current field if no field ID specified
            if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
            {
                fieldId = _fieldOrchestrator.CurrentFieldId;
            }

            var results = await _calculationService.GetCalculationResultsAsync(wellId, poolId, fieldId, "ECONOMIC");
            return Ok(results.EconomicResults);
        }

        #endregion

        #region Nodal Analysis

        /// <summary>
        /// Perform Nodal Analysis (legacy calculation-integration entry).
        /// </summary>
        /// <remarks>
        /// Prefer <c>/api/nodalanalysis/*</c> (<see cref="NodalAnalysisHttpRoutes"/>) for <c>NodalAnalysisRunResult</c> workflows (analyze, save, history, diagnostics).
        /// </remarks>
        [HttpPost("nodal")]
        public async Task<ActionResult<NodalAnalysisResult>> PerformNodalAnalysis([FromBody] NodalAnalysisRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null)
                return BadRequest(new { error = "Request body is required." });
            // Set field context if available
            if (_fieldOrchestrator != null && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId) && string.IsNullOrEmpty(request.FieldId))
            {
                request.FieldId = _fieldOrchestrator.CurrentFieldId;
            }

            request.UserId = userId;

            var result = await _calculationService.PerformNodalAnalysisAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get Nodal Analysis calculation result by ID (legacy calculation store).
        /// </summary>
        /// <remarks>
        /// For well-scoped <c>NodalAnalysisRunResult</c> history from PPDM, use <c>GET /api/nodalanalysis/history/{{wellUWI}}</c> (<see cref="NodalAnalysisHttpRoutes"/>).
        /// </remarks>
        [HttpGet("nodal/{calculationId}")]
        public async Task<ActionResult<NodalAnalysisResult>> GetNodalAnalysisResult(string calculationId)
        {
            if (string.IsNullOrWhiteSpace(calculationId)) return BadRequest(new { error = "Calculation ID is required." });
            var result = await _calculationService.GetCalculationResultAsync(calculationId, "NODAL");
            if (result == null)
            {
                    return NotFound(new { error = $"Nodal Analysis calculation {calculationId} not found." });
            }

            if (result is NodalAnalysisResult nodalResult)
            {
                return Ok(nodalResult);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get Nodal Analysis calculation results for a well, pool, or field (legacy calculation store).
        /// </summary>
        /// <remarks>
        /// Prefer <c>/api/nodalanalysis/*</c> (<see cref="NodalAnalysisHttpRoutes"/>) for dedicated nodal workflows.
        /// </remarks>
        [HttpGet("nodal")]
        public async Task<ActionResult<List<NodalAnalysisResult>>> GetNodalAnalysisResults(
            [FromQuery] string? wellId = null,
            [FromQuery] string? wellboreId = null,
            [FromQuery] string? fieldId = null)
        {
            // Use current field if no field ID specified
            if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
            {
                fieldId = _fieldOrchestrator.CurrentFieldId;
            }

            var results = await _calculationService.GetCalculationResultsAsync(wellId, null, fieldId, "NODAL");
            return Ok(results.NodalResults);
        }

        #endregion

        #region Well Test Analysis

        /// <summary>
        /// Perform Well Test Analysis
        /// </summary>
        [HttpPost("well-test")]
        public async Task<ActionResult<WELL_TEST_ANALYSIS_RESULT>> PerformWellTestAnalysis([FromBody] WellTestAnalysisCalculationRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null)
            {
                return BadRequest(new { error = "Well test analysis request body is required." });
            }

            request.UserId = userId;

            var result = await _calculationService.PerformWellTestAnalysisAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get Well Test Analysis result by ID
        /// </summary>
        [HttpGet("well-test/{calculationId}")]
        public async Task<ActionResult<WELL_TEST_ANALYSIS_RESULT>> GetWellTestAnalysisResult(string calculationId)
        {
            if (string.IsNullOrWhiteSpace(calculationId)) return BadRequest(new { error = "Calculation ID is required." });
            var result = await _calculationService.GetCalculationResultAsync(calculationId, "WELL_TEST");
            if (result == null)
            {
                return NotFound(new { error = $"Well Test Analysis calculation {calculationId} not found." });
            }

            if (result is WELL_TEST_ANALYSIS_RESULT wellTestResult)
            {
                return Ok(wellTestResult);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get Well Test Analysis results for a well or field
        /// </summary>
        [HttpGet("well-test")]
        public async Task<ActionResult<List<WELL_TEST_ANALYSIS_RESULT>>> GetWellTestAnalysisResults(
            [FromQuery] string? wellId = null,
            [FromQuery] string? testId = null,
            [FromQuery] string? fieldId = null)
        {
            var results = await _calculationService.GetCalculationResultsAsync(wellId, null, fieldId, "WELL_TEST");
            var history = results.WellTestResults;

            if (!string.IsNullOrWhiteSpace(testId))
            {
                history = history
                    .Where(result => string.Equals(result.TEST_ID, testId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return Ok(history);
        }

        #endregion

        #region General Calculation Operations

        /// <summary>
        /// Get all calculation results for a well, pool, or field (any type)
        /// </summary>
        [HttpGet("results")]
        public async Task<ActionResult<CalculationResultsResponse>> GetCalculationResults(
            [FromQuery] string? wellId = null,
            [FromQuery] string? poolId = null,
            [FromQuery] string? fieldId = null,
            [FromQuery] string? calculationType = null)
        {
            // Use current field if no field ID specified
            if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
            {
                fieldId = _fieldOrchestrator.CurrentFieldId;
            }

            var results = await _calculationService.GetCalculationResultsAsync(wellId, poolId, fieldId, calculationType);
            return Ok(results);
        }

        #endregion
    }
}
