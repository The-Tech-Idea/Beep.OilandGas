using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class UNIT_PARTICIPANT : ModelEntityBase {
        private System.String UNIT_PARTICIPANT_IDValue;
        public System.String UNIT_PARTICIPANT_ID
        {
            get { return this.UNIT_PARTICIPANT_IDValue; }
            set { SetProperty(ref UNIT_PARTICIPANT_IDValue, value); }
        }

        private System.String UNIT_OPERATING_AGREEMENT_IDValue;
        public System.String UNIT_OPERATING_AGREEMENT_ID
        {
            get { return this.UNIT_OPERATING_AGREEMENT_IDValue; }
            set { SetProperty(ref UNIT_OPERATING_AGREEMENT_IDValue, value); }
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

        private string PARTICIPANT_IDValue;
        public string PARTICIPANT_ID
        {
            get { return this.PARTICIPANT_IDValue; }
            set { SetProperty(ref PARTICIPANT_IDValue, value); }
        }

        private string COMPANY_NAMEValue;
        public string COMPANY_NAME
        {
            get { return this.COMPANY_NAMEValue; }
            set { SetProperty(ref COMPANY_NAMEValue, value); }
        }

        private decimal WORKING_INTERESTValue;
        public decimal WORKING_INTEREST
        {
            get { return this.WORKING_INTERESTValue; }
            set { SetProperty(ref WORKING_INTERESTValue, value); }
        }

        private decimal NET_REVENUE_INTERESTValue;
        public decimal NET_REVENUE_INTEREST
        {
            get { return this.NET_REVENUE_INTERESTValue; }
            set { SetProperty(ref NET_REVENUE_INTERESTValue, value); }
        }

        private bool IS_OPERATORValue;
        public bool IS_OPERATOR
        {
            get { return this.IS_OPERATORValue; }
            set { SetProperty(ref IS_OPERATORValue, value); }
        }

        private decimal VOTING_PERCENTAGEValue;
        public decimal VOTING_PERCENTAGE
        {
            get { return this.VOTING_PERCENTAGEValue; }
            set { SetProperty(ref VOTING_PERCENTAGEValue, value); }
        }
    }
}
