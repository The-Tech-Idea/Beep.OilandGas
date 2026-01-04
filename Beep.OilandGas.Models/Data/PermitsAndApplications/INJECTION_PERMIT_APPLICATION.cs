using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class INJECTION_PERMIT_APPLICATION : Entity, Core.Interfaces.IPPDMEntity
    {
        private String INJECTION_PERMIT_APPLICATION_IDValue;
        public String INJECTION_PERMIT_APPLICATION_ID
        {
            get { return this.INJECTION_PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref INJECTION_PERMIT_APPLICATION_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String INJECTION_TYPEValue;
        public String INJECTION_TYPE
        {
            get { return this.INJECTION_TYPEValue; }
            set { SetProperty(ref INJECTION_TYPEValue, value); }
        }

        private String INJECTION_ZONEValue;
        public String INJECTION_ZONE
        {
            get { return this.INJECTION_ZONEValue; }
            set { SetProperty(ref INJECTION_ZONEValue, value); }
        }

        private String INJECTION_FLUIDValue;
        public String INJECTION_FLUID
        {
            get { return this.INJECTION_FLUIDValue; }
            set { SetProperty(ref INJECTION_FLUIDValue, value); }
        }

        private Decimal? MAXIMUM_INJECTION_PRESSUREValue;
        public Decimal? MAXIMUM_INJECTION_PRESSURE
        {
            get { return this.MAXIMUM_INJECTION_PRESSUREValue; }
            set { SetProperty(ref MAXIMUM_INJECTION_PRESSUREValue, value); }
        }

        private Decimal? MAXIMUM_INJECTION_RATEValue;
        public Decimal? MAXIMUM_INJECTION_RATE
        {
            get { return this.MAXIMUM_INJECTION_RATEValue; }
            set { SetProperty(ref MAXIMUM_INJECTION_RATEValue, value); }
        }

        private String INJECTION_RATE_UNITValue;
        public String INJECTION_RATE_UNIT
        {
            get { return this.INJECTION_RATE_UNITValue; }
            set { SetProperty(ref INJECTION_RATE_UNITValue, value); }
        }

        private String MONITORING_REQUIREMENTSValue;
        public String MONITORING_REQUIREMENTS
        {
            get { return this.MONITORING_REQUIREMENTSValue; }
            set { SetProperty(ref MONITORING_REQUIREMENTSValue, value); }
        }

        private String INJECTION_WELL_UWIValue;
        public String INJECTION_WELL_UWI
        {
            get { return this.INJECTION_WELL_UWIValue; }
            set { SetProperty(ref INJECTION_WELL_UWIValue, value); }
        }

        private String IS_CO2_STORAGE_INDValue;
        public String IS_CO2_STORAGE_IND
        {
            get { return this.IS_CO2_STORAGE_INDValue; }
            set { SetProperty(ref IS_CO2_STORAGE_INDValue, value); }
        }

        private String IS_GAS_STORAGE_INDValue;
        public String IS_GAS_STORAGE_IND
        {
            get { return this.IS_GAS_STORAGE_INDValue; }
            set { SetProperty(ref IS_GAS_STORAGE_INDValue, value); }
        }

        private String IS_BRINE_MINING_INDValue;
        public String IS_BRINE_MINING_IND
        {
            get { return this.IS_BRINE_MINING_INDValue; }
            set { SetProperty(ref IS_BRINE_MINING_INDValue, value); }
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
