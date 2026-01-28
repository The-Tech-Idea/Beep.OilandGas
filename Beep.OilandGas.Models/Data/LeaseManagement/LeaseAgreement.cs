
namespace Beep.OilandGas.Models.Data.LeaseManagement
{
    public class LeaseAgreement : ModelEntityBase
    {
        private string? LeaseAgreementIdValue;

        public string? LeaseAgreementId

        {

            get { return this.LeaseAgreementIdValue; }

            set { SetProperty(ref LeaseAgreementIdValue, value); }

        }
        private string? LeaseIdValue;

        public string? LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string? AgreementNumberValue;

        public string? AgreementNumber

        {

            get { return this.AgreementNumberValue; }

            set { SetProperty(ref AgreementNumberValue, value); }

        }
        private DateTime? EffectiveDateValue;

        public DateTime? EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private decimal? RoyaltyRateValue;

        public decimal? RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }
        private string? OperatorNameValue;

        public string? OperatorName

        {

            get { return this.OperatorNameValue; }

            set { SetProperty(ref OperatorNameValue, value); }

        }
        private string? AgreementTypeValue;

        public string? AgreementType

        {

            get { return this.AgreementTypeValue; }

            set { SetProperty(ref AgreementTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
