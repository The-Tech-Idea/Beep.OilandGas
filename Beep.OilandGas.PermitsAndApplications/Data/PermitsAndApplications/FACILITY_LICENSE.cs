using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class FACILITY_LICENSE : ModelEntityBase
    {
        private String FACILITY_IDValue;
        public String FACILITY_ID
        {
            get { return this.FACILITY_IDValue; }
            set { SetProperty(ref FACILITY_IDValue, value); }
        }

        private String FACILITY_TYPEValue;
        public String FACILITY_TYPE
        {
            get { return this.FACILITY_TYPEValue; }
            set { SetProperty(ref FACILITY_TYPEValue, value); }
        }

        private String LICENSE_IDValue;
        public String LICENSE_ID
        {
            get { return this.LICENSE_IDValue; }
            set { SetProperty(ref LICENSE_IDValue, value); }
        }

        private String LICENSE_NUMValue;
        public String LICENSE_NUM
        {
            get { return this.LICENSE_NUMValue; }
            set { SetProperty(ref LICENSE_NUMValue, value); }
        }

        private String FACILITY_LICENSE_TYPEValue;
        public String FACILITY_LICENSE_TYPE
        {
            get { return this.FACILITY_LICENSE_TYPEValue; }
            set { SetProperty(ref FACILITY_LICENSE_TYPEValue, value); }
        }

        private DateTime? GRANTED_DATEValue;
        public DateTime? GRANTED_DATE
        {
            get { return this.GRANTED_DATEValue; }
            set { SetProperty(ref GRANTED_DATEValue, value); }
        }

        private String GRANTED_BY_BA_IDValue;
        public String GRANTED_BY_BA_ID
        {
            get { return this.GRANTED_BY_BA_IDValue; }
            set { SetProperty(ref GRANTED_BY_BA_IDValue, value); }
        }

        private String GRANTED_TO_BA_IDValue;
        public String GRANTED_TO_BA_ID
        {
            get { return this.GRANTED_TO_BA_IDValue; }
            set { SetProperty(ref GRANTED_TO_BA_IDValue, value); }
        }

        private String FEES_PAID_INDValue;
        public String FEES_PAID_IND
        {
            get { return this.FEES_PAID_INDValue; }
            set { SetProperty(ref FEES_PAID_INDValue, value); }
        }

        private String VIOLATION_INDValue;
        public String VIOLATION_IND
        {
            get { return this.VIOLATION_INDValue; }
            set { SetProperty(ref VIOLATION_INDValue, value); }
        }

        private String DESCRIPTIONValue;
        public String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        private String LICENSE_LOCATIONValue;
        public String LICENSE_LOCATION
        {
            get { return this.LICENSE_LOCATIONValue; }
            set { SetProperty(ref LICENSE_LOCATIONValue, value); }
        }
    }
}
