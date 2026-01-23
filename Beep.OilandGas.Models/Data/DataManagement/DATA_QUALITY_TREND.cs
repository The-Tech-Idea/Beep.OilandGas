using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Entity for storing historical data quality trends over time
    /// </summary>
    public partial class DATA_QUALITY_TREND : ModelEntityBase
    {
        private System.String TREND_IDValue;
        public System.String TREND_ID
        {
            get { return this.TREND_IDValue; }
            set { SetProperty(ref TREND_IDValue, value); }
        }

        private System.String TABLE_NAMEValue;
        public System.String TABLE_NAME
        {
            get { return this.TABLE_NAMEValue; }
            set { SetProperty(ref TABLE_NAMEValue, value); }
        }

        private System.DateTime? TREND_DATEValue;
        public System.DateTime? TREND_DATE
        {
            get { return this.TREND_DATEValue; }
            set { SetProperty(ref TREND_DATEValue, value); }
        }

        private System.Decimal? QUALITY_SCOREValue;
        public System.Decimal? QUALITY_SCORE
        {
            get { return this.QUALITY_SCOREValue; }
            set { SetProperty(ref QUALITY_SCOREValue, value); }
        }

        private System.Int32? RECORD_COUNTValue;
        public System.Int32? RECORD_COUNT
        {
            get { return this.RECORD_COUNTValue; }
            set { SetProperty(ref RECORD_COUNTValue, value); }
        }

        private System.Int32? ISSUE_COUNTValue;
        public System.Int32? ISSUE_COUNT
        {
            get { return this.ISSUE_COUNTValue; }
            set { SetProperty(ref ISSUE_COUNTValue, value); }
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


