using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class UNIT_AGREEMENT : ModelEntityBase {
      
      

       

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

        private string UNIT_IDValue;
        public string UNIT_ID
        {
            get { return this.UNIT_IDValue; }
            set { SetProperty(ref UNIT_IDValue, value); }
        }

        private string UNIT_NAMEValue;
        public string UNIT_NAME
        {
            get { return this.UNIT_NAMEValue; }
            set { SetProperty(ref UNIT_NAMEValue, value); }
        }

        private DateTime EFFECTIVE_DATEValue;
        public DateTime EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRATION_DATEValue;
        public DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private string UNIT_OPERATORValue;
        public string UNIT_OPERATOR
        {
            get { return this.UNIT_OPERATORValue; }
            set { SetProperty(ref UNIT_OPERATORValue, value); }
        }

        private PARTICIPATING_AREA PARTICIPATING_AREAValue;
        public PARTICIPATING_AREA PARTICIPATING_AREA
        {
            get { return this.PARTICIPATING_AREAValue; }
            set { SetProperty(ref PARTICIPATING_AREAValue, value); }
        }

        private string? TERMS_AND_CONDITIONSValue;
        public string? TERMS_AND_CONDITIONS
        {
            get { return this.TERMS_AND_CONDITIONSValue; }
            set { SetProperty(ref TERMS_AND_CONDITIONSValue, value); }
        }
    }
}
