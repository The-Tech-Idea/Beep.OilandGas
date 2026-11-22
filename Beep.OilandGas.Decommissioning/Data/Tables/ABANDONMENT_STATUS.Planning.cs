using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.Decommissioning
{
    /// <summary>
    /// The planning columns of a well's abandonment record (DEC-02), added to the table that already holds the state.
    /// </summary>
    /// <remarks>
    /// DEC-01 recorded that neither status table carried a planned date, so its "planned year" and "overdue" had no recorded
    /// source. The date belongs here: a programme is planned for a date, and the abandonment record is the programme. The
    /// column is additive, so the recorded status rows stay valid and read as "no planned date" until one is set.
    /// </remarks>
    public partial class ABANDONMENT_STATUS
    {
        /// <summary>The date the programme is planned to be executed, when one has been agreed.</summary>
        public DateTime? PLANNED_DATE
        {
            get => PlannedDateValue;
            set => SetProperty(ref PlannedDateValue, value);
        }

        private DateTime? PlannedDateValue;

        /// <summary>The person who planned the programme, so closure can be kept apart from it (DEC-02 AC-6).</summary>
        [MaxLength(128)]
        public string? PLANNED_BY
        {
            get => PlannedByValue;
            set => SetProperty(ref PlannedByValue, value);
        }

        private string? PlannedByValue;
    }
}
