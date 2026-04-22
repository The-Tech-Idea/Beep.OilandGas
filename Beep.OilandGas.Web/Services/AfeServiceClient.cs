using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed class AfeServiceClient : IAfeServiceClient
{
    private const string BaseEndpoint = "/api/accounting/cost/afe";

    private readonly ApiClient _apiClient;
    private readonly ILogger<AfeServiceClient> _logger;

    public AfeServiceClient(ApiClient apiClient, ILogger<AfeServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<AfeSummaryModel>> GetAfesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<AfeSummaryModel>>(BaseEndpoint, cancellationToken);
            return result ?? new List<AfeSummaryModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AFEs.");
            throw;
        }
    }

    public async Task<CreateAfeResponse?> CreateAfeAsync(CreateAFERequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<CreateAFERequest, CreateAfeResponse>(
                BaseEndpoint,
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AFE {AfeNumber}.", request.AfeNumber);
            throw;
        }
    }

    public async Task ApproveAfeAsync(string afeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(afeId))
            throw new ArgumentException("AFE ID is required.", nameof(afeId));

        try
        {
            var succeeded = await _apiClient.PatchAsync(
                $"{BaseEndpoint}/{Uri.EscapeDataString(afeId)}/approve",
                new { },
                cancellationToken);

            if (!succeeded)
                throw new InvalidOperationException($"Failed to approve AFE {afeId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving AFE {AfeId}.", afeId);
            throw;
        }
    }

    public async Task RejectAfeAsync(string afeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(afeId))
            throw new ArgumentException("AFE ID is required.", nameof(afeId));

        try
        {
            var succeeded = await _apiClient.PatchAsync(
                $"{BaseEndpoint}/{Uri.EscapeDataString(afeId)}/reject",
                new { },
                cancellationToken);

            if (!succeeded)
                throw new InvalidOperationException($"Failed to reject AFE {afeId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting AFE {AfeId}.", afeId);
            throw;
        }
    }
}