using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed class WellLookupServiceClient : IWellLookupServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<WellLookupServiceClient> _logger;

    public WellLookupServiceClient(ApiClient apiClient, ILogger<WellLookupServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WELL?> GetWellAsync(string uwi, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uwi))
            throw new ArgumentException("UWI is required.", nameof(uwi));

        try
        {
            return await _apiClient.GetAsync<WELL>(
                $"/api/well/uwi/{Uri.EscapeDataString(uwi)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting well by UWI {Uwi}.", uwi);
            throw;
        }
    }
}