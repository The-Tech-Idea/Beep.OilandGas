using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for authorization workflows tied to accounting activity.
    /// </summary>
    public interface IAuthorizationWorkflowService
    {
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionFacts?> ReadWorkOrderBudgetDecisionFactsAsync(Beep.OilandGas.Models.Data.Field.FieldSelectionReview context,Beep.OilandGas.Models.Data.AccessControl.AssetDatabaseScope database,Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionQuery query,Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetFacts draft,Func<Task> authority);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionReview> ReviewWorkOrderBudgetDecisionAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetReview current,Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionQuery query,Func<Task> authority);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionRecovery> RecordWorkOrderBudgetDecisionAsync(Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionCommand command,Func<Task> authority,Func<Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionReview>> readCurrent);
        Task<Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionRecovery> RecoverWorkOrderBudgetDecisionAsync(Beep.OilandGas.Models.Data.Field.FieldSelectionReview context,Beep.OilandGas.Models.Data.AccessControl.AssetDatabaseScope database,Beep.OilandGas.Models.Data.WorkOrder.CompanyWorkOrderBudgetDecisionReference reference,Func<Task> authority);
        Task<bool> ValidateAfeAuthorizationAsync(string afeId, string userId, string connectionName = "PPDM39");
        Task<AFE> ApproveAfeAsync(string afeId, DateTime approvalDate, string userId, string connectionName = "PPDM39");
        Task<bool> IsCostAuthorizedAsync(ACCOUNTING_COST cost, string connectionName = "PPDM39");
    }
}
