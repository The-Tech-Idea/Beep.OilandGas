using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public partial class NET_PROFIT_LEASE : ModelEntityBase
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

        private System.Decimal  NET_PROFIT_INTERESTValue;
        public System.Decimal  NET_PROFIT_INTEREST
        {
            get { return this.NET_PROFIT_INTERESTValue; }
            set { SetProperty(ref NET_PROFIT_INTERESTValue, value); }
        }

        private System.String COSTS_RECOVERABLEValue;
        public System.String COSTS_RECOVERABLE
        {
            get { return this.COSTS_RECOVERABLEValue; }
            set { SetProperty(ref COSTS_RECOVERABLEValue, value); }
        }

        private System.Decimal  RECOVERY_PERCENTAGEValue;
        public System.Decimal  RECOVERY_PERCENTAGE
        {
            get { return this.RECOVERY_PERCENTAGEValue; }
            set { SetProperty(ref RECOVERY_PERCENTAGEValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

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

        private System.Decimal  WORKING_INTERESTValue;
        public System.Decimal  WORKING_INTEREST
        {
            get { return this.WORKING_INTERESTValue; }
            set { SetProperty(ref WORKING_INTERESTValue, value); }
        }

        private System.Decimal  NET_REVENUE_INTERESTValue;
        public System.Decimal  NET_REVENUE_INTEREST
        {
            get { return this.NET_REVENUE_INTERESTValue; }
            set { SetProperty(ref NET_REVENUE_INTERESTValue, value); }
        }

        private System.Decimal  ROYALTY_RATEValue;
        public System.Decimal  ROYALTY_RATE
        {
            get { return this.ROYALTY_RATEValue; }
            set { SetProperty(ref ROYALTY_RATEValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        // Additional IPPDMEntity properties
        private System.String AREA_IDValue;
        public System.String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private System.String AREA_TYPEValue;
        public System.String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private System.String BUSINESS_ASSOCIATE_IDValue;
        public System.String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private System.DateTime? EXPIRY_DATEValue;

    }
}
