using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class APPLIC_REMARK : ModelEntityBase
    {
        private String APPLIC_REMARK_IDValue;
        public String APPLIC_REMARK_ID
        {
            get { return this.APPLIC_REMARK_IDValue; }
            set { SetProperty(ref APPLIC_REMARK_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String REMARK_TYPEValue;
        public String REMARK_TYPE
        {
            get { return this.REMARK_TYPEValue; }
            set { SetProperty(ref REMARK_TYPEValue, value); }
        }

        private String REMARK_TEXTValue;
        public String REMARK_TEXT
        {
            get { return this.REMARK_TEXTValue; }
            set { SetProperty(ref REMARK_TEXTValue, value); }
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
    }
}
