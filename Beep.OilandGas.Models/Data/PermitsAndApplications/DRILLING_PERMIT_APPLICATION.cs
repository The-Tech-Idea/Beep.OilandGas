using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class DRILLING_PERMIT_APPLICATION : ModelEntityBase {
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

    }
}
