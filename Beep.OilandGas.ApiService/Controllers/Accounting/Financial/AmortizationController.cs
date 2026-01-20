using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

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
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
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
                    userId: userId ?? "system");

                return Ok(new { AmortizationAmount = amortization, JournalEntryId = journalEntryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating amortization");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Calculate interest capitalization.
        /// </summary>
        [HttpPost("interest-capitalization")]
        public async Task<ActionResult<object>> CalculateInterestCapitalization(
            [FromBody] InterestCapitalizationData data,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
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
                    userId: userId ?? "system");

                return Ok(new { CapitalizedInterest = capitalizedInterest, JournalEntryId = journalEntryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating interest capitalization");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Convert production to BOE.
        /// </summary>
        [HttpPost("convert-production-to-boe")]
        public ActionResult<object> ConvertProductionToBOE(
            [FromBody] ProductionData production,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var boe = ProductionAccountingService.ConvertProductionToBOE(production);
                return Ok(new { BOE = boe });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting production to BOE");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Convert reserves to BOE.
        /// </summary>
        [HttpPost("convert-reserves-to-boe")]
        public ActionResult<object> ConvertReservesToBOE(
            [FromBody] ProvedReserves reserves,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var boe = ProductionAccountingService.ConvertReservesToBOE(reserves);
                return Ok(new { BOE = boe });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting reserves to BOE");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

