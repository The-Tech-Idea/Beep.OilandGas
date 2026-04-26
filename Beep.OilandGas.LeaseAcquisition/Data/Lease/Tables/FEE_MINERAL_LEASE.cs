using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public partial class FEE_MINERAL_LEASE : ModelEntityBase
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

        private System.String LEASE_NUMBERValue;
        public System.String LEASE_NUMBER
        {
            get { return this.LEASE_NUMBERValue; }
            set { SetProperty(ref LEASE_NUMBERValue, value); }
        }

        private System.String LEASE_NAMEValue;
        public System.String LEASE_NAME
        {
            get { return this.LEASE_NAMEValue; }
            set { SetProperty(ref LEASE_NAMEValue, value); }
        }

        private System.String MINERAL_OWNER_BA_IDValue;
        public System.String MINERAL_OWNER_BA_ID
        {
            get { return this.MINERAL_OWNER_BA_IDValue; }
            set { SetProperty(ref MINERAL_OWNER_BA_IDValue, value); }
        }

        private System.String SURFACE_OWNER_BA_IDValue;
        public System.String SURFACE_OWNER_BA_ID
        {
            get { return this.SURFACE_OWNER_BA_IDValue; }
            set { SetProperty(ref SURFACE_OWNER_BA_IDValue, value); }
        }

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.Int32? PRIMARY_TERM_MONTHSValue;
        public System.Int32? PRIMARY_TERM_MONTHS
        {
            get { return this.PRIMARY_TERM_MONTHSValue; }
            set { SetProperty(ref PRIMARY_TERM_MONTHSValue, value); }
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

        private System.String IS_HELD_BY_PRODUCTIONValue;
        public System.String IS_HELD_BY_PRODUCTION
        {
            get { return this.IS_HELD_BY_PRODUCTIONValue; }
            set { SetProperty(ref IS_HELD_BY_PRODUCTIONValue, value); }
        }

        // ── O&G standard lease fields ────────────────────────────────────────────
        private string LESSEE_BA_IDValue = string.Empty;
        public string LESSEE_BA_ID { get => LESSEE_BA_IDValue; set => SetProperty(ref LESSEE_BA_IDValue, value); }

        private string COUNTY_IDValue = string.Empty;
        public string COUNTY_ID { get => COUNTY_IDValue; set => SetProperty(ref COUNTY_IDValue, value); }

        private string STATE_IDValue = string.Empty;
        public string STATE_ID { get => STATE_IDValue; set => SetProperty(ref STATE_IDValue, value); }

        private string COUNTRY_IDValue = string.Empty;
        public string COUNTRY_ID { get => COUNTRY_IDValue; set => SetProperty(ref COUNTRY_IDValue, value); }

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

        private decimal DELAY_RENTAL_AMOUNTValue;
        public decimal DELAY_RENTAL_AMOUNT { get => DELAY_RENTAL_AMOUNTValue; set => SetProperty(ref DELAY_RENTAL_AMOUNTValue, value); }

        private DateTime? ACQUISITION_DATEValue;
        public DateTime? ACQUISITION_DATE { get => ACQUISITION_DATEValue; set => SetProperty(ref ACQUISITION_DATEValue, value); }

        private string ACQUISITION_STATUSValue = string.Empty;
        public string ACQUISITION_STATUS { get => ACQUISITION_STATUSValue; set => SetProperty(ref ACQUISITION_STATUSValue, value); }
    }
}
