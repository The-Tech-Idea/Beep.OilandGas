using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting.Financial;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Microsoft.Extensions.Logging;
using CeilingTestRequest = Beep.OilandGas.Models.Data.Accounting.Financial.CeilingTestRequest;
using Beep.OilandGas.ApiService.Services;


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
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateFullCostAccounting(connectionName);
            accounting.RecordExplorationCosts(request.CostCenterId, request.Costs, connectionName);

            // Post to GL: Debit Capitalized Cost, Credit AP/Cash
            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostCostToGL(
                    request.Costs.PropertyId,
                    request.Costs.TotalExplorationCosts,
                    isCapitalized: true,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId),
                $"Exploration costs for cost center {request.CostCenterId}", request.CostCenterId, "FULL_COST");

            return Ok(new { CostCenterId = request.CostCenterId, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Record development costs to a cost center.
        /// </summary>
        [HttpPost("development-costs")]
        public async Task<ActionResult<object>> RecordDevelopmentCosts(
            [FromBody] FullCostDevelopmentRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateFullCostAccounting(connectionName);
            accounting.RecordDevelopmentCosts(request.CostCenterId, request.Costs, connectionName);

            // Post to GL: Debit Capitalized Cost, Credit AP/Cash
            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostCostToGL(
                    request.Costs.PropertyId,
                    request.Costs.TotalDevelopmentCosts,
                    isCapitalized: true,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId),
                $"Development costs for cost center {request.CostCenterId}", request.CostCenterId, "FULL_COST");

            return Ok(new { CostCenterId = request.CostCenterId, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Record acquisition costs to a cost center.
        /// </summary>
        [HttpPost("acquisition-costs")]
        public async Task<ActionResult<object>> RecordAcquisitionCosts(
            [FromBody] FullCostAcquisitionRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateFullCostAccounting(connectionName);
            var property = new UnprovedProperty
            {
                PropertyId = request.Property.PropertyId,
                AcquisitionCost = request.Property.AcquisitionCost,
                ProvedDate = request.Property.ProvedDate,
                IsProved = true
            };

            accounting.RecordAcquisitionCosts(request.CostCenterId, property, connectionName);

            // Post to GL: Debit Capitalized Cost, Credit AP/Cash
            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostCostToGL(
                    request.Property.PropertyId,
                    request.Property.AcquisitionCost,
                    isCapitalized: true,
                    isCash: false,
                    transactionDate: DateTime.UtcNow,
                    userId: userId),
                $"Acquisition costs for cost center {request.CostCenterId}", request.CostCenterId, "FULL_COST");

            return Ok(new { CostCenterId = request.CostCenterId, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Calculate total capitalized costs for a cost center.
        /// </summary>
        [HttpGet("cost-center/{costCenterId}/total-costs")]
        public ActionResult<object> GetTotalCapitalizedCosts(
            string costCenterId,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(costCenterId))
                return BadRequest(new { error = "Cost center ID is required." });
            var accounting = _service.CreateFullCostAccounting(connectionName);
            var totalCosts = accounting.CalculateTotalCapitalizedCosts(costCenterId, connectionName);
            return Ok(new { CostCenterId = costCenterId, TotalCapitalizedCosts = totalCosts });
        }

        /// <summary>
        /// Perform ceiling test calculation.
        /// </summary>
        [HttpPost("ceiling-test")]
        public ActionResult<object> PerformCeilingTest(
            [FromBody] CeilingTestRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accounting = _service.CreateFullCostAccounting(connectionName);
            var result = accounting.PerformCeilingTest(request.CostCenterId, request.Reserves, request.DiscountRate ?? 0.10m, connectionName);
            
            return Ok(result);
        }
    }

}

