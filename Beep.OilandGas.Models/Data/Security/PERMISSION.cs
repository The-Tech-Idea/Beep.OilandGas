using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class PERMISSION : Entity, IPPDMEntity
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
        private string ACTIVE_INDValue = "Y";
        public string ACTIVE_IND
        {
            get => ACTIVE_INDValue;
            set => SetProperty(ref ACTIVE_INDValue, value);
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get => ROW_CREATED_BYValue;
            set => SetProperty(ref ROW_CREATED_BYValue, value);
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get => ROW_CREATED_DATEValue;
            set => SetProperty(ref ROW_CREATED_DATEValue, value);
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get => ROW_CHANGED_BYValue;
            set => SetProperty(ref ROW_CHANGED_BYValue, value);
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get => ROW_CHANGED_DATEValue;
            set => SetProperty(ref ROW_CHANGED_DATEValue, value);
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
    }
}



