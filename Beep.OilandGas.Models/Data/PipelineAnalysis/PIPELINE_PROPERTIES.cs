using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class PIPELINE_PROPERTIES : Entity, IPPDMEntity
    {
        private String PIPELINE_PROPERTIES_IDValue;
        public String PIPELINE_PROPERTIES_ID
        {
            get { return this.PIPELINE_PROPERTIES_IDValue; }
            set { SetProperty(ref PIPELINE_PROPERTIES_IDValue, value); }
        }

        private Decimal? DIAMETERValue;
        public Decimal? DIAMETER
        {
            get { return this.DIAMETERValue; }
            set { SetProperty(ref DIAMETERValue, value); }
        }

        private Decimal? LENGTHValue;
        public Decimal? LENGTH
        {
            get { return this.LENGTHValue; }
            set { SetProperty(ref LENGTHValue, value); }
        }

        private Decimal? ROUGHNESSValue;
        public Decimal? ROUGHNESS
        {
            get { return this.ROUGHNESSValue; }
            set { SetProperty(ref ROUGHNESSValue, value); }
        }

        private Decimal? ELEVATION_CHANGEValue;
        public Decimal? ELEVATION_CHANGE
        {
            get { return this.ELEVATION_CHANGEValue; }
            set { SetProperty(ref ELEVATION_CHANGEValue, value); }
        }

        private Decimal? INLET_PRESSUREValue;
        public Decimal? INLET_PRESSURE
        {
            get { return this.INLET_PRESSUREValue; }
            set { SetProperty(ref INLET_PRESSUREValue, value); }
        }

        private Decimal? OUTLET_PRESSUREValue;
        public Decimal? OUTLET_PRESSURE
        {
            get { return this.OUTLET_PRESSUREValue; }
            set { SetProperty(ref OUTLET_PRESSUREValue, value); }
        }

        private Decimal? AVERAGE_TEMPERATUREValue;
        public Decimal? AVERAGE_TEMPERATURE
        {
            get { return this.AVERAGE_TEMPERATUREValue; }
            set { SetProperty(ref AVERAGE_TEMPERATUREValue, value); }
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



