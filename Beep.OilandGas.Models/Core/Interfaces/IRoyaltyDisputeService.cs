using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Royalty;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for royalty dispute workflows.
    /// </summary>
    public interface IRoyaltyDisputeService
    {
        Task<ROYALTY_DISPUTE> CreateDisputeAsync(ROYALTY_DISPUTE dispute, string userId, string cn = "PPDM39");
        Task<ROYALTY_DISPUTE> ResolveDisputeAsync(string disputeId, DateTime resolutionDate, string resolutionNotes, string userId, string cn = "PPDM39");
        Task<List<ROYALTY_DISPUTE>> GetDisputesAsync(string royaltyOwnerBaId, string? status, string cn = "PPDM39");
    }
}
