using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ApiService.Services;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// API controller for Production Accounting operations
    /// Supports volume reconciliation, royalty calculations, and cost allocation
    /// </summary>
    [ApiController]
    [Route("api/accounting")]
    public class AccountingController : ControllerBase
    {
        private readonly IAccountingService _accountingService;
        private readonly IFieldOrchestrator? _fieldOrchestrator;
        private readonly IProgressTrackingService? _progressTracking;
        private readonly ILogger<AccountingController> _logger;

        public AccountingController(
            IAccountingService accountingService,
            IFieldOrchestrator? fieldOrchestrator,
            IProgressTrackingService? progressTracking,
            ILogger<AccountingController> logger)
        {
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            _fieldOrchestrator = fieldOrchestrator;
            _progressTracking = progressTracking;
            _logger = logger;
        }

        #region Volume Reconciliation

        /// <summary>
        /// Reconcile production volumes between different sources
        /// </summary>
        [HttpPost("reconcile-volumes")]
        public async Task<ActionResult<object>> ReconcileVolumes(
            [FromBody] VolumeReconciliationRequest request)
        {
            string? operationId = null;
            try
            {
                // Use current field if available and fieldId not specified
                var fieldId = request.FieldId;
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                if (string.IsNullOrEmpty(fieldId))
                {
                    return BadRequest(new { error = "Field ID is required" });
                }

                // Start progress tracking
                operationId = _progressTracking?.StartOperation("VolumeReconciliation", $"Volume Reconciliation for Field {fieldId}");
                _progressTracking?.UpdateProgress(operationId!, 10, "Starting volume reconciliation...");

                // Execute reconciliation with progress updates
                var result = await Task.Run(async () =>
                {
                    try
                    {
                        _progressTracking?.UpdateProgress(operationId!, 20, "Fetching production data...");
                        var reconciliationResult = await _accountingService.ReconcileVolumesAsync(
                            fieldId, 
                            request.StartDate, 
                            request.EndDate, 
                            request.AdditionalFilters);
                        _progressTracking?.UpdateProgress(operationId!, 90, "Calculating discrepancies...");
                        return reconciliationResult;
                    }
                    catch (Exception ex)
                    {
                        _progressTracking?.CompleteOperation(operationId!, false, errorMessage: ex.Message);
                        throw;
                    }
                });

                _progressTracking?.CompleteOperation(operationId!, true, "Volume reconciliation completed successfully");
                return Ok(new { OperationId = operationId, Result = result });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid volume reconciliation request");
                if (operationId != null)
                {
                    _progressTracking?.CompleteOperation(operationId, false, errorMessage: ex.Message);
                }
                return BadRequest(new { error = ex.Message, OperationId = operationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling volumes");
                if (operationId != null)
                {
                    _progressTracking?.CompleteOperation(operationId, false, errorMessage: ex.Message);
                }
                return StatusCode(500, new { error = ex.Message, OperationId = operationId });
            }
        }

        /// <summary>
        /// Get volume reconciliation results
        /// </summary>
        [HttpGet("reconcile-volumes")]
        public async Task<ActionResult<List<VolumeReconciliationResult>>> GetVolumeReconciliations(
            [FromQuery] string? fieldId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Use current field if available and fieldId not specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                // Note: This would require storing reconciliation results in a table
                // For now, return empty list as placeholder
                return Ok(new List<VolumeReconciliationResult>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting volume reconciliations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Royalty Calculations

        /// <summary>
        /// Calculate royalties for production
        /// </summary>
        [HttpPost("calculate-royalties")]
        public async Task<ActionResult<object>> CalculateRoyalties(
            [FromBody] RoyaltyCalculationRequest request)
        {
            string? operationId = null;
            try
            {
                // Use current field if available and fieldId not specified
                var fieldId = request.FieldId;
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                if (string.IsNullOrEmpty(fieldId))
                {
                    return BadRequest(new { error = "Field ID is required" });
                }

                // Start progress tracking
                operationId = _progressTracking?.StartOperation("RoyaltyCalculation", $"Royalty Calculation for Field {fieldId}");
                _progressTracking?.UpdateProgress(operationId!, 10, "Starting royalty calculation...");

                // Determine date range from request
                var startDate = request.CalculationDate.Date;
                var endDate = request.CalculationDate.Date;

                // Execute calculation with progress updates
                var result = await Task.Run(async () =>
                {
                    try
                    {
                        _progressTracking?.UpdateProgress(operationId!, 30, "Fetching production data...");
                        var royaltyResult = await _accountingService.CalculateRoyaltiesAsync(
                            fieldId,
                            startDate,
                            endDate,
                            request.PoolId);
                        _progressTracking?.UpdateProgress(operationId!, 70, "Calculating royalty amounts...");

                        // Save royalty calculation to database
                        var royaltyData = new Dictionary<string, object>
                        {
                            { "FIELD_ID", fieldId },
                            { "POOL_ID", request.PoolId ?? "" },
                            { "CALCULATION_DATE", request.CalculationDate },
                            { "GROSS_OIL_VOLUME", royaltyResult.GrossOilVolume ?? 0 },
                            { "GROSS_GAS_VOLUME", royaltyResult.GrossGasVolume ?? 0 },
                            { "OIL_ROYALTY_RATE", request.OilRoyaltyRate ?? 12.5m },
                            { "GAS_ROYALTY_RATE", request.GasRoyaltyRate ?? 12.5m },
                            { "ROYALTY_OIL_VOLUME", royaltyResult.RoyaltyOilVolume ?? 0 },
                            { "ROYALTY_GAS_VOLUME", royaltyResult.RoyaltyGasVolume ?? 0 },
                            { "ROYALTY_OIL_VALUE", royaltyResult.RoyaltyOilValue ?? 0 },
                            { "ROYALTY_GAS_VALUE", royaltyResult.RoyaltyGasValue ?? 0 },
                            { "TOTAL_ROYALTY_VALUE", royaltyResult.TotalRoyaltyValue ?? 0 }
                        };

                        _progressTracking?.UpdateProgress(operationId!, 90, "Saving calculation results...");
                        await _accountingService.SaveRoyaltyCalculationAsync(royaltyData, "system");
                        return royaltyResult;
                    }
                    catch (Exception ex)
                    {
                        _progressTracking?.CompleteOperation(operationId!, false, errorMessage: ex.Message);
                        throw;
                    }
                });

                _progressTracking?.CompleteOperation(operationId!, true, "Royalty calculation completed successfully");
                return Ok(new { OperationId = operationId, Result = result });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid royalty calculation request");
                if (operationId != null)
                {
                    _progressTracking?.CompleteOperation(operationId, false, errorMessage: ex.Message);
                }
                return BadRequest(new { error = ex.Message, OperationId = operationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating royalties");
                if (operationId != null)
                {
                    _progressTracking?.CompleteOperation(operationId, false, errorMessage: ex.Message);
                }
                return StatusCode(500, new { error = ex.Message, OperationId = operationId });
            }
        }

        /// <summary>
        /// Get royalty calculation records
        /// </summary>
        [HttpGet("royalties")]
        public async Task<ActionResult<List<ROYALTY_CALCULATION>>> GetRoyaltyCalculations(
            [FromQuery] string? fieldId = null,
            [FromQuery] string? poolId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Use current field if available and fieldId not specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                var results = await _accountingService.GetRoyaltyCalculationsAsync(
                    fieldId, poolId, startDate, endDate);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting royalty calculations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Cost Allocation

        /// <summary>
        /// Allocate costs to production entities
        /// </summary>
        [HttpPost("allocate-costs")]
        public async Task<ActionResult<CostAllocationResult>> AllocateCosts(
            [FromBody] CostAllocationRequest request)
        {
            try
            {
                // Use current field if available and fieldId not specified
                var fieldId = request.FieldId;
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                if (string.IsNullOrEmpty(fieldId))
                {
                    return BadRequest(new { error = "Field ID is required" });
                }

                // Determine date range from request
                var startDate = request.AllocationDate.Date;
                var endDate = request.AllocationDate.Date;

                var result = await _accountingService.AllocateCostsAsync(
                    fieldId,
                    startDate,
                    endDate,
                    request.AllocationMethod);

                // Save cost allocation to database
                var costAllocationData = new Dictionary<string, object>
                {
                    { "FIELD_ID", fieldId },
                    { "ALLOCATION_DATE", request.AllocationDate },
                    { "ALLOCATION_METHOD", request.AllocationMethod.ToString() },
                    { "TOTAL_OPERATING_COSTS", request.TotalOperatingCosts ?? 0 },
                    { "TOTAL_CAPITAL_COSTS", request.TotalCapitalCosts ?? 0 },
                    { "TOTAL_COSTS", (request.TotalOperatingCosts ?? 0) + (request.TotalCapitalCosts ?? 0) }
                };

                await _accountingService.SaveCostAllocationAsync(costAllocationData, "system");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid cost allocation request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating costs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get cost allocation records
        /// </summary>
        [HttpGet("cost-allocations")]
        public async Task<ActionResult<List<COST_ALLOCATION>>> GetCostAllocations(
            [FromQuery] string? fieldId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Use current field if available and fieldId not specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                var results = await _accountingService.GetCostAllocationsAsync(
                    fieldId, startDate, endDate);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cost allocations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Accounting Allocations

        /// <summary>
        /// Get accounting allocation records
        /// </summary>
        [HttpGet("allocations")]
        public async Task<ActionResult<List<ACCOUNTING_ALLOCATION>>> GetAccountingAllocations(
            [FromQuery] string? fieldId = null,
            [FromQuery] string? poolId = null,
            [FromQuery] string? wellId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Use current field if available and fieldId not specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                var results = await _accountingService.GetAccountingAllocationsAsync(
                    fieldId, poolId, wellId, startDate, endDate);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounting allocations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create or update an accounting allocation record
        /// </summary>
        [HttpPost("allocations")]
        public async Task<ActionResult<object>> CreateAccountingAllocation(
            [FromBody] AccountingAllocationRequest request,
            [FromQuery] string? userId = null)
        {
            try
            {
                // Use current field if available and fieldId not specified
                var fieldId = request.FieldId;
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                if (string.IsNullOrEmpty(fieldId))
                {
                    return BadRequest(new { error = "Field ID is required" });
                }

                var allocationData = new Dictionary<string, object>
                {
                    { "FIELD_ID", fieldId },
                    { "POOL_ID", request.PoolId ?? "" },
                    { "WELL_ID", request.WellId ?? "" },
                    { "FACILITY_ID", request.FacilityId ?? "" },
                    { "ALLOCATION_DATE", request.AllocationDate },
                    { "PRODUCT_TYPE", request.ProductType ?? "" },
                    { "ALLOCATED_VOLUME", request.AllocatedVolume ?? 0 },
                    { "ALLOCATION_PERCENTAGE", request.AllocationPercentage ?? 0 },
                    { "ALLOCATION_METHOD", request.AllocationMethod ?? "" }
                };

                if (request.AdditionalData != null)
                {
                    foreach (var kvp in request.AdditionalData)
                    {
                        allocationData[kvp.Key] = kvp.Value;
                    }
                }

                var result = await _accountingService.SaveAccountingAllocationAsync(
                    allocationData, userId ?? "system");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accounting allocation");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion
    }

    #region Request DTOs

    /// <summary>
    /// Request for volume reconciliation
    /// </summary>
    public class VolumeReconciliationRequest
    {
        public string? FieldId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<AppFilter>? AdditionalFilters { get; set; }
    }

    #endregion
}
