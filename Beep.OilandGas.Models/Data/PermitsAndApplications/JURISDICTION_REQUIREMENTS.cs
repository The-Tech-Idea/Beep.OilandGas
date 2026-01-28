using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class JURISDICTION_REQUIREMENTS : ModelEntityBase {
        private String JURISDICTION_REQUIREMENTS_IDValue;
        public String JURISDICTION_REQUIREMENTS_ID
        {
            get { return this.JURISDICTION_REQUIREMENTS_IDValue; }
            set { SetProperty(ref JURISDICTION_REQUIREMENTS_IDValue, value); }
        }

        private String COUNTRYValue;
        public String COUNTRY
        {
            get { return this.COUNTRYValue; }
            set { SetProperty(ref COUNTRYValue, value); }
        }

        private String STATE_PROVINCEValue;
        public String STATE_PROVINCE
        {
            get { return this.STATE_PROVINCEValue; }
            set { SetProperty(ref STATE_PROVINCEValue, value); }
        }

        private String REGULATORY_AUTHORITYValue;
        public String REGULATORY_AUTHORITY
        {
            get { return this.REGULATORY_AUTHORITYValue; }
            set { SetProperty(ref REGULATORY_AUTHORITYValue, value); }
        }

        private String REQUIREMENTS_DESCRIPTIONValue;
        public String REQUIREMENTS_DESCRIPTION
        {
            get { return this.REQUIREMENTS_DESCRIPTIONValue; }
            set { SetProperty(ref REQUIREMENTS_DESCRIPTIONValue, value); }
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
