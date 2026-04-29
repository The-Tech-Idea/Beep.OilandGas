using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Storage;
using Beep.OilandGas.Models.Data.Inventory;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Storage
{
    /// <summary>
    /// API controller for Storage operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/storage")]
    public class StorageController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<StorageController> _logger;

        public StorageController(
            ProductionAccountingService service,
            IInventoryService inventoryService,
            ILogger<StorageController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _logger = logger;
        }

        /// <summary>
        /// Get storage facilities.
        /// </summary>
        [HttpGet("facilities")]
        public ActionResult<object> GetStorageFacilities(
            [FromQuery] string? facilityId = null,
            [FromQuery] string? leaseId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(facilityId))
                {
                    var facility = _service.StorageManager.GetFacility(facilityId);
                    if (facility == null)
                            return NotFound(new { error = $"Storage facility {facilityId} not found." });
                    return Ok(new { FacilityId = facility.FacilityId, FacilityName = facility.FacilityName, Location = facility.Location });
                }
                
                if (!string.IsNullOrEmpty(leaseId))
                {
                    var tankBatteries = _service.StorageManager.GetTankBatteriesByLease(leaseId).ToList();
                    return Ok(tankBatteries.Select(tb => new { BatteryId = tb.BatteryId, BatteryName = tb.BatteryName, LeaseId = tb.LeaseId }));
                }
                
                    return BadRequest(new { error = "Either facility ID or lease ID parameter is required." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage facilities");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Create storage facility.
        /// </summary>
        [HttpPost("facilities")]
        public ActionResult<object> CreateStorageFacility(
            [FromBody] CreateStorageFacilityRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var facility = new StorageFacility
                {
                    FacilityId = request.FacilityId ?? Guid.NewGuid().ToString(),
                    FacilityName = request.FacilityName,
                    Location = request.Location
                };

                _service.StorageManager.RegisterFacility(facility);
                return Ok(new { FacilityId = facility.FacilityId, FacilityName = facility.FacilityName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating storage facility");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed tank inventory update for storage workflows.</summary>
        [HttpPost("service/tanks/{tankId}/update")]
        public async Task<ActionResult<TANK_INVENTORY>> UpdateTankInventoryAsync(
            string tankId,
            [FromBody] StorageTankInventoryUpdateRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (string.IsNullOrWhiteSpace(tankId))
                    return BadRequest(new { error = "Tank ID is required." });

                var result = await _inventoryService.UpdateInventoryAsync(
                    tankId,
                    request.VolumeDelta,
                    ResolveUserId(),
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating storage tank inventory for {TankId}", tankId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed tank inventory lookup for storage workflows.</summary>
        [HttpGet("service/tanks/{tankId}")]
        public async Task<ActionResult<TANK_INVENTORY>> GetTankInventoryAsync(
            string tankId,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tankId))
                    return BadRequest(new { error = "Tank ID is required." });

                var result = await _inventoryService.GetInventoryAsync(
                    tankId,
                    connectionName ?? _service.DefaultConnectionName);

                if (result == null)
                    return NotFound(new { error = $"Tank inventory {tankId} not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving storage tank inventory for {TankId}", tankId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed storage valuation endpoint.</summary>
        [HttpPost("service/inventory/{inventoryItemId}/valuation")]
        public async Task<ActionResult<INVENTORY_VALUATION>> CalculateValuationAsync(
            string inventoryItemId,
            [FromBody] StorageInventoryValuationRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (string.IsNullOrWhiteSpace(inventoryItemId))
                    return BadRequest(new { error = "Inventory item ID is required." });

                var result = await _inventoryService.CalculateValuationAsync(
                    inventoryItemId,
                    request.ValuationDate,
                    request.Method,
                    ResolveUserId(),
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating storage valuation for {InventoryItemId}", inventoryItemId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed storage reconciliation summary endpoint.</summary>
        [HttpPost("service/inventory/{inventoryItemId}/reconciliation-report")]
        public async Task<ActionResult<INVENTORY_REPORT_SUMMARY>> GenerateReconciliationReportAsync(
            string inventoryItemId,
            [FromBody] StorageReconciliationReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (string.IsNullOrWhiteSpace(inventoryItemId))
                    return BadRequest(new { error = "Inventory item ID is required." });

                var result = await _inventoryService.GenerateReconciliationReportAsync(
                    inventoryItemId,
                    request.PeriodStart,
                    request.PeriodEnd,
                    ResolveUserId(),
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating storage reconciliation report for {InventoryItemId}", inventoryItemId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string ResolveUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? "system";
        }
    }

}

