using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.DCA
{
    public partial class DCA_FIT_RESULT : Entity, Core.Interfaces.IPPDMEntity
    {
        private String DCA_FIT_RESULT_IDValue;
        public String DCA_FIT_RESULT_ID
        {
            get { return this.DCA_FIT_RESULT_IDValue; }
            set { SetProperty(ref DCA_FIT_RESULT_IDValue, value); }
        }

        private String PARAMETERS_JSONValue;
        public String PARAMETERS_JSON
        {
            get { return this.PARAMETERS_JSONValue; }
            set { SetProperty(ref PARAMETERS_JSONValue, value); }
        }

        private String OBSERVED_VALUES_JSONValue;
        public String OBSERVED_VALUES_JSON
        {
            get { return this.OBSERVED_VALUES_JSONValue; }
            set { SetProperty(ref OBSERVED_VALUES_JSONValue, value); }
        }

        private String PREDICTED_VALUES_JSONValue;
        public String PREDICTED_VALUES_JSON
        {
            get { return this.PREDICTED_VALUES_JSONValue; }
            set { SetProperty(ref PREDICTED_VALUES_JSONValue, value); }
        }

        private String RESIDUALS_JSONValue;
        public String RESIDUALS_JSON
        {
            get { return this.RESIDUALS_JSONValue; }
            set { SetProperty(ref RESIDUALS_JSONValue, value); }
        }

        private Decimal? R_SQUAREDValue;
        public Decimal? R_SQUARED
        {
            get { return this.R_SQUAREDValue; }
            set { SetProperty(ref R_SQUAREDValue, value); }
        }

        private Decimal? ADJUSTED_R_SQUAREDValue;
        public Decimal? ADJUSTED_R_SQUARED
        {
            get { return this.ADJUSTED_R_SQUAREDValue; }
            set { SetProperty(ref ADJUSTED_R_SQUAREDValue, value); }
        }

        private Decimal? RMSEValue;
        public Decimal? RMSE
        {
            get { return this.RMSEValue; }
            set { SetProperty(ref RMSEValue, value); }
        }

        private Decimal? MAEValue;
        public Decimal? MAE
        {
            get { return this.MAEValue; }
            set { SetProperty(ref MAEValue, value); }
        }

        private Decimal? AICValue;
        public Decimal? AIC
        {
            get { return this.AICValue; }
            set { SetProperty(ref AICValue, value); }
        }

        private Decimal? BICValue;
        public Decimal? BIC
        {
            get { return this.BICValue; }
            set { SetProperty(ref BICValue, value); }
        }

        private String CONFIDENCE_INTERVALS_JSONValue;
        public String CONFIDENCE_INTERVALS_JSON
        {
            get { return this.CONFIDENCE_INTERVALS_JSONValue; }
            set { SetProperty(ref CONFIDENCE_INTERVALS_JSONValue, value); }
        }

        private Int32? ITERATIONSValue;
        public Int32? ITERATIONS
        {
            get { return this.ITERATIONSValue; }
            set { SetProperty(ref ITERATIONSValue, value); }
        }

        private Boolean? CONVERGEDValue;
        public Boolean? CONVERGED
        {
            get { return this.CONVERGEDValue; }
            set { SetProperty(ref CONVERGEDValue, value); }
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

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
    }
}
