using System;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Royalty
{
    /// <summary>Request payload for dispute resolution endpoint.</summary>
    public class ResolveRoyaltyDisputeRequest
    {
        public DateTime? ResolutionDate { get; set; }
        public string? ResolutionNotes { get; set; }
    }
}
