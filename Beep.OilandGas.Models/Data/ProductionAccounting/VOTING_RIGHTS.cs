using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class VOTING_RIGHTS : ModelEntityBase {
        private System.String VOTING_RIGHTS_IDValue;
        public System.String VOTING_RIGHTS_ID
        {
            get { return this.VOTING_RIGHTS_IDValue; }
            set { SetProperty(ref VOTING_RIGHTS_IDValue, value); }
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

        private System.Decimal? MINIMUM_VOTING_THRESHOLDValue;
        public System.Decimal? MINIMUM_VOTING_THRESHOLD
        {
            get { return this.MINIMUM_VOTING_THRESHOLDValue; }
            set { SetProperty(ref MINIMUM_VOTING_THRESHOLDValue, value); }
        }

        private System.String UNANIMOUS_CONSENT_REQUIREDValue;
        public System.String UNANIMOUS_CONSENT_REQUIRED
        {
            get { return this.UNANIMOUS_CONSENT_REQUIREDValue; }
            set { SetProperty(ref UNANIMOUS_CONSENT_REQUIREDValue, value); }
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
