using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public partial class SUCKER_ROD_LOAD_RESULT : Entity, IPPDMEntity
    {
        private String SUCKER_ROD_LOAD_RESULT_IDValue;
        public String SUCKER_ROD_LOAD_RESULT_ID
        {
            get { return this.SUCKER_ROD_LOAD_RESULT_IDValue; }
            set { SetProperty(ref SUCKER_ROD_LOAD_RESULT_IDValue, value); }
        }

        private String SUCKER_ROD_SYSTEM_PROPERTIES_IDValue;
        public String SUCKER_ROD_SYSTEM_PROPERTIES_ID
        {
            get { return this.SUCKER_ROD_SYSTEM_PROPERTIES_IDValue; }
            set { SetProperty(ref SUCKER_ROD_SYSTEM_PROPERTIES_IDValue, value); }
        }

        private Decimal? PEAK_LOADValue;
        public Decimal? PEAK_LOAD
        {
            get { return this.PEAK_LOADValue; }
            set { SetProperty(ref PEAK_LOADValue, value); }
        }

        private Decimal? MINIMUM_LOADValue;
        public Decimal? MINIMUM_LOAD
        {
            get { return this.MINIMUM_LOADValue; }
            set { SetProperty(ref MINIMUM_LOADValue, value); }
        }

        private Decimal? POLISHED_ROD_LOADValue;
        public Decimal? POLISHED_ROD_LOAD
        {
            get { return this.POLISHED_ROD_LOADValue; }
            set { SetProperty(ref POLISHED_ROD_LOADValue, value); }
        }

        private Decimal? ROD_STRING_WEIGHTValue;
        public Decimal? ROD_STRING_WEIGHT
        {
            get { return this.ROD_STRING_WEIGHTValue; }
            set { SetProperty(ref ROD_STRING_WEIGHTValue, value); }
        }

        private Decimal? FLUID_LOADValue;
        public Decimal? FLUID_LOAD
        {
            get { return this.FLUID_LOADValue; }
            set { SetProperty(ref FLUID_LOADValue, value); }
        }

        private Decimal? DYNAMIC_LOADValue;
        public Decimal? DYNAMIC_LOAD
        {
            get { return this.DYNAMIC_LOADValue; }
            set { SetProperty(ref DYNAMIC_LOADValue, value); }
        }

        private Decimal? LOAD_RANGEValue;
        public Decimal? LOAD_RANGE
        {
            get { return this.LOAD_RANGEValue; }
            set { SetProperty(ref LOAD_RANGEValue, value); }
        }

        private Decimal? STRESS_RANGEValue;
        public Decimal? STRESS_RANGE
        {
            get { return this.STRESS_RANGEValue; }
            set { SetProperty(ref STRESS_RANGEValue, value); }
        }

        private Decimal? MAXIMUM_STRESSValue;
        public Decimal? MAXIMUM_STRESS
        {
            get { return this.MAXIMUM_STRESSValue; }
            set { SetProperty(ref MAXIMUM_STRESSValue, value); }
        }

        private Decimal? LOAD_FACTORValue;
        public Decimal? LOAD_FACTOR
        {
            get { return this.LOAD_FACTORValue; }
            set { SetProperty(ref LOAD_FACTORValue, value); }
        }

        // Standard PPDM columns
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }
    }
}
