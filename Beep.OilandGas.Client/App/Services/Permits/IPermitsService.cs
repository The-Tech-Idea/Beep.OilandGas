using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Permits
{
    /// <summary>
    /// Service interface for Permits and Applications operations
    /// </summary>
    public interface IPermitsService
    {
        Task<object> CreatePermitApplicationAsync(object request, CancellationToken cancellationToken = default);
        Task<object> GetPermitStatusAsync(string permitId, CancellationToken cancellationToken = default);
        Task<List<object>> GetRequiredDocumentsAsync(string permitType, string jurisdiction, CancellationToken cancellationToken = default);
        Task<object> SubmitPermitAsync(string permitId, CancellationToken cancellationToken = default);
        Task<object> UpdatePermitApplicationAsync(string permitId, object request, CancellationToken cancellationToken = default);
        Task<List<object>> GetPermitHistoryAsync(string assetId, CancellationToken cancellationToken = default);
        Task<object> GetPermitComplianceAsync(string permitId, CancellationToken cancellationToken = default);
        Task<object> RenewPermitAsync(string permitId, object request, CancellationToken cancellationToken = default);
        Task<List<object>> GetPendingPermitsAsync(string operatorId, CancellationToken cancellationToken = default);
        Task<object> GetJurisdictionRequirementsAsync(string jurisdiction, string permitType, CancellationToken cancellationToken = default);
    }
}

