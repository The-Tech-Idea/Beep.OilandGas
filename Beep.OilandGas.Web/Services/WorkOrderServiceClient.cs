using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WorkOrder;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    public class WorkOrderServiceClient : IWorkOrderServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<WorkOrderServiceClient> _logger;

        public WorkOrderServiceClient(
            ApiClient apiClient,
            ILogger<WorkOrderServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<WorkOrderSummary>> GetWorkOrdersAsync(string? state = null, string? woSubType = null)
        {
            try
            {
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(state))
                    query.Add($"state={Uri.EscapeDataString(state)}");
                if (!string.IsNullOrWhiteSpace(woSubType))
                    query.Add($"woSubType={Uri.EscapeDataString(woSubType)}");

                var endpoint = "/api/field/current/workorder";
                if (query.Count > 0)
                    endpoint += "?" + string.Join("&", query);

                var result = await _apiClient.GetAsync<List<WorkOrderSummary>>(endpoint);
                return result ?? new List<WorkOrderSummary>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work orders for current field");
                return new List<WorkOrderSummary>();
            }
        }

        public async Task<WorkOrderSummary?> CreateWorkOrderAsync(CreateWorkOrderRequest request)
        {
            try
            {
                return await _apiClient.PostAsync<CreateWorkOrderRequest, WorkOrderSummary>(
                    "/api/field/current/workorder", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work order {InstanceName}", request.InstanceName);
                return null;
            }
        }

        public async Task<WorkOrderDetailModel?> GetWorkOrderAsync(string instanceId)
        {
            try
            {
                return await _apiClient.GetAsync<WorkOrderDetailModel>(
                    $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work order {InstanceId}", instanceId);
                return null;
            }
        }

        public async Task<List<string>> GetTransitionsAsync(string instanceId)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<string>>(
                    $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/transitions");
                return result ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transitions for work order {InstanceId}", instanceId);
                return new List<string>();
            }
        }

        public async Task<WorkOrderSummary?> TransitionAsync(string instanceId, TransitionWorkOrderRequest request)
        {
            try
            {
                return await _apiClient.PostAsync<TransitionWorkOrderRequest, WorkOrderSummary>(
                    $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/transition", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transitioning work order {InstanceId} to {ToState}", instanceId, request.ToState);
                return null;
            }
        }

        public async Task<List<InspectionCondition>> GetChecklistAsync(string instanceId)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<InspectionCondition>>(
                    $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/checklist");
                return result ?? new List<InspectionCondition>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checklist for work order {InstanceId}", instanceId);
                return new List<InspectionCondition>();
            }
        }

        public async Task<bool> RecordConditionAsync(string instanceId, int condSeq, RecordInspectionResultRequest request)
        {
            try
            {
                return await _apiClient.PostAsync(
                    $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/checklist/{condSeq}",
                    request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording checklist result for work order {InstanceId} condition {ConditionSeq}", instanceId, condSeq);
                return false;
            }
        }

        public async Task<List<CostVarianceLine>> GetCostsAsync(string instanceId)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<CostVarianceLine>>(
                    $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/costs");
                return result ?? new List<CostVarianceLine>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting costs for work order {InstanceId}", instanceId);
                return new List<CostVarianceLine>();
            }
        }

        public async Task<List<ContractorAssignment>> GetContractorsAsync(string instanceId)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<ContractorAssignment>>(
                    $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/contractors");
                return result ?? new List<ContractorAssignment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contractors for work order {InstanceId}", instanceId);
                return new List<ContractorAssignment>();
            }
        }

        public async Task<List<CalendarSlot>> GetCalendarAsync(DateTime from, DateTime to)
        {
            try
            {
                var endpoint =
                    $"/api/field/current/workorder/calendar?from={Uri.EscapeDataString(from.ToString("o"))}&to={Uri.EscapeDataString(to.ToString("o"))}";
                var result = await _apiClient.GetAsync<List<CalendarSlot>>(endpoint);
                return result ?? new List<CalendarSlot>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work order calendar for current field");
                return new List<CalendarSlot>();
            }
        }

        public async Task<AFEResponse> GetAfeAsync(string instanceId)
        {
            try
            {
                return await _apiClient.GetAsync<AFEResponse>(
                    $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/afe") ?? new AFEResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting linked AFE for work order {InstanceId}", instanceId);
                return new AFEResponse();
            }
        }

        public async Task<AFEResponse> CreateOrLinkAfeAsync(string instanceId)
        {
            try
            {
                return await _apiClient.PostAsync<AFEResponse>(
                    $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/afe",
                    (System.Net.Http.HttpContent?)null) ?? new AFEResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or linking AFE for work order {InstanceId}", instanceId);
                return new AFEResponse();
            }
        }
    }
}