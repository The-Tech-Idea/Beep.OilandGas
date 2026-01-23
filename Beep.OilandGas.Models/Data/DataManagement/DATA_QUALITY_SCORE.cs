using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Entity for storing individual entity quality scores
    /// </summary>
    public partial class DATA_QUALITY_SCORE : ModelEntityBase
    {
        private System.String QUALITY_SCORE_IDValue;
        public System.String QUALITY_SCORE_ID
        {
            get { return this.QUALITY_SCORE_IDValue; }
            set { SetProperty(ref QUALITY_SCORE_IDValue, value); }
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

        private System.Decimal? OVERALL_SCOREValue;
        public System.Decimal? OVERALL_SCORE
        {
            get { return this.OVERALL_SCOREValue; }
            set { SetProperty(ref OVERALL_SCOREValue, value); }
        }

        private System.DateTime? SCORE_DATEValue;
        public System.DateTime? SCORE_DATE
        {
            get { return this.SCORE_DATEValue; }
            set { SetProperty(ref SCORE_DATEValue, value); }
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


