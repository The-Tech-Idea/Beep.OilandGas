using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class USER : ModelEntityBase
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

    }
}
