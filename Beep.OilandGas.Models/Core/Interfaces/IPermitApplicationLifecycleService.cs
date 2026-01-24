using System.Collections.Generic;
using System.Threading.Tasks;
using PermitApplicationModel = Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplication;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for managing permit applications using domain models.
    /// </summary>
    public interface IPermitApplicationLifecycleService
    {
        Task<PermitApplicationModel> CreateAsync(PermitApplicationModel application, string userId);
        Task<PermitApplicationModel> UpdateAsync(string applicationId, PermitApplicationModel application, string userId);
        Task<PermitApplicationModel?> GetByIdAsync(string applicationId);
        Task<IReadOnlyList<PermitApplicationModel>> GetByStatusAsync(PermitApplicationStatus status);
        Task<PermitApplicationModel> SubmitAsync(string applicationId, string userId);
        Task<PermitApplicationModel> ProcessDecisionAsync(string applicationId, string decision, string decisionRemarks, string userId);
    }
}
