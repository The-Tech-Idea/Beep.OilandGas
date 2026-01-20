using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.ProductionAccounting.GeneralLedger;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional
{
    /// <summary>
    /// API controller for Purchase Order operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/traditional/purchase-order")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<PurchaseOrderController> _logger;

        public PurchaseOrderController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<PurchaseOrderController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Get purchase order by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<object> GetPurchaseOrder(
            string id,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var po = _service.TraditionalAccounting.PurchaseOrder.GetPurchaseOrder(id);
                if (po == null)
                    return NotFound(new { error = $"Purchase order with ID {id} not found" });

                return Ok(new
                {
                    PurchaseOrderId = po.PURCHASE_ORDER_ID,
                    PoNumber = po.PO_NUMBER,
                    VendorBaId = po.VENDOR_BA_ID,
                    PoDate = po.PO_DATE,
                    Status = po.STATUS
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase order {POId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new purchase order.
        /// </summary>
        [HttpPost]
        public ActionResult<object> CreatePurchaseOrder(
            [FromBody] CreatePurchaseOrderRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var po = _service.TraditionalAccounting.PurchaseOrder.CreatePurchaseOrder(request, userId ?? "system");
                // Note: PO creation doesn't post to GL until receipt
                return Ok(new { PurchaseOrderId = po.PURCHASE_ORDER_ID, PoNumber = po.PO_NUMBER });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

