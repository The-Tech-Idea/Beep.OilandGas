using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JOURNAL_ENTRY : ModelEntityBase {
        private System.String JOURNAL_ENTRY_IDValue;
        public System.String JOURNAL_ENTRY_ID
        {
            get { return this.JOURNAL_ENTRY_IDValue; }
            set { SetProperty(ref JOURNAL_ENTRY_IDValue, value); }
        }

        private System.String ENTRY_NUMBERValue;
        public System.String ENTRY_NUMBER
        {
            get { return this.ENTRY_NUMBERValue; }
            set { SetProperty(ref ENTRY_NUMBERValue, value); }
        }

        private System.DateTime? ENTRY_DATEValue;
        public System.DateTime? ENTRY_DATE
        {
            get { return this.ENTRY_DATEValue; }
            set { SetProperty(ref ENTRY_DATEValue, value); }
        }

        private System.String ENTRY_TYPEValue;
        public System.String ENTRY_TYPE
        {
            get { return this.ENTRY_TYPEValue; }
            set { SetProperty(ref ENTRY_TYPEValue, value); }
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

        private System.String REFERENCE_NUMBERValue;
        public System.String REFERENCE_NUMBER
        {
            get { return this.REFERENCE_NUMBERValue; }
            set { SetProperty(ref REFERENCE_NUMBERValue, value); }
        }

        private System.String SOURCE_MODULEValue;
        public System.String SOURCE_MODULE
        {
            get { return this.SOURCE_MODULEValue; }
            set { SetProperty(ref SOURCE_MODULEValue, value); }
        }

        private System.Decimal? TOTAL_DEBITValue;
        public System.Decimal? TOTAL_DEBIT
        {
            get { return this.TOTAL_DEBITValue; }
            set { SetProperty(ref TOTAL_DEBITValue, value); }
        }

        private System.Decimal? TOTAL_CREDITValue;
        public System.Decimal? TOTAL_CREDIT
        {
            get { return this.TOTAL_CREDITValue; }
            set { SetProperty(ref TOTAL_CREDITValue, value); }
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


