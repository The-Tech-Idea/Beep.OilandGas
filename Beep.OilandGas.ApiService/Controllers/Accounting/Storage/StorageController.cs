using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Storage;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.Data.Accounting.Storage;
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
        private readonly ILogger<StorageController> _logger;

        public StorageController(
            ProductionAccountingService service,
            ILogger<StorageController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
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
                        return NotFound(new { error = $"Storage facility {facilityId} not found" });
                    return Ok(new { FacilityId = facility.FacilityId, FacilityName = facility.FacilityName, Location = facility.Location });
                }
                
                if (!string.IsNullOrEmpty(leaseId))
                {
                    var tankBatteries = _service.StorageManager.GetTankBatteriesByLease(leaseId).ToList();
                    return Ok(tankBatteries.Select(tb => new { BatteryId = tb.BatteryId, BatteryName = tb.BatteryName, LeaseId = tb.LeaseId }));
                }
                
                return BadRequest(new { error = "Either facilityId or leaseId parameter is required" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage facilities");
                return StatusCode(500, new { error = ex.Message });
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
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}

