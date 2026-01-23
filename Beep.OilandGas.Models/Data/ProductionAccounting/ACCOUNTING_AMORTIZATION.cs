using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ACCOUNTING_AMORTIZATION : ModelEntityBase {
        private String ACCOUNTING_AMORTIZATION_IDValue;
        public String ACCOUNTING_AMORTIZATION_ID
        {
            get { return this.ACCOUNTING_AMORTIZATION_IDValue; }
            set { SetProperty(ref ACCOUNTING_AMORTIZATION_IDValue, value); }
        }

        private String PROPERTY_IDValue;
        public String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }
        private String WELL_IDValue;
        public String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private String POOL_IDValue;
        public String POOL_ID
        {
            get { return this.POOL_IDValue; }
            set { SetProperty(ref POOL_IDValue, value); }
        }

        private String FIELD_IDValue;
        public String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private DateTime? PERIOD_START_DATEValue;
        public DateTime? PERIOD_START_DATE
        {
            get { return this.PERIOD_START_DATEValue; }
            set { SetProperty(ref PERIOD_START_DATEValue, value); }
        }

        private DateTime? PERIOD_END_DATEValue;
        public DateTime? PERIOD_END_DATE
        {
            get { return this.PERIOD_END_DATEValue; }
            set { SetProperty(ref PERIOD_END_DATEValue, value); }
        }

        private Decimal CAPITALIZED_COSTValue;
        public Decimal CAPITALIZED_COST
        {
            get { return this.CAPITALIZED_COSTValue; }
            set { SetProperty(ref CAPITALIZED_COSTValue, value); }
        }

        private Decimal? PRODUCTION_BOEValue;
        public Decimal? PRODUCTION_BOE
        {
            get { return this.PRODUCTION_BOEValue; }
            set { SetProperty(ref PRODUCTION_BOEValue, value); }
        }

        private Decimal? TOTAL_RESERVES_BOEValue;
        public Decimal? TOTAL_RESERVES_BOE
        {
            get { return this.TOTAL_RESERVES_BOEValue; }
            set { SetProperty(ref TOTAL_RESERVES_BOEValue, value); }
        }

        private Decimal AMORTIZATION_AMOUNTValue;
        public Decimal AMORTIZATION_AMOUNT
        {
            get { return this.AMORTIZATION_AMOUNTValue; }
            set { SetProperty(ref AMORTIZATION_AMOUNTValue, value); }
        }

        private String AMORTIZATION_METHODValue;
        public String AMORTIZATION_METHOD
        {
            get { return this.AMORTIZATION_METHODValue; }
            set { SetProperty(ref AMORTIZATION_METHODValue, value); }
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


