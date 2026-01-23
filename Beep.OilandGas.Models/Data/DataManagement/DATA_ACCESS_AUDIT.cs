using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Entity for tracking data access events for compliance and auditing
    /// Records who accessed what data and when
    /// </summary>
    public partial class DATA_ACCESS_AUDIT : ModelEntityBase
    {
        private System.String ACCESS_AUDIT_IDValue;
        public System.String ACCESS_AUDIT_ID
        {
            get { return this.ACCESS_AUDIT_IDValue; }
            set { SetProperty(ref ACCESS_AUDIT_IDValue, value); }
        }

        private System.String USER_IDValue;
        public System.String USER_ID
        {
            get { return this.USER_IDValue; }
            set { SetProperty(ref USER_IDValue, value); }
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

        private System.String ACCESS_TYPEValue;
        public System.String ACCESS_TYPE
        {
            get { return this.ACCESS_TYPEValue; }
            set { SetProperty(ref ACCESS_TYPEValue, value); }
        }

        private System.DateTime? ACCESS_DATEValue;
        public System.DateTime? ACCESS_DATE
        {
            get { return this.ACCESS_DATEValue; }
            set { SetProperty(ref ACCESS_DATEValue, value); }
        }

        private System.String IP_ADDRESSValue;
        public System.String IP_ADDRESS
        {
            get { return this.IP_ADDRESSValue; }
            set { SetProperty(ref IP_ADDRESSValue, value); }
        }

        private System.String SESSION_IDValue;
        public System.String SESSION_ID
        {
            get { return this.SESSION_IDValue; }
            set { SetProperty(ref SESSION_IDValue, value); }
        }

        private System.String REMARKValue;

        // Standard PPDM columns

        private System.String SOURCEValue;

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


