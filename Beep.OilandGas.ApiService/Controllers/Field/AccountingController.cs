using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    [ApiController]
    [Route("api/field/current/accounting")]
    public class AccountingController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IAccountingService _accountingService;
        private readonly ProductionAccountingService _productionAccountingService;
        private readonly ILogger<AccountingController> _logger;

        public AccountingController(
            IFieldOrchestrator fieldOrchestrator,
            IAccountingService accountingService,
            ProductionAccountingService productionAccountingService,
            ILogger<AccountingController> logger)
        {
            _fieldOrchestrator  = fieldOrchestrator  ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _accountingService  = accountingService  ?? throw new ArgumentNullException(nameof(accountingService));
            _productionAccountingService = productionAccountingService ?? throw new ArgumentNullException(nameof(productionAccountingService));
            _logger             = logger;
        }

        /// <summary>GET /api/field/current/accounting/recent-activities</summary>
        /// Returns the most recent accounting activities (receivables) mapped to a display DTO.
        [HttpGet("recent-activities")]
        public async Task<ActionResult<List<AccountingActivityDto>>> GetRecentActivitiesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            try
            {
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddMonths(-3);
                var revenueTransactions = await _productionAccountingService.GetRevenueTransactionsAsync(fieldId, startDate, endDate);
                var activities = revenueTransactions
                    .Select(t => new AccountingActivityDto
                    {
                        ActivityType = string.IsNullOrWhiteSpace(t.REVENUE_TYPE) ? "REVENUE" : t.REVENUE_TYPE,
                        Date         = t.TRANSACTION_DATE?.ToString("yyyy-MM-dd") ?? string.Empty,
                        Description  = string.IsNullOrWhiteSpace(t.DESCRIPTION) ? $"Revenue {t.REVENUE_TRANSACTION_ID}" : t.DESCRIPTION,
                        Amount       = t.NET_REVENUE ?? t.GROSS_REVENUE ?? 0m,
                        Status       = "ACTIVE",
                        ReferenceId  = t.REVENUE_TRANSACTION_ID,
                    })
                    .OrderByDescending(a => a.Date)
                    .Take(100)
                    .ToList();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent accounting activities for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/accounting/production-summary</summary>
        [HttpGet("production-summary")]
        public async Task<ActionResult<ProductionAccountingSummaryDto>> GetProductionSummaryAsync([FromQuery] DateTime? periodEnd = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            try
            {
                var effectivePeriodEnd = periodEnd?.Date ?? DateTime.UtcNow.Date;
                var accountingStatus = await _productionAccountingService.GetAccountingStatusAsync(fieldId, effectivePeriodEnd);

                return Ok(new ProductionAccountingSummaryDto
                {
                    GrossRevenue   = accountingStatus.TotalRevenue,
                    TotalOpex      = accountingStatus.TotalCosts,
                    NetRevenue     = accountingStatus.NetIncome,
                    OpenInvoices   = 0,
                    OutstandingUsd = 0m,
                    PeriodStatus   = accountingStatus.PeriodStatus,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production summary for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/accounting/revenue-lines</summary>
        [HttpGet("revenue-lines")]
        public async Task<ActionResult<List<RevenueLine>>> GetRevenueLinesAsync([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            try
            {
                var effectiveEndDate = endDate?.Date ?? DateTime.UtcNow.Date;
                var effectiveStartDate = startDate?.Date ?? new DateTime(effectiveEndDate.Year, effectiveEndDate.Month, 1);
                var transactions = await _productionAccountingService.GetRevenueTransactionsAsync(
                    fieldId,
                    effectiveStartDate,
                    effectiveEndDate);

                var lines = transactions.Select(t => new RevenueLine
                {
                    Description = string.IsNullOrWhiteSpace(t.DESCRIPTION) ? $"Revenue {t.REVENUE_TRANSACTION_ID}" : t.DESCRIPTION,
                    Product     = (t.GAS_VOLUME ?? 0m) > 0m && (t.OIL_VOLUME ?? 0m) <= 0m ? "GAS" : "OIL",
                    Volume      = (t.GAS_VOLUME ?? 0m) > 0m && (t.OIL_VOLUME ?? 0m) <= 0m ? (t.GAS_VOLUME ?? 0m) : (t.OIL_VOLUME ?? 0m),
                    Unit        = (t.GAS_VOLUME ?? 0m) > 0m && (t.OIL_VOLUME ?? 0m) <= 0m ? "Mcf" : "Bbl",
                    Price       = (t.GAS_VOLUME ?? 0m) > 0m && (t.OIL_VOLUME ?? 0m) <= 0m ? (t.GAS_PRICE ?? 0m) : (t.OIL_PRICE ?? 0m),
                    AmountUsd   = t.NET_REVENUE ?? t.GROSS_REVENUE ?? 0m,
                    Type        = string.IsNullOrWhiteSpace(t.REVENUE_TYPE) ? "REVENUE" : t.REVENUE_TYPE,
                }).ToList();

                return Ok(lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching revenue lines for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>POST /api/field/current/accounting/close-period</summary>
        [HttpPost("close-period")]
        public async Task<IActionResult> ClosePeriodAsync([FromBody] CloseAccountingPeriodRequest request)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            if (request == null || request.PeriodEnd == default)
                return BadRequest(new { error = "A valid period end date is required." });

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                var succeeded = await _productionAccountingService.ClosePeriodAsync(fieldId, request.PeriodEnd.Date, userId);
                if (!succeeded)
                    return StatusCode(500, new { error = "Period close failed." });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing accounting period for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }

}
