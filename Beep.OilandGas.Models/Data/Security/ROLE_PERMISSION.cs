using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class ROLE_PERMISSION : ModelEntityBase
    {
        private string ROLE_PERMISSION_IDValue = string.Empty;
        public string ROLE_PERMISSION_ID
        {
            get => ROLE_PERMISSION_IDValue;
            set => SetProperty(ref ROLE_PERMISSION_IDValue, value);
        }

        private string ROLE_IDValue = string.Empty;
        public string ROLE_ID
        {
            get => ROLE_IDValue;
            set => SetProperty(ref ROLE_IDValue, value);
        }

        private string PERMISSION_IDValue = string.Empty;
        public string PERMISSION_ID
        {
            get => PERMISSION_IDValue;
            set => SetProperty(ref PERMISSION_IDValue, value);
        }
    }
}
