using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    public partial class ECONOMIC_CASH_FLOW : ModelEntityBase {
        private String ECONOMIC_CASH_FLOW_IDValue;
        public String ECONOMIC_CASH_FLOW_ID
        {
            get { return this.ECONOMIC_CASH_FLOW_IDValue; }
            set { SetProperty(ref ECONOMIC_CASH_FLOW_IDValue, value); }
        }

        private String ECONOMIC_ANALYSIS_IDValue;
        public String ECONOMIC_ANALYSIS_ID
        {
            get { return this.ECONOMIC_ANALYSIS_IDValue; }
            set { SetProperty(ref ECONOMIC_ANALYSIS_IDValue, value); }
        }

        private Int32? PERIODValue;
        public Int32? PERIOD
        {
            get { return this.PERIODValue; }
            set { SetProperty(ref PERIODValue, value); }
        }

        private Decimal? AMOUNTValue;
        public Decimal? AMOUNT
        {
            get { return this.AMOUNTValue; }
            set { SetProperty(ref AMOUNTValue, value); }
        }

        private String DESCRIPTIONValue;
        public String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        // Standard PPDM columns

    }
}


