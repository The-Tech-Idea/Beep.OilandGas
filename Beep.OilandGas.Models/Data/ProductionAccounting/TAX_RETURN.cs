using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TAX_RETURN : ModelEntityBase {
        private System.String TAX_RETURN_IDValue;
        public System.String TAX_RETURN_ID
        {
            get { return this.TAX_RETURN_IDValue; }
            set { SetProperty(ref TAX_RETURN_IDValue, value); }
        }

        private System.String RETURN_NUMBERValue;
        public System.String RETURN_NUMBER
        {
            get { return this.RETURN_NUMBERValue; }
            set { SetProperty(ref RETURN_NUMBERValue, value); }
        }

        private System.String TAX_TYPEValue;
        public System.String TAX_TYPE
        {
            get { return this.TAX_TYPEValue; }
            set { SetProperty(ref TAX_TYPEValue, value); }
        }

        private System.DateTime? RETURN_PERIOD_STARTValue;
        public System.DateTime? RETURN_PERIOD_START
        {
            get { return this.RETURN_PERIOD_STARTValue; }
            set { SetProperty(ref RETURN_PERIOD_STARTValue, value); }
        }

        private System.DateTime? RETURN_PERIOD_ENDValue;
        public System.DateTime? RETURN_PERIOD_END
        {
            get { return this.RETURN_PERIOD_ENDValue; }
            set { SetProperty(ref RETURN_PERIOD_ENDValue, value); }
        }

        private System.DateTime? FILING_DATEValue;
        public System.DateTime? FILING_DATE
        {
            get { return this.FILING_DATEValue; }
            set { SetProperty(ref FILING_DATEValue, value); }
        }

        private System.Decimal? TOTAL_TAX_LIABILITYValue;
        public System.Decimal? TOTAL_TAX_LIABILITY
        {
            get { return this.TOTAL_TAX_LIABILITYValue; }
            set { SetProperty(ref TOTAL_TAX_LIABILITYValue, value); }
        }

        private System.Decimal? TAX_PAIDValue;
        public System.Decimal? TAX_PAID
        {
            get { return this.TAX_PAIDValue; }
            set { SetProperty(ref TAX_PAIDValue, value); }
        }

        private System.Decimal? TAX_DUEValue;
        public System.Decimal? TAX_DUE
        {
            get { return this.TAX_DUEValue; }
            set { SetProperty(ref TAX_DUEValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
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


