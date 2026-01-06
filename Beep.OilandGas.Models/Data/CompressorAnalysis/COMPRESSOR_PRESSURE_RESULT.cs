using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    public partial class COMPRESSOR_PRESSURE_RESULT : Entity, IPPDMEntity
    {
        private String COMPRESSOR_PRESSURE_RESULT_IDValue;
        public String COMPRESSOR_PRESSURE_RESULT_ID
        {
            get { return this.COMPRESSOR_PRESSURE_RESULT_IDValue; }
            set { SetProperty(ref COMPRESSOR_PRESSURE_RESULT_IDValue, value); }
        }

        private String COMPRESSOR_OPERATING_CONDITIONS_IDValue;
        public String COMPRESSOR_OPERATING_CONDITIONS_ID
        {
            get { return this.COMPRESSOR_OPERATING_CONDITIONS_IDValue; }
            set { SetProperty(ref COMPRESSOR_OPERATING_CONDITIONS_IDValue, value); }
        }

        private Decimal? REQUIRED_DISCHARGE_PRESSUREValue;
        public Decimal? REQUIRED_DISCHARGE_PRESSURE
        {
            get { return this.REQUIRED_DISCHARGE_PRESSUREValue; }
            set { SetProperty(ref REQUIRED_DISCHARGE_PRESSUREValue, value); }
        }

        private Decimal? COMPRESSION_RATIOValue;
        public Decimal? COMPRESSION_RATIO
        {
            get { return this.COMPRESSION_RATIOValue; }
            set { SetProperty(ref COMPRESSION_RATIOValue, value); }
        }

        private Decimal? REQUIRED_POWERValue;
        public Decimal? REQUIRED_POWER
        {
            get { return this.REQUIRED_POWERValue; }
            set { SetProperty(ref REQUIRED_POWERValue, value); }
        }

        private Decimal? DISCHARGE_TEMPERATUREValue;
        public Decimal? DISCHARGE_TEMPERATURE
        {
            get { return this.DISCHARGE_TEMPERATUREValue; }
            set { SetProperty(ref DISCHARGE_TEMPERATUREValue, value); }
        }

        private Boolean? IS_FEASIBLEValue;
        public Boolean? IS_FEASIBLE
        {
            get { return this.IS_FEASIBLEValue; }
            set { SetProperty(ref IS_FEASIBLEValue, value); }
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



