using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class ENVIRONMENTAL_PERMIT_APPLICATION : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private String ENVIRONMENTAL_PERMIT_APPLICATION_IDValue;
        public String ENVIRONMENTAL_PERMIT_APPLICATION_ID
        {
            get { return this.ENVIRONMENTAL_PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref ENVIRONMENTAL_PERMIT_APPLICATION_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String ENVIRONMENTAL_PERMIT_TYPEValue;
        public String ENVIRONMENTAL_PERMIT_TYPE
        {
            get { return this.ENVIRONMENTAL_PERMIT_TYPEValue; }
            set { SetProperty(ref ENVIRONMENTAL_PERMIT_TYPEValue, value); }
        }

        private String WASTE_TYPEValue;
        public String WASTE_TYPE
        {
            get { return this.WASTE_TYPEValue; }
            set { SetProperty(ref WASTE_TYPEValue, value); }
        }

        private Decimal? WASTE_VOLUMEValue;
        public Decimal? WASTE_VOLUME
        {
            get { return this.WASTE_VOLUMEValue; }
            set { SetProperty(ref WASTE_VOLUMEValue, value); }
        }

        private String WASTE_VOLUME_UNITValue;
        public String WASTE_VOLUME_UNIT
        {
            get { return this.WASTE_VOLUME_UNITValue; }
            set { SetProperty(ref WASTE_VOLUME_UNITValue, value); }
        }

        private String DISPOSAL_METHODValue;
        public String DISPOSAL_METHOD
        {
            get { return this.DISPOSAL_METHODValue; }
            set { SetProperty(ref DISPOSAL_METHODValue, value); }
        }

        private String ENVIRONMENTAL_IMPACTValue;
        public String ENVIRONMENTAL_IMPACT
        {
            get { return this.ENVIRONMENTAL_IMPACTValue; }
            set { SetProperty(ref ENVIRONMENTAL_IMPACTValue, value); }
        }

        private String MONITORING_PLANValue;
        public String MONITORING_PLAN
        {
            get { return this.MONITORING_PLANValue; }
            set { SetProperty(ref MONITORING_PLANValue, value); }
        }

        private String NORM_INVOLVED_INDValue;
        public String NORM_INVOLVED_IND
        {
            get { return this.NORM_INVOLVED_INDValue; }
            set { SetProperty(ref NORM_INVOLVED_INDValue, value); }
        }

        private String FACILITY_LOCATIONValue;
        public String FACILITY_LOCATION
        {
            get { return this.FACILITY_LOCATIONValue; }
            set { SetProperty(ref FACILITY_LOCATIONValue, value); }
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



