using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JIB_PARTICIPANT : ModelEntityBase {
        private System.String JIB_PARTICIPANT_IDValue;
        public System.String JIB_PARTICIPANT_ID
        {
            get { return this.JIB_PARTICIPANT_IDValue; }
            set { SetProperty(ref JIB_PARTICIPANT_IDValue, value); }
        }

        private System.String JOINT_INTEREST_STATEMENT_IDValue;
        public System.String JOINT_INTEREST_STATEMENT_ID
        {
            get { return this.JOINT_INTEREST_STATEMENT_IDValue; }
            set { SetProperty(ref JOINT_INTEREST_STATEMENT_IDValue, value); }
        }

        private System.String COMPANY_NAMEValue;
        public System.String COMPANY_NAME
        {
            get { return this.COMPANY_NAMEValue; }
            set { SetProperty(ref COMPANY_NAMEValue, value); }
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

     
    }
}
