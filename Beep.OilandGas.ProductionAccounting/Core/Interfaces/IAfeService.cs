using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.ProductionOperations.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for AFE (Authorization for Expenditure) management.
    /// </summary>
    public interface IAfeService
    {
        Task<IReadOnlyList<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetFacts>> ReadWorkOrderBudgetFactsAsync(Beep.OilandGas.Models.Data.Field.FieldSelectionReview context,Beep.OilandGas.Models.Data.AccessControl.AssetDatabaseScope database,Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderFinancialQuery query,Func<Task> authority);
        Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostReview> ReviewWorkOrderMaterialCostAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionStatus budget,Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostQuery query,Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderMaterialCostSource material,Func<Task> authority);
        Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostRecovery> RecordWorkOrderMaterialCostAsync(Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostCommand command,Func<Task> authority,Func<Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostReview>> readCurrent,Func<Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderMaterialCostSource>> readOwnedSource);
        Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostRecovery> RecoverWorkOrderMaterialCostAsync(Beep.OilandGas.Models.Data.Field.FieldSelectionReview context,Beep.OilandGas.Models.Data.AccessControl.AssetDatabaseScope database,Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderMaterialCostReference reference,Func<Task> authority);
        Task<Beep.OilandGas.ProductionOperations.Models.CompanyWorkOrderActualCostReview> ReadWorkOrderActualCostsAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionStatus budget,Func<Task> authority);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostReview> ReviewWorkOrderLaborCostAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionStatus budget,Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostQuery query,Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborUsageFacts labor,Func<Task> authority);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostRecovery> RecordWorkOrderLaborCostAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostCommand command,Func<Task> authority,Func<Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostReview>> readCurrent);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostRecovery> RecoverWorkOrderLaborCostAsync(Beep.OilandGas.Models.Data.Field.FieldSelectionReview context,Beep.OilandGas.Models.Data.AccessControl.AssetDatabaseScope database,Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderLaborCostReference reference,Func<Task> authority);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetReview> ReviewWorkOrderBudgetAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetReview basis,Func<Task> authority);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetRecovery> CreateWorkOrderBudgetAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetCommand command,Func<Task> authority,Func<Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetReview>> readCurrent);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetRecovery> RecoverWorkOrderBudgetAsync(Beep.OilandGas.Models.Data.Field.FieldSelectionReview context,Beep.OilandGas.Models.Data.AccessControl.AssetDatabaseScope database,Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetReference reference,Func<Task> authority);
        Task<AFE> CreateAfeAsync(AFE afe, string userId, string connectionName = "PPDM39");
        Task<AFE_LINE_ITEM> AddLineItemAsync(AFE_LINE_ITEM lineItem, string userId, string connectionName = "PPDM39");
        Task<AFE> ApproveAfeAsync(string afeId, DateTime approvalDate, string userId, string connectionName = "PPDM39");
        Task<ACCOUNTING_COST> RecordCostAsync(string afeId, ACCOUNTING_COST cost, string userId, string connectionName = "PPDM39");
        Task<List<AFE_LINE_ITEM>> GetLineItemsAsync(string afeId, string connectionName = "PPDM39");
        Task<COST_VARIANCE_REPORT> GenerateBudgetVarianceReportAsync(string afeId, decimal varianceThreshold, string userId, string connectionName = "PPDM39");
        Task<List<COST_VARIANCE_REPORT>> GetVarianceReportsAsync(string? afeId, string? costCenterId, DateTime? startDate, DateTime? endDate, string connectionName = "PPDM39");
    }
}
