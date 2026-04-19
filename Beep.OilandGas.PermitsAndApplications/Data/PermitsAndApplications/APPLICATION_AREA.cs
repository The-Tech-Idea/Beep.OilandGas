using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class APPLICATION_AREA : ModelEntityBase {
        private String APPLICATION_AREA_IDValue;
        public String APPLICATION_AREA_ID
        {
            get { return this.APPLICATION_AREA_IDValue; }
            set { SetProperty(ref APPLICATION_AREA_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String AREA_NAMEValue;
        public String AREA_NAME
        {
            get { return this.AREA_NAMEValue; }
            set { SetProperty(ref AREA_NAMEValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String LEGAL_DESCRIPTIONValue;
        public String LEGAL_DESCRIPTION
        {
            get { return this.LEGAL_DESCRIPTIONValue; }
            set { SetProperty(ref LEGAL_DESCRIPTIONValue, value); }
        }

        private Decimal  LATITUDEValue;
        public Decimal  LATITUDE
        {
            get { return this.LATITUDEValue; }
            set { SetProperty(ref LATITUDEValue, value); }
        }

        private Decimal  LONGITUDEValue;
        public Decimal  LONGITUDE
        {
            get { return this.LONGITUDEValue; }
            set { SetProperty(ref LONGITUDEValue, value); }
        }

        // Standard PPDM columns

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

    }
}
