using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class USER_ROLE : ModelEntityBase
    {
        private string USER_ROLE_IDValue = string.Empty;
        public string USER_ROLE_ID
        {
            get => USER_ROLE_IDValue;
            set => SetProperty(ref USER_ROLE_IDValue, value);
        }

        private string USER_IDValue = string.Empty;
        public string USER_ID
        {
            get => USER_IDValue;
            set => SetProperty(ref USER_IDValue, value);
        }

        private string ROLE_IDValue = string.Empty;
        public string ROLE_ID
        {
            get => ROLE_IDValue;
            set => SetProperty(ref ROLE_IDValue, value);
        }
        
        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get => EFFECTIVE_DATEValue;
            set => SetProperty(ref EFFECTIVE_DATEValue, value);
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get => EXPIRY_DATEValue;
            set => SetProperty(ref EXPIRY_DATEValue, value);
        }

        // Standard PPDM columns
    }
}
