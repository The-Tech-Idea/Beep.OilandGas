using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// One period of reporting responsibility for a drilling execution (DEV-06 AC-4). Append-only: a transfer closes
    /// the current period and opens a new one, so the history names who was responsible, from when, and why it moved.
    /// </summary>
    public class DEVELOPMENT_REPORT_RESPONSIBILITY : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string RESPONSIBILITY_ID
        {
            get => ResponsibilityIdValue;
            set => SetProperty(ref ResponsibilityIdValue, value);
        }

        private string ResponsibilityIdValue = string.Empty;

        [MaxLength(64)]
        public string EXECUTION_ID
        {
            get => ExecutionIdValue;
            set => SetProperty(ref ExecutionIdValue, value);
        }

        private string ExecutionIdValue = string.Empty;

        [MaxLength(128)]
        public string ASSIGNED_TO
        {
            get => AssignedToValue;
            set => SetProperty(ref AssignedToValue, value);
        }

        private string AssignedToValue = string.Empty;

        /// <summary>The role the holder acts in (for example COMPANY_MAN, TOOLPUSHER, CONTRACTOR).</summary>
        [MaxLength(64)]
        public string? ROLE
        {
            get => RoleValue;
            set => SetProperty(ref RoleValue, value);
        }

        private string? RoleValue;

        public DateTime? FROM_AT
        {
            get => FromAtValue;
            set => SetProperty(ref FromAtValue, value);
        }

        private DateTime? FromAtValue;

        /// <summary>Null while the period is open.</summary>
        public DateTime? TO_AT
        {
            get => ToAtValue;
            set => SetProperty(ref ToAtValue, value);
        }

        private DateTime? ToAtValue;

        /// <summary>Why the responsibility moved (AC-4).</summary>
        public string? REASON
        {
            get => ReasonValue;
            set => SetProperty(ref ReasonValue, value);
        }

        private string? ReasonValue;

        /// <summary>The person who made the transfer; empty on the opening assignment.</summary>
        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;

        /// <summary>'Y' while this period is the current holder; 'N' once superseded.</summary>
        [MaxLength(1)]
        public string CURRENT_IND
        {
            get => CurrentIndValue;
            set => SetProperty(ref CurrentIndValue, value);
        }

        private string CurrentIndValue = "Y";
    }
}
