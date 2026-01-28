using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JIB_CHARGE : ModelEntityBase {
        private System.String JIB_CHARGE_IDValue;
        public System.String JIB_CHARGE_ID
        {
            get { return this.JIB_CHARGE_IDValue; }
            set { SetProperty(ref JIB_CHARGE_IDValue, value); }
        }

        private System.String JOINT_INTEREST_STATEMENT_IDValue;
        public System.String JOINT_INTEREST_STATEMENT_ID
        {
            get { return this.JOINT_INTEREST_STATEMENT_IDValue; }
            set { SetProperty(ref JOINT_INTEREST_STATEMENT_IDValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        private System.Decimal? AMOUNTValue;
        public System.Decimal? AMOUNT
        {
            get { return this.AMOUNTValue; }
            set { SetProperty(ref AMOUNTValue, value); }
        }

        private System.String CATEGORYValue;
        public System.String CATEGORY
        {
            get { return this.CATEGORYValue; }
            set { SetProperty(ref CATEGORYValue, value); }
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

        private string CHARGE_IDValue;
        public string CHARGE_ID
        {
            get { return this.CHARGE_IDValue; }
            set { SetProperty(ref CHARGE_IDValue, value); }
        }

        
    }
}
