using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class COMPLETION_STRING : Entity, IPPDMEntity
    {
        private String COMPLETION_IDValue;
        public String COMPLETION_ID
        {
            get { return this.COMPLETION_IDValue; }
            set { SetProperty(ref COMPLETION_IDValue, value); }
        }

        private String WELL_CONSTRUCTION_IDValue;
        public String WELL_CONSTRUCTION_ID
        {
            get { return this.WELL_CONSTRUCTION_IDValue; }
            set { SetProperty(ref WELL_CONSTRUCTION_IDValue, value); }
        }

        private String COMPLETION_TYPEValue;
        public String COMPLETION_TYPE
        {
            get { return this.COMPLETION_TYPEValue; }
            set { SetProperty(ref COMPLETION_TYPEValue, value); }
        }

        private Decimal? TOP_DEPTHValue;
        public Decimal? TOP_DEPTH
        {
            get { return this.TOP_DEPTHValue; }
            set { SetProperty(ref TOP_DEPTHValue, value); }
        }

        private Decimal? BOTTOM_DEPTHValue;
        public Decimal? BOTTOM_DEPTH
        {
            get { return this.BOTTOM_DEPTHValue; }
            set { SetProperty(ref BOTTOM_DEPTHValue, value); }
        }

        private Decimal? DIAMETERValue;
        public Decimal? DIAMETER
        {
            get { return this.DIAMETERValue; }
            set { SetProperty(ref DIAMETERValue, value); }
        }

        private String DIAMETER_UNITValue;
        public String DIAMETER_UNIT
        {
            get { return this.DIAMETER_UNITValue; }
            set { SetProperty(ref DIAMETER_UNITValue, value); }
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

        public COMPLETION_STRING() { }
    }
}




