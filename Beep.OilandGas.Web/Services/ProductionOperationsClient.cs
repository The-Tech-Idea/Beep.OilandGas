using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Web.Services;

public class ProductionOperationsClient : IProductionOperationsClient
{
    private readonly ApiClient _apiClient;

    public ProductionOperationsClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<WellProductionRecord>> GetWellProductionAsync(string wellUwi, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var url = $"/api/field/current/production/wells/{wellUwi}/production?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        return await _apiClient.GetAsync<List<WellProductionRecord>>(url, cancellationToken) ?? new();
    }

    public async Task<WellUptimeSummary> GetWellUptimeAsync(string wellUwi, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var url = $"/api/field/current/production/wells/{wellUwi}/uptime?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        return await _apiClient.GetAsync<WellUptimeSummary>(url, cancellationToken) ?? new WellUptimeSummary();
    }

    public async Task<List<EquipmentMaintenanceRecord>> GetEquipmentMaintenanceAsync(string equipmentId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var url = $"/api/field/current/production/equipment/{equipmentId}/maintenance?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        return await _apiClient.GetAsync<List<EquipmentMaintenanceRecord>>(url, cancellationToken) ?? new();
    }

    public async Task<List<FacilityProductionRecord>> GetFacilityProductionAsync(string facilityId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var url = $"/api/field/current/production/facilities/{facilityId}/production?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        return await _apiClient.GetAsync<List<FacilityProductionRecord>>(url, cancellationToken) ?? new();
    }
}
