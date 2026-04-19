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

   
        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

        private string OPERATING_AGREEMENT_IDValue;
        public string OPERATING_AGREEMENT_ID
        {
            get { return this.OPERATING_AGREEMENT_IDValue; }
            set { SetProperty(ref OPERATING_AGREEMENT_IDValue, value); }
        }

      

        private VOTING_RIGHTS VOTING_RIGHTSValue;
        public VOTING_RIGHTS VOTING_RIGHTS
        {
            get { return this.VOTING_RIGHTSValue; }
            set { SetProperty(ref VOTING_RIGHTSValue, value); }
        }

        private COST_SHARING COST_SHARINGValue;
        public COST_SHARING COST_SHARING
        {
            get { return this.COST_SHARINGValue; }
            set { SetProperty(ref COST_SHARINGValue, value); }
        }

        private REVENUE_SHARING REVENUE_SHARINGValue;
        public REVENUE_SHARING REVENUE_SHARING
        {
            get { return this.REVENUE_SHARINGValue; }
            set { SetProperty(ref REVENUE_SHARINGValue, value); }
        }
    }
}
