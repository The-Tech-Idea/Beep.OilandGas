using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class DIVISION_ORDER : ModelEntityBase {
        private System.String DIVISION_ORDER_IDValue;
        public System.String DIVISION_ORDER_ID
        {
            get { return this.DIVISION_ORDER_IDValue; }
            set { SetProperty(ref DIVISION_ORDER_IDValue, value); }
        }

        private System.String PROPERTY_OR_LEASE_IDValue;
        public System.String PROPERTY_OR_LEASE_ID
        {
            get { return this.PROPERTY_OR_LEASE_IDValue; }
            set { SetProperty(ref PROPERTY_OR_LEASE_IDValue, value); }
        }

        private System.String OWNER_IDValue;
        public System.String OWNER_ID
        {
            get { return this.OWNER_IDValue; }
            set { SetProperty(ref OWNER_IDValue, value); }
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

        private System.Decimal? ROYALTY_INTERESTValue;
        public System.Decimal? ROYALTY_INTEREST
        {
            get { return this.ROYALTY_INTERESTValue; }
            set { SetProperty(ref ROYALTY_INTERESTValue, value); }
        }

        private System.Decimal? OVERRIDING_ROYALTY_INTERESTValue;
        public System.Decimal? OVERRIDING_ROYALTY_INTEREST
        {
            get { return this.OVERRIDING_ROYALTY_INTERESTValue; }
            set { SetProperty(ref OVERRIDING_ROYALTY_INTERESTValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.DateTime? APPROVAL_DATEValue;
        public System.DateTime? APPROVAL_DATE
        {
            get { return this.APPROVAL_DATEValue; }
            set { SetProperty(ref APPROVAL_DATEValue, value); }
        }

        private System.String APPROVED_BYValue;
        public System.String APPROVED_BY
        {
            get { return this.APPROVED_BYValue; }
            set { SetProperty(ref APPROVED_BYValue, value); }
        }

        private System.String NOTESValue;
        public System.String NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
