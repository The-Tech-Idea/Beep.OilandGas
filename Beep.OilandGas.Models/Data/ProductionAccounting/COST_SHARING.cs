using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class COST_SHARING : ModelEntityBase {
        private System.String COST_SHARING_IDValue;
        public System.String COST_SHARING_ID
        {
            get { return this.COST_SHARING_IDValue; }
            set { SetProperty(ref COST_SHARING_IDValue, value); }
        }

        private System.String UNIT_OPERATING_AGREEMENT_IDValue;
        public System.String UNIT_OPERATING_AGREEMENT_ID
        {
            get { return this.UNIT_OPERATING_AGREEMENT_IDValue; }
            set { SetProperty(ref UNIT_OPERATING_AGREEMENT_IDValue, value); }
        }

        private System.String BASED_ON_WORKING_INTERESTValue;
        public System.String BASED_ON_WORKING_INTEREST
        {
            get { return this.BASED_ON_WORKING_INTERESTValue; }
            set { SetProperty(ref BASED_ON_WORKING_INTERESTValue, value); }
        }

        private System.String BASED_ON_TRACT_PARTICIPATIONValue;
        public System.String BASED_ON_TRACT_PARTICIPATION
        {
            get { return this.BASED_ON_TRACT_PARTICIPATIONValue; }
            set { SetProperty(ref BASED_ON_TRACT_PARTICIPATIONValue, value); }
        }

        private System.Decimal? OPERATOR_OVERHEAD_PERCENTAGEValue;
        public System.Decimal? OPERATOR_OVERHEAD_PERCENTAGE
        {
            get { return this.OPERATOR_OVERHEAD_PERCENTAGEValue; }
            set { SetProperty(ref OPERATOR_OVERHEAD_PERCENTAGEValue, value); }
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
