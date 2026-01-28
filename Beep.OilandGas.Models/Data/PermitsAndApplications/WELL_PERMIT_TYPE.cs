using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class WELL_PERMIT_TYPE : ModelEntityBase
    {
        private String GRANTED_BY_BA_IDValue;
        public String GRANTED_BY_BA_ID
        {
            get { return this.GRANTED_BY_BA_IDValue; }
            set { SetProperty(ref GRANTED_BY_BA_IDValue, value); }
        }

        private String PERMIT_TYPEValue;
        public String PERMIT_TYPE
        {
            get { return this.PERMIT_TYPEValue; }
            set { SetProperty(ref PERMIT_TYPEValue, value); }
        }

        private String ABBREVIATIONValue;
        public String ABBREVIATION
        {
            get { return this.ABBREVIATIONValue; }
            set { SetProperty(ref ABBREVIATIONValue, value); }
        }

        private String LONG_NAMEValue;
        public String LONG_NAME
        {
            get { return this.LONG_NAMEValue; }
            set { SetProperty(ref LONG_NAMEValue, value); }
        }

        private String SHORT_NAMEValue;
        public String SHORT_NAME
        {
            get { return this.SHORT_NAMEValue; }
            set { SetProperty(ref SHORT_NAMEValue, value); }
        }

        private String RATE_SCHEDULE_IDValue;
        public String RATE_SCHEDULE_ID
        {
            get { return this.RATE_SCHEDULE_IDValue; }
            set { SetProperty(ref RATE_SCHEDULE_IDValue, value); }
        }
    }
}
