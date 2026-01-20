using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    public partial class CHOKE_FLOW_RESULT : ModelEntityBase {
        private String CHOKE_FLOW_RESULT_IDValue;
        public String CHOKE_FLOW_RESULT_ID
        {
            get { return this.CHOKE_FLOW_RESULT_IDValue; }
            set { SetProperty(ref CHOKE_FLOW_RESULT_IDValue, value); }
        }

        private String CHOKE_PROPERTIES_IDValue;
        public String CHOKE_PROPERTIES_ID
        {
            get { return this.CHOKE_PROPERTIES_IDValue; }
            set { SetProperty(ref CHOKE_PROPERTIES_IDValue, value); }
        }

        private String GAS_CHOKE_PROPERTIES_IDValue;
        public String GAS_CHOKE_PROPERTIES_ID
        {
            get { return this.GAS_CHOKE_PROPERTIES_IDValue; }
            set { SetProperty(ref GAS_CHOKE_PROPERTIES_IDValue, value); }
        }

        private Decimal? FLOW_RATEValue;
        public Decimal? FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private Decimal? DOWNSTREAM_PRESSUREValue;
        public Decimal? DOWNSTREAM_PRESSURE
        {
            get { return this.DOWNSTREAM_PRESSUREValue; }
            set { SetProperty(ref DOWNSTREAM_PRESSUREValue, value); }
        }

        private Decimal? UPSTREAM_PRESSUREValue;
        public Decimal? UPSTREAM_PRESSURE
        {
            get { return this.UPSTREAM_PRESSUREValue; }
            set { SetProperty(ref UPSTREAM_PRESSUREValue, value); }
        }

        private Decimal? PRESSURE_RATIOValue;
        public Decimal? PRESSURE_RATIO
        {
            get { return this.PRESSURE_RATIOValue; }
            set { SetProperty(ref PRESSURE_RATIOValue, value); }
        }

        private String FLOW_REGIMEValue;
        public String FLOW_REGIME
        {
            get { return this.FLOW_REGIMEValue; }
            set { SetProperty(ref FLOW_REGIMEValue, value); }
        }

        private Decimal? CRITICAL_PRESSURE_RATIOValue;
        public Decimal? CRITICAL_PRESSURE_RATIO
        {
            get { return this.CRITICAL_PRESSURE_RATIOValue; }
            set { SetProperty(ref CRITICAL_PRESSURE_RATIOValue, value); }
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



