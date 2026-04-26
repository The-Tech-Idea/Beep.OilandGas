using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public partial class LEASE_ACQUISITION : ModelEntityBase
    {
        private String LEASE_ACQUISITION_IDValue;
        public String LEASE_ACQUISITION_ID
        {
            get { return this.LEASE_ACQUISITION_IDValue; }
            set { SetProperty(ref LEASE_ACQUISITION_IDValue, value); }
        }

        private String FIELD_IDValue;
        public String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private String LEASE_NAMEValue = "";
        public String LEASE_NAME
        {
            get { return this.LEASE_NAMEValue; }
            set { SetProperty(ref LEASE_NAMEValue, value); }
        }

        private String LOCATIONIDValue = "";
        public String LOCATION_ID
        {
            get { return this.LOCATIONIDValue; }
            set { SetProperty(ref LOCATIONIDValue, value); }
        }

        private Decimal  ACREAGE_SIZEValue;
        public Decimal  ACREAGE_SIZE
        {
            get { return this.ACREAGE_SIZEValue; }
            set { SetProperty(ref ACREAGE_SIZEValue, value); }
        }

        private DateTime? ACQUISITION_DATEValue;
        public DateTime? ACQUISITION_DATE
        {
            get { return this.ACQUISITION_DATEValue; }
            set { SetProperty(ref ACQUISITION_DATEValue, value); }
        }

        private Decimal  ACQUISITION_COSTValue;
        public Decimal  ACQUISITION_COST
        {
            get { return this.ACQUISITION_COSTValue; }
            set { SetProperty(ref ACQUISITION_COSTValue, value); }
        }

        private String COUNTYIDValue = "";
        public String COUNTY_ID
        {
            get { return this.COUNTYIDValue; }
            set { SetProperty(ref COUNTYIDValue, value); }
        }

        private String STATEIDValue = "";
        public String STATE_ID
        {
            get { return this.STATEIDValue; }
            set { SetProperty(ref STATEIDValue, value); }
        }

        private String COUNTRYIDValue = "";
        public String COUNTRY_ID
        {
            get { return this.COUNTRYIDValue; }
            set { SetProperty(ref COUNTRYIDValue, value); }
        }

        private String ACQUISITIONSTATUSValue = "";
        public String ACQUISITION_STATUS
        {
            get { return this.ACQUISITIONSTATUSValue; }
            set { SetProperty(ref ACQUISITIONSTATUSValue, value); }
        }

        private String OPERATORValue = "";
        public String OPERATOR
        {
            get { return this.OPERATORValue; }
            set { SetProperty(ref OPERATORValue, value); }
        }

        private String OPERATORIDValue = "";
        public String OPERATOR_ID
        {
            get { return this.OPERATORIDValue; }
            set { SetProperty(ref OPERATORIDValue, value); }
        }

        private DateTime? LEASE_EXPIRATIONValue;
        public DateTime? LEASE_EXPIRATION
        {
            get { return this.LEASE_EXPIRATIONValue; }
            set { SetProperty(ref LEASE_EXPIRATIONValue, value); }
        }

        private Decimal  WORKING_INTERESTValue;
        public Decimal  WORKING_INTEREST
        {
            get { return this.WORKING_INTERESTValue; }
            set { SetProperty(ref WORKING_INTERESTValue, value); }
        }

        private Decimal  NET_REVENUE_INTERESTValue;
        public Decimal  NET_REVENUE_INTEREST
        {
            get { return this.NET_REVENUE_INTERESTValue; }
            set { SetProperty(ref NET_REVENUE_INTERESTValue, value); }
        }

        // ── O&G standard lease fields ────────────────────────────────────────────
        private string LEASE_TYPEValue = string.Empty;
        public string LEASE_TYPE { get => LEASE_TYPEValue; set => SetProperty(ref LEASE_TYPEValue, value); }

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

        private decimal ROYALTY_RATEValue;
        public decimal ROYALTY_RATE { get => ROYALTY_RATEValue; set => SetProperty(ref ROYALTY_RATEValue, value); }

        private decimal DELAY_RENTAL_AMOUNTValue;
        public decimal DELAY_RENTAL_AMOUNT { get => DELAY_RENTAL_AMOUNTValue; set => SetProperty(ref DELAY_RENTAL_AMOUNTValue, value); }

        private DateTime? RENTALS_DUE_DATEValue;
        public DateTime? RENTALS_DUE_DATE { get => RENTALS_DUE_DATEValue; set => SetProperty(ref RENTALS_DUE_DATEValue, value); }

        private int? PRIMARY_TERM_MONTHSValue;
        public int? PRIMARY_TERM_MONTHS { get => PRIMARY_TERM_MONTHSValue; set => SetProperty(ref PRIMARY_TERM_MONTHSValue, value); }

        private string HELD_BY_PRODUCTION_INDValue = "N";
        public string HELD_BY_PRODUCTION_IND { get => HELD_BY_PRODUCTION_INDValue; set => SetProperty(ref HELD_BY_PRODUCTION_INDValue, value); }

        private string LESSOR_BA_IDValue = string.Empty;
        public string LESSOR_BA_ID { get => LESSOR_BA_IDValue; set => SetProperty(ref LESSOR_BA_IDValue, value); }

        private string LESSEE_BA_IDValue = string.Empty;
        public string LESSEE_BA_ID { get => LESSEE_BA_IDValue; set => SetProperty(ref LESSEE_BA_IDValue, value); }

        private string ACQUISITION_COST_OUOMValue = "USD";
        public string ACQUISITION_COST_OUOM { get => ACQUISITION_COST_OUOMValue; set => SetProperty(ref ACQUISITION_COST_OUOMValue, value); }
    }
}
