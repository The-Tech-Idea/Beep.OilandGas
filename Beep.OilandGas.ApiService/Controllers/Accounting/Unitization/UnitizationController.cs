using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.Unitization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

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
        public async Task<ActionResult<List<object>>> GetUnits([FromQuery] string? connectionName = null)
        {
            try
            {
                var repository = _service.GetRepository(typeof(UNIT_AGREEMENT), connectionName, "UNIT_AGREEMENT");
                var units = (await repository.GetAsync(new List<AppFilter>())).OfType<UNIT_AGREEMENT>().ToList();
                var dtos = units.Select(u => new 
                { 
                    UnitId = u.UNIT_ID,
                    UnitName = u.UNIT_NAME,
                    EffectiveDate = u.EFFECTIVE_DATE,
                    ExpirationDate = u.EXPIRATION_DATE,
                    UnitOperator = u.UNIT_OPERATOR
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
        public async Task<ActionResult<object>> CreateUnitAgreement(
            [FromBody] CreateUnitAgreementRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var repository = _service.GetRepository(typeof(UNIT_AGREEMENT), connectionName, "UNIT_AGREEMENT");
                var unit = new UNIT_AGREEMENT
                {
                    UNIT_ID = Guid.NewGuid().ToString(),
                    UNIT_NAME = request.UnitName,
                    EFFECTIVE_DATE = request.EffectiveDate,
                    EXPIRATION_DATE = request.ExpiryDate,
                    UNIT_OPERATOR = request.UnitOperator,
                    TERMS_AND_CONDITIONS = request.TermsAndConditions
                };

                await repository.InsertAsync(unit, "system");

                return Ok(new { UnitId = unit.UNIT_ID, UnitName = unit.UNIT_NAME });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating unit agreement");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}

