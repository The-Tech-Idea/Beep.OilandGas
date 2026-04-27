using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Domain hook for lead-to-prospect workflow completion (Phase 2 — extend with LEAD/PROSPECT persistence).
    /// </summary>
    public interface ILeadExplorationService
    {
        /// <summary>
        /// Invoked after the process engine completes the PROSPECT_CREATION step for a lead workflow instance.
        /// </summary>
        Task AfterProspectCreationStepCompletedAsync(
            string processInstanceId,
            string userId,
            CancellationToken cancellationToken = default);
    }
}
