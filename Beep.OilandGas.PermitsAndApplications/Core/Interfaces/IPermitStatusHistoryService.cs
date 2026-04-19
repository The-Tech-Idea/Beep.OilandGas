using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for tracking permit status and workflow.
    /// </summary>
    public interface IPermitStatusHistoryService
    {
        Task<PERMIT_APPLICATION?> GetCurrentAsync(string applicationId);
        Task<IReadOnlyList<PERMIT_STATUS_HISTORY>> GetHistoryAsync(string applicationId);
        Task<PERMIT_APPLICATION> UpdateStatusAsync(
            string applicationId,
            string status,
            string? remarks,
            string userId);
    }
}
