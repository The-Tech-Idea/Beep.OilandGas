using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class PERFORATION : Entity, IPPDMEntity
    {
        private String PERFORATION_IDValue;
        public String PERFORATION_ID
        {
            get { return this.PERFORATION_IDValue; }
            set { SetProperty(ref PERFORATION_IDValue, value); }
        }

        private String COMPLETION_IDValue;
        public String COMPLETION_ID
        {
            get { return this.COMPLETION_IDValue; }
            set { SetProperty(ref COMPLETION_IDValue, value); }
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

        private Int32? SHOTS_PER_FOOTValue;
        public Int32? SHOTS_PER_FOOT
        {
            get { return this.SHOTS_PER_FOOTValue; }
            set { SetProperty(ref SHOTS_PER_FOOTValue, value); }
        }

        private String PERFORATION_TYPEValue;
        public String PERFORATION_TYPE
        {
            get { return this.PERFORATION_TYPEValue; }
            set { SetProperty(ref PERFORATION_TYPEValue, value); }
        }

        private DateTime? PERFORATION_DATEValue;
        public DateTime? PERFORATION_DATE
        {
            get { return this.PERFORATION_DATEValue; }
            set { SetProperty(ref PERFORATION_DATEValue, value); }
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

        public PERFORATION() { }
    }
}

