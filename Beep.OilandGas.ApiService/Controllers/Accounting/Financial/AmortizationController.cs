using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.ApiService.Services;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Financial
{
    /// <summary>
    /// API controller for amortization calculations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/financial/amortization")]
    public class AmortizationController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<AmortizationController> _logger;

        public AmortizationController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<AmortizationController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Calculate amortization using units-of-production method.
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ActionResult<object>> CalculateAmortization(
            [FromBody] AmortizationCalculationRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var amortization = ProductionAccountingService.CalculateAmortization(
                request.NetCapitalizedCosts,
                request.TotalProvedReservesBOE,
                request.ProductionBOE);

            // Post to GL: Debit Amortization Expense, Credit Accumulated Amortization
            var journalEntryId = await _glIntegration.PostFinancialAccountingToGL(
                request.PropertyId ?? Guid.NewGuid().ToString(),
                "AmortizationExpense",
                amortization,
                isCash: false,
                transactionDate: DateTime.UtcNow,
                userId: userId);

            return Ok(new { AmortizationAmount = amortization, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Calculate interest capitalization.
        /// </summary>
        [HttpPost("interest-capitalization")]
        public async Task<ActionResult<object>> CalculateInterestCapitalization(
            [FromBody] InterestCapitalizationData data,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var capitalizedInterest = ProductionAccountingService.CalculateInterestCapitalization(data);

            // Post to GL: Debit Capitalized Cost, Credit Interest Expense
            var journalEntryId = await _glIntegration.PostFinancialAccountingToGL(
                data.PropertyId ?? Guid.NewGuid().ToString(),
                "DevelopmentCost",
                capitalizedInterest,
                isCash: false,
                transactionDate: DateTime.UtcNow,
                userId: userId);

            return Ok(new { CapitalizedInterest = capitalizedInterest, JournalEntryId = journalEntryId });
        }

        /// <summary>
        /// Convert production to BOE.
        /// </summary>
        [HttpPost("convert-production-to-boe")]
        public ActionResult<object> ConvertProductionToBOE(
            [FromBody] ProductionData production,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var boe = ProductionAccountingService.ConvertProductionToBOE(production);
            return Ok(new { BOE = boe });
        }

        /// <summary>
        /// Convert reserves to BOE.
        /// </summary>
        [HttpPost("convert-reserves-to-boe")]
        public ActionResult<object> ConvertReservesToBOE(
            [FromBody] ProvedReserves reserves,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var boe = ProductionAccountingService.ConvertReservesToBOE(reserves);
            return Ok(new { BOE = boe });
        }
    }
}

