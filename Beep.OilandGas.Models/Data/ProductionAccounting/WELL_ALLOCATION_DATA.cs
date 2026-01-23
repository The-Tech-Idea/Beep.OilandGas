using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class WELL_ALLOCATION_DATA : ModelEntityBase {
        private System.String WELL_ALLOCATION_DATA_IDValue;
        public System.String WELL_ALLOCATION_DATA_ID
        {
            get { return this.WELL_ALLOCATION_DATA_IDValue; }
            set { SetProperty(ref WELL_ALLOCATION_DATA_IDValue, value); }
        }

        private System.String WELL_IDValue;
        public System.String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private System.String LEASE_IDValue;
        public System.String LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
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

        private System.Decimal? MEASURED_PRODUCTIONValue;
        public System.Decimal? MEASURED_PRODUCTION
        {
            get { return this.MEASURED_PRODUCTIONValue; }
            set { SetProperty(ref MEASURED_PRODUCTIONValue, value); }
        }

        private System.Decimal? ESTIMATED_PRODUCTIONValue;
        public System.Decimal? ESTIMATED_PRODUCTION
        {
            get { return this.ESTIMATED_PRODUCTIONValue; }
            set { SetProperty(ref ESTIMATED_PRODUCTIONValue, value); }
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


