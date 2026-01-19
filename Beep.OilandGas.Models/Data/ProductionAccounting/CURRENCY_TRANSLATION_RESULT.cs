using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CURRENCY_TRANSLATION_RESULT : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private string TRANSLATION_RESULT_IDValue;
        public string TRANSLATION_RESULT_ID
        {
            get { return this.TRANSLATION_RESULT_IDValue; }
            set { SetProperty(ref TRANSLATION_RESULT_IDValue, value); }
        }

        private string ENTITY_IDValue;
        public string ENTITY_ID
        {
            get { return this.ENTITY_IDValue; }
            set { SetProperty(ref ENTITY_IDValue, value); }
        }

        private DateTime? PERIOD_ENDValue;
        public DateTime? PERIOD_END
        {
            get { return this.PERIOD_ENDValue; }
            set { SetProperty(ref PERIOD_ENDValue, value); }
        }

        private string REPORTING_CURRENCYValue;
        public string REPORTING_CURRENCY
        {
            get { return this.REPORTING_CURRENCYValue; }
            set { SetProperty(ref REPORTING_CURRENCYValue, value); }
        }

        private string ORIGINAL_CURRENCYValue;
        public string ORIGINAL_CURRENCY
        {
            get { return this.ORIGINAL_CURRENCYValue; }
            set { SetProperty(ref ORIGINAL_CURRENCYValue, value); }
        }

        private decimal? TRANSLATED_AMOUNTValue;
        public decimal? TRANSLATED_AMOUNT
        {
            get { return this.TRANSLATED_AMOUNTValue; }
            set { SetProperty(ref TRANSLATED_AMOUNTValue, value); }
        }

        private decimal? RATE_USEDValue;
        public decimal? RATE_USED
        {
            get { return this.RATE_USEDValue; }
            set { SetProperty(ref RATE_USEDValue, value); }
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
    }
}
