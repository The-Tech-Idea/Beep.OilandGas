using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ASSET_RETIREMENT_OBLIGATION : ModelEntityBase {
        private String ARO_IDValue;
        public String ARO_ID
        {
            get { return this.ARO_IDValue; }
            set { SetProperty(ref ARO_IDValue, value); }
        }

        private String FIELD_IDValue;
        public String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }
        private String WELL_IDValue;
        public String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private String FACILITY_IDValue;
        public String FACILITY_ID
        {
            get { return this.FACILITY_IDValue; }
            set { SetProperty(ref FACILITY_IDValue, value); }
        }

        private Decimal ESTIMATED_COSTValue;
        public Decimal ESTIMATED_COST
        {
            get { return this.ESTIMATED_COSTValue; }
            set { SetProperty(ref ESTIMATED_COSTValue, value); }
        }

        private DateTime? ESTIMATED_RETIREMENT_DATEValue;
        public DateTime? ESTIMATED_RETIREMENT_DATE
        {
            get { return this.ESTIMATED_RETIREMENT_DATEValue; }
            set { SetProperty(ref ESTIMATED_RETIREMENT_DATEValue, value); }
        }

        private Decimal  DISCOUNT_RATEValue;
        public Decimal  DISCOUNT_RATE
        {
            get { return this.DISCOUNT_RATEValue; }
            set { SetProperty(ref DISCOUNT_RATEValue, value); }
        }

        private Decimal  PRESENT_VALUEValue;
        public Decimal  PRESENT_VALUE
        {
            get { return this.PRESENT_VALUEValue; }
            set { SetProperty(ref PRESENT_VALUEValue, value); }
        }

        private Decimal  ACCRETION_EXPENSEValue;
        public Decimal  ACCRETION_EXPENSE
        {
            get { return this.ACCRETION_EXPENSEValue; }
            set { SetProperty(ref ACCRETION_EXPENSEValue, value); }
        }

        private String STATUSValue;
        public String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private String DESCRIPTIONValue;
        public String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        private String ROW_IDValue;
        public String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
