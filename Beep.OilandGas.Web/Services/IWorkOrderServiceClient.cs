using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Web.Services
{
    public interface IWorkOrderServiceClient
    {
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderFinancialSources> ReadFinancialSourcesAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderFinancialQuery query,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostReview> ReviewMaterialCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostQuery query,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostRecovery> RecordMaterialCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostCommand command,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostRecovery> RecoverMaterialCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostReference reference,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderActualCostReview> ReadActualCostsAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionQuery query,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostReview> ReviewLaborCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostQuery query,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostRecovery> RecordLaborCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostCommand command,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostRecovery> RecoverLaborCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostReference reference,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionStatus> ReadBudgetDecisionStatusAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionQuery query,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionReview> ReviewBudgetDecisionAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionQuery query,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionRecovery> RecordBudgetDecisionAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionCommand command,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionRecovery> RecoverBudgetDecisionAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionReference reference,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetReview> ReviewBudgetAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetQuery query,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetRecovery> CreateBudgetAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetCommand command,CancellationToken cancellationToken=default);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetRecovery> RecoverBudgetAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetReference reference,CancellationToken cancellationToken=default);
        Task<List<WorkOrderSummary>> GetWorkOrdersAsync(string? state = null, string? woSubType = null);
        Task<List<WorkOrderSummary>> GetWorkOrderSampleAsync(CancellationToken cancellationToken = default);
        Task<WorkOrderSummary?> CreateWorkOrderAsync(CreateWorkOrderRequest request);
        Task<WorkOrderDetailModel?> GetWorkOrderAsync(string instanceId);
        Task<TheTechIdea.Data.OilGas.WorkOrderProcessReview> GetProcessesAsync(string instanceId);
        Task<List<string>> GetTransitionsAsync(string instanceId);
        Task<WorkOrderSummary?> TransitionAsync(string instanceId, TransitionWorkOrderRequest request);
        Task<InspectionCondition> AddInspectionRequirementAsync(string instanceId,AddWorkOrderInspectionRequirementRequest request);
        Task<List<InspectionCondition>> GetChecklistAsync(string instanceId);
        Task<InspectionCondition> RecordConditionAsync(string instanceId, int condSeq, RecordInspectionResultRequest request);
        Task<List<ContractorAssignment>> GetContractorsAsync(string instanceId);
        Task<List<CalendarSlot>> GetCalendarAsync(DateTime from, DateTime to);
    }
}
