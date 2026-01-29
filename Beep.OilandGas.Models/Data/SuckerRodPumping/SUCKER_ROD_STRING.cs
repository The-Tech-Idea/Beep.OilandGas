using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public partial class SUCKER_ROD_STRING : ModelEntityBase {
        private String SUCKER_ROD_STRING_IDValue;
        public String SUCKER_ROD_STRING_ID
        {
            get { return this.SUCKER_ROD_STRING_IDValue; }
            set { SetProperty(ref SUCKER_ROD_STRING_IDValue, value); }
        }

        private Decimal  TOTAL_LENGTHValue;
        public Decimal  TOTAL_LENGTH
        {
            get { return this.TOTAL_LENGTHValue; }
            set { SetProperty(ref TOTAL_LENGTHValue, value); }
        }

        private Decimal  TOTAL_WEIGHTValue;
        public Decimal  TOTAL_WEIGHT
        {
            get { return this.TOTAL_WEIGHTValue; }
            set { SetProperty(ref TOTAL_WEIGHTValue, value); }
        }

        public List<ROD_SECTION> SECTIONS { get; set; }

        // Standard PPDM columns

    }
}
