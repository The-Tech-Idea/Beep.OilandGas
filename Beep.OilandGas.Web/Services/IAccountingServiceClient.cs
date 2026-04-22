using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Accounting;

namespace Beep.OilandGas.Web.Services;

public interface IAccountingServiceClient
{
    Task<List<AccountingActivityDto>> GetRecentActivitiesAsync(CancellationToken cancellationToken = default);
    Task<ProductionAccountingSummaryDto?> GetProductionSummaryAsync(DateTime? periodEnd = null, CancellationToken cancellationToken = default);
    Task<List<RevenueLine>> GetRevenueLinesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<bool> ClosePeriodAsync(CloseAccountingPeriodRequest request, CancellationToken cancellationToken = default);
}