using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Common
{
    public partial class R_INSPECTION_TYPE : ModelEntityBase
    {
        private String INSPECTION_TYPEValue;
        public String INSPECTION_TYPE
        {
            get { return this.INSPECTION_TYPEValue; }
            set { SetProperty(ref INSPECTION_TYPEValue, value); }
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


