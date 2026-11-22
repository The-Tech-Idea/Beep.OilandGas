using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The open or closed state of one field's reconciliation month (PRD-05 AC-4). A closed month is read-only: its
    /// variance reasons can no longer be assigned, and the page marks it closed with who closed it and when.
    /// </summary>
    /// <remarks>
    /// No accounting period entity exists in this repository to read the state from, so the reconciliation owns the marker
    /// it enforces. Closing is one-way by design: reopening a closed month would need an approval trail the feature does
    /// not define.
    /// </remarks>
    public class RECONCILIATION_PERIOD : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string PERIOD_ID
        {
            get => PeriodIdValue;
            set => SetProperty(ref PeriodIdValue, value);
        }

        private string PeriodIdValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        /// <summary>The first day of the month.</summary>
        public DateTime PERIOD_MONTH
        {
            get => PeriodMonthValue;
            set => SetProperty(ref PeriodMonthValue, value);
        }

        private DateTime PeriodMonthValue;

        /// <summary>OPEN or CLOSED.</summary>
        [MaxLength(16)]
        public string STATE
        {
            get => StateValue;
            set => SetProperty(ref StateValue, value);
        }

        private string StateValue = "OPEN";

        [MaxLength(128)]
        public string? CLOSED_BY
        {
            get => ClosedByValue;
            set => SetProperty(ref ClosedByValue, value);
        }

        private string? ClosedByValue;

        public DateTime? CLOSED_AT
        {
            get => ClosedAtValue;
            set => SetProperty(ref ClosedAtValue, value);
        }

        private DateTime? ClosedAtValue;

        [MaxLength(512)]
        public string? NOTE
        {
            get => NoteValue;
            set => SetProperty(ref NoteValue, value);
        }

        private string? NoteValue;
    }
}
