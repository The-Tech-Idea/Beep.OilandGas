using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public partial class ROD_SECTION : ModelEntityBase {
        private String ROD_SECTION_IDValue;
        public String ROD_SECTION_ID
        {
            get { return this.ROD_SECTION_IDValue; }
            set { SetProperty(ref ROD_SECTION_IDValue, value); }
        }

        private String SUCKER_ROD_STRING_IDValue;
        public String SUCKER_ROD_STRING_ID
        {
            get { return this.SUCKER_ROD_STRING_IDValue; }
            set { SetProperty(ref SUCKER_ROD_STRING_IDValue, value); }
        }

        private Decimal  DIAMETERValue;
        public Decimal  DIAMETER
        {
            get { return this.DIAMETERValue; }
            set { SetProperty(ref DIAMETERValue, value); }
        }

        private Decimal  LENGTHValue;
        public Decimal  LENGTH
        {
            get { return this.LENGTHValue; }
            set { SetProperty(ref LENGTHValue, value); }
        }

        private Decimal  DENSITYValue;
        public Decimal  DENSITY
        {
            get { return this.DENSITYValue; }
            set { SetProperty(ref DENSITYValue, value); }
        }

        private Decimal  WEIGHTValue;
        public Decimal  WEIGHT
        {
            get { return this.WEIGHTValue; }
            set { SetProperty(ref WEIGHTValue, value); }
        }

        private Int32? SECTION_ORDERValue;
        public Int32? SECTION_ORDER
        {
            get { return this.SECTION_ORDERValue; }
            set { SetProperty(ref SECTION_ORDERValue, value); }
        }

        // Standard PPDM columns

    }
}
