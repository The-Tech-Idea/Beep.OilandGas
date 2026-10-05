using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.ApiService.Services;

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
            [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Purchase order ID is required." });
            var po = _service.TraditionalAccounting.PurchaseOrder.GetPurchaseOrder(id);
            if (po == null)
                    return NotFound(new { error = $"Purchase order with ID {id} not found." });

            return Ok(new
            {
                PurchaseOrderId = po.PURCHASE_ORDER_ID,
                PoNumber = po.PO_NUMBER,
                VendorBaId = po.VENDOR_BA_ID,
                PoDate = po.PO_DATE,
                Status = po.STATUS
            });
        }

        /// <summary>
        /// Create a new purchase order.
        /// </summary>
        [HttpPost]
        public ActionResult<object> CreatePurchaseOrder(
            [FromBody] CreatePurchaseOrderRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var po = _service.TraditionalAccounting.PurchaseOrder.CreatePurchaseOrder(request, userId);
            // Note: PO creation doesn't post to GL until receipt
            return Ok(new { PurchaseOrderId = po.PURCHASE_ORDER_ID, PoNumber = po.PO_NUMBER });
        }
    }
}

