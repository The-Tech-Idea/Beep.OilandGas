using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// One obligation of a lease (LSE-03 AC-5): a rental or a minimum-spend commitment with a due date. Overdue is
    /// never stored - it is computed from the due date and whether a payment references the obligation.
    /// </summary>
    public class LEASE_OBLIGATION : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string OBLIGATION_ID
        {
            get => ObligationIdValue;
            set => SetProperty(ref ObligationIdValue, value);
        }

        private string ObligationIdValue = string.Empty;

        [MaxLength(64)]
        public string LEASE_ID
        {
            get => LeaseIdValue;
            set => SetProperty(ref LeaseIdValue, value);
        }

        private string LeaseIdValue = string.Empty;

        [MaxLength(32)]
        public string? OBLIGATION_TYPE
        {
            get => ObligationTypeValue;
            set => SetProperty(ref ObligationTypeValue, value);
        }

        private string? ObligationTypeValue;

        public string? DESCRIPTION
        {
            get => DescriptionValue;
            set => SetProperty(ref DescriptionValue, value);
        }

        private string? DescriptionValue;

        public DateTime? DUE_DATE
        {
            get => DueDateValue;
            set => SetProperty(ref DueDateValue, value);
        }

        private DateTime? DueDateValue;

        public decimal MINIMUM_SPEND
        {
            get => MinimumSpendValue;
            set => SetProperty(ref MinimumSpendValue, value);
        }

        private decimal MinimumSpendValue;

        [MaxLength(16)]
        public string? CURRENCY
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;

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
