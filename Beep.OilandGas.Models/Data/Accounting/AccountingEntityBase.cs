using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public abstract class AccountingEntityBase : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private string ACTIVE_INDValue = "Y";
        public string ACTIVE_IND { get => ACTIVE_INDValue; set => SetProperty(ref ACTIVE_INDValue, value); }

        private string ROW_CREATED_BYValue = string.Empty;
        public string ROW_CREATED_BY { get => ROW_CREATED_BYValue; set => SetProperty(ref ROW_CREATED_BYValue, value); }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE { get => ROW_CREATED_DATEValue; set => SetProperty(ref ROW_CREATED_DATEValue, value); }

        private string ROW_CHANGED_BYValue = string.Empty;
        public string ROW_CHANGED_BY { get => ROW_CHANGED_BYValue; set => SetProperty(ref ROW_CHANGED_BYValue, value); }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE { get => ROW_CHANGED_DATEValue; set => SetProperty(ref ROW_CHANGED_DATEValue, value); }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE { get => ROW_EFFECTIVE_DATEValue; set => SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE { get => ROW_EXPIRY_DATEValue; set => SetProperty(ref ROW_EXPIRY_DATEValue, value); }

        private string ROW_QUALITYValue = string.Empty;
        public string ROW_QUALITY { get => ROW_QUALITYValue; set => SetProperty(ref ROW_QUALITYValue, value); }

        private string PPDM_GUIDValue = string.Empty;
        public string PPDM_GUID { get => PPDM_GUIDValue; set => SetProperty(ref PPDM_GUIDValue, value); }
    }
}
