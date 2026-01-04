using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    public partial class CENTRIFUGAL_COMPRESSOR_PROPERTIES : Entity, IPPDMEntity
    {
        private String CENTRIFUGAL_COMPRESSOR_PROPERTIES_IDValue;
        public String CENTRIFUGAL_COMPRESSOR_PROPERTIES_ID
        {
            get { return this.CENTRIFUGAL_COMPRESSOR_PROPERTIES_IDValue; }
            set { SetProperty(ref CENTRIFUGAL_COMPRESSOR_PROPERTIES_IDValue, value); }
        }

        private String COMPRESSOR_OPERATING_CONDITIONS_IDValue;
        public String COMPRESSOR_OPERATING_CONDITIONS_ID
        {
            get { return this.COMPRESSOR_OPERATING_CONDITIONS_IDValue; }
            set { SetProperty(ref COMPRESSOR_OPERATING_CONDITIONS_IDValue, value); }
        }

        private Decimal? POLYTROPIC_EFFICIENCYValue;
        public Decimal? POLYTROPIC_EFFICIENCY
        {
            get { return this.POLYTROPIC_EFFICIENCYValue; }
            set { SetProperty(ref POLYTROPIC_EFFICIENCYValue, value); }
        }

        private Decimal? SPECIFIC_HEAT_RATIOValue;
        public Decimal? SPECIFIC_HEAT_RATIO
        {
            get { return this.SPECIFIC_HEAT_RATIOValue; }
            set { SetProperty(ref SPECIFIC_HEAT_RATIOValue, value); }
        }

        private Int32? NUMBER_OF_STAGESValue;
        public Int32? NUMBER_OF_STAGES
        {
            get { return this.NUMBER_OF_STAGESValue; }
            set { SetProperty(ref NUMBER_OF_STAGESValue, value); }
        }

        private Decimal? SPEEDValue;
        public Decimal? SPEED
        {
            get { return this.SPEEDValue; }
            set { SetProperty(ref SPEEDValue, value); }
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
