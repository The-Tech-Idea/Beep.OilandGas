using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class GAS_PIPELINE_FLOW_PROPERTIES : Entity, IPPDMEntity
    {
        private String GAS_PIPELINE_FLOW_PROPERTIES_IDValue;
        public String GAS_PIPELINE_FLOW_PROPERTIES_ID
        {
            get { return this.GAS_PIPELINE_FLOW_PROPERTIES_IDValue; }
            set { SetProperty(ref GAS_PIPELINE_FLOW_PROPERTIES_IDValue, value); }
        }

        private String PIPELINE_PROPERTIES_IDValue;
        public String PIPELINE_PROPERTIES_ID
        {
            get { return this.PIPELINE_PROPERTIES_IDValue; }
            set { SetProperty(ref PIPELINE_PROPERTIES_IDValue, value); }
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

        private Decimal? BASE_PRESSUREValue;
        public Decimal? BASE_PRESSURE
        {
            get { return this.BASE_PRESSUREValue; }
            set { SetProperty(ref BASE_PRESSUREValue, value); }
        }

        private Decimal? BASE_TEMPERATUREValue;
        public Decimal? BASE_TEMPERATURE
        {
            get { return this.BASE_TEMPERATUREValue; }
            set { SetProperty(ref BASE_TEMPERATUREValue, value); }
        }

        private Decimal? Z_FACTORValue;
        public Decimal? Z_FACTOR
        {
            get { return this.Z_FACTORValue; }
            set { SetProperty(ref Z_FACTORValue, value); }
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
