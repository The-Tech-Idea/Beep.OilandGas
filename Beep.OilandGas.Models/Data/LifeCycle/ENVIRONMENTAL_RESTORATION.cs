using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public partial class ENVIRONMENTAL_RESTORATION : ModelEntityBase
    {
        private System.String RESTORATION_IDValue;
        public System.String RESTORATION_ID
        {
            get => RESTORATION_IDValue;
            set => SetProperty(ref RESTORATION_IDValue, value);
        }

        private System.String SITE_IDValue;
        public System.String SITE_ID
        {
            get => SITE_IDValue;
            set => SetProperty(ref SITE_IDValue, value);
        }

        private System.String SITE_TYPEValue;
        public System.String SITE_TYPE
        {
            get => SITE_TYPEValue;
            set => SetProperty(ref SITE_TYPEValue, value);
        }

        private System.String RESTORATION_TYPEValue;
        public System.String RESTORATION_TYPE
        {
            get => RESTORATION_TYPEValue;
            set => SetProperty(ref RESTORATION_TYPEValue, value);
        }

        private System.DateTime? START_DATEValue;
        public System.DateTime? START_DATE
        {
            get => START_DATEValue;
            set => SetProperty(ref START_DATEValue, value);
        }

        private System.DateTime? COMPLETION_DATEValue;
        public System.DateTime? COMPLETION_DATE
        {
            get => COMPLETION_DATEValue;
            set => SetProperty(ref COMPLETION_DATEValue, value);
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get => STATUSValue;
            set => SetProperty(ref STATUSValue, value);
        }

        private System.String CERTIFICATION_STATUSValue;
        public System.String CERTIFICATION_STATUS
        {
            get => CERTIFICATION_STATUSValue;
            set => SetProperty(ref CERTIFICATION_STATUSValue, value);
        }

        private System.DateTime? CERTIFICATION_DATEValue;
        public System.DateTime? CERTIFICATION_DATE
        {
            get => CERTIFICATION_DATEValue;
            set => SetProperty(ref CERTIFICATION_DATEValue, value);
        }

        private System.String CERTIFYING_AUTHORITYValue;
        public System.String CERTIFYING_AUTHORITY
        {
            get => CERTIFYING_AUTHORITYValue;
            set => SetProperty(ref CERTIFYING_AUTHORITYValue, value);
        }

        private System.String CONTRACTOR_IDValue;
        public System.String CONTRACTOR_ID
        {
            get => CONTRACTOR_IDValue;
            set => SetProperty(ref CONTRACTOR_IDValue, value);
        }

        private System.String ENVIRONMENTAL_IMPACTValue;
        public System.String ENVIRONMENTAL_IMPACT
        {
            get => ENVIRONMENTAL_IMPACTValue;
            set => SetProperty(ref ENVIRONMENTAL_IMPACTValue, value);
        }

        private System.String REMEDIATION_METHODValue;
        public System.String REMEDIATION_METHOD
        {
            get => REMEDIATION_METHODValue;
            set => SetProperty(ref REMEDIATION_METHODValue, value);
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get => REMARKValue;
            set => SetProperty(ref REMARKValue, value);
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get => SOURCEValue;
            set => SetProperty(ref SOURCEValue, value);
        }

        public ENVIRONMENTAL_RESTORATION() { }
    }
}
