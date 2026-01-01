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

        // Optional IPPDMEntity properties
        private string AREA_IDValue;
        public string AREA_ID
        {
            get => AREA_IDValue;
            set => SetProperty(ref AREA_IDValue, value);
        }

        private string AREA_TYPEValue;
        public string AREA_TYPE
        {
            get => AREA_TYPEValue;
            set => SetProperty(ref AREA_TYPEValue, value);
        }

        private string BUSINESS_ASSOCIATE_IDValue;
        public string BUSINESS_ASSOCIATE_ID
        {
            get => BUSINESS_ASSOCIATE_IDValue;
            set => SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value);
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

        private string SOURCEValue;
        public string SOURCE
        {
            get => SOURCEValue;
            set => SetProperty(ref SOURCEValue, value);
        }

        private string REMARKValue;
        public string REMARK
        {
            get => REMARKValue;
            set => SetProperty(ref REMARKValue, value);
        }
    }
}
