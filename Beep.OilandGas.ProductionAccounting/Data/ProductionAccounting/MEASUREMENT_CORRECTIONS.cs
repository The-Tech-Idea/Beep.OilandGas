using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class MEASUREMENT_CORRECTIONS : ModelEntityBase {
        private System.String MEASUREMENT_CORRECTIONS_IDValue;
        public System.String MEASUREMENT_CORRECTIONS_ID
        {
            get { return this.MEASUREMENT_CORRECTIONS_IDValue; }
            set { SetProperty(ref MEASUREMENT_CORRECTIONS_IDValue, value); }
        }

        private System.String MEASUREMENT_RECORD_IDValue;
        public System.String MEASUREMENT_RECORD_ID
        {
            get { return this.MEASUREMENT_RECORD_IDValue; }
            set { SetProperty(ref MEASUREMENT_RECORD_IDValue, value); }
        }

        private System.Decimal  TEMPERATURE_CORRECTION_FACTORValue;
        public System.Decimal  TEMPERATURE_CORRECTION_FACTOR
        {
            get { return this.TEMPERATURE_CORRECTION_FACTORValue; }
            set { SetProperty(ref TEMPERATURE_CORRECTION_FACTORValue, value); }
        }

        private System.Decimal  PRESSURE_CORRECTION_FACTORValue;
        public System.Decimal  PRESSURE_CORRECTION_FACTOR
        {
            get { return this.PRESSURE_CORRECTION_FACTORValue; }
            set { SetProperty(ref PRESSURE_CORRECTION_FACTORValue, value); }
        }

        private System.Decimal  METER_FACTORValue;
        public System.Decimal  METER_FACTOR
        {
            get { return this.METER_FACTORValue; }
            set { SetProperty(ref METER_FACTORValue, value); }
        }

        private System.Decimal  SHRINKAGE_FACTORValue;
        public System.Decimal  SHRINKAGE_FACTOR
        {
            get { return this.SHRINKAGE_FACTORValue; }
            set { SetProperty(ref SHRINKAGE_FACTORValue, value); }
        }

        // Standard PPDM columns

  

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }


       
    }
}
