using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

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

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
