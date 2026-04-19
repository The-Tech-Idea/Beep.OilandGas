using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class REQUIRED_FORM : ModelEntityBase {
        private String REQUIRED_FORM_IDValue;
        public String REQUIRED_FORM_ID
        {
            get { return this.REQUIRED_FORM_IDValue; }
            set { SetProperty(ref REQUIRED_FORM_IDValue, value); }
        }

        private String JURISDICTION_REQUIREMENTS_IDValue;
        public String JURISDICTION_REQUIREMENTS_ID
        {
            get { return this.JURISDICTION_REQUIREMENTS_IDValue; }
            set { SetProperty(ref JURISDICTION_REQUIREMENTS_IDValue, value); }
        }

        private String FORM_IDValue;
        public String FORM_ID
        {
            get { return this.FORM_IDValue; }
            set { SetProperty(ref FORM_IDValue, value); }
        }

        private String FORM_NAMEValue;
        public String FORM_NAME
        {
            get { return this.FORM_NAMEValue; }
            set { SetProperty(ref FORM_NAMEValue, value); }
        }

        private String DESCRIPTIONValue;
        public String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        private String FORM_URLValue;
        public String FORM_URL
        {
            get { return this.FORM_URLValue; }
            set { SetProperty(ref FORM_URLValue, value); }
        }

        private String ONLINE_FILING_AVAILABLE_INDValue;
        public String ONLINE_FILING_AVAILABLE_IND
        {
            get { return this.ONLINE_FILING_AVAILABLE_INDValue; }
            set { SetProperty(ref ONLINE_FILING_AVAILABLE_INDValue, value); }
        }

        private String FORM_TYPEValue;
        public String FORM_TYPE
        {
            get { return this.FORM_TYPEValue; }
            set { SetProperty(ref FORM_TYPEValue, value); }
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

        public string REQUIRED_IND { get; set; }
        public string FORM_CODE { get; set; }
    }
}
