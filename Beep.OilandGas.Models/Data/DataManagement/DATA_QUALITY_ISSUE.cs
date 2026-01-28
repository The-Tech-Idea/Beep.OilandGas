using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public partial class DATA_QUALITY_ISSUE : ModelEntityBase
    {
        private System.String QUALITY_ISSUE_IDValue;
        public System.String QUALITY_ISSUE_ID
        {
            get { return this.QUALITY_ISSUE_IDValue; }
            set { SetProperty(ref QUALITY_ISSUE_IDValue, value); }
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

        private System.String ENTITY_IDValue;
        public System.String ENTITY_ID
        {
            get { return this.ENTITY_IDValue; }
            set { SetProperty(ref ENTITY_IDValue, value); }
        }

        private System.String ISSUE_TYPEValue;
        public System.String ISSUE_TYPE
        {
            get { return this.ISSUE_TYPEValue; }
            set { SetProperty(ref ISSUE_TYPEValue, value); }
        }

        private System.String ISSUE_DESCRIPTIONValue;
        public System.String ISSUE_DESCRIPTION
        {
            get { return this.ISSUE_DESCRIPTIONValue; }
            set { SetProperty(ref ISSUE_DESCRIPTIONValue, value); }
        }

        private System.String SEVERITYValue;
        public System.String SEVERITY
        {
            get { return this.SEVERITYValue; }
            set { SetProperty(ref SEVERITYValue, value); }
        }

        private System.DateTime? ISSUE_DATEValue;
        public System.DateTime? ISSUE_DATE
        {
            get { return this.ISSUE_DATEValue; }
            set { SetProperty(ref ISSUE_DATEValue, value); }
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


       
        private object RECORD_IDValue;
        public object RECORD_ID
        {
            get { return this.RECORD_IDValue; }
            set { SetProperty(ref RECORD_IDValue, value); }
        }

     
    }
}
