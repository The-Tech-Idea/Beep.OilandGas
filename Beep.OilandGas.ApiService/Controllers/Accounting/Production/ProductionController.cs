using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.Data.Accounting.Production;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Production
{
    /// <summary>
    /// API controller for Production operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/production")]
    public class ProductionController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly ILogger<ProductionController> _logger;

        public ProductionController(
            ProductionAccountingService service,
            ILogger<ProductionController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Get tank inventory by ID.
        /// </summary>
        [HttpGet("inventory/{id}")]
        public ActionResult<object> GetTankInventory(
            string id,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var inventory = _service.ProductionManager.GetTankInventory(id);
                if (inventory == null)
                    return NotFound(new { error = $"Tank inventory with ID {id} not found" });

                return Ok(new
                {
                    InventoryId = inventory.InventoryId,
                    TankBatteryId = inventory.TankBatteryId,
                    InventoryDate = inventory.InventoryDate,
                    OpeningInventory = inventory.OpeningInventory,
                    Receipts = inventory.Receipts,
                    Deliveries = inventory.Deliveries,
                    ClosingInventory = inventory.ClosingInventory
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tank inventory {InventoryId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create tank inventory.
        /// </summary>
        [HttpPost("inventory")]
        public ActionResult<object> CreateTankInventory(
            [FromBody] CreateTankInventoryRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var inventory = _service.ProductionManager.CreateTankInventory(
                    request.TankBatteryId,
                    request.InventoryDate ?? DateTime.UtcNow,
                    request.OpeningInventory,
                    request.Receipts,
                    request.Deliveries,
                    request.ActualClosingInventory);

                return Ok(new { InventoryId = inventory.InventoryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tank inventory");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}

