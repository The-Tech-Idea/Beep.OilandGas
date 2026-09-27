using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.Inventory;
using Beep.OilandGas.ProductionAccounting.Services;
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
        private readonly TankInventoryStore _service;
        private readonly ILogger<ProductionController> _logger;

        public ProductionController(
            TankInventoryStore service,
            ILogger<ProductionController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Get tank inventory by ID.
        /// </summary>
        [HttpGet("inventory/{id}")]
        public async Task<ActionResult<object>> GetTankInventory(
            string id,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Inventory ID is required." });
            try
            {
                var inventory = await _service.GetAsync(id);
                if (inventory == null)
                        return NotFound(new { error = $"Tank inventory with ID {id} not found." });

                return Ok(new
                {
                    InventoryId = inventory.TANK_INVENTORY_ID,
                    TankBatteryId = inventory.TANK_BATTERY_ID,
                    InventoryDate = inventory.INVENTORY_DATE,
                    OpeningInventory = inventory.OPENING_INVENTORY,
                    Receipts = inventory.RECEIPTS,
                    Deliveries = inventory.DELIVERIES,
                    ClosingInventory = inventory.ACTUAL_CLOSING_INVENTORY
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tank inventory {InventoryId}", id);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Create tank inventory.
        /// </summary>
        [HttpPost("inventory")]
        public async Task<ActionResult<object>> CreateTankInventory(
            [FromBody] CreateTankInventoryRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var actor = User.ActingUserId();
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var inventory = await _service.CreateAsync(request, actor);

                return Ok(new { InventoryId = inventory.TANK_INVENTORY_ID });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tank inventory");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }

}
