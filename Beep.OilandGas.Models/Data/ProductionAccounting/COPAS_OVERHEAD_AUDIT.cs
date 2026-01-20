using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class COPAS_OVERHEAD_AUDIT : ModelEntityBase {
        private string COPAS_OVERHEAD_AUDIT_IDValue;
        public string COPAS_OVERHEAD_AUDIT_ID
        {
            get { return this.COPAS_OVERHEAD_AUDIT_IDValue; }
            set { SetProperty(ref COPAS_OVERHEAD_AUDIT_IDValue, value); }
        }

        private string COPAS_OVERHEAD_SCHEDULE_IDValue;
        public string COPAS_OVERHEAD_SCHEDULE_ID
        {
            get { return this.COPAS_OVERHEAD_SCHEDULE_IDValue; }
            set { SetProperty(ref COPAS_OVERHEAD_SCHEDULE_IDValue, value); }
        }

        private DateTime? CHANGE_DATEValue;
        public DateTime? CHANGE_DATE
        {
            get { return this.CHANGE_DATEValue; }
            set { SetProperty(ref CHANGE_DATEValue, value); }
        }

        private decimal? OLD_RATEValue;
        public decimal? OLD_RATE
        {
            get { return this.OLD_RATEValue; }
            set { SetProperty(ref OLD_RATEValue, value); }
        }

        private decimal? NEW_RATEValue;
        public decimal? NEW_RATE
        {
            get { return this.NEW_RATEValue; }
            set { SetProperty(ref NEW_RATEValue, value); }
        }

        private string CHANGE_REASONValue;
        public string CHANGE_REASON
        {
            get { return this.CHANGE_REASONValue; }
            set { SetProperty(ref CHANGE_REASONValue, value); }
        }

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

        private string ROW_QUALITYValue;
        public string ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
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

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
