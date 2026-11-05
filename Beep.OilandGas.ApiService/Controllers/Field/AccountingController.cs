using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Accounting.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.Repository;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    [ApiController]
    [Route("api/field/current/accounting")]
    [RequireCurrentFieldAccess]
    public class AccountingController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IAccountingService _accountingService;
        private readonly IProductionAccountingService _productionAccountingService;
        private readonly ILogger<AccountingController> _logger;

        public AccountingController(
            IFieldOrchestrator fieldOrchestrator,
            IAccountingService accountingService,
            IProductionAccountingService productionAccountingService,
            ILogger<AccountingController> logger)
        {
            _fieldOrchestrator  = fieldOrchestrator  ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _accountingService  = accountingService  ?? throw new ArgumentNullException(nameof(accountingService));
            _productionAccountingService = productionAccountingService ?? throw new ArgumentNullException(nameof(productionAccountingService));
            _logger             = logger;
        }

        [Authorize(Roles="Accountant,Administrator")]
        [HttpGet("period-checks")]
        public async Task<ActionResult<AccountingPeriodCheckReview>> GetPeriodChecksAsync([FromQuery] DateTime periodEnd,[FromQuery] string bookId,
            [FromServices] Beep.OilandGas.Accounting.Services.PeriodClosingService periods,[FromServices] RepositoryTenantContext tenant,
            [FromServices] IRepositoryAccessService access,[FromServices] IAccessControlService resources,[FromServices] IFailureReporter failures,
            System.Threading.CancellationToken token)
        {
            if(periodEnd==default || periodEnd.TimeOfDay!=TimeSpan.Zero || string.IsNullOrWhiteSpace(bookId) || bookId.Length>255)return BadRequest();
            var actor=User.ActingUserId();var field=_fieldOrchestrator.CurrentFieldId;
            if(tenant.Company is not { } company || company.UserId!=actor || string.IsNullOrWhiteSpace(field))return Forbid();
            async Task<bool> CanReadAsync()
            {
                var current=await access.GetCompanyAccessAsync(actor,company.Scope,token);
                return AccountingPeriodRoleChecks.CanAct(User,company.Scope,current) &&
                    (await resources.CheckAssetAccessAsync(actor,field,"FIELD","READ")).HasAccess;
            }
            try
            {
                if(!await CanReadAsync())return Forbid();
                var checks=await periods.ValidatePeriodCloseAsync(periodEnd,bookId);
                if(_fieldOrchestrator.CurrentFieldId!=field || !await CanReadAsync())return Forbid();
                return Ok(AccountingPeriodCheckReviews.Create(company.Scope,actor,field,bookId,periodEnd,checks));
            }
            catch(InvalidOperationException changed){failures.ReportHandled(changed,"reading accounting period checks","period check evidence is unavailable",FailureSeverity.Degraded);return Conflict(new{error="Period checks could not be confirmed. Reload before continuing."});}
        }

        /// <summary>GET /api/field/current/accounting/recent-activities</summary>
        /// Returns the most recent accounting activities (receivables) mapped to a display DTO.
        [HttpGet("recent-activities")]
        public async Task<ActionResult<List<AccountingActivityDto>>> GetRecentActivitiesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

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

        /// <summary>GET /api/field/current/accounting/production-summary</summary>
        [HttpGet("production-summary")]
        public async Task<ActionResult<ProductionAccountingSummaryDto>> GetProductionSummaryAsync([FromQuery] DateTime? periodEnd = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
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

        /// <summary>GET /api/field/current/accounting/revenue-lines</summary>
        [HttpGet("revenue-lines")]
        public async Task<ActionResult<List<RevenueLine>>> GetRevenueLinesAsync([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
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

        /// <summary>POST /api/field/current/accounting/close-period</summary>
        [Authorize(Roles="Accountant,Administrator")]
        [RequireCurrentFieldAccess("WRITE")]
        [HttpPost("close-period")]
        public async Task<IActionResult> ClosePeriodAsync([FromBody] CloseAccountingPeriodRequest request,
            [FromServices] RepositoryTenantContext tenant,[FromServices] IRepositoryAccessService access,
            [FromServices] IAccessControlService resources,[FromServices] ModuleConnectionResolver modules)
        {
            var userId = User.ActingUserId();
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            if (request == null || request.PeriodEnd == default || request.PeriodEnd.TimeOfDay!=TimeSpan.Zero)
                return BadRequest(new { error = "A valid period end date is required." });

            if(tenant.Company is not { } company || company.UserId!=userId)return Forbid();
            var current=await access.GetCompanyAccessAsync(userId,company.Scope,HttpContext.RequestAborted);
            if(!AccountingPeriodRoleChecks.CanAct(User,company.Scope,current) ||
                !(await resources.CheckAssetAccessAsync(userId,fieldId,"FIELD","WRITE")).HasAccess)return Forbid();
            var connection=await modules.ResolveAsync("PRODUCTION",HttpContext.RequestAborted);
            current=await access.GetCompanyAccessAsync(userId,company.Scope,HttpContext.RequestAborted);
            if(_fieldOrchestrator.CurrentFieldId!=fieldId || !AccountingPeriodRoleChecks.CanAct(User,company.Scope,current) ||
                !(await resources.CheckAssetAccessAsync(userId,fieldId,"FIELD","WRITE")).HasAccess)return Forbid();
            var succeeded = await _productionAccountingService.ClosePeriodAsync(fieldId, request.PeriodEnd.Date, userId, connection);
            if (!succeeded)
                return StatusCode(500, new { error = "Period close failed." });

            return Ok();
        }
    }

}
