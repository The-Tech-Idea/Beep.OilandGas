using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CURRENCY_TRANSLATION_RESULT : ModelEntityBase
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
    }
}


