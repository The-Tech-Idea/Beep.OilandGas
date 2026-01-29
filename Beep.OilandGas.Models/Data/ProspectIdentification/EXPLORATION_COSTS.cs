using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public partial class EXPLORATION_COSTS : ModelEntityBase
    {
        private System.String EXPLORATION_COST_IDValue;
        public System.String EXPLORATION_COST_ID
        {
            get { return this.EXPLORATION_COST_IDValue; }
            set { SetProperty(ref EXPLORATION_COST_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.Decimal  GEOLOGICAL_GEOPHYSICAL_COSTSValue;
        public System.Decimal  GEOLOGICAL_GEOPHYSICAL_COSTS
        {
            get { return this.GEOLOGICAL_GEOPHYSICAL_COSTSValue; }
            set { SetProperty(ref GEOLOGICAL_GEOPHYSICAL_COSTSValue, value); }
        }

        private System.Decimal  EXPLORATORY_DRILLING_COSTSValue;
        public System.Decimal  EXPLORATORY_DRILLING_COSTS
        {
            get { return this.EXPLORATORY_DRILLING_COSTSValue; }
            set { SetProperty(ref EXPLORATORY_DRILLING_COSTSValue, value); }
        }

        private System.Decimal  EXPLORATORY_WELL_EQUIPMENTValue;
        public System.Decimal  EXPLORATORY_WELL_EQUIPMENT
        {
            get { return this.EXPLORATORY_WELL_EQUIPMENTValue; }
            set { SetProperty(ref EXPLORATORY_WELL_EQUIPMENTValue, value); }
        }

        private System.Decimal  TOTAL_EXPLORATION_COSTSValue;
        public System.Decimal  TOTAL_EXPLORATION_COSTS
        {
            get { return this.TOTAL_EXPLORATION_COSTSValue; }
            set { SetProperty(ref TOTAL_EXPLORATION_COSTSValue, value); }
        }

        private System.DateTime? COST_DATEValue;
        public System.DateTime? COST_DATE
        {
            get { return this.COST_DATEValue; }
            set { SetProperty(ref COST_DATEValue, value); }
        }

        private System.String WELL_IDValue;
        public System.String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private System.String IS_DRY_HOLEValue;
        public System.String IS_DRY_HOLE
        {
            get { return this.IS_DRY_HOLEValue; }
            set { SetProperty(ref IS_DRY_HOLEValue, value); }
        }

        private System.String FOUND_PROVED_RESERVESValue;
        public System.String FOUND_PROVED_RESERVES
        {
            get { return this.FOUND_PROVED_RESERVESValue; }
            set { SetProperty(ref FOUND_PROVED_RESERVESValue, value); }
        }

        private System.String IS_DEFERRED_CLASSIFICATIONValue;
        public System.String IS_DEFERRED_CLASSIFICATION
        {
            get { return this.IS_DEFERRED_CLASSIFICATIONValue; }
            set { SetProperty(ref IS_DEFERRED_CLASSIFICATIONValue, value); }
        }

        private System.String COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
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
