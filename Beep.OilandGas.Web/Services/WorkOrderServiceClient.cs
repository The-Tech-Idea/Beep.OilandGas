using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Web.Services
{
    public class WorkOrderServiceClient : IWorkOrderServiceClient
    {
        private readonly ApiClient _apiClient;

        public WorkOrderServiceClient(ApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<List<WorkOrderSummary>> GetWorkOrdersAsync(string? state = null, string? woSubType = null)
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

        public async Task<WorkOrderSummary?> CreateWorkOrderAsync(CreateWorkOrderRequest request)
        {
            return await _apiClient.PostAsync<CreateWorkOrderRequest, WorkOrderSummary>(
                "/api/field/current/workorder", request);
        }

        public async Task<WorkOrderDetailModel?> GetWorkOrderAsync(string instanceId)
        {
            return await _apiClient.GetAsync<WorkOrderDetailModel>(
                $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}");
        }

        public async Task<List<string>> GetTransitionsAsync(string instanceId)
        {
            var result = await _apiClient.GetAsync<List<string>>(
                $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/transitions");
            return result ?? new List<string>();
        }

        public async Task<WorkOrderSummary?> TransitionAsync(string instanceId, TransitionWorkOrderRequest request)
        {
            return await _apiClient.PostAsync<TransitionWorkOrderRequest, WorkOrderSummary>(
                $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/transition", request);
        }

        public async Task<List<InspectionCondition>> GetChecklistAsync(string instanceId)
        {
            var result = await _apiClient.GetAsync<List<InspectionCondition>>(
                $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/checklist");
            return result ?? new List<InspectionCondition>();
        }

        public async Task RecordConditionAsync(string instanceId, int condSeq, RecordInspectionResultRequest request)
        {
            await _apiClient.PostAsync(
                $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/checklist/{condSeq}",
                request);
        }

        public async Task<List<CostVarianceLine>> GetCostsAsync(string instanceId)
        {
            var result = await _apiClient.GetAsync<List<CostVarianceLine>>(
                $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/costs");
            return result ?? new List<CostVarianceLine>();
        }

        public async Task<List<ContractorAssignment>> GetContractorsAsync(string instanceId)
        {
            var result = await _apiClient.GetAsync<List<ContractorAssignment>>(
                $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/contractors");
            return result ?? new List<ContractorAssignment>();
        }

        public async Task<List<CalendarSlot>> GetCalendarAsync(DateTime from, DateTime to)
        {
            var endpoint =
                $"/api/field/current/workorder/calendar?from={Uri.EscapeDataString(from.ToString("o"))}&to={Uri.EscapeDataString(to.ToString("o"))}";
            var result = await _apiClient.GetAsync<List<CalendarSlot>>(endpoint);
            return result ?? new List<CalendarSlot>();
        }

        public async Task<AFEResponse> GetAfeAsync(string instanceId)
        {
            return await _apiClient.GetAsync<AFEResponse>(
                $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/afe") ?? new AFEResponse();
        }

        public async Task<AFEResponse> CreateOrLinkAfeAsync(string instanceId)
        {
            return await _apiClient.PostAsync<AFEResponse>(
                $"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/afe",
                (System.Net.Http.HttpContent?)null) ?? new AFEResponse();
        }
    }
}