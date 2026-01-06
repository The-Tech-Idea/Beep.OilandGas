using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public partial class HEAT_MAP_DATA_POINT : Entity, IPPDMEntity
    {
        private String HEAT_MAP_DATA_POINT_IDValue;
        public String HEAT_MAP_DATA_POINT_ID
        {
            get { return this.HEAT_MAP_DATA_POINT_IDValue; }
            set { SetProperty(ref HEAT_MAP_DATA_POINT_IDValue, value); }
        }

        private String HEAT_MAP_IDValue;
        public String HEAT_MAP_ID
        {
            get { return this.HEAT_MAP_IDValue; }
            set { SetProperty(ref HEAT_MAP_IDValue, value); }
        }

        private Decimal? XValue;
        public Decimal? X
        {
            get { return this.XValue; }
            set { SetProperty(ref XValue, value); }
        }

        private Decimal? YValue;
        public Decimal? Y
        {
            get { return this.YValue; }
            set { SetProperty(ref YValue, value); }
        }

        private Decimal? ORIGINAL_XValue;
        public Decimal? ORIGINAL_X
        {
            get { return this.ORIGINAL_XValue; }
            set { SetProperty(ref ORIGINAL_XValue, value); }
        }

        private Decimal? ORIGINAL_YValue;
        public Decimal? ORIGINAL_Y
        {
            get { return this.ORIGINAL_YValue; }
            set { SetProperty(ref ORIGINAL_YValue, value); }
        }

        private Decimal? VALUEValue;
        public Decimal? VALUE
        {
            get { return this.VALUEValue; }
            set { SetProperty(ref VALUEValue, value); }
        }

        private String LABELValue;
        public String LABEL
        {
            get { return this.LABELValue; }
            set { SetProperty(ref LABELValue, value); }
        }

        // Standard PPDM columns
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
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

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
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

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
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

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }
    }
}



