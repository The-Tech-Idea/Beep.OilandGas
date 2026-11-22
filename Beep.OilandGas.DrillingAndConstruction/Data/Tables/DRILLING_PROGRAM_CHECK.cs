using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// The recorded status of one design check of a drilling program (DEV-05 AC-2). Three rows are written when a
    /// program is created, each <c>NOT_RUN</c>, so a check that was never run is shown as not run rather than as passed.
    /// </summary>
    public class DRILLING_PROGRAM_CHECK : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string CHECK_ID
        {
            get => CheckIdValue;
            set => SetProperty(ref CheckIdValue, value);
        }

        private string CheckIdValue = string.Empty;

        [MaxLength(64)]
        public string PROGRAM_ID
        {
            get => ProgramIdValue;
            set => SetProperty(ref ProgramIdValue, value);
        }

        private string ProgramIdValue = string.Empty;

        /// <summary>HYDRAULICS, TORQUE_DRAG or CASING_DESIGN.</summary>
        [MaxLength(32)]
        public string CHECK_NAME
        {
            get => CheckNameValue;
            set => SetProperty(ref CheckNameValue, value);
        }

        private string CheckNameValue = string.Empty;

        /// <summary>NOT_RUN, PASS or FAIL.</summary>
        [MaxLength(16)]
        public string STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string StatusValue = "NOT_RUN";

        /// <summary>The route or id of the detailed screen/result this check belongs to, when one exists.</summary>
        [MaxLength(256)]
        public string? REFERENCE
        {
            get => ReferenceValue;
            set => SetProperty(ref ReferenceValue, value);
        }

        private string? ReferenceValue;

        public string? NOTE
        {
            get => NoteValue;
            set => SetProperty(ref NoteValue, value);
        }

        private string? NoteValue;

        [MaxLength(128)]
        public string? CHECKED_BY
        {
            get => CheckedByValue;
            set => SetProperty(ref CheckedByValue, value);
        }

        private string? CheckedByValue;

        public DateTime? CHECKED_AT
        {
            get => CheckedAtValue;
            set => SetProperty(ref CheckedAtValue, value);
        }

        private DateTime? CheckedAtValue;
    }
}
