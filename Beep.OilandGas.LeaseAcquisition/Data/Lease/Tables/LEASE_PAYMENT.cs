using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// One payment against a lease (LSE-03). A payment may reference the obligation it settles; the obligation's
    /// overdue state is still computed from its due date and the presence of a payment, never stored.
    /// </summary>
    public class LEASE_PAYMENT : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string PAYMENT_ID
        {
            get => PaymentIdValue;
            set => SetProperty(ref PaymentIdValue, value);
        }

        private string PaymentIdValue = string.Empty;

        [MaxLength(64)]
        public string LEASE_ID
        {
            get => LeaseIdValue;
            set => SetProperty(ref LeaseIdValue, value);
        }

        private string LeaseIdValue = string.Empty;

        [MaxLength(64)]
        public string? OBLIGATION_ID
        {
            get => ObligationIdValue;
            set => SetProperty(ref ObligationIdValue, value);
        }

        private string? ObligationIdValue;

        public DateTime? PAID_AT
        {
            get => PaidAtValue;
            set => SetProperty(ref PaidAtValue, value);
        }

        private DateTime? PaidAtValue;

        public decimal AMOUNT
        {
            get => AmountValue;
            set => SetProperty(ref AmountValue, value);
        }

        private decimal AmountValue;

        [MaxLength(16)]
        public string? CURRENCY
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;

        [MaxLength(32)]
        public string? KIND
        {
            get => KindValue;
            set => SetProperty(ref KindValue, value);
        }

        private string? KindValue;

        [MaxLength(128)]
        public string? REFERENCE
        {
            get => ReferenceValue;
            set => SetProperty(ref ReferenceValue, value);
        }

        private string? ReferenceValue;

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
