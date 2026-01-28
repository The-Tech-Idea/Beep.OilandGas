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

        private Decimal? ACREAGE_SIZEValue;
        public Decimal? ACREAGE_SIZE
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

        private Decimal? ACQUISITION_COSTValue;
        public Decimal? ACQUISITION_COST
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

        private Decimal? WORKING_INTERESTValue;
        public Decimal? WORKING_INTEREST
        {
            get { return this.WORKING_INTERESTValue; }
            set { SetProperty(ref WORKING_INTERESTValue, value); }
        }

        private Decimal? NET_REVENUE_INTERESTValue;
        public Decimal? NET_REVENUE_INTEREST
        {
            get { return this.NET_REVENUE_INTERESTValue; }
            set { SetProperty(ref NET_REVENUE_INTERESTValue, value); }
        }

        // PPDM Audit Fields
    }
}
