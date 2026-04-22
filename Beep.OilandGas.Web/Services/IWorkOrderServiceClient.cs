using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Web.Services
{
    public interface IWorkOrderServiceClient
    {
        Task<List<WorkOrderSummary>> GetWorkOrdersAsync(string? state = null, string? woSubType = null);
        Task<WorkOrderSummary?> CreateWorkOrderAsync(CreateWorkOrderRequest request);
        Task<WorkOrderDetailModel?> GetWorkOrderAsync(string instanceId);
        Task<List<string>> GetTransitionsAsync(string instanceId);
        Task<WorkOrderSummary?> TransitionAsync(string instanceId, TransitionWorkOrderRequest request);
        Task<List<InspectionCondition>> GetChecklistAsync(string instanceId);
        Task<bool> RecordConditionAsync(string instanceId, int condSeq, RecordInspectionResultRequest request);
        Task<List<CostVarianceLine>> GetCostsAsync(string instanceId);
        Task<List<ContractorAssignment>> GetContractorsAsync(string instanceId);
        Task<List<CalendarSlot>> GetCalendarAsync(DateTime from, DateTime to);
        Task<AFEResponse> GetAfeAsync(string instanceId);
        Task<AFEResponse> CreateOrLinkAfeAsync(string instanceId);
    }
}