using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Data.Accounting.Revenue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Revenue
{
    /// <summary>
    /// API controller for Revenue Allocation operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/revenue/allocations")]
    public class RevenueAllocationController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly ILogger<RevenueAllocationController> _logger;

        public RevenueAllocationController(
            ProductionAccountingService service,
            ILogger<RevenueAllocationController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Allocate revenue to working interests.
        /// </summary>
        [HttpPost("allocate")]
        public async Task<ActionResult<object>> AllocateRevenue(
            [FromBody] RevenueAllocationRequest request,
            [FromQuery] string? connectionName = null,
            [FromQuery] string? userId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var connName = connectionName ?? _service.DefaultConnectionName;
                var repository = _service.GetRepository(typeof(REVENUE_ALLOCATION), connName, "REVENUE_ALLOCATION");

                var allocations = new List<object>();
                var totalAllocated = 0m;

                if (request.WorkingInterests != null && request.WorkingInterests.Any())
                {
                    foreach (var interest in request.WorkingInterests)
                    {
                        var allocatedAmount = request.TotalRevenue * (interest.InterestPercentage / 100m);
                        totalAllocated += allocatedAmount;

                        var allocation = new REVENUE_ALLOCATION
                        {
                            REVENUE_ALLOCATION_ID = Guid.NewGuid().ToString(),
                            REVENUE_TRANSACTION_ID = request.RevenueTransactionId ?? Guid.NewGuid().ToString(),
                            INTEREST_OWNER_BA_ID = interest.OwnerId,
                            INTEREST_PERCENTAGE = interest.InterestPercentage,
                            ALLOCATED_AMOUNT = allocatedAmount,
                            ALLOCATION_METHOD = request.AllocationMethod ?? "WorkingInterest",
                            ROW_EFFECTIVE_DATE = request.AllocationDate,
                            ACTIVE_IND = "Y",
                            ROW_CREATED_DATE = DateTime.UtcNow,
                            ROW_CREATED_BY = userId ?? "system"
                        };

                        await repository.InsertAsync(allocation, userId ?? "system");

                        allocations.Add(new
                        {
                            OwnerId = interest.OwnerId,
                            InterestPercentage = interest.InterestPercentage,
                            AllocatedAmount = allocatedAmount
                        });
                    }
                }

                return Ok(new
                {
                    message = "Revenue allocation completed",
                    TotalRevenue = request.TotalRevenue,
                    TotalAllocated = totalAllocated,
                    Allocations = allocations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating revenue");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}

