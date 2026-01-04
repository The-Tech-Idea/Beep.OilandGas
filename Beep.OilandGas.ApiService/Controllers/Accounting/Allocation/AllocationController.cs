using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ProductionAccounting.Allocation;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Allocation
{
    /// <summary>
    /// API controller for Production Allocation operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/allocation")]
    public class AllocationController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly IAccountingService _accountingService;
        private readonly ILogger<AllocationController> _logger;

        public AllocationController(
            ProductionAccountingService service,
            IAccountingService accountingService,
            ILogger<AllocationController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            _logger = logger;
        }

        /// <summary>
        /// Reconcile production volumes between different sources.
        /// </summary>
        [HttpPost("reconcile")]
        public async Task<ActionResult<object>> ReconcileVolumes(
            [FromBody] VolumeReconciliationRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrEmpty(request.FieldId))
                    return BadRequest(new { error = "FieldId is required" });

                var result = await _accountingService.ReconcileVolumesAsync(
                    request.FieldId,
                    request.StartDate,
                    request.EndDate,
                    null);

                return Ok(new
                {
                    Status = result.Status.ToString(),
                    FieldProductionVolume = result.FieldProductionVolume,
                    AllocatedVolume = result.AllocatedVolume,
                    Discrepancy = result.Discrepancy,
                    DiscrepancyPercentage = result.DiscrepancyPercentage,
                    OilVolume = result.OilVolume != null ? new
                    {
                        FieldVolume = result.OilVolume.FieldVolume,
                        AllocatedVolume = result.OilVolume.AllocatedVolume,
                        Discrepancy = result.OilVolume.Discrepancy
                    } : null,
                    GasVolume = result.GasVolume != null ? new
                    {
                        FieldVolume = result.GasVolume.FieldVolume,
                        AllocatedVolume = result.GasVolume.AllocatedVolume,
                        Discrepancy = result.GasVolume.Discrepancy
                    } : null,
                    Issues = result.Issues?.Select(i => new
                    {
                        IssueType = i.IssueType,
                        Description = i.Description,
                        Severity = i.Severity
                    }) ?? new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling volumes");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Perform allocation to wells.
        /// </summary>
        [HttpPost("allocate")]
        public ActionResult<AllocationResultDto> Allocate(
            [FromBody] AllocationRequest request, 
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var wells = request.Entities.Select(e => new WellAllocationData
                {
                    WellId = e.EntityId,
                    WellName = e.EntityName ?? e.EntityId,
                    WorkingInterest = e.WorkingInterest ?? 0m,
                    NetRevenueInterest = e.NetRevenueInterest ?? 0m,
                    MeasuredProduction = e.ProductionHistory ?? 0m,
                    EstimatedProduction = e.ProductionHistory ?? 0m
                }).ToList();

                var result = AllocationEngine.AllocateToWells(
                    request.TotalVolume,
                    wells,
                    request.Method);

                return Ok(MapToAllocationResultDto(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing allocation");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private AllocationResultDto MapToAllocationResultDto(AllocationResult result)
        {
            return new AllocationResultDto
            {
                AllocationId = result.AllocationId,
                AllocationDate = result.AllocationDate,
                Method = result.Method.ToString(),
                TotalVolume = result.TotalVolume,
                AllocatedVolume = result.AllocatedVolume,
                AllocationVariance = result.AllocationVariance,
                Details = result.Details.Select(d => new AllocationDetailDto
                {
                    EntityId = d.EntityId,
                    EntityName = d.EntityName,
                    AllocatedVolume = d.AllocatedVolume,
                    AllocationPercentage = d.AllocationPercentage,
                    WorkingInterest = d.WorkingInterest,
                    NetRevenueInterest = d.NetRevenueInterest
                }).ToList()
            };
        }
    }
}

