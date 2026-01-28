using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Ownership;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.Data.Accounting.Ownership;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Ownership
{
    /// <summary>
    /// API controller for Ownership operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/ownership")]
    public class OwnershipController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly ILogger<OwnershipController> _logger;

        public OwnershipController(
            ProductionAccountingService service,
            ILogger<OwnershipController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Get ownership interests.
        /// </summary>
        [HttpGet("interests")]
        public ActionResult<List<OWNERSHIP_INTEREST>> GetOwnershipInterests(
            [FromQuery] string? propertyOrLeaseId = null,
            [FromQuery] DateTime? asOfDate = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(propertyOrLeaseId))
                    return BadRequest(new { error = "Property or lease ID is required" });

                var date = asOfDate ?? DateTime.Now;
                var interests = _service.OwnershipManager.GetOwnershipInterests(propertyOrLeaseId, date).ToList();
                var dtos = interests.Select(MapToOwnershipInterestDto).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ownership interests");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Register ownership interest.
        /// </summary>
        [HttpPost("interests")]
        public ActionResult<OWNERSHIP_INTEREST> RegisterOwnershipInterest(
            [FromBody] RegisterOwnershipInterestRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ownerInfo = new OWNER_INFORMATION
                {
                    OwnerId = Guid.NewGuid().ToString(),
                    OwnerName = request.OWNER.OWNER_NAME,
                    TaxId = request.OWNER.TAX_ID
                };

                var DIVISION_ORDER = _service.OwnershipManager.CreateDivisionOrder(
                    request.PROPERTY_OR_LEASE_ID,
                    ownerInfo,
                    request.WORKING_INTEREST,
                    request.NET_REVENUE_INTEREST,
                    request.EFFECTIVE_DATE);

                _service.OwnershipManager.ApproveDivisionOrder(DIVISION_ORDER.DIVISION_ORDER_ID, "system");

                var interests = _service.OwnershipManager.GetOwnershipInterests(request.PROPERTY_OR_LEASE_ID, request.EFFECTIVE_DATE);
                var interest = interests.FirstOrDefault(i => i.OWNER_ID == ownerInfo.OWNER_ID);
                if (interest == null)
                    return StatusCode(500, new { error = "Failed to create ownership interest" });

                return Ok(MapToOwnershipInterestDto(interest));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering ownership interest");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private OWNERSHIP_INTEREST MapToOwnershipInterestDto(OWNERSHIP_INTEREST interest)
        {
            return new OWNERSHIP_INTEREST
            {
                InterestId = interest.OWNERSHIP_ID,
                PropertyOrLeaseId = interest.PROPERTY_OR_LEASE_ID,
                OwnerId = interest.OWNER_ID,
                WorkingInterest = interest.WORKING_INTEREST,
                NetRevenueInterest = interest.NET_REVENUE_INTEREST,
                ROYALTY_INTEREST = interest.ROYALTY_INTEREST,
                EffectiveDate = interest.EFFECTIVE_START_DATE,
                ExpirationDate = interest.EFFECTIVE_END_DATE
            };
        }
    }

}

