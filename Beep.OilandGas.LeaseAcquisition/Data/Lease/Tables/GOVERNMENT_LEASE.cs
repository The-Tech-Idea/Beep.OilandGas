using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public partial class GOVERNMENT_LEASE : ModelEntityBase
    {
        private System.String LEASE_IDValue;
        public System.String LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.String GOVERNMENT_AGENCYValue;
        public System.String GOVERNMENT_AGENCY
        {
            get { return this.GOVERNMENT_AGENCYValue; }
            set { SetProperty(ref GOVERNMENT_AGENCYValue, value); }
        }

        private System.String LEASE_NUMBERValue;
        public System.String LEASE_NUMBER
        {
            get { return this.LEASE_NUMBERValue; }
            set { SetProperty(ref LEASE_NUMBERValue, value); }
        }

        private System.String IS_FEDERALValue;
        public System.String IS_FEDERAL
        {
            get { return this.IS_FEDERALValue; }
            set { SetProperty(ref IS_FEDERALValue, value); }
        }

        private System.String IS_INDIANValue;
        public System.String IS_INDIAN
        {
            get { return this.IS_INDIANValue; }
            set { SetProperty(ref IS_INDIANValue, value); }
        }

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.Decimal? WORKING_INTERESTValue;
        public System.Decimal? WORKING_INTEREST
        {
            get { return this.WORKING_INTERESTValue; }
            set { SetProperty(ref WORKING_INTERESTValue, value); }
        }

        private System.Decimal? NET_REVENUE_INTERESTValue;
        public System.Decimal? NET_REVENUE_INTEREST
        {
            get { return this.NET_REVENUE_INTERESTValue; }
            set { SetProperty(ref NET_REVENUE_INTERESTValue, value); }
        }

        private System.Decimal? ROYALTY_RATEValue;
        public System.Decimal? ROYALTY_RATE
        {
            get { return this.ROYALTY_RATEValue; }
            set { SetProperty(ref ROYALTY_RATEValue, value); }
        }

        // ── O&G standard lease fields ──
        private string LEASE_NAMEValue = string.Empty;
        public string LEASE_NAME { get => LEASE_NAMEValue; set => SetProperty(ref LEASE_NAMEValue, value); }

        private decimal GROSS_ACREAGEValue;
        public decimal GROSS_ACREAGE { get => GROSS_ACREAGEValue; set => SetProperty(ref GROSS_ACREAGEValue, value); }

        private decimal NET_ACREAGEValue;
        public decimal NET_ACREAGE { get => NET_ACREAGEValue; set => SetProperty(ref NET_ACREAGEValue, value); }

        private string ACREAGE_OUOMValue = "ac";
        public string ACREAGE_OUOM { get => ACREAGE_OUOMValue; set => SetProperty(ref ACREAGE_OUOMValue, value); }

        private decimal BONUS_AMOUNTValue;
        public decimal BONUS_AMOUNT { get => BONUS_AMOUNTValue; set => SetProperty(ref BONUS_AMOUNTValue, value); }

        private string BONUS_AMOUNT_OUOMValue = "USD";
        public string BONUS_AMOUNT_OUOM { get => BONUS_AMOUNT_OUOMValue; set => SetProperty(ref BONUS_AMOUNT_OUOMValue, value); }

        private string HELD_BY_PRODUCTION_INDValue = "N";
        public string HELD_BY_PRODUCTION_IND { get => HELD_BY_PRODUCTION_INDValue; set => SetProperty(ref HELD_BY_PRODUCTION_INDValue, value); }

        private int? PRIMARY_TERM_MONTHSValue;
        public int? PRIMARY_TERM_MONTHS { get => PRIMARY_TERM_MONTHSValue; set => SetProperty(ref PRIMARY_TERM_MONTHSValue, value); }

        private string FEDERAL_LEASE_CLASSValue = string.Empty;
        public string FEDERAL_LEASE_CLASS { get => FEDERAL_LEASE_CLASSValue; set => SetProperty(ref FEDERAL_LEASE_CLASSValue, value); }

        private string COUNTY_IDValue = string.Empty;
        public string COUNTY_ID { get => COUNTY_IDValue; set => SetProperty(ref COUNTY_IDValue, value); }

        private string STATE_IDValue = string.Empty;
        public string STATE_ID { get => STATE_IDValue; set => SetProperty(ref STATE_IDValue, value); }
    }
}
