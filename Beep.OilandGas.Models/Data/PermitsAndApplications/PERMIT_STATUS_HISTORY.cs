using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class PERMIT_STATUS_HISTORY : ModelEntityBase
    {
        private string PERMIT_STATUS_HISTORY_IDValue = string.Empty;
        public string PERMIT_STATUS_HISTORY_ID
        {
            get { return this.PERMIT_STATUS_HISTORY_IDValue; }
            set { SetProperty(ref PERMIT_STATUS_HISTORY_IDValue, value); }
        }

        private string? PERMIT_APPLICATION_IDValue;
        public string? PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private string? STATUSValue;
        public string? STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private DateTime? STATUS_DATEValue;
        public DateTime? STATUS_DATE
        {
            get { return this.STATUS_DATEValue; }
            set { SetProperty(ref STATUS_DATEValue, value); }
        }

        private string? STATUS_REMARKSValue;
        public string? STATUS_REMARKS
        {
            get { return this.STATUS_REMARKSValue; }
            set { SetProperty(ref STATUS_REMARKSValue, value); }
        }

        private string? UPDATED_BYValue;
        public string? UPDATED_BY
        {
            get { return this.UPDATED_BYValue; }
            set { SetProperty(ref UPDATED_BYValue, value); }
        }
    }
}
