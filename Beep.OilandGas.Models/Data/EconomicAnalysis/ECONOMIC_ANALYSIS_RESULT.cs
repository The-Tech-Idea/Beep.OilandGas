using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    public partial class ECONOMIC_ANALYSIS_RESULT : ModelEntityBase {
        private System.String ANALYSIS_IDValue;
        public System.String ANALYSIS_ID
        {
            get { return this.ANALYSIS_IDValue; }
            set { SetProperty(ref ANALYSIS_IDValue, value); }
        }

        private System.DateTime? ANALYSIS_DATEValue;
        public System.DateTime? ANALYSIS_DATE
        {
            get { return this.ANALYSIS_DATEValue; }
            set { SetProperty(ref ANALYSIS_DATEValue, value); }
        }

        private System.Decimal? NPVValue;
        public System.Decimal? NPV
        {
            get { return this.NPVValue; }
            set { SetProperty(ref NPVValue, value); }
        }

        private System.Decimal? IRRValue;
        public System.Decimal? IRR
        {
            get { return this.IRRValue; }
            set { SetProperty(ref IRRValue, value); }
        }

        private System.Decimal? PAYBACK_PERIODValue;
        public System.Decimal? PAYBACK_PERIOD
        {
            get { return this.PAYBACK_PERIODValue; }
            set { SetProperty(ref PAYBACK_PERIODValue, value); }
        }

        private System.Decimal? DISCOUNT_RATEValue;
        public System.Decimal? DISCOUNT_RATE
        {
            get { return this.DISCOUNT_RATEValue; }
            set { SetProperty(ref DISCOUNT_RATEValue, value); }
        }

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}


