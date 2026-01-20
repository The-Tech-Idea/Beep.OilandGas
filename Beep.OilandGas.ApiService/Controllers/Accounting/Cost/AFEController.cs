using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Cost
{
    /// <summary>
    /// API controller for AFE (Authorization for Expenditure) operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/cost/afe")]
    public class AFEController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly ILogger<AFEController> _logger;

        public AFEController(
            ProductionAccountingService service,
            ILogger<AFEController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Create an AFE.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateAFE(
            [FromBody] CreateAFERequest request,
            [FromQuery] string? connectionName = null,
            [FromQuery] string? userId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var connName = connectionName ?? _service.DefaultConnectionName;
                var repository = _service.GetRepository(typeof(AFE), connName, "AFE");

                var afe = new AFE
                {
                    AFE_ID = Guid.NewGuid().ToString(),
                    AFE_NUMBER = request.AfeNumber,
                    AFE_NAME = request.AfeName ?? request.AfeNumber,
                    PROPERTY_ID = request.PropertyId,
                    ESTIMATED_COST = request.BudgetAmount,
                    ROW_EFFECTIVE_DATE = request.EffectiveDate ?? DateTime.UtcNow,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId ?? "system"
                };

                var result = await repository.InsertAsync(afe, userId ?? "system");
                var afeId = afe.AFE_ID;

                return Ok(new { AfeId = afeId, AfeNumber = request.AfeNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AFE");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get AFE by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAFE(
            string id,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var connName = connectionName ?? _service.DefaultConnectionName;
                var repository = _service.GetRepository(typeof(AFE), connName, "AFE");

                var afe = await repository.GetByIdAsync(id) as AFE;
                if (afe == null)
                    return NotFound(new { error = $"AFE with ID {id} not found" });

                return Ok(new
                {
                    AfeId = afe.AFE_ID,
                    AfeNumber = afe.AFE_NUMBER,
                    AfeName = afe.AFE_NAME,
                    PropertyId = afe.PROPERTY_ID,
                    EstimatedCost = afe.ESTIMATED_COST,
                    EffectiveDate = afe.ROW_EFFECTIVE_DATE
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AFE {AfeId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

