using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Client.App.Services.Permits
{
    /// <summary>
    /// Service interface for Permits and Applications operations
    /// </summary>
    public interface IPermitsService
    {
        Task<PermitApplication> CreatePermitApplicationAsync(PermitApplication request, CancellationToken cancellationToken = default);
        Task<string> GetPermitStatusAsync(string permitId, CancellationToken cancellationToken = default);
        Task<List<RequiredDocument>> GetRequiredDocumentsAsync(string permitType, string jurisdiction, CancellationToken cancellationToken = default);
        Task<bool> SubmitPermitAsync(string permitId, CancellationToken cancellationToken = default);
        Task<PermitApplication> UpdatePermitApplicationAsync(string permitId, PermitApplication request, CancellationToken cancellationToken = default);
        Task<List<PermitHistory>> GetPermitHistoryAsync(string assetId, CancellationToken cancellationToken = default);
        Task<PermitComplianceResult> GetPermitComplianceAsync(string permitId, CancellationToken cancellationToken = default);
        Task<PermitApplication> RenewPermitAsync(string permitId, PermitApplication request, CancellationToken cancellationToken = default);
        Task<List<PermitApplication>> GetPendingPermitsAsync(string operatorId, CancellationToken cancellationToken = default);
        Task<PermitRequirements> GetJurisdictionRequirementsAsync(string jurisdiction, string permitType, CancellationToken cancellationToken = default);
    }
}
