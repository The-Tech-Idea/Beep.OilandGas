using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class USER : Entity, IPPDMEntity
    {
        private string USER_IDValue = string.Empty;
        public string USER_ID
        {
            get => USER_IDValue;
            set => SetProperty(ref USER_IDValue, value);
        }

        private string USERNAMEValue = string.Empty;
        public string USERNAME
        {
            get => USERNAMEValue;
            set => SetProperty(ref USERNAMEValue, value);
        }

        private string EMAILValue = string.Empty;
        public string EMAIL
        {
            get => EMAILValue;
            set => SetProperty(ref EMAILValue, value);
        }

        private string PASSWORD_HASHValue = string.Empty;
        public string PASSWORD_HASH
        {
            get => PASSWORD_HASHValue;
            set => SetProperty(ref PASSWORD_HASHValue, value);
        }

        private bool IS_ACTIVEValue;
        public bool IS_ACTIVE
        {
            get => IS_ACTIVEValue;
            set => SetProperty(ref IS_ACTIVEValue, value);
        }

        private string TENANT_IDValue;
        public string TENANT_ID
        {
            get => TENANT_IDValue;
            set => SetProperty(ref TENANT_IDValue, value);
        }

        // Standard PPDM columns
        private string ACTIVE_INDValue = "Y";
        public string ACTIVE_IND
        {
            get => ACTIVE_INDValue;
            set => SetProperty(ref ACTIVE_INDValue, value);
        }

        private string ROW_QUALITYValue;
        public string ROW_QUALITY
        {
            get => ROW_QUALITYValue;
            set => SetProperty(ref ROW_QUALITYValue, value);
        }

        private string PPDM_GUIDValue;
        public string PPDM_GUID
        {
            get => PPDM_GUIDValue;
            set => SetProperty(ref PPDM_GUIDValue, value);
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get => ROW_CREATED_DATEValue;
            set => SetProperty(ref ROW_CREATED_DATEValue, value);
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get => ROW_CREATED_BYValue;
            set => SetProperty(ref ROW_CREATED_BYValue, value);
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get => ROW_CHANGED_DATEValue;
            set => SetProperty(ref ROW_CHANGED_DATEValue, value);
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get => ROW_CHANGED_BYValue;
            set => SetProperty(ref ROW_CHANGED_BYValue, value);
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get => ROW_EFFECTIVE_DATEValue;
            set => SetProperty(ref ROW_EFFECTIVE_DATEValue, value);
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get => ROW_EXPIRY_DATEValue;
            set => SetProperty(ref ROW_EXPIRY_DATEValue, value);
        }
    }
}



