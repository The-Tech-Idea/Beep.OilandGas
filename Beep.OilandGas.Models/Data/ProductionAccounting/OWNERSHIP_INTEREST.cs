using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class OWNERSHIP_INTEREST : ModelEntityBase {
        private System.String OWNERSHIP_IDValue;
        public System.String OWNERSHIP_ID
        {
            get { return this.OWNERSHIP_IDValue; }
            set { SetProperty(ref OWNERSHIP_IDValue, value); }
        }

        private System.String OWNER_IDValue;
        public System.String OWNER_ID
        {
            get { return this.OWNER_IDValue; }
            set { SetProperty(ref OWNER_IDValue, value); }
        }

        private System.String PROPERTY_OR_LEASE_IDValue;
        public System.String PROPERTY_OR_LEASE_ID
        {
            get { return this.PROPERTY_OR_LEASE_IDValue; }
            set { SetProperty(ref PROPERTY_OR_LEASE_IDValue, value); }
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

        private System.DateTime? EFFECTIVE_START_DATEValue;
        public System.DateTime? EFFECTIVE_START_DATE
        {
            get { return this.EFFECTIVE_START_DATEValue; }
            set { SetProperty(ref EFFECTIVE_START_DATEValue, value); }
        }

        private System.DateTime? EFFECTIVE_END_DATEValue;
        public System.DateTime? EFFECTIVE_END_DATE
        {
            get { return this.EFFECTIVE_END_DATEValue; }
            set { SetProperty(ref EFFECTIVE_END_DATEValue, value); }
        }

        private System.String DIVISION_ORDER_IDValue;
        public System.String DIVISION_ORDER_ID
        {
            get { return this.DIVISION_ORDER_IDValue; }
            set { SetProperty(ref DIVISION_ORDER_IDValue, value); }
        }


        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

    }
}
