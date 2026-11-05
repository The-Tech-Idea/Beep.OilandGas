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
        Task<IReadOnlyList<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetFacts>> ReadWorkOrderBudgetFactsAsync(TheTechIdea.Data.OilGas.FieldSelectionReview context,TheTechIdea.Data.OilGas.AssetDatabaseScope database,TheTechIdea.Data.OilGas.CompanyWorkOrderFinancialQuery query,Func<Task> authority);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostReview> ReviewWorkOrderMaterialCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionStatus budget,TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostQuery query,TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostSource material,Func<Task> authority);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostRecovery> RecordWorkOrderMaterialCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostCommand command,Func<Task> authority,Func<Task<TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostReview>> readCurrent,Func<Task<TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostSource>> readOwnedSource);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostRecovery> RecoverWorkOrderMaterialCostAsync(TheTechIdea.Data.OilGas.FieldSelectionReview context,TheTechIdea.Data.OilGas.AssetDatabaseScope database,TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialCostReference reference,Func<Task> authority);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderActualCostReview> ReadWorkOrderActualCostsAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionStatus budget,Func<Task> authority);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostReview> ReviewWorkOrderLaborCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionStatus budget,TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostQuery query,TheTechIdea.Data.OilGas.CompanyWorkOrderLaborUsageFacts labor,Func<Task> authority);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostRecovery> RecordWorkOrderLaborCostAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostCommand command,Func<Task> authority,Func<Task<TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostReview>> readCurrent);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostRecovery> RecoverWorkOrderLaborCostAsync(TheTechIdea.Data.OilGas.FieldSelectionReview context,TheTechIdea.Data.OilGas.AssetDatabaseScope database,TheTechIdea.Data.OilGas.CompanyWorkOrderLaborCostReference reference,Func<Task> authority);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetReview> ReviewWorkOrderBudgetAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetReview basis,Func<Task> authority);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetRecovery> CreateWorkOrderBudgetAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetCommand command,Func<Task> authority,Func<Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetReview>> readCurrent);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetRecovery> RecoverWorkOrderBudgetAsync(TheTechIdea.Data.OilGas.FieldSelectionReview context,TheTechIdea.Data.OilGas.AssetDatabaseScope database,TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetReference reference,Func<Task> authority);
        Task<AFE> CreateAfeAsync(AFE afe, string userId, string connectionName = "PPDM39");
        Task<AFE_LINE_ITEM> AddLineItemAsync(AFE_LINE_ITEM lineItem, string userId, string connectionName = "PPDM39");
        Task<AFE> ApproveAfeAsync(string afeId, DateTime approvalDate, string userId, string connectionName = "PPDM39");
        Task<ACCOUNTING_COST> RecordCostAsync(string afeId, ACCOUNTING_COST cost, string userId, string connectionName = "PPDM39");
        Task<List<AFE_LINE_ITEM>> GetLineItemsAsync(string afeId, string connectionName = "PPDM39");
        Task<COST_VARIANCE_REPORT> GenerateBudgetVarianceReportAsync(string afeId, decimal varianceThreshold, string userId, string connectionName = "PPDM39");
        Task<List<COST_VARIANCE_REPORT>> GetVarianceReportsAsync(string? afeId, string? costCenterId, DateTime? startDate, DateTime? endDate, string connectionName = "PPDM39");
    }
}
