using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for internal control workflows and segregation-of-duties checks.
    /// </summary>
    public interface IInternalControlService
    {
        Task<APPROVAL_WORKFLOW> RequestApprovalAsync(string entityName, string entityId, decimal amount, string requestedBy, string? comment, string cn = "PPDM39");
        Task<APPROVAL_WORKFLOW> ApproveAsync(string approvalId, string approverId, string? comment, string cn = "PPDM39");
        Task<List<APPROVAL_WORKFLOW>> GetApprovalsForEntityAsync(string entityName, string entityId, string cn = "PPDM39");
        Task<bool> ValidateSegregationOfDutiesAsync(string entityName, string requestedBy, string approverId, decimal amount, string cn = "PPDM39");
    }
}
