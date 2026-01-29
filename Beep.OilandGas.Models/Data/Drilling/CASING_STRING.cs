using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class CASING_STRING : ModelEntityBase
    {
        private String CASING_IDValue;
        public String CASING_ID
        {
            get { return this.CASING_IDValue; }
            set { SetProperty(ref CASING_IDValue, value); }
        }

        private String WELL_CONSTRUCTION_IDValue;
        public String WELL_CONSTRUCTION_ID
        {
            get { return this.WELL_CONSTRUCTION_IDValue; }
            set { SetProperty(ref WELL_CONSTRUCTION_IDValue, value); }
        }

        private String CASING_TYPEValue;
        public String CASING_TYPE
        {
            get { return this.CASING_TYPEValue; }
            set { SetProperty(ref CASING_TYPEValue, value); }
        }

        private Decimal  TOP_DEPTHValue;
        public Decimal  TOP_DEPTH
        {
            get { return this.TOP_DEPTHValue; }
            set { SetProperty(ref TOP_DEPTHValue, value); }
        }

        private Decimal  BOTTOM_DEPTHValue;
        public Decimal  BOTTOM_DEPTH
        {
            get { return this.BOTTOM_DEPTHValue; }
            set { SetProperty(ref BOTTOM_DEPTHValue, value); }
        }

        private Decimal  DIAMETERValue;
        public Decimal  DIAMETER
        {
            get { return this.DIAMETERValue; }
            set { SetProperty(ref DIAMETERValue, value); }
        }

        private String DIAMETER_UNITValue;
        public String DIAMETER_UNIT
        {
            get { return this.DIAMETER_UNITValue; }
            set { SetProperty(ref DIAMETER_UNITValue, value); }
        }

        private String GRADEValue;
        public String GRADE
        {
            get { return this.GRADEValue; }
            set { SetProperty(ref GRADEValue, value); }
        }

        private Decimal  WEIGHTValue;
        public Decimal  WEIGHT
        {
            get { return this.WEIGHTValue; }
            set { SetProperty(ref WEIGHTValue, value); }
        }

        private String WEIGHT_UNITValue;
        public String WEIGHT_UNIT
        {
            get { return this.WEIGHT_UNITValue; }
            set { SetProperty(ref WEIGHT_UNITValue, value); }
        }

        // Standard PPDM columns

        public CASING_STRING() { }

        private string CONSTRUCTION_IDValue;
        public string CONSTRUCTION_ID
        {
            get { return this.CONSTRUCTION_IDValue; }
            set { SetProperty(ref CONSTRUCTION_IDValue, value); }
        }
    }
}
