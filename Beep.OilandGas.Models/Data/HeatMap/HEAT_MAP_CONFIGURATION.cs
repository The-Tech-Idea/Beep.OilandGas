using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public partial class HEAT_MAP_CONFIGURATION : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private string HEAT_MAP_IDValue;
        public string HEAT_MAP_ID
        {
            get { return this.HEAT_MAP_IDValue; }
            set { SetProperty(ref HEAT_MAP_IDValue, value); }
        }

        private string CONFIGURATION_NAMEValue;
        public string CONFIGURATION_NAME
        {
            get { return this.CONFIGURATION_NAMEValue; }
            set { SetProperty(ref CONFIGURATION_NAMEValue, value); }
        }

        // Standard PPDM columns
        private string ACTIVE_INDValue;
        public string ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private string PPDM_GUIDValue;
        public string PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }
    }
}




