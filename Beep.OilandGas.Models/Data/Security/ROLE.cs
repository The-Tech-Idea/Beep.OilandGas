using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class ROLE : Entity, IPPDMEntity
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
        private string ROW_QUALITYValue;
        public string ROW_QUALITY
        {
            get => ROW_QUALITYValue;
            set => SetProperty(ref ROW_QUALITYValue, value);
        }
    }
}
