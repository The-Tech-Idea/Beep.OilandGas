using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TRACT_ALLOCATION_DATA : ModelEntityBase {
        private System.String TRACT_ALLOCATION_DATA_IDValue;
        public System.String TRACT_ALLOCATION_DATA_ID
        {
            get { return this.TRACT_ALLOCATION_DATA_IDValue; }
            set { SetProperty(ref TRACT_ALLOCATION_DATA_IDValue, value); }
        }

        private System.String TRACT_IDValue;
        public System.String TRACT_ID
        {
            get { return this.TRACT_IDValue; }
            set { SetProperty(ref TRACT_IDValue, value); }
        }

        private System.String UNIT_IDValue;
        public System.String UNIT_ID
        {
            get { return this.UNIT_IDValue; }
            set { SetProperty(ref UNIT_IDValue, value); }
        }

        private System.Decimal? TRACT_PARTICIPATIONValue;
        public System.Decimal? TRACT_PARTICIPATION
        {
            get { return this.TRACT_PARTICIPATIONValue; }
            set { SetProperty(ref TRACT_PARTICIPATIONValue, value); }
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


