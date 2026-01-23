using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JOA_INTEREST : ModelEntityBase {
        private System.String JOA_INTEREST_IDValue;
        public System.String JOA_INTEREST_ID
        {
            get { return this.JOA_INTEREST_IDValue; }
            set { SetProperty(ref JOA_INTEREST_IDValue, value); }
        }

        private System.String JOA_IDValue;
        public System.String JOA_ID
        {
            get { return this.JOA_IDValue; }
            set { SetProperty(ref JOA_IDValue, value); }
        }

        private System.String INTEREST_OWNER_BA_IDValue;
        public System.String INTEREST_OWNER_BA_ID
        {
            get { return this.INTEREST_OWNER_BA_IDValue; }
            set { SetProperty(ref INTEREST_OWNER_BA_IDValue, value); }
        }

        private System.Decimal? WORKING_INTEREST_PERCENTAGEValue;
        public System.Decimal? WORKING_INTEREST_PERCENTAGE
        {
            get { return this.WORKING_INTEREST_PERCENTAGEValue; }
            set { SetProperty(ref WORKING_INTEREST_PERCENTAGEValue, value); }
        }

        private System.Decimal? REVENUE_INTEREST_PERCENTAGEValue;
        public System.Decimal? REVENUE_INTEREST_PERCENTAGE
        {
            get { return this.REVENUE_INTEREST_PERCENTAGEValue; }
            set { SetProperty(ref REVENUE_INTEREST_PERCENTAGEValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

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


