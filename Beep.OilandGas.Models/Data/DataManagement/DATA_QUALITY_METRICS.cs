using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public partial class DATA_QUALITY_METRICS : ModelEntityBase
    {
        private System.String METRICS_IDValue;
        public System.String METRICS_ID
        {
            get { return this.METRICS_IDValue; }
            set { SetProperty(ref METRICS_IDValue, value); }
        }

        private System.String TABLE_NAMEValue;
        public System.String TABLE_NAME
        {
            get { return this.TABLE_NAMEValue; }
            set { SetProperty(ref TABLE_NAMEValue, value); }
        }

        private System.Int32? TOTAL_RECORDSValue;
        public System.Int32? TOTAL_RECORDS
        {
            get { return this.TOTAL_RECORDSValue; }
            set { SetProperty(ref TOTAL_RECORDSValue, value); }
        }

        private System.Int32? COMPLETE_RECORDSValue;
        public System.Int32? COMPLETE_RECORDS
        {
            get { return this.COMPLETE_RECORDSValue; }
            set { SetProperty(ref COMPLETE_RECORDSValue, value); }
        }

        private System.Int32? INCOMPLETE_RECORDSValue;
        public System.Int32? INCOMPLETE_RECORDS
        {
            get { return this.INCOMPLETE_RECORDSValue; }
            set { SetProperty(ref INCOMPLETE_RECORDSValue, value); }
        }

        private System.Decimal? COMPLETENESS_SCOREValue;
        public System.Decimal? COMPLETENESS_SCORE
        {
            get { return this.COMPLETENESS_SCOREValue; }
            set { SetProperty(ref COMPLETENESS_SCOREValue, value); }
        }

        private System.Decimal? ACCURACY_SCOREValue;
        public System.Decimal? ACCURACY_SCORE
        {
            get { return this.ACCURACY_SCOREValue; }
            set { SetProperty(ref ACCURACY_SCOREValue, value); }
        }

        private System.Decimal? CONSISTENCY_SCOREValue;
        public System.Decimal? CONSISTENCY_SCORE
        {
            get { return this.CONSISTENCY_SCOREValue; }
            set { SetProperty(ref CONSISTENCY_SCOREValue, value); }
        }

        private System.Decimal? OVERALL_QUALITY_SCOREValue;
        public System.Decimal? OVERALL_QUALITY_SCORE
        {
            get { return this.OVERALL_QUALITY_SCOREValue; }
            set { SetProperty(ref OVERALL_QUALITY_SCOREValue, value); }
        }

        private System.DateTime? METRICS_DATEValue;
        public System.DateTime? METRICS_DATE
        {
            get { return this.METRICS_DATEValue; }
            set { SetProperty(ref METRICS_DATEValue, value); }
        }

        private System.String FIELD_METRICS_JSONValue;
        public System.String FIELD_METRICS_JSON
        {
            get { return this.FIELD_METRICS_JSONValue; }
            set { SetProperty(ref FIELD_METRICS_JSONValue, value); }
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
