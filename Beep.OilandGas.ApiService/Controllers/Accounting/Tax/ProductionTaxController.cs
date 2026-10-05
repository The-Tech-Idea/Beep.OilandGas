using System;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Tax
{
    /// <summary>
    /// Service-backed production tax endpoints aligned with <see cref="IProductionTaxService"/>.
    /// </summary>
    [ApiController]
    [Route("api/accounting/tax/production")]
    public class ProductionTaxController : ControllerBase
    {
        private readonly IProductionTaxService _productionTaxService;
        private readonly ILogger<ProductionTaxController> _logger;

        public ProductionTaxController(
            IProductionTaxService productionTaxService,
            ILogger<ProductionTaxController> logger)
        {
            _productionTaxService = productionTaxService ?? throw new ArgumentNullException(nameof(productionTaxService));
            _logger = logger;
        }

        /// <summary>
        /// Calculates and persists severance/ad valorem tax for a revenue transaction.
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ActionResult<TAX_TRANSACTION>> CalculateAsync(
            [FromBody] REVENUE_TRANSACTION revenueTransaction,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tax = await _productionTaxService.CalculateProductionTaxesAsync(
                revenueTransaction,
                userId,
                connectionName ?? "PPDM39");

            if (tax == null)
                return Ok(new { message = "No production tax generated for this transaction." });

            return Ok(tax);
        }
    }
}
