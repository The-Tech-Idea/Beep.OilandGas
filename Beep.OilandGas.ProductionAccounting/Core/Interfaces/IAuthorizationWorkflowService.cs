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
        Task<bool> ValidateAfeAuthorizationAsync(string afeId, string userId, string cn = "PPDM39");
        Task<AFE> ApproveAfeAsync(string afeId, DateTime approvalDate, string userId, string cn = "PPDM39");
        Task<bool> IsCostAuthorizedAsync(ACCOUNTING_COST cost, string cn = "PPDM39");
    }
}
