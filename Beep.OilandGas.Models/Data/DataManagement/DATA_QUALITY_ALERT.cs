using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Entity for storing data quality alerts that need attention
    /// </summary>
    public partial class DATA_QUALITY_ALERT : ModelEntityBase
    {
        private System.String ALERT_IDValue;
        public System.String ALERT_ID
        {
            get { return this.ALERT_IDValue; }
            set { SetProperty(ref ALERT_IDValue, value); }
        }

        private System.String TABLE_NAMEValue;
        public System.String TABLE_NAME
        {
            get { return this.TABLE_NAMEValue; }
            set { SetProperty(ref TABLE_NAMEValue, value); }
        }

        private System.String FIELD_NAMEValue;
        public System.String FIELD_NAME
        {
            get { return this.FIELD_NAMEValue; }
            set { SetProperty(ref FIELD_NAMEValue, value); }
        }

        private System.String SEVERITYValue;
        public System.String SEVERITY
        {
            get { return this.SEVERITYValue; }
            set { SetProperty(ref SEVERITYValue, value); }
        }

        private System.String ALERT_MESSAGEValue;
        public System.String ALERT_MESSAGE
        {
            get { return this.ALERT_MESSAGEValue; }
            set { SetProperty(ref ALERT_MESSAGEValue, value); }
        }

        private System.DateTime? CREATED_DATEValue;
        public System.DateTime? CREATED_DATE
        {
            get { return this.CREATED_DATEValue; }
            set { SetProperty(ref CREATED_DATEValue, value); }
        }

        private System.String RESOLVED_INDValue;
        public System.String RESOLVED_IND
        {
            get { return this.RESOLVED_INDValue; }
            set { SetProperty(ref RESOLVED_INDValue, value); }
        }

        private System.DateTime? RESOLVED_DATEValue;
        public System.DateTime? RESOLVED_DATE
        {
            get { return this.RESOLVED_DATEValue; }
            set { SetProperty(ref RESOLVED_DATEValue, value); }
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


