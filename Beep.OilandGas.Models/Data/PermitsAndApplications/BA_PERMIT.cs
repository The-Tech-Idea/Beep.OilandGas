using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class BA_PERMIT : ModelEntityBase
    {
        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private String JURISDICTIONValue;
        public String JURISDICTION
        {
            get { return this.JURISDICTIONValue; }
            set { SetProperty(ref JURISDICTIONValue, value); }
        }

        private String PERMIT_TYPEValue;
        public String PERMIT_TYPE
        {
            get { return this.PERMIT_TYPEValue; }
            set { SetProperty(ref PERMIT_TYPEValue, value); }
        }

        private Decimal? PERMIT_OBS_NOValue;
        public Decimal? PERMIT_OBS_NO
        {
            get { return this.PERMIT_OBS_NOValue; }
            set { SetProperty(ref PERMIT_OBS_NOValue, value); }
        }

        private String PERMIT_NUMValue;
        public String PERMIT_NUM
        {
            get { return this.PERMIT_NUMValue; }
            set { SetProperty(ref PERMIT_NUMValue, value); }
        }
    }
}
