using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Ownership;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.DTOs.Accounting.Ownership;
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
        public ActionResult<List<OwnershipInterestDto>> GetOwnershipInterests(
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
        public ActionResult<OwnershipInterestDto> RegisterOwnershipInterest(
            [FromBody] RegisterOwnershipInterestRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ownerInfo = new OwnerInformation
                {
                    OwnerId = Guid.NewGuid().ToString(),
                    OwnerName = request.Owner.OwnerName,
                    TaxId = request.Owner.TaxId
                };

                var divisionOrder = _service.OwnershipManager.CreateDivisionOrder(
                    request.PropertyOrLeaseId,
                    ownerInfo,
                    request.WorkingInterest,
                    request.NetRevenueInterest,
                    request.EffectiveDate);

                _service.OwnershipManager.ApproveDivisionOrder(divisionOrder.DivisionOrderId, "system");

                var interests = _service.OwnershipManager.GetOwnershipInterests(request.PropertyOrLeaseId, request.EffectiveDate);
                var interest = interests.FirstOrDefault(i => i.OwnerId == ownerInfo.OwnerId);
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

        private OwnershipInterestDto MapToOwnershipInterestDto(OwnershipInterest interest)
        {
            return new OwnershipInterestDto
            {
                InterestId = interest.OwnershipId,
                PropertyOrLeaseId = interest.PropertyOrLeaseId,
                OwnerId = interest.OwnerId,
                WorkingInterest = interest.WorkingInterest,
                NetRevenueInterest = interest.NetRevenueInterest,
                RoyaltyInterest = interest.RoyaltyInterest,
                EffectiveDate = interest.EffectiveStartDate,
                ExpirationDate = interest.EffectiveEndDate
            };
        }
    }

}

