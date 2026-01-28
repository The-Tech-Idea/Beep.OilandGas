using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public partial class DATA_VERSION_COMPARISON : ModelEntityBase
    {
        private System.String COMPARISON_IDValue;
        public System.String COMPARISON_ID
        {
            get { return this.COMPARISON_IDValue; }
            set { SetProperty(ref COMPARISON_IDValue, value); }
        }

        private System.String TABLE_NAMEValue;
        public System.String TABLE_NAME
        {
            get { return this.TABLE_NAMEValue; }
            set { SetProperty(ref TABLE_NAMEValue, value); }
        }

        private System.String ENTITY_IDValue;
        public System.String ENTITY_ID
        {
            get { return this.ENTITY_IDValue; }
            set { SetProperty(ref ENTITY_IDValue, value); }
        }

        private System.Int32? VERSION_1_NUMBERValue;
        public System.Int32? VERSION_1_NUMBER
        {
            get { return this.VERSION_1_NUMBERValue; }
            set { SetProperty(ref VERSION_1_NUMBERValue, value); }
        }

        private System.Int32? VERSION_2_NUMBERValue;
        public System.Int32? VERSION_2_NUMBER
        {
            get { return this.VERSION_2_NUMBERValue; }
            set { SetProperty(ref VERSION_2_NUMBERValue, value); }
        }

        private System.String DIFFERENCES_JSONValue;
        public System.String DIFFERENCES_JSON
        {
            get { return this.DIFFERENCES_JSONValue; }
            set { SetProperty(ref DIFFERENCES_JSONValue, value); }
        }

        private System.String HAS_DIFFERENCES_INDValue;
        public System.String HAS_DIFFERENCES_IND
        {
            get { return this.HAS_DIFFERENCES_INDValue; }
            set { SetProperty(ref HAS_DIFFERENCES_INDValue, value); }
        }

        private System.DateTime? COMPARISON_DATEValue;
        public System.DateTime? COMPARISON_DATE
        {
            get { return this.COMPARISON_DATEValue; }
            set { SetProperty(ref COMPARISON_DATEValue, value); }
        }

        // Standard PPDM columns

        private System.String SOURCEValue;

        private System.String REMARKValue;

        // Optional IPPDMEntity properties
        private System.String AREA_IDValue;
        public System.String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private System.String AREA_TYPEValue;
        public System.String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private System.String BUSINESS_ASSOCIATE_IDValue;
        public System.String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

    }
}
