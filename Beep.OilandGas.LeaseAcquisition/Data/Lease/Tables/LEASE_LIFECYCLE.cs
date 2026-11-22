using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// One lifecycle event of a lease (LSE-03): renewed, relinquished or expired. Append-only, so the current
    /// state is the newest event and a renewal never erases the term that came before it (AC-4).
    /// </summary>
    public class LEASE_LIFECYCLE : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string EVENT_ID
        {
            get => EventIdValue;
            set => SetProperty(ref EventIdValue, value);
        }

        private string EventIdValue = string.Empty;

        [MaxLength(64)]
        public string LEASE_ID
        {
            get => LeaseIdValue;
            set => SetProperty(ref LeaseIdValue, value);
        }

        private string LeaseIdValue = string.Empty;

        [MaxLength(32)]
        public string? EVENT
        {
            get => EventValue;
            set => SetProperty(ref EventValue, value);
        }

        private string? EventValue;

        public DateTime? EFFECTIVE_DATE
        {
            get => EffectiveDateValue;
            set => SetProperty(ref EffectiveDateValue, value);
        }

        private DateTime? EffectiveDateValue;

        public string? RATIONALE
        {
            get => RationaleValue;
            set => SetProperty(ref RationaleValue, value);
        }

        private string? RationaleValue;

        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;

        public DateTime? RECORDED_AT
        {
            get => RecordedAtValue;
            set => SetProperty(ref RecordedAtValue, value);
        }

        private DateTime? RecordedAtValue;
    }
}
