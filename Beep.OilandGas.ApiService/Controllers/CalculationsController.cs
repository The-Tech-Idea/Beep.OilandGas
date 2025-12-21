using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.ApiService.Services;
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
        public async Task<ActionResult<object>> PerformDCAAnalysis([FromBody] DCARequest request, [FromQuery] string? userId = null)
        {
            string? operationId = null;
            try
            {
                // Set field context if available
                if (_fieldOrchestrator != null && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId) && string.IsNullOrEmpty(request.FieldId))
                {
                    request.FieldId = _fieldOrchestrator.CurrentFieldId;
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    request.UserId = userId;
                }

                // Start progress tracking
                operationId = _progressTracking?.StartOperation("DCA", $"DCA Analysis for Well {request.WellId ?? "N/A"}");
                _progressTracking?.UpdateProgress(operationId!, 10, "Initializing DCA calculation...");

                // Execute calculation asynchronously with progress updates
                var result = await Task.Run(async () =>
                {
                    try
                    {
                        _progressTracking?.UpdateProgress(operationId!, 20, "Fetching production data...");
                        var dcaResult = await _calculationService.PerformDCAAnalysisAsync(request, operationId, _progressTracking);
                        _progressTracking?.UpdateProgress(operationId!, 90, "Saving calculation results...");
                        return dcaResult;
                    }
                    catch (Exception ex)
                    {
                        _progressTracking?.CompleteOperation(operationId!, false, errorMessage: ex.Message);
                        throw;
                    }
                });

                _progressTracking?.CompleteOperation(operationId!, true, "DCA analysis completed successfully");
                return Ok(new { OperationId = operationId, Result = result });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid DCA request");
                if (operationId != null)
                {
                    _progressTracking?.CompleteOperation(operationId, false, errorMessage: ex.Message);
                }
                return BadRequest(new { error = ex.Message, OperationId = operationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing DCA analysis");
                if (operationId != null)
                {
                    _progressTracking?.CompleteOperation(operationId, false, errorMessage: ex.Message);
                }
                return StatusCode(500, new { error = ex.Message, OperationId = operationId });
            }
        }

        /// <summary>
        /// Get DCA calculation result by ID
        /// </summary>
        [HttpGet("dca/{calculationId}")]
        public async Task<ActionResult<DCAResult>> GetDCAResult(string calculationId)
        {
            try
            {
                var result = await _calculationService.GetCalculationResultAsync(calculationId, "DCA");
                if (result == null)
                {
                    return NotFound(new { error = $"DCA calculation {calculationId} not found" });
                }

                if (result is DCAResult dcaResult)
                {
                    return Ok(dcaResult);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DCA result");
                return StatusCode(500, new { error = ex.Message });
            }
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
            try
            {
                // Use current field if no field ID specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                var results = await _calculationService.GetCalculationResultsAsync(wellId, poolId, fieldId, "DCA");
                return Ok(results.Cast<DCAResult>().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DCA results");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Economic Analysis

        /// <summary>
        /// Perform Economic Analysis
        /// </summary>
        [HttpPost("economic")]
        public async Task<ActionResult<EconomicAnalysisResult>> PerformEconomicAnalysis([FromBody] EconomicAnalysisRequest request, [FromQuery] string? userId = null)
        {
            try
            {
                // Set field context if available
                if (_fieldOrchestrator != null && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId) && string.IsNullOrEmpty(request.FieldId))
                {
                    request.FieldId = _fieldOrchestrator.CurrentFieldId;
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    request.UserId = userId;
                }

                var result = await _calculationService.PerformEconomicAnalysisAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid Economic Analysis request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Economic Analysis");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Economic Analysis calculation result by ID
        /// </summary>
        [HttpGet("economic/{calculationId}")]
        public async Task<ActionResult<EconomicAnalysisResult>> GetEconomicAnalysisResult(string calculationId)
        {
            try
            {
                var result = await _calculationService.GetCalculationResultAsync(calculationId, "ECONOMIC");
                if (result == null)
                {
                    return NotFound(new { error = $"Economic Analysis calculation {calculationId} not found" });
                }

                if (result is EconomicAnalysisResult economicResult)
                {
                    return Ok(economicResult);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Economic Analysis result");
                return StatusCode(500, new { error = ex.Message });
            }
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
            try
            {
                // Use current field if no field ID specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                var results = await _calculationService.GetCalculationResultsAsync(wellId, poolId, fieldId, "ECONOMIC");
                return Ok(results.Cast<EconomicAnalysisResult>().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Economic Analysis results");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Nodal Analysis

        /// <summary>
        /// Perform Nodal Analysis
        /// </summary>
        [HttpPost("nodal")]
        public async Task<ActionResult<NodalAnalysisResult>> PerformNodalAnalysis([FromBody] NodalAnalysisRequest request, [FromQuery] string? userId = null)
        {
            try
            {
                // Set field context if available
                if (_fieldOrchestrator != null && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId) && string.IsNullOrEmpty(request.FieldId))
                {
                    request.FieldId = _fieldOrchestrator.CurrentFieldId;
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    request.UserId = userId;
                }

                var result = await _calculationService.PerformNodalAnalysisAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid Nodal Analysis request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Nodal Analysis");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Nodal Analysis calculation result by ID
        /// </summary>
        [HttpGet("nodal/{calculationId}")]
        public async Task<ActionResult<NodalAnalysisResult>> GetNodalAnalysisResult(string calculationId)
        {
            try
            {
                var result = await _calculationService.GetCalculationResultAsync(calculationId, "NODAL");
                if (result == null)
                {
                    return NotFound(new { error = $"Nodal Analysis calculation {calculationId} not found" });
                }

                if (result is NodalAnalysisResult nodalResult)
                {
                    return Ok(nodalResult);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Nodal Analysis result");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Nodal Analysis calculation results for a well, pool, or field
        /// </summary>
        [HttpGet("nodal")]
        public async Task<ActionResult<List<NodalAnalysisResult>>> GetNodalAnalysisResults(
            [FromQuery] string? wellId = null,
            [FromQuery] string? wellboreId = null,
            [FromQuery] string? fieldId = null)
        {
            try
            {
                // Use current field if no field ID specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                var results = await _calculationService.GetCalculationResultsAsync(wellId, null, fieldId, "NODAL");
                return Ok(results.Cast<NodalAnalysisResult>().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Nodal Analysis results");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region General Calculation Operations

        /// <summary>
        /// Get all calculation results for a well, pool, or field (any type)
        /// </summary>
        [HttpGet("results")]
        public async Task<ActionResult<List<object>>> GetCalculationResults(
            [FromQuery] string? wellId = null,
            [FromQuery] string? poolId = null,
            [FromQuery] string? fieldId = null,
            [FromQuery] string? calculationType = null)
        {
            try
            {
                // Use current field if no field ID specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                var results = await _calculationService.GetCalculationResultsAsync(wellId, poolId, fieldId, calculationType);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting calculation results");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion
    }
}
