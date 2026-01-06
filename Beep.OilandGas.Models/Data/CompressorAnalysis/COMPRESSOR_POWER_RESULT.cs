using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    public partial class COMPRESSOR_POWER_RESULT : Entity, IPPDMEntity
    {
        private String COMPRESSOR_POWER_RESULT_IDValue;
        public String COMPRESSOR_POWER_RESULT_ID
        {
            get { return this.COMPRESSOR_POWER_RESULT_IDValue; }
            set { SetProperty(ref COMPRESSOR_POWER_RESULT_IDValue, value); }
        }

        private String COMPRESSOR_OPERATING_CONDITIONS_IDValue;
        public String COMPRESSOR_OPERATING_CONDITIONS_ID
        {
            get { return this.COMPRESSOR_OPERATING_CONDITIONS_IDValue; }
            set { SetProperty(ref COMPRESSOR_OPERATING_CONDITIONS_IDValue, value); }
        }

        private Decimal? THEORETICAL_POWERValue;
        public Decimal? THEORETICAL_POWER
        {
            get { return this.THEORETICAL_POWERValue; }
            set { SetProperty(ref THEORETICAL_POWERValue, value); }
        }

        private Decimal? BRAKE_HORSEPOWERValue;
        public Decimal? BRAKE_HORSEPOWER
        {
            get { return this.BRAKE_HORSEPOWERValue; }
            set { SetProperty(ref BRAKE_HORSEPOWERValue, value); }
        }

        private Decimal? MOTOR_HORSEPOWERValue;
        public Decimal? MOTOR_HORSEPOWER
        {
            get { return this.MOTOR_HORSEPOWERValue; }
            set { SetProperty(ref MOTOR_HORSEPOWERValue, value); }
        }

        private Decimal? POWER_CONSUMPTION_KWValue;
        public Decimal? POWER_CONSUMPTION_KW
        {
            get { return this.POWER_CONSUMPTION_KWValue; }
            set { SetProperty(ref POWER_CONSUMPTION_KWValue, value); }
        }

        private Decimal? COMPRESSION_RATIOValue;
        public Decimal? COMPRESSION_RATIO
        {
            get { return this.COMPRESSION_RATIOValue; }
            set { SetProperty(ref COMPRESSION_RATIOValue, value); }
        }

        private Decimal? POLYTROPIC_HEADValue;
        public Decimal? POLYTROPIC_HEAD
        {
            get { return this.POLYTROPIC_HEADValue; }
            set { SetProperty(ref POLYTROPIC_HEADValue, value); }
        }

        private Decimal? ADIABATIC_HEADValue;
        public Decimal? ADIABATIC_HEAD
        {
            get { return this.ADIABATIC_HEADValue; }
            set { SetProperty(ref ADIABATIC_HEADValue, value); }
        }

        private Decimal? DISCHARGE_TEMPERATUREValue;
        public Decimal? DISCHARGE_TEMPERATURE
        {
            get { return this.DISCHARGE_TEMPERATUREValue; }
            set { SetProperty(ref DISCHARGE_TEMPERATUREValue, value); }
        }

        private Decimal? OVERALL_EFFICIENCYValue;
        public Decimal? OVERALL_EFFICIENCY
        {
            get { return this.OVERALL_EFFICIENCYValue; }
            set { SetProperty(ref OVERALL_EFFICIENCYValue, value); }
        }

        private Decimal? POLYTROPIC_EFFICIENCYValue;
        public Decimal? POLYTROPIC_EFFICIENCY
        {
            get { return this.POLYTROPIC_EFFICIENCYValue; }
            set { SetProperty(ref POLYTROPIC_EFFICIENCYValue, value); }
        }

        private Decimal? ADIABATIC_EFFICIENCYValue;
        public Decimal? ADIABATIC_EFFICIENCY
        {
            get { return this.ADIABATIC_EFFICIENCYValue; }
            set { SetProperty(ref ADIABATIC_EFFICIENCYValue, value); }
        }

        private Decimal? CYLINDER_DISPLACEMENTValue;
        public Decimal? CYLINDER_DISPLACEMENT
        {
            get { return this.CYLINDER_DISPLACEMENTValue; }
            set { SetProperty(ref CYLINDER_DISPLACEMENTValue, value); }
        }

        private Decimal? VOLUMETRIC_EFFICIENCYValue;
        public Decimal? VOLUMETRIC_EFFICIENCY
        {
            get { return this.VOLUMETRIC_EFFICIENCYValue; }
            set { SetProperty(ref VOLUMETRIC_EFFICIENCYValue, value); }
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



