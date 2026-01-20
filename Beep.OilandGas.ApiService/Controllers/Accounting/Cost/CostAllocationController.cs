using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Data.Accounting.Cost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Cost
{
    /// <summary>
    /// API controller for Cost Allocation operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/cost/allocations")]
    public class CostAllocationController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly IAccountingService _accountingService;
        private readonly ILogger<CostAllocationController> _logger;

        public CostAllocationController(
            ProductionAccountingService service,
            IAccountingService accountingService,
            ILogger<CostAllocationController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            _logger = logger;
        }

        /// <summary>
        /// Allocate costs to entities.
        /// </summary>
        [HttpPost("allocate")]
        public async Task<ActionResult<object>> AllocateCosts(
            [FromBody] CostAllocationRequest request, 
            [FromQuery] string? connectionName = null,
            [FromQuery] string? userId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrEmpty(request.FieldId))
                    return BadRequest(new { error = "FieldId is required" });

                if (!Enum.TryParse<CostAllocationMethod>(request.AllocationMethod, true, out var allocationMethod))
                    return BadRequest(new { error = $"Invalid allocation method: {request.AllocationMethod}" });

                var startDate = request.AllocationDate.Date;
                var endDate = request.AllocationDate.Date;

                var result = await _accountingService.AllocateCostsAsync(
                    request.FieldId,
                    startDate,
                    endDate,
                    allocationMethod,
                    null);

                var totalOperatingCosts = request.TotalOperatingCosts ?? result.TotalOperatingCosts ?? 0m;
                var totalCapitalCosts = request.TotalCapitalCosts ?? result.TotalCapitalCosts ?? 0m;
                var totalCosts = totalOperatingCosts + totalCapitalCosts;

                var costAllocation = new COST_ALLOCATION
                {
                    COST_ALLOCATION_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = request.FieldId,
                    ALLOCATION_METHOD = request.AllocationMethod,
                    ALLOCATED_AMOUNT = totalCosts,
                    ROW_EFFECTIVE_DATE = request.AllocationDate,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId ?? "system"
                };

                var connName = connectionName ?? _service.DefaultConnectionName;
                var repository = _service.GetRepository(typeof(COST_ALLOCATION), connName, "COST_ALLOCATION");
                await repository.InsertAsync(costAllocation, userId ?? "system");

                return Ok(new
                {
                    AllocationMethod = request.AllocationMethod,
                    TotalOperatingCosts = totalOperatingCosts,
                    TotalCapitalCosts = totalCapitalCosts,
                    TotalCosts = totalCosts,
                    AllocationDetails = result.AllocationDetails?.Select(d => new
                    {
                        EntityType = d.EntityType,
                        EntityName = d.EntityName,
                        AllocatedOperatingCost = d.AllocatedOperatingCost,
                        AllocatedCapitalCost = d.AllocatedCapitalCost,
                        TotalAllocatedCost = d.TotalAllocatedCost,
                        AllocationPercentage = d.AllocationPercentage
                    }) ?? new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating costs");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}

