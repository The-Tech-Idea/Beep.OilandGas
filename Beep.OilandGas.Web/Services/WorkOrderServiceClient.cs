using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Web.Services
{
    public partial class WorkOrderServiceClient : IWorkOrderServiceClient
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
            return result ?? throw new InvalidOperationException("Recorded work orders were not returned.");
        }

        public async Task<List<WorkOrderSummary>> GetWorkOrderSampleAsync(CancellationToken cancellationToken = default)
        {
            var result = await _apiClient.GetAsync<List<WorkOrderSummary>>("/api/field/current/workorder/sample", cancellationToken);
            return result ?? throw new InvalidOperationException("The recorded work-order sample was not returned.");
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

        public async Task<TheTechIdea.Data.OilGas.WorkOrderProcessReview> GetProcessesAsync(string instanceId) =>
            await _apiClient.GetAsync<TheTechIdea.Data.OilGas.WorkOrderProcessReview>($"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/processes")
                ?? throw new InvalidOperationException("Recorded work-order process evidence was not returned.");

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
            return result ?? throw new InvalidOperationException("Recorded inspection evidence was not returned.");
        }

        public async Task<InspectionCondition> AddInspectionRequirementAsync(string instanceId,AddWorkOrderInspectionRequirementRequest request)
            => await _apiClient.PostAsync<AddWorkOrderInspectionRequirementRequest,InspectionCondition>($"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/checklist",request)
                ?? throw new InvalidOperationException("The recorded requirement outcome was not returned.");
        public async Task<InspectionCondition> RecordConditionAsync(string instanceId,int condSeq,RecordInspectionResultRequest request)
            => await _apiClient.PostAsync<RecordInspectionResultRequest,InspectionCondition>($"/api/field/current/workorder/{Uri.EscapeDataString(instanceId)}/checklist/{condSeq}",request)
                ?? throw new InvalidOperationException("The recorded result outcome was not returned.");

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

    }
}
