using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class DEFERRED_TAX_BALANCE : ModelEntityBase {
        private string DEFERRED_TAX_BALANCE_IDValue;
        public string DEFERRED_TAX_BALANCE_ID
        {
            get { return this.DEFERRED_TAX_BALANCE_IDValue; }
            set { SetProperty(ref DEFERRED_TAX_BALANCE_IDValue, value); }
        }

        private string PROPERTY_IDValue;
        public string PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private DateTime? PERIOD_END_DATEValue;
        public DateTime? PERIOD_END_DATE
        {
            get { return this.PERIOD_END_DATEValue; }
            set { SetProperty(ref PERIOD_END_DATEValue, value); }
        }

        private decimal? DEFERRED_TAX_ASSETValue;
        public decimal? DEFERRED_TAX_ASSET
        {
            get { return this.DEFERRED_TAX_ASSETValue; }
            set { SetProperty(ref DEFERRED_TAX_ASSETValue, value); }
        }

        private decimal? DEFERRED_TAX_LIABILITYValue;
        public decimal? DEFERRED_TAX_LIABILITY
        {
            get { return this.DEFERRED_TAX_LIABILITYValue; }
            set { SetProperty(ref DEFERRED_TAX_LIABILITYValue, value); }
        }

        private string NOTESValue;
        public string NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

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

        private string REMARKValue;
        public string REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private string SOURCEValue;
        public string SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private string ROW_QUALITYValue;
        public string ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
