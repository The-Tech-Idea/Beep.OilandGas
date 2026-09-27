using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Accounting.Cost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
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
        private readonly IAllocationService _allocationService;
        private readonly IAccountingService _accountingService;
        private readonly ILogger<CostAllocationController> _logger;

        public CostAllocationController(
            ProductionAccountingService service,
            IAllocationService allocationService,
            IAccountingService accountingService,
            ILogger<CostAllocationController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _allocationService = allocationService ?? throw new ArgumentNullException(nameof(allocationService));
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            _logger = logger;
        }

        /// <summary>
        /// Calculate a read-only breakdown from recorded costs and direct assignments.
        /// </summary>
        [HttpPost("allocate")]
        public async Task<ActionResult<CostAllocationComputationResult>> AllocateCosts(
            [FromBody] CostAllocationRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrEmpty(request.FieldId))
                        return BadRequest(new { error = "Field ID is required." });

                if (!Enum.TryParse<CostAllocationMethod>(request.AllocationMethod, true, out var allocationMethod) || !Enum.IsDefined(allocationMethod))
                    return BadRequest(new { error = $"Invalid allocation method: {request.AllocationMethod}" });

                if (request.TotalOperatingCosts.HasValue || request.TotalCapitalCosts.HasValue)
                    return BadRequest(new { error = "Totals are derived from recorded costs and cannot be overridden." });

                var startDate = request.AllocationDate.Date;
                var endDate = request.AllocationDate.Date;

                var result = await _accountingService.AllocateCostsAsync(
                    request.FieldId,
                    startDate,
                    endDate,
                    allocationMethod,
                    connectionName);

                return Ok(result);
            }
            catch (NotSupportedException ex) { return BadRequest(new { error = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return UnprocessableEntity(new { error = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating costs");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed production allocation from a run ticket.</summary>
        [HttpPost("service/allocate")]
        public async Task<ActionResult<ALLOCATION_RESULT>> AllocateProductionAsync(
            [FromBody] AllocateProductionRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (request == null || request.RunTicket == null)
                    return BadRequest(new { error = "Run ticket payload is required." });
                if (string.IsNullOrWhiteSpace(request.Method))
                    return BadRequest(new { error = "Allocation method is required." });

                var result = await _allocationService.AllocateAsync(
                    request.RunTicket,
                    request.Method,
                    userId,
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating production via service endpoint");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed allocation lookup by id.</summary>
        [HttpGet("service/{allocationId}")]
        public async Task<ActionResult<ALLOCATION_RESULT>> GetAllocationAsync(
            string allocationId,
            [FromQuery] string connectionName = "PPDM39")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(allocationId))
                    return BadRequest(new { error = "Allocation ID is required." });

                var result = await _allocationService.GetAsync(allocationId, connectionName ?? _service.DefaultConnectionName);
                if (result == null)
                    return NotFound(new { error = $"Allocation {allocationId} not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allocation {AllocationId}", allocationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed allocation detail lookup.</summary>
        [HttpGet("service/{allocationId}/details")]
        public async Task<ActionResult<List<ALLOCATION_DETAIL>>> GetAllocationDetailsAsync(
            string allocationId,
            [FromQuery] string connectionName = "PPDM39")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(allocationId))
                    return BadRequest(new { error = "Allocation ID is required." });

                var details = await _allocationService.GetDetailsAsync(allocationId, connectionName ?? _service.DefaultConnectionName);
                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allocation details for {AllocationId}", allocationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed reverse allocation endpoint.</summary>
        [HttpPost("service/{allocationId}/reverse")]
        public async Task<ActionResult> ReverseAllocationAsync(
            string allocationId,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            try
            {
                if (string.IsNullOrWhiteSpace(allocationId))
                    return BadRequest(new { error = "Allocation ID is required." });

                await _allocationService.ReverseAsync(
                    allocationId,
                    userId,
                    connectionName ?? _service.DefaultConnectionName);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reversing allocation {AllocationId}", allocationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }

}
