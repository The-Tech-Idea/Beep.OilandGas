using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class UNIT_OPERATING_AGREEMENT : ModelEntityBase {
        private System.String UNIT_OPERATING_AGREEMENT_IDValue;
        public System.String UNIT_OPERATING_AGREEMENT_ID
        {
            get { return this.UNIT_OPERATING_AGREEMENT_IDValue; }
            set { SetProperty(ref UNIT_OPERATING_AGREEMENT_IDValue, value); }
        }

        private System.String UNIT_IDValue;
        public System.String UNIT_ID
        {
            get { return this.UNIT_IDValue; }
            set { SetProperty(ref UNIT_IDValue, value); }
        }

        private System.String VOTING_RIGHTS_IDValue;
        public System.String VOTING_RIGHTS_ID
        {
            get { return this.VOTING_RIGHTS_IDValue; }
            set { SetProperty(ref VOTING_RIGHTS_IDValue, value); }
        }

        private System.String COST_SHARING_IDValue;
        public System.String COST_SHARING_ID
        {
            get { return this.COST_SHARING_IDValue; }
            set { SetProperty(ref COST_SHARING_IDValue, value); }
        }

        private System.String REVENUE_SHARING_IDValue;
        public System.String REVENUE_SHARING_ID
        {
            get { return this.REVENUE_SHARING_IDValue; }
            set { SetProperty(ref REVENUE_SHARING_IDValue, value); }
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


