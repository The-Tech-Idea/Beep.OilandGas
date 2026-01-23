using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Royalty
{
    public partial class ROYALTY_DISPUTE : ModelEntityBase
    {
        private System.String ROYALTY_DISPUTE_IDValue;
        public System.String ROYALTY_DISPUTE_ID
        {
            get { return this.ROYALTY_DISPUTE_IDValue; }
            set { SetProperty(ref ROYALTY_DISPUTE_IDValue, value); }
        }

        private System.String ROYALTY_STATEMENT_IDValue;
        public System.String ROYALTY_STATEMENT_ID
        {
            get { return this.ROYALTY_STATEMENT_IDValue; }
            set { SetProperty(ref ROYALTY_STATEMENT_IDValue, value); }
        }

        private System.String ROYALTY_OWNER_BA_IDValue;
        public System.String ROYALTY_OWNER_BA_ID
        {
            get { return this.ROYALTY_OWNER_BA_IDValue; }
            set { SetProperty(ref ROYALTY_OWNER_BA_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.DateTime? DISPUTE_DATEValue;
        public System.DateTime? DISPUTE_DATE
        {
            get { return this.DISPUTE_DATEValue; }
            set { SetProperty(ref DISPUTE_DATEValue, value); }
        }

        private System.String DISPUTE_REASONValue;
        public System.String DISPUTE_REASON
        {
            get { return this.DISPUTE_REASONValue; }
            set { SetProperty(ref DISPUTE_REASONValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.DateTime? RESOLUTION_DATEValue;
        public System.DateTime? RESOLUTION_DATE
        {
            get { return this.RESOLUTION_DATEValue; }
            set { SetProperty(ref RESOLUTION_DATEValue, value); }
        }

        private System.String RESOLUTION_NOTESValue;
        public System.String RESOLUTION_NOTES
        {
            get { return this.RESOLUTION_NOTESValue; }
            set { SetProperty(ref RESOLUTION_NOTESValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}


