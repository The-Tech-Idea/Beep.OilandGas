using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    [ApiController]
    [Route("api/field/current/accounting")]
    public class AccountingController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IAccountingService _accountingService;
        private readonly ILogger<AccountingController> _logger;

        public AccountingController(
            IFieldOrchestrator fieldOrchestrator,
            IAccountingService accountingService,
            ILogger<AccountingController> logger)
        {
            _fieldOrchestrator  = fieldOrchestrator  ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _accountingService  = accountingService  ?? throw new ArgumentNullException(nameof(accountingService));
            _logger             = logger;
        }

        /// <summary>GET /api/field/current/accounting/recent-activities</summary>
        /// Returns the most recent accounting activities (receivables) mapped to a display DTO.
        [HttpGet("recent-activities")]
        public async Task<ActionResult<List<AccountingActivityDto>>> GetRecentActivitiesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });

            try
            {
                var receivables = await _accountingService.GetAllReceivablesAsync();
                var activities = (receivables ?? new())
                    .Select(r => new AccountingActivityDto
                    {
                        ActivityType = "RECEIVABLE",
                        Date         = r.INVOICE_DATE?.ToString("yyyy-MM-dd") ?? string.Empty,
                        Description  = $"Invoice {r.INVOICE_NUMBER} — Customer: {r.CUSTOMER} — Amount: {r.ORIGINAL_AMOUNT:N2}",
                        Amount       = r.ORIGINAL_AMOUNT,
                        Status       = r.STATUS ?? "UNKNOWN",
                        ReferenceId  = r.RECEIVABLE_ID ?? string.Empty,
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
        public async Task<ActionResult<ProductionAccountingSummaryDto>> GetProductionSummaryAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var receivables = await _accountingService.GetAllReceivablesAsync() ?? new();
                var transactions = await _accountingService.GetSalesTransactionsByDateRangeAsync(
                    DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow) ?? new();

                var grossRevenue = transactions.Sum(t => t.TOTAL_AMOUNT);
                var outstanding  = receivables
                    .Where(r => r.STATUS != "PAID")
                    .Sum(r => r.ORIGINAL_AMOUNT);

                return Ok(new ProductionAccountingSummaryDto
                {
                    GrossRevenue   = grossRevenue,
                    TotalOpex      = 0m,
                    NetRevenue     = grossRevenue,
                    OpenInvoices   = receivables.Count(r => r.STATUS != "PAID"),
                    OutstandingUsd = outstanding,
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
        public async Task<ActionResult<List<RevenueLine>>> GetRevenueLinesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var transactions = await _accountingService.GetSalesTransactionsByDateRangeAsync(
                    DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow) ?? new();

                var lines = transactions.Select(t => new RevenueLine
                {
                    Description = $"Sale {t.SALES_TRANSACTION_ID}",
                    Product     = "OIL",
                    Volume      = t.NET_VOLUME,
                    Unit        = "Bbl",
                    Price       = t.PRICE_PER_BARREL,
                    AmountUsd   = t.TOTAL_AMOUNT,
                    Type        = "REVENUE",
                }).ToList();

                return Ok(lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching revenue lines for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────
    public class AccountingActivityDto
    {
        public string  ActivityType { get; set; } = string.Empty;
        public string  Date         { get; set; } = string.Empty;
        public string  Description  { get; set; } = string.Empty;
        public decimal Amount       { get; set; }
        public string  Status       { get; set; } = string.Empty;
        public string  ReferenceId  { get; set; } = string.Empty;
    }

    public class ProductionAccountingSummaryDto
    {
        public decimal GrossRevenue   { get; set; }
        public decimal TotalOpex      { get; set; }
        public decimal NetRevenue     { get; set; }
        public int     OpenInvoices   { get; set; }
        public decimal OutstandingUsd { get; set; }
    }

    public class RevenueLine
    {
        public string  Description { get; set; } = string.Empty;
        public string  Product     { get; set; } = string.Empty;
        public decimal Volume      { get; set; }
        public string  Unit        { get; set; } = string.Empty;
        public decimal Price       { get; set; }
        public decimal AmountUsd   { get; set; }
        public string  Type        { get; set; } = string.Empty;
    }
}
