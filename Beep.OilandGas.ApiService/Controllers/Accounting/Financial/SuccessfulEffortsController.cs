using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.ProductionAccounting.Financial.SuccessfulEfforts;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Financial
{
    /// <summary>
    /// API controller for Successful Efforts accounting operations (FASB Statement No. 19).
    /// </summary>
    [ApiController]
    [Route("api/accounting/financial/successful-efforts")]
    public class SuccessfulEffortsController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<SuccessfulEffortsController> _logger;

        public SuccessfulEffortsController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<SuccessfulEffortsController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Record acquisition of an unproved property.
        /// </summary>
        [HttpPost("acquisition")]
        public async Task<ActionResult<object>> RecordAcquisition(
            [FromBody] UnprovedProperty property,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
                accounting.RecordAcquisition(property, connectionName);

                // Post to GL: Debit Unproved Property, Credit AP/Cash
                var journalEntryId = await _glIntegration.PostFinancialAccountingToGL(
                    property.PropertyId,
                    "UnprovedProperty",
                    property.AcquisitionCost,
                    isCash: false, // Typically AP
                    transactionDate: DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { PropertyId = property.PropertyId, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for acquisition {PropertyId}", property?.PropertyId);
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording acquisition");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Record exploration costs.
        /// </summary>
        [HttpPost("exploration-costs")]
        public async Task<ActionResult<object>> RecordExplorationCosts(
            [FromBody] ExplorationCosts costs,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
                accounting.RecordExplorationCosts(costs, connectionName);

                // Post to GL: Debit Exploration Expense (if dry hole) or Unproved Property (if capitalized), Credit AP/Cash
                var entryType = costs.IsDryHole ? "ExplorationExpense" : "UnprovedProperty";
                var journalEntryId = await _glIntegration.PostFinancialAccountingToGL(
                    costs.PropertyId,
                    entryType,
                    costs.TotalExplorationCosts,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { PropertyId = costs.PropertyId, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for exploration costs {PropertyId}", costs?.PropertyId);
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording exploration costs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Record development costs.
        /// </summary>
        [HttpPost("development-costs")]
        public async Task<ActionResult<object>> RecordDevelopmentCosts(
            [FromBody] DevelopmentCosts costs,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
                accounting.RecordDevelopmentCosts(costs, connectionName);

                // Post to GL: Debit Proved Property, Credit AP/Cash
                var journalEntryId = await _glIntegration.PostFinancialAccountingToGL(
                    costs.PropertyId,
                    "ProvedProperty",
                    costs.TotalDevelopmentCosts,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { PropertyId = costs.PropertyId, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for development costs {PropertyId}", costs?.PropertyId);
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording development costs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Record production costs (lifting costs).
        /// </summary>
        [HttpPost("production-costs")]
        public async Task<ActionResult<object>> RecordProductionCosts(
            [FromBody] ProductionCosts costs,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
                accounting.RecordProductionCosts(costs, connectionName);

                // Post to GL: Debit Operating Expense, Credit AP/Cash
                var journalEntryId = await _glIntegration.PostCostToGL(
                    costs.PropertyId,
                    costs.TotalProductionCosts,
                    isCapitalized: false,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { PropertyId = costs.PropertyId, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for production costs {PropertyId}", costs?.PropertyId);
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording production costs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Record a dry hole expense.
        /// </summary>
        [HttpPost("dry-hole")]
        public async Task<ActionResult<object>> RecordDryHole(
            [FromBody] ExplorationCosts costs,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
                accounting.RecordDryHole(costs, connectionName);

                // Post to GL: Debit Exploration Expense, Credit AP/Cash
                var journalEntryId = await _glIntegration.PostFinancialAccountingToGL(
                    costs.PropertyId,
                    "ExplorationExpense",
                    costs.TotalExplorationCosts,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { PropertyId = costs.PropertyId, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for dry hole {PropertyId}", costs?.PropertyId);
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording dry hole");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Record impairment of an unproved property.
        /// </summary>
        [HttpPost("impairment")]
        public async Task<ActionResult<object>> RecordImpairment(
            [FromBody] ImpairmentRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
                accounting.RecordImpairment(request.PropertyId, request.ImpairmentAmount, connectionName);

                // Post to GL: Debit Impairment Expense, Credit Unproved Property
                var journalEntryId = await _glIntegration.PostFinancialAccountingToGL(
                    request.PropertyId,
                    "ImpairmentExpense",
                    request.ImpairmentAmount,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { PropertyId = request.PropertyId, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for impairment {PropertyId}", request?.PropertyId);
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording impairment");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ImpairmentRequest
    {
        public string PropertyId { get; set; } = string.Empty;
        public decimal ImpairmentAmount { get; set; }
    }
}

