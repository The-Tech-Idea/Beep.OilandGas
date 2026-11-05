using Beep.OilandGas.ProductionOperations.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WorkOrder;

namespace Beep.OilandGas.Web.Services
{
    public interface IWorkOrderServiceClient
    {
        Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderFinancialSources> ReadFinancialSourcesAsync(Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderFinancialQuery query,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostReview> ReviewMaterialCostAsync(Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostQuery query,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostRecovery> RecordMaterialCostAsync(Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostCommand command,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostRecovery> RecoverMaterialCostAsync(Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostReference reference,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderActualCostReview> ReadActualCostsAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionQuery query,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostReview> ReviewLaborCostAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostQuery query,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostRecovery> RecordLaborCostAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostCommand command,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostRecovery> RecoverLaborCostAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostReference reference,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionStatus> ReadBudgetDecisionStatusAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionQuery query,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionReview> ReviewBudgetDecisionAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionQuery query,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionRecovery> RecordBudgetDecisionAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionCommand command,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionRecovery> RecoverBudgetDecisionAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionReference reference,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetReview> ReviewBudgetAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetQuery query,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetRecovery> CreateBudgetAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetCommand command,CancellationToken cancellationToken=default);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetRecovery> RecoverBudgetAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetReference reference,CancellationToken cancellationToken=default);
        Task<List<WorkOrderSummary>> GetWorkOrdersAsync(string? state = null, string? woSubType = null);
        Task<List<WorkOrderSummary>> GetWorkOrderSampleAsync(CancellationToken cancellationToken = default);
        Task<WorkOrderSummary?> CreateWorkOrderAsync(CreateWorkOrderRequest request);
        Task<WorkOrderDetailModel?> GetWorkOrderAsync(string instanceId);
        Task<Beep.OilandGas.ProductionOperations.Models.WorkOrderProcessReview> GetProcessesAsync(string instanceId);
        Task<List<string>> GetTransitionsAsync(string instanceId);
        Task<WorkOrderSummary?> TransitionAsync(string instanceId, TransitionWorkOrderRequest request);
        Task<InspectionCondition> AddInspectionRequirementAsync(string instanceId,AddWorkOrderInspectionRequirementRequest request);
        Task<List<InspectionCondition>> GetChecklistAsync(string instanceId);
        Task<InspectionCondition> RecordConditionAsync(string instanceId, int condSeq, RecordInspectionResultRequest request);
        Task<List<ContractorAssignment>> GetContractorsAsync(string instanceId);
        Task<List<CalendarSlot>> GetCalendarAsync(DateTime from, DateTime to);
    }
}
