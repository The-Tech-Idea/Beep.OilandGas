using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Entity for storing entity version snapshots for audit and rollback
    /// </summary>
    public partial class DATA_VERSION_SNAPSHOT : ModelEntityBase
    {
        private System.String VERSION_SNAPSHOT_IDValue;
        public System.String VERSION_SNAPSHOT_ID
        {
            get { return this.VERSION_SNAPSHOT_IDValue; }
            set { SetProperty(ref VERSION_SNAPSHOT_IDValue, value); }
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

        private System.Int32? VERSION_NUMBERValue;
        public System.Int32? VERSION_NUMBER
        {
            get { return this.VERSION_NUMBERValue; }
            set { SetProperty(ref VERSION_NUMBERValue, value); }
        }

        private System.String VERSION_LABELValue;
        public System.String VERSION_LABEL
        {
            get { return this.VERSION_LABELValue; }
            set { SetProperty(ref VERSION_LABELValue, value); }
        }

        private System.String ENTITY_DATA_JSONValue;
        public System.String ENTITY_DATA_JSON
        {
            get { return this.ENTITY_DATA_JSONValue; }
            set { SetProperty(ref ENTITY_DATA_JSONValue, value); }
        }

        private System.String CHANGE_DESCRIPTIONValue;
        public System.String CHANGE_DESCRIPTION
        {
            get { return this.CHANGE_DESCRIPTIONValue; }
            set { SetProperty(ref CHANGE_DESCRIPTIONValue, value); }
        }

        private System.DateTime? CREATED_DATEValue;
        public System.DateTime? CREATED_DATE
        {
            get { return this.CREATED_DATEValue; }
            set { SetProperty(ref CREATED_DATEValue, value); }
        }

        private System.String CREATED_BYValue;
        public System.String CREATED_BY
        {
            get { return this.CREATED_BYValue; }
            set { SetProperty(ref CREATED_BYValue, value); }
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


