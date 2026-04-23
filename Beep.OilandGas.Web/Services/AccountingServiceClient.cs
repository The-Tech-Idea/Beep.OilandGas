using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Accounting.Cost;
using Beep.OilandGas.Models.Data.Accounting.Royalty;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed class AccountingServiceClient : IAccountingServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<AccountingServiceClient> _logger;

    public AccountingServiceClient(ApiClient apiClient, ILogger<AccountingServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<AccountingActivityDto>> GetRecentActivitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<AccountingActivityDto>>(
                "/api/field/current/accounting/recent-activities",
                cancellationToken);
            return result ?? new List<AccountingActivityDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent accounting activities.");
            throw;
        }
    }

    public async Task<ProductionAccountingSummaryDto?> GetProductionSummaryAsync(DateTime? periodEnd = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/accounting/production-summary";
            if (periodEnd.HasValue)
                endpoint += $"?periodEnd={Uri.EscapeDataString(periodEnd.Value.ToString("yyyy-MM-dd"))}";

            return await _apiClient.GetAsync<ProductionAccountingSummaryDto>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production accounting summary for period end {PeriodEnd}.", periodEnd);
            throw;
        }
    }

    public async Task<List<RevenueLine>> GetRevenueLinesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new List<string>();
            if (startDate.HasValue)
                query.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("yyyy-MM-dd"))}");
            if (endDate.HasValue)
                query.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("yyyy-MM-dd"))}");

            var endpoint = "/api/field/current/accounting/revenue-lines";
            if (query.Count > 0)
                endpoint += "?" + string.Join("&", query);

            var result = await _apiClient.GetAsync<List<RevenueLine>>(endpoint, cancellationToken);
            return result ?? new List<RevenueLine>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue lines from {StartDate} to {EndDate}.", startDate, endDate);
            throw;
        }
    }

    public async Task<VolumeReconciliationResult?> ReconcileVolumesAsync(VolumeReconciliationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<VolumeReconciliationRequest, VolumeReconciliationResult>(
                "/api/accounting/allocation/reconcile",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling accounting volumes for field {FieldId}.", request.FieldId);
            throw;
        }
    }

    public async Task<CostAllocationComputationResult?> AllocateCostsAsync(CostAllocationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<CostAllocationRequest, CostAllocationComputationResult>(
                "/api/accounting/cost/allocations/allocate",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error allocating accounting costs for field {FieldId}.", request.FieldId);
            throw;
        }
    }

    public async Task<List<ROYALTY_CALCULATION>> GetRoyaltyCalculationsAsync(string? fieldId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(fieldId))
                query.Add($"fieldId={Uri.EscapeDataString(fieldId)}");
            if (startDate.HasValue)
                query.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("yyyy-MM-dd"))}");
            if (endDate.HasValue)
                query.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("yyyy-MM-dd"))}");

            var endpoint = "/api/accounting/royalty/calculations";
            if (query.Count > 0)
                endpoint += "?" + string.Join("&", query);

            var result = await _apiClient.GetAsync<List<ROYALTY_CALCULATION>>(endpoint, cancellationToken);
            return result ?? new List<ROYALTY_CALCULATION>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting royalty calculations for field {FieldId} from {StartDate} to {EndDate}.", fieldId, startDate, endDate);
            throw;
        }
    }

    public async Task<bool> CalculateRoyaltiesAsync(CalculateRoyaltyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync(
                "/api/accounting/royalty/calculate",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating royalties for field {FieldId}.", request.FieldId);
            throw;
        }
    }

    public async Task<bool> ClosePeriodAsync(CloseAccountingPeriodRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync(
                "/api/field/current/accounting/close-period",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing accounting period ending {PeriodEnd}.", request.PeriodEnd);
            throw;
        }
    }
}