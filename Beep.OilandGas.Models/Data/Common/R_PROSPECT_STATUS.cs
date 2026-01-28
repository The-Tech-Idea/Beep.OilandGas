using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class R_PROSPECT_STATUS : ModelEntityBase
    {
        private String PROSPECT_STATUSValue;
        public String PROSPECT_STATUS
        {
            get { return this.PROSPECT_STATUSValue; }
            set { SetProperty(ref PROSPECT_STATUSValue, value); }
        }

        private String ABBREVIATIONValue;
        public String ABBREVIATION
        {
            get { return this.ABBREVIATIONValue; }
            set { SetProperty(ref ABBREVIATIONValue, value); }
        }

        private String LONG_NAMEValue;
        public String LONG_NAME
        {
            get { return this.LONG_NAMEValue; }
            set { SetProperty(ref LONG_NAMEValue, value); }
        }

        private String SHORT_NAMEValue;
        public String SHORT_NAME
        {
            get { return this.SHORT_NAMEValue; }
            set { SetProperty(ref SHORT_NAMEValue, value); }
        }

        // Standard PPDM columns

    }
}
