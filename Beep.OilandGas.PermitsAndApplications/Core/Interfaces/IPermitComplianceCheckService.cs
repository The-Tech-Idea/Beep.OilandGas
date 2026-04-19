using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for compliance checks against jurisdiction requirements.
    /// </summary>
    public interface IPermitComplianceCheckService
    {
        Task<PermitComplianceResult> CheckComplianceAsync(string applicationId, string? configDirectory = null);
    }
}
