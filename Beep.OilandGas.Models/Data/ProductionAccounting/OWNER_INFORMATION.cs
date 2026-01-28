using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class OWNER_INFORMATION : ModelEntityBase {
        private System.String OWNER_INFORMATION_IDValue;
        public System.String OWNER_INFORMATION_ID
        {
            get { return this.OWNER_INFORMATION_IDValue; }
            set { SetProperty(ref OWNER_INFORMATION_IDValue, value); }
        }

        private System.String OWNER_NAMEValue;
        public System.String OWNER_NAME
        {
            get { return this.OWNER_NAMEValue; }
            set { SetProperty(ref OWNER_NAMEValue, value); }
        }

        private System.String TAX_IDValue;
        public System.String TAX_ID
        {
            get { return this.TAX_IDValue; }
            set { SetProperty(ref TAX_IDValue, value); }
        }

        private System.String ADDRESS_IDValue;
        public System.String ADDRESS_ID
        {
            get { return this.ADDRESS_IDValue; }
            set { SetProperty(ref ADDRESS_IDValue, value); }
        }

        private System.String CONTACT_INFORMATION_IDValue;
        public System.String CONTACT_INFORMATION_ID
        {
            get { return this.CONTACT_INFORMATION_IDValue; }
            set { SetProperty(ref CONTACT_INFORMATION_IDValue, value); }
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
