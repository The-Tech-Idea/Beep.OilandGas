using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting.Financial;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Microsoft.Extensions.Logging;
using ProductionCosts = Beep.OilandGas.Models.Data.ProductionAccounting.ProductionCosts;
using ImpairmentRequest = Beep.OilandGas.Models.Data.Accounting.Financial.ImpairmentRequest;
using Beep.OilandGas.ApiService.Services;


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
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
            accounting.RecordAcquisition(property, connectionName);

            // Post to GL: Debit Unproved Property, Credit AP/Cash
            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostFinancialAccountingToGL(
                    property.PropertyId,
                    "UnprovedProperty",
                    property.AcquisitionCost,
                    isCash: false, // Typically AP
                    transactionDate: DateTime.UtcNow,
                    userId: userId),
                $"Acquisition of property {property.PropertyId}", property.PropertyId, "SUCCESSFUL_EFFORTS");

            return Ok(new { PropertyId = property.PropertyId, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Record exploration costs.
        /// </summary>
        [HttpPost("exploration-costs")]
        public async Task<ActionResult<object>> RecordExplorationCosts(
            [FromBody] ExplorationCosts costs,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
            accounting.RecordExplorationCosts(costs, connectionName);

            // Post to GL: Debit Exploration Expense (if dry hole) or Unproved Property (if capitalized), Credit AP/Cash
            var entryType = costs.IsDryHole ? "ExplorationExpense" : "UnprovedProperty";
            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostFinancialAccountingToGL(
                    costs.PropertyId,
                    entryType,
                    costs.TotalExplorationCosts,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId),
                $"Exploration costs for property {costs.PropertyId}", costs.PropertyId, "SUCCESSFUL_EFFORTS");

            return Ok(new { PropertyId = costs.PropertyId, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Record development costs.
        /// </summary>
        [HttpPost("development-costs")]
        public async Task<ActionResult<object>> RecordDevelopmentCosts(
            [FromBody] DevelopmentCosts costs,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
            accounting.RecordDevelopmentCosts(costs, connectionName);

            // Post to GL: Debit Proved Property, Credit AP/Cash
            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostFinancialAccountingToGL(
                    costs.PropertyId,
                    "ProvedProperty",
                    costs.TotalDevelopmentCosts,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId),
                $"Development costs for property {costs.PropertyId}", costs.PropertyId, "SUCCESSFUL_EFFORTS");

            return Ok(new { PropertyId = costs.PropertyId, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Record production costs (lifting costs).
        /// </summary>
        [HttpPost("production-costs")]
        public async Task<ActionResult<object>> RecordProductionCosts(
            [FromBody] ProductionCosts costs,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
            accounting.RecordProductionCosts(costs, connectionName);

            // Post to GL: Debit Operating Expense, Credit AP/Cash
            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostCostToGL(
                    costs.PropertyId,
                    costs.TotalProductionCosts,
                    isCapitalized: false,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId),
                $"Production costs for property {costs.PropertyId}", costs.PropertyId, "SUCCESSFUL_EFFORTS");

            return Ok(new { PropertyId = costs.PropertyId, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Record a dry hole expense.
        /// </summary>
        [HttpPost("dry-hole")]
        public async Task<ActionResult<object>> RecordDryHole(
            [FromBody] ExplorationCosts costs,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
            accounting.RecordDryHole(costs, connectionName);

            // Post to GL: Debit Exploration Expense, Credit AP/Cash
            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostFinancialAccountingToGL(
                    costs.PropertyId,
                    "ExplorationExpense",
                    costs.TotalExplorationCosts,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId),
                $"Dry hole costs for property {costs.PropertyId}", costs.PropertyId, "SUCCESSFUL_EFFORTS");

            return Ok(new { PropertyId = costs.PropertyId, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Record impairment of an unproved property.
        /// </summary>
        [HttpPost("impairment")]
        public async Task<ActionResult<object>> RecordImpairment(
            [FromBody] ImpairmentRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateSuccessfulEffortsAccounting(connectionName);
            accounting.RecordImpairment(request.PropertyId, request.ImpairmentAmount, connectionName);

            // Post to GL: Debit Impairment Expense, Credit Unproved Property
            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostFinancialAccountingToGL(
                    request.PropertyId,
                    "ImpairmentExpense",
                    request.ImpairmentAmount,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId),
                $"Impairment of property {request.PropertyId}", request.PropertyId, "SUCCESSFUL_EFFORTS");

            return Ok(new { PropertyId = request.PropertyId, JournalEntryId = journalEntryId });
        }
    }

}

