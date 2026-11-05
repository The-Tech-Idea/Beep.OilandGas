using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Accounting.Cost;
using Beep.OilandGas.Models.Data.Accounting.Royalty;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Reporting;

namespace Beep.OilandGas.Web.Services;

public interface IAccountingServiceClient
{
    Task<List<AccountingActivityDto>> GetRecentActivitiesAsync(CancellationToken cancellationToken = default);
    Task<ProductionAccountingSummaryDto?> GetProductionSummaryAsync(DateTime? periodEnd = null, CancellationToken cancellationToken = default);
    Task<List<RevenueLine>> GetRevenueLinesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<VolumeReconciliationResult?> ReconcileVolumesAsync(VolumeReconciliationRequest request, CancellationToken cancellationToken = default);
    Task<CostAllocationComputationResult?> AllocateCostsAsync(CostAllocationRequest request, CancellationToken cancellationToken = default);
    Task<List<ROYALTY_CALCULATION>> GetRoyaltyCalculationsAsync(string fieldId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<ROYALTY_CALCULATION>> PreviewRoyaltiesAsync(PreviewRoyaltiesRequest request, CancellationToken cancellationToken = default);
    Task<ROYALTY_CALCULATION> GetRoyaltyAsync(string calculationId, CancellationToken cancellationToken = default);
    Task<List<ROYALTY_PAYMENT>> GetRoyaltyPaymentsAsync(string calculationId, CancellationToken cancellationToken = default);
    Task<List<RoyaltyPostingReview>> GetRoyaltyPostingReviewAsync(string calculationId, CancellationToken cancellationToken = default);
    Task ClosePeriodAsync(CloseAccountingPeriodRequest request, CancellationToken cancellationToken = default);
    /// <summary>Whether the current field's period ending <paramref name="periodEnd"/> may be closed, and what stands in the way.</summary>
    Task<PeriodCloseReadinessResult> GetCloseReadinessAsync(DateTime periodEnd, CancellationToken cancellationToken = default);
    /// <summary>Generates a joint interest billing statement for a lease and answers the statement written.</summary>
    Task<JOINT_INTEREST_STATEMENT> GenerateJibStatementAsync(GenerateJIBStatementRequest request, CancellationToken cancellationToken = default);
}
