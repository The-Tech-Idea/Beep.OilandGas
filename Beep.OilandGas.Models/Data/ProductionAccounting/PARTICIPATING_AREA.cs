using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PARTICIPATING_AREA : ModelEntityBase {
        private System.String PARTICIPATING_AREA_IDValue;
        public System.String PARTICIPATING_AREA_ID
        {
            get { return this.PARTICIPATING_AREA_IDValue; }
            set { SetProperty(ref PARTICIPATING_AREA_IDValue, value); }
        }

        private System.String UNIT_IDValue;
        public System.String UNIT_ID
        {
            get { return this.UNIT_IDValue; }
            set { SetProperty(ref UNIT_IDValue, value); }
        }

        private System.String PARTICIPATING_AREA_NAMEValue;
        public System.String PARTICIPATING_AREA_NAME
        {
            get { return this.PARTICIPATING_AREA_NAMEValue; }
            set { SetProperty(ref PARTICIPATING_AREA_NAMEValue, value); }
        }

        private System.String TRACTS_JSONValue;
        public System.String TRACTS_JSON
        {
            get { return this.TRACTS_JSONValue; }
            set { SetProperty(ref TRACTS_JSONValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.Decimal? TOTAL_PARTICIPATIONValue;
        public System.Decimal? TOTAL_PARTICIPATION
        {
            get { return this.TOTAL_PARTICIPATIONValue; }
            set { SetProperty(ref TOTAL_PARTICIPATIONValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
