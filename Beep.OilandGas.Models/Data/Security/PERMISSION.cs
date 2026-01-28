using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class PERMISSION : ModelEntityBase
    {
        private string PERMISSION_IDValue = string.Empty;
        public string PERMISSION_ID
        {
            get => PERMISSION_IDValue;
            set => SetProperty(ref PERMISSION_IDValue, value);
        }

        private string PERMISSION_CODEValue = string.Empty; // e.g., Module:Read, DataSource:Access:Production
        public string PERMISSION_CODE
        {
            get => PERMISSION_CODEValue;
            set => SetProperty(ref PERMISSION_CODEValue, value);
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
