using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class DRILLING_PERMIT_APPLICATION : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private String DRILLING_PERMIT_APPLICATION_IDValue;
        public String DRILLING_PERMIT_APPLICATION_ID
        {
            get { return this.DRILLING_PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref DRILLING_PERMIT_APPLICATION_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String WELL_UWIValue;
        public String WELL_UWI
        {
            get { return this.WELL_UWIValue; }
            set { SetProperty(ref WELL_UWIValue, value); }
        }

        private String LEGAL_DESCRIPTIONValue;
        public String LEGAL_DESCRIPTION
        {
            get { return this.LEGAL_DESCRIPTIONValue; }
            set { SetProperty(ref LEGAL_DESCRIPTIONValue, value); }
        }

        private String TARGET_FORMATIONValue;
        public String TARGET_FORMATION
        {
            get { return this.TARGET_FORMATIONValue; }
            set { SetProperty(ref TARGET_FORMATIONValue, value); }
        }

        private Decimal? PROPOSED_DEPTHValue;
        public Decimal? PROPOSED_DEPTH
        {
            get { return this.PROPOSED_DEPTHValue; }
            set { SetProperty(ref PROPOSED_DEPTHValue, value); }
        }

        private String DRILLING_METHODValue;
        public String DRILLING_METHOD
        {
            get { return this.DRILLING_METHODValue; }
            set { SetProperty(ref DRILLING_METHODValue, value); }
        }

        private String SURFACE_OWNER_NOTIFIED_INDValue;
        public String SURFACE_OWNER_NOTIFIED_IND
        {
            get { return this.SURFACE_OWNER_NOTIFIED_INDValue; }
            set { SetProperty(ref SURFACE_OWNER_NOTIFIED_INDValue, value); }
        }

        private DateTime? SURFACE_OWNER_NOTIFICATION_DATEValue;
        public DateTime? SURFACE_OWNER_NOTIFICATION_DATE
        {
            get { return this.SURFACE_OWNER_NOTIFICATION_DATEValue; }
            set { SetProperty(ref SURFACE_OWNER_NOTIFICATION_DATEValue, value); }
        }

        private String ENVIRONMENTAL_ASSESSMENT_REQUIRED_INDValue;
        public String ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND
        {
            get { return this.ENVIRONMENTAL_ASSESSMENT_REQUIRED_INDValue; }
            set { SetProperty(ref ENVIRONMENTAL_ASSESSMENT_REQUIRED_INDValue, value); }
        }

        private String ENVIRONMENTAL_ASSESSMENT_REFERENCEValue;
        public String ENVIRONMENTAL_ASSESSMENT_REFERENCE
        {
            get { return this.ENVIRONMENTAL_ASSESSMENT_REFERENCEValue; }
            set { SetProperty(ref ENVIRONMENTAL_ASSESSMENT_REFERENCEValue, value); }
        }

        private String SPACING_UNITValue;
        public String SPACING_UNIT
        {
            get { return this.SPACING_UNITValue; }
            set { SetProperty(ref SPACING_UNITValue, value); }
        }

        private String PERMIT_TYPEValue;
        public String PERMIT_TYPE
        {
            get { return this.PERMIT_TYPEValue; }
            set { SetProperty(ref PERMIT_TYPEValue, value); }
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



