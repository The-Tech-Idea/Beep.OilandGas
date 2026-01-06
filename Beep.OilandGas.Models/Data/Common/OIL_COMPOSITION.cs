using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class OIL_COMPOSITION : Entity
    {
        private string OIL_COMPOSITION_IDValue;
        public string OIL_COMPOSITION_ID
        {
            get { return this.OIL_COMPOSITION_IDValue; }
            set { SetProperty(ref OIL_COMPOSITION_IDValue, value); }
        }

        private string COMPOSITION_NAMEValue;
        public string COMPOSITION_NAME
        {
            get { return this.COMPOSITION_NAMEValue; }
            set { SetProperty(ref COMPOSITION_NAMEValue, value); }
        }

        private DateTime? COMPOSITION_DATEValue;
        public DateTime? COMPOSITION_DATE
        {
            get { return this.COMPOSITION_DATEValue; }
            set { SetProperty(ref COMPOSITION_DATEValue, value); }
        }

        private decimal? OIL_GRAVITYValue;
        public decimal? OIL_GRAVITY
        {
            get { return this.OIL_GRAVITYValue; }
            set { SetProperty(ref OIL_GRAVITYValue, value); }
        }

        private decimal? GAS_OIL_RATIOValue;
        public decimal? GAS_OIL_RATIO
        {
            get { return this.GAS_OIL_RATIOValue; }
            set { SetProperty(ref GAS_OIL_RATIOValue, value); }
        }

        private decimal? WATER_CUTValue;
        public decimal? WATER_CUT
        {
            get { return this.WATER_CUTValue; }
            set { SetProperty(ref WATER_CUTValue, value); }
        }

        private decimal? BUBBLE_POINT_PRESSUREValue;
        public decimal? BUBBLE_POINT_PRESSURE
        {
            get { return this.BUBBLE_POINT_PRESSUREValue; }
            set { SetProperty(ref BUBBLE_POINT_PRESSUREValue, value); }
        }

        // Standard PPDM columns
        private string ACTIVE_INDValue;
        public string ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private string PPDM_GUIDValue;
        public string PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private string REMARKValue;
        public string REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private string SOURCEValue;
        public string SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }
    }
}




