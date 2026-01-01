using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    public partial class FLASH_COMPONENT : Entity, IPPDMEntity
    {
        private String FLASH_COMPONENT_IDValue;
        public String FLASH_COMPONENT_ID
        {
            get { return this.FLASH_COMPONENT_IDValue; }
            set { SetProperty(ref FLASH_COMPONENT_IDValue, value); }
        }

        private String FLASH_CALCULATION_IDValue;
        public String FLASH_CALCULATION_ID
        {
            get { return this.FLASH_CALCULATION_IDValue; }
            set { SetProperty(ref FLASH_CALCULATION_IDValue, value); }
        }

        private String COMPONENT_NAMEValue;
        public String COMPONENT_NAME
        {
            get { return this.COMPONENT_NAMEValue; }
            set { SetProperty(ref COMPONENT_NAMEValue, value); }
        }

        private Decimal? MOLE_FRACTIONValue;
        public Decimal? MOLE_FRACTION
        {
            get { return this.MOLE_FRACTIONValue; }
            set { SetProperty(ref MOLE_FRACTIONValue, value); }
        }

        private Decimal? CRITICAL_TEMPERATUREValue;
        public Decimal? CRITICAL_TEMPERATURE
        {
            get { return this.CRITICAL_TEMPERATUREValue; }
            set { SetProperty(ref CRITICAL_TEMPERATUREValue, value); }
        }

        private Decimal? CRITICAL_PRESSUREValue;
        public Decimal? CRITICAL_PRESSURE
        {
            get { return this.CRITICAL_PRESSUREValue; }
            set { SetProperty(ref CRITICAL_PRESSUREValue, value); }
        }

        private Decimal? ACENTRIC_FACTORValue;
        public Decimal? ACENTRIC_FACTOR
        {
            get { return this.ACENTRIC_FACTORValue; }
            set { SetProperty(ref ACENTRIC_FACTORValue, value); }
        }

        private Decimal? MOLECULAR_WEIGHTValue;
        public Decimal? MOLECULAR_WEIGHT
        {
            get { return this.MOLECULAR_WEIGHTValue; }
            set { SetProperty(ref MOLECULAR_WEIGHTValue, value); }
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
