using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// One term of a lease (LSE-03). Terms are append-only: a renewal appends a new row and deactivates the old
    /// one, so the previous bonus, rental, royalty and NRI are retained rather than overwritten (AC-4).
    /// </summary>
    public class LEASE_TERM : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string TERM_ID
        {
            get => TermIdValue;
            set => SetProperty(ref TermIdValue, value);
        }

        private string TermIdValue = string.Empty;

        [MaxLength(64)]
        public string LEASE_ID
        {
            get => LeaseIdValue;
            set => SetProperty(ref LeaseIdValue, value);
        }

        private string LeaseIdValue = string.Empty;

        [MaxLength(32)]
        public string? TERM_TYPE
        {
            get => TermTypeValue;
            set => SetProperty(ref TermTypeValue, value);
        }

        private string? TermTypeValue;

        public DateTime? EFFECTIVE_DATE
        {
            get => EffectiveDateValue;
            set => SetProperty(ref EffectiveDateValue, value);
        }

        private DateTime? EffectiveDateValue;

        public DateTime? EXPIRY_DATE
        {
            get => ExpiryDateValue;
            set => SetProperty(ref ExpiryDateValue, value);
        }

        private DateTime? ExpiryDateValue;

        public decimal BONUS_AMOUNT
        {
            get => BonusAmountValue;
            set => SetProperty(ref BonusAmountValue, value);
        }

        private decimal BonusAmountValue;

        [MaxLength(16)]
        public string? BONUS_CURRENCY
        {
            get => BonusCurrencyValue;
            set => SetProperty(ref BonusCurrencyValue, value);
        }

        private string? BonusCurrencyValue;

        public decimal ROYALTY_RATE
        {
            get => RoyaltyRateValue;
            set => SetProperty(ref RoyaltyRateValue, value);
        }

        private decimal RoyaltyRateValue;

        public decimal NET_REVENUE_INTEREST
        {
            get => NetRevenueInterestValue;
            set => SetProperty(ref NetRevenueInterestValue, value);
        }

        private decimal NetRevenueInterestValue;

        public decimal OVERRIDING_ROYALTY
        {
            get => OverridingRoyaltyValue;
            set => SetProperty(ref OverridingRoyaltyValue, value);
        }

        private decimal OverridingRoyaltyValue;

        public string? BURDENS_TEXT
        {
            get => BurdensTextValue;
            set => SetProperty(ref BurdensTextValue, value);
        }

        private string? BurdensTextValue;

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
