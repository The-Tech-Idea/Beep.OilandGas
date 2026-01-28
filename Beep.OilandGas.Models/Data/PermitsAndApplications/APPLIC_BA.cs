using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class APPLIC_BA : ModelEntityBase
    {
        private String APPLIC_BA_IDValue;
        public String APPLIC_BA_ID
        {
            get { return this.APPLIC_BA_IDValue; }
            set { SetProperty(ref APPLIC_BA_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private String BA_ROLEValue;
        public String BA_ROLE
        {
            get { return this.BA_ROLEValue; }
            set { SetProperty(ref BA_ROLEValue, value); }
        }

        private String REMARKSValue;
        public String REMARKS
        {
            get { return this.REMARKSValue; }
            set { SetProperty(ref REMARKSValue, value); }
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
