using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Security
{
    public partial class ROLE : ModelEntityBase
    {
        private string ROLE_IDValue = string.Empty;
        public string ROLE_ID
        {
            get => ROLE_IDValue;
            set => SetProperty(ref ROLE_IDValue, value);
        }

        private string ROLE_NAMEValue = string.Empty;
        public string ROLE_NAME
        {
            get => ROLE_NAMEValue;
            set => SetProperty(ref ROLE_NAMEValue, value);
        }

        private string DESCRIPTIONValue = string.Empty;
        public string DESCRIPTION
        {
            get => DESCRIPTIONValue;
            set => SetProperty(ref DESCRIPTIONValue, value);
        }

        // Standard PPDM columns

    }
}


