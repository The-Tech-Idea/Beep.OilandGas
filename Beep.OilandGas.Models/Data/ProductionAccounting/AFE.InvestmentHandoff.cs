using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The investment-handoff columns of an AFE (ECO-03): which reviewed proposal it came from and the conditions the
    /// decision attached.
    /// </summary>
    /// <remarks>
    /// AC-4 says a handoff creates exactly one AFE per decision, and AC-6 says a lost acknowledgement must not duplicate
    /// it. Both need the AFE to remember the proposal it was created for, which is why this column is on the AFE rather
    /// than only on the approval row: after a lost acknowledgement the approval row may still point nowhere, and the
    /// handoff is found by asking the AFEs which proposal they belong to.
    /// </remarks>
    public partial class AFE
    {
        /// <summary>The investment proposal this AFE was handed off from, or null for a directly raised AFE.</summary>
        [MaxLength(128)]
        public string? SOURCE_PROPOSAL_ID
        {
            get => SourceProposalValue;
            set => SetProperty(ref SourceProposalValue, value);
        }

        private string? SourceProposalValue;

        /// <summary>The conditions the investment decision attached, one per line (AC-5).</summary>
        [MaxLength(4096)]
        public string? CONDITIONS_TEXT
        {
            get => ConditionsValue;
            set => SetProperty(ref ConditionsValue, value);
        }

        private string? ConditionsValue;
    }
}
