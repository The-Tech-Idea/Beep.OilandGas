using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Entity for storing table-level data quality metrics
    /// </summary>
    public partial class DATA_QUALITY_METRICS : Entity, Beep.OilandGas.Models.Core.Interfaces.IPPDMEntity
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
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

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
        public System.DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? EXPIRY_DATEValue;
        public System.DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
    }
}
