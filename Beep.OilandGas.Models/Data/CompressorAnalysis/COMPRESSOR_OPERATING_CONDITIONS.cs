using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    public partial class COMPRESSOR_OPERATING_CONDITIONS : Entity, IPPDMEntity
    {
        private String COMPRESSOR_OPERATING_CONDITIONS_IDValue;
        public String COMPRESSOR_OPERATING_CONDITIONS_ID
        {
            get { return this.COMPRESSOR_OPERATING_CONDITIONS_IDValue; }
            set { SetProperty(ref COMPRESSOR_OPERATING_CONDITIONS_IDValue, value); }
        }

        private Decimal? SUCTION_PRESSUREValue;
        public Decimal? SUCTION_PRESSURE
        {
            get { return this.SUCTION_PRESSUREValue; }
            set { SetProperty(ref SUCTION_PRESSUREValue, value); }
        }

        private Decimal? DISCHARGE_PRESSUREValue;
        public Decimal? DISCHARGE_PRESSURE
        {
            get { return this.DISCHARGE_PRESSUREValue; }
            set { SetProperty(ref DISCHARGE_PRESSUREValue, value); }
        }

        private Decimal? SUCTION_TEMPERATUREValue;
        public Decimal? SUCTION_TEMPERATURE
        {
            get { return this.SUCTION_TEMPERATUREValue; }
            set { SetProperty(ref SUCTION_TEMPERATUREValue, value); }
        }

        private Decimal? DISCHARGE_TEMPERATUREValue;
        public Decimal? DISCHARGE_TEMPERATURE
        {
            get { return this.DISCHARGE_TEMPERATUREValue; }
            set { SetProperty(ref DISCHARGE_TEMPERATUREValue, value); }
        }

        private Decimal? GAS_FLOW_RATEValue;
        public Decimal? GAS_FLOW_RATE
        {
            get { return this.GAS_FLOW_RATEValue; }
            set { SetProperty(ref GAS_FLOW_RATEValue, value); }
        }

        private Decimal? GAS_SPECIFIC_GRAVITYValue;
        public Decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal? GAS_MOLECULAR_WEIGHTValue;
        public Decimal? GAS_MOLECULAR_WEIGHT
        {
            get { return this.GAS_MOLECULAR_WEIGHTValue; }
            set { SetProperty(ref GAS_MOLECULAR_WEIGHTValue, value); }
        }

        private Decimal? COMPRESSOR_EFFICIENCYValue;
        public Decimal? COMPRESSOR_EFFICIENCY
        {
            get { return this.COMPRESSOR_EFFICIENCYValue; }
            set { SetProperty(ref COMPRESSOR_EFFICIENCYValue, value); }
        }

        private Decimal? MECHANICAL_EFFICIENCYValue;
        public Decimal? MECHANICAL_EFFICIENCY
        {
            get { return this.MECHANICAL_EFFICIENCYValue; }
            set { SetProperty(ref MECHANICAL_EFFICIENCYValue, value); }
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



