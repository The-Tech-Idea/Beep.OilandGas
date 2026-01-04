using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.ProductionAccounting.Financial.FullCost;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Beep.OilandGas.Models.DTOs.Accounting.Financial;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Financial
{
    /// <summary>
    /// API controller for Full Cost accounting operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/financial/full-cost")]
    public class FullCostController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<FullCostController> _logger;

        public FullCostController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<FullCostController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Record exploration costs to a cost center.
        /// </summary>
        [HttpPost("exploration-costs")]
        public async Task<ActionResult<object>> RecordExplorationCosts(
            [FromBody] FullCostExplorationRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateFullCostAccounting(connectionName);
                accounting.RecordExplorationCosts(request.CostCenterId, request.Costs, connectionName);

                // Post to GL: Debit Capitalized Cost, Credit AP/Cash
                var journalEntryId = await _glIntegration.PostCostToGL(
                    request.Costs.PropertyId,
                    request.Costs.TotalExplorationCosts,
                    isCapitalized: true,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { CostCenterId = request.CostCenterId, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for exploration costs");
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording exploration costs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Record development costs to a cost center.
        /// </summary>
        [HttpPost("development-costs")]
        public async Task<ActionResult<object>> RecordDevelopmentCosts(
            [FromBody] FullCostDevelopmentRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateFullCostAccounting(connectionName);
                accounting.RecordDevelopmentCosts(request.CostCenterId, request.Costs, connectionName);

                // Post to GL: Debit Capitalized Cost, Credit AP/Cash
                var journalEntryId = await _glIntegration.PostCostToGL(
                    request.Costs.PropertyId,
                    request.Costs.TotalDevelopmentCosts,
                    isCapitalized: true,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { CostCenterId = request.CostCenterId, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for development costs");
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording development costs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Record acquisition costs to a cost center.
        /// </summary>
        [HttpPost("acquisition-costs")]
        public async Task<ActionResult<object>> RecordAcquisitionCosts(
            [FromBody] FullCostAcquisitionRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateFullCostAccounting(connectionName);
                accounting.RecordAcquisitionCosts(request.CostCenterId, request.Property, connectionName);

                // Post to GL: Debit Capitalized Cost, Credit AP/Cash
                var journalEntryId = await _glIntegration.PostCostToGL(
                    request.Property.PropertyId,
                    request.Property.AcquisitionCost,
                    isCapitalized: true,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { CostCenterId = request.CostCenterId, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for acquisition costs");
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording acquisition costs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Calculate total capitalized costs for a cost center.
        /// </summary>
        [HttpGet("cost-center/{costCenterId}/total-costs")]
        public ActionResult<object> GetTotalCapitalizedCosts(
            string costCenterId,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var accounting = _service.CreateFullCostAccounting(connectionName);
                var totalCosts = accounting.CalculateTotalCapitalizedCosts(costCenterId, connectionName);
                return Ok(new { CostCenterId = costCenterId, TotalCapitalizedCosts = totalCosts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total capitalized costs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Perform ceiling test calculation.
        /// </summary>
        [HttpPost("ceiling-test")]
        public ActionResult<object> PerformCeilingTest(
            [FromBody] CeilingTestRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateFullCostAccounting(connectionName);
                var result = accounting.PerformCeilingTest(request.CostCenterId, request.Reserves, request.DiscountRate ?? 0.10m, connectionName);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing ceiling test");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}

