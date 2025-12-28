using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Unitization;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Unitization
{
    /// <summary>
    /// API controller for Unitization operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/unitization")]
    public class UnitizationController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly ILogger<UnitizationController> _logger;

        public UnitizationController(
            ProductionAccountingService service,
            ILogger<UnitizationController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Get unit agreements.
        /// </summary>
        [HttpGet("units")]
        public ActionResult<List<object>> GetUnits([FromQuery] string? connectionName = null)
        {
            try
            {
                var unitManager = new UnitManager();
                var units = unitManager.GetAllUnitAgreements().ToList();
                var dtos = units.Select(u => new 
                { 
                    UnitId = u.UnitId, 
                    UnitName = u.UnitName, 
                    EffectiveDate = u.EffectiveDate,
                    ExpirationDate = u.ExpirationDate,
                    UnitOperator = u.UnitOperator
                }).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting units");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create unit agreement.
        /// </summary>
        [HttpPost("units")]
        public ActionResult<object> CreateUnitAgreement(
            [FromBody] CreateUnitAgreementRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var unitManager = new UnitManager();
                var unit = unitManager.CreateUnitAgreement(
                    request.UnitName,
                    request.EffectiveDate,
                    request.UnitOperator);

                return Ok(new { UnitId = unit.UnitId, UnitName = unit.UnitName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating unit agreement");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class CreateUnitAgreementRequest
    {
        public string UnitName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public string UnitOperator { get; set; } = string.Empty;
    }
}

