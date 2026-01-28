using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public partial class EXPLORATION_BUDGET : ModelEntityBase
    {
        private System.String PROGRAM_IDValue;
        public System.String PROGRAM_ID
        {
            get
            {
                return this.PROGRAM_IDValue;
            }
            set { SetProperty(ref PROGRAM_IDValue, value); }
        }
        private System.Decimal BUDGET_YEARValue;
        public System.Decimal BUDGET_YEAR
        {
            get
            {
                return this.BUDGET_YEARValue;
            }
            set { SetProperty(ref BUDGET_YEARValue, value); }
        }
        private System.Decimal? BUDGET_AMOUNTValue;
        public System.Decimal? BUDGET_AMOUNT
        {
            get
            {
                return this.BUDGET_AMOUNTValue;
            }
            set { SetProperty(ref BUDGET_AMOUNTValue, value); }
        }
        private System.String BUDGET_CURRENCYValue;
        public System.String BUDGET_CURRENCY
        {
            get
            {
                return this.BUDGET_CURRENCYValue;
            }
            set { SetProperty(ref BUDGET_CURRENCYValue, value); }
        }
        private System.String BUDGET_CATEGORYValue;
        public System.String BUDGET_CATEGORY
        {
            get
            {
                return this.BUDGET_CATEGORYValue;
            }
            set { SetProperty(ref BUDGET_CATEGORYValue, value); }
        }
        private System.Decimal? ACTUAL_AMOUNTValue;
        public System.Decimal? ACTUAL_AMOUNT
        {
            get
            {
                return this.ACTUAL_AMOUNTValue;
            }
            set { SetProperty(ref ACTUAL_AMOUNTValue, value); }
        }
        private System.Decimal? VARIANCEValue;
        public System.Decimal? VARIANCE
        {
            get
            {
                return this.VARIANCEValue;
            }
            set { SetProperty(ref VARIANCEValue, value); }
        }
        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get
            {
                return this.DESCRIPTIONValue;
            }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }
        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

        private System.String REMARKValue;

        private System.String SOURCEValue;

        public EXPLORATION_BUDGET() { }
    }
}
