using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.Decommissioning
{
    /// <summary>
    /// The planning columns of a facility's decommissioning record (DEC-03), added to the table that already holds its state.
    /// </summary>
    /// <remarks>
    /// DEC-01 recorded that the facility status table carried no planned date, so a facility's planned year and overdue had no
    /// recorded source. DEC-02 closed that for wells on the abandonment table; this closes it for facilities on theirs.
    /// </remarks>
    public partial class DECOMMISSIONING_STATUS
    {
        /// <summary>The date the decommissioning is planned to be executed, when one has been agreed.</summary>
        public DateTime? PLANNED_DATE
        {
            get => PlannedDateValue;
            set => SetProperty(ref PlannedDateValue, value);
        }

        private DateTime? PlannedDateValue;

        /// <summary>The person who planned it, so an approval can be kept apart from it (DEC-03 AC-6).</summary>
        [MaxLength(128)]
        public string? PLANNED_BY
        {
            get => PlannedByValue;
            set => SetProperty(ref PlannedByValue, value);
        }

        private string? PlannedByValue;

        /// <summary>The decommissioning method the plan assumes (REMOVAL, IN_SITU, PARTIAL_REMOVAL).</summary>
        [MaxLength(64)]
        public string? DECOMMISSIONING_METHOD
        {
            get => MethodValue;
            set => SetProperty(ref MethodValue, value);
        }

        private string? MethodValue;
    }
}
