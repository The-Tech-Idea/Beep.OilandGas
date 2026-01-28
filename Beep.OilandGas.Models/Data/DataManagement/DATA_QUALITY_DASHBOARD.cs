using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public partial class DATA_QUALITY_DASHBOARD : ModelEntityBase
    {
        private System.String DASHBOARD_IDValue;
        public System.String DASHBOARD_ID
        {
            get { return this.DASHBOARD_IDValue; }
            set { SetProperty(ref DASHBOARD_IDValue, value); }
        }

        private System.String TABLE_NAMEValue;
        public System.String TABLE_NAME
        {
            get { return this.TABLE_NAMEValue; }
            set { SetProperty(ref TABLE_NAMEValue, value); }
        }

        private System.DateTime? LAST_UPDATEDValue;
        public System.DateTime? LAST_UPDATED
        {
            get { return this.LAST_UPDATEDValue; }
            set { SetProperty(ref LAST_UPDATEDValue, value); }
        }

        private System.Decimal? OVERALL_QUALITY_SCOREValue;
        public System.Decimal? OVERALL_QUALITY_SCORE
        {
            get { return this.OVERALL_QUALITY_SCOREValue; }
            set { SetProperty(ref OVERALL_QUALITY_SCOREValue, value); }
        }

        private System.String CURRENT_METRICS_IDValue;
        public System.String CURRENT_METRICS_ID
        {
            get { return this.CURRENT_METRICS_IDValue; }
            set { SetProperty(ref CURRENT_METRICS_IDValue, value); }
        }

        private System.String FIELD_SCORES_JSONValue;
        public System.String FIELD_SCORES_JSON
        {
            get { return this.FIELD_SCORES_JSONValue; }
            set { SetProperty(ref FIELD_SCORES_JSONValue, value); }
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
