using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.Models
{
    public partial class PIPELINE : Entity, IPPDMEntity
    {
        private string PIPELINE_IDValue = string.Empty;
        public string PIPELINE_ID
        {
            get { return PIPELINE_IDValue; }
            set { SetProperty(ref PIPELINE_IDValue, value); }
        }

        private string PIPELINE_NAMEValue = string.Empty;
        public string PIPELINE_NAME
        {
            get { return PIPELINE_NAMEValue; }
            set { SetProperty(ref PIPELINE_NAMEValue, value); }
        }

        private string FIELD_IDValue = string.Empty;
        public string FIELD_ID
        {
            get { return FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private string PIPELINE_TYPEValue = string.Empty;
        public string PIPELINE_TYPE
        {
            get { return PIPELINE_TYPEValue; }
            set { SetProperty(ref PIPELINE_TYPEValue, value); }
        }

        private decimal  DIAMETERValue;
        public decimal  DIAMETER
        {
            get { return DIAMETERValue; }
            set { SetProperty(ref DIAMETERValue, value); }
        }

        private decimal  LENGTHValue;
        public decimal  LENGTH
        {
            get { return LENGTHValue; }
            set { SetProperty(ref LENGTHValue, value); }
        }

        private string MATERIALValue = string.Empty;
        public string MATERIAL
        {
            get { return MATERIALValue; }
            set { SetProperty(ref MATERIALValue, value); }
        }

        private string ACTIVE_INDValue = string.Empty;
        public string ACTIVE_IND
        {
            get { return ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private string ROW_CREATED_BYValue = string.Empty;
        public string ROW_CREATED_BY
        {
            get { return ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue = string.Empty;
        public string ROW_CHANGED_BY
        {
            get { return ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private string ROW_QUALITYValue = string.Empty;
        public string ROW_QUALITY
        {
            get { return ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private string PPDM_GUIDValue = string.Empty;
        public string PPDM_GUID
        {
            get { return PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get { return EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get { return EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }

        private string SOURCEValue = string.Empty;
        public string SOURCE
        {
            get { return SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private string REMARKValue = string.Empty;
        public string REMARK
        {
            get { return REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        public PIPELINE() { }
    }
}
