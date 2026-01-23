using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class GL_ACCOUNT : ModelEntityBase {
        private System.String GL_ACCOUNT_IDValue;
        public System.String GL_ACCOUNT_ID
        {
            get { return this.GL_ACCOUNT_IDValue; }
            set { SetProperty(ref GL_ACCOUNT_IDValue, value); }
        }

        private System.String ACCOUNT_NUMBERValue;
        public System.String ACCOUNT_NUMBER
        {
            get { return this.ACCOUNT_NUMBERValue; }
            set { SetProperty(ref ACCOUNT_NUMBERValue, value); }
        }

        private System.String ACCOUNT_NAMEValue;
        public System.String ACCOUNT_NAME
        {
            get { return this.ACCOUNT_NAMEValue; }
            set { SetProperty(ref ACCOUNT_NAMEValue, value); }
        }

        private System.String ACCOUNT_TYPEValue;
        public System.String ACCOUNT_TYPE
        {
            get { return this.ACCOUNT_TYPEValue; }
            set { SetProperty(ref ACCOUNT_TYPEValue, value); }
        }

        private System.String PARENT_ACCOUNT_IDValue;
        public System.String PARENT_ACCOUNT_ID
        {
            get { return this.PARENT_ACCOUNT_IDValue; }
            set { SetProperty(ref PARENT_ACCOUNT_IDValue, value); }
        }

        private System.String NORMAL_BALANCEValue;
        public System.String NORMAL_BALANCE
        {
            get { return this.NORMAL_BALANCEValue; }
            set { SetProperty(ref NORMAL_BALANCEValue, value); }
        }

        private System.Decimal? OPENING_BALANCEValue;
        public System.Decimal? OPENING_BALANCE
        {
            get { return this.OPENING_BALANCEValue; }
            set { SetProperty(ref OPENING_BALANCEValue, value); }
        }

        private System.Decimal? CURRENT_BALANCEValue;
        public System.Decimal? CURRENT_BALANCE
        {
            get { return this.CURRENT_BALANCEValue; }
            set { SetProperty(ref CURRENT_BALANCEValue, value); }
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


