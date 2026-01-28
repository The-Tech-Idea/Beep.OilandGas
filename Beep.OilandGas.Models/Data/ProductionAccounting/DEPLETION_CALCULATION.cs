using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class DEPLETION_CALCULATION : ModelEntityBase {
        private String DEPLETION_IDValue;
        public String DEPLETION_ID
        {
            get { return this.DEPLETION_IDValue; }
            set { SetProperty(ref DEPLETION_IDValue, value); }
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

        private String DEPLETION_METHODValue;
        public String DEPLETION_METHOD
        {
            get { return this.DEPLETION_METHODValue; }
            set { SetProperty(ref DEPLETION_METHODValue, value); }
        }

        private Decimal? COST_BASISValue;
        public Decimal? COST_BASIS
        {
            get { return this.COST_BASISValue; }
            set { SetProperty(ref COST_BASISValue, value); }
        }

        private Decimal? ESTIMATED_RESERVESValue;
        public Decimal? ESTIMATED_RESERVES
        {
            get { return this.ESTIMATED_RESERVESValue; }
            set { SetProperty(ref ESTIMATED_RESERVESValue, value); }
        }

        private Decimal? PRODUCTIONValue;
        public Decimal? PRODUCTION
        {
            get { return this.PRODUCTIONValue; }
            set { SetProperty(ref PRODUCTIONValue, value); }
        }

        private Decimal? DEPLETION_RATEValue;
        public Decimal? DEPLETION_RATE
        {
            get { return this.DEPLETION_RATEValue; }
            set { SetProperty(ref DEPLETION_RATEValue, value); }
        }

        private Decimal? DEPLETION_AMOUNTValue;
        public Decimal? DEPLETION_AMOUNT
        {
            get { return this.DEPLETION_AMOUNTValue; }
            set { SetProperty(ref DEPLETION_AMOUNTValue, value); }
        }

        private Decimal? PERCENTAGE_RATEValue;
        public Decimal? PERCENTAGE_RATE
        {
            get { return this.PERCENTAGE_RATEValue; }
            set { SetProperty(ref PERCENTAGE_RATEValue, value); }
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
