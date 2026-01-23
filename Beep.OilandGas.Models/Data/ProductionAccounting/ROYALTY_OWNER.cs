using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ROYALTY_OWNER : ModelEntityBase {
        private System.String ROYALTY_OWNER_IDValue;
        public System.String ROYALTY_OWNER_ID
        {
            get { return this.ROYALTY_OWNER_IDValue; }
            set { SetProperty(ref ROYALTY_OWNER_IDValue, value); }
        }

        private System.String ROYALTY_OWNER_BA_IDValue;
        public System.String ROYALTY_OWNER_BA_ID
        {
            get { return this.ROYALTY_OWNER_BA_IDValue; }
            set { SetProperty(ref ROYALTY_OWNER_BA_IDValue, value); }
        }

        private System.String ROYALTY_OWNER_NAMEValue;
        public System.String ROYALTY_OWNER_NAME
        {
            get { return this.ROYALTY_OWNER_NAMEValue; }
            set { SetProperty(ref ROYALTY_OWNER_NAMEValue, value); }
        }

        private System.String CONTACT_INFOValue;
        public System.String CONTACT_INFO
        {
            get { return this.CONTACT_INFOValue; }
            set { SetProperty(ref CONTACT_INFOValue, value); }
        }

        private System.String PAYMENT_METHODValue;
        public System.String PAYMENT_METHOD
        {
            get { return this.PAYMENT_METHODValue; }
            set { SetProperty(ref PAYMENT_METHODValue, value); }
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


