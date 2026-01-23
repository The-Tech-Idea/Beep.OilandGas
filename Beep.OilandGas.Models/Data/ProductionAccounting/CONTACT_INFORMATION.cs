using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CONTACT_INFORMATION : ModelEntityBase {
        private System.String CONTACT_INFORMATION_IDValue;
        public System.String CONTACT_INFORMATION_ID
        {
            get { return this.CONTACT_INFORMATION_IDValue; }
            set { SetProperty(ref CONTACT_INFORMATION_IDValue, value); }
        }

        private System.String OWNER_INFORMATION_IDValue;
        public System.String OWNER_INFORMATION_ID
        {
            get { return this.OWNER_INFORMATION_IDValue; }
            set { SetProperty(ref OWNER_INFORMATION_IDValue, value); }
        }

        private System.String PHONE_NUMBERValue;
        public System.String PHONE_NUMBER
        {
            get { return this.PHONE_NUMBERValue; }
            set { SetProperty(ref PHONE_NUMBERValue, value); }
        }

        private System.String EMAIL_ADDRESSValue;
        public System.String EMAIL_ADDRESS
        {
            get { return this.EMAIL_ADDRESSValue; }
            set { SetProperty(ref EMAIL_ADDRESSValue, value); }
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


