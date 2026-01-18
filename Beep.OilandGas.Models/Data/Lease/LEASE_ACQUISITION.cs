using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// Lease acquisition entity - represents a lease acquisition transaction
    /// PPDM39 compliant with full audit trail
    /// </summary>
    public partial class LEASE_ACQUISITION : Entity, IPPDMEntity
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
        private String ROW_CREATED_BYValue = "";
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue = DateTime.UtcNow;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue = "";
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue = DateTime.UtcNow;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ACTIVE_INDValue = "Y";
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue = "";
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String ROW_QUALITYValue = "";
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }
    }
}
