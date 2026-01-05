using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class DRILLING_REPORT : Entity, IPPDMEntity
    {
        private String DRILLING_REPORT_IDValue;
        public String DRILLING_REPORT_ID
        {
            get { return this.DRILLING_REPORT_IDValue; }
            set { SetProperty(ref DRILLING_REPORT_IDValue, value); }
        }

        private String DRILLING_OPERATION_IDValue;
        public String DRILLING_OPERATION_ID
        {
            get { return this.DRILLING_OPERATION_IDValue; }
            set { SetProperty(ref DRILLING_OPERATION_IDValue, value); }
        }

        private DateTime? REPORT_DATEValue;
        public DateTime? REPORT_DATE
        {
            get { return this.REPORT_DATEValue; }
            set { SetProperty(ref REPORT_DATEValue, value); }
        }

        private Decimal? DEPTHValue;
        public Decimal? DEPTH
        {
            get { return this.DEPTHValue; }
            set { SetProperty(ref DEPTHValue, value); }
        }

        private String ACTIVITYValue;
        public String ACTIVITY
        {
            get { return this.ACTIVITYValue; }
            set { SetProperty(ref ACTIVITYValue, value); }
        }

        private Decimal? HOURSValue;
        public Decimal? HOURS
        {
            get { return this.HOURSValue; }
            set { SetProperty(ref HOURSValue, value); }
        }

        private String REPORTED_BYValue;
        public String REPORTED_BY
        {
            get { return this.REPORTED_BYValue; }
            set { SetProperty(ref REPORTED_BYValue, value); }
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

        public DRILLING_REPORT() { }
    }
}

