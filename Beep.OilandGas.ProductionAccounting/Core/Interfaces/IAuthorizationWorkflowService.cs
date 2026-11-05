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
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionFacts?> ReadWorkOrderBudgetDecisionFactsAsync(TheTechIdea.Data.OilGas.FieldSelectionReview context,TheTechIdea.Data.OilGas.AssetDatabaseScope database,TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionQuery query,TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetFacts draft,Func<Task> authority);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionReview> ReviewWorkOrderBudgetDecisionAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetReview current,TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionQuery query,Func<Task> authority);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionRecovery> RecordWorkOrderBudgetDecisionAsync(TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionCommand command,Func<Task> authority,Func<Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionReview>> readCurrent);
        Task<TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionRecovery> RecoverWorkOrderBudgetDecisionAsync(TheTechIdea.Data.OilGas.FieldSelectionReview context,TheTechIdea.Data.OilGas.AssetDatabaseScope database,TheTechIdea.Data.OilGas.CompanyWorkOrderBudgetDecisionReference reference,Func<Task> authority);
        Task<bool> ValidateAfeAuthorizationAsync(string afeId, string userId, string connectionName = "PPDM39");
        Task<AFE> ApproveAfeAsync(string afeId, DateTime approvalDate, string userId, string connectionName = "PPDM39");
        Task<bool> IsCostAuthorizedAsync(ACCOUNTING_COST cost, string connectionName = "PPDM39");
    }
}
